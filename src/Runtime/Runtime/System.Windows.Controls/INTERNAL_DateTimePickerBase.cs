﻿
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using System.Windows.Controls.Primitives;

// Credits: https://github.com/MicrosoftArchive/SilverlightToolkit/tree/master/Release/Silverlight4/Source/Controls/DatePicker
// (c) Copyright Microsoft Corporation.
// License: Microsoft Public License (Ms-PL)

namespace System.Windows.Controls
{
    public abstract class INTERNAL_DateTimePickerBase : Control
    {
        protected FrameworkElement _root;
        protected TextBox _textBox;
        protected Button _dropDownButton;
        protected Popup _popup;

        protected string _defaultText = "Select Date...";
        protected INTERNAL_CalendarOrClockBase _calendarOrClock;
        protected const string ElementRoot = "Root";
        protected const string ElementTextBox = "TextBox";
        protected const string ElementButton = "Button";
        protected const string ElementPopup = "Popup";

        protected INTERNAL_DateTimePickerBase()
        {
            _calendarOrClock = GenerateCalendarOrClock();
            _calendarOrClock.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(CalendarOrClock_SelectionChanged);
            INTERNAL_SelectedDate = null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_popup != null)
            {
                _popup.IsOpen = false;
                _popup.Child = null;
                _popup.ClosedDueToOutsideClick -= Popup_ClosedDueToOutsideClick;
            }

            if (_dropDownButton != null)
            {
                _dropDownButton.Click -= new RoutedEventHandler(DropDownButton_Click);
            }

            _root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            _textBox = GetTemplateChild(ElementTextBox) as TextBox;
            _dropDownButton = GetTemplateChild(ElementButton) as Button;
            _popup = this.GetTemplateChild(ElementPopup) as Popup;

            if (_dropDownButton != null)
            {
                _dropDownButton.Click += new RoutedEventHandler(DropDownButton_Click);
            }

            if (_popup != null)
            {
                _popup.StayOpen = false;
                _popup.StaysWithinScreenBounds = true;
                _popup.Child = _calendarOrClock;
                _popup.IsOpen = false;
                _popup.ClosedDueToOutsideClick += Popup_ClosedDueToOutsideClick;
            }

            SetWaterMarkText();
            RefreshTextBox();
        }

        void Popup_ClosedDueToOutsideClick(object sender, EventArgs e)
        {
            if (this._dropDownButton != null)
            {
                // See comment below
                if (this._dropDownButton.IsMouseCaptured)
                {
                    this._dropDownButton.ReleaseMouseCapture();
                }
                
                this.IsDropDownOpen = false; // Note: this has other effects as well: see the "IsDropDownOpen_Changed" method
            }
        }

        protected void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsDropDownOpen = !this.IsDropDownOpen;
        }

        #region DropDownHandler

        protected void OpenDropDown()
        {
            _popup.IsOpen = true;
        }

        protected void CloseDropDown()
        {
            _popup.IsOpen = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the drop-down is open or closed.
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(
            "IsDropDownOpen",
            typeof(bool),
            typeof(INTERNAL_DateTimePickerBase),
            new PropertyMetadata(false, OnIsDropDownOpenChanged)
            { CallPropertyChangedWhenLoadedIntoVisualTree = WhenToCallPropertyChangedEnum.IfPropertyIsSet });

        /// <summary>
        /// IsDropDownOpenProperty property changed handler.
        /// </summary>
        /// <param name="d">the control that changed its IsDropDownOpen.</param>
        /// <param name="e">The DependencyPropertyChangedEventArgs.</param>
        protected static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            INTERNAL_DateTimePickerBase dp = d as INTERNAL_DateTimePickerBase;
            bool newValue = (bool)e.NewValue;

            if (dp._popup != null && dp._popup.Child != null)
            {
                if (newValue)
                {
                    dp.OpenDropDown();
                }
                else
                {
                    dp.CloseDropDown();
                }
            }
        }

        #endregion IsDropDownOpen

        #region SelectedDate

        // method triggered when the date has changed from flatpickr side 
        protected void CalendarOrClock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // we don't set INTERNAL_SelectedDate because it is already linked to the flatpickr

            OnSelectionChanged((DateTime?)OpenSilver.Interop.Unbox(e.AddedItems[0])); // overridden by date picker and time picker for specific behavior

            RefreshTextBox();

            if (SelectedDateChanged != null)
                SelectedDateChanged(this, e); // fire public event
        }

        protected abstract INTERNAL_CalendarOrClockBase GenerateCalendarOrClock();

        protected abstract void OnSelectionChanged(DateTime? newSelectedDate);

        // get or set the true SelectedDate in the flat picker (and trigger the events and dependency)
        protected DateTime? INTERNAL_SelectedDate
        {
            get { return _calendarOrClock.SelectedValue; }
            set { _calendarOrClock.SelectedValue = value; }
        }

        #endregion SelectedDate

        #region Text

        /// <summary>
        /// Gets or sets the text that is displayed by the textbox
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(INTERNAL_DateTimePickerBase),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTextChanged)
            { CallPropertyChangedWhenLoadedIntoVisualTree = WhenToCallPropertyChangedEnum.IfPropertyIsSet });

        /// <summary>
        /// TextProperty property changed handler.
        /// </summary>
        /// <param name="d">Date picker or time picker that changed its Text.</param>
        /// <param name="e">The DependencyPropertyChangedEventArgs.</param>
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            INTERNAL_DateTimePickerBase dp = (INTERNAL_DateTimePickerBase)d;
            if (dp.IsHandlerSuspended(TextProperty))
                return;

            string newText = (string)e.NewValue;
            if (string.IsNullOrEmpty(newText))
            {
                dp.OnEmptyText();
            }
            else
            {
                if (dp._textBox == null)
                    dp.ApplyTemplate();

                if (dp._textBox != null)
                {
                    dp._textBox.Text = newText;
                }
                else
                {
                    dp._defaultText = newText;
                }

                dp.OnTextChanged();
            }
        }

        protected virtual void OnTextChanged()
        {
        }


        protected virtual void OnEmptyText()
        {

        }

        protected void RefreshTextBox()
        {
            if (this._textBox != null)
            {
                DateTime? selectedDate = this.INTERNAL_SelectedDate;
                if (selectedDate != null)
                {
                    Text = SetTextFromDate(selectedDate);
                }
                else
                {
                    SetWaterMarkText();
                }
            }
        }

        protected abstract string SetTextFromDate(DateTime? newDate);

        protected virtual void SetWaterMarkText()
        {
            if (this._textBox != null)
            {
                Text = _defaultText;
            }
        }

        /// <summary>
        /// Input text is parsed in the correct format and changed into a DateTime object.
        /// If the text can not be parsed TextParseError Event is thrown.
        /// </summary>
        private DateTime? ParseText(string text)
        {
            DateTime newSelectedDate;

            newSelectedDate = ToDateTime(text);

            return newSelectedDate;
        }

        //todo: see if we can replace this method with a built-in one
        public static DateTime ToDateTime(string dateTimeAsString)
        {
            int year, month, day, hours, minutes, seconds;
            string[] split = dateTimeAsString.Split(' ');
            string datePart = split[0];
            string[] splittedDate = datePart.Split('/');
            month = int.Parse(splittedDate[0]);
            day = int.Parse(splittedDate[1]);
            year = int.Parse(splittedDate[2]);

            string TimePart = split[1];
            string[] splittedTime = TimePart.Split(':');
            hours = int.Parse(splittedTime[0]);
            minutes = int.Parse(splittedTime[1]);
            seconds = int.Parse(splittedTime[2]);

            return new DateTime(year, month, day, hours, minutes, seconds);
        }

        #endregion Text

        public event EventHandler<SelectionChangedEventArgs> SelectedDateChanged;

        //public global::System.Globalization.CultureInfo Culture { get; set; }

    }
}

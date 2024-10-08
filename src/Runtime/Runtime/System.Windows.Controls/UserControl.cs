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

using System.Collections;
using System.Windows.Markup;
using OpenSilver.Internal.Controls;

namespace System.Windows.Controls
{
    /// <summary>
    /// Provides the base class for defining a new control that encapsulates related
    /// existing controls and provides its own logic.
    /// </summary>
    [ContentProperty(nameof(Content))]
    public class UserControl : Control, IUserControl
    {
        /// <summary>
        /// Gets an enumerator to the user control's logical child elements.
        /// </summary>
        /// <returns>
        /// An enumerator. The default value is null.
        /// </returns>
        protected internal override IEnumerator LogicalChildren
        {
            get
            {
                if (Content is not UIElement content)
                {
                    return EmptyEnumerator.Instance;
                }

                // otherwise, its logical children is its visual children
                return new SingleChildEnumerator(content);
            }
        }

#region Constructors

        public UserControl()
        {
            IsTabStop = false; //we want to avoid stopping on this element's div when pressing tab.
        }

#endregion Constructors

        /// <summary>
        /// Gets or sets the content that is contained within a user control.
        /// </summary>
        public UIElement Content
        {
            get { return (UIElement)GetValue(ContentProperty); }
            set { SetValueInternal(ContentProperty, value); }
        }

        /// <summary>
        /// Identifies the Content dependency property
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(UIElement),
                typeof(UserControl),
                new PropertyMetadata(null, OnContentChanged));

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UserControl uc = (UserControl)d;

            uc.TemplateChild = null;
            uc.RemoveLogicalChild(e.OldValue);
            uc.AddLogicalChild(e.NewValue);

            uc.InvalidateMeasure();
        }

        /// <summary>
        /// Gets the element that should be used as the StateGroupRoot for VisualStateMangager.GoToState calls
        /// </summary>
        internal override FrameworkElement StateGroupsRoot
        {
            get
            {
                return Content as FrameworkElement;
            }
        }

        internal override FrameworkTemplate TemplateCache
        {
            get { return DefaultTemplate; }
            set { }
        }

        internal override FrameworkTemplate TemplateInternal
        {
            get { return DefaultTemplate; }
        }

        private static UseContentTemplate DefaultTemplate { get; } = new UseContentTemplate();

        private class UseContentTemplate : FrameworkTemplate
        {
            public UseContentTemplate()
            {
                Seal();
            }

            internal override bool BuildVisualTree(IFrameworkElement container)
            {
                UserControl uc = (UserControl)container;
                uc.TemplateChild = uc.Content as FrameworkElement;
                return false;
            }
        }
    }
}


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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xaml.Markup;
using CSHTML5.Internal;
using static System.Net.Mime.MediaTypeNames;

namespace OpenSilver.Internal.Controls;

internal sealed class TextBoxView : TextViewBase
{
    private readonly StringBuilder _textWithoutSubstitutes = new();
    private string _lastText;

    static TextBoxView()
    {
        TextElement.CharacterSpacingProperty.AddOwner(
            typeof(TextBoxView),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetCharacterSpacing((int)newValue),
            });

        TextElement.FontFamilyProperty.AddOwner(
            typeof(TextBoxView),
            new FrameworkPropertyMetadata(FontFamily.Default, FrameworkPropertyMetadataOptions.Inherits, OnFontFamilyChanged)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetFontFamily((FontFamily)newValue),
            });

        TextElement.FontSizeProperty.AddOwner(
            typeof(TextBoxView),
            new FrameworkPropertyMetadata(11d, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetFontSize((double)newValue),
            });

        TextElement.FontStyleProperty.AddOwner(
            typeof(TextBoxView),
            new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetFontStyle((FontStyle)newValue),
            });

        TextElement.FontWeightProperty.AddOwner(
           typeof(TextBoxView),
           new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
           {
               MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetFontWeight((FontWeight)newValue),
           });

        TextElement.ForegroundProperty.AddOwner(
            typeof(TextBoxView),
            new FrameworkPropertyMetadata(
                TextElement.ForegroundProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.Inherits,
                OnForegroundChanged)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetForeground(oldValue as Brush, (Brush)newValue),
            });

        Block.LineHeightProperty.AddOwner(
            typeof(TextBoxView),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetLineHeight((double)newValue),
            });

        Block.TextAlignmentProperty.AddOwner(
            typeof(TextBoxView),
            new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.Inherits)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((TextBoxView)d).SetTextAlignment((TextAlignment)newValue),
            });

        IsHitTestableProperty.OverrideMetadata(typeof(TextBoxView), new PropertyMetadata(BooleanBoxes.TrueBox));
    }

    private WeakEventListener<TextBoxView, Brush, EventArgs> _foregroundChangedListener;

    internal TextBoxView(TextBox host)
        : base(host)
    {
    }

    internal new TextBox Host => (TextBox)base.Host;

    public sealed override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren)
    {
        domElementWhereToPlaceChildren = null;
        return INTERNAL_HtmlDomManager.CreateTextBoxViewDomElementAndAppendIt((INTERNAL_HtmlDomElementReference)parentRef, this);
    }

    protected sealed internal override void INTERNAL_OnAttachedToVisualTree()
    {
        base.INTERNAL_OnAttachedToVisualTree();

        SetProperties();

        if (FocusManager.GetFocusedElement() == Host)
        {
            InputManager.SetFocusNative(OuterDiv);
        }
    }

    internal protected sealed override void OnInput(object data)
    {
        string text = GetText();
        ReplaceSilverlightSpecialCharacters(text, data as string);

        Host.UpdateTextProperty(_textWithoutSubstitutes.Length > 0 ? _textWithoutSubstitutes.ToString() : text);

        InvalidateMeasure();
    }

    internal string OnCopy()
    {
        string selectedOriginalText = _textWithoutSubstitutes.ToString(SelectionStart, SelectionLength);
        if (selectedOriginalText.Contains((char)173))
        {
            return selectedOriginalText;
        }

        return null;
    }

    private void ReplaceSilverlightSpecialCharacters(string text, string data)
    {
        if (_textWithoutSubstitutes.ToString().Contains((char)173) || data?.Contains((char)173) == true)
        {
            if (_textWithoutSubstitutes.Length > 0)
            {
                UpdateTextWithoutSubstitutes(text);

                int oldSelectionStart = SelectionStart;
                SetTextNative(text, true);
                SelectionStart = oldSelectionStart;

                _lastText = text;
            }
            else
            {
                int oldSelectionStart = SelectionStart;
                SetTextNative(text, false);
                SelectionStart = oldSelectionStart;
            }
        }
    }

    private void UpdateTextWithoutSubstitutes(string text)
    {
        (int diffStart, int diffEnd) = GetDifferenceIndices(text, _lastText);
        if (diffStart == -1 && diffEnd == -1)
        {
            return;
        }
        
        int diffLength = diffEnd - diffStart;
        int removedCount = diffLength - (text.Length - _lastText.Length);
        if (removedCount > 0)
        {
            _textWithoutSubstitutes.Remove(SelectionStart - diffLength, removedCount);
        }

        if (diffLength > 0)
        {
            string insertedText = text.Substring(diffStart, diffLength);
            _textWithoutSubstitutes.Insert(SelectionStart - diffLength, insertedText);
        }
    }

    static (int, int) GetDifferenceIndices(string str1, string str2)
    {
        int minLength = Math.Min(str1.Length, str2.Length);

        // Find the start of the difference
        int start = -1;
        for (int i = 0; i < minLength; i++)
        {
            if (str1[i] != str2[i])
            {
                start = i;
                break;
            }
        }

        // If no differences were found within the common length
        if (start == -1)
        {
            return str1.Length == str2.Length ? (-1, -1) : (minLength, Math.Max(str1.Length, str2.Length) - 1);
        }

        // Find the end of the difference
        int end1 = str1.Length - 1;
        int end2 = str2.Length - 1;
        while (end1 >= start && end2 >= start && str1[end1] == str2[end2])
        {
            end1--;
            end2--;
        }

        return (start, Math.Max(end1, end2));
    }

    internal void SetTextNative(string text, bool isProcessingInput)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            string escapedText = INTERNAL_HtmlDomManager.EscapeStringForUseInJavaScript(text);
            string sElement = Interop.GetVariableStringForJS(OuterDiv);
            Interop.ExecuteJavaScriptVoid(
                $"{sElement}.value = \"{escapedText}\";");

            if (!isProcessingInput)
            {
                _textWithoutSubstitutes.Clear();

                if (!string.IsNullOrEmpty(text) && text.Contains((char)173))
                {
                    _textWithoutSubstitutes.Append(text);
                    Console.WriteLine($"Initialize _textWithoutSubstitutes {_textWithoutSubstitutes} _lastText {escapedText}");
                    _lastText = escapedText;
                }
            }

            InvalidateMeasure();
        }
    }

    private void SetProperties()
    {
        TextBox host = Host;

        this.SetTextDecorations(host.TextDecorations);
        this.SetTextWrapping(host.TextWrapping);
        this.SetCaretColor(host.CaretBrush);

        if (host.IsReadOnly)
        {
            INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "readonly", string.Empty);
        }

        int maxlength = host.MaxLength;
        if (maxlength > 0)
        {
            INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "maxlength", maxlength);
        }

        // Disable spell check
        INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "spellcheck", host.IsSpellCheckEnabled);

        // Set the "data-accepts-return" property (that we have invented) so that the
        // "KeyDown" and "Paste" JavaScript events can retrieve this value:
        INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "data-acceptsreturn", host.AcceptsReturn);
        INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "data-acceptstab", host.AcceptsTab);

        if (Interop.IsRunningInTheSimulator)
        {
            string sElement = Interop.GetVariableStringForJS(OuterDiv);
            Interop.ExecuteJavaScriptVoidAsync($"document.textviewManager.handleKeyDownFromSimulator({sElement})");
        }

        SetTextNative(host.Text, false);
    }

    internal void ProcessKeyDown(KeyEventArgs e)
    {
        if (OuterDiv is null) return;

        if (TextViewManager.Instance.OnKeyDown(this, e))
        {
            e.Handled = true;
            e.Cancellable = false;
        }
    }

    internal void OnAcceptsReturnChanged(bool acceptsReturn)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv != null)
        {
            // Set the "data-accepts-return" property (that we have invented)
            // so that the "keydown" JavaScript event can retrieve this value:
            INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "data-acceptsreturn", acceptsReturn);
        }
    }

    internal void OnTextWrappingChanged(TextWrapping textWrapping)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv != null)
        {
            this.SetTextWrapping(textWrapping);
        }
    }

    internal void OnMaxLengthChanged(int maxLength)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv != null)
        {
            if (maxLength > 0)
            {
                INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "maxlength", maxLength);
            }
            else
            {
                INTERNAL_HtmlDomManager.RemoveAttribute(OuterDiv, "maxlength");
            }
        }
    }

    internal void OnTextDecorationsChanged(TextDecorationCollection tdc)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            this.SetTextDecorations(tdc);
        }
    }

    internal void OnIsReadOnlyChanged(bool isReadOnly)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv != null)
        {
            if (isReadOnly)
            {
                INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "readonly", string.Empty);
            }
            else
            {
                INTERNAL_HtmlDomManager.RemoveAttribute(OuterDiv, "readonly");
            }
        }
    }

    internal void OnIsSpellCheckEnabledChanged(bool isSpellCheckEnabled)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv != null)
        {
            INTERNAL_HtmlDomManager.SetDomElementAttribute(OuterDiv, "spellcheck", isSpellCheckEnabled);
        }
    }

    internal void SetCaretBrush(Brush brush)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            this.SetCaretColor(brush);
        }
    }

    internal int SelectionStart
    {
        get
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
            {
                return TextViewManager.Instance.GetSelectionStart(this);
            }

            return 0;
        }
        set
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
            {
                TextViewManager.Instance.SetSelectionStart(this, value);
            }
        }
    }

    internal int SelectionLength
    {
        get
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
            {
                return TextViewManager.Instance.GetSelectionLength(this);
            }

            return 0;
        }
        set
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
            {
                TextViewManager.Instance.SetSelectionLength(this, value);
            }
        }
    }

    internal string SelectedText
    {
        get
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
            {
                return TextViewManager.Instance.GetSelectedText(this);
            }

            return string.Empty;
        }
        set
        {
            if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
            {
                TextViewManager.Instance.SetSelectedText(this, value);
                
                Host.UpdateTextProperty(GetText());
                InvalidateMeasure();
            }
        }
    }

    internal void SetSelectionRange(int start, int end)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            string sElement = Interop.GetVariableStringForJS(OuterDiv);
            Interop.ExecuteJavaScriptVoid(
                $"{sElement}.setSelectionRange({start.ToInvariantString()}, {end.ToInvariantString()});");
        }
    }

    private string GetText()
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && OuterDiv is not null)
        {
            string sElement = Interop.GetVariableStringForJS(OuterDiv);
            return Interop.ExecuteJavaScriptString($"{sElement}.value;") ?? string.Empty;
        }

        return string.Empty;
    }

    protected sealed override Size MeasureContent(Size constraint)
    {
        return ParentWindow.TextMeasurementService.MeasureView(
            OuterDiv.UniqueIdentifier,
            Host.TextWrapping == TextWrapping.NoWrap ? "pre" : "pre-wrap",
            Host.TextWrapping == TextWrapping.NoWrap ? string.Empty : "break-word",
            constraint.Width,
            "M");
    }

    private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UIElementHelpers.InvalidateMeasureOnFontFamilyChanged((TextBoxView)d, (FontFamily)e.NewValue);
    }

    private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (TextBoxView)d;

        if (view._foregroundChangedListener != null)
        {
            view._foregroundChangedListener.Detach();
            view._foregroundChangedListener = null;
        }

        if (e.NewValue is Brush newBrush)
        {
            view._foregroundChangedListener = new(view, newBrush)
            {
                OnEventAction = static (instance, sender, args) => instance.OnForegroundChanged(sender, args),
                OnDetachAction = static (listener, source) => source.Changed -= listener.OnEvent,
            };
            newBrush.Changed += view._foregroundChangedListener.OnEvent;
        }
    }

    private void OnForegroundChanged(object sender, EventArgs e)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this))
        {
            var foreground = (Brush)sender; 
            this.SetForeground(foreground, foreground);
        }
    }
}

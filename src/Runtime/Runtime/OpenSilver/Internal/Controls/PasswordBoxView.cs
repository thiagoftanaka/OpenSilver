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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CSHTML5.Internal;

namespace OpenSilver.Internal.Controls;

internal sealed class PasswordBoxView : TextViewBase<PasswordBox>
{
    static PasswordBoxView()
    {
        TextElement.CharacterSpacingProperty.AddOwner(
            typeof(PasswordBoxView),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((PasswordBoxView)d).SetCharacterSpacing((int)newValue),
            });

        TextElement.FontFamilyProperty.AddOwner(
            typeof(PasswordBoxView),
            new FrameworkPropertyMetadata(FontFamily.Default, FrameworkPropertyMetadataOptions.Inherits, OnFontFamilyChanged)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((PasswordBoxView)d).SetFontFamily((FontFamily)newValue),
            });

        TextElement.FontSizeProperty.AddOwner(
            typeof(PasswordBoxView),
            new FrameworkPropertyMetadata(11d, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((PasswordBoxView)d).SetFontSize((double)newValue),
            });

        TextElement.FontStyleProperty.AddOwner(
            typeof(PasswordBoxView),
            new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((PasswordBoxView)d).SetFontStyle((FontStyle)newValue),
            });

        TextElement.FontWeightProperty.AddOwner(
           typeof(PasswordBoxView),
           new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure)
           {
               MethodToUpdateDom2 = static (d, oldValue, newValue) => ((PasswordBoxView)d).SetFontWeight((FontWeight)newValue),
           });

        TextElement.ForegroundProperty.AddOwner(
            typeof(PasswordBoxView),
            new FrameworkPropertyMetadata(
                TextElement.ForegroundProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.Inherits,
                OnForegroundChanged)
            {
                MethodToUpdateDom2 = static (d, oldValue, newValue) => ((PasswordBoxView)d).SetForeground((Brush)newValue),
            });
    }

    private WeakEventListener<PasswordBoxView, Brush, EventArgs> _foregroundChangedListener;

    internal PasswordBoxView(PasswordBox host)
        : base(host)
    {
    }

    public override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren) =>
        AddPasswordInputDomElement(parentRef, out domElementWhereToPlaceChildren);

    protected internal override void INTERNAL_OnAttachedToVisualTree()
    {
        base.INTERNAL_OnAttachedToVisualTree();

        SetPasswordNative(Host.Password);

        if (FocusManager.GetFocusedElement() == Host)
        {
            InputManager.SetFocusNative(InputDiv);
        }
    }

    protected sealed override void OnInput()
    {
        Host.UpdatePasswordProperty(GetPassword());
        InvalidateMeasure();
    }

    protected sealed override Size MeasureContent(Size constraint)
    {
        int pwdLength = Host.Password.Length;

        return ParentWindow.TextMeasurementService.MeasureText(
            ((INTERNAL_HtmlDomElementReference)OuterDiv).UniqueIdentifier,
            "pre",
            string.Empty,
            constraint.Width,
            pwdLength > 0 ? new string('•', pwdLength) : "M");
    }

    internal void SelectNative()
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && InputDiv is not null)
        {
            string sElement = CSHTML5.InteropImplementation.GetVariableStringForJS(InputDiv);
            Interop.ExecuteJavaScriptVoid($"{sElement}.select();");
        }
    }

    internal void OnMaxLengthChanged(int maxLength)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && InputDiv is not null)
        {
            INTERNAL_HtmlDomManager.SetDomElementProperty(InputDiv, "maxLength", maxLength);
        }
    }

    internal void SetPasswordNative(string text)
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && InputDiv is not null)
        {
            string sElement = CSHTML5.InteropImplementation.GetVariableStringForJS(InputDiv);
            Interop.ExecuteJavaScriptVoid(
                $"{sElement}.value = \"{INTERNAL_HtmlDomManager.EscapeStringForUseInJavaScript(text)}\";");

            InvalidateMeasure();
        }
    }

    private object AddPasswordInputDomElement(object parentRef, out object domElementWhereToPlaceChildren)
    {
        var passwordFieldStyle = INTERNAL_HtmlDomManager.CreateDomLayoutElementAppendItAndGetStyle(
            "input", parentRef, this, out object passwordField);

        domElementWhereToPlaceChildren = passwordField; // Note: this value is used by the Padding_Changed method to set the padding of the PasswordBox.

        passwordFieldStyle.border = "transparent"; // This removes the border. We do not need it since we are templated
        passwordFieldStyle.outline = "none";
        passwordFieldStyle.backgroundColor = "transparent";
        passwordFieldStyle.fontFamily = "inherit"; // Not inherited by default for "input" DOM elements
        passwordFieldStyle.fontSize = "inherit"; // Not inherited by default for "input" DOM elements
        passwordFieldStyle.color = "inherit"; //This is to inherit the foreground value from parent div.
        passwordFieldStyle.letterSpacing = "inherit"; // Not inherited by default for "input" DOM elements
        passwordFieldStyle.width = "100%";
        passwordFieldStyle.height = "100%";
        passwordFieldStyle.padding = "0px";

        INTERNAL_HtmlDomManager.SetDomElementAttribute(passwordField, "type", "password");

        // disable native tab navigation
        INTERNAL_HtmlDomManager.SetDomElementAttribute(passwordField, "tabindex", "-1");

        return passwordField;
    }

    private string GetPassword()
    {
        if (INTERNAL_VisualTreeManager.IsElementInVisualTree(this) && InputDiv is not null)
        {
            string sElement = CSHTML5.InteropImplementation.GetVariableStringForJS(InputDiv);
            return Interop.ExecuteJavaScriptString($"{sElement}.value;") ?? string.Empty;
        }

        return string.Empty;
    }

    private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        UIElementHelpers.InvalidateMeasureOnFontFamilyChanged((PasswordBoxView)d, (FontFamily)e.NewValue);
    }

    private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (PasswordBoxView)d;

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
            this.SetForeground((Brush)sender);
        }
    }
}
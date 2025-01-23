
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
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CSHTML5.Internal;

namespace OpenSilver.Internal.Controls;

internal sealed class RichTextViewManager
{
    private static JsonSerializerOptions SerializerOptions { get; } =
        new JsonSerializerOptions
        {
            IgnoreNullValues = true,
        };

    private readonly JavaScriptCallback _selectionChangedHandler;
    private readonly JavaScriptCallback _contentChangedHandler;
    private readonly JavaScriptCallback _scrollHandler;
    private readonly JavaScriptCallback _copyHandler;

    private RichTextViewManager()
    {
        _selectionChangedHandler = JavaScriptCallback.Create(OnSelectionChangedNative);
        _contentChangedHandler = JavaScriptCallback.Create(OnContentChangedNative);
        _scrollHandler = JavaScriptCallback.Create(OnScrollNative);
        _copyHandler = JavaScriptCallback.Create(OnCopyNative);
        string sSelectionChangedHandler = Interop.GetVariableStringForJS(_selectionChangedHandler);
        string sContentChangedHandler = Interop.GetVariableStringForJS(_contentChangedHandler);
        string sScrollHandler = Interop.GetVariableStringForJS(_scrollHandler);
        string sCopyHandler = Interop.GetVariableStringForJS(_copyHandler);
        Interop.ExecuteJavaScriptVoidAsync($"document.createRichTextViewManager({sSelectionChangedHandler}, {sContentChangedHandler}, {sScrollHandler}, {sCopyHandler})");
    }

    public static RichTextViewManager Instance { get; } = new();

    public void CreateView(string id, string parentId) =>
        Interop.ExecuteJavaScriptVoidAsync($"document.richTextViewManager.createView('{id}','{parentId}')");

    public bool OnKeyDown(RichTextBoxView richTextBoxView, KeyEventArgs e)
    {
        Debug.Assert(richTextBoxView is not null && richTextBoxView.OuterDiv is not null);
        Debug.Assert(e is not null);

        string sElement = Interop.GetVariableStringForJS(richTextBoxView.OuterDiv);
        string sArgs = Interop.GetVariableStringForJS(e.UIEventArg);
        return Interop.ExecuteJavaScriptBoolean($"document.richTextViewManager.onKeyDownNative({sElement}, {sArgs})");
    }

    private static void OnSelectionChangedNative(string id, int start, int length)
    {
        if (INTERNAL_HtmlDomManager.GetElementById(id) is RichTextBoxView view)
        {
            view.Host.UpdateSelection(start, length);
        }
    }

    private static void OnContentChangedNative(string id, string sDelta, string sOldDelta)
    {
        if (INTERNAL_HtmlDomManager.GetElementById(id) is RichTextBoxView view)
        {
            QuillDelta[] delta = sDelta switch
            {
                "" or null => Array.Empty<QuillDelta>(),
                string contents => JsonSerializer.Deserialize<QuillDelta[]>(contents, SerializerOptions)
            };
            QuillDelta[] oldDelta = sOldDelta switch
            {
                "" or null => Array.Empty<QuillDelta>(),
                string contents => JsonSerializer.Deserialize<QuillDelta[]>(contents, SerializerOptions)
            };

            view.OnInput(delta);
        }
    }

    private static void OnScrollNative(string id)
    {
        if (INTERNAL_HtmlDomManager.GetElementById(id) is RichTextBoxView view)
        {
            if (!view.IsScrollClient) return;

            string sDiv = Interop.GetVariableStringForJS(view.OuterDiv);
            double scrollLeft = Interop.ExecuteJavaScriptDouble($"{sDiv}.scrollLeft");
            double scrollTop = Interop.ExecuteJavaScriptDouble($"{sDiv}.scrollTop");

            view.UpdateOffsets(new Point(scrollLeft, scrollTop));
        }
    }

    private static string OnCopyNative(string id)
    {
        if (INTERNAL_HtmlDomManager.GetElementById(id) is RichTextBoxView view)
        {
            return view.OnCopy();
        }

        return null;
    }
}

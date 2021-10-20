

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


#if MIGRATION
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
#else
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#endif

#if MIGRATION
namespace Microsoft.Windows
#else
namespace System.Windows
#endif
{
    /// <summary>
    /// Provides helper methods and fields for drag-and-drop operations.
    /// </summary>
    public static class DragDrop
    {
        private static readonly IDictionary<DependencyObject, bool> _allowDrop = new Dictionary<DependencyObject, bool>();

        /// <summary>
        /// Gets a value indicating whether a drag is in progress.
        /// </summary>
        public static bool IsDragInProgress
        {
            get
            {
                return (Pointer.INTERNAL_captured != null);
            }
        }

        /// <summary>
        /// Attached property to allow for dropping other elements into it.
        /// </summary>
        public static readonly DependencyProperty AllowDropProperty =
            DependencyProperty.RegisterAttached(
            "AllowDrop",
            typeof(bool),
            typeof(DragDrop),
            new PropertyMetadata(false, AllowDropPropertyChanged));

        private static void AllowDropPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement && e?.NewValue is bool value)
            {
                SetAllowDrop(uiElement, value);
            }
        }

        /// <summary>
        /// Gets whether an UI element can be dropped on.
        /// </summary>
        /// <param name="element">The UI element.</param>
        /// <returns>Whether it can be dropped on.</returns>
        public static bool GetAllowDrop(UIElement element)
        {
            if (!_allowDrop.ContainsKey(element))
            {
                return false;
            }
            return _allowDrop[element];
        }

        /// <summary>
        /// Sets whether an UI element can be dropped on.
        /// </summary>
        /// <param name="element">The UI element.</param>
        /// <param name="value">Whether it can be dropped on.</param>
        public static void SetAllowDrop(UIElement element, bool value)
        {
            _allowDrop[element] = value;
        }
    }
}

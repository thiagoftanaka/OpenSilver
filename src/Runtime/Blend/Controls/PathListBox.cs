
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
using System.Windows;
using System.Windows.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Microsoft.Expression.Controls
{
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(PathListBoxItem))]
    public sealed class PathListBox : ListBox
    {
        public static readonly DependencyProperty LayoutPathsProperty = DependencyProperty.Register(nameof(LayoutPaths),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty StartItemIndexProperty = DependencyProperty.Register(nameof(StartItemIndex),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        //public static readonly DependencyProperty WrapItemsProperty = DependencyProperty.Register(nameof(WrapItems),
        //    typeof(FrameworkElement),
        //    typeof(LayoutPath),
        //    new PropertyMetadata());

        //public PathListBox() { }

        public LayoutPathCollection LayoutPaths { get; } = new LayoutPathCollection();

        public double StartItemIndex { get; set; }

        //public bool WrapItems { get; set; }

        //protected override DependencyObject GetContainerForItemOverride() { return default(DependencyObject); }

        //protected override bool IsItemItsOwnContainerOverride(object item) { return true; }
    }
}
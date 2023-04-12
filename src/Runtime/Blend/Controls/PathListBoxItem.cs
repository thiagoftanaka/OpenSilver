
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

#if MIGRATION
using System.Windows;
using System.Windows.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace Microsoft.Expression.Controls
{
    public sealed class PathListBoxItem : ListBoxItem, IPathLayoutItem
    {
        public static readonly DependencyProperty GlobalIndexProperty = DependencyProperty.Register(nameof(GlobalIndex),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty GlobalOffsetProperty = DependencyProperty.Register(nameof(GlobalOffset),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty IsArrangedProperty = DependencyProperty.Register(nameof(IsArranged),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty LayoutPathIndexProperty = DependencyProperty.Register(nameof(LayoutPathIndex),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty LocalIndexProperty = DependencyProperty.Register(nameof(LocalIndex),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty LocalOffsetProperty = DependencyProperty.Register(nameof(LocalOffset),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty NormalAngleProperty = DependencyProperty.Register(nameof(NormalAngle),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty OrientationAngleProperty = DependencyProperty.Register(nameof(OrientationAngle),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());

        public PathListBoxItem() { }

        public int GlobalIndex { get; }

        public double GlobalOffset { get; }

        public bool IsArranged { get; }

        public int LayoutPathIndex { get; }

        public int LocalIndex { get; }

        public double LocalOffset { get; }

        public double NormalAngle { get; }

        public double OrientationAngle { get; }

        public event EventHandler<PathLayoutUpdatedEventArgs> PathLayoutUpdated;

        public void Update(PathLayoutData data) { }
    }
}
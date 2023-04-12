
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

using System.ComponentModel;

#if MIGRATION
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.Expression.Controls
{
    public sealed class LayoutPath : DependencyObject
    {
        public static readonly DependencyProperty CapacityProperty = DependencyProperty.Register(nameof(Capacity),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty DistributionProperty = DependencyProperty.Register(nameof(Distribution),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty FillBehaviorProperty = DependencyProperty.Register(nameof(FillBehavior),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(nameof(Padding),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty SourceElementProperty = DependencyProperty.Register(nameof(SourceElement),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty SpanProperty = DependencyProperty.Register(nameof(Span),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register(nameof(Start),
            typeof(FrameworkElement),
            typeof(LayoutPath),
            new PropertyMetadata());

        public LayoutPath() { }


        public double ActualCapacity { get; }

        [TypeConverter(typeof(LayoutPathCapacityConverter))]
        public double Capacity { get; set; }

        public Distribution Distribution { get; set; }

        public FillBehavior FillBehavior { get; set; }

        public bool IsValid { get; }

        public Orientation Orientation { get; set; }

        public double Padding { get; set; }

        public FrameworkElement SourceElement { get; set; }

        public double Span { get; set; }

        public double Start { get; set; }
    }
}
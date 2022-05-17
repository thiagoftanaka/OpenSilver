﻿using System;

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    public partial class MenuItem : Button
    {

        //DependencyProperty defined in HeaderedItemsControl from which MenuItem inherits in Silverlight
        public static readonly DependencyProperty HeaderTemplateProperty;

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }
    }
}
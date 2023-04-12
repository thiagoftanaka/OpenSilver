
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
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Expression.Controls
{
    public sealed class LayoutPathCapacityConverter : TypeConverter
    {
        public LayoutPathCapacityConverter() { }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) { return true;  }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) { return null; }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) { return null;  }
    }
}
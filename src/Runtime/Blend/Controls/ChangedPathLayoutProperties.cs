
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

namespace Microsoft.Expression.Controls
{
    [Flags]
    public enum ChangedPathLayoutProperties
    {
        None = 0,
        LayoutPathIndex = 1,
        GlobalIndex = 2,
        LocalIndex = 4,
        GlobalOffset = 8,
        LocalOffset = 16,
        NormalAngle = 32,
        OrientationAngle = 64,
        IsArranged = 128
    }
}
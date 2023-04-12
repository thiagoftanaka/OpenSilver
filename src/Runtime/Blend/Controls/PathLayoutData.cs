
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

namespace Microsoft.Expression.Controls
{
    public class PathLayoutData
    {
        public PathLayoutData() { }

        public int GlobalIndex { get; set; }

        public double GlobalOffset { get; set; }

        public bool IsArranged { get; set; }

        public int LayoutPathIndex { get; set; }

        public int LocalIndex { get; set; }

        public double LocalOffset { get; set; }

        public double NormalAngle { get; set; }

        public double OrientationAngle { get; set; }
    }
}
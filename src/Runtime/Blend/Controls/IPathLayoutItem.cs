
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
    public interface IPathLayoutItem
    {
        int GlobalIndex { get; }

        double GlobalOffset { get; }

        bool IsArranged { get; }

        int LayoutPathIndex { get; }

        int LocalIndex { get; }

        double LocalOffset { get; }

        double NormalAngle { get; }

        double OrientationAngle { get; }

        event EventHandler<PathLayoutUpdatedEventArgs> PathLayoutUpdated;

        void Update(PathLayoutData data);
    }
}
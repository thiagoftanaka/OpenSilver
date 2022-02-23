

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


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#if MIGRATION
using System.Windows;
#endif

#if MIGRATION
namespace Microsoft.Windows
#else
namespace System.Windows
#endif
{
    /// <summary>
    /// Represents and identifies a routed event and declares its characteristics.
    /// </summary>
    /// <QualityBand>Experimental</QualityBand>
    public sealed class ExtendedRoutedEvent
    {
        /// <summary>
        /// Creates a new instance of the ExtendedRoutedEvent class.
        /// </summary>
        internal ExtendedRoutedEvent()
        {
        }
    }
}
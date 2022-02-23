

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


using System;

#if MIGRATION
namespace Microsoft.Windows
#else
namespace System.Windows
#endif
{
    /// <summary>
    /// Contains state information and event data associated with a routed event.
    /// </summary>
    /// <QualityBand>Experimental</QualityBand>
    public abstract class ExtendedRoutedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the present state of the 
        /// event handling for a routed event as it travels the route.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the original reporting source as determined by pure hit testing, before
        /// any possible System.Windows.RoutedEventArgs.Source adjustment by a parent
        /// class.
        /// </summary>
        public object OriginalSource { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the ExtendedRoutedEventArgs class.
        /// </summary>
        internal ExtendedRoutedEventArgs()
        {
        }
    }
}
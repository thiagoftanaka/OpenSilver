

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


namespace Microsoft.Windows
{
    /// <summary>
    /// An object that can handle routed drag and drop events.
    /// </summary>
    /// <QualityBand>Experimental</QualityBand>
    public interface IAcceptDrop
    {
        /// <summary>
        /// Initiates a DragEnter event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        void OnDragEnter(DragEventArgs args);

        /// <summary>
        /// Initiates a DragOver event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        void OnDragOver(DragEventArgs args);

        /// <summary>
        /// Initiates a DragLeave event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        void OnDragLeave(DragEventArgs args);

        /// <summary>
        /// Initiates a Drop event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        void OnDrop(DragEventArgs args);

        /// <summary>
        /// Initiates a GiveFeedback event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        void OnGiveFeedback(GiveFeedbackEventArgs args);

        /// <summary>
        /// Initiates a QueryContinueDrag event.
        /// </summary>
        /// <param name="args">Information about the event.</param>
        void OnQueryContinueDrag(QueryContinueDragEventArgs args);
    }
}
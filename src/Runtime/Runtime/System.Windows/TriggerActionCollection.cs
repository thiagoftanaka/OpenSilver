﻿
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

using System.Windows.Media.Animation;

namespace System.Windows
{
    /// <summary>
    /// Represents a collection of <see cref="BeginStoryboard"/> objects.
    /// </summary>
    public sealed class TriggerActionCollection : PresentationFrameworkCollection<TriggerAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerActionCollection"/> class.
        /// </summary>
        public TriggerActionCollection() { }

        internal override void AddOverride(TriggerAction value)
        {
            this.AddDependencyObjectInternal(value);
        }

        internal override void ClearOverride()
        {
            this.ClearDependencyObjectInternal();
        }

        internal override void InsertOverride(int index, TriggerAction value)
        {
            this.InsertDependencyObjectInternal(index, value);
        }

        internal override void RemoveAtOverride(int index)
        {
            this.RemoveAtDependencyObjectInternal(index);
        }

        internal override TriggerAction GetItemOverride(int index)
        {
            return this.GetItemInternal(index);
        }

        internal override void SetItemOverride(int index, TriggerAction value)
        {
            this.SetItemDependencyObjectInternal(index, value);
        }
    }
}

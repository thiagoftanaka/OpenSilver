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

using System.Diagnostics;
using System.Windows;

namespace OpenSilver.Internal
{
    internal sealed class VisualStatesCollection : PresentationFrameworkCollection<VisualState>
    {
        public VisualStatesCollection(VisualStateGroup owner)
        {
            Debug.Assert(owner != null);
            owner.ProvideSelfAsInheritanceContext(this, null);
        }

        internal override void AddOverride(VisualState value)
            => AddDependencyObjectInternal(value);

        internal override void ClearOverride()
            => ClearDependencyObjectInternal();

        internal override void InsertOverride(int index, VisualState value)
            => InsertDependencyObjectInternal(index, value);

        internal override void RemoveAtOverride(int index)
            => RemoveAtDependencyObjectInternal(index);

        internal override VisualState GetItemOverride(int index)
            => GetItemInternal(index);

        internal override void SetItemOverride(int index, VisualState value)
            => SetItemDependencyObjectInternal(index, value);
    }
}
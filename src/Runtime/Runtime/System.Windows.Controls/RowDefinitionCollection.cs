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

namespace System.Windows.Controls
{
    /// <exclude/>
    public sealed class RowDefinitionCollection : PresentationFrameworkCollection<RowDefinition>
    {
        private readonly Grid _parentGrid;

        internal RowDefinitionCollection(Grid parent)
            : base(true)
        {
            _parentGrid = parent;
            parent.ProvideSelfAsInheritanceContext(this, null);
        }

        internal override bool IsReadOnlyImpl => AreDefinitionsLocked();

        internal override void AddOverride(RowDefinition value)
        {
            VerifyWriteAccess();
            AddDependencyObjectInternal(value);
            value.SetParent(_parentGrid);
        }

        internal override void ClearOverride()
        {
            VerifyWriteAccess();

            if (_parentGrid != null)
            {
                foreach (RowDefinition column in this)
                {
                    column.SetParent(null);
                }
            }

            ClearDependencyObjectInternal();
        }

        internal override void InsertOverride(int index, RowDefinition value)
        {
            VerifyWriteAccess();
            value.SetParent(_parentGrid);
            InsertDependencyObjectInternal(index, value);
        }

        internal override void RemoveAtOverride(int index)
        {
            VerifyWriteAccess();
            RowDefinition removedRow = GetItemInternal(index);
            removedRow.SetParent(null);
            RemoveAtDependencyObjectInternal(index);
        }

        internal override RowDefinition GetItemOverride(int index) => GetItemInternal(index);

        internal override void SetItemOverride(int index, RowDefinition value)
        {
            VerifyWriteAccess();
            RowDefinition originalItem = GetItemInternal(index);
            originalItem.SetParent(null);
            SetItemDependencyObjectInternal(index, value);
        }

        private void VerifyWriteAccess()
        {
            if (AreDefinitionsLocked())
            {
                throw new InvalidOperationException("Cannot modify 'RowDefinitionCollection' in read-only state.");
            }
        }

        private bool AreDefinitionsLocked() =>
            _parentGrid is not null &&
            (_parentGrid.MeasureOverrideInProgress || _parentGrid.ArrangeOverrideInProgress);
    }
}

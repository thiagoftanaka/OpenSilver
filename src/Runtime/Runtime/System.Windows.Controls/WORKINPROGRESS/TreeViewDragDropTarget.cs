

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

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    public partial class TreeViewDragDropTarget : ItemsControlDragDropTarget<ItemsControl, TreeViewItem>
    {
        /// <inheritdoc/>
        protected override void AddItem(ItemsControl control, object data)
        {
            control.ItemsHost.Children.Add(data);
        }

        /// <inheritdoc/>
        protected override UIElement ContainerFromIndex(ItemsControl itemsControl, int index)
        {
            return itemsControl.ItemsHost.Children[index] as UIElement;
        }

        /// <inheritdoc/>
        protected override int? IndexFromContainer(ItemsControl itemsControl, UIElement itemContainer)
        {
            int index = itemsControl.ItemsHost.Children.IndexOf(itemContainer);
            return (index == -1) ? null : new int?(index);
        }

        /// <inheritdoc/>
        protected override void InsertItem(ItemsControl itemsControl, int index, object data)
        {
            itemsControl.ItemsHost.Children.Insert(index, data);
        }

        /// <inheritdoc/>
        protected override ItemsControl INTERNAL_ReturnNewTItemsControl()
        {
            return new TreeViewItem();
        }

        /// <inheritdoc/>
        protected override void RemoveItem(ItemsControl itemsControl, object data)
        {
            itemsControl.ItemsHost.Children.Remove(data);
        }

        /// <inheritdoc/>
        protected override void RemoveItemAtIndex(ItemsControl itemsControl, int index)
        {
            itemsControl.ItemsHost.Children.RemoveAt(index);
        }

        /// <inheritdoc/>
        internal override int INTERNAL_GetNumberOfElementsBetweenItemsRootAndDragDropTarget()
        {
            // Number of elements between this and the TreeViewItem being dragged
            return 14;
        }
    }
}

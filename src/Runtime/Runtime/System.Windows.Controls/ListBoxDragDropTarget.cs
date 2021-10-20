
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


#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    public sealed partial class ListBoxDragDropTarget : ItemsControlDragDropTarget<ListBox, ListBoxItem>
    {
        /// <summary>
        /// Adds an item at the last position of the item control.
        /// </summary>
        /// <param name="control">The item control.</param>
        /// <param name="data">The item to add.</param>
        protected override void AddItem(ListBox control, object data)
        {
            control.ItemsHost.Children.Add((UIElement)data);
        }

        /// <summary>
        /// Gets the item at an index of the item control.
        /// </summary>
        /// <param name="itemsControl">The item control.</param>
        /// <param name="index">The item index.</param>
        /// <returns>The item at the index, null otherwise.</returns>
        protected override UIElement ContainerFromIndex(ListBox itemsControl, int index)
        {
            if (itemsControl.ItemsHost.Children.Count > index)
            {
                return itemsControl.ItemsHost.Children[index];
            }
            return null;
        }

        /// <summary>
        /// Gets the index of an item on the item control.
        /// </summary>
        /// <param name="itemsControl">The item control.</param>
        /// <param name="itemContainer">The item.</param>
        /// <returns>Index of the item, null otherwise.</returns>
        protected override int? IndexFromContainer(ListBox itemsControl, UIElement itemContainer)
        {
            var index = itemsControl.ItemsHost.Children.IndexOf(itemContainer);
            return (index != -1) ? new int?(index) : null;
        }

        /// <summary>
        /// Inserts an item at an specific index.
        /// </summary>
        /// <param name="itemsControl">The item control.</param>
        /// <param name="index">The index to insert the item.</param>
        /// <param name="data">The item.</param>
        protected override void InsertItem(ListBox itemsControl, int index, object data)
        {
            itemsControl.ItemsHost.Children.Insert(index, data);
        }

        /// <summary>
        /// Gets a new instance of the item control.
        /// </summary>
        /// <returns>The item control.</returns>
        protected override ListBox INTERNAL_ReturnNewTItemsControl()
        {
            return new ListBox();
        }

        /// <summary>
        /// Removes an item from the item control.
        /// </summary>
        /// <param name="itemsControl">The item control.</param>
        /// <param name="data">The item to remove.</param>
        protected override void RemoveItem(ListBox itemsControl, object data)
        {
            itemsControl.ItemsHost.Children.Remove(data);
        }

        /// <summary>
        /// Removes an item at a specific index from the item control.
        /// </summary>
        /// <param name="itemsControl"> The item control.</param>
        /// <param name="index">The index to remove an item.</param>
        protected override void RemoveItemAtIndex(ListBox itemsControl, int index)
        {
            itemsControl.ItemsHost.Children.RemoveAt(index);
        }

        /// <summary>
        /// Gets the number of nested elements between a DragDropTarget and its root element
        /// (root inclusive, DragDropTarget exclusive).
        /// </summary>
        /// <returns>The number of elements.</returns>
        internal override int INTERNAL_GetNumberOfElementsBetweenItemsRootAndDragDropTarget()
        {
<<<<<<< HEAD
            // Number of elements between this and the dragged ListBoxItem
=======
>>>>>>> efad28bc (feat: implement ListBoxDragDropTarget)
            return 9;
        }
    }
}

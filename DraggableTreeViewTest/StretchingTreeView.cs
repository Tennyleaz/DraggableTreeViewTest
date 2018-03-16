using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DraggableTreeViewTest
{
    /// <summary>
    /// This is a workaround from:
    /// https://blogs.msdn.microsoft.com/jpricket/2008/08/05/wpf-a-stretching-treeview/
    /// </summary>
    class StretchingTreeView : TreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }

        /// <summary>
        /// This will clear selecteditem, and fire selectionchanged event.
        /// see: https://stackoverflow.com/questions/676819/wpf-treeview-clear-selection
        /// </summary>
        public void ClearSelection()
        {
            ItemCollection ic = base.Items;
            ItemContainerGenerator icg = base.ItemContainerGenerator;
            ClearTreeViewItemsControlSelection(ic, icg);
        }

        private void ClearTreeViewItemsControlSelection(ItemCollection ic, ItemContainerGenerator icg)
        {
            if ((ic != null) && (icg != null))
            {
                for (int i = 0; i < ic.Count; i++)
                {
                    TreeViewItem tvi = icg.ContainerFromIndex(i) as TreeViewItem;
                    if (tvi != null)
                    {
                        ClearTreeViewItemsControlSelection(tvi.Items, tvi.ItemContainerGenerator);
                        tvi.IsSelected = false;
                    }
                }
            }
        }
    }

    class StretchingTreeViewItem : TreeViewItem
    {
        public StretchingTreeViewItem()
        {
            this.Loaded += new RoutedEventHandler(StretchingTreeViewItem_Loaded);
        }

        private void StretchingTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            // The purpose of this code is to stretch the Header Content all the way accross the TreeView. 
            if (this.VisualChildrenCount > 0)
            {
                Grid grid = this.GetVisualChild(0) as Grid;
                if (grid != null && grid.ColumnDefinitions.Count == 3)
                {
                    // Remove the middle column which is set to Auto and let it get replaced with the 
                    // last column that is set to Star.
                    grid.ColumnDefinitions.RemoveAt(1);
                    //grid.Background = System.Windows.Media.Brushes.Yellow;
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }
    }
}

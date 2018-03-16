using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DraggableTreeViewTest
{
    public class MyTree
    {
        public MyTree()
        {
            rootNode = new MyNode(null);
            cateNames = new HashSet<string>();
        }
        public MyNode rootNode;
        private MyNode cutTemplate = null;
        private HashSet<string> cateNames;
        private string lastExpandedNodeID = null;

        /// <summary>
        /// Collapse the whole sub-tree containing lastExpandedNodeID.
        /// </summary>
        public void CollapseLastNodeOnExpand(string newGuid)
        {
            if (lastExpandedNodeID == null)
            {
                lastExpandedNodeID = newGuid;
                return;
            }
            if (newGuid == null)
                return;
            if (lastExpandedNodeID == newGuid)
                return;

            rootNode.CollapseOldNode(lastExpandedNodeID, newGuid);

            lastExpandedNodeID = newGuid;
        }

        public bool AddNodeUnderRoot(MyNode newNode)
        {
            /*if (cateNames.Contains(newNode.Name))
            {
                WSBSystem.WSSystem.GetSystem().ShowMessageBox("名稱重複了");
                return false;
            }*/
            rootNode.AddMember(newNode);
            //cateNames.Add(newNode.Name);
            return true;
        }

        public bool AddNodeAt(string guid, MyNode newNode)
        {
            /*if (cateNames.Contains(newNode.Name))
            {
                WSBSystem.WSSystem.GetSystem().ShowMessageBox("名稱重複了");
                return false;
            }
            cateNames.Add(newNode.Name);*/
            return rootNode.AddNodeAt(guid, newNode);
        }

        public bool DeleteNode(MyNode node)
        {
            if (rootNode.ID == node.ID)
                return false;
            //cateNames.Remove(node.Name);
            return rootNode.DeleteChild(node.ID);
        }

        public bool DeleteBranch(MyNode node)
        {
            return rootNode.DeleteChildBranch(node);
        }

        public bool RenameNode(string newName)
        {
            /*if (cateNames.Contains(newName))
            {
                WSBSystem.WSSystem.GetSystem().ShowMessageBox("名稱重複了");
                return false;
            }
            cateNames.Remove(newName);
            cateNames.Add(newName);*/
            //return rootNode.RenameNode(guid, newName);
            return true;
        }

        public bool MoveUp(MyNode node)
        {
            string parentID = node.ParentID;
            return rootNode.MoveUp(parentID, node);
        }

        public bool MoveDown(MyNode node)
        {
            string parentID = node.ParentID;
            return rootNode.MoveDown(parentID, node);
        }

        public void Cut(MyNode node)
        {
            cutTemplate = node;
            cutTemplate._parent = null;
            rootNode.DeleteChildBranch(node);
        }

        public bool Paste(string parentID)
        {
            if (rootNode.AddNodeAt(parentID, cutTemplate))
            {
                cutTemplate = null;
                return true;
            }
            return false;
        }
    }


    /// <summary>
    /// Implements an attached property used for styling TreeViewItems when
    /// they're a possible drop target.
    /// </summary>
    public static class TreeViewDropHighlighter
    {
        #region private variables
        /// <summary>
        /// the TreeViewItem that is the current drop target
        /// </summary>
        private static TreeViewItem _currentItem = null;

        /// <summary>
        /// Indicates whether the current TreeViewItem is a possible
        /// drop target
        /// </summary>
        private static bool _dropPossible;
        #endregion

        #region IsPossibleDropTarget
        /// <summary>
        /// Property key (since this is a read-only DP) for the IsPossibleDropTarget property.
        /// </summary>
        private static readonly DependencyPropertyKey IsPossibleDropTargetKey =
                                    DependencyProperty.RegisterAttachedReadOnly(
                                        "IsPossibleDropTarget",
                                        typeof(bool),
                                        typeof(TreeViewDropHighlighter),
                                        new FrameworkPropertyMetadata(null,
                                            new CoerceValueCallback(CalculateIsPossibleDropTarget)));


        /// <summary>
        /// Dependency Property IsPossibleDropTarget.
        /// Is true if the TreeViewItem is a possible drop target (i.e., if it would receive
        /// the OnDrop event if the mouse button is released right now).
        /// </summary>
        public static readonly DependencyProperty IsPossibleDropTargetProperty = IsPossibleDropTargetKey.DependencyProperty;

        /// <summary>
        /// Getter for IsPossibleDropTarget
        /// </summary>
        public static bool GetIsPossibleDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPossibleDropTargetProperty);
        }

        /// <summary>
        /// Coercion method which calculates the IsPossibleDropTarget property.
        /// </summary>
        private static object CalculateIsPossibleDropTarget(DependencyObject item, object value)
        {
            if ((item == _currentItem) && (_dropPossible))
                return true;
            else
                return false;
        }
        #endregion

        /// <summary>
        /// Initializes the <see cref="TreeViewDropHighlighter"/> class.
        /// </summary>
        static TreeViewDropHighlighter()
        {
            // Get all drag enter/leave events for TreeViewItem.
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.PreviewDragEnterEvent,
                                      new DragEventHandler(OnDragEvent), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.PreviewDragLeaveEvent,
                                      new DragEventHandler(OnDragLeave), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                                      TreeViewItem.PreviewDragOverEvent,
                                      new DragEventHandler(OnDragEvent), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem),
                TreeViewItem.PreviewDropEvent,
                new DragEventHandler(OnDragDrop), true);
        }

        #region event handlers
        /// <summary>
        /// Called when an item is dragged over the TreeViewItem.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        static void OnDragEvent(object sender, DragEventArgs args)
        {
            lock (IsPossibleDropTargetProperty)
            {
                _dropPossible = false;

                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsPossibleDropTargetProperty);
                }

                if (args.Effects != DragDropEffects.None)
                {
                    _dropPossible = true;
                }

                TreeViewItem tvi = sender as TreeViewItem;
                if (tvi != null)
                {
                    _currentItem = tvi;
                    // Tell that item to re-calculate the IsPossibleDropTarget property
                    _currentItem.InvalidateProperty(IsPossibleDropTargetProperty);
                }
            }
        }

        /// <summary>
        /// Called when the drag cursor leaves the TreeViewItem
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        static void OnDragLeave(object sender, DragEventArgs args)
        {
            lock (IsPossibleDropTargetProperty)
            {
                _dropPossible = false;

                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsPossibleDropTargetProperty);
                }

                TreeViewItem tvi = sender as TreeViewItem;
                if (tvi != null)
                {
                    _currentItem = tvi;
                    tvi.InvalidateProperty(IsPossibleDropTargetProperty);
                }
            }
        }

        static void OnDragDrop(object sender, DragEventArgs args)
        {
            lock (IsPossibleDropTargetProperty)
            {
                _dropPossible = false;

                if (_currentItem != null)
                {
                    _currentItem.InvalidateProperty(IsPossibleDropTargetProperty);
                }

                TreeViewItem tvi = sender as TreeViewItem;
                if (tvi != null)
                {
                    tvi.InvalidateProperty(IsPossibleDropTargetProperty);
                }
            }
        }
        #endregion
    }

    public static class MyTreeViewHelper
    {
        //
        // The TreeViewItem that the mouse is currently directly over (or null).
        //
        private static TreeViewItem _currentItem = null;

        //
        // IsMouseDirectlyOverItem:  A DependencyProperty that will be true only on the 
        // TreeViewItem that the mouse is directly over.  I.e., this won't be set on that 
        // parent item.
        //
        // This is the only public member, and is read-only.
        //

        // The property key (since this is a read-only DP)
        private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey =
            DependencyProperty.RegisterAttachedReadOnly("IsMouseDirectlyOverItem",
                                                typeof(bool),
                                                typeof(MyTreeViewHelper),
                                                new FrameworkPropertyMetadata(null, new CoerceValueCallback(CalculateIsMouseDirectlyOverItem)));

        // The DP itself
        public static readonly DependencyProperty IsMouseDirectlyOverItemProperty =
            IsMouseDirectlyOverItemKey.DependencyProperty;

        // A strongly-typed getter for the property.
        public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
        }

        // A coercion method for the property
        private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
        {
            // This method is called when the IsMouseDirectlyOver property is being calculated
            // for a TreeViewItem.  

            if (item == _currentItem)
                return true;
            else
                return false;
        }

        //
        // UpdateOverItem:  A private RoutedEvent used to find the nearest encapsulating
        // TreeViewItem to the mouse's current position.
        //

        private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent(
            "UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyTreeViewHelper));

        //
        // Class constructor
        //

        static MyTreeViewHelper()
        {
            // Get all Mouse enter/leave events for TreeViewItem.
            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseEnterEvent, new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeaveEvent, new MouseEventHandler(OnMouseTransition), true);

            // Listen for the UpdateOverItemEvent on all TreeViewItem's.
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UpdateOverItemEvent, new RoutedEventHandler(OnUpdateOverItem));
        }


        //
        // OnUpdateOverItem:  This method is a listener for the UpdateOverItemEvent.  When it is received,
        // it means that the sender is the closest TreeViewItem to the mouse (closest in the sense of the tree,
        // not geographically).

        static void OnUpdateOverItem(object sender, RoutedEventArgs args)
        {
            // Mark this object as the tree view item over which the mouse
            // is currently positioned.
            _currentItem = sender as TreeViewItem;

            // Tell that item to re-calculate the IsMouseDirectlyOverItem property
            _currentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);

            // Prevent this event from notifying other tree view items higher in the tree.
            args.Handled = true;
        }

        //
        // OnMouseTransition:  This method is a listener for both the MouseEnter event and
        // the MouseLeave event on TreeViewItems.  It updates the _currentItem, and updates
        // the IsMouseDirectlyOverItem property on the previous TreeViewItem and the new
        // TreeViewItem.

        static void OnMouseTransition(object sender, MouseEventArgs args)
        {
            lock (IsMouseDirectlyOverItemProperty)
            {
                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
                }

                // Get the element that is currently under the mouse.
                IInputElement currentPosition = Mouse.DirectlyOver;

                // See if the mouse is still over something (any element, not just a tree view item).
                if (currentPosition != null)
                {
                    // Yes, the mouse is over something.
                    // Raise an event from that point.  If a TreeViewItem is anywhere above this point
                    // in the tree, it will receive this event and update _currentItem.

                    RoutedEventArgs newItemArgs = new RoutedEventArgs(UpdateOverItemEvent);
                    currentPosition.RaiseEvent(newItemArgs);

                }
            }
        }
    }
}

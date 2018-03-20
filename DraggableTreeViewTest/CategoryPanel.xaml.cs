using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DraggableTreeViewTest
{
    /// <summary>
    /// CategoryPanel.xaml 的互動邏輯
    /// </summary>
    public partial class CategoryPanel : Window
    {
        public List<MyNode> pinnedCategory { get; private set; }
        private MyTree myTree;
        private MyNode draggedItem;
        private Point startPoint;

        public CategoryPanel()
        {
            InitializeComponent();
            Loaded += CategoryPanel_Loaded;
            // 編輯或選擇的事件
            FavoriteUncheckedListBox.SelectionChanged += FavoriteUncheckedListBox_SelectionChanged;
            tvCategory.PreviewMouseDoubleClick += tvCategory_MouseDoubleClick;
            tvCategory.SelectedItemChanged += tvCategory_SelectedItemChanged;
            tvCategory.MouseRightButtonUp += tvCategory_MouseRightButtonUp;
            // 拖曳用的事件
            tvCategory.PreviewMouseLeftButtonDown += tvCategory_PreviewMouseLeftButtonDown;
            tvCategory.PreviewMouseMove += tvCategory_MouseMove;
            tvCategory.Drop += tvCategory_Drop;
            tvCategory.DragEnter += tvCategory_DragEnter;
            tvCategory.DragOver += tvCategory_DragOver;

            pinnedCategory = new List<MyNode>();
            radioTree.IsChecked = true;
        }

        private void CategoryPanel_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateFakeNodes();
        }

        private void GenerateFakeNodes()
        {
            MyNode root = new MyNode(null) { Name = "所有聯絡人" };
            root.IsExpand = true;            

            MyNode others = new MyNode(root) { Name = "其他聯絡人" };
            root.AddMember(others);

            MyNode family1 = new MyNode(root) { Name = "The Doe's" };
            family1.AddMember(new MyNode(family1) { Name = "John Doe" });
            family1.AddMember(new MyNode(family1) { Name = "Jane Doe" });
            family1.AddMember(new MyNode(family1) { Name = "Sammy Doe" });

            MyNode f2 = new MyNode(family1) { Name = "Alice Doe" };
            MyNode f3 = new MyNode(f2) { Name = "Bob Doe" };
            f2.AddMember(f3);
            family1.AddMember(f2);


            MyNode family2 = new MyNode(root) { Name = "The Moe's" };
            family2.AddMember(new MyNode(family2) { Name = "Mark Moe" });
            family2.AddMember(new MyNode(family2) { Name = "Norma Moe" });
            root.AddMember(family1);
            root.AddMember(family2);


            List<MyNode> families = new List<MyNode>();
            families.Add(root);

            tvCategory.ItemsSource = families;

            myTree = new MyTree();
            myTree.rootNode = families[0];
        }

        private void FavoriteUncheckedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FavoriteUncheckedListBox.SelectedItem != null)
            {
                // 先取消tree的focuse
                tvCategory.ClearSelection();
            }
        }

        #region Tree item drag drop events
        private void tvCategory_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MyNode)))
            {
                TreeViewItem treeViewItem =
                    FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                MyNode dropTarget = treeViewItem.Header as MyNode;

                if (dropTarget != null && draggedItem != null && dropTarget.Equals(draggedItem))
                {
                    e.Effects = DragDropEffects.None;
                }
                else
                {
                    // 判斷樹深度是否超過三層，chung說client要自己先檢查一次
                    if (dropTarget.GetDepth() + draggedItem.GetSubTreeHeight() > 3)
                        e.Effects = DragDropEffects.None;
                    else
                        e.Effects = DragDropEffects.Move;
                    if (dropTarget.Members.Count > 0)
                        dropTarget.IsExpand = true;
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// The DragEnter event handler simply looks for the type of the data it gets from the DragEventArgs and 
        /// sets the drag drop effect to none when it is not of the expected type. 
        /// </summary>
        private void tvCategory_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(MyNode)) || sender != e.Source)
            {
                //e.Effects = DragDropEffects.None;
            }
        }

        private void tvCategory_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MyNode)))
            {
                var folderViewModel = e.Data.GetData(typeof(MyNode)) as MyNode;

                var treeViewItem =
                    FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                var dropTarget = treeViewItem.Header as MyNode;

                if (dropTarget == null || folderViewModel == null)
                    return;

                // process drop
                DropToTarget(dropTarget, folderViewModel);
                dropTarget.IsExpand = true;
            }
        }

        /// <summary>
        /// When the user moves the mouse the mouse move event handler is called and 
        /// calculates the difference between the starting point and the current point. 
        /// If the difference is greater than the given system parameters, 
        /// then it tries to get the underlying view model for the selected tree item and 
        /// stores it into a DataObject which can be passed to the DoDragDrop method.
        /// </summary>
        private void tvCategory_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePos = e.GetPosition(null);
                var diff = startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    TreeView treeView = sender as TreeView;
                    TreeViewItem treeViewItem = FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                    if (treeView == null || treeViewItem == null)
                        return;

                    draggedItem = treeView.SelectedItem as MyNode;
                    if (draggedItem == null)
                        return;

                    DataObject dragData = new DataObject(draggedItem);
                    MyNode currentItem = treeViewItem.DataContext as MyNode;
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        private void tvCategory_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        /// <summary>
        /// Cut the nodes/branch at movingNode, and paste under dropTarget.
        /// </summary>
        private void DropToTarget(MyNode dropTarget, MyNode movingNode)
        {
            if (dropTarget == null || movingNode == null)
                return;
            if (dropTarget.ID == movingNode.ID)  // same node cannot move
                return;
            if (movingNode.ID == myTree.rootNode.ID)  // root cannot move
                return;
            if (movingNode.ContainsNode(dropTarget.ID))  // cannot move into self child
                return;
            if (dropTarget.IsDirectChild(movingNode.ID))  // cannot move to same place
                return;
            if (dropTarget.ContainsSameDirectChildName(movingNode.Name))  // 目的類別，若是有同名的類別存在時無法搬移
                return;

            // 不能搬移至，我的最愛、未校正聯絡人、其他聯絡人及From別人幫掃的
            // note: chung說這些判斷能不能移動/修改，都只由server決定，cliet只負責告訴server說要放到哪裡去而已

            string s = "Move \"" + movingNode.Name + "\" under \"" + dropTarget.Name + "\" ？";
            if (MessageBox.Show(s, "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                myTree.Cut(movingNode);
                myTree.Paste(dropTarget.ID);
            }
            draggedItem = null;
        }

        /// <summary>
        /// Helper to search up the VisualTree
        /// </summary>
        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
        #endregion        

        #region Tree view edit events

        private void tvCategory_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MyNode selectedItem = tvCategory.SelectedItem as MyNode;
            if (selectedItem != null)
            {
                // 先取消上面兩個listbox的focuse
                FavoriteUncheckedListBox.SelectedItem = null;

                // change the selected category...
            }
        }

        private void tvCategory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;  // 避免double click去開關子目錄
        }

        /// <summary>
        /// Collapse other nodes when expand a new node.
        /// </summary>
        private void tvCategory_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = e.OriginalSource as TreeViewItem;
            if (treeViewItem != null && myTree != null)
            {
                MyNode node = treeViewItem.DataContext as MyNode;
                if (node != null)
                {
                    myTree.CollapseLastNodeOnExpand(node.ID);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 右鍵選單
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvCategory_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                // select the target category
                treeViewItem.Focus();
                e.Handled = true;

                // get target object
                MyNode targetNode = tvCategory.SelectedItem as MyNode;
                if (targetNode == null)
                    return;

                // make a contex menu
                ContextMenu categoryMenu = new ContextMenu();
                tvCategory.ContextMenu = categoryMenu;

                // add items
                MenuItem newMenuItem = new MenuItem();
                newMenuItem.Click += PinItem_Click;
                if (targetNode.IsPinned)
                    newMenuItem.Header = "取消釘選";
                else
                    newMenuItem.Header = "釘選";
                newMenuItem.IsEnabled = true;
                categoryMenu.Items.Add(newMenuItem);

                newMenuItem = new MenuItem();
                newMenuItem.Header = "釘選類別管理";
                newMenuItem.Click += PinManage_Click;
                categoryMenu.Items.Add(newMenuItem);

                Separator separator = new Separator();
                categoryMenu.Items.Add(separator);

                newMenuItem = new MenuItem();
                newMenuItem.Header = "變更類別";
                newMenuItem.Click += ChangeCategory_Click;
                categoryMenu.Items.Add(newMenuItem);

                newMenuItem = new MenuItem();
                newMenuItem.Header = "新增類別...";
                newMenuItem.Click += AddItem_Click;
                categoryMenu.Items.Add(newMenuItem);

                newMenuItem = new MenuItem();
                newMenuItem.Header = "變更位置";
                newMenuItem.Click += MoveItem_Click;
                categoryMenu.Items.Add(newMenuItem);

                newMenuItem = new MenuItem();
                newMenuItem.Header = "重新命名";
                newMenuItem.Click += RenameItem_Click;
                categoryMenu.Items.Add(newMenuItem);

                newMenuItem = new MenuItem();
                newMenuItem.Header = "刪除";
                newMenuItem.Click += DeleteItem_Click;
                categoryMenu.Items.Add(newMenuItem);

                // show it
                categoryMenu.IsOpen = true;
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            // get target object
            MyNode targetNode = tvCategory.SelectedItem as MyNode;
            if (targetNode != null)
            {

            }
        }

        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            // get target object
            MyNode targetNode = tvCategory.SelectedItem as MyNode;
            if (targetNode != null)
            {

            }
        }

        private void MoveItem_Click(object sender, RoutedEventArgs e)
        {
            // get target object
            MyNode targetNode = tvCategory.SelectedItem as MyNode;
            if (targetNode != null)
            {
                CategorySelectWindow mcw = new CategorySelectWindow(targetNode, tvCategory.ItemsSource as List<MyNode>, false);
                mcw.ShowDialog();

                // if success, refresh tree UI?
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            // get target object
            MyNode targetNode = tvCategory.SelectedItem as MyNode;
            if (targetNode != null)
            {

            }
        }

        private void PinItem_Click(object sender, RoutedEventArgs e)
        {
            // get target object
            MyNode targetNode = tvCategory.SelectedItem as MyNode;
            if (targetNode != null)
                targetNode.IsPinned = !targetNode.IsPinned;
        }

        private void PinManage_Click(object sender, RoutedEventArgs e)
        {
            PinnedCategoryManage pcm = new PinnedCategoryManage();
            pcm.ShowDialog();
        }

        private void ChangeCategory_Click(object sender, RoutedEventArgs e)
        {
            MyNode targetNode = tvCategory.SelectedItem as MyNode;
            if (targetNode != null)
            {
                CategoryMultiSelectWindow cmsw = new CategoryMultiSelectWindow(targetNode, false);
                cmsw.ShowDialog();
            }
        }

        #endregion

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void testBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mwnd = new MainWindow();
            mwnd.ShowDialog();
        }

        private void radioTree_Checked(object sender, RoutedEventArgs e)
        {
            SwitchTreeAndPinned(true);
        }

        private void radioPinned_Checked(object sender, RoutedEventArgs e)
        {
            SwitchTreeAndPinned(false);
        }

        private void SwitchTreeAndPinned(bool isTree)
        {
            if (isTree)
            {
                treeViewGrid.Visibility = Visibility.Visible;
                PinnedListBox.Visibility = Visibility.Collapsed;
            }
            else
            {

                treeViewGrid.Visibility = Visibility.Collapsed;
                PinnedListBox.Visibility = Visibility.Visible;
            }
        }
    }
}

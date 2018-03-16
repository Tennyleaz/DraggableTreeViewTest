using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DraggableTreeViewTest
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyTree myTree;
        private MyNode draggedItem;
        private Point startPoint;

        public MainWindow()
        {
            InitializeComponent();

            trvFamilies.PreviewMouseLeftButtonDown += Tree_PreviewMouseLeftButtonDown;
            trvFamilies.PreviewMouseMove += Tree_MouseMove;
            trvFamilies.Drop += DropTree_Drop;
            trvFamilies.DragEnter += DropTree_DragEnter;
            trvFamilies.DragOver += TreeViewItemDragOver;
            trvFamilies.PreviewMouseDoubleClick += TrvFamilies_MouseDoubleClick;
            trvFamilies.SelectedItemChanged += TrvFamilies_SelectedItemChanged;
        }

        private void GenerateFakeNodes()
        {
            MyNode root = new MyNode(null) { Name = "root" };
            root.IsExpand = true;

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

            trvFamilies.ItemsSource = families;

            myTree = new MyTree();
            myTree.rootNode = families[0];
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateFakeNodes();
        }        

        private void TrvFamilies_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MyNode selectedItem = trvFamilies.SelectedItem as MyNode;
            if (selectedItem != null)
            {
                if (selectedItem.Members.Count > 0)
                    DeleteBtn.IsEnabled = false;
                else
                    DeleteBtn.IsEnabled = true;
            }
        }

        private void TrvFamilies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RenameBtn_Click(sender, null);
            e.Handled = true;  // 避免double click去開關子目錄
        }

        #region Tree Drag Events
        private void TreeViewItemDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MyNode)))
            {
                var treeViewItem =
                    FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                var dropTarget = treeViewItem.Header as MyNode;

                //Family f = e.Data.GetData(typeof(Family)) as Family;
                /*if (dropTarget != null)
                    debugLabel2.Content = "drag over f=" + dropTarget.Name;
                else
                    debugLabel2.Content = "drag over f=null";*/
                if (dropTarget != null && draggedItem != null && dropTarget.Equals(draggedItem))
                {
                    e.Effects = DragDropEffects.None;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                    if (dropTarget.Members.Count > 0)
                        dropTarget.IsExpand = true;
                }
                e.Handled = true;
            }
        }


        /// <summary>
        /// The button down event handler is simply used to store the current mouse position. 
        /// </summary>
        private void Tree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        /// <summary>
        /// When the user moves the mouse the mouse move event handler is called and calculates the difference between the starting point and the current point. 
        /// If the difference is greater than the given system parameters, then it tries to get the underlying view model for the selected tree item and stores it into a DataObject which can be passed to the DoDragDrop method.
        /// </summary>
        private void Tree_MouseMove(object sender, MouseEventArgs e)
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

        /// <summary>
        /// The DragEnter event handler simply looks for the type of the data it gets from the DragEventArgs and sets the drag drop effect to none when it is not of the expected type. 
        /// </summary>
        private void DropTree_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(MyNode)) || sender != e.Source)
            {
                //e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// If the user releases the mouse button over an allowed drop target the drop event handler is called. 
        /// </summary>
        private void DropTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MyNode)))
            {
                var folderViewModel = e.Data.GetData(typeof(MyNode)) as MyNode;

                var treeViewItem =
                    FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                var dropTarget = treeViewItem.Header as MyNode;

                if (dropTarget == null || folderViewModel == null)
                    return;

                //folderViewModel.Parent = dropTarget;
                string s = folderViewModel.Name + ".Parent = " + dropTarget.Name;
                debugLabel.Content = s;
                DropToTarget(dropTarget, folderViewModel);
                dropTarget.IsExpand = true;
            }
        }

        /// <summary>
        /// Cut the nodes/branch at movingNode, and paste under dropTarget.
        /// </summary>
        private void DropToTarget(MyNode dropTarget, MyNode movingNode)
        {
            if (dropTarget == null || movingNode == null)
                return;
            if (dropTarget.ID == movingNode.ID)
                return;
            if (movingNode.ID == myTree.rootNode.ID)
                return;
            if (movingNode.ContainsNode(dropTarget.ID))
                return;
            if (dropTarget.IsDirectChild(movingNode.ID))
                return;

            string s = "Move \"" + movingNode.Name + "\" under \"" + dropTarget.Name + "\" ？";
            if (MessageBox.Show(s, "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                myTree.Cut(movingNode);
                myTree.Paste(dropTarget.ID);
            }
            draggedItem = null;
        }

        // Helper to search up the VisualTree
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

        #region Button Events

        private void AddCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            MyNode f = trvFamilies.SelectedItem as MyNode;
            if (f != null)
            {
                EditDlg newDlg = new EditDlg("New Tree Node");
                newDlg.Owner = GetWindow(this);
                newDlg.ShowDialog();
                string newName = newDlg.strCategoryName;
                MyNode newNode = new MyNode(f) { Name = newName };

                // 檢查新名稱是否重複

                myTree.AddNodeAt(f.ID, newNode);
                f.IsExpand = true;
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            MyNode f = trvFamilies.SelectedItem as MyNode;
            if (f != null)
            {
                myTree.DeleteNode(f);
            }
        }

        private void RenameBtn_Click(object sender, RoutedEventArgs e)
        {
            MyNode f = trvFamilies.SelectedItem as MyNode;

            if (f != null)
            {
                EditDlg aDlg = new EditDlg(f.Name);
                aDlg.Owner = GetWindow(this);
                aDlg.ShowDialog();
                if (aDlg.strCategoryName.Length > 0)
                {
                    // 確認是否存在此類別
                    bool bExist = false;
                    /*foreach (var aCategory in m_Category_Tmp)
                    {
                        if (aCategory.CategoryName == aDlg.strCategoryName)
                        {
                            bExist = true;
                            break;
                        }
                    }*/

                    if (bExist)
                    {
                        MessageBox.Show("This name already exist!");
                    }
                    else
                    {
                        f.Name = aDlg.strCategoryName;
                    }
                }
            }
        }

        private void MoveUpBtn_Click(object sender, RoutedEventArgs e)
        {
            MyNode f = trvFamilies.SelectedItem as MyNode;
            if (f != null)
            {
                myTree.MoveUp(f);
            }
        }

        private void MoveDownBtn_Click(object sender, RoutedEventArgs e)
        {
            MyNode f = trvFamilies.SelectedItem as MyNode;
            if (f != null)
            {
                myTree.MoveDown(f);
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            MyNode f = trvFamilies.SelectedItem as MyNode;
            if (f != null)
            {
                myTree.Cut(f);
                Paste.IsEnabled = true;
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            MyNode f = trvFamilies.SelectedItem as MyNode;
            if (f != null)
            {
                myTree.Paste(f.ID);
                Paste.IsEnabled = false;
            }
        }

        private void DefaultBtn_Click(object sender, RoutedEventArgs e)
        {
            GenerateFakeNodes();
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            //WSSystem.GetSystem().GetEvent().NotifyReloadLocalCategory(null);
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PinBtn_Click(object sender, RoutedEventArgs e)
        {
            MyNode mynode = trvFamilies.SelectedItem as MyNode;
            if (mynode != null)
            {
                mynode.IsPinned = !mynode.IsPinned;
            }
        }
        #endregion

        /// <summary>
        /// Collapse other nodes when expand a new node.
        /// </summary>
        private void trvFamilies_Expanded(object sender, RoutedEventArgs e)
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
    }    
}

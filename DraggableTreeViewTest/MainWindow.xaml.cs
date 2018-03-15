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

    public class MyNode : INotifyPropertyChanged, IEquatable<MyNode>
    {
        public MyNode(MyNode Parent, string guid = null, string name = null)
        {
            this.Members = new ObservableCollection<MyNode>();
            if (guid != null)
                this.ID = guid;
            else
                this.ID = Guid.NewGuid().ToString();

            if (name != null)
                Name = name;
            allChilrenID = new HashSet<string>();
            IsExpand = false;
            allChilrenID.Add(this.ID);
            _parent = Parent;
        }

        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public string ID { get; set; }
        public string ParentID { get; set; }
        public MyNode _parent;
        public bool IsExpand
        {
            get { return _isExpand; }
            set
            {
                _isExpand = value;
                OnPropertyChanged("IsExpand");
            }
        }
        public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
                _isPinned = value;
                OnPropertyChanged("IsPinned");
            }
        }

        private HashSet<string> allChilrenID;
        private int position = 0;
        private string _name;
        private bool _isExpand;
        private bool _isPinned;

        public ObservableCollection<MyNode> Members { get; set; }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool Equals(MyNode other)
        {
            if (other == null)
                return false;
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as MyNode);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public void AddMember(MyNode childNode)
        {
            Members.Add(childNode);
            childNode.ParentID = ID;
            childNode._parent = this;
            foreach (string guid in childNode.allChilrenID)
            {
                allChilrenID.Add(guid);
            }
        }

        public bool MoveUp(string parentID, MyNode child)
        {
            if (ID == parentID)
            {
                // 移動自己的直屬child
                return MoveChildUp(child);
            }
            else if (!allChilrenID.Contains(parentID))
                return false;
            else
            {
                // 叫自己的Child想辦法移動
                for (int i = 0; i < Members.Count; i++)
                {
                    // 尋找包含id的child
                    if (Members[i].ContainsNode(parentID))
                    {
                        return Members[i].MoveUp(parentID, child);
                    }
                }
            }
            return false;
        }

        public bool MoveDown(string parentID, MyNode child)
        {
            if (ID == parentID)
            {
                // 移動自己的直屬child
                return MoveChildDown(child);
            }
            else if (!allChilrenID.Contains(parentID))
                return false;
            else
            {
                // 叫自己的Child想辦法移動
                for (int i = 0; i < Members.Count; i++)
                {
                    // 尋找包含id的child
                    if (Members[i].ContainsNode(parentID))
                    {
                        return Members[i].MoveDown(parentID, child);
                    }
                }
            }
            return false;
        }

        public MyNode GetNode(string guid)
        {
            if (ID == guid)
            {
                return this;
            }
            else if (!allChilrenID.Contains(guid))
                return null;
            else
            {
                // 重新命名自己的Child
                foreach (MyNode child in Members)
                {
                    // 尋找包含id的child
                    if (child.ContainsNode(guid))
                    {
                        return child.GetNode(guid);
                    }
                }
            }
            return null;
        }

        private bool MoveChildUp(MyNode child)
        {
            if (ID == child.ID)
                return false; // 不能動自己
            else if (!Members.Contains(child))
                return false;
            else
            {
                int index = Members.IndexOf(child);
                if (index - 1 >= 0)
                {
                    Members.Move(index, index - 1);
                    return true;
                }
            }
            return false;
        }

        private bool MoveChildDown(MyNode child)
        {
            if (ID == child.ID)
                return false; // 不能動自己
            else if (!Members.Contains(child))
                return false;
            else
            {
                int index = Members.IndexOf(child);
                if (index + 1 < Members.Count)
                {
                    Members.Move(index, index + 1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 所有分支內children中是否包含id
        /// </summary>
        public bool ContainsNode(string id)
        {
            if (ID == id)
                return true;
            else
                return allChilrenID.Contains(id);
        }

        public bool AddNodeAt(string guid, MyNode newNode)
        {
            if (ID == guid)
            {
                // 加在自己下面
                AddMember(newNode);
                return true;
            }
            else if (!allChilrenID.Contains(guid))
                return false;
            else
            {
                // 加在自己的Child內
                foreach (MyNode child in Members)
                {
                    // 尋找包含id的child
                    if (child.ContainsNode(guid))
                    {
                        if (child.AddNodeAt(guid, newNode))
                        {
                            allChilrenID.Add(newNode.ID); // 加成功了再更新ID
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool DeleteChild(string guid)
        {
            if (ID == guid)
            {
                return false;  // 不能刪除自己
            }
            else if (!allChilrenID.Contains(guid))
                return false;
            else
            {
                // 從自己的child刪除
                for (int i = Members.Count - 1; i >= 0; i--)
                {
                    // 直接刪除某個child
                    if (Members[i].ID == guid)
                    {
                        if (Members[i].Members.Count > 0)  // 仍然有子類別就不能刪除
                            return false;
                        else
                        {
                            Members.RemoveAt(i);
                            return allChilrenID.Remove(guid);
                        }
                    }

                    // 尋找包含id的child
                    if (Members[i].ContainsNode(guid))
                    {
                        if (Members[i].DeleteChild(guid))
                            return allChilrenID.Remove(guid); // 加成功了再更新ID
                    }
                }
            }
            return false;
        }

        public bool DeleteChildBranch(MyNode child)
        {
            if (ID == child.ID)
            {
                return false;  // 不能刪除自己
            }
            else if (!allChilrenID.Contains(child.ID))
                return false;
            else
            {
                try
                {
                    // 從自己的child刪除
                    for (int i = Members.Count - 1; i >= 0; i--)
                    {
                        // 直接刪除某個child
                        if (Members[i].ID == child.ID)
                        {
                            foreach (string id in child.allChilrenID)
                                allChilrenID.Remove(id);  // 移除所有child ID
                            Members.RemoveAt(i);
                            return true;
                        }

                        // 尋找包含id的child
                        if (Members[i].ContainsNode(child.ID))
                        {
                            if (Members[i].DeleteChildBranch(child))
                            {
                                foreach (string id in child.allChilrenID)
                                    allChilrenID.Remove(id);  // 移除所有child ID
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return false;
        }

        public bool IsDirectChild(string id)
        {
            foreach (var child in Members)
            {
                if (child.ID == id)
                    return true;
            }
            return false;
        }

        public bool RenameNode(string guid, string newName)
        {
            if (ID == guid)
            {
                // 重新命名自己
                this.Name = newName;
                return true;
            }
            else if (!allChilrenID.Contains(guid))
                return false;
            else
            {
                // 重新命名自己的Child
                foreach (MyNode child in Members)
                {
                    // 尋找包含id的child
                    if (child.ContainsNode(guid))
                    {
                        return child.RenameNode(guid, newName);
                    }
                }
            }
            return false;
        }

        public void CollapseOldNode(string oldGuid, string newGuid)
        {
            if (allChilrenID.Contains(oldGuid))
            {
                // 不含newGuid，直接關閉整條Branch
                if (IsExpand)
                {                    
                    if (oldGuid != ID)
                    {
                        // 含oldGuid，且是自己的child，要關掉它
                        foreach (MyNode child in Members)
                        {
                            // 尋找包含id的child
                            if (child.ContainsNode(oldGuid))
                            {
                                child.CollapseOldNode(oldGuid, newGuid);
                                break;
                            }
                        }
                    }

                    if (allChilrenID.Contains(newGuid))
                    {
                        // 含newGuid，則不關閉自己
                    }
                    else
                    {
                        IsExpand = false;  // 關閉自己
                    }
                }
            }
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
}

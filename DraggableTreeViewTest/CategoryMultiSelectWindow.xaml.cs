using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// CategoryMultiSelectWindow.xaml 的互動邏輯
    /// </summary>
    public partial class CategoryMultiSelectWindow : Window
    {
        private MyTree myTree;
        private MyNode _targetNode;        
        private bool _isRootCheckable;

        public CategoryMultiSelectWindow(MyNode targetNode, bool isRootCheckable)
        {
            InitializeComponent();
            Loaded += CategoryMultiSelectWindow_Loaded;
            _targetNode = targetNode;
            _isRootCheckable = isRootCheckable;
        }

        private void CategoryMultiSelectWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateFakeNodes();
            // 由單筆資料變更類別時，則開啟變更類別介面，須標示出該筆資料的原本所屬類別資訊，讓用戶勾選類別
        }

        private void GenerateFakeNodes()
        {
            myTree = new MyTree();

            MyNode root = new MyNode(null) { Name = "所有聯絡人" };
            root.IsExpand = true;
            root.IsCheckboxAvailable = _isRootCheckable;  // 變更自己類別時候所有聯絡人不能選，變更秘書卡片的時候可以額外勾選root node

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


            //List<MyNode> families = new List<MyNode>();
            //families.Add(root);

            tvCategory.ItemsSource = myTree.RootNodes;
            myTree.Clear();
            myTree.RootNodes.Add(root);
        }

        private void tvCategory_Expanded(object sender, RoutedEventArgs e)
        {
            
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            MyNode newParent = tvCategory.SelectedItem as MyNode;
            if (newParent != null)
            {
                // note: chung說這些判斷能不能移動/修改，都只由server決定，cliet只負責告訴server說要放到哪裡去而已

                // find all checked category IDs

                // send to server                

                // return
                Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

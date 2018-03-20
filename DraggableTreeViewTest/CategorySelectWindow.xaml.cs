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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DraggableTreeViewTest
{
    /// <summary>
    /// MoveCategoryWindow.xaml 的互動邏輯
    /// </summary>
    public partial class CategorySelectWindow : Window
    {
        public string resultGuid;

        private MyNode _targetNode;
        private MyTree myTree;
        private List<MyNode> _vCategories;
        private bool _isPinManage;

        public CategorySelectWindow(MyNode targetNode, List<MyNode> vCategories, bool isPinManage)
        {
            InitializeComponent();
            Loaded += MoveCategoryWindow_Loaded;
            _targetNode = targetNode;
            _vCategories = vCategories;
            _isPinManage = isPinManage;
        }

        private void MoveCategoryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // load real-time category tree here
            if (_isPinManage)
            {
                //CancelBtn.Visibility = Visibility.Hidden;
                //// move the ok button to center
                //Thickness newMargin = OKBtn.Margin;
                //newMargin.Bottom = 0;
                //newMargin.Top = 0;
                //newMargin.Left = 0;
                //newMargin.Right = 0;
                //OKBtn.Margin = newMargin;
                //OKBtn.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                tvCategory.ItemsSource = _vCategories;
                myTree = new MyTree();
                myTree.rootNode = _vCategories[0];
            }
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
                // check if newParent is in target's subtree
                if (_targetNode.ContainsNode(newParent.ID))
                {
                    MessageBox.Show("Cannot move to self!");
                    return;
                }

                // check invalid parent id

                // send to server                

                // return
                resultGuid = newParent.ID;
                Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

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
    /// EditDlg.xaml 的互動邏輯
    /// </summary>
    public partial class EditDlg : Window
    {
        string _strCategoryName = "";
        public string strCategoryName
        {
            get { return _strCategoryName; }
        }

        public EditDlg(string oldName)
        {
            InitializeComponent();

            TB_categoryName.Text = oldName;
            TB_categoryName.SelectAll();
            TB_categoryName.Focus();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TB_categoryName.Text.Trim().Length > 0 && TB_categoryName.Text.Trim().Length <= 40)
            {
                _strCategoryName = TB_categoryName.Text;
                this.Close();
            }
            else
            {
                TextBlock_msg.Visibility = Visibility.Visible;
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _strCategoryName = "";
            this.Close();
        }
    }
}

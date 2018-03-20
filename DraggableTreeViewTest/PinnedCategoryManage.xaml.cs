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
    /// PinnedCategoryManage.xaml 的互動邏輯
    /// </summary>
    public partial class PinnedCategoryManage : Window
    {
        public PinnedCategoryManage()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // read all pinned category
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            // there is no way to cancel, this window do real-time modify.
            Close();
        }

        private void AddCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            // select an existing category
            CategorySelectWindow csw = new CategorySelectWindow(null, null, true);
            csw.ShowDialog();

            // get seleted id, and send to server
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoveUpBtn_Click(object sender, RoutedEventArgs e)
        {
            SwitchListItem(ContactListBox.SelectedIndex, ContactListBox.SelectedIndex - 1);
        }

        private void MoveDownBtn_Click(object sender, RoutedEventArgs e)
        {
            SwitchListItem(ContactListBox.SelectedIndex, ContactListBox.SelectedIndex + 1);
        }

        private void ContactListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactListBox.SelectedIndex <= 0)
                MoveUpBtn.IsEnabled = false;
            else
                MoveUpBtn.IsEnabled = true;

            if (ContactListBox.SelectedIndex == ContactListBox.Items.Count - 1)
                MoveDownBtn.IsEnabled = false;
            else
                MoveDownBtn.IsEnabled = true;

            if (ContactListBox.SelectedIndex >= 0 && ContactListBox.SelectedIndex < ContactListBox.Items.Count)
            {
                //Category aItem = ContactListBox.SelectedItem as Category;
                //if (aItem.CategoryID == WCTDefaultCategory.defaultCate.All.CategoryID || aItem.CategoryID == WCTDefaultCategory.defaultCate.Other.CategoryID)
                //{
                //    DeleteBtn.IsEnabled = false;
                //}
                //else
                //{
                //    DeleteBtn.IsEnabled = true;
                //}
            }
        }

        private void SwitchListItem(int selectIndex, int changeIndex)
        {

        }
    }
}

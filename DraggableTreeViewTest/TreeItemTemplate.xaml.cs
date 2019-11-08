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
    /// TreeItemTemplate.xaml 的互動邏輯
    /// </summary>
    public partial class TreeItemTemplate : UserControl
    {
        public TreeItemTemplate()
        {
            InitializeComponent();
        }

        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            /// Update border width by text value,
            /// see: https://stackoverflow.com/a/32233156/3576052
            string value = tbCount.Text;
            if (value.ToString().Length >= 4)
                Border_Count.Width = (value.ToString().Length) * 10 + 2;
            else
                Border_Count.Width = 35;
        }
    }
}

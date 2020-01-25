using MonoMax.Studio.Contracts;
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

namespace MonoMax.Studio.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void treeview_Selected(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;

            if(tvi != null && (treeview.SelectedItem as Node)?.Header != "Root")
            {
                var pos = tvi.TranslatePoint(new Point(), PositionCanvas);

                Canvas.SetLeft(SuggestionPopup, pos.X + 19 + 100);
                Canvas.SetTop(SuggestionPopup, pos.Y + 36);


            }

            e.Handled = true;
        }

        private void treeview_Unselected(object sender, RoutedEventArgs e)
        {

        }
    }
}

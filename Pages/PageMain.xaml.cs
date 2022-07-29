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

namespace GraphicalMirai.Pages
{
    /// <summary>
    /// PageMain.xaml 的交互逻辑
    /// </summary>
    public partial class PageMain : Page
    {
        public PageMain()
        {
            InitializeComponent();
            frame.Navigate(App.PageMainConsole);
            listBoxItemConsole.Selected += delegate { frame.Navigate(App.PageMainConsole); };
        }

        private void BtnPluginCenter_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(App.PagePluginCenter);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(App.PageMainLogin);
            listBox.SelectedIndex = -1;
        }
    }
}

using System.Windows;
using System.Windows.Controls;

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

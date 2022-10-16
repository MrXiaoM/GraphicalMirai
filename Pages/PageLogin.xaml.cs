using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace GraphicalMirai.Pages
{
    /// <summary>
    /// PageLogin.xaml 的交互逻辑
    /// </summary>
    public partial class PageLogin : Page
    {
        public PageLogin()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string qq = textQQ.Text;
            string password = textPW.Password;
            App.PageMain.frame.Navigate(App.PageMainConsole);
            App.mirai.WriteLine($"/login {qq} {password}");
        }

        private void BtnAutoLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow.Msg.ShowAsync("敬请期待", "未实现");
        }

        private void BtnDevice_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow.Msg.ShowAsync("敬请期待", "未实现");
        }
    }
}

using ICSharpCode.SharpZipLib.Zip;
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
            textQQ.Text = textPW.Password = "";
            App.PageMain.listBox.SelectedIndex = 0;
            if (CheckAutoLogin.IsChecked ?? false)
            {
                bool useMD5 = CheckUseMD5.IsChecked ?? false;
                if (useMD5) password = App.MD5(password);
                App.mirai.WriteLine($"/autologin add {qq} {password}" + (useMD5 ? " md5":""));
                App.mirai.WriteLine($"/login {qq}");
            }
            else App.mirai.WriteLine($"/login {qq} {password}");
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

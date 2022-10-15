using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LoginSolver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 这是从我的手机里提取的 UA
        /// 虽然腾讯可能不会检查 UA，但还是提前做好设备仿真比较好
        /// 
        /// 手机 QQ 内置浏览器访问 https://ie.icoa.cn/ 即可获取 UA
        /// </summary>
        private static string userAgent = "Mozilla/5.0 (Linux; Android 9; Redmi 6A Build/PPR1.180610.011; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/105.0.5195.136 Mobile Safari/537.36 V1_AND_SQ_8.4.1_1442_YYB_D QQ/8.4.1.4680 NetType/4G WebP/0.4.1 Pixel/720 StatusBarHeight/49 SimpleUISwitch/0 QQTheme/2040";
        public static string UserAgent => userAgent;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string url = "";
            string[] args = e.Args;
            for (int i = 0; i < args.Length; i++)
            {
                string s = args[i];
                if (s.StartsWith("--url="))
                {
                    url = Encoding.UTF8.GetString(Convert.FromBase64String(s.Substring(6)));
                }
                if (s.StartsWith("--user-agent="))
                {
                    userAgent = Encoding.UTF8.GetString(Convert.FromBase64String(s.Substring(13)));
                }
            }
            if (url.Length > 0)
            {
                Console.WriteLine("url: " + url);
                this.MainWindow = new MainWindow(url);
                this.MainWindow.Show();
            }
            else
            {
                Console.WriteLine("Parameter miss: --url=(base64 encoded url)");
                Environment.Exit(-1);
            }
        }
    }
}

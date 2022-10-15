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
            }
            if (url.Length > 0)
            {
                Console.WriteLine("url: " + url);
                this.MainWindow = new MainWindow(url);
                this.MainWindow.Show();
            }
            else
            {
                Console.WriteLine("Parameter miss: --url=");
                Environment.Exit(-1);
            }
        }
    }
}

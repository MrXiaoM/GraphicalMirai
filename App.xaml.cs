using GraphicalMirai.Pages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GraphicalMirai
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mirai? mirai;
        // 储存一些仅会用到一次的页面实例。
        private static PageInit? pageInit;
        public static PageInit PageInit
        {
            get { return pageInit ??= new PageInit(); }
        }
        private static PageMain? pageMain;
        public static PageMain PageMain
        {
            get { return pageMain ??= new PageMain(); }
        }
        private static PageConsole? pageMainConsole;
        public static PageConsole PageMainConsole
        {
            get { return pageMainConsole ??= new PageConsole(); }
        }
        private static PageLogin? pageMainLogin;
        public static PageLogin PageMainLogin
        {
            get { return pageMainLogin ??= new PageLogin(); }
        }
        private static PagePluginCenter? pagePluginCenter;
        public static PagePluginCenter PagePluginCenter
        {
            get { return pagePluginCenter ??= new PagePluginCenter(); }
        }

        public static long NowTimestamp { get { return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(); } }
        public static DateTime FromTimestamp(long time)
        {
            return new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(time));
        }

        private static readonly string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
        public static string SizeToString(double? size, uint point = 2)
        {
            if (size == null || size <= 0) return "?";
            string unit = units[0];
            int i = 0;
            while (size > 1024)
            {
                i++;
                if (i >= units.Length) break;
                size /= 1024;
                unit = units[i];
            }
            return string.Format("{0:N" + point + "}", size) + unit;
        }

        public static void openUrl(string s)
        {
            if (s.StartsWith("https://") || s.StartsWith("http://"))
                System.Diagnostics.Process.Start("explorer", s);
        }

        public static bool exists(string path) { return exists(new string[] { path }); }

        public static bool exists(string[] path)
        {
            foreach (string p in path)
            {
                if (!File.Exists(App.path(p))) return false;
            }
            return true;
        }

        public static void mkdir(string path)
        {
            if (!Directory.Exists(App.path(path))) {
                Directory.CreateDirectory(App.path(path));
            }
        }

        public static string path(string path)
        {
            return Environment.CurrentDirectory + "\\" + path.Replace("/", "\\");
        }

        public static void resetDir(string path)
        {
            path = App.path(path);
            if (Directory.Exists(path)) Directory.Delete(path, true);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static SolidColorBrush hexBrush(string s)
        {
            return new SolidColorBrush(hex(s));
        }

        public static Color hex(string s)
        {
            return (Color) ColorConverter.ConvertFromString(s);
        }

        public static string MD5(string s)
        {
            string result = "";
            foreach (byte b in System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(s))) result += b.ToString("x2");
            return result;
        }
    }
}

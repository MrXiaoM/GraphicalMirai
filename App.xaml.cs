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
        // 储存一些单例页面实例。
        private static PageInit? pageInit = PageInit;
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
        private static PagePluginCenter? pagePluginCenter = PagePluginCenter;
        public static PagePluginCenter PagePluginCenter
        {
            get { return pagePluginCenter ??= new PagePluginCenter(); }
        }

        public static long NowTimestamp { get { return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(); } }
        public static DateTime FromTimestamp(long time)
        {
            return new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(time));
        }

        public static string FormatTimestamp(long time)
        {
            TimeSpan span = TimeSpan.FromMilliseconds(NowTimestamp - time);
            if (span.TotalMinutes < 60)
                return "大约" + Math.Ceiling(span.TotalMinutes).ToString("0") + "分钟之前";
            else if (span.TotalHours < 24)
                return "大约" + Math.Ceiling(span.TotalHours).ToString("0") + "小时之前";
            else if (span.TotalDays < 30)
                return Math.Ceiling(span.TotalDays).ToString("0") + "天之前";
            else
                return FromTimestamp(time).ToString("yyyy年MM月dd日 HH:mm");
        }

        private static readonly string[] SIZE_UNITS = { "B", "KB", "MB", "GB", "TB" };
        public static string FormatSize(double? size, uint point = 2)
        {
            if (size == null || size <= 0) return "?";
            string unit = SIZE_UNITS[0];
            int i = 0;
            while (size > 1024)
            {
                i++;
                if (i >= SIZE_UNITS.Length) break;
                size /= 1024;
                unit = SIZE_UNITS[i];
            }
            return string.Format("{0:N" + point + "}", size) + unit;
        }
        private static readonly string[] NUMBER_UNITS = { "", "K", "M", "B" };
        public static string FormatNumber(double? number, uint point = 1)
        {
            if (number == null) return "?";
            if (number < 1000) return number.GetValueOrDefault(0).ToString();
            string unit = NUMBER_UNITS[0];
            int i = 0;
            while (number >= 1000)
            {
                i++;
                if (i >= SIZE_UNITS.Length) break;
                number /= 1000;
                unit = SIZE_UNITS[i];
            }
            return string.Format("{0:N" + point + "}", number) + unit;
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

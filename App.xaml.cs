using GraphicalMirai.LoginSolver;
using GraphicalMirai.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using File = System.IO.File;

namespace GraphicalMirai
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ASCIIEncoding ASCII = new();

        public static readonly string UserAgent = "GraphicalMirai/" + ((object?)System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? "1.0.0").ToString() +
            " Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.81 Safari/537.36 Edg/104.0.1293.47";
        private static Lazy<Mirai> miraiLazy = new();
        public static Mirai mirai => miraiLazy.Value;

        public App()
        {
#if !DEBUG
            DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif
            // 初始化登录处理器
            LoginSolverSetup.Instance.Setup();
        }
#if !DEBUG
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            if (!ex.StackTrace?.Contains("GraphicalMirai.InnerMessageBox") ?? false)
            {
                Task.Run(async () =>
                {
                    var result = await GraphicalMirai.MainWindow.Msg.ShowAsync(() =>
                    {
                        var error = $"{ex.GetType().Name}: {ex.Message}\n" +
                            $"  Source:\n{ex.Source}\n" +
                            $"  Stacktrace:\n{ex.StackTrace}\n";

                        while(ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                            error += $"  InnerException: {ex.GetType().Name}: {ex.Message}\n" +
                            $"  Source:\n{ex.Source}\n" +
                            $"  Stacktrace:\n{ex.StackTrace}\n";
                        }
                        List<Inline> content = new();
                        content.Add(new Run("GraphicalMirai 在运行时出现一个异常!\n请通过 "));
                        content.Add(InnerMessageBox.hyperlink("Github Issues", () => openUrl("https://github.com/MrXiaoM/GraphicalMirai/issues/new/choose")));
                        content.Add(new Run(" 将以下信息反馈给作者\n" +
                            "「是」\t忽略异常继续使用\n" +
                            "「否」\t退出程序\n("));
                        content.Add(InnerMessageBox.hyperlink("点此复制", () => copy(error)));
                        content.Add(new Run($"以下内容。)\n\n{error}"));
                        return content.ToArray();
                    }, "出现错误", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                    {
                        Environment.Exit(-1);
                    }
                });
                e.Handled = true;
            }
        }
#endif
        // 储存一些单例页面实例。
        private static Lazy<PageInit> pageInit = new();
        private static Lazy<PageMain> pageMain = new();
        private static Lazy<PageConsole> pageMainConsole = new();
        private static Lazy<PageLogin> pageMainLogin = new();
        private static Lazy<PagePluginCenter> pagePluginCenter = new();
        private static Lazy<PageOptions> pageOptions = new();

        public static PageInit PageInit => pageInit.Value;
        public static PageMain PageMain => pageMain.Value;
        public static PageConsole PageMainConsole => pageMainConsole.Value;
        public static PageLogin PageMainLogin => pageMainLogin.Value;
        public static PagePluginCenter PagePluginCenter => pagePluginCenter.Value;
        public static PageOptions PageOptions => pageOptions.Value;

        public static long NowTimestamp => ToTimestamp(DateTime.UtcNow);
        public static DateTime FromTimestamp(long time) => new DateTime(1970, 1, 1).Add(TimeSpan.FromMilliseconds(time));

        public static long ToTimestamp(DateTime time) => new DateTimeOffset(time).ToUnixTimeMilliseconds();

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
                if (i >= NUMBER_UNITS.Length) break;
                number /= 1000;
                unit = NUMBER_UNITS[i];
            }
            return string.Format("{0:N" + point + "}", number) + unit;
        }

        public static void openUrl(string s)
        {
            if (s.StartsWith("https://") || s.StartsWith("http://") || s.Substring(1).StartsWith(":\\"))
                System.Diagnostics.Process.Start("explorer", "/e," + s);
        }

        public static bool exists(string path) => exists(new string[] { path });

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
            if (!Directory.Exists(App.path(path)))
            {
                Directory.CreateDirectory(App.path(path));
            }
        }

        public static string path(string path) => Environment.CurrentDirectory + "\\" + path.Replace("/", "\\");

        public static void resetDir(string path)
        {
            path = App.path(path);
            if (Directory.Exists(path)) Directory.Delete(path, true);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void copy(string data) => Clipboard.SetText(data);

        public static string pasteText() => Clipboard.GetText();

        public static SolidColorBrush hexBrush(string s) => new(hex(s));

        public static Color hex(string s) => (Color)ColorConverter.ConvertFromString(s);

        public static string MD5(string s)
        {
            string result = "";
            foreach (byte b in System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(s))) result += b.ToString("x2");
            return result;
        }

        public static string MD5(Stream s) => ASCII.GetString(System.Security.Cryptography.MD5.Create().ComputeHash(s));

        public static string MD5(byte[] b) => ASCII.GetString(System.Security.Cryptography.MD5.Create().ComputeHash(b));

        public static string FormatXaml(string xaml)
        {
            XmlWriterSettings settings = new()
            {
                Indent = true,
                IndentChars = "    ",
                NewLineOnAttributes = true
            };
            StringBuilder builder = new();
            XmlWriter xmlWriter = XmlWriter.Create(builder, settings);
            XamlWriter.Save(XamlReader.Parse(xaml), xmlWriter);
            return builder.ToString();
        }

        public static bool ExtractResource(string resource, string filePath)
        {
            Assembly? assembly = Assembly.GetEntryAssembly();
            var stream = assembly?.GetManifestResourceStream($"GraphicalMirai.{resource}");
            if (stream == null) return false;
            if (File.Exists(filePath))
            {
                var md5stream = MD5(stream);
                var md5file = MD5(File.ReadAllBytes(filePath));
                if (md5stream == md5file) return true;
            }
            else
            {
                var parent = new FileInfo(filePath).Directory;
                if ((!parent?.Exists) ?? false) parent?.Create();
            }
            try
            {
                using (var input = new BufferedStream(stream))
                {
                    using (var output = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buf = new byte[1024];
                        int len;
                        while ((len = input.Read(buf, 0, buf.Length)) > 0) output.Write(buf, 0, len);
                        output.Flush();
                    }
                }
            }
            catch
            {
                // 收声
                return false;
            }
            return true;
        }

        public static bool CheckBridge() => ExtractResource("bridge.jar", path("mirai/plugins/GraphicalMiraiBridge.mirai2.jar"));
    }

    public static class HttpClientExt
    {
        public static async Task<bool> GetByteArrayAsync(this HttpClient httpClient, string url, Action<byte[]> success, Action<HttpRequestException> fail)
        {
            try
            {
                byte[] bytes = await httpClient.GetByteArrayAsync(url);
                success(bytes);
                return true;
            }
            catch (HttpRequestException e)
            {
                fail(e);
                return false;
            }
        }
        public static async Task<bool> GetStringAsync(this HttpClient httpClient, string url, Action<string> success, Action<HttpRequestException> fail)
        {
            try
            {
                string s = await httpClient.GetStringAsync(url);
                success(s);
                return true;
            }
            catch (HttpRequestException e)
            {
                fail(e);
                return false;
            }
        }
    }
    public static class EnumExt
    {
        /// <summary>
        /// 获取枚举所有的值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns></returns>
        public static T[] values<T>() where T : Enum => typeof(T).GetEnumValues().OfType<T>().ToArray();

        /// <summary>
        /// 获取枚举所有的值以及名称
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns></returns>
        public static Dictionary<string, T> valuesWithNames<T>() where T : Enum
        {
            Type type = typeof(T);
            return type.GetEnumValues().OfType<T>().ToDictionary(a => type.GetEnumName(a) ?? a.ToString());
        }
    }
}

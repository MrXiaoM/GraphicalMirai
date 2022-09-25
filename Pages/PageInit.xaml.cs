using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

namespace GraphicalMirai
{
    /// <summary>
    /// PageInit.xaml 的交互逻辑
    /// </summary>
    public partial class PageInit : Page
    {
        public PageInit()
        {
            InitializeComponent();
            Load();
        }

        public void Load()
        {
            App.mkdir("mirai/plugins");
            Config config = Config.Instance;
            PackagesData packages = PackagesData.Instance;
            foreach ((string url, string name) in packages.Repositories)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = name;
                item.Tag = url;
                ComboRepo.Items.Add(item);
            }
            if (ComboRepo.Items.Count > 0)
            {
                ComboRepo.SelectedIndex = 0;
            }
            UpdateInfo();
            Config.Save();

            if (Config.Instance.webp_codec_check && !CheckWebpCodec())
            {
                MainWindow.Msg?.ShowAsync(
                    @"未找到 webp 解码器，插件中心的用户头像将无法显示！
是否需要安装 Google Webp Codec?
「是」	下载并安装
「否」	不安装
「取消」	不再提醒", "无法解码 Webp 图片", MessageBoxButton.YesNoCancel).ContinueWith(t =>
                    {
                        if (t.Result == MessageBoxResult.Yes)
                        {
                            DownloadWebpCodec();
                        }
                        else if (t.Result == MessageBoxResult.Cancel)
                        {
                            Config.Instance.webp_codec_check = false;
                            Config.Save();
                        }
                    });
            }
        }

        public bool CheckWebpCodec()
        {
            try
            {
                BitmapDecoder.Create(new Uri("pack://application:,,,/sample.webp", UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.Default);
                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        private void DownloadWebpCodec()
        {
            Dispatcher.Invoke(() =>
            {
                BtnUpdate.IsEnabled = BtnInstall.IsEnabled = ComboMiraiVer.IsEnabled = ComboRepo.IsEnabled = false;
            });
            MainWindow.Instance.download.StartDownload(async (httpClient, s) =>
            {
                // 正式下载
                s.Invoke("WebpCodecSetup.exe");
                byte[] codec = await httpClient.GetByteArrayAsync("https://storage.googleapis.com/downloads.webmproject.org/releases/webp/WebpCodecSetup.exe");
                string exepath = App.path("WebpCodecSetup.exe");
                File.WriteAllBytes(exepath, codec);
                Process proc = new Process();
                proc.StartInfo.FileName = exepath;
                proc.Start();
            },
            // 回调进度
            (e, s) => { },
            // 下载完成
            () => Dispatcher.Invoke(() =>
                {
                    //downloadProcess.Width = 0;
                    //downloadInfo.Text = "";
                    BtnUpdate.IsEnabled = BtnInstall.IsEnabled = ComboMiraiVer.IsEnabled = ComboRepo.IsEnabled = true;
                    MainWindow.Msg?.ShowAsync("已为你打开安装包，请手动安装 Google Webp Codec\n安装后重启 GraphicalMirai 生效", "下载完成");
                })
            );
        }

        private byte hex(string s)
        {
            return Convert.ToByte(s, 16);
        }

        private string hex(byte b)
        {
            return Convert.ToString(b, 16).PadLeft(2, '0');
        }

        private void UpdateInfo()
        {
            PackagesData packages = PackagesData.Instance;
            BtnStart.IsEnabled = packages.selectedMiraiVersion != null && packages.SelectedMiraiVersion != null;
            string miraiVer = BtnStart.IsEnabled ? (packages.selectedMiraiVersion ?? "") : "未安装";
            int plugins = new DirectoryInfo(App.path("mirai/plugins")).GetFiles("*.jar").Length;

            TextInfo.Text = "已安装插件数量: $plugins\n已选择 mirai 版本: $mirai".Replace("$mirai", miraiVer).Replace("$plugins", plugins.ToString());
            ;
        }

        private string? GetRepoUrl()
        {
            object sel = ComboRepo.SelectedItem;
            if (sel == null || (sel is not ComboBoxItem)) return null;
            ComboBoxItem item = (ComboBoxItem)sel;
            string repo = (string)item.Tag;
            if (!repo.EndsWith("/")) repo += "/";
            return repo;
        }

        private void UpdateVersionList()
        {
            string? repo = GetRepoUrl();
            if (repo == null)
            {
                MessageBox.Show("请选择一个下载源");
                return;
            }
            BtnUpdate.IsEnabled = false;
            BtnInstall.IsEnabled = false;
            ComboMiraiVer.IsEnabled = false;
            ComboRepo.IsEnabled = false;
            ComboMiraiVer.Items.Clear();
            // 异步
            new Task(async () =>
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(repo);
                string xml = await httpClient.GetStringAsync("net/mamoe/mirai-bom/maven-metadata.xml");
                XmlDocument objDoc = new XmlDocument();
                objDoc.LoadXml(xml);
                XmlNodeList? versions = objDoc?["metadata"]?["versioning"]?["versions"]?.ChildNodes;
                // 同步
                Dispatcher.Invoke(() =>
                {
                    if (versions != null)
                    {
                        // 反转列表，使最新版在最上面
                        string[] nodes = versions.OfType<XmlNode>().Reverse().Select(node => node.InnerText).ToArray();
                        foreach (string ver in nodes)
                        {
                            ComboMiraiVer.Items.Add(ver);
                        }
                    }
                    if (ComboMiraiVer.Items.Count > 0) ComboMiraiVer.SelectedIndex = 0;
                    BtnUpdate.IsEnabled = true;
                    BtnInstall.IsEnabled = true;
                    ComboMiraiVer.IsEnabled = true;
                    ComboRepo.IsEnabled = true;
                });
            }).Start();
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(App.PageOptions);
        }

        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            string? repo = GetRepoUrl();
            if (repo == null)
            {
                MessageBox.Show("请选择一个下载源");
                return;
            }
            var sel = ComboMiraiVer.SelectedItem;
            if (sel == null)
            {
                MessageBox.Show("请选择一个版本");
                return;
            }
            if (await MainWindow.Msg.ShowAsync("安装 mirai 之前会清空 ./mirai/content 文件夹，确定要安装吗?", "警告", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            App.resetDir("mirai/content");
            BtnUpdate.IsEnabled = false;
            BtnInstall.IsEnabled = false;
            BtnInstall.Content = "正在下载并安装...";
            ComboMiraiVer.IsEnabled = false;
            ComboRepo.IsEnabled = false;

            MainWindow.Instance.download.StartDownload(async (httpClient, s) =>
            {
                httpClient.BaseAddress = new Uri(repo);
                #region 下载文件
                // 获取 bcprov-jdk15on 的版本
                s.Invoke("[bcprov-jdk15on] maven-metadata.xml");
                string xml = await httpClient.GetStringAsync("org/bouncycastle/bcprov-jdk15on/maven-metadata.xml");
                XmlDocument objDoc = new XmlDocument();
                objDoc.LoadXml(xml);
                string bcprovVer = objDoc["metadata"]?["versioning"]?["release"]?.InnerText ?? "1.70";

                // 正式下载
                s.Invoke("bcprov-jdk15on-" + sel + ".jar (1/4)");
                byte[] bcprov = await httpClient.GetByteArrayAsync("org/bouncycastle/bcprov-jdk15on/" + bcprovVer + "/bcprov-jdk15on-" + bcprovVer + ".jar");
                File.WriteAllBytes(App.path("mirai/content/bcprov-jdk15on-" + bcprovVer + ".jar"), bcprov);

                s.Invoke("mirai-core-all-" + sel + "-all.jar (2/4)");
                byte[] core = await httpClient.GetByteArrayAsync("net/mamoe/mirai-core-all/" + sel + "/mirai-core-all-" + sel + "-all.jar");
                File.WriteAllBytes(App.path("mirai/content/mirai-core-all-" + sel + "-all.jar"), core);

                s.Invoke("mirai-console-" + sel + "-all.jar (3/4)");
                byte[] console = await httpClient.GetByteArrayAsync("net/mamoe/mirai-console/" + sel + "/mirai-console-" + sel + "-all.jar");
                File.WriteAllBytes(App.path("mirai/content/mirai-console-" + sel + "-all.jar"), console);

                s.Invoke("mirai-console-terminal-" + sel + "-all.jar (4/4)");
                byte[] terminal = await httpClient.GetByteArrayAsync("net/mamoe/mirai-console-terminal/" + sel + "/mirai-console-terminal-" + sel + "-all.jar");
                File.WriteAllBytes(App.path("mirai/content/mirai-console-terminal-" + sel + "-all.jar"), terminal);
                #endregion
            },
            // 进度回调
            (e, s) => { },
            // 下载完成
            () => Dispatcher.Invoke(() =>
                {
                    PackagesData.Instance.selectedMiraiVersion = (string)sel;
                    PackagesData.Save();
                    UpdateInfo();
                    BtnInstall.Content = "安装";
                    BtnUpdate.IsEnabled = true;
                    BtnInstall.IsEnabled = true;
                    ComboMiraiVer.IsEnabled = true;
                    ComboRepo.IsEnabled = true;
                })
            );
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateVersionList();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = false;
            Config config = Config.Instance;
            PackagesData packages = PackagesData.Instance;
            string? ver = packages.selectedMiraiVersion;
            if (ver == null)
            {
                MessageBox.Show("你没有指定启动版本");
                return;
            }
            if (!App.exists(new string[] {
                "mirai/content/mirai-core-all-" + ver + "-all.jar",
                "mirai/content/mirai-console-" + ver + "-all.jar",
                "mirai/content/mirai-console-terminal-" + ver + "-all.jar"
            }))
            {
                if (MessageBox.Show("必要文件不全，建议重新安装 mirai，若坚持启动，请点击「是」", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            }
            DirectoryInfo dir = new DirectoryInfo(App.path("mirai/content"));
            List<string> libraries = new List<string>();
            foreach (FileInfo file in dir.GetFiles("*.jar"))
            {
                libraries.Add(file.FullName);
            }
            string javaPath = config.javaPath;
            string extArgs = config.extArgs;
            string mainClass = config.mainClass;

            App.mirai.InitMirai(javaPath, App.path("mirai"), libraries, mainClass, extArgs);
            MainWindow.Navigate(App.PageMain);
            App.mirai.Start();
        }

        private void BtnPluginsCenter_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(App.PagePluginCenter);
        }
    }
}

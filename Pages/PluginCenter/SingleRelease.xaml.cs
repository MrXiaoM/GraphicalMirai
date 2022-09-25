using Pages.PluginCenter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using static Pages.PluginCenter.IDevPlatformApi.Release;

namespace GraphicalMirai.Pages.PluginCenter
{
    /// <summary>
    /// SinglePlugin.xaml 的交互逻辑
    /// </summary>
    public partial class SingleRelease : Grid
    {
        Asset selectedAsset;
        IDevPlatformApi.Release release;
        public SingleRelease(IDevPlatformApi.Release release)
        {
            InitializeComponent();
            this.release = release;
            TitleTag.Text = release.TagName;
            TitleText.Text = release.Name;

            ReleaseInfo1.Inlines.Clear();
            Hyperlink authorLink = new(new Run(release.Author.Name));
            authorLink.Click += delegate { App.openUrl(release.Author.Url); };
            ReleaseInfo1.Inlines.Add(authorLink);
            ReleaseInfo1.Inlines.Add(" 创建于");
            ReleaseInfo2.Text = App.FromTimestamp(release.CreatedTime).ToString("yyyy年MM月dd日 HH:mm:ss");

            if (release.Author.AvatarUrl != null)
            {
                AuthorHeadimg.Source = new BitmapImage(new Uri(release.Author.AvatarUrl));
            }

            ComboAssets.Items.Clear();
            foreach (Asset asset in release.Assets)
            {
                ComboBoxItem item = new();
                item.Content = asset.Name;
                item.Tag = asset;
                item.Selected += delegate { selectedAsset = (Asset)item.Tag; };
                ComboAssets.Items.Add(item);
            }
            if (ComboAssets.Items.Count > 0)
            {
                string suffix = PackagesData.Instance.SelectedMiraiVersion >= Version.MIRAI_2 ? ".mirai2.jar" : ".mirai.jar";
                // 按照当前 mirai 版本选择文件，或选择第一个文件
                ComboAssets.SelectedItem = ComboAssets.Items.OfType<ComboBoxItem>()
                    .FirstOrDefault(i => ((string)((ComboBoxItem)i).Content).EndsWith(suffix), ComboAssets.Items[0]);
            }
            string html = Markdig.Markdown.ToHtml(release.Body);
            string xaml = HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(html, true);
            ReleaseBody.Document = (FlowDocument)XamlReader.Parse(xaml);
            ReleaseBody.Document.LineHeight = 1;
            ReleaseBody.Document.TextAlignment = TextAlignment.Left;
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            // TODO 添加到下载任务
            string fileName = selectedAsset.Name;
            string fileUrl = selectedAsset.DownloadUrl;
            int i = selectedAsset.Name.LastIndexOf('-');
            string prefix = i > 0 ? selectedAsset.Name.Substring(0, i) : selectedAsset.Name;
            var existPlugins = new DirectoryInfo(App.path("mirai/plugins")).GetFiles().TakeWhile(f => f.Name.ToLower().StartsWith(prefix.ToLower()));
            if (existPlugins.Count() > 0)
            {
                var content = new List<Inline>();
                content.Add(new Run("GraphicalMirai 在你的 "));
                var linkPlugins = new Hyperlink(new Run("plugins 文件夹") { Foreground = App.hexBrush("#FFF000") });
                linkPlugins.Click += delegate { App.openUrl(App.path("mirai/plugins")); };
                content.Add(linkPlugins);
                content.Add(new Run(" 下发现\n" +
                    "存在以下疑似同名插件。\n"));

                foreach (FileInfo f in existPlugins)
                {
                    content.Add(new LineBreak());
                    content.Add(new Run(f.Name));
                }
                content.Add(new LineBreak());
                content.Add(new LineBreak());
                content.Add(new Run(
                    "「是」\t删除这些插件并开始下载\n" +
                    "「否」\t不删除这些插件并开始下载\n" +
                    "「取消」\t取消下载\n\n你也可以 "));
                var link = new Hyperlink(new Run("点击这里") { Foreground = App.hexBrush("#FFF000") });
                link.Click += delegate { App.copy(fileUrl); };
                content.Add(link);
                content.Add(new Run(" 复制下载链接"));

                var result = await MainWindow.Msg.ShowAsync(content.ToArray(), "存在其他版本插件", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    await Task.Run(() => { foreach (FileInfo f in existPlugins) f.Delete(); });
                }
                else if (result != MessageBoxResult.No) return;
            }
            MainWindow.Instance.download.StartDownload(async (httpClient, s) =>
            {
                // 正式下载
                s.Invoke(fileName);
                byte[] bytes = await httpClient.GetByteArrayAsync(fileUrl);
                File.WriteAllBytes(App.path("mirai/plugins/" + fileName), bytes);
            },
            // 回调进度
            (e, s) => { },
            // 下载完成
            () => Dispatcher.Invoke(() =>
            {
                MainWindow.Msg?.ShowAsync(fileName + " 已下载完成并存放到 mirai/plugins/ 中", "下载完成");
            })
            );
        }
    }
}

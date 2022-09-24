using Pages.PluginCenter;
using System;
using System.Linq;
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

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            // TODO 添加到下载任务
            MessageBox.Show("当前选中文件: " + selectedAsset.Name + "\n" + selectedAsset.DownloadUrl);
        }
    }
}

using Newtonsoft.Json;
using GraphicalMirai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Pages.PluginCenter;
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
            Border tag = new Border()
            {
                CornerRadius = new CornerRadius(4),
                Background = App.hexBrush("#23000000"),
                Padding = new Thickness(5, 2, 5, 2),
            };
            TextBlock tb = new TextBlock()
            {
                FontWeight = FontWeights.Bold,
                Foreground = App.hexBrush("#999999"),
                Text = release.TagName
            };
            tag.Child = tb;
            ReleaseTitle.Inlines.Clear();
            ReleaseTitle.Inlines.Add(tag);
            ReleaseTitle.Inlines.Add(release.Name);

            ReleaseInfo.Inlines.Clear();
            Hyperlink authorLink = new(new Run(release.Author.Name));
            authorLink.Click += delegate { App.openUrl(release.Author.Url); };
            ReleaseInfo.Inlines.Add(authorLink);
            ReleaseInfo.Inlines.Add(" 创建于 " + App.FromTimestamp(release.CreatedTime).ToString("yyyy年MM月dd日 HH:mm:ss"));

            if (release.Author.AvatarUrl != null) {
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
                string suffix = Config.Instance.SelectedMiraiVersion >= Version.MIRAI_2 ? ".mirai2.jar" : ".mirai.jar";
                // 按照当前 mirai 版本选择文件，或选择第一个文件
                ComboAssets.SelectedItem = ComboAssets.Items.OfType<ComboBoxItem>()
                    .FirstOrDefault(i => ((string)((ComboBoxItem)i).Content).EndsWith(suffix), ComboAssets.Items[0]);
            }

            Markdown.Xaml.TextToFlowDocumentConverter md = new() { Markdown = new Markdown.Xaml.Markdown() };
            ReleaseBody.Document = new Markdown.Xaml.Markdown().Transform(release.Body);
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            // TODO 添加到下载任务
            MessageBox.Show("当前选中文件: " + selectedAsset.Name + "\n" + selectedAsset.DownloadUrl);
        }
    }
}

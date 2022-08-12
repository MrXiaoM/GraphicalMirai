using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace GraphicalMirai.Pages.PluginCenter
{
    /// <summary>
    /// PagePlugin.xaml 的交互逻辑
    /// </summary>
    public partial class PagePlugin : Page
    {

        private static readonly Regex regexLink = new Regex("(<a .*?)(target=\".*?\")(.*?>)", RegexOptions.IgnoreCase);
        private static readonly Regex regexImage = new Regex("(<img .*?)(src=\"/)(.*?\")(.*?>)", RegexOptions.IgnoreCase);
        Topic? topic;
        public PagePlugin(int tid)
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(Load, tid);
        }

        private async void Load(int tid)
        {
            string json = await App.PagePluginCenter.forumClient.GetStringAsync("topic/" + tid);
            Topic? topic = JsonConvert.DeserializeObject<Topic>(json);
            if (topic == null)
            {
                MessageBox.Show("json 解析错误");
                return;
            }
            this.topic = topic;
            bool isDeleted = topic.deleted == 1;
            string time = App.FormatTimestamp(topic.timestamp);
            string content = topic.ToString();
            // 技术问题，需要移除所有 target="*" 避免打开新窗口
            // 并将链接重定向到系统浏览器
            content = regexLink.Replace(content, new MatchEvaluator((m) => {
                return string.Join("", m.Groups
                             .OfType<Group>()
                             .Select((g, i) => i == 2 ? "" : g.Value)
                             .Skip(1)
                             .ToArray());
            }));
            // 将相对于网站根目录的图片路径替换成链接
            content = regexImage.Replace(content, new MatchEvaluator((m) => {
                return string.Join("", m.Groups
                             .OfType<Group>()
                             .Select((g, i) => i == 2 ? "src=\"https://mirai.mamoe.net/" : g.Value)
                             .Skip(1)
                             .ToArray());
            }));

            CUser? author = topic.posts.Count > 0 ? topic.posts[0].user : null;
            Dispatcher.Invoke(() =>
            {
                if (!isDeleted && topic.tags != null)
                {
                    foreach (var tag in topic.tags)
                    {
                        Border border = new Border()
                        {
                            CornerRadius = new CornerRadius(4),
                            Background = App.hexBrush("#23000000"),
                            Padding = new Thickness(5, 2, 5, 2),
                        };
                        TextBlock tb = new TextBlock()
                        {
                            FontWeight = FontWeights.Bold,
                            Text = tag.value
                        };
                        border.Child = tb;
                        TextSubTitle.Inlines.Add(border);
                        TextSubTitle.Inlines.Add(new Rectangle() { Width = 5 });
                    }
                    TextSubTitle.Inlines.Add(new LineBreak());
                }
                Border border1 = new Border();
                TextBlock tb1 = new TextBlock()
                {
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "发布于 " + time
                };
                border1.Child = tb1;
                TextSubTitle.Inlines.Add(border1);
                
                TextTitle.Text = topic.titleRaw;
                temp.Text = content;

                if (author != null)
                {
                    AuthorName.Inlines.Clear();
                    AuthorName.Inlines.Add(new Run(author.displayname) { FontWeight = FontWeights.Bold });
                    AuthorName.Inlines.Add(new LineBreak());
                    AuthorName.Inlines.Add(new Run("主题数: " + author.topiccount) { FontSize = 12, Foreground = App.hexBrush("#DDD") });
                    AuthorName.Inlines.Add(new LineBreak());
                    AuthorName.Inlines.Add(new Run("声望: " + author.reputation) { FontSize = 12, Foreground = App.hexBrush("#DDD") });
                    AuthorHeadimgSimple.Text = author.displayname.Length > 0 ? author.displayname.Substring(0, 1).ToUpper() : "";
                    string? picture = author.picture;
                    if (picture != null)
                    {
                        if (!picture.StartsWith("http://") && !picture.StartsWith("https://"))
                        {
                            if (!picture.StartsWith("/")) picture = "/" + picture;
                            picture = "https://mirai.mamoe.net" + picture;
                        }
                        AuthorHeadimg.Source = new BitmapImage(new Uri(picture));
                    }
                }

                temp.Text += "\n\n额外调试信息:\n  Github/Gitee 链接列表:\n    " + string.Join("\n    ", topic.posts[0].repo().Select(r => r.ToString()).ToArray());
            });
            
            await webInfo.EnsureCoreWebView2Async();
            webInfo.NavigateToString(content);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(App.PagePluginCenter);
        }

        private void webInfo_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("https://") || e.Uri.StartsWith("http://")) {
                App.openUrl(e.Uri);
                e.Cancel = true;
            }
        }
    }
}

using Newtonsoft.Json;
using Pages.PluginCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
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
            AuthorTag.Visibility = Visibility.Hidden;
            
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
            // 将相对于网站根目录的图片路径替换成链接
            content = regexImage.Replace(content, new MatchEvaluator((m) =>
            {
                return string.Join("", m.Groups
                             .OfType<Group>()
                             .Select((g, i) => i == 2 ? "src=\"https://mirai.mamoe.net/" : g.Value)
                             .Skip(1)
                             .ToArray());
            }));

            string xaml = HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(content, true);
            
            FlowDocument doc = (FlowDocument)XamlReader.Parse(xaml);
            doc.PagePadding = new Thickness(10);
            doc.FontFamily = (System.Windows.Media.FontFamily) Application.Current.Resources["SourceHanSans"];
            flowInfo.Document = doc;

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
                        TextTag.Inlines.Add(border);
                        TextTag.Inlines.Add(new Rectangle() { Width = 5 });
                    }
                }

                TextTime.Text = "发布于 " + time;
                TextTitle.Text = topic.titleRaw;
                temp.Text = App.FormatXaml(xaml);

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
                    UGroup? group = author.selectedGroups.FirstOrDefault();
                    if (group != null)
                    {
                        AuthorTag.Background = App.hexBrush(group.labelColor);
                        AuthorTagText.Foreground = App.hexBrush(group.textColor);
                        AuthorTagText.Text = group.userTitle;
                        AuthorTag.Visibility = Visibility.Visible;
                    }
                }
               
                temp.Text += "\n\n额外调试信息:\n  Github/Gitee 链接列表:\n    " + string.Join("\n    ", topic.posts[0].repo().Select(r => r.ToString()).ToArray());
            });
            await refreshDownloadList();
        }

        public async Task refreshDownloadList()
        {
            if (topic == null) return;
            void err(string msg, bool center = true)
            {
                StackReleases.Children.Clear();
                StackReleases.Children.Add(new TextBlock()
                {
                    Text = msg,
                    TextAlignment = center ? TextAlignment.Center : TextAlignment.Left,
                    Padding = new Thickness(center ? 0 : 20, 50, center ? 0 : 20, 10),
                    TextWrapping = TextWrapping.Wrap
                });
                Button retry = new Button()
                {
                    Content = "重试",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 100
                };
                retry.Click += async delegate { await refreshDownloadList(); };
                StackReleases.Children.Add(retry);
            }
            List<Repo> repos = topic.posts[0].repo();
            repos.RemoveAll(repo => repo.username == "mamoe" && repo.repo == "mirai");
            if (repos.Count <= 0)
            {
                err("无法找到合适的开源仓库");
                return;
            }
            Repo repo = repos[0];
            if (!IDevPlatformApi.apis.ContainsKey(repo.platform.ToLower()))
            {
                err("无法找到合适的开源平台解析接口");
                return;
            }
            IDevPlatformApi api = IDevPlatformApi.apis[repo.platform.ToLower()];
            (List<IDevPlatformApi.Release>, Exception?) response = await api.GetReleasesAsync(repo.username, repo.repo);
            if (response.Item2 != null)
            {
                Exception e = response.Item2;
                err("请求接口时出现异常: " + e.GetType().FullName + "\n" +
                    "消息: " + e.Message + "\n" +
                    "源: " + e.Source + "\n" +
                    "堆栈跟踪: \n" + e.StackTrace, false);
                return;
            }
            StackReleases.Children.Clear();
            List<IDevPlatformApi.Release> releases = response.Item1;
            foreach (IDevPlatformApi.Release r in releases)
            {
                StackReleases.Children.Add(new SingleRelease(r));
                StackReleases.Children.Add(new Rectangle() { Height = 5 });
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(App.PagePluginCenter);
        }
    }
}

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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YamlDotNet.Core.Tokens;

namespace GraphicalMirai.Pages.PluginCenter
{
    /// <summary>
    /// SingleComment.xaml 的交互逻辑
    /// </summary>
    public partial class SingleComment : Grid
    {
        private static readonly Regex regexImage = new Regex("(<img .*?)(src=\"/)(.*?\")(.*?>)", RegexOptions.IgnoreCase);

        Post post;
        public SingleComment(Post post, FontFamily font)
        {
            this.post = post;
            InitializeComponent();
            AuthorTag.Width = ReplyBox.Width = 0;
            AuthorName.Text = post.user.displayname;
            TextTime.Text = App.FormatTimestamp(post.timestamp);
            string content = post.content;

            bool isDeleted = post.deleted != 0;
            if (isDeleted)
            {
                content = "此回复已被删除！";
                TextVote.Text = "*";
                CommentBody.Opacity = 0.5;
                IsEnabled = false;
            }
            else
            {
                TextVote.Text = post.votes.ToString();
                // 将相对于网站根目录的图片路径替换成链接
                content = regexImage.Replace(content, new MatchEvaluator((m) =>
                {
                    return string.Join("", m.Groups
                                 .OfType<Group>()
                                 .Select((g, i) => i == 2 ? "src=\"https://mirai.mamoe.net/" : g.Value)
                                 .Skip(1)
                                 .ToArray());
                }));
                AuthorHeadimgSimple.Text = post.user.displayname.First().ToString().ToUpper();
                string ? picture = post.user.picture;
                if (picture != null)
                {
                    if (!picture.StartsWith("http://") && !picture.StartsWith("https://"))
                    {
                        if (!picture.StartsWith("/")) picture = "/" + picture;
                        picture = "https://mirai.mamoe.net" + picture;
                    }
                    AuthorHeadimg.Source = new BitmapImage(new Uri(picture));
                }
                UGroup? group = post.user.selectedGroups.FirstOrDefault();
                if (group != null)
                {
                    AuthorTag.Background = App.hexBrush(group.labelColor);
                    AuthorTagText.Foreground = App.hexBrush(group.textColor);
                    AuthorTagText.Text = group.userTitle;
                    AuthorTag.Visibility = Visibility.Visible;
                    AuthorTag.Width = double.NaN;
                }
                // TODO: 回复显示
                // ReplyBox.Width = double.NaN;
            }
            string xaml = HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(content, true);

            FlowDocument doc = (FlowDocument)XamlReader.Parse(xaml);
            doc.PagePadding = new Thickness(10);
            doc.FontFamily = font;
            CommentBody.Document = doc;
        }

        private void PackIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.openUrl($"https://mirai.mamoe.net/post/{post.pid}");
        }

    }
}

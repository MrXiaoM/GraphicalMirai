using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YamlDotNet.Core.Tokens;

namespace GraphicalMirai.Pages.PluginCenter
{
    /// <summary>
    /// SinglePlugin.xaml 的交互逻辑
    /// </summary>
    public partial class SinglePlugin : Grid
    {
        public SinglePlugin(CTopic topic)
        {
            InitializeComponent();

            List<string> tags = new List<string>();
            foreach (CTag tag in topic.tags)
            {
                tags.Add(tag.value);
            }
            bool isDeleted = topic.deleted != 0;
            string author = topic.user.displayname;
            string? picture = topic.user.picture;
            if (isDeleted)
            {
                this.Cursor = Cursors.No;
                this.Opacity = 0.5;
                HoverGrid.Style = null;
            }
            else
            {
                this.Cursor = Cursors.Hand;
                this.MouseDown += delegate
                {
                    MainWindow.Navigate(new PagePlugin(topic.tid));
                };
            }
            TopicTitle.Text = isDeleted ? "此主题已被删除!" : topic.titleRaw;
            TopicTitle.ToolTip = isDeleted ? "此主题已被删除!" : topic.titleRaw;

            AuthorHeadimgSimple.Text = author.Length > 0 ? author.Substring(0, 1).ToUpper() : "";
            TextLike.Text = App.FormatNumber(topic.votes);
            TextView.Text = App.FormatNumber(topic.viewcount);

            string time = App.FormatTimestamp(topic.timestamp);
            if (topic.pinned == 1)
            {
                Border border = new Border()
                {
                    CornerRadius = new CornerRadius(4),
                    Background = App.hexBrush("#A9672F"),
                    Padding = new Thickness(5, 2, 5, 2),
                };
                TextBlock tb = new TextBlock()
                {
                    FontWeight = FontWeights.Bold,
                    Foreground = App.hexBrush("#FFFFFF"),
                    Text = "置顶"
                };
                border.Child = tb;
                TopicSubtitle.Inlines.Add(border);
                TopicSubtitle.Inlines.Add(new Rectangle() { Width = 5 });
            }
            if (!isDeleted && tags != null)
            {
                foreach (string tag in tags)
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
                        Foreground = App.hexBrush("#999999"),
                        Text = tag
                    };
                    border.Child = tb;
                    TopicSubtitle.Inlines.Add(border);
                    TopicSubtitle.Inlines.Add(new Rectangle() { Width = 5 });
                }
            }
            Border border1 = new Border() { Padding = new Thickness(5, 2, 5, 2) };
            TextBlock tb1 = new TextBlock()
            {
                FontWeight = FontWeights.Bold,
                Foreground = App.hexBrush("#999999"),
                ToolTip = time + " 由 " + author + " 发布",
                Text = time + " 由 " + author + " 发布"
            };
            border1.Child = tb1;
            TopicSubtitle.Inlines.Add(border1);
            if (picture != null)
            {
                if (!picture.StartsWith("http://") && !picture.StartsWith("https://"))
                {
                    if (!picture.StartsWith("/")) picture = "/" + picture;
                    picture = "https://mirai.mamoe.net" + picture;
                }
                AuthorHeadimg.Source = new BitmapImage(new Uri(picture));
            };
        }
    }
}

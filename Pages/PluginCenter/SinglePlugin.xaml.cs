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

namespace GraphicalMirai.Pages.PluginCenter
{
    /// <summary>
    /// SinglePlugin.xaml 的交互逻辑
    /// </summary>
    public partial class SinglePlugin : Grid
    {
        public SinglePlugin(bool IsDeleted, int tid, string Title = "", string Author = "", int Votes = 0, int ViewCount = 0, List<string>? TopicTags = null, long CreateTime = 0, string? picture = null)
        {
            InitializeComponent();
            if (IsDeleted)
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
                    MainWindow.Navigate(new PagePlugin(tid));
                };
            }
            TopicTitle.Text = IsDeleted ? "此主题已被删除!" : Title;
            TopicTitle.ToolTip = IsDeleted ? "此主题已被删除!" : Title;
            
            AuthorHeadimgSimple.Text = Author.Length > 0 ? Author.Substring(0, 1).ToUpper() : "";
            TextLike.Text = App.FormatNumber(Votes);
            TextView.Text = App.FormatNumber(ViewCount);

            string time = App.FormatTimestamp(CreateTime);
            
            if (!IsDeleted && TopicTags != null)
            {
                foreach (string tag in TopicTags)
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
                ToolTip = time + " 由 " + Author + " 发布",
                Text = time + " 由 " + Author + " 发布"
            };
            border1.Child = tb1;
            TopicSubtitle.Inlines.Add(border1);
            if (picture != null) { 
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

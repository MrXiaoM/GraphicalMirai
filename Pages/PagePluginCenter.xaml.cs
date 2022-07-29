using GraphicalMirai.Pages.PluginCenter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace GraphicalMirai.Pages
{
    /// <summary>
    /// PagePluginCenter.xaml 的交互逻辑
    /// </summary>
    public partial class PagePluginCenter : Page
    {
        private static readonly string MIRAI_REPO_PACKAGES = "https://mirai.mamoe.net/assets/mcl/packages.json";
        private static readonly string MIRAI_FORUM_API = "https://mirai.mamoe.net/api/";
        HttpClient repoClient = new HttpClient();
        HttpClient forumClient = new HttpClient() { BaseAddress = new Uri(MIRAI_FORUM_API) };
        Task? refreshTask;
        int forumPage = 1;
        public PagePluginCenter()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(App.mirai == null ? App.PageInit : App.PageMain);
        }
        private void OpenMiraiForum(object sender, RoutedEventArgs e)
        {
            App.openUrl("https://mirai.mamoe.net/category/11");
        }

        private void ForumRefresh_Click(object sender, RoutedEventArgs e)
        {
            ForumRefresh.IsEnabled = false;
            ForumPage.Text = "...正在刷新";
            if (refreshTask == null)
            {
                refreshTask = new Task(Refresh_Forum);
                refreshTask.Start();
            }
        }

        private async void Refresh_Forum()
        {
            Dispatcher.Invoke(() => StackPluginList.Children.Clear());
            string json = await forumClient.GetStringAsync("category/11?page=" + forumPage);
            Category? category = JsonConvert.DeserializeObject<Category>(json);
            if (category == null)
            {
                MessageBox.Show("json 解析错误");
                return;
            }
            Pagination page = category.pagination;
            Dispatcher.Invoke(() =>
            {
                ForumPrevPage.IsEnabled = page.currentPage > 1;
                ForumNextPage.IsEnabled = page.currentPage < page.pageCount;
                ForumPage.Text = "第 " + page.currentPage + "/" + page.pageCount + " 页";
            });
            foreach (Topic topic in category.topics)
            {
                List<string> tags = new List<string>();
                foreach(Tag tag in topic.tags)
                {
                    tags.Add(tag.value);
                }
                Dispatcher.Invoke(() =>
                {
                    StackPluginList.Children.Add(new SinglePlugin(
                        topic.deleted == 1,
                        topic.tid,
                        topic.titleRaw,
                        topic.user.displayname,
                        topic.votes.ToString(),
                        topic.viewcount.ToString(),
                        tags,
                        topic.timestamp,
                        topic.user.picture
                    ));
                    StackPluginList.Children.Add(new Rectangle() { Height = 10 });
                });
            }
            refreshTask = null;
            Dispatcher.Invoke(() => ForumRefresh.IsEnabled = true);
        }

        private void ForumPrevPage_Click(object sender, RoutedEventArgs e)
        {
            forumPage--;
            ForumRefresh_Click(sender, e);
        }

        private void ForumNextPage_Click(object sender, RoutedEventArgs e)
        {
            forumPage++;
            ForumRefresh_Click(sender, e);
        }
    }
}

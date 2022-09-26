using GraphicalMirai.Pages.PluginCenter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public HttpClient forumClient = new HttpClient() { BaseAddress = new Uri(MIRAI_FORUM_API) };
        Task? refreshTask;
        int forumPage = 1;
        string forumSort = "newest_to_oldest";
        public PagePluginCenter()
        {
            repoClient.DefaultRequestHeaders.Add("User-Agent", App.UserAgent);
            forumClient.DefaultRequestHeaders.Add("User-Agent", App.UserAgent);
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navigate(!App.mirai.IsRunning ? App.PageInit : App.PageMain);
        }
        private void OpenMiraiForum(object sender, RoutedEventArgs e)
        {
            App.openUrl("https://mirai.mamoe.net/category/11");
        }

        private void ForumRefresh_Click(object sender, RoutedEventArgs e)
        {
            forumRefresh();
        }
        public void forumRefresh()
        {
            forumElementsEnable(false);
            ForumPage.Text = "...正在刷新";
            if (refreshTask == null)
            {
                refreshTask = new Task(Refresh_Forum);
                refreshTask.Start();
            }
        }


        private async void Refresh_Forum()
        {
            Dispatcher.Invoke(() =>
            {
                StackPluginList.IsEnabled = false;
                StackPluginList.Opacity = 0.5;
            });
            try
            {
                string json = await forumClient.GetStringAsync("category/11?page=" + forumPage + "&sort=" + forumSort);
                Category? category = JsonConvert.DeserializeObject<Category>(json);
                if (category == null)
                {
                    MessageBox.Show("json 解析错误");
                    return;
                }
                CPagination page = category.pagination;
                Dispatcher.Invoke(() =>
                {
                    ForumPrevPage.IsEnabled = page.currentPage > 1;
                    ForumNextPage.IsEnabled = page.currentPage < page.pageCount;
                    ForumPage.Text = "第 " + page.currentPage + "/" + page.pageCount + " 页";
                    ForumComboPages.Items.Clear();
                    for (int i = 1; i <= page.pageCount; i++)
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        int nowPage = i;
                        item.Content = "第 " + nowPage + " 页";
                        item.Selected += delegate
                        {
                            if (!ForumComboPages.IsEnabled) return;
                            forumPage = nowPage;
                            forumRefresh();
                        };
                        ForumComboPages.Items.Add(item);
                    }
                    ForumComboPages.SelectedIndex = page.currentPage - 1;
                    StackPluginList.Children.Clear();
                });
                foreach (CTopic topic in category.topics)
                {
                    List<string> tags = new List<string>();
                    foreach (CTag tag in topic.tags)
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
                            topic.votes,
                            topic.viewcount,
                            tags,
                            topic.timestamp,
                            topic.user.picture
                        ));
                        StackPluginList.Children.Add(new Rectangle() { Height = 10 });
                    });
                }
            }
            catch (Exception e)
            {
                // 极其敷衍
                MessageBox.Show(e.Message, "错误");
            }
            refreshTask = null;
            Dispatcher.Invoke(() =>
            {
                StackPluginList.IsEnabled = ForumComboSort.IsEnabled = ForumComboPages.IsEnabled = ForumRefresh.IsEnabled = true;
                StackPluginList.Opacity = 1.0;
                StackPluginListViewer.ScrollToTop();
            });

        }

        private void ForumPrevPage_Click(object sender, RoutedEventArgs e)
        {
            forumElementsEnable(false);
            forumPage--;
            forumRefresh();
        }

        private void ForumNextPage_Click(object sender, RoutedEventArgs e)
        {
            forumElementsEnable(false);
            forumPage++;
            forumRefresh();
        }

        private void ForumComboSort_ItemSelected(object sender, RoutedEventArgs e)
        {
            if (!ForumComboSort.IsEnabled) return;
            ComboBoxItem item = (ComboBoxItem)sender;
            object tag = item.Tag;
            if (tag != null)
            {
                forumElementsEnable(false);
                forumSort = (string)tag;
                Refresh_Forum();
            }
        }

        private void forumElementsEnable(bool isEnabled)
        {
            if (ForumRefresh != null && ForumComboSort != null && ForumComboPages != null && ForumPrevPage != null && ForumNextPage != null)
                ForumRefresh.IsEnabled = ForumComboSort.IsEnabled = ForumComboPages.IsEnabled = ForumPrevPage.IsEnabled = ForumNextPage.IsEnabled = isEnabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ForumComboSort.IsEnabled = true;
        }
    }
}

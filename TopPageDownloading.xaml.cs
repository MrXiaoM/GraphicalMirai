using GraphicalMirai.Pages.PluginCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Handlers;
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
using System.Xml;
using System.IO;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Shell;

namespace GraphicalMirai
{
    /// <summary>
    /// TopPageDownloading.xaml 的交互逻辑
    /// </summary>
    public partial class TopPageDownloading : UserControl
    {
        Storyboard AniOptFadeIn = new();
        Storyboard AniOptFadeOut = new();
        public TopPageDownloading()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;

            DoubleAnimation aniOptFadeIn = new()
            {
                From = 0,
                To = 1,
                Duration = new(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.6,
                DecelerationRatio = 0.3
            };
            DoubleAnimation aniOptFadeOut = new()
            {
                From = 1,
                To = 0,
                Duration = new(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.6,
                DecelerationRatio = 0.3
            };
            Storyboard.SetTarget(aniOptFadeIn, this);
            Storyboard.SetTargetProperty(aniOptFadeIn, new("Opacity"));
            Storyboard.SetTarget(aniOptFadeOut, this);
            Storyboard.SetTargetProperty(aniOptFadeOut, new("Opacity"));
            aniOptFadeOut.Completed += delegate { Visibility = Visibility.Hidden; };
            AniOptFadeIn.Children.Add(aniOptFadeIn);
            AniOptFadeOut.Children.Add(aniOptFadeOut);
        }

        public void StartDownload(Func<HttpClient, Action<string>, Task> actionDownload, Action<HttpProgressEventArgs, string> actionProogress, Action actionDone)
        {
            Opacity = 0;
            Visibility = Visibility.Visible;
            BeginStoryboard(AniOptFadeIn);
            new Task(async () =>
            {
                HttpClientHandler handler = new HttpClientHandler();
                ProgressMessageHandler processHandler = new ProgressMessageHandler(handler);
                HttpClient httpClient = new HttpClient(processHandler);

                string nowFile = "正在准备";

                // 回调进度
                processHandler.HttpReceiveProgress += (sender, e) => Dispatcher.Invoke(() =>
                {
                    double received = e.BytesTransferred;
                    double? total = e.TotalBytes;
                    string percent = total == null ? "未知进度" : string.Format("{0:N2}%", received / total * 100d);
                    actionProogress.Invoke(e, percent);
                    downloadProcess.Width = downloaGrid.ActualWidth * received / (total ?? received);
                    downloadInfo.Text = "正在下载 " + nowFile + " | " + App.FormatSize(received) + "/" + App.FormatSize(total) + " | " + percent;
                });

                await actionDownload.Invoke(httpClient, (s) => nowFile = s);
                
                Dispatcher.Invoke(() =>
                {
                    BeginStoryboard(AniOptFadeOut);
                    downloadProcess.Width = 0;
                    downloadInfo.Text = "";
                    actionDone.Invoke();
                });
            }).Start();
        }
    }
}

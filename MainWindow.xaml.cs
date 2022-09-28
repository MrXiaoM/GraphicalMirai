using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace GraphicalMirai
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
#pragma warning disable CS8618
        public static MainWindow Instance { get; private set; }
#pragma warning restore CS8618
        public static void Navigate(object content) => Instance.Navigate0(content);
        public static void SetTitle(string title) => Instance.Title = title;
        public static InnerMessageBox Msg
        {
            get { return Instance.msgBox; }
        }
        object pageSwitchTo;
        Storyboard aniSwitchPage0;
        Storyboard aniSwitchPage1;

        public MainWindow()
        {
            InitializeComponent();

            DoubleAnimation aniIn = new()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            DoubleAnimation aniOut = new()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            aniOut.Completed += delegate
            {
                frame.Navigate(pageSwitchTo);
            };
            Storyboard.SetTarget(aniIn, frame);
            Storyboard.SetTargetProperty(aniIn, new("Opacity"));
            Storyboard.SetTarget(aniOut, frame);
            Storyboard.SetTargetProperty(aniOut, new("Opacity"));
            aniSwitchPage0 = new Storyboard();
            aniSwitchPage1 = new Storyboard();
            aniSwitchPage0.Children.Add(aniOut);
            aniSwitchPage1.Children.Add(aniIn);
            Instance = this;
            frame.Navigate(App.PageInit);
        }

        private void Navigate0(object content)
        {
            pageSwitchTo = content;
            BeginStoryboard(aniSwitchPage0);
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            object content = e.Content;
            if (content != null && content is Page)
            {
                Title = ((Page)content).Title;
            }
            BeginStoryboard(aniSwitchPage1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MinWidth = ActualWidth;
            MinHeight = ActualHeight;
            SizeToContent = SizeToContent.Manual;
            this.Activate();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!stopping) stop();
        }
        bool stopping = false;
        private void stop()
        {
            stopping = true;
            App.mirai.Stop();
        }
    }
}

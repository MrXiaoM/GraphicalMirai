using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GraphicalMirai
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow? instance;
        public static MainWindow? Instance => instance;
        public static void Navigate(object content)
        {
            instance?.frame.Navigate(content);
        }
        public static void SetTitle(string title)
        {
            if (instance != null)
            {
                instance.Title = title;
            }
        }
        public static InnerMessageBox? Msg
        {
            get { return instance?.msgBox; }
        }
        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            frame.Navigate(App.PageInit);
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            object content = e.Content;
            if (content != null && content is Page)
            {
                Title = ((Page)content).Title;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MinWidth = MinWidth + MinWidth - grid.ActualWidth;
            MinHeight = MinHeight + MinHeight - grid.ActualHeight;
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!stopping) stop();
        }
        bool stopping = false;
        private async void stop()
        {
            stopping = true;
            await App.mirai.Stop();
        }
    }
}

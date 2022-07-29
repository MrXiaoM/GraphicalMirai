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

namespace GraphicalMirai
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow? instance;
        public static void Navigate(object content)
        {
            instance.frame.Navigate(content);
        }
        public static void SetTitle(string title)
        {
            instance.Title = title;
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
                Title = (content as Page).Title;
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
            if (App.mirai != null)
            {
                if(!stopping) stop();
            }
        }
        bool stopping = false;
        private async void stop()
        {
            stopping = true;
            await App.mirai.Stop();
            App.mirai = null;
        }
    }
}

using System.Windows.Controls;
using System.Windows.Input;

namespace GraphicalMirai.Pages
{
    /// <summary>
    /// PageConsole.xaml 的交互逻辑
    /// </summary>
    public partial class PageConsole : Page
    {
        public PageConsole()
        {
            InitializeComponent();
            StoppedPanel.Visibility = System.Windows.Visibility.Hidden;
            InitConsole(App.mirai);
        }

        public void InitConsole(Mirai mirai)
        {
            mirai.onDataReceived += DataReceived;
            mirai.onExited += Exited;
        }

        private void Exited()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    StoppedPanel.Visibility = System.Windows.Visibility.Visible;
                });
            }
            catch
            {
                // 收声
            }
        }
        private void DataReceived(string data)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    doc.Blocks.Add(ConsoleFormatTransfer.ToParagraph(data, flow.Foreground));
                    if (autoScroll.IsChecked.GetValueOrDefault(false)) scroll.ScrollToEnd();
                });
            }
            catch { }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (App.mirai != null && e.Key == Key.Enter && textInput.Text.Length > 0)
            {
                App.mirai.WriteLine(textInput.Text);
                textInput.Text = "";
            }
        }

        private void ButtonRestart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StoppedPanel.Visibility = System.Windows.Visibility.Hidden;
            flow.Document.Blocks.Clear();
            App.mirai.Start();
        }

        private void ButtonInitMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StoppedPanel.Visibility = System.Windows.Visibility.Hidden;
            flow.Document.Blocks.Clear();
            App.PageInit.BtnStart.IsEnabled = true;
            MainWindow.Navigate(App.PageInit);
        }
    }
}

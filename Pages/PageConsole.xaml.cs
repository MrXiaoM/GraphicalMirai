using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;

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
            InitConsole(App.mirai);
        }

        public void InitConsole(Mirai mirai)
        {
            mirai.onDataReceived += DataReceived;
            mirai.onExited += Exited;
        }

        private void Exited()
        {
            Dispatcher.Invoke(() =>
            {
                Paragraph p = new Paragraph();
                p.LineHeight = 5;
                Button btnRestart = new Button() { Content = "重新启动" };
                btnRestart.Click += delegate
                {
                    flow.Document.Blocks.Clear();
                    App.mirai.Start();
                };
                Button btnInitMenu = new Button() { Content = "返回欢迎菜单" };
                btnInitMenu.Click += delegate
                {
                    flow.Document.Blocks.Clear();
                    App.PageInit.BtnStart.IsEnabled = true;
                    MainWindow.Navigate(App.PageInit);
                };
                p.Inlines.Add(btnRestart);
                p.Inlines.Add(new Rectangle() { Width = 5 });
                p.Inlines.Add(btnInitMenu);
                p.Inlines.Add(new LineBreak());
                doc.Blocks.Add(p);
            });
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
    }
}

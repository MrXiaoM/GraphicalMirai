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
            App.mirai.onDataReceived += DataReceived;
            App.mirai.onExited += Exited;
        }

        private void Exited()
        {
            Dispatcher.Invoke(() =>
            {
                if (textConsole.Inlines.Count > 0) textConsole.Inlines.Add(new LineBreak());
                Button btnRestart = new Button() { Content = "重新启动" };
                btnRestart.Click += delegate
                {
                    textConsole.Inlines.Clear();
                    App.mirai.Start();
                };
                Button btnInitMenu = new Button() { Content = "返回欢迎菜单" };
                btnInitMenu.Click += delegate
                {
                    textConsole.Inlines.Clear();
                    App.PageInit.BtnStart.IsEnabled = true;
                    MainWindow.Navigate(App.PageInit);
                };
                textConsole.Inlines.Add(btnRestart);
                textConsole.Inlines.Add(new Rectangle() { Width = 5 });
                textConsole.Inlines.Add(btnInitMenu);
                textConsole.Inlines.Add(new LineBreak());
            });
        }
        private void DataReceived(string data)
        {
            Dispatcher.Invoke(() =>
            {
                if (textConsole.Inlines.Count > 0) textConsole.Inlines.Add(new LineBreak());
                
                //textConsole.Inlines.Add(data.Replace("\0", "\\0"));
                ConsoleFormatTransfer.AppendTo(textConsole, data);
                if (autoScroll.IsChecked.GetValueOrDefault(false)) scroll.ScrollToBottom();
            });
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && textInput.Text.Length > 0)
            {
                App.mirai.WriteLine(textInput.Text);
                textInput.Text = "";
            }
        }
    }
}

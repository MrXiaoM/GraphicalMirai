using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
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
    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = ((string)(parameter ?? "0,0,0,0")).Replace(";", ",");
            if (value != null)
            {
                param = param.Replace("x", value.ToString());
            }
            return param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// InnerMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class InnerMessageBox : UserControl
    {
        AutoResetEvent notice = new(false);
        MessageBoxResult result = MessageBoxResult.None;
        public InnerMessageBox()
        {
            InitializeComponent();
            BgRect.Opacity = 0;
            BgMsgBox.Height = 0;
        }

        private void SetButton(Button button, bool visible)
        {
            button.IsEnabled = visible;
            button.Width = visible ? 150 : 0;
            int x = visible ? 2 : 0;
            button.Margin = new Thickness(x, 0, x, 0);
        }
        private void SetButton(MessageBoxButton button)
        {
            bool ok = button == MessageBoxButton.OK || button == MessageBoxButton.OKCancel;
            bool cancel = button == MessageBoxButton.OKCancel || button == MessageBoxButton.YesNoCancel;
            bool yesno = button == MessageBoxButton.YesNo || button == MessageBoxButton.YesNoCancel;
            SetButton(BtnOK, ok);
            SetButton(BtnCancel, cancel);
            SetButton(BtnYes, yesno);
            SetButton(BtnNo, yesno);
        }

        public MessageBoxResult Show(string content, string title, MessageBoxButton button = MessageBoxButton.OK)
        {
            return Show(new Inline[] { new Run(content) }, title, button);
        }
        public MessageBoxResult Show(Inline[] content, string title, MessageBoxButton button = MessageBoxButton.OK)
        {
            Dispatcher.Invoke(() =>
            {
                SetButton(button);
                MsgTitle.Text = title;
                MsgContent.Inlines.Clear();
                MsgContent.Inlines.AddRange(content);
            });
            // TODO 执行动画
            ShowMessageBg();
            // 等待响应
            notice.WaitOne();
            notice.Reset();
            HideMessageBg();
            return result;
        }

        public void ShowMessageBg()
        {
            StackButton.IsEnabled = true;
            BgRect.Opacity = 1;
            BgMsgBox.Height = double.NaN;
        }
        public void HideMessageBg()
        {
            StackButton.IsEnabled = false;
            BgRect.Opacity = 0;
            BgMsgBox.Height = 0;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            result = MessageBoxResult.OK;
            notice.Set();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            result = MessageBoxResult.Yes;
            notice.Set();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            result = MessageBoxResult.No;
            notice.Set();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            result = MessageBoxResult.Cancel;
            notice.Set();
        }
    }
}

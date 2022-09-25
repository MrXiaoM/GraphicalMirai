using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Animation;

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
        TaskCompletionSource<MessageBoxResult>? task;
        DoubleAnimation AniOptFadeIn = new()
        {
            From = 0,
            To = 1,
            Duration = new(TimeSpan.FromMilliseconds(200)),
            AccelerationRatio = 0.6,
            DecelerationRatio = 0.3
        };
        DoubleAnimation AniOptFadeOut = new()
        {
            From = 1,
            To = 0,
            Duration = new(TimeSpan.FromMilliseconds(200)),
            AccelerationRatio = 0.6,
            DecelerationRatio = 0.3
        };
        Storyboard AniIn
        {
            get
            {
                Visibility = Visibility.Hidden;
                BgMsgBox.Height = double.NaN;
                BgMsgBox.UpdateLayout();
                Thickness padding = BgMsgBox.Padding;
                BgMsgBoxInner.UpdateLayout();
                double height = BgMsgBoxInner.DesiredSize.Height + padding.Top + padding.Bottom;
                Storyboard storyboard = new();
                DoubleAnimation aniIn = new()
                {
                    From = 0,
                    To = height,
                    Duration = new(TimeSpan.FromMilliseconds(200)),
                    AccelerationRatio = 0.6,
                    DecelerationRatio = 0.3
                };
                Storyboard.SetTarget(aniIn, BgMsgBox);
                Storyboard.SetTargetProperty(aniIn, new("Height"));
                storyboard.Children.Add(AniOptFadeIn);
                storyboard.Children.Add(aniIn);
                storyboard.Completed += delegate { BgMsgBox.Height = double.NaN; };
                BgMsgBox.Height = 0;
                Visibility = Visibility.Visible;
                return storyboard;
            }
        }
        Storyboard AniOut
        {
            get
            {
                double height = BgMsgBox.ActualHeight;
                Storyboard storyboard = new();
                DoubleAnimation aniOut = new()
                {
                    From = height,
                    To = 0,
                    Duration = new(TimeSpan.FromMilliseconds(200)),
                    AccelerationRatio = 0.6,
                    DecelerationRatio = 0.3
                };
                Storyboard.SetTarget(aniOut, BgMsgBox);
                Storyboard.SetTargetProperty(aniOut, new("Height"));
                storyboard.Children.Add(AniOptFadeOut);
                storyboard.Children.Add(aniOut);
                storyboard.Completed += delegate { Visibility = Visibility.Hidden; BgMsgBox.Height = double.NaN; };
                return storyboard;
            }
        }
        public InnerMessageBox()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            BgRect.Opacity = 0;
            BgMsgBox.Height = 0;
            Storyboard.SetTarget(AniOptFadeIn, BgRect);
            Storyboard.SetTargetProperty(AniOptFadeIn, new("Opacity"));
            Storyboard.SetTarget(AniOptFadeOut, BgRect);
            Storyboard.SetTargetProperty(AniOptFadeOut, new("Opacity"));
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

        public Task<MessageBoxResult> ShowAsync(string content, string title, MessageBoxButton button = MessageBoxButton.OK)
        {
            return ShowAsync(() => new[] { new Run(content) }, title, button);
        }
        public async Task<MessageBoxResult> ShowAsync(Func<Inline[]> content, string title, MessageBoxButton button = MessageBoxButton.OK)
        {
            if (!task?.Task.IsCompleted ?? false)
            {
                task?.SetResult(MessageBoxResult.None);
            }
            Dispatcher.Invoke(() =>
            {
                Visibility = Visibility.Visible;
                SetButton(button);
                MsgTitle.Text = title;
                MsgContent.Inlines.Clear();
                MsgContent.Inlines.AddRange(content.Invoke());
            });
            await Task.Run(async () =>
            {
                // 似乎在窗口 Loading 期间不能计算高度，这时执行动画会错位
                // 故等待到高度不为 0 时再执行动画
                while (BgMsgBoxInner.ActualHeight == 0)
                {
                    await Task.Delay(200);
                }
            });
            ShowMessageBg();
            task = new TaskCompletionSource<MessageBoxResult>();
            // 等待响应
            MessageBoxResult result = await task.Task;
            HideMessageBg();
            return result;
        }

        public void ShowMessageBg()
        {
            Dispatcher.Invoke(() =>
            {
                IsHitTestVisible = true;
                StackButton.IsEnabled = true;
                BeginStoryboard(AniIn);
            });
        }
        public void HideMessageBg()
        {
            Dispatcher.Invoke(() =>
            {
                IsHitTestVisible = false;
                StackButton.IsEnabled = false;
                BeginStoryboard(AniOut);
            });
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            task?.SetResult(MessageBoxResult.OK);
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            task?.SetResult(MessageBoxResult.Yes);
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            task?.SetResult(MessageBoxResult.No);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            StackButton.IsEnabled = false;
            task?.SetResult(MessageBoxResult.Cancel);
        }
    }
}

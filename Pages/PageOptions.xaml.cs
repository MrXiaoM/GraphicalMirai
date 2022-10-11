using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GraphicalMirai.Pages
{
    /// <summary>
    /// PageOptions.xaml 的交互逻辑
    /// </summary>
    public partial class PageOptions : Page
    {
        private static void saveConfig() => Config.Save();
        public static void ListenProperty(CheckBox cb, Action<bool> action) => ListenProperty(cb, action, saveConfig);
        public static void ListenProperty(CheckBox cb, Action<bool> action, Action action1)
        {
            cb.Checked += delegate { action(true); action1(); };
            cb.Unchecked += delegate { action(false); action1(); };
        }

        public static void ListenProperty(ToggleButton tb, Action<bool> action) => ListenProperty(tb, action, saveConfig);
        public static void ListenProperty(ToggleButton tb, Action<bool> action, Action action1)
        {
            tb.Checked += delegate { action(true); action1(); };
            tb.Unchecked += delegate { action(false); action1(); };
        }

        public static void ListenProperty(TextBox tb, Action<string> action) => ListenProperty(tb, action, saveConfig);
        public static void ListenProperty(TextBox tb, Action<string> action, Action action1)
        {
            tb.LostFocus += delegate { action(tb.Text); action1(); };
        }

        public static void ListenProperty(IntegerTextBox tb, Action<int> action) => ListenProperty(tb, action, saveConfig);
        public static void ListenProperty(IntegerTextBox tb, Action<int> action, Action action1)
        {
            tb.ValueChanged += delegate { action(tb.Value); action1(); };
        }

        private static Config config => Config.Instance;

        public PageOptions()
        {
            InitializeComponent();
            InitializePropertiesListenenr();
        }

        public void InitializePropertiesListenenr()
        {
            // 先初始化值
            CheckUseGhProxy.IsChecked = config.useGhProxy;
            CheckSocketBridge.IsChecked = config.useBridge;
            TextJavaPath.Text = config.javaPath;
            TextJavaExtArgs.Text = config.extArgs;
            TextJavaMainClass.Text = config.mainClass;
            TextBridgePort.Value = config.bridgePort;

            // 再注册监听器
            ListenProperty(CheckUseGhProxy, v => config.useGhProxy = v);
            ListenProperty(CheckSocketBridge, v => config.useBridge = v);
            ListenProperty(TextJavaPath, v => config.javaPath = v);
            ListenProperty(TextJavaExtArgs, v => config.extArgs = v);
            ListenProperty(TextJavaMainClass, v => config.mainClass = v);
            ListenProperty(TextBridgePort, v => config.bridgePort = v);
        }
        
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow.Navigate(!App.mirai.IsRunning ? App.PageInit : App.PageMain);
        }
    }
}

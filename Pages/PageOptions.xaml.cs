using System;
using System.Windows.Controls;

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

        private static Config config => Config.Instance;

        public PageOptions()
        {
            InitializeComponent();
            InitializePropertiesListenenr();
        }

        public void InitializePropertiesListenenr()
        {
            ListenProperty(CheckUseGhProxy, v => config.useGhProxy = v);
        }
        
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow.Navigate(App.mirai == null ? App.PageInit : App.PageMain);
        }
    }
}

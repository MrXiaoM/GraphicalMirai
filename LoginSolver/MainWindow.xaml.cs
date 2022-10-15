using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Windows;

namespace LoginSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string url;
        public MainWindow(string url)
        {
            this.url = url;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Creating CoreWebView2...");
            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: "cache");
            await webView.EnsureCoreWebView2Async(env);
        }

        private async void webView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                Console.WriteLine("Created failed!");
                Console.WriteLine(e.InitializationException);
                Environment.Exit(-1);
                return;
            }
            webView.CoreWebView2.NavigationCompleted += delegate { textLoading.Visibility = Visibility.Hidden; };
            Console.WriteLine("Created successful!");
            Console.WriteLine("Enabling devTools network features...");
            // 开启设备仿真
            Console.WriteLine("Emulation.setUserAgentOverride(App.UserAgent) >> " + await webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setUserAgentOverride",
                JsonConvert.SerializeObject(new { userAgent = App.UserAgent })
            ));
            Console.WriteLine("Emulation.setTouchEmulationEnabled(true) >> " + await webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setTouchEmulationEnabled",
                JsonConvert.SerializeObject(new { enabled = true })
            ));
            // 打开网络监视
            Console.WriteLine("Network.enable() >> " + await webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.enable", "{}"));
            var eventRecieiver = webView.CoreWebView2.GetDevToolsProtocolEventReceiver("Network.responseReceived");
            eventRecieiver.DevToolsProtocolEventReceived += async (s, e) =>
            {
                var received = JsonConvert.DeserializeObject<ResponseReceived>(e.ParameterObjectAsJson);
                if (!(received?.response.url.ToLower().Contains("t.captcha.qq.com/cap_union_new_verify") ?? false)) return;

                string id = received.requestId;
                string responseBody = await webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.getResponseBody",
                    JsonConvert.SerializeObject(new { requestId = id, })
                );
                var body = JsonConvert.DeserializeObject<ResponseBody>(responseBody)?.GetBody();
                if (body == null) return;
                var captcha = JsonConvert.DeserializeObject<CaptchaResult>(body);
                if (captcha == null) return;
                if (captcha.errorCode == "0")
                {
                    Console.WriteLine("ticket: " + captcha.ticket);
                    this.Close();
                    return;
                }
            };
            Console.WriteLine("Everything done! Navigating to url and waiting for response.");
            webView.Source = new(url);
        }
    }
}

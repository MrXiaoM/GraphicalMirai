using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
            var env = await CoreWebView2Environment.CreateAsync();
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
            Console.WriteLine("Created successful!");
            Console.WriteLine("Enabling devTools network features...");
            await webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.enable", "{}");
            var eventRecieiver = webView.CoreWebView2.GetDevToolsProtocolEventReceiver("Network.responseReceived");
            eventRecieiver.DevToolsProtocolEventReceived += async (s, e) =>
            {
                var received = JsonConvert.DeserializeObject<ResponseReceived>(e.ParameterObjectAsJson);
                if (!(received?.response.url.ToLower().Contains("t.captcha.qq.com/cap_union_new_verify") ?? false)) return;

                string id = received.requestId;
                string responseBody = await webView.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.getResponseBody",
                    JsonConvert.SerializeObject(new{ requestId = id, })
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

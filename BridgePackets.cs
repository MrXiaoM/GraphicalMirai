using Net.Codecrete.QrCodeGenerator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphicalMirai
{
    /// <summary>
    /// 通信桥数据包管理
    /// 
    /// 注意: GraphicalMirai 启动器与插件的数据包命名方式不同。
    /// 一方的 In 对应另一方的 Out，一方的 Out 对应另一方的 In
    /// In 永远是自身收到的包，Out 永远是自身发出去的包
    /// </summary>
    public class BridgePackets
    {
        public static readonly ReadOnlyDictionary<string, Type> packetsIn = new(new Dictionary<string, Type>() {
            { "SolveSliderCaptcha", typeof(InSolveSliderCaptcha) },
            { "LoginVerify", typeof(InLoginVerify) },
            { "SmsVerify", typeof(InSmsVerify) },
        });
    }
#pragma warning disable CS8618
    public interface IPacketIn
    {
        public void handle();
    }
    public class InSolveSliderCaptcha : IPacketIn
    {
        public string url;
        public void handle()
        {
            App.mirai.onDataReceived($"[通信桥] 滑块验证码请求 {url}");
            Process process = new Process();
            process.StartInfo = new(App.path("tools/LoginSolver/LoginSolver.exe"))
            {
                WorkingDirectory = App.path("tools/LoginSolver"),
                Arguments = "--url=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(url)),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                CreateNoWindow = true
            };
            void processLogReceive(string data)
            {
                Console.WriteLine("[LoginSolver] " + data);
                if (data.StartsWith("ticket: "))
                {
                    string ticket = data.Substring(8);
                    // 回调滑块验证码
                    App.mirai.onDataReceived($"[通信桥] 回应滑块验证码 {ticket}");
                    App.mirai.WriteLine(ticket);
                }
            };
            process.OutputDataReceived += (s, e) => processLogReceive(e.Data ?? "");
            process.ErrorDataReceived += (s, e) => processLogReceive(e.Data ?? "");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
    }
    public class InLoginVerify : IPacketIn
    {
        public string url;
        public void handle()
        {
            Console.WriteLine("登录验证 " + url);
            MainWindow.Instance.Dispatcher.BeginInvoke(async () =>
            {
                Geometry qrcode = await Task.Run(() => Geometry.Parse(QrCode.EncodeText(url, QrCode.Ecc.Medium).ToGraphicsPath(1)));
                qrcode.Freeze();
                await MainWindow.Msg.ShowAsync(() =>
                {

                    List<Inline> content = new();
                    content.Add(new Run()
                    {
                        Text = "该账户有设备锁/不常用登录地点/不常用设备登录的问题\n" +
                        "请在手机 QQ 扫描以下二维码，确认后请点击「确定」\n"
                    });
                    content.Add(new InlineUIContainer(new Path()
                    {
                        Fill = App.hexBrush("#FFFFFF"),
                        Data = qrcode,
                        LayoutTransform = new ScaleTransform(2, 2)
                    }));
                    return content.ToArray();
                }, "需要进行账户安全认证");
                App.mirai.WriteLine("LoginVerfied");
            });
        }
    }

    public class InSmsVerify : IPacketIn
    {

        public string countryCode;
        public string phoneNumber;
        public async void handle()
        {
            string phone = countryCode.Length > 0 && phoneNumber.Length > 0 ? $"(+{countryCode}) {phoneNumber}" : "(无法获取到手机号码)";
            if(await MainWindow.Msg.ShowAsync(
                $"一条短信验证码将发送到你的手机 {phone}.\n" +
                $"运营商可能会收取正常短信费用, 是否继续?", 
                "短信验证", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                App.mirai.WriteLine("yes");
                TextBox? tb = null;
                await MainWindow.Msg.ShowAsync(() =>
                {
                    tb = new();
                    List<Inline> content = new();
                    content.Add(new Run("验证码已发送，请注意查收。\n" +
                        "收到验证码后填入下方并点击「确定」完成短信验证。\n\n"));
                    content.Add(new InlineUIContainer(tb));
                    return content.ToArray();
                }, "短信验证", System.Windows.MessageBoxButton.OK);
                App.mirai.WriteLine(tb?.Text ?? "");
            }
            else
            {
                App.mirai.WriteLine("no");
            }
        }
    }

    public interface IPacketOut
    {
        public string type();
    }
    public class OutLoginVerify : IPacketOut
    {
        public string type() => "LoginVerify";
    }
#pragma warning restore CS8618
    public static class SocketWrapperExt
    {
        public static bool SendPacket<T>(this SocketWrapper socket, T packet) where T : IPacketOut
        {
            var json = JObject.FromObject(packet);
            json.Add(new JProperty("type", packet.type()));
            string data = json.ToString(Formatting.None);
            return socket.SendRawMessage(data);
        }
    }
}

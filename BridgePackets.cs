using Markdig.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

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
            { "SolveSliderCaptcha", typeof(InSolveSliderCaptcha) }
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
            process.ErrorDataReceived += (s, e)=> processLogReceive(e.Data ?? "");
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
            Console.WriteLine("登录验证 "+url);
            Task.Run(async () =>
            {
                await MainWindow.Msg.ShowAsync(() =>
                {
                    List<Inline> content = new();
                    content.Add(new Run()
                    {
                        Text = "该账户有设备锁/不常用登录地点/不常用设备登录的问题\n" +
                        "请在手机 QQ 扫描以下二维码，确认后请点击「确定」\n"
                    });
                    content.Add(new InlineUIContainer(new Image()
                    {
                        Width = 300,
                        Height = 300,
                        Source = App.GenerateQRCode(url).ToBitmapImage()
                    }));
                    return content.ToArray();
                }, "需要进行账户安全认证");
                App.mirai.SendPacket(new OutLoginVerify());
            });

        }
    }
    public interface IPacketOut
    {
        public string type();
    }
    public class OutSolveSliderCaptcha : IPacketOut
    {
        public string type() => "OutSolveSliderCaptcha";
        string ticket;
        public OutSolveSliderCaptcha(string ticket)
        {
            this.ticket = ticket;
        }
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

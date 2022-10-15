using Markdig.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
            // TODO: 处理滑块验证码
            App.mirai.onDataReceived($"[通信桥] 滑块验证码请求 {url}");
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

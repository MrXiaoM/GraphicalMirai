using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace GraphicalMirai
{
    public class Mirai
    {
        public bool IsRunning { get; private set; } = false;
        public delegate void OnDataReceived(string s);
        public OnDataReceived onDataReceived;
        public delegate void OnExited();
        public OnExited onExited;
        public readonly List<string> log = new List<string>();
        public List<string> Libraries { get; private set; } = new List<string>();
        public Process Process { get; private set; } = new Process();
        public string? WorkingDir { get; private set; }
        public string? JavaHome { get; private set; }
        public string? MainClass { get; private set; }
        public Task? task;
        public int socketPort = 41919;
        public Task? taskSocket;
        Socket? socketServer;
        Socket? socket;

        public Mirai()
        {
            onDataReceived += delegate (string s)
            {
                Console.WriteLine(s);
                log.Add(s);
                if (log.Count > 500) log.RemoveAt(0);
            };
            onExited += delegate
            {
                IsRunning = false;
                onDataReceived("\u001b[91mmirai-console 已停止运行\u001b[0m");
            };
        }
        public bool InitMirai(string javaHome, string workingDir, List<string> libraries, string mainClass, string extArgs = "")
        {
            if (IsRunning && !Process.HasExited) return false;
            bool useBridge = Config.Instance.useBridge;
            if (useBridge)
            {
                socketServer = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketPort = Config.Instance.bridgePort;
                IPEndPoint ipEndPoint = new(IPAddress.Loopback, socketPort);
                try
                {
                    socketServer.Bind(ipEndPoint);
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            JavaHome = javaHome;
            WorkingDir = workingDir;
            Libraries = libraries;
            MainClass = mainClass;
            Process = new();
            ProcessStartInfo psi = new(JavaHome);
            psi.WorkingDirectory = WorkingDir;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.StandardInputEncoding = Encoding.UTF8;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.CreateNoWindow = true;
            psi.Arguments = (useBridge ? $"-Dgraphicalmirai.bridge.port={socketPort} " : "") +
                (extArgs + " -classpath \"" + string.Join(";", Libraries) + "\" " + MainClass).TrimStart();
            Process.StartInfo = psi;
            Process.EnableRaisingEvents = true;
            Process.Exited += delegate
            {
                IsRunning = false;
                onExited();
            };
            return true;
        }

        public void Start()
        {
            onDataReceived("\u001b[0mmirai-console 正在启动...\u001b[0m");
            onDataReceived("\u001b[0m运行目录: " + Process.StartInfo.WorkingDirectory + "\u001b[0m");
            onDataReceived("\u001b[0m可执行文件: " + Process.StartInfo.FileName + "\u001b[0m");
            onDataReceived("\u001b[0m启动参数: " + Process.StartInfo.Arguments + "\u001b[0m");
            IsRunning = true;
            Process.Start();
            WriteLine("status");
            if (task != null) task.Dispose();
            if (Config.Instance.useBridge && socketServer != null)
            {
                socketServer.Listen(1);
                taskSocket = new Task(() =>
                {
                    while (!Process.HasExited)
                    {
                        socket = socketServer.Accept();
                        while (socket.Connected)
                        {
                            byte[] data = new byte[1024 * 1024 * 1024];
                            int readLeng = socket.Receive(data, 0, data.Length, SocketFlags.None);
                            if (readLeng == 0)
                            {
                                socket.Shutdown(SocketShutdown.Both);
                                socket.Close();
                                break;
                            }
                            var msg = Encoding.UTF8.GetString(data, 0, readLeng);
                            ReceiveRawMessage(msg);
                        }
                    }
                });
                taskSocket.Start();
            }

            task = new Task(() =>
            {
                StreamReader output = Process.StandardOutput;
                while (!output.EndOfStream)
                {
                    string data = output.ReadLine() ?? "";
                    onDataReceived(data);
                }
            });
            task.Start();
        }

        private void ReceiveRawMessage(string msg)
        {
            if (msg.StartsWith("SolveSliderCaptcha,"))
            {
                var @params = msg.Substring(19);
                var botId = @params.SubstringBefore(",").ToLong();
                var url = @params.SubstringAfter(",");
                // TODO: 处理滑块验证码
            }
        }

        private void SendRawMessage(string msg) => socket?.Send(Encoding.UTF8.GetBytes(msg));

        public void SliderCaptcha(string botId, string ticket) => SendRawMessage($"SolveSliderCaptcha,{botId},{ticket}");

        public async void Stop()
        {
            if (!IsRunning || Process.HasExited) return;
            IsRunning = false;
            WriteLine("stop");
            await Process.WaitForExitAsync();
            Process.Close();
            socketServer?.Shutdown(SocketShutdown.Both);
            socketServer?.Close();
        }

        public void WriteLine(string s)
        {
            Process.StandardInput.WriteLine(s);
        }
    }
}

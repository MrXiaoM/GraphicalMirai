using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public List<string> log = new List<string>();
        public readonly List<string> Libraries = new List<string>();
        public readonly Process Process = new Process();
        public readonly string WorkingDir;
        public readonly string JavaPath;
        public string MainClass;
        public Task? task;
        public Mirai(string javaPath, string workingDir, List<string> libraries, string mainClass, string extArgs = "")
        {
            JavaPath = javaPath;
            WorkingDir = workingDir;
            Libraries = libraries;
            MainClass = mainClass;
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

            ProcessStartInfo psi = new ProcessStartInfo(JavaPath);
            psi.WorkingDirectory = WorkingDir;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.StandardInputEncoding = Encoding.UTF8;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.CreateNoWindow = true;
            psi.Arguments = (extArgs + " -classpath \"" + string.Join(";", Libraries) + "\" " + MainClass).TrimStart();
            Process.StartInfo = psi;
            Process.EnableRaisingEvents = true;
            Process.Exited += delegate { onExited(); } ;
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

        public async Task Stop()
        {
            WriteLine("stop");
            await Process.WaitForExitAsync();
            Process.Close();
        }

        public void WriteLine(string s)
        {
            Process.StandardInput.WriteLine(s);
        }
    }
}

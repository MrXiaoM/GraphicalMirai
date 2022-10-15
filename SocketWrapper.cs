using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphicalMirai
{
    public class SocketWrapper : IDisposable
    {
        public delegate void OnClientConnected(Socket client);
        public OnClientConnected onClientConnected;
        public delegate void OnClientDisconnected(Exception? ex);
        public OnClientDisconnected onClientDisconnected;
        public delegate void OnDataReceive(Socket client, string data);
        public OnDataReceive onDataReceive;

        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket? client;
        int port;
        bool isDisposed = false;
        Thread thread;
        /// <summary>
        /// 通信桥 Socket 包装
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <param name="port">端口</param>
        public SocketWrapper(int port)
        {
            onClientConnected += delegate { };
            onClientDisconnected += delegate { };
            onDataReceive += delegate { };
            this.port = port;
            socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            socket.Listen(1);
            thread = new Thread(ConnectionLoop);
            thread.Start();
        }

        private void ConnectionLoop()
        {
            while (!isDisposed)
            {
                try 
                {
                    client = socket.Accept();
                }
                catch 
                {
                    continue; 
                }
                onClientConnected(client);
                byte[] buffer = new byte[1024];
                int length;
                try
                {
                    while (true)
                    {
                        if ((length = client.Receive(buffer)) == 0) break;
                        string data = Encoding.UTF8.GetString(buffer, 0, length);
                        onDataReceive(client, data);
                    }
                    onClientDisconnected(null);
                }
                catch(Exception ex)
                {
                    onClientDisconnected(ex);
                    ex.PrintStacktrace();
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client.Dispose();
                }

            }
        }

        public bool SendRawMessage(string data)
        {
            if (client == null || client.Connected) return false;
            client.Send(Encoding.UTF8.GetBytes(data));
            return true;
        }

        public void Dispose()
        {
            socket.Close();
            socket.Dispose();
        }
    }
}

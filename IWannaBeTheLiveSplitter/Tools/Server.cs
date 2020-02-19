using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IWannaBeTheLiveSplitter.Tools
{
    public class Server : IDisposable
    {
        TcpListener Listener;
        public string Text = "N\\A";

        public void Start(int Port)
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();

            while (true)
            {
                var client = Listener.AcceptTcpClient();
                string Html = Text;
                string Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length + "\n\n" + Html;
                byte[] Buffer = Encoding.ASCII.GetBytes(Str);
                client.GetStream().Write(Buffer, 0, Buffer.Length);
                client.Close();
            }
        }

        void IDisposable.Dispose()
        {
            Listener?.Stop();
        }
    }
}

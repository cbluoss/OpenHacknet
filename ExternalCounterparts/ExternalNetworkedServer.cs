using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Hacknet.ExternalCounterparts
{
    public class ExternalNetworkedServer
    {
        private const int BUFFER_SIZE = 4096;
        private static ASCIIEncoding encoder;
        private readonly Dictionary<NetworkStream, byte[]> buffers;
        private readonly List<TcpClient> connections;
        private TcpListener listener;
        public Action<string> messageReceived;

        public ExternalNetworkedServer()
        {
            connections = new List<TcpClient>();
            buffers = new Dictionary<NetworkStream, byte[]>();
            if (encoder != null)
                return;
            encoder = new ASCIIEncoding();
        }

        public void initializeListener()
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, Multiplayer.PORT));
            listener.AllowNatTraversal(true);
            listener.Start();
            listener.BeginAcceptTcpClient(AcceptTcpConnectionCallback, listener);
        }

        public void closeServer()
        {
            listener.Stop();
            foreach (var tcpClient in connections)
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
        }

        private void AcceptTcpConnectionCallback(IAsyncResult ar)
        {
            var tcpListener = ar.AsyncState as TcpListener;
            if (tcpListener == null)
                return;
            try
            {
                var tcpClient = listener.EndAcceptTcpClient(ar);
                if (tcpClient == null)
                    return;
                connections.Add(tcpClient);
                var stream = tcpClient.GetStream();
                buffers.Add(stream, new byte[4096]);
                if (messageReceived != null)
                    messageReceived("Connection");
                tcpClient.GetStream().BeginRead(buffers[stream], 0, 4096, TcpReadCallback, stream);
                tcpListener.BeginAcceptTcpClient(AcceptTcpConnectionCallback, tcpListener);
            }
            catch (Exception ex)
            {
            }
        }

        private void TcpReadCallback(IAsyncResult ar)
        {
            var index = ar.AsyncState as NetworkStream;
            if (index == null)
                return;
            try
            {
                var count = index.EndRead(ar);
                if (count == 0)
                    return;
                var bytes = buffers[index];
                var @string = encoder.GetString(bytes, 0, count);
                if (messageReceived != null)
                    messageReceived(@string);
                index.BeginRead(buffers[index], 0, 4096, TcpReadCallback, index);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
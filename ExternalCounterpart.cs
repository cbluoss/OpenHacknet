// Decompiled with JetBrains decompiler
// Type: Hacknet.ExternalCounterpart
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Hacknet
{
    public class ExternalCounterpart
    {
        private static Dictionary<string, string> networkIPList;
        private static ASCIIEncoding encoder;
        private byte[] buffer;
        private TcpClient connection;
        public string connectionIP;
        public string idName;
        public bool isConnected;

        public ExternalCounterpart(string idName, string ipEndpoint)
        {
            this.idName = idName;
            connectionIP = ipEndpoint;
            buffer = new byte[4096];
            if (encoder != null)
                return;
            encoder = new ASCIIEncoding();
        }

        public static string getIPForServerName(string serverName)
        {
            if (networkIPList == null)
                loadNetIPList();
            if (networkIPList.ContainsKey(serverName))
                return networkIPList[serverName];
            throw new InvalidOperationException("Server Name Not Found");
        }

        private static void loadNetIPList()
        {
            networkIPList = new Dictionary<string, string>();
            var str1 = Utils.readEntireFile("Content/Network/NetworkIPList.txt");
            var separator = new string[2]
            {
                "\n\r",
                "\r\n"
            };
            var num = 1;
            foreach (var str2 in str1.Split(separator, (StringSplitOptions) num))
            {
                var strArray = str2.Split(Utils.spaceDelim);
                networkIPList.Add(strArray[0], strArray[1]);
            }
        }

        public void sendMessage(string message)
        {
            if (!isConnected)
                return;
            writeMessage(message);
        }

        public void disconnect()
        {
            if (!isConnected)
                return;
            writeMessage("Disconnecting");
            connection.Close();
            isConnected = false;
        }

        public void testConnection()
        {
            establishConnection();
            while (!isConnected)
                Thread.Sleep(5);
            writeMessage("Test Message From " + idName);
        }

        public void establishConnection()
        {
            var tcpClient = new TcpClient();
            tcpClient.BeginConnect(IPAddress.Parse(connectionIP), Multiplayer.PORT, DoTcpConnectionCallback, tcpClient);
        }

        private void DoTcpConnectionCallback(IAsyncResult ar)
        {
            var tcpClient = ar.AsyncState as TcpClient;
            if (tcpClient == null || !tcpClient.Connected)
                return;
            isConnected = true;
            connection = tcpClient;
            tcpClient.EndConnect(ar);
        }

        public void writeMessage(string message)
        {
            if (isConnected)
            {
                var stream = connection.GetStream();
                buffer = encoder.GetBytes(message);
                stream.BeginWrite(buffer, 0, buffer.Length, TCPWriteMessageCallback, stream);
            }
            else
                establishConnection();
        }

        private void TCPWriteMessageCallback(IAsyncResult ar)
        {
            var networkStream = ar.AsyncState as NetworkStream;
            if (networkStream == null)
                return;
            try
            {
                networkStream.EndWrite(ar);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
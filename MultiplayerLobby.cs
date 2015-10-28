// Decompiled with JetBrains decompiler
// Type: Hacknet.MultiplayerLobby
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Hacknet.Gui;
using Hacknet.Magic;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class MultiplayerLobby : GameScreen
    {
        public static List<string> allLocalIPs = new List<string>();
        private byte[] buffer;
        private TcpClient chatclient;
        private NetworkStream chatclientStream;
        private string chatIP = "";
        private TcpClient client;
        private NetworkStream clientStream;
        private bool connectingToServer;
        private readonly Color dark_ish_gray = new Color(20, 20, 20);
        private readonly Color darkgrey = new Color(9, 9, 9);
        private string destination = "192.168.1.1";
        private ASCIIEncoding encoder;
        private string externalIP = "Loading...";
        private Rectangle fullscreen;
        private bool isConnecting;
        private TcpListener listener;
        private Thread listenerThread;
        private List<string> messages;
        private string messageString = "Test Message";
        private string myIP = "?";
        private bool shouldAddServer;

        public MultiplayerLobby()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.1);
            TransitionOffTime = TimeSpan.FromSeconds(0.3);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            try
            {
                listener = new TcpListener(IPAddress.Any, Multiplayer.PORT);
                listener.Start();
                listener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, listener);
                buffer = new byte[4096];
                encoder = new ASCIIEncoding();
                if (allLocalIPs.Count == 0)
                {
                    myIP = getLocalIP();
                    new Thread(getExternalIP).Start();
                    Console.WriteLine("Started Multiplayer IP Getter Thread");
                }
                else
                    myIP = allLocalIPs[0];
                messages = new List<string>();
                var viewport = ScreenManager.GraphicsDevice.Viewport;
                fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            }
            catch (Exception ex)
            {
                ScreenManager.RemoveScreen(this);
                ScreenManager.AddScreen(new MainMenu(), ScreenManager.controllingPlayer);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (!shouldAddServer || ScreenState == ScreenState.TransitionOff)
                return;
            ExitScreen();
            ScreenManager.AddScreen(new OS(client, clientStream, true, ScreenManager), ScreenManager.controllingPlayer);
            if (chatclient == null)
                return;
            chatclient.Close();
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            GuiData.doInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            GuiData.startDraw();
            ScreenManager.SpriteBatch.Draw(Utils.white, fullscreen, isConnecting ? Color.Gray : Color.Black);
            doGui();
            GuiData.endDraw();
            ScreenManager.FadeBackBufferToBlack(byte.MaxValue - TransitionAlpha);
        }

        public void drawConnectingGui()
        {
            PatternDrawer.draw(new Rectangle(100, 200, 600, 250), 1.4f, Color.DarkGreen*0.3f, Color.DarkGreen,
                GuiData.spriteBatch);
            TextItem.doLabel(new Vector2(110f, 210f), "Connecting...", new Color?());
        }

        public void doGui()
        {
            PatternDrawer.draw(new Rectangle(180, 160, 500, 85), 1f, darkgrey, dark_ish_gray, GuiData.spriteBatch);
            TextItem.doSmallLabel(new Vector2(200f, 170f), "IP To Connect to:", new Color?());
            destination = TextBox.doTextBox(100, 200, 200, 300, 1, destination, GuiData.smallfont);
            if (Button.doButton(123, 510, 201, 120, 23, "Connect", new Color?()) || TextBox.BoxWasActivated)
            {
                ConnectToServer(destination);
            }
            else
            {
                if (isConnecting)
                    drawConnectingGui();
                TextItem.doLabel(new Vector2(200f, 300f), "Local IPs: " + myIP, new Color?());
                var y = 340f;
                for (var index = 0; index < allLocalIPs.Count - 1; ++index)
                {
                    TextItem.doLabel(new Vector2(351f, y), allLocalIPs[index], new Color?());
                    y += 40f;
                }
                TextItem.doLabel(new Vector2(200f, y + 40f), "Extrn IP: " + externalIP, new Color?());
                var pos = new Vector2(610f, 280f);
                TextItem.doLabel(pos, "Info:", new Color?());
                pos.Y += 40f;
                var text =
                    DisplayModule.cleanSplitForWidth(
                        "To Begin a multiplayer session, type in the IP of the computer you want to connect to and press enter or connect. Both players must be on this screen. To connect over the internet, use the extern IP address and ensure port 3030 is open.",
                        400);
                TextItem.doFontLabel(pos, text, GuiData.tinyfont, Color.DarkGray, float.MaxValue, float.MaxValue);
                if (!Button.doButton(999, 10, 10, 200, 30, "<- Back to Menu", Color.Gray))
                    return;
                ExitScreen();
                ScreenManager.AddScreen(new MainMenu(), ScreenManager.controllingPlayer);
            }
        }

        public void ConnectToServer(string ip)
        {
            connectingToServer = true;
            try
            {
                var socket = new TcpClient();
                var remoteEP = new IPEndPoint(IPAddress.Parse(ip), Multiplayer.PORT);
                socket.Connect(remoteEP);
                var stream = socket.GetStream();
                buffer = encoder.GetBytes("connect Client Connecting");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
                stream.Read(buffer, 0, buffer.Length);
                ExitScreen();
                ScreenManager.AddScreen(new OS(socket, stream, false, ScreenManager), ScreenManager.controllingPlayer);
            }
            catch (Exception ex)
            {
                DebugLog.add(ex.ToString());
                isConnecting = false;
            }
        }

        public void chatToServer(string ip, string msg)
        {
            try
            {
                if (chatclientStream == null)
                {
                    chatclient = new TcpClient();
                    chatclient.Connect(new IPEndPoint(IPAddress.Parse(ip), Multiplayer.PORT));
                    chatclientStream = chatclient.GetStream();
                }
                buffer = encoder.GetBytes("Chat " + msg);
                messages.Add("Me: " + msg);
                chatclientStream.Write(buffer, 0, buffer.Length);
                chatclientStream.Flush();
                messageString = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            var tcpListener = (TcpListener) ar.AsyncState;
            try
            {
                client = tcpListener.EndAcceptTcpClient(ar);
                clientStream = client.GetStream();
                var flag = true;
                while (flag)
                {
                    var num = 0;
                    try
                    {
                        num = clientStream.Read(buffer, 0, 4096);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        if (Game1.threadsExiting)
                            break;
                    }
                    if (num == 0)
                    {
                        client.Close();
                        if (Game1.threadsExiting)
                            break;
                    }
                    var @string = encoder.GetString(buffer);
                    var chArray1 = new char[1]
                    {
                        ' '
                    };
                    if (@string.Split(chArray1)[0].Equals("Chat"))
                    {
                        var chArray2 = new char[3]
                        {
                            ' ',
                            '\n',
                            char.MinValue
                        };
                        messages.Add(@string.Substring(4).Trim(chArray2).Replace("\0", ""));
                        if (Game1.threadsExiting)
                            break;
                    }
                    else
                    {
                        buffer = encoder.GetBytes("Replying - Hello World");
                        clientStream.Write(buffer, 0, buffer.Length);
                        clientStream.Flush();
                        shouldAddServer = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void listenForConnections()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, Multiplayer.PORT);
                listener.Start();
                client = listener.AcceptTcpClient();
                clientStream = client.GetStream();
                var flag = true;
                while (flag)
                {
                    var num = 0;
                    try
                    {
                        num = clientStream.Read(buffer, 0, 4096);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        if (Game1.threadsExiting)
                            break;
                    }
                    if (num == 0)
                    {
                        client.Close();
                        if (Game1.threadsExiting)
                            break;
                    }
                    var @string = encoder.GetString(buffer);
                    var chArray1 = new char[1]
                    {
                        ' '
                    };
                    if (@string.Split(chArray1)[0].Equals("Chat"))
                    {
                        var chArray2 = new char[3]
                        {
                            ' ',
                            '\n',
                            char.MinValue
                        };
                        messages.Add(@string.Substring(4).Trim(chArray2).Replace("\0", ""));
                        if (Game1.threadsExiting)
                            break;
                    }
                    else
                    {
                        buffer = encoder.GetBytes("Replying - Hello World");
                        clientStream.Write(buffer, 0, buffer.Length);
                        clientStream.Flush();
                        shouldAddServer = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static string getLocalIP()
        {
            var str = "?";
            foreach (var ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    str = ipAddress.ToString();
                    allLocalIPs.Add(ipAddress.ToString());
                }
            }
            return str;
        }

        public void getExternalIPwithPF()
        {
            try
            {
                externalIP = "Attempting automated port-fowarding...";
                try
                {
                    if (NAT.Discover())
                    {
                        Console.WriteLine("Attempting port foward");
                        NAT.ForwardPort(Multiplayer.PORT, ProtocolType.Tcp, "Hacknet (TCP)");
                        externalIP = NAT.GetExternalIP().ToString();
                    }
                    else
                        ScreenManager.ShowPopup("You dont have UPNP enabled - Internet play will not work");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                if (!externalIP.Equals("Attempting automated port-fowarding..."))
                    return;
                externalIP = "Automated port-fowarding Failed - Internet Play Disabled";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                externalIP = "Automated port-fowarding Failed - Internet Play Disabled";
            }
        }

        public void getExternalIP()
        {
            try
            {
                externalIP = "Unknown...";
                externalIP = new WebClient().DownloadString("http://icanhazip.com/");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                externalIP = "Could not Find Conection";
            }
        }
    }
}
using System.Collections.Generic;
using Hacknet.ExternalCounterparts;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class ServerScreen : GameScreen
    {
        private readonly Color backgroundColor = new Color(6, 6, 6);
        private bool canCloseServer;
        public bool drawingWithEffects;
        private string mainIP;
        private readonly List<string> messages;
        private readonly ExternalNetworkedServer server;

        public ServerScreen()
        {
            mainIP = MultiplayerLobby.getLocalIP();
            messages = new List<string>();
            server = new ExternalNetworkedServer();
            server.initializeListener();
            server.messageReceived += parseMessage;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            canCloseServer = !otherScreenHasFocus && !coveredByOtherScreen;
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            GuiData.doInput(input);
        }

        public void parseMessage(string msg)
        {
            addDisplayMessage(msg);
            var chArray = new char[2]
            {
                ' ',
                '\n'
            };
            var strArray = msg.Split(chArray);
            if (!strArray[0].Equals("cCDDrive"))
                return;
            if (strArray[2].Equals("open"))
                Programs.cdDrive(true);
            else
                Programs.cdDrive(false);
        }

        public void addDisplayMessage(string msg)
        {
            messages.Add(msg);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (drawingWithEffects)
                PostProcessor.begin();
            GuiData.startDraw();
            var viewport = ScreenManager.GraphicsDevice.Viewport;
            GuiData.spriteBatch.Draw(Utils.white, new Rectangle(0, 0, viewport.Width, viewport.Height), backgroundColor);
            var x = 80;
            var num1 = 80;
            TextItem.doFontLabel(new Vector2(x, num1), "HACKNET RELAY SERVER", GuiData.titlefont, new Color?(), 500f,
                50f);
            var y1 = num1 + 55;
            if (canCloseServer && Button.doButton(800, x, y1, 160, 30, "Shut Down Server", new Color?()))
            {
                server.closeServer();
                ExitScreen();
            }
            var num2 = y1 + 35;
            for (var index = 0; index < MultiplayerLobby.allLocalIPs.Count; ++index)
            {
                TextItem.doFontLabel(new Vector2(x, num2), "IP: " + MultiplayerLobby.allLocalIPs[index],
                    GuiData.smallfont, new Color?(), float.MaxValue, float.MaxValue);
                num2 += 20;
            }
            var y2 = num2 + 30;
            drawMessageLog(x, y2);
            GuiData.endDraw();
            if (!drawingWithEffects)
                return;
            PostProcessor.end();
        }

        public void drawMessageLog(int x, int y)
        {
            var num = 1f;
            var pos = new Vector2(x, y);
            for (var index = 0; index < 20 && index < messages.Count; ++index)
            {
                if (index > 10)
                    num = (float) (1.0 - (index - 10)/10.0);
                TextItem.doTinyLabel(pos, messages[messages.Count - 1 - index], Color.White*num);
                pos.Y += 13f;
            }
        }
    }
}
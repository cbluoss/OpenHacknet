// Decompiled with JetBrains decompiler
// Type: Hacknet.ISPDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class ISPDaemon : Daemon
    {
        private const float MAX_WIDTH = 7f;
        private const float MAX_RATE = 16f;
        private const float EFFECT_TIMER = 30f;
        private const float SEARCH_TIME = 2f;
        private const string ABOUT_MESSAGE_FILE = "ISP_About_Message.txt";
        private bool inspectionFlagged;
        private string ipSearch;
        private float lastTimer;
        private readonly List<ExpandingRectangleData> outlineEffectEntries = new List<ExpandingRectangleData>();
        private Computer scannedComputer;
        private ISPDaemonState state;
        private float timeEnteredLoadingScreen;

        public ISPDaemon(Computer c, OS os)
            : base(c, "ISP Management System", os)
        {
        }

        public override void initFiles()
        {
            base.initFiles();
            var folder = comp.files.root.searchForFolder("home");
            if (folder == null)
                return;
            var fileEntry =
                new FileEntry(
                    "This server is an ISP routing and management system\n" +
                    "designed to allow for easy setup and administration\nof " +
                    "third party ISP management companies.\n To use the management system, first " +
                    "aquire an\nadministrator account for this machine\n(contact your sysadmin team), " +
                    "then\n search for the IP address in question.\n\n" +
                    "If this program has technical issues,\ncontact customer support on\n" + "1-800-827-6364",
                    "ISP_About_Message.txt");
            folder.files.Add(fileEntry);
        }

        public override void navigatedTo()
        {
            state = ISPDaemonState.Welcome;
            base.navigatedTo();
        }

        public override string getSaveString()
        {
            return "<ispSystem />";
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            if (state == ISPDaemonState.AdminOnlyError || state == ISPDaemonState.NotFoundError)
                PatternDrawer.draw(bounds, 0.1f, Color.Transparent, os.lockedColor, sb, PatternDrawer.errorTile);
            var bounds1 = new Rectangle(bounds.X + 40, bounds.Y + 40, bounds.Width - 80, bounds.Height - 80);
            switch (state)
            {
                case ISPDaemonState.Welcome:
                    DrawWelcomeScreen(bounds1, sb, bounds);
                    break;
                case ISPDaemonState.About:
                    DrawAboutScreen(bounds1, sb);
                    break;
                case ISPDaemonState.Loading:
                    DrawLoadingScreen(bounds1, sb);
                    break;
                case ISPDaemonState.IPEntry:
                    DrawIPEntryScreen(bounds1, sb);
                    break;
                case ISPDaemonState.EnterIP:
                    DrawEnterIPScreen(bounds1, sb);
                    break;
                case ISPDaemonState.AdminOnlyError:
                    DrawAdminOnlyError(bounds1, sb);
                    break;
                case ISPDaemonState.NotFoundError:
                    DrawNotFoundError(bounds1, sb);
                    break;
            }
            drawOutlineEffect(bounds, sb);
        }

        private void DrawIPEntryScreen(Rectangle bounds, SpriteBatch sb)
        {
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 20), "IP Entry :: " + scannedComputer.ip,
                GuiData.font, Color.White, bounds.Width, float.MaxValue);
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 70),
                "Identified as :\"" + scannedComputer.name + "\"", GuiData.smallfont, Color.White, bounds.Width,
                float.MaxValue);
            sb.Draw(os.display.GetComputerImage(scannedComputer), new Vector2(bounds.X + 30, bounds.Y + 100),
                Color.White);
            var x = bounds.X + 30 + 130;
            var y1 = bounds.Y + 100;
            if (Button.doButton(3388301, x, y1, bounds.Width - 180, 30, "Assign New IP", os.brightUnlockedColor))
            {
                bool flag;
                do
                {
                    scannedComputer.ip = NetworkMap.generateRandomIP();
                    flag = false;
                    for (var index = 0; index < os.netMap.nodes.Count; ++index)
                    {
                        if (os.netMap.nodes[index].ip == scannedComputer.ip &&
                            os.netMap.nodes[index].idName != scannedComputer.idName)
                            flag = true;
                    }
                } while (flag);
                if (os.thisComputer.idName == scannedComputer.idName)
                    os.thisComputerIPReset();
            }
            var y2 = y1 + 34;
            if (Button.doButton(3388304, x, y2, bounds.Width - 180, 30,
                "Flag for Inspection" + (inspectionFlagged ? " : ACTIVE" : ""),
                inspectionFlagged ? Color.Gray : os.highlightColor))
                inspectionFlagged = true;
            var y3 = y2 + 34;
            Button.doButton(3388308, x, y3, bounds.Width - 180, 30, "Pioritize Routing", os.highlightColor);
            drawBackButton(bounds, ISPDaemonState.Welcome);
        }

        private void DrawLoadingScreen(Rectangle bounds, SpriteBatch sb)
        {
            var num1 = os.timer - timeEnteredLoadingScreen;
            if (num1 >= 2.0)
            {
                scannedComputer = null;
                for (var index = 0; index < os.netMap.nodes.Count; ++index)
                {
                    if (ipSearch == os.netMap.nodes[index].ip)
                    {
                        scannedComputer = os.netMap.nodes[index];
                        break;
                    }
                }
                if (scannedComputer != null)
                {
                    state = ISPDaemonState.IPEntry;
                    inspectionFlagged = false;
                }
                else
                    state = ISPDaemonState.NotFoundError;
            }
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + bounds.Height/2 - 35),
                "Scanning for " + ipSearch + " ...", GuiData.font, Color.White, bounds.Width, float.MaxValue);
            var destinationRectangle = new Rectangle(bounds.X + 10, bounds.Y + bounds.Height/2, bounds.Width - 20, 16);
            sb.Draw(Utils.white, destinationRectangle, Color.Gray);
            ++destinationRectangle.X;
            ++destinationRectangle.Y;
            destinationRectangle.Width -= 2;
            destinationRectangle.Height -= 2;
            sb.Draw(Utils.white, destinationRectangle, Color.Black);
            var num2 = Utils.QuadraticOutCurve(num1/2f);
            destinationRectangle.Width = (int) (destinationRectangle.Width*(double) num2);
            sb.Draw(Utils.white, destinationRectangle, os.highlightColor);
        }

        private void DrawEnterIPScreen(Rectangle bounds, SpriteBatch sb)
        {
            var strArray = os.getStringCache.Split(new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            }, StringSplitOptions.None);
            string str = null;
            var x = bounds.X + 10;
            var num1 = bounds.Y + 10;
            var vector2_1 = TextItem.doMeasuredSmallLabel(new Vector2(x, num1), "Enter IP Address to Scan for :",
                new Color?());
            var y1 = num1 + ((int) vector2_1.Y + 5);
            if (strArray.Length > 1)
            {
                str = strArray[1];
                if (str.Equals(""))
                    str = os.terminal.currentLine;
            }
            var destinationRectangle = new Rectangle(x, y1, bounds.Width - 20, 200);
            sb.Draw(Utils.white, destinationRectangle, os.darkBackgroundColor);
            var num2 = y1 + 80;
            var vector2_2 = TextItem.doMeasuredSmallLabel(new Vector2(x, num2), "IP Address: " + str, new Color?());
            destinationRectangle.X = x + (int) vector2_2.X + 2;
            destinationRectangle.Y = num2;
            destinationRectangle.Width = 7;
            destinationRectangle.Height = 20;
            if (os.timer%1.0 < 0.300000011920929)
                sb.Draw(Utils.white, destinationRectangle, os.outlineColor);
            var y2 = num2 + 122;
            if (strArray.Length > 2 || Button.doButton(30, x, y2, 300, 22, "Scan", os.highlightColor))
            {
                if (strArray.Length <= 2)
                    os.terminal.executeLine();
                ipSearch = str;
                state = ISPDaemonState.Loading;
                timeEnteredLoadingScreen = os.timer;
                os.getStringCache = "";
            }
            var y3 = y2 + 26;
            if (!Button.doButton(35, x, y3, 300, 22, "Cancel", os.lockedColor))
                return;
            os.terminal.executeLine();
            state = ISPDaemonState.Welcome;
            os.getStringCache = "";
        }

        private void drawBackButton(Rectangle bounds, ISPDaemonState stateTo = ISPDaemonState.Welcome)
        {
            if (
                !Button.doButton(92271094, bounds.X + 30, bounds.Y + bounds.Height - 50, 200, 25, "Back", os.lockedColor))
                return;
            state = stateTo;
        }

        private void DrawAdminOnlyError(Rectangle bounds, SpriteBatch sb)
        {
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 20), "Insufficient Permissions", GuiData.font,
                Color.White, bounds.Width, float.MaxValue);
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 70),
                "IP Search is limited to administrators\nof this machine only.\nIf you require access, contact customer support.",
                GuiData.smallfont, Color.White, bounds.Width, float.MaxValue);
            drawBackButton(bounds, ISPDaemonState.Welcome);
        }

        private void DrawNotFoundError(Rectangle bounds, SpriteBatch sb)
        {
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 20), "IP Not Found", GuiData.font, Color.White,
                bounds.Width, float.MaxValue);
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 70),
                "IP Address is not registered with\nour servers.\nCheck address and try again.", GuiData.smallfont,
                Color.White, bounds.Width, float.MaxValue);
            drawBackButton(bounds, ISPDaemonState.Welcome);
        }

        private void DrawAboutScreen(Rectangle bounds, SpriteBatch sb)
        {
            TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 20), "About this server", GuiData.font,
                Color.White, bounds.Width, float.MaxValue);
            var folder = comp.files.root.searchForFolder("home");
            if (folder != null)
            {
                var fileEntry = folder.searchForFile("ISP_About_Message.txt");
                if (fileEntry != null)
                {
                    TextItem.DrawShadow = false;
                    TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + 70), fileEntry.data, GuiData.smallfont,
                        Color.White, bounds.Width, float.MaxValue);
                }
            }
            drawBackButton(bounds, ISPDaemonState.Welcome);
        }

        private void DrawWelcomeScreen(Rectangle bounds, SpriteBatch sb, Rectangle fullBounds)
        {
            TextItem.doFontLabel(new Vector2(bounds.X + 20, bounds.Y + 20), "ISP Management System", GuiData.font,
                Color.White, bounds.Width, float.MaxValue);
            var height = 30;
            var width = (int) (bounds.Width*0.800000011920929);
            var num = bounds.Y + 90;
            var destinationRectangle = fullBounds;
            destinationRectangle.Y = num - 20;
            destinationRectangle.Height = 22;
            var flag = comp.adminIP == os.thisComputer.ip;
            sb.Draw(Utils.white, destinationRectangle, flag ? os.unlockedColor : os.lockedColor);
            var text = "Valid Administrator Account Detected";
            if (!flag)
                text = "Non-Admin Account Active";
            var vector2 = GuiData.smallfont.MeasureString(text);
            var position = new Vector2(destinationRectangle.X + destinationRectangle.Width/2 - vector2.X/2f,
                destinationRectangle.Y);
            sb.DrawString(GuiData.smallfont, text, position, Color.White);
            var y1 = num + 30;
            if (Button.doButton(95371001, bounds.X + 20, y1, width, height, "About", os.highlightColor))
                state = ISPDaemonState.About;
            var y2 = y1 + (height + 10);
            if (Button.doButton(95371004, bounds.X + 20, y2, width, height, "Search for IP", os.highlightColor))
            {
                if (comp.adminIP == os.thisComputer.ip)
                {
                    state = ISPDaemonState.EnterIP;
                    os.execute("getString IP_Address");
                    ipSearch = null;
                }
                else
                    state = ISPDaemonState.AdminOnlyError;
            }
            var y3 = y2 + (height + 10);
            if (!Button.doButton(95371008, bounds.X + 20, y3, width, height, "Exit", os.highlightColor))
                return;
            os.display.command = "connect";
        }

        private void drawOutlineEffect(Rectangle bounds, SpriteBatch sb)
        {
            if (lastTimer == 0.0)
                lastTimer = os.timer;
            if (os.timer%0.5 > 0.100000001490116 && lastTimer%0.5 < 0.100000001490116)
                addNewOutlineEffect();
            for (var index = 0; index < outlineEffectEntries.Count; ++index)
            {
                var expandingRectangleData = outlineEffectEntries[index];
                expandingRectangleData.scaleIn +=
                    (float) ((os.timer - (double) lastTimer)*expandingRectangleData.rate*1.0);
                if (expandingRectangleData.scaleIn > 30.0)
                {
                    outlineEffectEntries.RemoveAt(index);
                    --index;
                }
                else
                {
                    drawRect(
                        new Rectangle((int) (bounds.X + (double) expandingRectangleData.scaleIn),
                            (int) (bounds.Y + (double) expandingRectangleData.scaleIn),
                            (int) (bounds.Width - 2.0*expandingRectangleData.scaleIn),
                            (int) (bounds.Height - 2.0*expandingRectangleData.scaleIn)), sb,
                        (int) expandingRectangleData.thickness, (float) (1.0 - expandingRectangleData.scaleIn/30.0),
                        expandingRectangleData.blackLerp);
                    outlineEffectEntries[index] = expandingRectangleData;
                }
            }
            drawRect(bounds, sb, 4, 1f, 0.0f);
            lastTimer = os.timer;
        }

        private void drawRect(Rectangle rect, SpriteBatch sb, int thickness, float opacity, float blackLerp)
        {
            var destinationRectangle = rect;
            destinationRectangle.Width = thickness;
            var color = Color.Lerp(os.highlightColor, Color.Black, blackLerp)*opacity;
            sb.Draw(Utils.white, destinationRectangle, color);
            destinationRectangle.X += rect.Width - thickness;
            sb.Draw(Utils.white, destinationRectangle, color);
            destinationRectangle.X = rect.X + thickness;
            destinationRectangle.Width = rect.Width - 2*thickness;
            destinationRectangle.Height = thickness;
            sb.Draw(Utils.white, destinationRectangle, color);
            destinationRectangle.Y += rect.Height - thickness;
            sb.Draw(Utils.white, destinationRectangle, color);
        }

        private void addNewOutlineEffect()
        {
            outlineEffectEntries.Add(new ExpandingRectangleData
            {
                rate = (float) (Utils.randm(1f)*15.0 + 1.0),
                scaleIn = 1f,
                thickness = (float) (1.0 + Utils.randm(1f)*(double) Utils.randm(1f)*6.0),
                blackLerp = Utils.randm(0.8f)
            });
        }

        private struct ExpandingRectangleData
        {
            public float scaleIn;
            public float rate;
            public float thickness;
            public float blackLerp;
        }

        private enum ISPDaemonState
        {
            Welcome,
            About,
            Loading,
            IPEntry,
            EnterIP,
            AdminOnlyError,
            NotFoundError
        }
    }
}
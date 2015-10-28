using System;
using System.Collections.Generic;
using System.Text;
using Hacknet.Effects;
using Hacknet.Gui;
using Hacknet.Modules.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class DisplayModule : CoreModule
    {
        private const int MAX_DISPLAY_STRING_LENGTH = 6000;
        private Vector2 catScroll;
        public string command = "";
        public string[] commandArgs;
        private Dictionary<string, Texture2D> compAltIcons;
        private List<Texture2D> computers;
        private Texture2D defaultComputer;
        private int errorCount;
        private Texture2D fancyCornerSprite;
        private Texture2D fancyPanelSprite;
        private bool hasSentErrorEmail;
        private float invioabilityCharChangeTimer;
        private string invioableSecurityCacheString;
        private Texture2D lockSprite;
        private readonly DisplayModuleLSHelper lsModuleHelper = new DisplayModuleLSHelper();
        private Texture2D openLockSprite;
        private Vector2 scroll;
        private Rectangle tmpRect;
        private int x;
        private int y;

        public DisplayModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
        }

        public override void LoadContent()
        {
            computers = new List<Texture2D>();
            computers.Add(TextureBank.load("Sprites/CompLogos/Sec0Computer", os.content));
            computers.Add(TextureBank.load("Sprites/CompLogos/Sec1Computer", os.content));
            computers.Add(TextureBank.load("Sprites/CompLogos/Computer", os.content));
            computers.Add(TextureBank.load("Sprites/CompLogos/OldServer", os.content));
            computers.Add(TextureBank.load("Sprites/CompLogos/Sec2Computer", os.content));
            computers.Add(TextureBank.load("Sprites/CompLogos/Sec2Computer", os.content));
            compAltIcons = new Dictionary<string, Texture2D>();
            compAltIcons.Add("laptop", TextureBank.load("Sprites/CompLogos/Laptop", os.content));
            compAltIcons.Add("chip", TextureBank.load("Sprites/CompLogos/chip", os.content));
            compAltIcons.Add("kellis", TextureBank.load("Sprites/CompLogos/KellisCompIcon", os.content));
            compAltIcons.Add("tablet", TextureBank.load("Sprites/CompLogos/Tablet", os.content));
            compAltIcons.Add("ePhone", TextureBank.load("Sprites/CompLogos/Phone1", os.content));
            compAltIcons.Add("ePhone2", TextureBank.load("Sprites/CompLogos/Phone2", os.content));
            defaultComputer = TextureBank.load("Sprites/CompLogos/Computer", os.content);
            lockSprite = TextureBank.load("Lock", os.content);
            openLockSprite = TextureBank.load("OpenLock", os.content);
            fancyCornerSprite = TextureBank.load("Corner", os.content);
            fancyPanelSprite = TextureBank.load("Panel", os.content);
            tmpRect = new Rectangle();
            scroll = Vector2.Zero;
            catScroll = Vector2.Zero;
            WebRenderer.setSize(bounds.Width, bounds.Height);
        }

        public override void Update(float t)
        {
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            try
            {
                doCommandModule();
                errorCount = 0;
            }
            catch (Exception ex)
            {
                ++errorCount;
                if (errorCount >= 3)
                {
                    if (!hasSentErrorEmail)
                    {
                        Utils.SendThreadedErrorReport(ex, "Display module Crash: " + command, DebugLog.GetDump());
                        hasSentErrorEmail = true;
                    }
                    command = "connect";
                    commandArgs = new string[1];
                    commandArgs[0] = "connect";
                    errorCount = 0;
                }
                else
                    DebugLog.add(Utils.GenerateReportFromException(ex));
            }
        }

        public void typeChanged()
        {
            scroll = Vector2.Zero;
            catScroll = Vector2.Zero;
            if (os.connectedComp != null)
            {
                var computer1 = os.connectedComp;
            }
            else
            {
                var computer2 = os.thisComputer;
            }
        }

        private void doCommandModule()
        {
            x = bounds.X + 5;
            y = bounds.Y + 5;
            spriteBatch.Draw(Utils.white, bounds, os.displayModuleExtraLayerBackingColor);
            if (os.connectedComp != null &&
                os.connectedComp.getDaemon(typeof (PorthackHeartDaemon)) is PorthackHeartDaemon)
                doDaemonDisplay();
            else if (command.Equals("ls") || command.Equals("dir") || command.Equals("cd"))
                doLsDisplay();
            else if (command.Equals("connect"))
            {
                doConnectDisplay();
                invioableSecurityCacheString = null;
            }
            else if (command.Equals("cat") || command.Equals("less"))
                docatDisplay();
            else if (command.Equals("probe") || command.Equals("nmap"))
                doProbeDisplay();
            else if (command.Equals("dc") || command.Equals("crash"))
                doDisconnectDisplay();
            else if (command.Equals("login"))
                doLoginDisplay();
            else
                doDaemonDisplay();
        }

        private void doDaemonDisplay()
        {
            var computer = os.connectedComp == null ? os.thisComputer : os.connectedComp;
            for (var index = 0; index < computer.daemons.Count; ++index)
            {
                if (computer.daemons[index].name.Equals(command) || computer.daemons[index] is PorthackHeartDaemon)
                {
                    computer.daemons[index].draw(bounds, spriteBatch);
                    break;
                }
            }
        }

        private void doLoginDisplay()
        {
            var strArray = os.displayCache.Split(new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            }, StringSplitOptions.None);
            var text1 = "";
            var text2 = "";
            var num1 = -1;
            var num2 = 0;
            if (strArray[0].Equals("loginData"))
            {
                text1 = !(strArray[1] != "") ? os.terminal.currentLine : strArray[1];
                if (strArray.Length > 2)
                {
                    num2 = 1;
                    text2 = strArray[2];
                    if (text2.Equals(""))
                    {
                        for (var index = 0; index < os.terminal.currentLine.Length; ++index)
                            text2 += "*";
                    }
                    else
                    {
                        var str = "";
                        for (var index = 0; index < text2.Length; ++index)
                            str += "*";
                        text2 = str;
                    }
                }
                if (strArray.Length > 3)
                {
                    num2 = 2;
                    num1 = Convert.ToInt32(strArray[3]);
                }
            }
            doConnectHeader();
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = bounds.X + 2;
            destinationRectangle.Y = this.y;
            destinationRectangle.Height = 200;
            destinationRectangle.Width = bounds.Width - 4;
            spriteBatch.Draw(Utils.white, destinationRectangle, num1 == 0 ? os.lockedColor : os.indentBackgroundColor);
            destinationRectangle.Height = 22;
            this.y += 30;
            var vector2 = TextItem.doMeasuredLabel(new Vector2(this.x, this.y), "Login ", Color.White);
            if (num1 == 0)
            {
                x += (int) vector2.X;
                TextItem.doLabel(new Vector2(x, y), "Failed", os.brightLockedColor);
                x -= (int) vector2.X;
            }
            else if (num1 != -1)
            {
                x += (int) vector2.X;
                TextItem.doLabel(new Vector2(x, y), "Successful", os.brightUnlockedColor);
                x -= (int) vector2.X;
            }
            this.y += 60;
            if (num2 == 0)
            {
                destinationRectangle.Y = y;
                spriteBatch.Draw(Utils.white, destinationRectangle, os.subtleTextColor);
            }
            spriteBatch.DrawString(GuiData.smallfont, "username :", new Vector2(this.x, this.y), Color.White);
            this.x += 100;
            spriteBatch.DrawString(GuiData.smallfont, text1, new Vector2(this.x, this.y), Color.White);
            this.x -= 100;
            this.y += 30;
            if (num2 == 1)
            {
                destinationRectangle.Y = y;
                spriteBatch.Draw(Utils.white, destinationRectangle, os.subtleTextColor);
            }
            spriteBatch.DrawString(GuiData.smallfont, "password :", new Vector2(this.x, this.y), Color.White);
            this.x += 100;
            spriteBatch.DrawString(GuiData.smallfont, text2, new Vector2(this.x, this.y), Color.White);
            this.y += 30;
            this.x -= 100;
            if (num1 != -1)
            {
                if (Button.doButton(12345, x, y, num1 > 0 ? 140 : 70, 30, num1 > 0 ? "Complete" : "Back",
                    os.indentBackgroundColor))
                    command = "connect";
                if (num1 > 0 || !Button.doButton(123456, x + 75, y, 70, 30, "Retry", os.indentBackgroundColor))
                    return;
                os.runCommand("login");
            }
            else
            {
                this.y += 65;
                var x = this.x;
                var y = this.y;
                var computer = os.connectedComp == null ? os.thisComputer : os.connectedComp;
                for (var index = 0; index < computer.users.Count; ++index)
                {
                    if (computer.users[index].known && Daemon.validUser(computer.users[index].type))
                    {
                        x = this.x + 320;
                        if (Button.doButton(123457 + index, this.x, this.y, 300, 25,
                            "Login - User: " + computer.users[index].name + " Pass: " + computer.users[index].pass,
                            os.darkBackgroundColor))
                            forceLogin(computer.users[index].name, computer.users[index].pass);
                        this.y += 27;
                    }
                }
                if (!Button.doButton(2111844, x, y, 100, 25, "Cancel", os.lockedColor))
                    return;
                forceLogin("", "");
                command = "connect";
            }
        }

        public void forceLogin(string username, string pass)
        {
            var str = os.terminal.prompt;
            os.terminal.currentLine = username;
            os.terminal.executeLine();
            do
                ; while (os.terminal.prompt.Equals(str));
            os.terminal.currentLine = pass;
            os.terminal.executeLine();
        }

        public Texture2D GetComputerImage(Computer comp)
        {
            if (comp.icon != null && compAltIcons.ContainsKey(comp.icon))
                return compAltIcons[comp.icon];
            if (comp.securityLevel < computers.Count)
                return computers[comp.securityLevel];
            return defaultComputer;
        }

        private void doConnectHeader()
        {
            var comp = os.connectedComp != null ? os.connectedComp : os.thisComputer;
            x += 20;
            y += 5;
            spriteBatch.Draw(GetComputerImage(comp), new Vector2(x, y), Color.White);
            spriteBatch.DrawString(GuiData.font, "Connected to ", new Vector2(x + 160, y), Color.White);
            y += 40;
            TextItem.doFontLabel(new Vector2(x + 160, y),
                os.connectedComp == null ? os.thisComputer.name : os.connectedComp.name, GuiData.font, Color.White,
                bounds.Width - 190f, 60f);
            y += 33;
            var str = os.connectedComp == null ? os.thisComputer.ip : os.connectedComp.ip;
            spriteBatch.DrawString(GuiData.smallfont, "@  " + str, new Vector2(x + 160, y), Color.White);
            y += 60;
            if (os.hasConnectionPermission(true))
            {
                y -= 20;
                var empty = Rectangle.Empty;
                empty.X = bounds.X + 1;
                empty.Y = y;
                empty.Width = bounds.Width - 2;
                empty.Height = 20;
                spriteBatch.Draw(Utils.white, empty, os.highlightColor);
                var text = "You are the Administrator of this System";
                var vector2 = GuiData.UISmallfont.MeasureString(text);
                var pos = new Vector2(empty.X + empty.Width/2 - vector2.X/2f, empty.Y);
                os.postFXDrawActions += () => spriteBatch.DrawString(GuiData.UISmallfont, text, pos, Color.Black);
                if (bounds.Height > 500)
                    y += 40;
                else
                    y += 12;
            }
            if (comp.portsNeededForCrack > 100)
            {
                y += 10;
                var rectangle = new Rectangle(bounds.X + 2, y, bounds.Width - 4, 20);
                var text = "INVIOABILITY DETECTED";
                var vector2 = GuiData.titlefont.MeasureString(text);
                var num1 = vector2.X/vector2.Y;
                var num2 = 10;
                var width = (int) (rectangle.Height*(double) num1);
                vector2.Y /= vector2.Y/20f;
                var num3 = (int) ((rectangle.Width - width)/2.0);
                var destinationRectangle = new Rectangle(rectangle.X, rectangle.Y, num3 - num2, rectangle.Height);
                var dest = new Rectangle(destinationRectangle.X + destinationRectangle.Width + num2,
                    destinationRectangle.Y, width, rectangle.Height);
                destinationRectangle.Y += 4;
                destinationRectangle.Height -= 8;
                spriteBatch.Draw(Utils.white, destinationRectangle, Color.Red);
                FlickeringTextEffect.DrawLinedFlickeringText(dest, text, 3f, 0.1f, GuiData.titlefont, os,
                    Utils.AddativeWhite, 2);
                destinationRectangle.X = rectangle.X + rectangle.Width - (destinationRectangle.Width + num2) + num2;
                spriteBatch.Draw(Utils.white, destinationRectangle, Color.Red);
                y += 40;
            }
            else
            {
                if (!os.hasConnectionPermission(true))
                    return;
                y += 40;
            }
        }

        private void doConnectDisplay()
        {
            doConnectHeader();
            var computer = os.connectedComp == null ? os.thisComputer : os.connectedComp;
            var num1 = computer.daemons.Count + 6;
            var height = 40;
            var num2 = bounds.Height - (y - bounds.Y) - 20 - num1*5;
            if (num2/(double) num1 < height)
                height = (int) (num2/(double) num1);
            for (var index = 0; index < computer.daemons.Count; ++index)
            {
                if (Button.doButton(29000 + index, x, y, 300, height, computer.daemons[index].name, os.highlightColor))
                {
                    command = computer.daemons[index].name;
                    computer.daemons[index].navigatedTo();
                }
                y += height + 5;
            }
            if (Button.doButton(300000, x, y, 300, height, "Login",
                computer.adminIP == os.thisComputer.adminIP ? os.subtleTextColor : os.highlightColor))
            {
                os.runCommand("login");
                os.terminal.clearCurrentLine();
            }
            y += height + 5;
            if (Button.doButton(300002, x, y, 300, height, "Probe System", os.highlightColor))
                os.runCommand("probe");
            y += height + 5;
            if (Button.doButton(300003, x, y, 300, height, "View Filesystem",
                os.hasConnectionPermission(false) ? os.highlightColor : os.subtleTextColor))
                os.runCommand("ls");
            y += height + 5;
            if (Button.doButton(300006, x, y, 300, height, "View Logs",
                os.hasConnectionPermission(false) ? os.highlightColor : os.subtleTextColor))
                os.runCommand("cd log");
            y += height + 5;
            if (Button.doButton(300009, x, y, 300, height, "Scan Network",
                os.hasConnectionPermission(true) ? os.highlightColor : os.subtleTextColor))
                os.runCommand("scan");
            y = bounds.Height;
            if (!Button.doButton(300012, x, y, 300, 20, "Disconnect", os.lockedColor))
                return;
            os.runCommand("dc");
        }

        private void doDisconnectDisplay()
        {
            tmpRect.X = bounds.X + 2;
            tmpRect.Width = bounds.Width - 4;
            tmpRect.Y = bounds.Y + bounds.Height/6*2;
            tmpRect.Height = bounds.Height/3;
            spriteBatch.Draw(Utils.white, tmpRect, os.indentBackgroundColor);
            var position = new Vector2(tmpRect.X + bounds.Width/2 - GuiData.font.MeasureString("Disconnected").X/2f,
                bounds.Y + bounds.Height/2 - 10);
            spriteBatch.DrawString(GuiData.font, "Disconnected", position, os.subtleTextColor);
        }

        private void docatDisplay()
        {
            if (os.hasConnectionPermission(false))
            {
                if (Button.doButton(299999, bounds.X + (bounds.Width - 41), bounds.Y + 12, 27, 29, "<-", new Color?()))
                    os.runCommand("ls");
                var rectangle = GuiData.tmpRect;
                rectangle.Width = bounds.Width;
                rectangle.X = bounds.X;
                rectangle.Y = bounds.Y + 1;
                rectangle.Height = bounds.Height - 2;
                rectangle.Height += bounds.Height;
                ScrollablePanel.beginPanel(484732, rectangle, catScroll);
                var text1 = "";
                for (var index = 1; index < commandArgs.Length; ++index)
                    text1 = text1 + commandArgs[index] + " ";
                x = 10;
                y = 5;
                try
                {
                    TextItem.doFontLabel(new Vector2(x - 5, y - 1), text1, GuiData.font, Color.White, bounds.Width - 70,
                        float.MaxValue);
                    var data = os.displayCache;
                    if (data.Length > 6000)
                        data = data.Substring(0, 6000) + "\n -- FILE TOO LONG TO DISPLAY -- ";
                    y += 70;
                    var text2 = Utils.SuperSmartTwimForWidth(data, bounds.Width - 40, GuiData.tinyfont);
                    GuiData.spriteBatch.DrawString(GuiData.tinyfont, text2, new Vector2(x, y), Color.White);
                }
                catch (Exception ex)
                {
                    commandArgs = new string[3];
                    commandArgs[0] = "cat";
                    commandArgs[1] = "Error";
                    commandArgs[2] = "Error reading file";
                    command = "cat error error reading file";
                }
                rectangle.Height -= bounds.Height;
                catScroll = ScrollablePanel.endPanel(484732, catScroll, rectangle, 1000f, false);
            }
            else
                command = "connect";
        }

        private void doProbeDisplay()
        {
            var rectangle = Rectangle.Empty;
            var computer = os.connectedComp == null ? os.thisComputer : os.connectedComp;
            if (computer.proxyActive)
            {
                rectangle = bounds;
                ++rectangle.X;
                ++rectangle.Y;
                rectangle.Width -= 2;
                rectangle.Height -= 2;
                PatternDrawer.draw(rectangle, 0.8f, Color.Transparent, os.superLightWhite, os.ScreenManager.SpriteBatch);
            }
            if (Button.doButton(299999, bounds.X + (bounds.Width - 50), bounds.Y + y, 27, 27, "<-", new Color?()))
                command = "connect";
            spriteBatch.DrawString(GuiData.font, "Open Ports", new Vector2(x, y), Color.White);
            y += 40;
            spriteBatch.DrawString(GuiData.smallfont, computer.name + " @" + computer.ip, new Vector2(x, y), Color.White);
            y += 30;
            var num = Math.Max(computer.portsNeededForCrack + 1, 0);
            var str = string.Concat(num);
            var flag1 = num > 100;
            if (flag1)
            {
                if (invioableSecurityCacheString == null)
                {
                    var stringBuilder = new StringBuilder();
                    for (var index = 0; index < str.Length; ++index)
                        stringBuilder.Append(Utils.getRandomChar());
                    invioableSecurityCacheString = stringBuilder.ToString();
                }
                else
                {
                    invioabilityCharChangeTimer -= (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
                    if (invioabilityCharChangeTimer <= 0.0)
                    {
                        var stringBuilder = new StringBuilder(invioableSecurityCacheString);
                        stringBuilder[Utils.random.Next(stringBuilder.Length)] = Utils.random.NextDouble() > 0.3
                            ? Utils.getRandomNumberChar()
                            : Utils.getRandomChar();
                        invioableSecurityCacheString = stringBuilder.ToString();
                        invioabilityCharChangeTimer = 0.025f;
                    }
                }
                str = invioableSecurityCacheString;
            }
            spriteBatch.DrawString(GuiData.smallfont, "Open Ports Required for Crack: " + str, new Vector2(x, y),
                flag1 ? Color.Lerp(Color.Red, os.brightLockedColor, Utils.randm(0.5f) + 0.5f) : os.highlightColor);
            y += 40;
            if (flag1)
            {
                rectangle.X = bounds.X + 2;
                rectangle.Y = y;
                rectangle.Width = bounds.Width - 4;
                rectangle.Height = 110;
                DrawInvioabilityEffect(rectangle);
                y += rectangle.Height + 10;
            }
            if (computer.hasProxy)
            {
                rectangle.X = x;
                rectangle.Y = y;
                rectangle.Width = bounds.Width - 10;
                rectangle.Height = 40;
                PatternDrawer.draw(rectangle, 1f,
                    computer.proxyActive ? os.topBarColor : Color.Lerp(os.unlockedColor, Color.Black, 0.2f),
                    computer.proxyActive ? os.shellColor*0.3f : os.unlockedColor, os.ScreenManager.SpriteBatch);
                if (computer.proxyActive)
                {
                    rectangle.Width =
                        (int)
                            (rectangle.Width*(1.0 - computer.proxyOverloadTicks/(double) computer.startingOverloadTicks));
                    spriteBatch.Draw(Utils.white, rectangle, Color.Black*0.5f);
                }
                spriteBatch.DrawString(GuiData.smallfont, computer.proxyActive ? "Proxy Detected" : "Proxy Bypassed",
                    new Vector2(x + 4, y + 2), Color.Black);
                spriteBatch.DrawString(GuiData.smallfont, computer.proxyActive ? "Proxy Detected" : "Proxy Bypassed",
                    new Vector2(x + 3, y + 1), computer.proxyActive ? Color.White : os.highlightColor);
                y += 60;
            }
            if (computer.firewall != null)
            {
                rectangle.X = x;
                rectangle.Y = y;
                rectangle.Width = bounds.Width - 10;
                rectangle.Height = 40;
                var flag2 = !computer.firewall.solved;
                PatternDrawer.draw(rectangle, 1f,
                    flag2 ? os.topBarColor : Color.Lerp(os.unlockedColor, Color.Black, 0.2f),
                    flag2 ? os.shellColor*0.3f : os.unlockedColor, os.ScreenManager.SpriteBatch);
                spriteBatch.DrawString(GuiData.smallfont, flag2 ? "Firewall Detected" : "Firewall Solved",
                    new Vector2(x + 4, y + 2), Color.Black);
                spriteBatch.DrawString(GuiData.smallfont, flag2 ? "Firewall Detected" : "Firewall Solved",
                    new Vector2(x + 3, y + 1), flag2 ? Color.White : os.highlightColor);
                y += 60;
            }
            var zero = Vector2.Zero;
            rectangle.X = x + 1;
            rectangle.Width = 420;
            rectangle.Height = 41;
            var position = new Vector2(rectangle.X + rectangle.Width - 36, rectangle.Y + 4);
            x += 10;
            for (var index = 0; index < computer.ports.Count; ++index)
            {
                rectangle.Y = y + 4;
                position.Y = rectangle.Y + 2;
                spriteBatch.Draw(Utils.white, rectangle,
                    computer.portsOpen[index] > 0 ? os.unlockedColor : os.lockedColor);
                spriteBatch.Draw(computer.portsOpen[index] > 0 ? openLockSprite : lockSprite, position, Color.White);
                var text = "Port#: " + computer.ports[index];
                var vector2 = GuiData.font.MeasureString(text);
                spriteBatch.DrawString(GuiData.font, text, new Vector2(x, y + 3), Color.White);
                spriteBatch.DrawString(GuiData.smallfont, " - " + PortExploits.services[computer.ports[index]],
                    new Vector2(x + vector2.X, y + 4), Color.White);
                y += 45;
            }
        }

        private void DrawInvioabilityEffect(Rectangle dest)
        {
            var color = Color.Lerp(os.lockedColor, os.brightLockedColor, Utils.randm(0.5f) + 0.5f);
            color.A = 0;
            var destinationRectangle1 = new Rectangle(dest.X + fancyCornerSprite.Width, dest.Y,
                dest.Width - fancyCornerSprite.Width*2, fancyPanelSprite.Height);
            spriteBatch.Draw(fancyPanelSprite, destinationRectangle1, color);
            spriteBatch.Draw(fancyCornerSprite, new Vector2(dest.X, dest.Y), new Rectangle?(), color, 0.0f, Vector2.Zero,
                1f, SpriteEffects.FlipHorizontally, 0.4f);
            spriteBatch.Draw(fancyCornerSprite,
                new Vector2(destinationRectangle1.X + destinationRectangle1.Width, dest.Y), color);
            var num1 = fancyCornerSprite.Width - 38;
            var dest1 = new Rectangle(dest.X + num1, dest.Y + fancyCornerSprite.Height/2, dest.Width - num1*2,
                dest.Height - fancyCornerSprite.Height);
            Color.Lerp(Utils.AddativeWhite, os.brightLockedColor, Utils.randm(0.5f) + 0.5f);
            FlickeringTextEffect.DrawLinedFlickeringText(dest1, "INVIOABILITY ERROR", 4f, 0.26f, GuiData.titlefont, os,
                Utils.AddativeWhite, 2);
            var destinationRectangle2 = destinationRectangle1;
            destinationRectangle2.Y = dest.Y + dest.Height - fancyPanelSprite.Height;
            spriteBatch.Draw(fancyPanelSprite, destinationRectangle2, new Rectangle?(), color, 0.0f, Vector2.Zero,
                SpriteEffects.FlipVertically, 0.5f);
            float num2 = fancyCornerSprite.Height - fancyPanelSprite.Height;
            spriteBatch.Draw(fancyCornerSprite, new Vector2(dest.X, destinationRectangle2.Y - num2), new Rectangle?(),
                color, 0.0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0.4f);
            spriteBatch.Draw(fancyCornerSprite,
                new Vector2(destinationRectangle1.X + destinationRectangle1.Width, destinationRectangle2.Y - num2),
                new Rectangle?(), color, 0.0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0.5f);
        }

        private void doLsDisplay()
        {
            if (os.hasConnectionPermission(false))
            {
                x = 5;
                y = 5;
                var num = bounds.Width;
                TextItem.doFontLabel(new Vector2(bounds.X + x, bounds.Y + y),
                    os.connectedComp == null ? os.thisComputer.name : os.connectedComp.name + " File System",
                    GuiData.font, Color.White, bounds.Width - 46f, 60f);
                if (Button.doButton(299999, bounds.X + (bounds.Width - 41), bounds.Y + 12, 27, 29, "<-", new Color?()))
                {
                    if (os.navigationPath.Count > 0)
                        os.runCommand("cd ..");
                    else
                        os.display.command = "connect";
                }
                y += 50;
                var dest = GuiData.tmpRect;
                dest.Width = bounds.Width;
                dest.X = bounds.X;
                dest.Y = bounds.Y + 55;
                dest.Height = bounds.Height - 57;
                lsModuleHelper.DrawUI(dest, os);
            }
            else
                command = "connect";
        }

        private void doFolderGui(int width, int height, int indexOffset, Folder f, int recItteration)
        {
            for (var i = 0; i < f.folders.Count; ++i)
            {
                if (Button.doButton(300000 + i + indexOffset, x, y, width, height, "/" + f.folders[i].name, new Color?()))
                {
                    var num = 0;
                    for (var index = 0; index < os.navigationPath.Count - recItteration; ++index)
                    {
                        Action action = () => os.runCommand("cd ..");
                        if (num > 0)
                            os.delayer.Post(ActionDelayer.Wait(num*1.0), action);
                        else
                            action();
                        ++num;
                    }
                    Action action1 = () => os.runCommand("cd " + f.folders[i].name);
                    if (num > 0)
                        os.delayer.Post(ActionDelayer.Wait(num*1.0), action1);
                    else
                        action1();
                }
                y += height + 2;
                x += 30;
                if (os.navigationPath.Count - 1 >= recItteration && os.navigationPath[recItteration] == i)
                    doFolderGui(width - 30, height, indexOffset + 10000*(i + 1), f.folders[i], recItteration + 1);
                x -= 30;
            }
            for (var index1 = 0; index1 < f.files.Count; ++index1)
            {
                if (Button.doButton(400000 + index1 + indexOffset/2 + (index1 + 1)*indexOffset, x, y, width, height,
                    f.files[index1].name, new Color?()))
                {
                    for (var index2 = 0; index2 < os.navigationPath.Count - recItteration; ++index2)
                        os.runCommand("cd ..");
                    os.runCommand("cat " + f.files[index1].name);
                }
                y += height + 2;
            }
            if (f.folders.Count != 0 || f.files.Count != 0)
                return;
            TextItem.doFontLabel(new Vector2(x, y), "-Empty-", GuiData.tinyfont, new Color?(), width, height);
            y += height + 2;
        }

        public static string splitForWidth(string s, int width)
        {
            var str = "";
            var num = 0;
            foreach (var ch in s)
            {
                str += ch;
                if (ch != 10)
                    ++num;
                else
                    num = 0;
                if (num > width/6 && (ch == 32 || num > width/5.19999980926514))
                {
                    str += '\n';
                    num = 0;
                }
            }
            return str;
        }

        public static string splitForWidth(string s, int width, bool correct)
        {
            var str = "";
            var num = 0;
            width /= 8;
            foreach (var ch in s)
            {
                str += ch;
                if (ch != 10)
                    ++num;
                else
                    num = 0;
                if (num >= width && (ch == 32 || num > width*0.899999976158142))
                {
                    str += '\n';
                    num = 0;
                }
            }
            return str;
        }

        public static string cleanSplitForWidth(string s, int width)
        {
            var num1 = 10;
            width /= num1;
            var str = "";
            var chArray = new char[1]
            {
                ' '
            };
            var strArray = s.Split(chArray);
            var index1 = 0;
            if (strArray.Length == 1)
                return splitForWidth(strArray[0], width*8, true);
            while (index1 < strArray.Length)
            {
                for (var index2 = 0; index2 < width && index1 < strArray.Length; ++index1)
                {
                    str = str + strArray[index1] + " ";
                    index2 += strArray[index1].Length;
                    var num2 = strArray[index1].IndexOf('\n');
                    if (num2 >= 0)
                        index2 = strArray[index1].Length - (num2 + 1);
                }
                str += '\n';
            }
            return str;
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.AddEmailDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Linq;
using System.Net;
using System.Threading;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class AddEmailDaemon : Daemon
    {
        private const string SEND_SCRIPT_URL = "http://www.tijital-games.com/hacknet/SendVictoryEmail.php?mail=[EMAIL]";
        private const int WAITING = 0;
        private const int ENTERING = 1;
        private const int CONFIRM = 2;
        private const int ERROR = 3;
        private static string lastSentEmail;
        private string email;
        private int state;

        public AddEmailDaemon(Computer computer, string serviceName, OS opSystem)
            : base(computer, serviceName, opSystem)
        {
            state = 0;
            email = "";
        }

        private void saveEmail()
        {
            try
            {
                Utils.appendToFile(email + "\r\n", "Emails.txt");
                lastSentEmail = email;
                SFX.addCircle(
                    os.netMap.GetNodeDrawPos(comp.location) + new Vector2(os.netMap.bounds.X, os.netMap.bounds.Y) +
                    new Vector2(NetworkMap.NODE_SIZE/2), os.thisComputerNode, 100f);
                state = 0;
                os.display.command = "connect";
            }
            catch (Exception ex)
            {
                state = 3;
            }
        }

        public void sendEmail()
        {
            if (lastSentEmail != null && lastSentEmail.Equals(email))
                return;
            if (email != null && email.Contains('@'))
            {
                new Thread(makeWebRequest)
                {
                    IsBackground = true
                }.Start();
                lastSentEmail = email;
                SFX.addCircle(
                    comp.location + new Vector2(os.netMap.bounds.X, os.netMap.bounds.Y) +
                    new Vector2(NetworkMap.NODE_SIZE/2), os.thisComputerNode, 100f);
                state = 0;
                os.display.command = "connect";
            }
            else
                state = 3;
        }

        public override string getSaveString()
        {
            return "<AddEmailServer name=\"" + name + "\"/>";
        }

        public void makeWebRequest()
        {
            try
            {
                Console.WriteLine(
                    new WebClient().DownloadString(
                        "http://www.tijital-games.com/hacknet/SendVictoryEmail.php?mail=[EMAIL]".Replace("[EMAIL]",
                            email)));
            }
            catch (Exception ex)
            {
            }
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            var x = bounds.X + 10;
            var num1 = bounds.Y + 10;
            TextItem.doFontLabel(new Vector2(x, num1), "Email Verification", GuiData.font, new Color?(),
                bounds.Width - 20, bounds.Height);
            var num2 = num1 + 50;
            switch (state)
            {
                case 1:
                    var vector2_1 = TextItem.doMeasuredSmallLabel(new Vector2(x, num2),
                        "Enter a secure email address :\nEnsure that you are the only one with access to it",
                        new Color?());
                    var y1 = num2 + ((int) vector2_1.Y + 10);
                    var strArray = os.getStringCache.Split(new string[1]
                    {
                        "#$#$#$$#$&$#$#$#$#"
                    }, StringSplitOptions.None);
                    if (strArray.Length > 1)
                    {
                        email = strArray[1];
                        if (email.Equals(""))
                            email = os.terminal.currentLine;
                    }
                    var destinationRectangle = new Rectangle(x, y1, bounds.Width - 20, 200);
                    sb.Draw(Utils.white, destinationRectangle, os.darkBackgroundColor);
                    var num3 = y1 + 80;
                    var vector2_2 = TextItem.doMeasuredSmallLabel(new Vector2(x, num3), "Email: " + email, new Color?());
                    destinationRectangle.X = x + (int) vector2_2.X + 2;
                    destinationRectangle.Y = num3;
                    destinationRectangle.Width = 7;
                    destinationRectangle.Height = 20;
                    if (os.timer%1.0 < 0.300000011920929)
                        sb.Draw(Utils.white, destinationRectangle, os.outlineColor);
                    var y2 = num3 + 122;
                    if (strArray.Length <= 2 && !Button.doButton(30, x, y2, 300, 22, "Confirm", os.highlightColor))
                        break;
                    if (strArray.Length <= 2)
                        os.terminal.executeLine();
                    state = 2;
                    break;
                case 2:
                    var num4 = num2 + 20;
                    TextItem.doSmallLabel(new Vector2(x, num4), "Confirm this Email Address :\n" + email, new Color?());
                    var y3 = num4 + 60;
                    if (Button.doButton(21, x, y3, 200, 50, "Confirm Email", os.highlightColor))
                    {
                        if (!Settings.isDemoMode)
                            sendEmail();
                        else
                            saveEmail();
                    }
                    if (!Button.doButton(20, x + 220, y3, 200, 50, "Re-Enter Email", new Color?()))
                        break;
                    state = 1;
                    email = "";
                    os.execute("getString Email");
                    break;
                default:
                    var y4 = num2 + (bounds.Height/2 - 60);
                    if (state == 3)
                        TextItem.doSmallLabel(new Vector2(x, y4 - 60), "Error - Invalid Email Address", new Color?());
                    if (Button.doButton(10, x, y4, 300, 50, "Add Email", os.highlightColor))
                    {
                        state = 1;
                        os.execute("getString Email");
                    }
                    if (!Button.doButton(12, x, y4 + 55, 300, 20, "Exit", os.lockedColor))
                        break;
                    os.display.command = "connect";
                    break;
            }
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.AuthenticatingDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Threading;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class AuthenticatingDaemon : Daemon
    {
        public UserDetail user;

        public AuthenticatingDaemon(Computer computer, string serviceName, OS opSystem)
            : base(computer, serviceName, opSystem)
        {
        }

        public virtual void loginGoBack()
        {
        }

        public virtual void userLoggedIn()
        {
        }

        public void startLogin()
        {
            os.displayCache = "";
            os.execute("login");
            do
                ; while (os.displayCache.Equals(""));
            os.display.command = name;
        }

        public void doLoginDisplay(Rectangle bounds, SpriteBatch sb)
        {
            var num1 = bounds.X + 20;
            var num2 = bounds.Y + 100;
            var strArray = os.displayCache.Split(new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            }, StringSplitOptions.None);
            var text1 = "";
            var text2 = "";
            var num3 = -1;
            var num4 = 0;
            if (strArray[0].Equals("loginData"))
            {
                text1 = !(strArray[1] != "") ? os.terminal.currentLine : strArray[1];
                if (strArray.Length > 2)
                {
                    num4 = 1;
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
                    num4 = 2;
                    num3 = Convert.ToInt32(strArray[3]);
                }
            }
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = bounds.X + 2;
            destinationRectangle.Y = num2;
            destinationRectangle.Height = 200;
            destinationRectangle.Width = bounds.Width - 4;
            sb.Draw(Utils.white, destinationRectangle, num3 == 0 ? os.lockedColor : os.indentBackgroundColor);
            if (num3 != 0 && num3 != -1)
            {
                for (var index = 0; index < comp.users.Count; ++index)
                {
                    if (comp.users[index].name.Equals(text1))
                        user = comp.users[index];
                }
                userLoggedIn();
            }
            destinationRectangle.Height = 22;
            var num5 = num2 + 30;
            var vector2 = TextItem.doMeasuredLabel(new Vector2(num1, num5), "Login ", Color.White);
            if (num3 == 0)
            {
                var num6 = num1 + (int) vector2.X;
                TextItem.doLabel(new Vector2(num6, num5), "Failed", os.brightLockedColor);
                num1 = num6 - (int) vector2.X;
            }
            var num7 = num5 + 60;
            if (num4 == 0)
            {
                destinationRectangle.Y = num7;
                sb.Draw(Utils.white, destinationRectangle, os.subtleTextColor);
            }
            sb.DrawString(GuiData.smallfont, "username :", new Vector2(num1, num7), Color.White);
            var num8 = num1 + 100;
            sb.DrawString(GuiData.smallfont, text1, new Vector2(num8, num7), Color.White);
            var num9 = num8 - 100;
            var num10 = num7 + 30;
            if (num4 == 1)
            {
                destinationRectangle.Y = num10;
                sb.Draw(Utils.white, destinationRectangle, os.subtleTextColor);
            }
            sb.DrawString(GuiData.smallfont, "password :", new Vector2(num9, num10), Color.White);
            var num11 = num9 + 100;
            sb.DrawString(GuiData.smallfont, text2, new Vector2(num11, num10), Color.White);
            var y1 = num10 + 30;
            var x = num11 - 100;
            if (num3 != -1)
            {
                if (Button.doButton(12345, x, y1, 70, 30, "Back", os.indentBackgroundColor))
                    loginGoBack();
                if (!Button.doButton(123456, x + 75, y1, 70, 30, "Retry", os.indentBackgroundColor))
                    return;
                os.displayCache = "";
                os.execute("login");
                do
                    ; while (os.displayCache.Equals(""));
                os.display.command = name;
            }
            else
            {
                var y2 = y1 + 65;
                for (var index = 0; index < comp.users.Count; ++index)
                {
                    if (comp.users[index].known && validUser(comp.users[index].type))
                    {
                        if (Button.doButton(123457 + index, x, y2, 300, 25,
                            "User: " + comp.users[index].name + " Pass: " + comp.users[index].pass,
                            os.darkBackgroundColor))
                            forceLogin(comp.users[index].name, comp.users[index].pass);
                        y2 += 27;
                    }
                }
            }
        }

        public new static bool validUser(byte type)
        {
            if (!Daemon.validUser(type))
                return type == 3;
            return true;
        }

        public void forceLogin(string username, string pass)
        {
            var str = os.terminal.prompt;
            os.terminal.currentLine = username;
            os.terminal.NonThreadedInstantExecuteLine();
            while (os.terminal.prompt.Equals(str))
                Thread.Sleep(0);
            os.terminal.currentLine = pass;
            os.terminal.NonThreadedInstantExecuteLine();
        }
    }
}
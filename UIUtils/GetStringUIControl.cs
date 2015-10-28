// Decompiled with JetBrains decompiler
// Type: Hacknet.UIUtils.GetStringUIControl
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.UIUtils
{
    public static class GetStringUIControl
    {
        public static void StartGetString(string prompt, object os_obj)
        {
            ((OS) os_obj).execute("getString " + prompt);
        }

        public static string DrawGetStringControl(string prompt, Rectangle bounds, Action errorOccurs, Action cancelled,
            SpriteBatch sb, object os_obj, Color SearchButtonColor, Color CancelButtonColor, string upperPrompt = null,
            Color? BackingPanelColor = null)
        {
            var os = (OS) os_obj;
            upperPrompt = upperPrompt == null ? prompt : upperPrompt;
            var str = "";
            var x = bounds.X + 6;
            var y1 = bounds.Y + 2;
            var zero = Vector2.Zero;
            if (upperPrompt.Length > 0)
            {
                var vector2 = TextItem.doMeasuredSmallLabel(new Vector2(x, y1), upperPrompt, new Color?());
                y1 += (int) vector2.Y + 10;
            }
            var separator = new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            };
            var strArray = os.getStringCache.Split(separator, StringSplitOptions.None);
            if (strArray.Length > 1)
            {
                str = strArray[1];
                if (str.Equals(""))
                    str = os.terminal.currentLine;
            }
            var destinationRectangle = new Rectangle(x, y1, bounds.Width - 12, bounds.Height - 46);
            sb.Draw(Utils.white, destinationRectangle,
                BackingPanelColor.HasValue ? BackingPanelColor.Value : os.darkBackgroundColor);
            var num = y1 + 18;
            var vector2_1 = TextItem.doMeasuredSmallLabel(new Vector2(x, num), prompt + str, new Color?());
            destinationRectangle.X = x + (int) vector2_1.X + 2;
            destinationRectangle.Y = num;
            destinationRectangle.Width = 7;
            destinationRectangle.Height = 20;
            if (os.timer%1.0 < 0.300000011920929)
                sb.Draw(Utils.white, destinationRectangle, os.outlineColor);
            var y2 = num + (bounds.Height - 44);
            if (strArray.Length > 2 || Button.doButton(30, x, y2, 300, 22, "Search", SearchButtonColor))
            {
                if (strArray.Length <= 2)
                    os.terminal.executeLine();
                if (str.Length > 0)
                    return str;
                errorOccurs();
                return null;
            }
            if (Button.doButton(38, x, y2 + 24, 300, 22, "Cancel", CancelButtonColor))
            {
                cancelled();
                os.terminal.clearCurrentLine();
                os.terminal.executeLine();
            }
            return null;
        }

        public static void DrawGetStringControlInactive(string prompt, string valueText, Rectangle bounds,
            SpriteBatch sb, object os_obj, string upperPrompt = null)
        {
            var os = (OS) os_obj;
            upperPrompt = upperPrompt == null ? prompt : upperPrompt;
            var str = valueText;
            var x = bounds.X + 6;
            var num1 = bounds.Y + 2;
            var vector2_1 = TextItem.doMeasuredSmallLabel(new Vector2(x, num1), upperPrompt, new Color?());
            var y = num1 + ((int) vector2_1.Y + 10);
            var destinationRectangle = new Rectangle(x, y, bounds.Width - 12, bounds.Height - 46);
            sb.Draw(Utils.white, destinationRectangle, os.darkBackgroundColor);
            var num2 = y + 28;
            var vector2_2 = TextItem.doMeasuredSmallLabel(new Vector2(x, num2), prompt + str, new Color?());
            destinationRectangle.X = x + (int) vector2_2.X + 2;
            destinationRectangle.Y = num2;
            destinationRectangle.Width = 7;
            destinationRectangle.Height = 20;
            var num3 = num2 + (bounds.Height - 44);
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.SliderBar
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet.Gui
{
    public static class SliderBar
    {
        public const int SELECTOR_BAR_WIDTH = 8;
        public const int BOARDER = 2;
        public const int SLIDE_BAR_HEIGHT = 10;

        public static float doSliderBar(int myID, int x, int y, int width, int height, float maxValue, float minValue,
            float currentValue, float barStep)
        {
            var num1 = -1f;
            var val1 = currentValue;
            if (GuiData.hot == myID)
            {
                if (GuiData.isMouseLeftDown())
                    GuiData.active = myID;
                else if (GuiData.active == myID)
                    GuiData.active = -1;
                if (GuiData.active == myID)
                {
                    num1 = Math.Min(Math.Max((GuiData.mouse.X - x)/(float) width, 0.0f), 1f);
                    val1 = minValue + num1*(maxValue - minValue);
                }
                val1 += barStep*GuiData.getMouseWheelScroll();
            }
            if (num1 == -1.0)
            {
                var num2 = maxValue - minValue;
                num1 = currentValue/num2;
                if (minValue < 0.0)
                    num1 += 0.5f;
            }
            GuiData.tmpRect.X = x;
            GuiData.tmpRect.Y = y;
            GuiData.tmpRect.Width = width;
            GuiData.tmpRect.Height = height;
            if (GuiData.tmpRect.Contains(GuiData.getMousePoint()))
                GuiData.hot = myID;
            else if (GuiData.hot == myID && GuiData.mouse.LeftButton == ButtonState.Released)
            {
                GuiData.hot = -1;
                if (GuiData.active == myID)
                    GuiData.active = -1;
            }
            var num3 = Math.Min(Math.Max(val1, minValue), maxValue);
            GuiData.tmpRect.Width = width;
            GuiData.tmpRect.X = x;
            GuiData.tmpRect.Height = 10;
            GuiData.tmpRect.Y = y + height/4;
            GuiData.spriteBatch.Draw(Utils.white, GuiData.tmpRect, GuiData.Default_Backing_Color);
            GuiData.tmpRect.Width -= 4;
            GuiData.tmpRect.X += 2;
            GuiData.tmpRect.Height -= 4;
            GuiData.tmpRect.Y += 2;
            GuiData.spriteBatch.Draw(Utils.white, GuiData.tmpRect, GuiData.Default_Dark_Background_Color);
            GuiData.tmpRect.Width = 8;
            GuiData.tmpRect.X = (int) (x + num1*(double) width) - 4;
            GuiData.tmpRect.Y = y - 5;
            GuiData.tmpRect.Height = height;
            GuiData.spriteBatch.Draw(Utils.white, GuiData.tmpRect,
                GuiData.active != myID
                    ? (GuiData.hot == myID ? GuiData.Default_Lit_Backing_Color : GuiData.Default_Selected_Color)
                    : GuiData.Default_Unselected_Color);
            if (GuiData.active == myID || GuiData.hot == myID)
            {
                var format = "0.000";
                var text = num3.ToString(format) ?? "";
                text.TrimEnd('0');
                GuiData.temp.X = (int) (x + num1*(double) width) + 4;
                GuiData.temp.Y = y - GuiData.smallfont.MeasureString(text).Y*0.8f;
                GuiData.spriteBatch.DrawString(GuiData.smallfont, text, GuiData.temp + Vector2.One, Color.Gray, 0.0f,
                    Vector2.Zero, 0.8f, SpriteEffects.None, 0.51f);
                GuiData.spriteBatch.DrawString(GuiData.smallfont, text, GuiData.temp, Color.White, 0.0f, Vector2.Zero,
                    0.8f, SpriteEffects.None, 0.5f);
            }
            return num3;
        }
    }
}
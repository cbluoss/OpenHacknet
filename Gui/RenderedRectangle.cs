// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.RenderedRectangle
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;

namespace Hacknet.Gui
{
    public static class RenderedRectangle
    {
        public static void doRectangle(int x, int y, int width, int height, Color? color)
        {
            if (!color.HasValue)
                color = GuiData.Default_Backing_Color;
            if (width < 0)
            {
                x += width;
                width = Math.Abs(width);
            }
            if (height < 0)
            {
                y += height;
                height = Math.Abs(height);
            }
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Width = width;
            destinationRectangle.Y = y;
            destinationRectangle.Height = height;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, color.Value);
        }

        public static void doRectangle(int x, int y, int width, int height, Color? color, bool blocking)
        {
            doRectangle(x, y, width, height, color);
            var rectangle = GuiData.tmpRect;
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = width;
            rectangle.Height = height;
            if (!blocking || !rectangle.Contains(GuiData.getMousePoint()))
                return;
            GuiData.blockingInput = true;
        }

        public static void doRectangleOutline(int x, int y, int width, int height, int thickness, Color? color)
        {
            if (!color.HasValue)
                color = GuiData.Default_Backing_Color;
            if (width < 0)
            {
                x += width;
                width = Math.Abs(width);
            }
            if (height < 0)
            {
                y += height;
                height = Math.Abs(height);
            }
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Width = width;
            destinationRectangle.Y = y;
            destinationRectangle.Height = thickness;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, color.Value);
            destinationRectangle.X = x;
            destinationRectangle.Width = width;
            destinationRectangle.Y = y + height - thickness;
            destinationRectangle.Height = thickness;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, color.Value);
            destinationRectangle.X = x;
            destinationRectangle.Width = thickness;
            destinationRectangle.Y = y;
            destinationRectangle.Height = height;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, color.Value);
            destinationRectangle.X = x + width - thickness;
            destinationRectangle.Width = thickness;
            destinationRectangle.Y = y;
            destinationRectangle.Height = height;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, color.Value);
        }
    }
}
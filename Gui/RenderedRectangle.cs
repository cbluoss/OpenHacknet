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
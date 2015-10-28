// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.Button
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet.Gui
{
    public static class Button
    {
        public const int BORDER_WIDTH = 1;
        public static bool wasPressedDown;
        public static bool wasReleased;
        public static bool drawingOutline = true;
        public static bool outlineOnly = false;
        public static bool smallButtonDraw = false;
        public static bool DisableIfAnotherIsActive = false;

        public static bool doButton(int myID, int x, int y, int width, int height, string text, Color? selectedColor)
        {
            return doButton(myID, x, y, width, height, text, selectedColor, Utils.white);
        }

        public static bool doButton(int myID, int x, int y, int width, int height, string text, Color? selectedColor,
            Texture2D tex)
        {
            var flag = false;
            if (GuiData.hot == myID && !GuiData.blockingInput && GuiData.active == myID &&
                (GuiData.mouseLeftUp() || GuiData.mouse.LeftButton == ButtonState.Released))
            {
                flag = true;
                GuiData.active = -1;
            }
            var rectangle = GuiData.tmpRect;
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = width;
            rectangle.Height = height;
            if (rectangle.Contains(GuiData.getMousePoint()) && !GuiData.blockingInput)
            {
                GuiData.hot = myID;
                if (GuiData.isMouseLeftDown() && (!DisableIfAnotherIsActive || GuiData.active == -1))
                    GuiData.active = myID;
            }
            else
            {
                if (GuiData.hot == myID)
                    GuiData.hot = -1;
                if (GuiData.isMouseLeftDown() && GuiData.active == myID && GuiData.active == myID)
                    GuiData.active = -1;
            }
            drawModernButton(myID, x, y, width, height, text, selectedColor, tex);
            return flag;
        }

        private static void drawButton(int myID, int x, int y, int width, int height, string text, Color? selectedColor,
            Texture2D tex)
        {
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = height;
            --destinationRectangle.X;
            destinationRectangle.Width += 2;
            --destinationRectangle.Y;
            destinationRectangle.Height += 2;
            if (outlineOnly)
                RenderedRectangle.doRectangleOutline(destinationRectangle.X, destinationRectangle.Y,
                    destinationRectangle.Width, destinationRectangle.Height, 1,
                    GuiData.hot == myID
                        ? (GuiData.active == myID ? GuiData.Default_Selected_Color : GuiData.Default_Lit_Backing_Color)
                        : GuiData.Default_Backing_Color);
            else if (drawingOutline)
                GuiData.spriteBatch.Draw(tex, destinationRectangle,
                    GuiData.hot == myID ? GuiData.Default_Lit_Backing_Color : GuiData.Default_Backing_Color);
            ++destinationRectangle.X;
            destinationRectangle.Width -= 2;
            ++destinationRectangle.Y;
            destinationRectangle.Height -= 2;
            if (!outlineOnly)
                GuiData.spriteBatch.Draw(tex, destinationRectangle,
                    GuiData.active == myID ? GuiData.Default_Unselected_Color : selectedColor.Value);
            var scale = GuiData.tinyfont.MeasureString(text);
            scale.X = scale.X <= (double) (width - 4) ? 1f : (width - 4)/scale.X;
            scale.Y = scale.Y <= (double) (height - 4) ? 1f : (height - 2)/scale.Y;
            scale.X = Math.Min(scale.X, scale.Y);
            scale.Y = scale.X;
            GuiData.spriteBatch.DrawString(GuiData.tinyfont, text, new Vector2(x + 2 + 1, y + 1 + 1), Color.Black, 0.0f,
                Vector2.Zero, scale, SpriteEffects.None, 0.5f);
            GuiData.spriteBatch.DrawString(GuiData.tinyfont, text, new Vector2(x + 2, y + 1), Color.White, 0.0f,
                Vector2.Zero, scale, SpriteEffects.None, 0.5f);
        }

        private static void drawModernButton(int myID, int x, int y, int width, int height, string text,
            Color? selectedColor, Texture2D tex)
        {
            var num1 = width > 65 ? 13 : 0;
            if (!selectedColor.HasValue)
                selectedColor = GuiData.Default_Trans_Grey_Strong;
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = height;
            if (tex.Equals(Utils.white))
            {
                if (!outlineOnly)
                {
                    GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                        GuiData.hot == myID
                            ? (GuiData.active == myID
                                ? GuiData.Default_Trans_Grey_Dark
                                : GuiData.Default_Trans_Grey_Bright)
                            : GuiData.Default_Trans_Grey);
                    destinationRectangle.Width = num1;
                    GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, selectedColor.Value);
                }
                RenderedRectangle.doRectangleOutline(x, y, width, height, 1,
                    outlineOnly ? selectedColor : GuiData.Default_Trans_Grey_Solid);
            }
            else
                GuiData.spriteBatch.Draw(tex, destinationRectangle,
                    GuiData.hot == myID
                        ? (GuiData.active == myID ? GuiData.Default_Unselected_Color : GuiData.Default_Lit_Backing_Color)
                        : selectedColor.Value);
            var spriteFont = smallButtonDraw ? GuiData.detailfont : GuiData.tinyfont;
            var scale = spriteFont.MeasureString(text);
            var num2 = scale.Y;
            scale.X = scale.X <= (double) (width - 4) ? 1f : (width - (4 + num1 + 5))/scale.X;
            scale.Y = scale.Y <= (double) height ? 1f : height/scale.Y;
            scale.X = Math.Min(scale.X, scale.Y);
            scale.Y = scale.X;
            var num3 = num1 + 4;
            var num4 = num2*scale.Y;
            var y1 = (float) (y + height/2.0 - num4/2.0 + 1.0);
            GuiData.spriteBatch.DrawString(spriteFont, text, new Vector2(x + 2 + num3, y1), Color.White, 0.0f,
                Vector2.Zero, scale, SpriteEffects.None, 0.5f);
        }

        public static bool doHoldDownButton(int myID, int x, int y, int width, int height, string text, bool hasOutline,
            Color? outlineColor, Color? selectedColor)
        {
            wasPressedDown = false;
            wasReleased = false;
            if (!outlineColor.HasValue)
                outlineColor = Color.White;
            if (!selectedColor.HasValue)
                selectedColor = GuiData.Default_Selected_Color;
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = height;
            var flag = GuiData.isMouseLeftDown() && GuiData.active == myID;
            if (destinationRectangle.Contains(GuiData.getMousePoint()))
            {
                GuiData.hot = myID;
                if (GuiData.mouseWasPressed())
                    wasPressedDown = true;
                if (GuiData.isMouseLeftDown())
                {
                    GuiData.active = myID;
                    flag = true;
                }
            }
            else
            {
                if (GuiData.hot == myID)
                    GuiData.hot = -1;
                if (!GuiData.isMouseLeftDown() && GuiData.active == myID && GuiData.active == myID)
                    GuiData.active = -1;
            }
            if (GuiData.mouseLeftUp())
                wasReleased = true;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                GuiData.active == myID
                    ? selectedColor.Value
                    : (GuiData.hot == myID ? GuiData.Default_Lit_Backing_Color : GuiData.Default_Light_Backing_Color));
            if (hasOutline)
                RenderedRectangle.doRectangleOutline(x, y, width, height, 2, outlineColor);
            return flag;
        }
    }
}
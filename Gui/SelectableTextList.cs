// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.SelectableTextList
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Gui
{
    public static class SelectableTextList
    {
        public const int BORDER_WIDTH = 2;
        public const float ITEM_HEIGHT = 18f;
        public static int scrollOffset;
        public static bool wasActivated;
        public static bool selectionWasChanged;
        private static readonly Color scrollBarColor = new Color(140, 140, 140, 80);

        public static int doList(int myID, int x, int y, int width, int height, string[] text, int lastSelectedIndex,
            Color? selectedColor)
        {
            if (!selectedColor.HasValue)
                selectedColor = GuiData.Default_Selected_Color;
            var num1 = -1;
            wasActivated = false;
            var num2 = lastSelectedIndex;
            selectionWasChanged = false;
            var mousePos = GuiData.getMousePos();
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = height;
            if (destinationRectangle.Contains(GuiData.getMousePoint()))
            {
                GuiData.hot = myID;
                scrollOffset += (int) GuiData.getMouseWheelScroll();
                scrollOffset = Math.Max(0, Math.Min(scrollOffset, text.Length - (int) (height/18.0)));
            }
            else if (GuiData.hot == myID)
                GuiData.hot = -1;
            var num3 = Math.Max(0, Math.Min(scrollOffset, text.Length - (int) (height/18.0)));
            if (GuiData.hot == myID)
            {
                for (var index = 0; index < text.Length; ++index)
                {
                    if (mousePos.Y >= y + index*18.0 && mousePos.Y <= y + (index + 1)*18.0 &&
                        mousePos.Y < (double) (y + height))
                    {
                        num1 = index + num3;
                        wasActivated = true;
                    }
                }
            }
            if (num1 != -1 && num1 != lastSelectedIndex && GuiData.mouseLeftUp())
                lastSelectedIndex = num1;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                GuiData.hot == myID ? GuiData.Default_Lit_Backing_Color : GuiData.Default_Backing_Color);
            destinationRectangle.X += 2;
            destinationRectangle.Width -= 4;
            destinationRectangle.Y += 2;
            destinationRectangle.Height -= 4;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, GuiData.Default_Dark_Background_Color);
            var position = new Vector2(destinationRectangle.X, destinationRectangle.Y);
            destinationRectangle.Height = 18;
            for (var index = num3; index < text.Length; ++index)
            {
                GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                    lastSelectedIndex == index
                        ? selectedColor.Value
                        : (num1 == index ? GuiData.Default_Unselected_Color : GuiData.Default_Dark_Neutral_Color));
                var scale = GuiData.UITinyfont.MeasureString(text[index]);
                scale.X = scale.X <= (double) (width - 4) ? 1f : (width - 4)/scale.X;
                scale.Y = scale.Y <= 18.0 ? 1f : 14f/scale.Y;
                GuiData.spriteBatch.DrawString(GuiData.UITinyfont, text[index], position, Color.White, 0.0f,
                    Vector2.Zero, scale, SpriteEffects.None, 0.5f);
                position.Y += 18f;
                destinationRectangle.Y += 18;
                if (position.Y > (double) (y + height - 4))
                    break;
            }
            if (text.Length*18.0 > height)
            {
                var num4 = 2f;
                var num5 = height/(text.Length*18f)*height;
                height -= 4;
                var num6 = -height + (float) ((height - (double) num5)*(num3/((text.Length*18.0 - height)/18.0)));
                destinationRectangle.X = (int) (position.X + (double) width - 3.0*num4 - 2.0);
                destinationRectangle.Y = (int) (position.Y + (double) num6 + 2.0);
                destinationRectangle.Height = (int) num5;
                destinationRectangle.Width = (int) num4;
                GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, scrollBarColor);
            }
            if (lastSelectedIndex != num2)
                selectionWasChanged = true;
            return lastSelectedIndex;
        }

        public static int doFancyList(int myID, int x, int y, int width, int height, string[] text,
            int lastSelectedIndex, Color? selectedColor, bool HasDraggableScrollbar = false)
        {
            if (!selectedColor.HasValue)
                selectedColor = GuiData.Default_Selected_Color;
            var num1 = -1;
            wasActivated = false;
            var num2 = lastSelectedIndex;
            selectionWasChanged = false;
            var mousePos = GuiData.getMousePos();
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = height;
            if (destinationRectangle.Contains(GuiData.getMousePoint()))
            {
                GuiData.hot = myID;
                scrollOffset += (int) GuiData.getMouseWheelScroll();
                scrollOffset = Math.Max(0, Math.Min(scrollOffset, text.Length - (int) (height/18.0)));
            }
            else if (GuiData.hot == myID)
                GuiData.hot = -1;
            var num3 = Math.Max(0, Math.Min(scrollOffset, text.Length - (int) (height/18.0)));
            var num4 = HasDraggableScrollbar ? 4f : 2f;
            if (GuiData.hot == myID && (!HasDraggableScrollbar || mousePos.X < x + width - 2.0*num4))
            {
                for (var index = 0; index < text.Length; ++index)
                {
                    if (mousePos.Y >= y + index*18.0 && mousePos.Y <= y + (index + 1)*18.0 &&
                        mousePos.Y < (double) (y + height))
                    {
                        num1 = index + num3;
                        wasActivated = true;
                    }
                }
            }
            if (num1 != -1 && num1 != lastSelectedIndex && GuiData.mouseLeftUp())
                lastSelectedIndex = num1;
            destinationRectangle.X += 2;
            destinationRectangle.Width -= 4;
            destinationRectangle.Y += 2;
            destinationRectangle.Height -= 4;
            var position = new Vector2(destinationRectangle.X, destinationRectangle.Y);
            destinationRectangle.Height = 18;
            for (var index = num3; index < text.Length; ++index)
            {
                GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                    lastSelectedIndex == index
                        ? selectedColor.Value
                        : (num1 == index ? selectedColor.Value*0.45f : GuiData.Default_Dark_Neutral_Color));
                var scale = GuiData.UITinyfont.MeasureString(text[index]);
                scale.X = scale.X <= (double) (width - 4) ? 1f : (width - 4)/scale.X;
                scale.Y = scale.Y <= 18.0 ? 1f : 14f/scale.Y;
                scale.X = Math.Min(scale.X, scale.Y);
                scale.Y = Math.Min(scale.X, scale.Y);
                GuiData.spriteBatch.DrawString(GuiData.UITinyfont, text[index], position, Color.White, 0.0f,
                    Vector2.Zero, scale, SpriteEffects.None, 0.5f);
                position.Y += 18f;
                destinationRectangle.Y += 18;
                if (position.Y > (double) (y + height - 4))
                    break;
            }
            if (text.Length*18.0 > height)
            {
                var num5 = num4;
                var num6 = height/(text.Length*18f)*height;
                height -= 4;
                var num7 = -height + (float) ((height - (double) num6)*(num3/((text.Length*18.0 - height)/18.0)));
                destinationRectangle.X =
                    (int) (position.X + (double) width - (HasDraggableScrollbar ? 2.0 : 3.0)*num5 - 2.0);
                destinationRectangle.Y = (int) (position.Y + (double) num7 + 2.0);
                destinationRectangle.Height = (int) num6;
                destinationRectangle.Width = (int) num5;
                if (!HasDraggableScrollbar)
                    GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, scrollBarColor);
                else
                    scrollOffset =
                        (int)
                            (ScrollBar.doVerticalScrollBar(myID + 101, destinationRectangle.X, y,
                                destinationRectangle.Width, height, (int) (text.Length*18.0), num3*18f)/18.0);
            }
            if (lastSelectedIndex != num2)
                selectionWasChanged = true;
            return lastSelectedIndex;
        }
    }
}
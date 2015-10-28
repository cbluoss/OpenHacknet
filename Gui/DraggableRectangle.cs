using System;
using Microsoft.Xna.Framework;

namespace Hacknet.Gui
{
    public static class DraggableRectangle
    {
        public static bool isDragging;
        public static Vector2 originalClickPos;
        public static Vector2 originalClickOffset;

        public static Vector2 doDraggableRectangle(int myID, float x, float y, int width, int height)
        {
            return doDraggableRectangle(myID, x, y, width, height, -1f, new Color?(), new Color?());
        }

        public static Vector2 doDraggableRectangle(int myID, float x, float y, int width, int height,
            float selectableBorder, Color? selectedColor, Color? deselectedColor)
        {
            return doDraggableRectangle(myID, x, y, width, height, selectableBorder, selectedColor, deselectedColor,
                true, true, float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
        }

        public static Vector2 doDraggableRectangle(int myID, float x, float y, int width, int height,
            float selectableBorder, Color? selectedColor, Color? deselectedColor, bool canMoveY, bool canMoveX,
            float xMax, float yMax, float xMin, float yMin)
        {
            isDragging = false;
            if (!selectedColor.HasValue)
                selectedColor = GuiData.Default_Selected_Color;
            if (!deselectedColor.HasValue)
                deselectedColor = GuiData.Default_Unselected_Color;
            var vector2 = GuiData.temp;
            vector2.X = 0.0f;
            vector2.Y = 0.0f;
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = (int) x;
            destinationRectangle.Y = (int) y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = height;
            var rectangle = GuiData.tmpRect;
            rectangle.X = (int) (x + (double) selectableBorder);
            rectangle.Y = (int) (y + (double) selectableBorder);
            rectangle.Width = selectableBorder == -1.0 ? 0 : (int) (width - 2.0*selectableBorder);
            rectangle.Height = selectableBorder == -1.0 ? 0 : (int) (height - 2.0*selectableBorder);
            if (destinationRectangle.Contains(GuiData.getMousePoint()) && !rectangle.Contains(GuiData.getMousePoint()))
            {
                GuiData.hot = myID;
                if (GuiData.active != myID && GuiData.mouseWasPressed())
                {
                    GuiData.active = myID;
                    originalClickPos = GuiData.getMousePos();
                    originalClickPos.X -= x;
                    originalClickPos.Y -= y;
                    originalClickOffset = new Vector2(originalClickPos.X - x, originalClickPos.Y - y);
                }
            }
            else if (GuiData.hot == myID)
                GuiData.hot = -1;
            if (GuiData.active == myID)
            {
                if (GuiData.mouseLeftUp())
                {
                    GuiData.active = -1;
                }
                else
                {
                    if (canMoveX)
                    {
                        vector2.X = GuiData.mouse.X - x - originalClickPos.X;
                        vector2.X = Math.Min(Math.Max(destinationRectangle.X + vector2.X, xMin), xMax) -
                                    destinationRectangle.X;
                    }
                    if (canMoveY)
                    {
                        vector2.Y = GuiData.mouse.Y - y - originalClickPos.Y;
                        vector2.Y = Math.Min(Math.Max(destinationRectangle.Y + vector2.Y, yMin), yMax) -
                                    destinationRectangle.Y;
                    }
                    destinationRectangle.X += (int) vector2.X;
                    destinationRectangle.Y += (int) vector2.Y;
                    isDragging = true;
                }
            }
            if (GuiData.active == myID || GuiData.hot == myID)
                GuiData.blockingInput = true;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                GuiData.hot == myID || GuiData.active == myID ? selectedColor.Value : deselectedColor.Value);
            return vector2;
        }
    }
}
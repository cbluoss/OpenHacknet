// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.CheckBox
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;

namespace Hacknet.Gui
{
    public static class CheckBox
    {
        public const int WIDTH = 20;
        public const int HEIGHT = 20;
        public const int INTERIOR_BORDER = 4;

        public static bool doCheckBox(int myID, int x, int y, bool isChecked, Color? selectedColor)
        {
            if (!selectedColor.HasValue)
                selectedColor = GuiData.Default_Selected_Color;
            if (GuiData.hot == myID && GuiData.active == myID && GuiData.mouseLeftUp())
            {
                isChecked = !isChecked;
                GuiData.active = -1;
            }
            RenderedRectangle.doRectangleOutline(x, y, 20, 20, 2,
                GuiData.hot == myID ? GuiData.Default_Lit_Backing_Color : GuiData.Default_Light_Backing_Color);
            RenderedRectangle.doRectangle(x + 4, y + 4, 12, 12,
                isChecked
                    ? selectedColor
                    : (GuiData.active == myID ? GuiData.Default_Unselected_Color : GuiData.Default_Backing_Color));
            var rectangle = GuiData.tmpRect;
            rectangle.X = x;
            rectangle.Y = y;
            rectangle.Width = 20;
            rectangle.Height = 20;
            if (rectangle.Contains(GuiData.getMousePoint()))
            {
                GuiData.hot = myID;
                if (GuiData.isMouseLeftDown())
                    GuiData.active = myID;
            }
            else
            {
                if (GuiData.hot == myID)
                    GuiData.hot = -1;
                if (GuiData.isMouseLeftDown() && GuiData.active == myID && GuiData.active == myID)
                    GuiData.active = -1;
            }
            return isChecked;
        }

        public static bool doCheckBox(int myID, int x, int y, bool isChecked, Color? selectedColor, string text)
        {
            if (GuiData.hot == myID)
                TextItem.doSmallLabel(new Vector2(x + 20, y - 20), text, new Color?());
            return doCheckBox(myID, x, y, isChecked, selectedColor);
        }
    }
}
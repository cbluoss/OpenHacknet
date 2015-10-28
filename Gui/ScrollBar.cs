// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.ScrollBar
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;

namespace Hacknet.Gui
{
    public static class ScrollBar
    {
        public static float doVerticalScrollBar(int id, int xPos, int yPos, int drawWidth, int drawHeight,
            int contentHeight, float scroll)
        {
            if (drawHeight > contentHeight)
                contentHeight = drawHeight;
            var destinationRectangle = new Rectangle(xPos, yPos, drawWidth, drawHeight);
            if (destinationRectangle.Contains(GuiData.getMousePoint()) || GuiData.active == id)
                GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, Color.Gray*0.1f);
            var num1 = scroll;
            float num2 = contentHeight - drawHeight;
            var num3 = drawHeight/(float) contentHeight*drawHeight;
            var num4 = scroll/num2*(drawHeight - num3);
            var num5 =
                DraggableRectangle.doDraggableRectangle(id, xPos, yPos + num4, drawWidth, (int) num3, drawWidth,
                    Color.White, Color.Gray, true, false, xPos, yPos + drawHeight - num3, xPos, yPos).Y;
            if (Math.Abs(num5) > 0.100000001490116)
                num1 = (float) ((num4 + (double) num5)/(drawHeight - (double) num3))*num2;
            return num1;
        }
    }
}
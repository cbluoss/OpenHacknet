// Decompiled with JetBrains decompiler
// Type: Hacknet.UIUtils.AttractModeMenuScreen
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.UIUtils
{
    public class AttractModeMenuScreen
    {
        public Action Exit;
        public Action Start;

        public void Draw(Rectangle dest, SpriteBatch sb)
        {
            var width1 = (int) (dest.Width*0.5);
            var height1 = (int) (dest.Height*0.400000005960464);
            var dest1 = new Rectangle(dest.Width/2 - width1/2, (int) (dest.Y + dest.Height/2.5 - height1/2), width1,
                height1);
            var destinationRectangle1 = new Rectangle(dest.X, dest1.Y + 115, dest.Width, dest1.Height - 245);
            sb.Draw(Utils.white, destinationRectangle1,
                Color.Lerp(new Color(115, 0, 0), new Color(122, 0, 0),
                    0.0f + Utils.randm((float) (1.0 - Utils.randm(1f)*(double) Utils.randm(1f)))));
            dest1.Y += 12;
            TextItem.doFontLabelToSize(dest1, Utils.FlipRandomChars("HACKNET", 0.028), GuiData.titlefont,
                Color.White*0.12f);
            FlickeringTextEffect.DrawLinedFlickeringText(dest1, Utils.FlipRandomChars("HACKNET", 0.003), 11f, 0.7f,
                GuiData.titlefont, null, Color.White, 5);
            var y = destinationRectangle1.Y + dest1.Height - 80;
            var width2 = dest.Width/4;
            var num1 = 20;
            var num2 = (width2 - num1)/2;
            var height2 = 42;
            var destinationRectangle2 = new Rectangle(dest.X + dest.Width/2 - width2/2, y, width2, height2);
            sb.Draw(Utils.white, destinationRectangle2, Color.Black*0.8f);
            if (
                !Button.doButton(18273302, destinationRectangle2.X, destinationRectangle2.Y, destinationRectangle2.Width,
                    destinationRectangle2.Height, "New Session", new Color(124, 137, 149)) || Start == null)
                return;
            Start();
        }
    }
}
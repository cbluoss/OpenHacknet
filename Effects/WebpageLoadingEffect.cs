// Decompiled with JetBrains decompiler
// Type: Hacknet.Effects.WebpageLoadingEffect
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    public static class WebpageLoadingEffect
    {
        public static void DrawLoadingEffect(Rectangle bounds, SpriteBatch sb, object OS_obj,
            bool drawLoadingText = true)
        {
            var os = (OS) OS_obj;
            var rectangle = new Rectangle(bounds.X + bounds.Width/2, bounds.Y + bounds.Height/2, 2, 70);
            var num1 = (float) Math.Abs(Math.Sin(os.timer*0.200000002980232)*100.0);
            var origin = new Vector2(2f, 10f);
            var num2 = 6.283185f/num1;
            var rotation = 0.0f;
            var num3 = 0;
            while (rotation < 6.28318548202515)
            {
                var destinationRectangle = rectangle;
                destinationRectangle.Height =
                    Math.Abs((int) (rectangle.Height*Math.Sin(2.0*os.timer + num3*0.200000002980232))) + 10;
                origin.Y = 1.6f;
                sb.Draw(Utils.white, destinationRectangle, new Rectangle?(), os.highlightColor, rotation, origin,
                    SpriteEffects.FlipVertically, 0.6f);
                ++num3;
                rotation += num2;
            }
            if (!drawLoadingText)
                return;
            TextItem.doFontLabelToSize(new Rectangle(bounds.X, bounds.Y + 20, bounds.Width, 30), "Loading...",
                GuiData.font, Utils.AddativeWhite);
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.UIUtils.FancyOutlines
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.UIUtils
{
    public static class FancyOutlines
    {
        public static void DrawCornerCutOutline(Rectangle bounds, SpriteBatch sb, float cornerCut, Color col)
        {
            var vector2_1 = new Vector2(bounds.X, bounds.Y + cornerCut);
            var vector2_2 = new Vector2(bounds.X + cornerCut, bounds.Y);
            Utils.drawLine(sb, vector2_1, vector2_2, Vector2.Zero, col, 0.6f);
            vector2_1 = new Vector2(bounds.X + bounds.Width - cornerCut, bounds.Y);
            Utils.drawLine(sb, vector2_2, vector2_1, Vector2.Zero, col, 0.6f);
            vector2_2 = new Vector2(bounds.X + bounds.Width, bounds.Y + cornerCut);
            Utils.drawLine(sb, vector2_2, vector2_1, Vector2.Zero, col, 0.6f);
            vector2_1 = new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height - cornerCut);
            Utils.drawLine(sb, vector2_2, vector2_1, Vector2.Zero, col, 0.6f);
            vector2_2 = new Vector2(bounds.X + bounds.Width - cornerCut, bounds.Y + bounds.Height);
            Utils.drawLine(sb, vector2_2, vector2_1, Vector2.Zero, col, 0.6f);
            vector2_1 = new Vector2(bounds.X + cornerCut, bounds.Y + bounds.Height);
            Utils.drawLine(sb, vector2_2, vector2_1, Vector2.Zero, col, 0.6f);
            vector2_2 = new Vector2(bounds.X, bounds.Y + bounds.Height - cornerCut);
            Utils.drawLine(sb, vector2_2, vector2_1, Vector2.Zero, col, 0.6f);
            vector2_1 = new Vector2(bounds.X, bounds.Y + cornerCut);
            Utils.drawLine(sb, vector2_2, vector2_1, Vector2.Zero, col, 0.6f);
        }
    }
}
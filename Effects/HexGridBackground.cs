// Decompiled with JetBrains decompiler
// Type: Hacknet.Effects.HexGridBackground
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    public class HexGridBackground
    {
        public enum ColoringAlgorithm
        {
            NegaitiveSinWash,
            CorrectedSinWash,
            OutlinedSinWash
        }

        public bool HasRedFlashyOnes;
        private readonly Texture2D Hex;
        public float HexScale = 0.1f;
        private readonly LCG lcg = new LCG(false);
        private readonly Texture2D Outline;
        private float timer;

        public HexGridBackground(ContentManager content)
        {
            Hex = content.Load<Texture2D>("Sprites/Misc/Hexagon");
            Outline = content.Load<Texture2D>("Sprites/Misc/HexOutline");
        }

        public void Update(float dt)
        {
            timer += dt;
        }

        public void Draw(Rectangle dest, SpriteBatch sb, Color first, Color second,
            ColoringAlgorithm algorithm = ColoringAlgorithm.CorrectedSinWash, float angle = 0.0f)
        {
            var num1 = 2f;
            float x = dest.X;
            float y = dest.Y;
            var num2 = HexScale*Hex.Width;
            var num3 = HexScale*Hex.Height;
            var seed = 10;
            var flag1 = true;
            while (x + (double) num2 < dest.X + dest.Width)
            {
                while (y + (double) num3 < dest.Y + dest.Height)
                {
                    lcg.reSeed(seed);
                    var num4 = lcg.NextFloat();
                    var flag2 = false;
                    var color = second;
                    if (HasRedFlashyOnes && num4 <= 1.0/1000.0)
                    {
                        color = Utils.AddativeRed*0.5f;
                        flag2 = true;
                    }
                    float amount;
                    switch (algorithm)
                    {
                        case ColoringAlgorithm.NegaitiveSinWash:
                            amount = (float) Math.Sin(num4 + timer*(double) lcg.NextFloat());
                            break;
                        default:
                            amount =
                                Math.Abs(
                                    (float)
                                        Math.Sin(num4 + timer*(double) Math.Abs(lcg.NextFloat()*(flag2 ? 1f : 0.3f))));
                            break;
                    }
                    sb.Draw(Hex, Utils.RotatePoint(new Vector2(x, y), angle), new Rectangle?(),
                        Color.Lerp(first, color, amount), angle, Vector2.Zero, new Vector2(HexScale), SpriteEffects.None,
                        0.4f);
                    if (algorithm == ColoringAlgorithm.OutlinedSinWash)
                    {
                        if (flag2)
                            color = Utils.AddativeRed;
                        sb.Draw(Outline, Utils.RotatePoint(new Vector2(x, y), angle), new Rectangle?(),
                            Color.Lerp(first, color, amount), angle, Vector2.Zero, new Vector2(HexScale),
                            SpriteEffects.None, 0.4f);
                    }
                    ++seed;
                    y += num3 + num1;
                }
                x += num2 - 60f*HexScale + num1;
                y = dest.Y;
                ++seed;
                if (flag1)
                    y -= num3/2f;
                flag1 = !flag1;
            }
        }
    }
}
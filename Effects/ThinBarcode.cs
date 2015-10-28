using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    public class ThinBarcode
    {
        private readonly List<int> gaps = new List<int>();
        private List<int> gapsLast = new List<int>();
        private readonly int height;
        private readonly int oldW;
        private readonly List<int> widths = new List<int>();
        private List<int> widthsLast = new List<int>();

        public ThinBarcode(int w, int height)
        {
            oldW = w;
            this.height = height;
            regenerate();
            widthsLast = widths;
            gapsLast = gaps;
        }

        public void regenerate()
        {
            var num1 = oldW;
            widthsLast = widths;
            gapsLast = gaps;
            widths.Clear();
            gaps.Clear();
            var num2 = 0;
            while (num2 < num1)
            {
                var num3 = Math.Min(num1 - num2, Utils.random.Next(1, 20));
                if (num3 > 0)
                {
                    var num4 = Utils.random.Next(3, 11);
                    widths.Add(num3);
                    gaps.Add(num4);
                    num2 += num3 + num4;
                }
            }
        }

        public void Draw(SpriteBatch sb, int posX, int posY, Color c)
        {
            var destinationRectangle = new Rectangle(posX, posY, 0, height);
            for (var index = 0; index < widths.Count; ++index)
            {
                destinationRectangle.Width = widths[index];
                sb.Draw(Utils.white, destinationRectangle, c);
                destinationRectangle.X += widths[index];
                destinationRectangle.X += gaps[index];
            }
        }
    }
}
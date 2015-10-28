using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.UIUtils
{
    internal class WaveformRenderer
    {
        private readonly double[] left;
        private readonly double[] right;
        private int SecondBlockSize = 100;

        public WaveformRenderer(string filename)
        {
            AudioUtils.openWav(filename, out left, out right);
        }

        public void RenderWaveform(double time, double totalTime, SpriteBatch sb, Rectangle bounds)
        {
            time %= totalTime;
            SecondBlockSize = (int) (left.Length/totalTime);
            var num1 = SecondBlockSize/100;
            var num2 = (int) (time*SecondBlockSize);
            var num3 = Math.Min(left.Length - 1, num2 + num1) - num2;
            var num4 = bounds.Width/(double) num3;
            var num5 = 0;
            for (var index = 0; index < num3; ++index)
            {
                var num6 = left[num2 + index];
                if (num6 == 0.0)
                    ++num5;
                if (num5 == 20)
                    Console.WriteLine("Zeroed at " + time);
                var num7 = num6*bounds.Height;
                var destinationRectangle = new Rectangle((int) (bounds.X + index*num4),
                    bounds.Y + bounds.Height/2 - (int) (num7/2.0), (int) num4, (int) num7);
                sb.Draw(Utils.white, destinationRectangle, Color.White*0.4f);
            }
        }
    }
}
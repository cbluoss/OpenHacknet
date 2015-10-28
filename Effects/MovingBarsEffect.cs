using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    public class MovingBarsEffect
    {
        public bool IsInverted;
        private readonly List<BarLine> Lines = new List<BarLine>();
        public float MaxLineChangeTime = 2f;
        public float MinLineChangeTime = 0.2f;

        public void Update(float dt)
        {
            for (var index = 0; index < Lines.Count; ++index)
            {
                var barLine = Lines[index];
                barLine.TimeRemaining -= dt;
                if (barLine.TimeRemaining <= 0.0)
                {
                    barLine.Current = barLine.Next;
                    barLine.Next = Utils.randm(1f);
                    barLine.TotalTimeThisStep = MinLineChangeTime + Utils.randm(MaxLineChangeTime - MinLineChangeTime);
                    barLine.TimeRemaining = barLine.TotalTimeThisStep;
                }
                Lines[index] = barLine;
            }
        }

        public void Draw(SpriteBatch sb, Rectangle bounds, float minHeight, float lineWidth, float lineSeperation,
            Color drawColor)
        {
            var num1 = 0;
            var num2 = 0.0f;
            while (num2 + (double) lineWidth < bounds.Width)
            {
                ++num1;
                num2 += lineWidth + lineSeperation;
            }
            var flag = false;
            while (Lines.Count - 1 < num1)
            {
                Lines.Add(new BarLine
                {
                    TimeRemaining = -1f
                });
                flag = true;
            }
            if (flag)
                Update(0.0f);
            float num3 = bounds.X;
            for (var index = 0; index < num1; ++index)
            {
                var barLine = Lines[index];
                var num4 =
                    Utils.QuadraticOutCurve((float) (1.0 - barLine.TimeRemaining/(double) barLine.TotalTimeThisStep));
                var num5 = barLine.Current + (barLine.Next - barLine.Current)*num4;
                var num6 = bounds.Height - minHeight;
                var height = (int) (minHeight + num6*(double) num5);
                var destinationRectangle = new Rectangle((int) num3, bounds.Y + bounds.Height - height, (int) lineWidth,
                    height);
                if (IsInverted)
                    destinationRectangle.Y = bounds.Y;
                sb.Draw(Utils.white, destinationRectangle, drawColor);
                num3 += lineWidth + lineSeperation;
            }
        }

        private struct BarLine
        {
            public float Current;
            public float Next;
            public float TimeRemaining;
            public float TotalTimeThisStep;
        }
    }
}
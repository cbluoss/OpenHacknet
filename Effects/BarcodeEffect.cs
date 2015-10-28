// Decompiled with JetBrains decompiler
// Type: Hacknet.Effects.BarcodeEffect
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    public class BarcodeEffect
    {
        private const double MAX_WIDTH = 8.0;
        private const double MAX_OFFSET = 2.0;
        private const double MAX_DELAY = 3.0;
        private const float COMPLETE_TIME = 4f;
        private const float COMPLETE_TIME_DELAY = 0.9f;
        private List<float> complete;
        private float completeTime;
        private List<float> delays;
        public bool isInverted;
        public bool leftRightBias;
        public bool LeftRightMaxSizeFalloff;
        public int maxWidth;
        private List<float> offsets;
        private List<float> widths;

        public BarcodeEffect(int width, bool inverted = false, bool leftRight = false)
        {
            maxWidth = width;
            isInverted = inverted;
            leftRightBias = leftRight;
            reset();
        }

        public void reset()
        {
            widths = new List<float>();
            offsets = new List<float>();
            delays = new List<float>();
            complete = new List<float>();
            var num1 = 0.0f;
            var flag = false;
            while (num1 < (double) maxWidth)
            {
                if (num1 + 8.0 >= maxWidth)
                {
                    widths.Add(maxWidth - num1);
                    delays.Add(0.0f);
                    complete.Add(0.0f);
                    num1 = maxWidth;
                }
                else
                {
                    var num2 = (float) (Utils.random.NextDouble()*(flag ? 2.0 : 8.0));
                    if (flag)
                    {
                        offsets.Add(num2);
                    }
                    else
                    {
                        widths.Add(num2);
                        complete.Add(0.0f);
                        if (leftRightBias)
                        {
                            var num3 = num1/maxWidth;
                            delays.Add((float) ((Utils.random.NextDouble()/2.0 + num3/2.0)*(3.0*num3)));
                        }
                        else
                            delays.Add((float) (Utils.random.NextDouble()*3.0));
                    }
                    num1 += num2;
                }
                flag = !flag;
            }
        }

        public void Update(float t)
        {
            var flag = true;
            for (var index1 = 0; index1 < delays.Count; ++index1)
            {
                if (delays[index1] > 0.0)
                {
                    List<float> list;
                    int index2;
                    (list = delays)[index2 = index1] = list[index2] - t;
                    if (delays[index1] < 0.0)
                    {
                        complete[index1] = (float) (-delays[index1]/4.0);
                        delays[index1] = 0.0f;
                    }
                    flag = false;
                }
                else
                {
                    List<float> list;
                    int index2;
                    (list = complete)[index2 = index1] = list[index2] + t/4f;
                    if (complete[index1] > 1.0)
                        complete[index1] = 1f;
                    else
                        flag = false;
                }
            }
            if (!flag)
                return;
            completeTime += t;
            if (completeTime <= 0.899999976158142)
                return;
            reset();
            completeTime = 0.0f;
        }

        public void Draw(int x, int y, int maxWidth, int maxHeight, SpriteBatch sb, Color? barColor = null)
        {
            var color = (barColor.HasValue ? barColor.Value : Color.White)*
                        (float) Math.Pow(1.0 - completeTime/0.899999976158142, 3.0);
            var position = new Vector2(x, y);
            var num1 = 0.0f;
            for (var index = 0; index < widths.Count; ++index)
            {
                float num2 = maxHeight;
                if (LeftRightMaxSizeFalloff)
                {
                    var num3 = 1f - Utils.QuadraticOutCurve(Math.Abs(index - widths.Count/2)/(widths.Count/2f));
                    num2 = maxHeight*num3;
                }
                position.X = x + num1;
                position.Y = y;
                var x1 = widths[index];
                var y1 = delays[index] <= 0.0 ? num2*(float) Math.Pow(complete[index], 0.5) : 0.0f;
                if (isInverted)
                    position.Y = y + maxHeight - y1;
                sb.Draw(Utils.white, position, new Rectangle?(), color, 0.0f, Vector2.Zero, new Vector2(x1, y1),
                    SpriteEffects.None, 0.5f);
                num1 += widths[index];
                if (offsets.Count > index)
                    num1 += offsets[index];
            }
        }
    }
}
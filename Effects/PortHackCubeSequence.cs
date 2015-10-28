using System;
using Microsoft.Xna.Framework;

namespace Hacknet.Effects
{
    public class PortHackCubeSequence
    {
        private float elapsedTime;
        public bool HeartFadeSequenceComplete;
        private float idle;
        private float rotTime;
        private float runtime;
        public bool ShouldCentralSpinInfinitley;
        private float spindown;
        private float spinup;
        private float startup = 0.3f;

        public void Reset()
        {
            spinup = startup = runtime = idle = spindown = rotTime = 0.0f;
            HeartFadeSequenceComplete = false;
        }

        public void DrawSequence(Rectangle dest, float t, float totalTime)
        {
            var num1 = 0.6f;
            var val1_1 = 1f;
            var val1_2 = ShouldCentralSpinInfinitley ? float.MaxValue : totalTime - 1.7f;
            var num2 = float.MaxValue;
            var num3 = float.MaxValue;
            elapsedTime += t;
            Math.Max(0.0f, elapsedTime);
            var num4 = 1f;
            var num5 = 1f;
            var num6 = 1f;
            if (startup < (double) num1)
            {
                startup += t;
                num4 = 0.0f;
                num5 = Utils.QuadraticOutCurve(Utils.QuadraticOutCurve(startup/num1));
            }
            else if (spinup < (double) val1_1)
            {
                spinup = Math.Min(val1_1, spinup + t);
                num4 = spinup/val1_1;
                rotTime += t*num4;
            }
            else if (runtime < (double) val1_2)
            {
                runtime = Math.Min(val1_2, runtime + t);
                rotTime += t;
            }
            else if (spindown < (double) num2)
            {
                spindown += t;
                var num7 = (float) (Math.Min(1f, spindown)/2.29999995231628/1.0);
                num4 = (float) (0.100000001490116 + 0.899999976158142*(1.0 - num7));
                if (num7 >= 0.400000005960464)
                    num4 = 0.2f;
                rotTime += t*num4;
                num6 = 1f - num7;
                num5 = (float) (0.300000011920929 + 0.699999988079071*Utils.QuadraticOutCurve(1f - num7));
            }
            else if (idle < (double) num3)
            {
                idle += t;
                num4 = 0.1f;
                rotTime += t*(num4*2f);
                num6 = 0.0f;
                num5 = 0.3f;
            }
            else
                spinup = startup = runtime = idle = spindown = rotTime = 0.0f;
            var num8 = 20;
            for (var index1 = 0; index1 < num8; ++index1)
            {
                var num7 = index1/(float) num8;
                var num9 = (float) (1.0 + 0.0500000007450581*num4*(num8 - index1)/num8*10.0);
                var num10 = num6;
                var num11 = Math.Max(0.0f, (float) (rotTime*(double) num9 - index1*0.100000001490116*num10))*1.5f;
                var num12 = num5;
                for (var index2 = 0; index2 < index1; ++index2)
                    num12 *= num5;
                var num13 = (num8 - index1)/(double) num8;
                Cube3D.RenderWireframe(new Vector3(dest.X/2, 0.0f, 0.0f), (float) (2.59999990463257 + index1/4.0*num12),
                    new Vector3(MathHelper.ToRadians(35.4f), MathHelper.ToRadians(45f) + num11,
                        MathHelper.ToRadians(0.0f)), Color.White);
            }
        }

        public void DrawHeartSequence(Rectangle dest, float t, float totalTime)
        {
            var num1 = 3f;
            var num2 = 10f;
            var num3 = 2f;
            var num4 = 9f;
            var num5 = float.MaxValue;
            var num6 = 2f;
            var num7 = 21f;
            elapsedTime += t;
            Math.Max(0.0f, elapsedTime);
            var num8 = 1f;
            var num9 = 1f;
            var num10 = 1f;
            var num11 = 0.0f;
            if (startup < (double) num1)
            {
                startup += t;
                num9 = Utils.QuadraticOutCurve(Utils.QuadraticOutCurve(startup/3f));
            }
            else if (spinup < (double) num2)
            {
                spinup = Math.Min(10f, spinup + t);
                num8 = spinup/10f;
                rotTime += t*num8;
            }
            else if (runtime < (double) num3)
            {
                runtime = Math.Min(2f, runtime + t);
                rotTime += t;
            }
            else if (spindown < (double) num4)
            {
                spindown += t;
                var num12 = spindown/9f;
                num8 = (float) (0.100000001490116 + 0.899999976158142*(1.0 - num12));
                rotTime += t*num8;
                num10 = 1f - num12;
                num9 = (float) (0.300000011920929 + 0.699999988079071*Utils.QuadraticOutCurve(1f - num12));
            }
            else if (idle < (double) num5)
            {
                idle += t;
                num8 = 0.1f;
                rotTime += t*num8;
                num10 = 0.0f;
                num9 = 0.3f;
                if (idle > (double) num6)
                {
                    num11 = Utils.QuadraticOutCurve(Math.Min(1f, (idle - num6)/num7));
                    HeartFadeSequenceComplete = num11 >= 1.0;
                }
            }
            else
                spinup = startup = runtime = idle = spindown = rotTime = 0.0f;
            var num13 = 20;
            for (var index1 = 0; index1 < num13; ++index1)
            {
                var num12 = index1/(float) num13;
                var num14 = (float) (1.0 + 0.0500000007450581*num8*(num13 - index1)/num13*10.0);
                var num15 = num10;
                var num16 = Math.Max(0.0f, (float) (rotTime*(double) num14 - index1*0.100000001490116*num15))*1.5f;
                var num17 = num9;
                for (var index2 = 0; index2 < index1; ++index2)
                    num17 *= num9;
                var num18 = (num13 - index1)/(double) num13;
                var num19 = 1f;
                var color = Color.White;
                if (num11 > 0.0)
                {
                    var num20 = 1f/num13;
                    if (index1*(double) num20 < num11)
                    {
                        var num21 = (index1 + 1)*num20;
                        if (num11 > (double) num21)
                        {
                            num19 = 0.0f;
                            color = Color.Transparent;
                        }
                        else
                        {
                            var amount = (float) (1.0 - num11%(double) num20/num20);
                            color = Color.Lerp(Utils.AddativeRed, Color.Red, amount)*amount;
                        }
                    }
                }
                Cube3D.RenderWireframe(new Vector3(dest.X/2, 0.0f, 0.0f), (float) (2.59999990463257 + index1/4.0*num17),
                    new Vector3(MathHelper.ToRadians(35.4f), MathHelper.ToRadians(45f) + num16,
                        MathHelper.ToRadians(0.0f)), color);
            }
        }
    }
}
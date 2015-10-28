// Decompiled with JetBrains decompiler
// Type: Hacknet.Misc.HSLColor
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;

namespace Hacknet.Misc
{
    public class HSLColor
    {
        public float Hue;
        public float Luminosity;
        public float Saturation;

        public HSLColor(float H, float S, float L)
        {
            Hue = H;
            Saturation = S;
            Luminosity = L;
        }

        public static HSLColor FromRGB(Color Clr)
        {
            return FromRGB(Clr.R, Clr.G, Clr.B);
        }

        public static HSLColor FromRGB(byte R, byte G, byte B)
        {
            var val1 = R/(float) byte.MaxValue;
            var val2_1 = G/(float) byte.MaxValue;
            var val2_2 = B/(float) byte.MaxValue;
            var num1 = Math.Min(Math.Min(val1, val2_1), val2_2);
            var num2 = Math.Max(Math.Max(val1, val2_1), val2_2);
            var num3 = num2 - num1;
            var H = 0.0f;
            var S = 0.0f;
            var L = (float) ((num2 + (double) num1)/2.0);
            if (num3 != 0.0)
            {
                S = L >= 0.5 ? num3/(2f - num2 - num1) : num3/(num2 + num1);
                if (val1 == (double) num2)
                    H = (val2_1 - val2_2)/num3;
                else if (val2_1 == (double) num2)
                    H = (float) (2.0 + (val2_2 - (double) val1)/num3);
                else if (val2_2 == (double) num2)
                    H = (float) (4.0 + (val1 - (double) val2_1)/num3);
            }
            return new HSLColor(H, S, L);
        }

        private float Hue_2_RGB(float v1, float v2, float vH)
        {
            if (vH < 0.0)
                ++vH;
            if (vH > 1.0)
                --vH;
            if (6.0*vH < 1.0)
                return v1 + (float) ((v2 - (double) v1)*6.0)*vH;
            if (2.0*vH < 1.0)
                return v2;
            if (3.0*vH < 2.0)
                return v1 + (float) ((v2 - (double) v1)*(0.0 - vH)*6.0);
            return v1;
        }

        public Color ToRGB()
        {
            byte num1;
            byte num2;
            byte num3;
            if (Saturation == 0.0)
            {
                num1 = (byte) Math.Round(Luminosity*(double) byte.MaxValue);
                num2 = (byte) Math.Round(Luminosity*(double) byte.MaxValue);
                num3 = (byte) Math.Round(Luminosity*(double) byte.MaxValue);
            }
            else
            {
                var num4 = Hue/6.0;
                var t2 = Luminosity >= 0.5
                    ? Luminosity + (double) Saturation - Luminosity*(double) Saturation
                    : Luminosity*(1.0 + Saturation);
                var t1 = 2.0*Luminosity - t2;
                var c1 = num4 + 1.0/3.0;
                var c2 = num4;
                var c3 = num4 - 1.0/3.0;
                var num5 = ColorCalc(c1, t1, t2);
                var num6 = ColorCalc(c2, t1, t2);
                var num7 = ColorCalc(c3, t1, t2);
                num1 = (byte) Math.Round(num5*byte.MaxValue);
                num2 = (byte) Math.Round(num6*byte.MaxValue);
                num3 = (byte) Math.Round(num7*byte.MaxValue);
            }
            return new Color
            {
                R = num1,
                G = num2,
                B = num3,
                A = byte.MaxValue
            };
        }

        private static double ColorCalc(double c, double t1, double t2)
        {
            if (c < 0.0)
                ++c;
            if (c > 1.0)
                --c;
            if (6.0*c < 1.0)
                return t1 + (t2 - t1)*6.0*c;
            if (2.0*c < 1.0)
                return t2;
            if (3.0*c < 2.0)
                return t1 + (t2 - t1)*(2.0/3.0 - c)*6.0;
            return t1;
        }
    }
}
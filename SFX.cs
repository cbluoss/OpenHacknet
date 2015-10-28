using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public static class SFX
    {
        private static readonly List<Vector2> circlePos = new List<Vector2>();
        private static readonly List<float> circleRadius = new List<float>();
        private static readonly List<float> circleExpand = new List<float>();
        private static readonly List<Color> circleColor = new List<Color>();
        private static Texture2D circleTex;
        private static Vector2 circleOrigin;

        public static void init(ContentManager content)
        {
            circleTex = content.Load<Texture2D>("CircleOutlineLarge");
            circleOrigin = new Vector2(circleTex.Width/2, circleTex.Height/2);
        }

        public static void addCircle(Vector2 pos, Color color, float radius)
        {
            circlePos.Add(pos);
            circleColor.Add(color);
            circleExpand.Add(0.0f);
            circleRadius.Add(radius/256f);
        }

        public static void Update(float t)
        {
            for (var index1 = 0; index1 < circlePos.Count; ++index1)
            {
                List<float> list;
                int index2;
                (list = circleExpand)[index2 = index1] = list[index2] + t;
                if (circleExpand[index1] >= 3.0)
                {
                    circlePos.RemoveAt(index1);
                    circleColor.RemoveAt(index1);
                    circleExpand.RemoveAt(index1);
                    circleRadius.RemoveAt(index1);
                    --index1;
                }
            }
        }

        public static void Draw(SpriteBatch sb)
        {
            try
            {
                for (var index = 0; index < circlePos.Count; ++index)
                {
                    var num1 = circleExpand[index]/3f;
                    var num2 = num1 >= 0.200000002980232 ? 1f - num1 : num1/0.2f;
                    sb.Draw(circleTex, circlePos[index], new Rectangle?(), circleColor[index]*num2, 0.0f, circleOrigin,
                        Vector2.One*circleRadius[index]*circleExpand[index], SpriteEffects.None, 0.2f);
                }
            }
            catch (IndexOutOfRangeException ex)
            {
            }
        }
    }
}
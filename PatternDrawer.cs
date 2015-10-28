using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public static class PatternDrawer
    {
        public static float time;
        public static Texture2D warningStripe;
        public static Texture2D errorTile;
        public static Texture2D binaryTile;
        public static Texture2D thinStripe;
        public static Texture2D star;

        public static void init(ContentManager content)
        {
            warningStripe = TextureBank.load("StripePattern", content);
            errorTile = TextureBank.load("ErrorTile", content);
            binaryTile = TextureBank.load("BinaryTile", content);
            thinStripe = TextureBank.load("ThinStripe", content);
            star = TextureBank.load("Sprites/Star", content);
        }

        public static void update(float t)
        {
            time += t;
        }

        public static void draw(Rectangle dest, float offset, Color backingColor, Color patternColor, SpriteBatch sb)
        {
            draw(dest, offset, backingColor, patternColor, sb, warningStripe);
        }

        public static void draw(Rectangle dest, float offset, Color backingColor, Color patternColor, SpriteBatch sb,
            Texture2D tex)
        {
            var num1 = dest.X;
            var y = dest.Y;
            var height = Math.Min(dest.Height, tex.Height);
            var width = (int) (time*(double) offset%1.0*tex.Width);
            var rectangle = Rectangle.Empty;
            sb.Draw(Utils.white, dest, backingColor);
            Rectangle destinationRectangle;
            for (; y - dest.Y + height <= dest.Height; destinationRectangle.Y = y)
            {
                var x = dest.X;
                destinationRectangle = new Rectangle(x, y, width, height);
                rectangle = new Rectangle(0, 0, width, height);
                rectangle.X = tex.Width - rectangle.Width;
                sb.Draw(tex, destinationRectangle, rectangle, patternColor);
                var num2 = x + width;
                destinationRectangle.X = num2;
                for (destinationRectangle.Width = tex.Width;
                    num2 <= dest.X + dest.Width - tex.Width;
                    destinationRectangle.X = num2)
                {
                    var sourceRectangle = new Rectangle?();
                    if (dest.Height < tex.Height)
                        sourceRectangle = new Rectangle(0, 0, tex.Width, dest.Height);
                    sb.Draw(tex, destinationRectangle, sourceRectangle, patternColor);
                    num2 += tex.Width;
                }
                destinationRectangle.X = num2;
                destinationRectangle.Width = dest.X + dest.Width - num2;
                rectangle.Width = destinationRectangle.Width;
                rectangle.X = 0;
                sb.Draw(tex, destinationRectangle, rectangle, patternColor);
                y += tex.Height;
            }
            destinationRectangle.Height = dest.Height - (y - dest.Y);
            rectangle.Height = destinationRectangle.Height;
            var num3 = dest.X;
            destinationRectangle.X = num3;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            rectangle.Width = width;
            rectangle.X = tex.Width - rectangle.Width;
            sb.Draw(tex, destinationRectangle, rectangle, patternColor);
            var num4 = num3 + width;
            destinationRectangle.X = num4;
            destinationRectangle.Width = tex.Width;
            rectangle.Width = tex.Width;
            for (rectangle.X = 0; num4 <= dest.X + dest.Width - tex.Width; destinationRectangle.X = num4)
            {
                sb.Draw(tex, destinationRectangle, rectangle, patternColor);
                num4 += tex.Width;
            }
            destinationRectangle.X = num4;
            destinationRectangle.Width = dest.X + dest.Width - num4;
            rectangle.Width = destinationRectangle.Width;
            rectangle.X = 0;
            sb.Draw(tex, destinationRectangle, rectangle, patternColor);
            var num5 = y + tex.Height;
            destinationRectangle.Y = num5;
        }
    }
}
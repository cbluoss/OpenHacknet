// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.TextItem
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Gui
{
    public static class TextItem
    {
        public static bool DrawShadow;

        public static Vector2 doMeasuredLabel(Vector2 pos, string text, Color? color)
        {
            if (!color.HasValue)
                color = Color.White;
            var vector2 = GuiData.font.MeasureString(text);
            GuiData.spriteBatch.DrawString(GuiData.font, text, pos + Vector2.One, Color.Gray);
            GuiData.spriteBatch.DrawString(GuiData.font, text, pos, color.Value);
            return vector2;
        }

        public static void doLabel(Vector2 pos, string text, Color? color)
        {
            if (!color.HasValue)
                color = Color.White;
            GuiData.spriteBatch.DrawString(GuiData.font, text, pos, color.Value);
        }

        public static void doFontLabelToSize(Rectangle dest, string text, SpriteFont font, Color color)
        {
            var vector2_1 = font.MeasureString(text);
            var scale = new Vector2(dest.Width/vector2_1.X, dest.Height/vector2_1.Y);
            var num = Math.Min(scale.X, scale.Y);
            var zero = Vector2.Zero;
            if (scale.X > (double) num)
            {
                zero.X = (float) (vector2_1.X*(scale.X - (double) num)/2.0);
                scale.X = num;
            }
            if (scale.Y > (double) num)
            {
                zero.Y = (float) (vector2_1.Y*(scale.Y - (double) num)/2.0);
                scale.Y = num;
            }
            var vector2_2 = new Vector2(dest.X, dest.Y);
            GuiData.spriteBatch.DrawString(font, text, vector2_2 + zero, color, 0.0f, Vector2.Zero, scale,
                SpriteEffects.None, 0.55f);
        }

        public static void doFontLabel(Vector2 pos, string text, SpriteFont font, Color? color,
            float widthTo = 3.402823E+38f, float heightTo = 3.402823E+38f)
        {
            if (!color.HasValue)
                color = Color.White;
            var scale = font.MeasureString(text);
            scale.X = scale.X <= (double) widthTo ? 1f : widthTo/scale.X;
            scale.Y = scale.Y <= (double) heightTo ? 1f : heightTo/scale.Y;
            scale.X = scale.Y = Math.Min(scale.X, scale.Y);
            if (DrawShadow)
                GuiData.spriteBatch.DrawString(font, text, pos + Vector2.One, Color.Gray, 0.0f, Vector2.Zero, scale,
                    SpriteEffects.None, 0.5f);
            GuiData.spriteBatch.DrawString(font, text, pos, color.Value, 0.0f, Vector2.Zero, scale, SpriteEffects.None,
                0.5f);
        }

        public static void doRightAlignedBackingLabel(Rectangle dest, string msg, SpriteFont font, Color back,
            Color front)
        {
            GuiData.spriteBatch.Draw(Utils.white, dest, back);
            var vector2 = font.MeasureString(msg);
            var position = new Vector2(dest.X + dest.Width - 4 - vector2.X, dest.Y + dest.Height - 2 - vector2.Y);
            GuiData.spriteBatch.DrawString(font, msg, position, front);
        }

        public static void doRightAlignedBackingLabelFill(Rectangle dest, string msg, SpriteFont font, Color back,
            Color front)
        {
            GuiData.spriteBatch.Draw(Utils.white, dest, back);
            var vector2_1 = font.MeasureString(msg);
            var dest1 = dest;
            dest1.Width -= 7;
            dest1.X += 4;
            var stringScaleForSize = GetStringScaleForSize(font, msg, dest1);
            var scale = stringScaleForSize[0];
            var vector2_2 = stringScaleForSize[1];
            vector2_2.Y *= 2f;
            vector2_2.Y -= 3f;
            var vector2_3 = new Vector2(dest1.X, dest.Y);
            GuiData.spriteBatch.DrawString(font, msg, vector2_3 + vector2_2, front, 0.0f, Vector2.Zero, scale,
                SpriteEffects.None, 0.6f);
            var destinationRectangle = new Rectangle(dest.X,
                (int) (dest.Y + (double) vector2_2.Y - 4.0 + vector2_1.Y*(double) scale.Y), dest.Width, 1);
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, front);
        }

        private static Vector2[] GetStringScaleForSize(SpriteFont font, string text, Rectangle dest)
        {
            var vector2_1 = font.MeasureString(text);
            var vector2_2 = new Vector2(dest.Width/vector2_1.X, dest.Height/vector2_1.Y);
            var num = Math.Min(vector2_2.X, vector2_2.Y);
            var zero = Vector2.Zero;
            if (vector2_2.X > (double) num)
            {
                zero.X = (float) (vector2_1.X*(vector2_2.X - (double) num)/2.0);
                vector2_2.X = num;
            }
            if (vector2_2.Y > (double) num)
            {
                zero.Y = (float) (vector2_1.Y*(vector2_2.Y - (double) num)/2.0);
                vector2_2.Y = num;
            }
            return new Vector2[2]
            {
                vector2_2,
                zero
            };
        }

        public static Vector2 doMeasuredFontLabel(Vector2 pos, string text, SpriteFont font, Color? color,
            float widthTo = 3.402823E+38f, float heightTo = 3.402823E+38f)
        {
            if (!color.HasValue)
                color = Color.White;
            var scale = font.MeasureString(text);
            scale.X = scale.X <= (double) widthTo ? 1f : widthTo/scale.X;
            scale.Y = scale.Y <= (double) heightTo ? 1f : heightTo/scale.Y;
            scale.X = scale.Y = Math.Min(scale.X, scale.Y);
            if (DrawShadow)
                GuiData.spriteBatch.DrawString(font, text, pos + Vector2.One, Color.Gray, 0.0f, Vector2.Zero, scale,
                    SpriteEffects.None, 0.5f);
            GuiData.spriteBatch.DrawString(font, text, pos, color.Value, 0.0f, Vector2.Zero, scale, SpriteEffects.None,
                0.5f);
            return font.MeasureString(text)*scale;
        }

        public static void doSmallLabel(Vector2 pos, string text, Color? color)
        {
            if (!color.HasValue)
                color = Color.White;
            if (DrawShadow)
                GuiData.spriteBatch.DrawString(GuiData.smallfont, text, pos + Vector2.One, Color.Gray);
            GuiData.spriteBatch.DrawString(GuiData.smallfont, text, pos, color.Value);
        }

        public static void doTinyLabel(Vector2 pos, string text, Color? color)
        {
            if (!color.HasValue)
                color = Color.White;
            GuiData.spriteBatch.DrawString(GuiData.tinyfont, text, pos, color.Value);
        }

        public static void doSmallLabel(Vector2 pos, string text, Color? color, float widthTo, float heightTo)
        {
            if (!color.HasValue)
                color = Color.White;
            var scale = GuiData.font.MeasureString(text);
            scale.X = scale.X <= (double) widthTo ? 1f : widthTo/scale.X;
            scale.Y = scale.Y <= (double) heightTo ? 1f : heightTo/scale.Y;
            if (DrawShadow)
                GuiData.spriteBatch.DrawString(GuiData.smallfont, text, pos + Vector2.One, Color.Gray, 0.0f,
                    Vector2.Zero, scale, SpriteEffects.None, 0.5f);
            GuiData.spriteBatch.DrawString(GuiData.smallfont, text, pos, color.Value, 0.0f, Vector2.Zero, scale,
                SpriteEffects.None, 0.5f);
        }

        public static Vector2 doMeasuredSmallLabel(Vector2 pos, string text, Color? color)
        {
            if (!color.HasValue)
                color = Color.White;
            var vector2 = GuiData.smallfont.MeasureString(text);
            if (DrawShadow)
                GuiData.spriteBatch.DrawString(GuiData.smallfont, text, pos + Vector2.One, Color.Gray);
            GuiData.spriteBatch.DrawString(GuiData.smallfont, text, pos, color.Value);
            return vector2;
        }

        public static Vector2 doMeasuredTinyLabel(Vector2 pos, string text, Color? color)
        {
            if (!color.HasValue)
                color = Color.White;
            var vector2 = GuiData.tinyfont.MeasureString(text);
            if (DrawShadow)
                GuiData.spriteBatch.DrawString(GuiData.tinyfont, text, pos + Vector2.One, Color.Gray);
            GuiData.spriteBatch.DrawString(GuiData.tinyfont, text, pos, color.Value);
            return vector2;
        }
    }
}
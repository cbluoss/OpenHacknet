using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    public static class FlickeringTextEffect
    {
        private static float internalTimer;
        private static RenderTarget2D LinedItemTarget;
        private static SpriteBatch LinedItemSB;

        public static void DrawFlickeringText(Rectangle dest, string text, float maxOffset, float rarity,
            SpriteFont font, object os_Obj, Color BaseCol)
        {
            OS os = null;
            if (os_Obj != null)
                os = (OS) os_Obj;
            else
                internalTimer += 0.01666667f;
            var color1 = new Color(BaseCol.R, 0, 0, 0);
            var color2 = new Color(0, BaseCol.G, 0, 0);
            var color3 = new Color(0, 0, BaseCol.B, 0);
            TextItem.doFontLabelToSize(
                RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(1.3f, 12.3f, rarity, os))), text, font,
                color1);
            TextItem.doFontLabelToSize(
                RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(0.8f, 29f, rarity, os))), text, font,
                color2);
            TextItem.doFontLabelToSize(
                RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(0.5f, -939.7f, rarity, os))), text, font,
                color3);
        }

        public static void DrawLinedFlickeringText(Rectangle dest, string text, float maxOffset, float rarity,
            SpriteFont font, object os_Obj, Color BaseCol, int segmentHeight = 2)
        {
            var graphicsDevice = GuiData.spriteBatch.GraphicsDevice;
            if (LinedItemTarget == null)
            {
                LinedItemTarget = new RenderTarget2D(graphicsDevice, 1920, 1080, false, SurfaceFormat.Rgba64,
                    DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                LinedItemSB = new SpriteBatch(graphicsDevice);
            }
            if (dest.Width > LinedItemTarget.Width || dest.Height > LinedItemTarget.Height)
                throw new InvalidOperationException("Target area is too large for the supported rendertarget size!");
            OS os = null;
            if (os_Obj != null)
                os = (OS) os_Obj;
            var dest1 = new Rectangle(0, 0, dest.Width, dest.Height);
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            graphicsDevice.SetRenderTarget(LinedItemTarget);
            graphicsDevice.Clear(Color.Transparent);
            var spriteBatch = GuiData.spriteBatch;
            LinedItemSB.Begin();
            GuiData.spriteBatch = LinedItemSB;
            DrawFlickeringText(dest1, text, maxOffset, rarity, font, os_Obj, BaseCol);
            LinedItemSB.End();
            GuiData.spriteBatch = spriteBatch;
            graphicsDevice.SetRenderTarget(currentRenderTarget);
            var num1 = (int) (dest.Height/19.0);
            var num2 = (int) (dest.Height/(double) (num1 + 1));
            var num3 = dest.Height/(double) (num1 + 1);
            for (var index = 0; index < num1 + 1; ++index)
            {
                var rectangle1 = new Rectangle(0, (int) (index*(double) num2 - segmentHeight), dest.Width,
                    num2 - segmentHeight);
                var destinationRectangle1 = new Rectangle(dest.X + dest1.X, dest.Y + rectangle1.Y, rectangle1.Width,
                    rectangle1.Height);
                GuiData.spriteBatch.Draw(LinedItemTarget, destinationRectangle1, rectangle1, Color.White);
                var rectangle2 = new Rectangle(0, rectangle1.Y + rectangle1.Height, rectangle1.Width, segmentHeight);
                var destinationRectangle2 = rectangle2;
                destinationRectangle2.X += dest.X;
                destinationRectangle2.Y += dest.Y;
                var num4 =
                    (int)
                        (maxOffset*1.20000004768372*
                         GetOffsetForSinTime((float) (1.29999995231628 + Math.Sin(index*2.5)), 12.3f*index, rarity, os));
                destinationRectangle2.X += num4;
                GuiData.spriteBatch.Draw(LinedItemTarget, destinationRectangle2, rectangle2, Color.White);
            }
        }

        public static void DrawFlickeringSprite(SpriteBatch sb, Rectangle dest, Texture2D texture, float maxOffset,
            float rarity, object os_Obj, Color BaseCol)
        {
            OS os = null;
            if (os_Obj != null)
                os = (OS) os_Obj;
            else
                internalTimer += 0.01666667f;
            var color1 = new Color(BaseCol.R, 0, 0, 0);
            var color2 = new Color(0, BaseCol.G, 0, 0);
            var color3 = new Color(0, 0, BaseCol.B, 0);
            sb.Draw(texture, RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(1.3f, 12.3f, rarity, os))),
                color1);
            sb.Draw(texture, RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(0.8f, 29f, rarity, os))),
                color2);
            sb.Draw(texture, RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(0.5f, -939.7f, rarity, os))),
                color3);
        }

        public static void DrawFlickeringSpriteAltWeightings(SpriteBatch sb, Rectangle dest, Texture2D texture,
            float maxOffset, float rarity, object os_Obj, Color BaseCol)
        {
            OS os = null;
            if (os_Obj != null)
                os = (OS) os_Obj;
            else
                internalTimer += 0.01666667f;
            var color1 = new Color(BaseCol.R, 0, 0, 0);
            var color2 = new Color(0, BaseCol.G, 0, 0);
            var color3 = new Color(0, 0, BaseCol.B, 0);
            sb.Draw(texture, RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(4f, 0.0f, rarity, os))),
                color1);
            sb.Draw(texture, RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(2f, 2f, rarity, os))), color2);
            sb.Draw(texture, RectAddX(dest, (int) (maxOffset*(double) GetOffsetForSinTime(0.5f, -4f, rarity, os))),
                color3);
        }

        public static float GetOffsetForSinTime(float frequency, float offset, float rarity, object os_Obj)
        {
            var range =
                (float) Math.Sin(((os_Obj != null ? ((OS) os_Obj).timer : internalTimer) + (double) offset)*frequency) -
                rarity;
            if (range < 0.0)
                range = 0.0f;
            return (float) (2.0*(0.5f + Utils.QuadraticOutCurve(Utils.randm(range)) - 0.5)) - range;
        }

        private static Rectangle RectAddX(Rectangle rect, int x)
        {
            rect.X += x;
            return rect;
        }

        public static string GetReportString()
        {
            return string.Concat("Target : ", LinedItemTarget, "\r\nTargetDisposed : ", LinedItemTarget.IsDisposed);
        }
    }
}
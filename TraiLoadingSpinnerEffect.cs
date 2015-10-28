using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class TraiLoadingSpinnerEffect
    {
        private readonly Texture2D circle;
        private SpriteBatch internalSB;
        private RenderTarget2D target;

        public TraiLoadingSpinnerEffect(OS operatingSystem)
        {
            circle = TextureBank.load("Circle", operatingSystem.content);
        }

        public void Draw(Rectangle bounds, SpriteBatch spriteBatch, float totalTime, float timeRemaining)
        {
            var destinationRectangle = new Rectangle(bounds.X + 2, bounds.Y + Module.PANEL_HEIGHT + 2, bounds.Width - 4,
                bounds.Height - 3 - Module.PANEL_HEIGHT);
            var flag = false;
            if (target == null)
            {
                target = new RenderTarget2D(spriteBatch.GraphicsDevice, destinationRectangle.Width,
                    destinationRectangle.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                    RenderTargetUsage.PreserveContents);
                internalSB = new SpriteBatch(spriteBatch.GraphicsDevice);
                flag = true;
            }
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            var dest = new Rectangle(0, 0, destinationRectangle.Width, destinationRectangle.Height);
            internalSB.Begin();
            if (flag)
                internalSB.GraphicsDevice.Clear(Color.Transparent);
            DrawLoading(timeRemaining, totalTime, dest, internalSB);
            internalSB.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(currentRenderTarget);
            spriteBatch.Draw(target, destinationRectangle, Color.White);
        }

        private void DrawLoading(float timeRemaining, float totalTime, Rectangle dest, SpriteBatch sb)
        {
            var loaderRadius = 20f;
            var loaderCentre = new Vector2(dest.X + dest.Width/2f, dest.Y + dest.Height/2f);
            var num1 = totalTime - timeRemaining;
            var num2 = num1/totalTime;
            sb.Draw(Utils.white, dest, Color.Black*0.05f);
            DrawLoadingCircle(timeRemaining, totalTime, dest, loaderCentre, loaderRadius, num1*0.2f, 1f, sb);
            DrawLoadingCircle(timeRemaining, totalTime, dest, loaderCentre, loaderRadius + 20f*num2, num1*-0.4f, 0.7f,
                sb);
            DrawLoadingCircle(timeRemaining, totalTime, dest, loaderCentre, loaderRadius + 35f*num2, num1*0.5f, 0.52f,
                sb);
        }

        private void DrawLoadingCircle(float timeRemaining, float totalTime, Rectangle dest, Vector2 loaderCentre,
            float loaderRadius, float baseRotationAdd, float rotationRateRPS, SpriteBatch sb)
        {
            var num1 = totalTime - timeRemaining;
            var num2 = 10;
            for (var index = 0; index < num2; ++index)
            {
                var num3 = index/(float) num2;
                var num4 = 2f;
                var num5 = 1f;
                var num6 = 6.283185f;
                var num7 = num6 + num5;
                var num8 = num3*num4;
                if (num1 > (double) num8)
                {
                    var num9 = num1/num8*rotationRateRPS%num7;
                    if (num9 >= (double) num6)
                        num9 = 0.0f;
                    var angle = num6*Utils.QuadraticOutCurve(num9/num6) + baseRotationAdd;
                    var vector2_1 = loaderCentre + Utils.PolarToCartesian(angle, loaderRadius);
                    sb.Draw(circle, vector2_1, new Rectangle?(), Utils.AddativeWhite, 0.0f, Vector2.Zero,
                        (float) (0.100000001490116*(loaderRadius/120.0)), SpriteEffects.None, 0.3f);
                    if (Utils.random.NextDouble() < 0.001)
                    {
                        var vector2_2 = loaderCentre + Utils.PolarToCartesian(angle, 20f + Utils.randm(45f));
                        sb.Draw(Utils.white, vector2_1, Utils.AddativeWhite);
                        Utils.drawLine(sb, vector2_1, vector2_2, Vector2.Zero, Utils.AddativeWhite*0.4f, 0.1f);
                    }
                }
            }
        }
    }
}
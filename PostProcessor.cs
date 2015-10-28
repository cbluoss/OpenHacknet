using Hacknet.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public static class PostProcessor
    {
        private static RenderTarget2D target;

        private static RenderTarget2D backTarget;

        private static RenderTarget2D dangerBufferTarget;

        private static GraphicsDevice device;

        private static SpriteBatch sb;

        private static Effect bloom;

        private static Effect blur;

        private static Effect danger;

        private static Color bloomColor;

        private static Color dangerLineColor;

        private static Color dangerLineColorAlt;

        private static Color bloomAbsenceHighlighterColor;

        public static bool bloomEnabled = true;

        public static bool scanlinesEnabled = true;

        public static bool dangerModeEnabled = false;

        public static float dangerModePercentComplete = 0f;

        public static bool EndingSequenceFlashOutActive = false;

        public static float EndingSequenceFlashOutPercentageComplete = 0f;

        public static void init(GraphicsDevice gDevice, SpriteBatch spriteBatch, ContentManager content)
        {
            device = gDevice;
            target = new RenderTarget2D(gDevice, gDevice.Viewport.Width, gDevice.Viewport.Height, false,
                SurfaceFormat.Rgba64, DepthFormat.None, 4, RenderTargetUsage.PlatformContents);
            backTarget = new RenderTarget2D(gDevice, gDevice.Viewport.Width, gDevice.Viewport.Height);
            dangerBufferTarget = new RenderTarget2D(gDevice, gDevice.Viewport.Width, gDevice.Viewport.Height);
            sb = spriteBatch;
            bloom = content.Load<Effect>("Shaders/Bloom");
            blur = content.Load<Effect>("Shaders/DOFBlur");
            danger = content.Load<Effect>("Shaders/DangerEffect");
            blur.CurrentTechnique = blur.Techniques["SmoothGaussBlur"];
            danger.CurrentTechnique = danger.Techniques["PostProcess"];
            bloomColor = new Color(90, 90, 90, 0);
            bloomAbsenceHighlighterColor = new Color(70, 70, 70, 0);
            dangerLineColor = new Color(255, 255, 255, 0);
            dangerLineColorAlt = new Color(240, 0, 0, 0);
        }

        public static void begin()
        {
            device.SetRenderTarget(target);
        }

        public static void end()
        {
            device.SetRenderTarget(backTarget);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                DepthStencilState.Default, RasterizerState.CullNone, blur);
            sb.Draw(target, Vector2.Zero, Color.White);
            sb.End();
            var renderTarget = dangerModeEnabled ? dangerBufferTarget : null;
            device.SetRenderTarget(renderTarget);
            if (EndingSequenceFlashOutActive)
            {
                device.Clear(Color.Black);
            }
            sb.Begin();
            var fullscreenRect = GetFullscreenRect();
            if (EndingSequenceFlashOutActive)
            {
                FlickeringTextEffect.DrawFlickeringSprite(sb, fullscreenRect, target, 12f, 0f, null, Color.White);
            }
            else
            {
                sb.Draw(target, fullscreenRect, Color.White);
            }
            if (bloomEnabled)
            {
                sb.Draw(backTarget, fullscreenRect, bloomColor);
            }
            else
            {
                sb.Draw(target, fullscreenRect, bloomAbsenceHighlighterColor);
            }
            sb.End();
            if (dangerModeEnabled)
            {
                DrawDangerModeFliters();
            }
        }

        public static string GetStatusReportString()
        {
            var arg = "Post Processor";
            arg = arg + "\r\n Target : " + target;
            arg = arg + "\r\n TargetDisposed : " + target.IsDisposed;
            arg = arg + "\r\n BTarget : " + backTarget;
            arg = arg + "\r\n BTargetDisposed : " + backTarget.IsDisposed;
            arg = arg + "\r\n DBTarget : " + dangerBufferTarget;
            return arg + "\r\n DBTargetDisposed : " + dangerBufferTarget.IsDisposed;
        }

        private static Rectangle GetFullscreenRect()
        {
            return new Rectangle(0, 0, target.Width, target.Height);
        }

        private static void DrawDangerModeFliters()
        {
            danger.Parameters["FlickerMultiplier"].SetValue(Utils.randm(0.4f));
            var y = (int) (dangerModePercentComplete*dangerBufferTarget.Height);
            danger.Parameters["PercentDown"].SetValue(dangerModePercentComplete);
            device.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                DepthStencilState.Default, RasterizerState.CullNone, danger);
            sb.Draw(dangerBufferTarget, Vector2.Zero, Color.White);
            sb.End();
            sb.Begin();
            var destinationRectangle = new Rectangle(0, y, dangerBufferTarget.Width, 1);
            sb.Draw(Utils.white, destinationRectangle, dangerLineColor*(Utils.randm(0.7f) + 0.3f));
            destinationRectangle.Y -= 1 + (Utils.flipCoin() ? 1 : 0);
            sb.Draw(Utils.white, destinationRectangle,
                Color.Lerp(dangerLineColor*0.4f, dangerLineColorAlt, Utils.randm(0.5f)));
            destinationRectangle.Y += 1 + (Utils.flipCoin() ? -2 : 0);
            sb.Draw(Utils.white, destinationRectangle,
                Color.Lerp(dangerLineColor*0.4f, dangerLineColorAlt, Utils.randm(0.5f)));
            sb.End();
        }
    }
}
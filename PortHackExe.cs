using System;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class PortHackExe : ExeModule
    {
        public static float CRACK_TIME = 6f;
        public static float TIME_BETWEEN_TEXT_SWITCH = 0.06f;
        public static float TIME_ALIVE_AFTER_SUCSESS = 5f;
        private readonly PortHackCubeSequence cubeSeq = new PortHackCubeSequence();
        private bool hasCheckedForheart;
        private bool hasCompleted;
        private bool IsTargetingPorthackHeart;
        private float progress;
        private RenderTarget2D renderTarget;
        private float sucsessTimer = TIME_ALIVE_AFTER_SUCSESS;
        private Computer target;
        private int[] textIndex;
        private int textOffsetIndex;
        private float textSwitchTimer = TIME_BETWEEN_TEXT_SWITCH;

        public PortHackExe(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            IdentifierName = "PortHack";
        }

        public override void LoadContent()
        {
            base.LoadContent();
            var num = PortExploits.passwords.Count/3;
            textIndex = new int[3];
            textIndex[0] = 0;
            textIndex[1] = num;
            textIndex[2] = 2*num;
            target = Programs.getComputer(os, targetIP);
            os.write("Porthack Initialized -- Running...");
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (IsTargetingPorthackHeart)
                progress = Utils.rand(0.98f);
            else
                progress += t/CRACK_TIME;
            if (progress >= 1.0)
            {
                progress = 1f;
                if (!hasCompleted)
                {
                    Completed();
                    hasCompleted = true;
                }
                sucsessTimer -= t;
                if (sucsessTimer <= 0.0)
                    isExiting = true;
            }
            else
                target.hostileActionTaken();
            if (progress < 1.0)
            {
                textSwitchTimer -= t;
                if (textSwitchTimer <= 0.0)
                {
                    textSwitchTimer = TIME_BETWEEN_TEXT_SWITCH;
                    ++textOffsetIndex;
                }
            }
            if (hasCheckedForheart || progress <= 0.5)
                return;
            hasCheckedForheart = true;
            var porthackHeartDaemon = target.getDaemon(typeof (PorthackHeartDaemon)) as PorthackHeartDaemon;
            if (porthackHeartDaemon == null)
                return;
            IsTargetingPorthackHeart = true;
            porthackHeartDaemon.BreakHeart();
            cubeSeq.ShouldCentralSpinInfinitley = true;
        }

        public override void Completed()
        {
            base.Completed();
            os.takeAdmin(targetIP);
            os.write("--Porthack Complete--");
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            var destinationRectangle1 = new Rectangle(bounds.X + 1, bounds.Y + PANEL_HEIGHT + 1, bounds.Width - 2,
                bounds.Height - (PANEL_HEIGHT + 2));
            if (renderTarget == null)
                renderTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, destinationRectangle1.Width,
                    destinationRectangle1.Height);
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            try
            {
                cubeSeq.DrawSequence(new Rectangle(0, 0, destinationRectangle1.Width, destinationRectangle1.Height), t,
                    CRACK_TIME);
            }
            catch (Exception ex)
            {
                Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex) + "\r\n\r\n");
            }
            spriteBatch.GraphicsDevice.SetRenderTarget(currentRenderTarget);
            var rectangle = bounds;
            ++rectangle.X;
            ++rectangle.Y;
            rectangle.Width -= 2;
            rectangle.Height -= 2;
            drawOutline();
            spriteBatch.Draw(renderTarget, destinationRectangle1, Utils.AddativeWhite*(progress >= 1.0 ? 0.2f : 0.6f));
            drawTarget("app:");
            if (progress < 1.0)
            {
                var destinationRectangle2 = new Rectangle(bounds.X, bounds.Y + PANEL_HEIGHT + 1,
                    (int) (bounds.Width/2.0), bounds.Height - (PANEL_HEIGHT + 2));
                spriteBatch.Draw(Utils.gradientLeftRight, destinationRectangle2, new Rectangle?(), Color.Black*0.9f,
                    0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0.2f);
                destinationRectangle2.X += bounds.Width - destinationRectangle2.Width - 1;
                spriteBatch.Draw(Utils.gradientLeftRight, destinationRectangle2, new Rectangle?(), Color.Black*0.9f,
                    0.0f, Vector2.Zero, SpriteEffects.None, 0.3f);
            }
            var num1 = (bounds.Height - 16 - 16 - 4)/12;
            var position = new Vector2(bounds.X + 3, bounds.Y + 20);
            var color = Color.White*fade;
            if (IsTargetingPorthackHeart)
                color *= 0.5f;
            if (progress >= 1.0)
                color = Color.Gray*fade;
            for (var index1 = 0; index1 < num1; ++index1)
            {
                var index2 = (textIndex[0] + textOffsetIndex + index1)%(PortExploits.passwords.Count - 1);
                spriteBatch.DrawString(GuiData.UITinyfont, PortExploits.passwords[index2], position, color);
                var index3 = (textIndex[1] + textOffsetIndex + index1)%(PortExploits.passwords.Count - 1);
                var vector2 = GuiData.UITinyfont.MeasureString(PortExploits.passwords[index3]);
                position.X = (float) (bounds.X + bounds.Width - (double) vector2.X - 3.0);
                var destinationRectangle2 = new Rectangle((int) position.X - 1, (int) position.Y, (int) vector2.X, 12);
                spriteBatch.Draw(Utils.white, destinationRectangle2, Color.Black*0.8f);
                spriteBatch.DrawString(GuiData.UITinyfont, PortExploits.passwords[index3], position, color);
                position.X = bounds.X + 3;
                position.Y += 12f;
            }
            if (progress >= 1.0)
            {
                var str1 = "PASSWORD";
                var vector2_1 = GuiData.font.MeasureString(str1);
                position.X = bounds.X + bounds.Width/2 - vector2_1.X/2f;
                position.Y = bounds.Y + bounds.Height/2 - vector2_1.Y;
                var num2 = Utils.QuadraticOutCurve(Math.Min(2f, TIME_ALIVE_AFTER_SUCSESS - sucsessTimer)/2f);
                var destinationRectangle2 = new Rectangle(0, (int) position.Y - 3,
                    (int) (num2*(double) bounds.Width - 30.0), 2);
                destinationRectangle2.X = bounds.X + (bounds.Width/2 - destinationRectangle2.Width/2);
                if (destinationRectangle2.Y > bounds.Y)
                    spriteBatch.Draw(Utils.white, destinationRectangle2, Utils.AddativeWhite*fade);
                spriteBatch.DrawString(GuiData.font, Utils.FlipRandomChars(str1, 0.012), position, Color.White*fade);
                spriteBatch.DrawString(GuiData.font, Utils.FlipRandomChars(str1, 0.11), position,
                    Utils.AddativeWhite*0.2f*fade*fade);
                var str2 = "FOUND";
                var vector2_2 = GuiData.font.MeasureString(str2);
                position.X = bounds.X + bounds.Width/2 - vector2_2.X/2f;
                position.Y += 35f;
                spriteBatch.DrawString(GuiData.font, Utils.FlipRandomChars(str2, 0.012), position, Color.White*fade);
                spriteBatch.DrawString(GuiData.font, Utils.FlipRandomChars(str2, 0.11), position,
                    Utils.AddativeWhite*0.2f*fade*fade);
                destinationRectangle2.Y = (int) (position.Y + 35.0 + 3.0);
                if (destinationRectangle2.Y > bounds.Y)
                    spriteBatch.Draw(Utils.white, destinationRectangle2, Utils.AddativeWhite*fade);
            }
            rectangle.X += 2;
            rectangle.Width -= 4;
            rectangle.Y = bounds.Y + bounds.Height - 2 - 16;
            rectangle.Height = 16;
            spriteBatch.Draw(Utils.white, rectangle, os.outlineColor*fade);
            ++rectangle.X;
            ++rectangle.Y;
            rectangle.Width -= 2;
            rectangle.Height -= 2;
            if (IsTargetingPorthackHeart)
            {
                spriteBatch.Draw(Utils.white, rectangle, Color.DarkRed*(Utils.rand(0.3f) + 0.7f)*fade);
                TextItem.doFontLabelToSize(rectangle, "UNKNOWN ERROR", GuiData.font, Utils.AddativeWhite);
            }
            else
            {
                spriteBatch.Draw(Utils.white, rectangle, os.darkBackgroundColor*fade);
                rectangle.Width = (int) (rectangle.Width*(double) progress);
                spriteBatch.Draw(Utils.white, rectangle, os.highlightColor*fade);
            }
        }
    }
}
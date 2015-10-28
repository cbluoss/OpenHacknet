using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class SMTPoverflowExe : ExeModule
    {
        public static float DURATION = 12f;
        public static float BAR_MOVEMENT = 30f;
        public static float BAR_HEIGHT = 2f;
        private readonly Color activeBarColor = new Color(34, 82, 64, byte.MaxValue);
        private readonly Color activeBarHighlightColor = new Color(0, 186, 99, 0);
        private float barSize;
        private int completedIndex;
        public bool hasCompleted;
        private List<Vector2> leftBars;
        public float progress;
        private List<Vector2> rightBars;
        private float sucsessTimer = 0.5f;
        private float timeAccum;

        public SMTPoverflowExe(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            needsProxyAccess = true;
            ramCost = 356;
            IdentifierName = "SMTP Overflow";
            activeBarColor = os.unlockedColor;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            barSize = BAR_HEIGHT + 1.4f;
            var capacity = (int) (bounds.Height/(double) barSize);
            var maxValue = bounds.Width/2 - 1;
            leftBars = new List<Vector2>(capacity);
            rightBars = new List<Vector2>(capacity);
            for (var index = 0; index < capacity; ++index)
            {
                leftBars.Add(new Vector2(Utils.random.Next(0, maxValue), Utils.random.Next(0, maxValue)));
                rightBars.Add(new Vector2(Utils.random.Next(0, maxValue), Utils.random.Next(0, maxValue)));
            }
            barSize = BAR_HEIGHT;
            Programs.getComputer(os, targetIP).hostileActionTaken();
        }

        public override void Update(float t)
        {
            base.Update(t);
            timeAccum += t*5f;
            progress += t/DURATION;
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
            var zero = Vector2.Zero;
            for (var index = 0; index < leftBars.Count; ++index)
            {
                if (index > completedIndex)
                {
                    var vector2 = leftBars[index];
                    vector2.X += (float) Math.Sin(timeAccum + (double) (index*index))*t*BAR_MOVEMENT;
                    vector2.Y += (float) Math.Sin(timeAccum + (double) index)*t*BAR_MOVEMENT;
                    leftBars[index] = vector2;
                    vector2 = rightBars[index];
                    vector2.X += (float) Math.Sin(timeAccum + (double) (index*index))*t*BAR_MOVEMENT;
                    vector2.Y += (float) Math.Sin(timeAccum + (double) index)*t*BAR_MOVEMENT;
                    rightBars[index] = vector2;
                }
            }
            completedIndex = (int) (progress*(double) (leftBars.Count - 1));
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            spriteBatch.DrawString(GuiData.UISmallfont, "SMTP Mail Server Overflow",
                new Vector2(bounds.X + 5, bounds.Y + 12), os.subtleTextColor);
            var vector2_1 = new Vector2(bounds.X + 2, bounds.Y + 57);
            var num = (bounds.Width - 4)/2;
            var rectangle1 = new Rectangle(bounds.X + 2, bounds.Y + 38, (bounds.Width - 4)/2, (int) barSize);
            var rectangle2 = new Rectangle(bounds.X + 2, bounds.Y + 38, (bounds.Width - 4)/2, (int) barSize);
            var flag1 = false;
            Vector2 vector2_2;
            for (var index = 0; index < leftBars.Count; ++index)
            {
                flag1 = index <= completedIndex;
                vector2_2 = leftBars[index];
                rectangle1.Width = (int) vector2_2.Y;
                spriteBatch.Draw(Utils.white, rectangle1,
                    (flag1
                        ? Color.Lerp(os.outlineColor, Utils.VeryDarkGray, Utils.randm(0.2f)*Utils.randm(1f))
                        : os.subtleTextColor)*fade);
                rectangle1.Y += (int) barSize + 1;
                vector2_2 = rightBars[index];
                rectangle2.Width = (int) vector2_2.Y;
                rectangle2.X = bounds.X + bounds.Width - 4 - (int) vector2_2.Y;
                spriteBatch.Draw(Utils.white, rectangle2,
                    (flag1
                        ? Color.Lerp(os.outlineColor, Utils.VeryDarkGray, Utils.randm(0.2f)*Utils.randm(1f))
                        : os.subtleTextColor)*fade);
                rectangle2.Y += (int) barSize + 1;
                if (rectangle2.Y + rectangle2.Height >= bounds.Y + bounds.Height)
                    break;
            }
            rectangle1 = new Rectangle(bounds.X + 2, bounds.Y + 38, (bounds.Width - 4)/2, (int) barSize);
            rectangle2 = new Rectangle(bounds.X + 2, bounds.Y + 38, (bounds.Width - 4)/2, (int) barSize);
            var flag2 = flag1;
            for (var index = 0; index < leftBars.Count; ++index)
            {
                var barActive = index <= completedIndex || flag2;
                vector2_2 = leftBars[index];
                rectangle1.Width = (int) vector2_2.X;
                DrawBar(rectangle1, barActive, true);
                rectangle1.Y += (int) barSize + 1;
                vector2_2 = rightBars[index];
                rectangle2.Width = (int) vector2_2.X;
                rectangle2.X = bounds.X + bounds.Width - 4 - (int) vector2_2.X;
                DrawBar(rectangle2, barActive, false);
                rectangle2.Y += (int) barSize + 1;
                if (rectangle2.Y + rectangle2.Height >= bounds.Y + bounds.Height)
                    break;
            }
        }

        private void DrawBar(Rectangle dest, bool barActive, bool isLeft)
        {
            var num1 = Math.Min(1f, dest.Width/(bounds.Width/2f));
            spriteBatch.Draw(Utils.white, dest,
                (hasCompleted
                    ? Color.Lerp(os.highlightColor, Utils.AddativeWhite, Math.Max(0.0f, sucsessTimer))
                    : (barActive ? activeBarColor : Color.White))*fade);
            if (!barActive)
                return;
            var num2 = dest.Height;
            dest.Height = 1;
            spriteBatch.Draw(Utils.gradientLeftRight, dest, new Rectangle?(), activeBarHighlightColor*0.6f*fade*num1,
                0.0f, Vector2.Zero, isLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.3f);
            ++dest.Y;
            spriteBatch.Draw(Utils.gradientLeftRight, dest, new Rectangle?(), activeBarHighlightColor*0.2f*fade*num1,
                0.0f, Vector2.Zero, isLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.3f);
            --dest.Y;
            dest.Height = num2;
        }

        public override void Completed()
        {
            base.Completed();
            var computer = Programs.getComputer(os, targetIP);
            if (computer == null)
                return;
            computer.openPort(25, os.thisComputer.ip);
        }
    }
}
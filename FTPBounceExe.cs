using System;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class FTPBounceExe : ExeModule
    {
        public static float DURATION = 17f;
        public static float SCROLL_RATE = 0.08f;
        private byte[] acceptedBinary1;
        private byte[] acceptedBinary2;
        private string binary;
        private int binaryChars;
        private int binaryIndex;
        private float binaryScrollTimer = SCROLL_RATE;
        private bool complete;
        private float progress;
        private float timeLeft = DURATION;
        private int unlockedChars1;
        private int unlockedChars2;
        private int[] unlockOrder;

        public FTPBounceExe(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            needsProxyAccess = true;
            ramCost = 210;
            IdentifierName = "FTP Bounce";
        }

        public override void LoadContent()
        {
            base.LoadContent();
            binary = Computer.generateBinaryString(1024);
            var num1 = 12;
            binaryChars = (bounds.Width - 4)/num1;
            acceptedBinary1 = new byte[binaryChars];
            acceptedBinary2 = new byte[binaryChars];
            for (var index = 0; index < num1; ++index)
            {
                acceptedBinary1[index] = Utils.random.NextDouble() > 0.5 ? (byte) 0 : (byte) 1;
                acceptedBinary2[index] = Utils.random.NextDouble() > 0.5 ? (byte) 0 : (byte) 1;
            }
            unlockOrder = new int[binaryChars];
            for (var index = 0; index < binaryChars; ++index)
                unlockOrder[index] = index;
            var num2 = 300;
            for (var index1 = 0; index1 < num2; ++index1)
            {
                var index2 = Utils.random.Next(0, unlockOrder.Length - 1);
                var index3 = Utils.random.Next(0, unlockOrder.Length - 1);
                var num3 = unlockOrder[index2];
                unlockOrder[index2] = unlockOrder[index3];
                unlockOrder[index3] = num3;
            }
            Programs.getComputer(os, targetIP).hostileActionTaken();
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (!complete)
            {
                binaryScrollTimer -= t;
                if (binaryScrollTimer <= 0.0)
                {
                    ++binaryIndex;
                    binaryScrollTimer = SCROLL_RATE;
                }
            }
            timeLeft -= t;
            if (timeLeft <= 0.0 && !complete)
            {
                complete = true;
                Completed();
            }
            progress = Math.Min(Math.Abs((float) (1.0 - timeLeft/(double) DURATION)), 1f);
            unlockedChars1 = Math.Min((int) (binaryChars*progress*2.0), binaryChars);
            unlockedChars2 = Math.Min((int) (binaryChars*(double) progress), binaryChars);
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            var position = new Vector2(bounds.X + 6, bounds.Y + 12);
            for (var index1 = 0; index1 < 6; ++index1)
            {
                for (var index2 = 0; index2 < binaryChars; ++index2)
                {
                    spriteBatch.DrawString(GuiData.UITinyfont,
                        string.Concat(binary[(binaryIndex + index2 + index1*20)%(binary.Length - 1)]), position,
                        Color.White);
                    position.X += 12f;
                }
                position.Y += 12f;
                position.X = bounds.X + 6;
                if (position.Y - (double) bounds.Y + 24.0 > bounds.Height)
                    return;
            }
            position.Y += 16f;
            if (position.Y - (double) bounds.Y + 24.0 > bounds.Height)
                return;
            spriteBatch.DrawString(GuiData.UISmallfont, "Working ::", position, os.subtleTextColor);
            position.Y += 20f;
            var destinationRectangle = new Rectangle((int) position.X, (int) position.Y, bounds.Width - 12, 80);
            destinationRectangle.Height =
                (int) Math.Min(destinationRectangle.Height, bounds.Height - (position.Y - bounds.Y));
            spriteBatch.Draw(Utils.white, destinationRectangle, (complete ? os.unlockedColor : os.lockedColor)*fade);
            if (position.Y - (double) bounds.Y + 80.0 > bounds.Height)
                return;
            for (var index1 = 0; index1 < unlockedChars1; ++index1)
            {
                var index2 = unlockOrder[index1];
                var index3 = unlockOrder[binaryChars - index1 - 1];
                position.X = bounds.X + 6 + 12*index2;
                spriteBatch.DrawString(GuiData.UISmallfont, string.Concat(acceptedBinary1[index2]), position,
                    Color.White*fade);
                spriteBatch.DrawString(GuiData.UISmallfont, string.Concat(acceptedBinary1[index3]),
                    position + new Vector2(0.0f, 32f), Color.White*fade);
            }
            position.Y += 16f;
            for (var index1 = 0; index1 < unlockedChars2; ++index1)
            {
                var index2 = unlockOrder[binaryChars - index1 - 1];
                var index3 = unlockOrder[index1];
                position.X = bounds.X + 6 + 12*index2;
                spriteBatch.DrawString(GuiData.UISmallfont, string.Concat(acceptedBinary2[index2]), position,
                    Color.White*fade);
                spriteBatch.DrawString(GuiData.UISmallfont, string.Concat(acceptedBinary2[index3]),
                    position + new Vector2(0.0f, 32f), Color.White*fade);
            }
        }

        public override void Completed()
        {
            base.Completed();
            var computer = Programs.getComputer(os, targetIP);
            if (computer != null)
                computer.openPort(21, os.thisComputer.ip);
            isExiting = true;
        }
    }
}
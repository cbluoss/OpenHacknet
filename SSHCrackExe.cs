using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class SSHCrackExe : ExeModule
    {
        public static float DURATION = 8f;

        private static readonly float GRID_REVEAL_DELAY = 0.6f;

        private static readonly float ENDING_FLASH = 0.7f;

        private static readonly float SHEEN_FLASH_DELAY = 0.03f;

        private bool complete;

        private SSHCrackGridEntry[,] Grid;

        private int GridEntryHeight;

        private int GridEntryWidth;

        private bool GridShowingSheen;

        private int height;

        private float timeLeft = DURATION;

        private Color unlockedFlashColor;

        private int width;

        public SSHCrackExe(Rectangle location, OS operatingSystem) : base(location, operatingSystem)
        {
            needsProxyAccess = true;
            IdentifierName = "SecureShellCrack";
            ramCost = 242;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            GridEntryWidth = 31;
            GridEntryHeight = 15;
            width = (int) ((bounds.Width - 4f)/GridEntryWidth);
            height = (int) ((bounds.Height - 24f)/GridEntryHeight);
            Grid = new SSHCrackGridEntry[width, height];
            var num = 0;
            var num2 = 0f;
            var num3 = DURATION - GRID_REVEAL_DELAY - ENDING_FLASH;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var num4 = Math.Max(Math.Abs(j - i/2), Math.Abs(j + i/2));
                    var num5 = i/2 + num4;
                    var num6 = num3 - num2;
                    if (num%2 == 0)
                    {
                        num6 = Utils.randm(num3);
                        num2 = num6;
                    }
                    Grid[j, i] = new SSHCrackGridEntry
                    {
                        TimeSinceActivated = 0f,
                        CurrentValue = Utils.getRandomByte(),
                        TimeTillActive = num5*SHEEN_FLASH_DELAY,
                        TimeTillSolved = num6
                    };
                    num++;
                }
            }
            unlockedFlashColor = Color.Lerp(os.unlockedColor, os.brightUnlockedColor, 0.4f);
            var computer = Programs.getComputer(os, targetIP);
            computer.hostileActionTaken();
            os.write("SecureShellCrack Running...");
        }

        public override void Update(float t)
        {
            base.Update(t);
            timeLeft -= t;
            if (timeLeft <= 0f && !complete)
            {
                complete = true;
                Completed();
                isExiting = true;
            }
            if (timeLeft < GRID_REVEAL_DELAY && GRID_REVEAL_DELAY - timeLeft <= t)
            {
                GridShowingSheen = true;
                for (var i = 0; i < height; i++)
                {
                    for (var j = 0; j < width; j++)
                    {
                        var num = Math.Max(Math.Abs(j - i/2), Math.Abs(j + i/2));
                        var num2 = i/2 + num;
                        var sSHCrackGridEntry = Grid[j, i];
                        sSHCrackGridEntry.TimeSinceActivated = -1f*num2*SHEEN_FLASH_DELAY;
                        Grid[j, i] = sSHCrackGridEntry;
                    }
                }
            }
            var num3 = DURATION - timeLeft;
            for (var k = 0; k < height; k++)
            {
                for (var l = 0; l < width; l++)
                {
                    var sSHCrackGridEntry2 = Grid[l, k];
                    sSHCrackGridEntry2.TimeTillActive -= t;
                    if (Utils.randm(0.5f) <= t)
                    {
                        sSHCrackGridEntry2.CurrentValue = Utils.getRandomByte();
                    }
                    if (sSHCrackGridEntry2.TimeTillActive <= 0f)
                    {
                        sSHCrackGridEntry2.TimeTillActive = 0f;
                        sSHCrackGridEntry2.TimeSinceActivated += t;
                        if (num3 > GRID_REVEAL_DELAY)
                        {
                            sSHCrackGridEntry2.TimeTillSolved -= t;
                            if (sSHCrackGridEntry2.TimeTillSolved <= 0f)
                            {
                                sSHCrackGridEntry2.CurrentValue = 0;
                            }
                        }
                    }
                    Grid[l, k] = sSHCrackGridEntry2;
                }
            }
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            var arg_0C_0 = Rectangle.Empty;
            drawOutline();
            drawTarget("app:");
            TextItem.doFontLabel(new Vector2(bounds.X + 2, bounds.Y + 14),
                complete ? "Operation Complete" : "SSH Crack in operation...", GuiData.UITinyfont,
                Utils.AddativeWhite*0.8f*fade, bounds.Width - 6, 3.40282347E+38f);
            var num = bounds.Y + 30;
            var num2 = bounds.X + 2;
            var num3 = 0.4f;
            var rectangle = new Rectangle(bounds.X + 2, bounds.Y + 16, GridEntryWidth - 2, GridEntryHeight - 2);
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var sSHCrackGridEntry = Grid[i, j];
                    rectangle.X = num2 + i*GridEntryWidth;
                    rectangle.Y = num + j*GridEntryHeight;
                    if (rectangle.Y + 1 <= bounds.Y + bounds.Height && sSHCrackGridEntry.TimeTillActive <= 0f)
                    {
                        var value = os.lockedColor;
                        if (sSHCrackGridEntry.TimeSinceActivated < num3)
                        {
                            var amount = sSHCrackGridEntry.TimeSinceActivated/num3;
                            value = Color.Lerp(os.brightLockedColor, os.lockedColor, amount);
                        }
                        if (sSHCrackGridEntry.TimeTillSolved <= 0f)
                        {
                            value = os.unlockedColor;
                            if (sSHCrackGridEntry.TimeTillSolved > -1f*num3)
                            {
                                var amount2 = -1f*sSHCrackGridEntry.TimeTillSolved/num3;
                                value = Color.Lerp(os.unlockedColor, unlockedFlashColor, amount2);
                            }
                        }
                        if (GridShowingSheen)
                        {
                            var num4 = 0f;
                            if (sSHCrackGridEntry.TimeSinceActivated >= 0f)
                            {
                                num4 = sSHCrackGridEntry.TimeSinceActivated/(num3/2f);
                                if (sSHCrackGridEntry.TimeSinceActivated > num3/2f)
                                {
                                    num4 = 1f - Math.Min(1f, (sSHCrackGridEntry.TimeSinceActivated - num3/2f)/(num3/2f));
                                }
                            }
                            if (num4 < 0.25f)
                            {
                                num4 = 0f;
                            }
                            else if (num4 < 0.75f)
                            {
                                num4 = 0.5f;
                            }
                            value = Color.Lerp(os.unlockedColor, unlockedFlashColor, num4);
                        }
                        var destinationRectangle = rectangle;
                        var flag = true;
                        if (rectangle.Y + rectangle.Height > bounds.Y + bounds.Height)
                        {
                            destinationRectangle.Height = rectangle.Height -
                                                          (rectangle.Y + rectangle.Height - (bounds.Y + bounds.Height));
                            flag = false;
                        }
                        spriteBatch.Draw(Utils.white, destinationRectangle, value*fade);
                        var num5 = (sSHCrackGridEntry.CurrentValue >= 10)
                            ? ((sSHCrackGridEntry.CurrentValue >= 100) ? 1f : 5f)
                            : 8f;
                        if (flag)
                        {
                            spriteBatch.DrawString(GuiData.UITinyfont, string.Concat(sSHCrackGridEntry.CurrentValue),
                                new Vector2(rectangle.X + num5, rectangle.Y - 1.5f), Color.White*fade);
                        }
                    }
                }
            }
        }

        public override void Completed()
        {
            base.Completed();
            var computer = Programs.getComputer(os, targetIP);
            if (computer != null)
            {
                computer.openPort(22, os.thisComputer.ip);
                os.write("-- SecureShellCrack Complete --");
            }
        }

        private struct SSHCrackGridEntry
        {
            public float TimeTillActive;

            public float TimeTillSolved;

            public float TimeSinceActivated;

            public byte CurrentValue;
        }
    }
}
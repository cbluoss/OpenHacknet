using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class SongChangerDaemon : Daemon
    {
        private readonly MovingBarsEffect botEffect;
        private readonly MovingBarsEffect topEffect;

        public SongChangerDaemon(Computer c, OS os)
            : base(c, "Music Changer", os)
        {
            topEffect = new MovingBarsEffect();
            botEffect = new MovingBarsEffect
            {
                IsInverted = true
            };
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            topEffect.Update((float) os.lastGameTime.ElapsedGameTime.TotalSeconds);
            botEffect.Update((float) os.lastGameTime.ElapsedGameTime.TotalSeconds);
            var height = 30;
            var rectangle1 = new Rectangle(bounds.X + 30, bounds.Y + bounds.Height/2 - height/2, bounds.Width - 60,
                height);
            var bounds1 = rectangle1;
            bounds1.Height = 60;
            bounds1.Y -= bounds1.Height;
            topEffect.Draw(sb, bounds1, 1f, 3f, 1f, os.highlightColor);
            if (Button.doButton(73518921, rectangle1.X, rectangle1.Y, rectangle1.Width, rectangle1.Height,
                "Shuffle Music", os.highlightColor))
            {
                var maxValue = 12;
                MissionFunctions.runCommand(Utils.random.Next(maxValue) + 1, "changeSong");
            }
            bounds1.Y += bounds1.Height + height;
            botEffect.Draw(sb, bounds1, 1f, 3f, 1f, os.highlightColor);
            var rectangle2 = new Rectangle(bounds.X + 4, bounds.Y + bounds.Height - 4 - 20, (int) (bounds.Width*0.5), 20);
            if (
                !Button.doButton(73518924, rectangle2.X, rectangle2.Y, rectangle2.Width, rectangle2.Height, "Exit",
                    os.lockedColor))
                return;
            os.display.command = "connect";
        }

        public override string getSaveString()
        {
            return "<SongChangerDaemon />";
        }
    }
}
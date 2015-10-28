using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class AvconDemoEndDaemon : Daemon
    {
        private bool confirmed;

        public AvconDemoEndDaemon(Computer c, string name, OS os)
            : base(c, name, os)
        {
            this.name = "Complete Demo";
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            confirmed = false;
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            if (Button.doButton(byte.MaxValue, bounds.X + bounds.Width/4, bounds.Y + bounds.Height/4, bounds.Width/2, 35,
                "Back", new Color?()))
                os.display.command = "connect";
            if (
                !Button.doButton(258, bounds.X + bounds.Width/4, bounds.Y + bounds.Height/4 + 40, bounds.Width/2, 35,
                    !confirmed ? "End Session" : "Confirm End", confirmed ? Color.Red : Color.DarkRed))
                return;
            if (confirmed)
                endDemo();
            else
                confirmed = true;
        }

        public override string getSaveString()
        {
            return "<addAvconDemoEndDaemon name=\"" + name + "\"/>";
        }

        private void endDemo()
        {
            os.ScreenManager.AddScreen(new MainMenu());
            os.ExitScreen();
        }
    }
}
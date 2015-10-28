using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class SecurityTraceExe : ExeModule
    {
        public SecurityTraceExe(Rectangle location, OS _os)
            : base(location, _os)
        {
            name = "SecurityTrace";
            IdentifierName = "Security Tracer";
            ramCost = 150;
            if (os.connectedComp == null)
                os.connectedComp = os.thisComputer;
            os.traceTracker.start(30f);
            os.warningFlash();
        }

        public override void Killed()
        {
            base.Killed();
            os.traceTracker.stop();
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            var dest = bounds;
            dest.X += 2;
            dest.Width -= 4;
            dest.Height -= PANEL_HEIGHT + 1;
            dest.Y += PANEL_HEIGHT;
            PatternDrawer.draw(dest, -1f, Color.Transparent, os.darkBackgroundColor, spriteBatch);
            PatternDrawer.draw(dest, 3f, Color.Transparent, os.lockedColor, spriteBatch, PatternDrawer.errorTile);
        }
    }
}
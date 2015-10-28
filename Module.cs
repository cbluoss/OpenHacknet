using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class Module
    {
        public static int PANEL_HEIGHT = 15;
        private static Rectangle tmpRect;
        public Rectangle bounds;
        public string name = "Unknown";
        public OS os;
        public SpriteBatch spriteBatch;
        public bool visible = true;

        public Module(Rectangle location, OS operatingSystem)
        {
            location.Y += PANEL_HEIGHT;
            location.Height -= PANEL_HEIGHT;
            bounds = location;
            os = operatingSystem;
            spriteBatch = os.ScreenManager.SpriteBatch;
        }

        public Rectangle Bounds
        {
            get { return bounds; }
            set
            {
                bounds = value;
                bounds.Y += PANEL_HEIGHT;
                bounds.Height -= PANEL_HEIGHT;
            }
        }

        public virtual void LoadContent()
        {
        }

        public virtual void Update(float t)
        {
        }

        public virtual void PreDrawStep()
        {
        }

        public virtual void Draw(float t)
        {
            drawFrame();
        }

        public virtual void PostDrawStep()
        {
        }

        public void drawFrame()
        {
            tmpRect = bounds;
            tmpRect.Y -= PANEL_HEIGHT;
            tmpRect.Height += PANEL_HEIGHT;
            spriteBatch.Draw(Utils.white, tmpRect, os.moduleColorBacking);
            RenderedRectangle.doRectangleOutline(tmpRect.X, tmpRect.Y, tmpRect.Width, tmpRect.Height, 1,
                os.moduleColorSolid);
            tmpRect.Height = PANEL_HEIGHT;
            spriteBatch.Draw(Utils.white, tmpRect, os.moduleColorStrong);
            spriteBatch.DrawString(GuiData.detailfont, name, new Vector2(tmpRect.X + 2, tmpRect.Y + 2), os.semiTransText);
        }
    }
}
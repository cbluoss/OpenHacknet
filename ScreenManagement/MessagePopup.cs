using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet.ScreenManagement
{
    internal class MessagePopup : GameScreen
    {
        private static readonly int BOX_WIDTH = 500;
        private static readonly int BOX_HEIGHT = 350;
        private Texture2D blankTexture;
        private readonly string messageContent = "";

        public MessagePopup(string message)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            messageContent = message;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
        }

        public override void HandleInput(InputState input)
        {
            var state = Keyboard.GetState();
            if (!state.IsKeyDown(Keys.Space) && !state.IsKeyDown(Keys.Up) &&
                (!state.IsKeyDown(Keys.Z) && !state.IsKeyDown(Keys.Left)) &&
                (!state.IsKeyDown(Keys.Right) && !state.IsKeyDown(Keys.X)))
                return;
            ExitScreen();
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = ScreenManager.SpriteBatch;
            var viewport = ScreenManager.GraphicsDevice.Viewport;
            var rectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);
            var destinationRectangle = new Rectangle(viewport.Width/2 - BOX_WIDTH/2, viewport.Height/2 - BOX_HEIGHT/2,
                BOX_WIDTH, BOX_HEIGHT);
            var transitionAlpha = TransitionAlpha;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.Draw(Utils.white, destinationRectangle,
                new Color(transitionAlpha, transitionAlpha, transitionAlpha));
            var font = ScreenManager.Font;
            var origin = new Vector2(0.0f, font.LineSpacing/2);
            var position = new Vector2(destinationRectangle.X, destinationRectangle.Y);
            spriteBatch.DrawString(font, messageContent, position, Color.White, 0.0f, origin, 1f, SpriteEffects.None,
                0.0f);
            spriteBatch.End();
        }

        public override void UnloadContent()
        {
        }
    }
}
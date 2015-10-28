using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MultiplayerGameOverScreen : GameScreen
    {
        private Rectangle contentRect;
        private SpriteFont font;
        private readonly bool isWinner;
        private Color lossBacking;
        private Color lossPattern;
        private int screenHeight;
        private int screenWidth;
        private Color winBacking;
        private Color winPattern;

        public MultiplayerGameOverScreen(bool winner)
        {
            isWinner = winner;
            TransitionOnTime = TimeSpan.FromSeconds(3.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.200000002980232);
            IsPopup = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            winBacking = new Color(5, 30, 8);
            lossBacking = new Color(34, 8, 5);
            winPattern = new Color(11, 66, 23);
            lossPattern = new Color(86, 11, 11);
            screenWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            screenHeight = ScreenManager.GraphicsDevice.Viewport.Height;
            contentRect = new Rectangle(0, screenHeight/6, screenWidth, screenHeight - screenHeight/3);
            font = ScreenManager.Game.Content.Load<SpriteFont>("Kremlin");
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            GuiData.doInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            ScreenManager.FadeBackBufferToBlack(Math.Min(TransitionAlpha, (byte) 150));
            GuiData.startDraw();
            var num = 1f - TransitionPosition;
            PatternDrawer.draw(contentRect, 1f, (isWinner ? winBacking : lossBacking)*num,
                (isWinner ? winPattern : lossPattern)*num, GuiData.spriteBatch);
            var text = isWinner ? "VICTORY" : "DEFEAT";
            var pos = font.MeasureString(text);
            pos.X = contentRect.X + contentRect.Width/2 - pos.X/2f;
            pos.Y = contentRect.Y + contentRect.Height/2 - pos.Y/2f;
            TextItem.DrawShadow = false;
            TextItem.doFontLabel(pos, text, font, Color.White*num, float.MaxValue, float.MaxValue);
            if (Button.doButton(1008, contentRect.X + 10, contentRect.Y + contentRect.Height - 60, 230, 55, "Exit",
                Color.Black))
            {
                if (OS.currentInstance != null)
                    OS.currentInstance.ExitScreen();
                ExitScreen();
                ScreenManager.AddScreen(new MainMenu());
            }
            GuiData.endDraw();
        }
    }
}
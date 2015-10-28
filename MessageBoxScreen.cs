// Decompiled with JetBrains decompiler
// Type: Hacknet.MessageBoxScreen
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MessageBoxScreen : GameScreen
    {
        public static float HEIGHT_BUFFER = 20f;
        private Texture2D bottom;
        private Rectangle contentBounds;
        private SpriteFont guideFont;
        private readonly bool hasEscGuide;
        private Texture2D inputGuide;
        private readonly string message;
        private Texture2D mid;
        private Texture2D top;
        private Vector2 topLeft;

        public MessageBoxScreen(string message)
            : this(message, false)
        {
        }

        public MessageBoxScreen(string message, bool includeUsageText)
        {
            this.message = !includeUsageText
                ? message
                : message + "\nA button, Space, Enter = ok\nB button, Esc = cancel";
            IsPopup = true;
            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.05);
        }

        public MessageBoxScreen(string message, bool includesUsageText, bool hasEscPrompt)
            : this(message, false)
        {
            hasEscGuide = hasEscPrompt;
        }

        public event EventHandler<PlayerIndexEventArgs> Accepted;

        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        public override void LoadContent()
        {
            var content = ScreenManager.Game.Content;
            top = TextureBank.load("PopupFrame", content);
            mid = TextureBank.load("PopupFrame", content);
            bottom = !ScreenManager.usingGamePad
                ? TextureBank.load("PopupBase", content)
                : TextureBank.load("PopupBase", content);
            var num = GuiData.font.MeasureString(message).Y + HEIGHT_BUFFER;
            topLeft = new Vector2((float) (ScreenManager.GraphicsDevice.Viewport.Width/2.0 - top.Width/2.0),
                (float) (ScreenManager.GraphicsDevice.Viewport.Height/2.0 - num/2.0) - top.Height);
            contentBounds = new Rectangle((int) topLeft.X, (int) topLeft.Y + top.Height, top.Width, (int) num);
            if (!hasEscGuide)
                return;
            guideFont = GuiData.font;
        }

        public override void HandleInput(InputState input)
        {
            PlayerIndex playerIndex;
            if (input.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                if (Accepted != null)
                    Accepted(this, new PlayerIndexEventArgs(playerIndex));
                ExitScreen();
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                if (Cancelled != null)
                    Cancelled(this, new PlayerIndexEventArgs(playerIndex));
                ExitScreen();
            }
            GuiData.doInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = ScreenManager.SpriteBatch;
            var spriteFont = GuiData.font;
            ScreenManager.FadeBackBufferToBlack(Math.Min(TransitionAlpha, (byte) 140));
            var viewport = ScreenManager.GraphicsDevice.Viewport;
            var position = new Vector2(viewport.Width, viewport.Height)/2f - spriteFont.MeasureString(message)/2f;
            var color = new Color(22, 22, 22, byte.MaxValue);
            double num = TransitionPosition;
            if (ScreenState == ScreenState.TransitionOff)
                return;
            spriteBatch.Begin();
            spriteBatch.Draw(top, topLeft, color);
            spriteBatch.Draw(mid, contentBounds, color);
            spriteBatch.Draw(top, topLeft + new Vector2(0.0f, top.Height + contentBounds.Height), color);
            spriteBatch.DrawString(spriteFont, message, position, Color.White);
            var x = contentBounds.X + contentBounds.Width - 125;
            var y = contentBounds.Y + contentBounds.Height + top.Height - 40;
            if (Button.doButton(331, x, y, 120, 27, "Resume", new Color?()))
            {
                if (Cancelled != null)
                    Cancelled(this, new PlayerIndexEventArgs(ScreenManager.controllingPlayer));
                ExitScreen();
            }
            if (Button.doButton(332, contentBounds.X + 5, y, 120, 27, "Quit Hacknet", new Color?()))
            {
                if (Accepted != null)
                    Accepted(this, new PlayerIndexEventArgs(ScreenManager.controllingPlayer));
                ExitScreen();
            }
            spriteBatch.End();
        }

        public override void inputMethodChanged(bool usingGamePad)
        {
        }
    }
}
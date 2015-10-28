// Decompiled with JetBrains decompiler
// Type: Hacknet.ScreenManager
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public class ScreenManager : DrawableGameComponent
    {
        private SoundEffect alertSound;
        public AudioEngine audioEngine;
        private Texture2D blankTexture;
        public PlayerIndex controllingPlayer;
        public SpriteFont hugeFont;
        private readonly InputState input = new InputState();
        private bool isInitialized;
        public WaveBank musicBank;
        public Color screenFillColor = Color.Black;
        private readonly List<GameScreen> screens = new List<GameScreen>();
        private readonly List<GameScreen> screensToUpdate = new List<GameScreen>();
        public SoundBank soundBank;
        public bool usingGamePad;
        public WaveBank waveBank;

        public ScreenManager(Game game)
            : base(game)
        {
        }

        public SpriteBatch SpriteBatch { get; private set; }

        public SpriteFont Font { get; private set; }

        public bool TraceEnabled { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
            TraceEnabled = false;
        }

        protected override void LoadContent()
        {
            var content = Game.Content;
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            GuiData.spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = content.Load<SpriteFont>("Font12");
            blankTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            blankTexture.SetData(new Color[1]
            {
                Color.White
            });
            alertSound = content.Load<SoundEffect>("SFX/Bip");
            foreach (var gameScreen in screens)
                gameScreen.LoadContent();
            controllingPlayer = PlayerIndex.One;
        }

        protected override void UnloadContent()
        {
            foreach (var gameScreen in screens)
                gameScreen.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            GameScreen screen = null;
            try
            {
                var flag1 = usingGamePad;
                usingGamePad = false;
                for (var index = 0; index < input.CurrentGamePadStates.Length; ++index)
                {
                    if (input.CurrentGamePadStates[index].IsConnected)
                        usingGamePad = true;
                }
                input.Update();
                screensToUpdate.Clear();
                foreach (var gameScreen in screens)
                    screensToUpdate.Add(gameScreen);
                if (screensToUpdate.Count == 0)
                {
                    foreach (var gameScreen in screens)
                        screensToUpdate.Add(gameScreen);
                }
                var otherScreenHasFocus = !Game.IsActive;
                var coveredByOtherScreen = false;
                var flag2 = false;
                while (screensToUpdate.Count > 0)
                {
                    screen = screensToUpdate[screensToUpdate.Count - 1];
                    screensToUpdate.RemoveAt(screensToUpdate.Count - 1);
                    if (!otherScreenHasFocus &&
                        (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active))
                    {
                        if (flag1 != usingGamePad)
                            screen.inputMethodChanged(usingGamePad);
                        screen.HandleInput(input);
                        flag2 = true;
                    }
                    screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
                    if (flag2)
                        otherScreenHasFocus = true;
                    if ((screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active) &&
                        !screen.IsPopup)
                        coveredByOtherScreen = true;
                }
                if (!TraceEnabled)
                    return;
                TraceScreens();
            }
            catch (Exception ex)
            {
                if (screen == null)
                    return;
                RemoveScreen(screen);
            }
        }

        private void TraceScreens()
        {
            var list = new List<string>();
            foreach (var gameScreen in screens)
                list.Add(gameScreen.GetType().Name);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(screenFillColor);
            for (var index = 0; index < screens.Count; ++index)
            {
                if (screens[index].ScreenState != ScreenState.Hidden)
                    screens[index].Draw(gameTime);
            }
        }

        private void handleCriticalErrorBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            try
            {
                SpriteBatch.End();
            }
            catch
            {
            }
            handleCriticalError();
        }

        public void handleCriticalError()
        {
            if (screens.Count > 1)
                return;
            AddScreen(new MainMenu(), controllingPlayer);
        }

        public void AddScreen(GameScreen screen)
        {
            AddScreen(screen, controllingPlayer);
        }

        public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
        {
            screen.ControllingPlayer = controllingPlayer;
            screen.ScreenManager = this;
            screen.IsExiting = false;
            if (isInitialized)
                screen.LoadContent();
            screens.Add(screen);
        }

        public void ShowPopup(string message)
        {
            AddScreen(new MessageBoxScreen(clipStringForMessageBox(message), false), controllingPlayer);
        }

        public void playAlertSound()
        {
            alertSound.Play(0.3f, 0.0f, 0.0f);
        }

        public void RemoveScreen(GameScreen screen)
        {
            if (isInitialized)
                screen.UnloadContent();
            screens.Remove(screen);
            screensToUpdate.Remove(screen);
            if (screens.Count > 0)
                return;
            handleCriticalError();
        }

        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }

        public void FadeBackBufferToBlack(int alpha)
        {
            var viewport = GraphicsDevice.Viewport;
            SpriteBatch.Begin();
            SpriteBatch.Draw(blankTexture, new Rectangle(0, 0, viewport.Width, viewport.Height),
                new Color(0, 0, 0, (byte) alpha));
            SpriteBatch.End();
        }

        public string clipStringForMessageBox(string s)
        {
            var str = "";
            var num = 0;
            foreach (var ch in s)
            {
                str = ch == 10 ? str + ' ' : str + ch;
                ++num;
                if (num > 50)
                {
                    str += '\n';
                    num = 0;
                }
            }
            return str;
        }

        private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
        }
    }
}
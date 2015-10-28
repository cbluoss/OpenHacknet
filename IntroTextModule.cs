// Decompiled with JetBrains decompiler
// Type: Hacknet.IntroTextModule
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    internal class IntroTextModule : Module
    {
        private static readonly float FLASH_TIME = 3.5f;
        private static readonly float STAY_ONSCREEN_TIME = 3f;
        private static readonly float MODULE_FLASH_TIME = 2f;
        private static readonly float CHAR_TIME = 0.048f;
        private static readonly float LINE_TIME = 0.28f;
        private static readonly float DELAY_FROM_START_MUSIC_TIMER = Settings.isConventionDemo ? 6.2f : 15.06f;
        private static readonly float DEMO_DELAY_FROM_START_MUSIC_TIMER = 5.18f;

        private static readonly string[] delims = new string[1]
        {
            "\r\n"
        };

        private int charIndex;
        private float charTimer;
        public bool complete;
        private bool finishedText;
        private Rectangle fullscreen;
        private float lineTimer;
        private readonly string[] text;
        private int textIndex;
        private float timer;

        public IntroTextModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            bounds = location;
            timer = 0.0f;
            complete = false;
            textIndex = 0;
            finishedText = false;
            fullscreen = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width,
                spriteBatch.GraphicsDevice.Viewport.Height);
            text =
                new StreamReader(
                    TitleContainer.OpenStream(os.multiplayer
                        ? "Content/MultiplayerIntroText.txt"
                        : "Content/BitSpeech.txt")).ReadToEnd().Split(delims, StringSplitOptions.RemoveEmptyEntries);
        }

        public override void Update(float t)
        {
            base.Update(t);
            var num1 = timer;
            timer += t;
            var num2 = Settings.isDemoMode ? DEMO_DELAY_FROM_START_MUSIC_TIMER : DELAY_FROM_START_MUSIC_TIMER;
            if (num1 < (double) num2 && timer >= (double) num2)
                MusicManager.playSong();
            if (finishedText)
            {
                if (timer <= (double) STAY_ONSCREEN_TIME || timer <= STAY_ONSCREEN_TIME + (double) MODULE_FLASH_TIME)
                    return;
                complete = true;
            }
            else if (timer > (double) FLASH_TIME)
            {
                charTimer += t;
                if (charTimer < (double) CHAR_TIME)
                    return;
                charTimer = Settings.isConventionDemo
                    ? CHAR_TIME*(GuiData.getKeyboadState().IsKeyDown(Keys.LeftShift) ? 0.99f : 0.5f)
                    : 0.0f;
                ++charIndex;
                if (charIndex < text[textIndex].Length)
                    return;
                charIndex = text[textIndex].Length - 1;
                lineTimer += t;
                if (lineTimer < (double) LINE_TIME)
                    return;
                lineTimer = Settings.isConventionDemo
                    ? LINE_TIME*(GuiData.getKeyboadState().IsKeyDown(Keys.LeftShift) ? 0.99f : 0.2f)
                    : 0.0f;
                ++textIndex;
                charIndex = 0;
                if (textIndex < text.Length)
                    return;
                if (!MusicManager.isPlaying)
                    MusicManager.playSong();
                finishedText = true;
                timer = 0.0f;
            }
            else
            {
                if (!Settings.isConventionDemo || !GuiData.getKeyboadState().IsKeyDown(Keys.LeftShift))
                    return;
                timer += t + t;
            }
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            spriteBatch.Draw(Utils.white, bounds, os.darkBackgroundColor);
            if (Utils.random.NextDouble() < timer/(double) FLASH_TIME || timer > (double) FLASH_TIME || finishedText)
                os.drawBackground();
            var color = os.terminalTextColor*(finishedText ? (float) (1.0 - timer/(double) STAY_ONSCREEN_TIME) : 1f);
            var position = new Vector2(120f, 100f);
            if (timer > (double) FLASH_TIME || finishedText)
            {
                for (var index = 0; index < textIndex; ++index)
                {
                    spriteBatch.DrawString(GuiData.smallfont, text[index], position, color);
                    position.Y += 16f;
                }
                if (!finishedText)
                {
                    var text = this.text[textIndex].Substring(0, charIndex + 1);
                    spriteBatch.DrawString(GuiData.smallfont, text, position, color);
                }
            }
            if (finishedText && timer > (double) STAY_ONSCREEN_TIME &&
                Utils.random.NextDouble() < (timer - (double) STAY_ONSCREEN_TIME)/MODULE_FLASH_TIME)
                os.drawModules(os.lastGameTime);
            os.drawScanlines();
        }
    }
}
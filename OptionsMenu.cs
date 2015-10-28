using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    internal class OptionsMenu : GameScreen
    {
        private int currentFontIndex = -1;
        private int currentResIndex;
        private string[] fontConfigs;
        private bool mouseHasBeenReleasedOnThisScreen;
        private bool needsApply;
        private bool resolutionChanged;
        private string[] resolutions;
        private bool windowed;

        private readonly char[] xArray = new char[1]
        {
            'x'
        };

        public OptionsMenu()
        {
            resolutionChanged = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            resolutions = new string[20];
            var list = new List<string>();
            list.Add("1152x768");
            list.Add("1280x854");
            list.Add("1440x960");
            list.Add("2880x1920");
            list.Add("1152x864");
            list.Add("1280x960");
            list.Add("1400x1050");
            list.Add("1600x1200");
            list.Add("1280x760");
            list.Add("1280x1024");
            list.Add("1280x720");
            list.Add("1365x760");
            list.Add("1408x792");
            list.Add("1600x900");
            list.Add("1920x1080");
            list.Add("2560x1440");
            list.Add("1280x800");
            list.Add("1440x900");
            list.Add("1680x1050");
            list.Add("1920x1200");
            list.Add("1792x1008");
            list.Add("2560x1600");
            list.Add("2560x1080");
            list.Add("3440x1440");
            list.Add("3440x1440");
            list.Add("3840x2160");
            list.Add("4096x2160");
            list.Sort();
            resolutions = list.ToArray();
            fontConfigs = new string[GuiData.FontConfigs.Count];
            for (var index = 0; index < GuiData.FontConfigs.Count; ++index)
            {
                fontConfigs[index] = GuiData.FontConfigs[index].name;
                if (GuiData.ActiveFontConfig.name == fontConfigs[index])
                    currentFontIndex = index;
            }
            for (var index = 0; index < resolutions.Length; ++index)
            {
                if (resolutions[index].Equals(getCurrentResolution()))
                {
                    currentResIndex = index;
                    break;
                }
            }
            windowed = getIfWindowed();
        }

        public string getCurrentResolution()
        {
            return "" + ScreenManager.GraphicsDevice.Viewport.Width + "x" + ScreenManager.GraphicsDevice.Viewport.Height;
        }

        public bool getIfWindowed()
        {
            return Game1.getSingleton().graphics.IsFullScreen;
        }

        public void apply()
        {
            var flag = false;
            if (windowed != getIfWindowed())
            {
                Game1.getSingleton().graphics.ToggleFullScreen();
                Settings.windowed = getIfWindowed();
                flag = true;
            }
            if (resolutionChanged)
            {
                var strArray = resolutions[currentResIndex].Split(xArray);
                var num1 = Convert.ToInt32(strArray[0]);
                var num2 = Convert.ToInt32(strArray[1]);
                Game1.getSingleton().graphics.PreferredBackBufferWidth = num1;
                Game1.getSingleton().graphics.PreferredBackBufferHeight = num2;
            }
            GuiData.ActivateFontConfig(fontConfigs[currentFontIndex]);
            if (resolutionChanged || flag)
            {
                Game1.getSingleton().graphics.ApplyChanges();
                Game1.getSingleton().setNewGraphics();
            }
            else
                ExitScreen();
            SettingsLoader.writeStatusFile();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (needsApply)
            {
                ScreenManager.GraphicsDevice.SetRenderTarget(null);
                apply();
                needsApply = false;
            }
            if (GuiData.mouse.LeftButton != ButtonState.Released)
                return;
            mouseHasBeenReleasedOnThisScreen = true;
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            GuiData.doInput(input);
            if (!Settings.debugCommandsEnabled || !Utils.keyPressed(input, Keys.F8, new PlayerIndex?()))
                return;
            ExitScreen();
            Game1.getSingleton().Exit();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            PostProcessor.begin();
            ScreenManager.FadeBackBufferToBlack(byte.MaxValue);
            GuiData.startDraw();
            PatternDrawer.draw(
                new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height), 0.5f, Color.Black, new Color(2, 2, 2),
                GuiData.spriteBatch);
            if (Button.doButton(999, 10, 10, 200, 30, "<- Back", Color.Gray))
            {
                SettingsLoader.writeStatusFile();
                ExitScreen();
            }
            TextItem.doLabel(new Vector2(400f, 65f), "Resolutions", new Color?());
            var num = currentResIndex;
            currentResIndex = SelectableTextList.doFancyList(10, 400, 100, 200, 450, resolutions, currentResIndex,
                new Color?(), false);
            if (!mouseHasBeenReleasedOnThisScreen)
                currentResIndex = num;
            else if (SelectableTextList.wasActivated)
                resolutionChanged = true;
            TextItem.doLabel(new Vector2(100f, 64f), "Fullscreen", new Color?());
            windowed = CheckBox.doCheckBox(20, 100, 100, windowed, new Color?());
            TextItem.doLabel(new Vector2(100f, 124f), "Bloom", new Color?());
            PostProcessor.bloomEnabled = CheckBox.doCheckBox(21, 100, 160, PostProcessor.bloomEnabled, new Color?());
            TextItem.doLabel(new Vector2(100f, 184f), "Scanlines", new Color?());
            PostProcessor.scanlinesEnabled = CheckBox.doCheckBox(22, 100, 220, PostProcessor.scanlinesEnabled,
                new Color?());
            TextItem.doLabel(new Vector2(100f, 244f), "Sound Enabled", new Color?());
            MusicManager.setIsMuted(!CheckBox.doCheckBox(23, 100, 280, !MusicManager.isMuted, new Color?()));
            TextItem.doLabel(new Vector2(100f, 305f), "Music Volume", new Color?());
            MusicManager.setVolume(SliderBar.doSliderBar(24, 100, 350, 210, 30, 1f, 0.0f, MusicManager.getVolume(),
                1.0f/1000.0f));
            TextItem.doLabel(new Vector2(100f, 384f), "Text Size", new Color?());
            currentFontIndex = SelectableTextList.doFancyList(25, 100, 414, 200, 160, fontConfigs, currentFontIndex,
                new Color?(), false);
            if (Button.doButton(990, 10, ScreenManager.GraphicsDevice.Viewport.Height - 120, 200, 30, "Apply Changes",
                Color.LightBlue))
                needsApply = true;
            GuiData.endDraw();
            PostProcessor.end();
        }
    }
}
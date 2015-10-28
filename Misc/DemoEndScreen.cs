// Decompiled with JetBrains decompiler
// Type: Hacknet.Misc.DemoEndScreen
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Windows.Forms;
using Hacknet.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Hacknet.Misc
{
    public class DemoEndScreen : GameScreen
    {
        private Rectangle Fullscreen;
        private SoundEffect spinDownSound;
        private double timeOnThisScreen;

        public override void LoadContent()
        {
            base.LoadContent();
            Fullscreen = new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height);
            MediaPlayer.Stop();
            PostProcessor.EndingSequenceFlashOutActive = false;
            PostProcessor.EndingSequenceFlashOutPercentageComplete = 0.0f;
            PostProcessor.dangerModeEnabled = false;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (!otherScreenHasFocus && !coveredByOtherScreen)
                timeOnThisScreen += gameTime.ElapsedGameTime.TotalSeconds;
            if (timeOnThisScreen > 5.0 && Keyboard.GetState().GetPressedKeys().Length > 0)
            {
                if (Settings.isConventionDemo)
                {
                    OS.currentInstance.ScreenManager.AddScreen(new MainMenu());
                    OS.currentInstance.ExitScreen();
                }
                else
                    Application.Exit();
            }
            else if (timeOnThisScreen > 20.0)
            {
                if (Settings.isConventionDemo)
                {
                    OS.currentInstance.ScreenManager.AddScreen(new MainMenu());
                    OS.currentInstance.ExitScreen();
                    ExitScreen();
                    MainMenu.resetOS();
                }
                else
                    Application.Exit();
            }
            PostProcessor.EndingSequenceFlashOutActive = false;
            PostProcessor.EndingSequenceFlashOutPercentageComplete = 0.0f;
            PostProcessor.dangerModeEnabled = false;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            PostProcessor.begin();
            GuiData.startDraw();
            GuiData.spriteBatch.Draw(Utils.white, Fullscreen, Color.Black);
            var dest = Utils.InsetRectangle(Fullscreen, 200);
            dest.Y = dest.Y + dest.Height/2 - 200;
            dest.Height = 400;
            var destinationRectangle = new Rectangle(Fullscreen.X, dest.Y + 50, Fullscreen.Width, dest.Height - 148);
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, Utils.AddativeRed*(0.5f + Utils.randm(0.1f)));
            var text = "HACKNET";
            FlickeringTextEffect.DrawLinedFlickeringText(dest, text, 18f, 0.7f, GuiData.titlefont, null, Color.White, 6);
            dest.Y += 400;
            dest.Height = 120;
            FlickeringTextEffect.DrawFlickeringText(dest, Utils.FlipRandomChars("MORE SOON", 0.008), -8f, 0.7f,
                GuiData.titlefont, null, Color.Gray);
            FlickeringTextEffect.DrawFlickeringText(dest, Utils.FlipRandomChars("MORE SOON", 0.03), -8f, 0.7f,
                GuiData.titlefont, null, Utils.AddativeWhite*0.15f);
            GuiData.endDraw();
            PostProcessor.end();
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.CrashModule
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class CrashModule : Module
    {
        public static float BLUESCREEN_TIME = 8f;
        public static float BOOT_TIME = Settings.isConventionDemo ? 5f : (Settings.FastBootText ? 1.2f : 14.5f);
        public static float BLACK_TIME = 2f;
        public static float POST_BLACK_TIME = 1f;
        private static SoundEffect beep;
        public Color bluescreenBlue = new Color(0, 0, 170);
        public Color bluescreenGrey = new Color(167, 167, 167);
        public string BootLoadErrors = "";
        private string[] bootText;
        private int bootTextCount;
        private float bootTextDelay = 1f;
        private float bootTextErrorDelay;
        private float bootTextTimer;
        private SpriteFont bsodFont;
        private string bsodText = "";
        public float elapsedTime;
        private bool graphicsErrorsDetected;
        private bool hasPlayedBeep;
        private string originalBootText;
        public int state;
        public Color textColor = new Color(0, 0, byte.MaxValue);

        public CrashModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            bounds = location;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            bsodFont = os.ScreenManager.Game.Content.Load<SpriteFont>("BSODFont");
            beep = os.content.Load<SoundEffect>("SFX/beep");
            var streamReader1 = new StreamReader(TitleContainer.OpenStream("Content/BSOD.txt"));
            bsodText = streamReader1.ReadToEnd();
            streamReader1.Close();
            var streamReader2 = new StreamReader(TitleContainer.OpenStream("Content/OSXBoot.txt"));
            originalBootText = streamReader2.ReadToEnd();
            streamReader2.Close();
            loadBootText();
            bootTextDelay = BOOT_TIME/((bootText.Length - 1)*2f);
        }

        private void loadBootText()
        {
            bootText = checkOSBootFiles(originalBootText).Split('\n');
        }

        public override void Update(float t)
        {
            base.Update(t);
            elapsedTime += t;
            if (elapsedTime < BLUESCREEN_TIME/4.0)
                state = 0;
            else if (elapsedTime < (double) BLUESCREEN_TIME)
                state = 1;
            else if (elapsedTime < BLUESCREEN_TIME + (double) BLACK_TIME)
                state = 2;
            else if (elapsedTime < BLUESCREEN_TIME + (double) BLACK_TIME + BOOT_TIME + bootTextErrorDelay)
            {
                state = 3;
                bootTextTimer -= t;
                if (bootTextTimer <= 0.0)
                {
                    bootTextTimer = bootTextDelay - (float) Utils.random.NextDouble()*bootTextDelay +
                                    (float) Utils.random.NextDouble()*bootTextDelay;
                    ++bootTextCount;
                    if (bootTextCount >= bootText.Length - 1)
                        bootTextCount = bootText.Length - 1;
                    if (bootText[bootTextCount].Equals(" "))
                        bootTextTimer = bootTextDelay*12f;
                    if (bootText[bootTextCount].StartsWith("ERROR:"))
                    {
                        bootTextTimer = bootTextDelay*29f;
                        os.thisComputer.bootupTick((float) -(bootTextDelay*42.0));
                        bootTextErrorDelay += bootTextDelay*42f;
                    }
                }
                if (hasPlayedBeep)
                    return;
                beep.Play(0.5f, 0.5f, 0.0f);
                os.delayer.Post(ActionDelayer.Wait(0.1), () => beep.Play(0.5f, 0.5f, 0.0f));
                hasPlayedBeep = true;
            }
            else
                state = 2;
        }

        public override void Draw(float t)
        {
            switch (state)
            {
                case 0:
                    spriteBatch.Draw(Utils.white, bounds, bluescreenBlue);
                    drawString(elapsedTime%0.800000011920929 > 0.5 ? "" : "_", new Vector2(bounds.X, bounds.Y + 10),
                        bsodFont);
                    break;
                case 1:
                    spriteBatch.Draw(Utils.white, bounds, bluescreenBlue);
                    drawString(bsodText, new Vector2(bounds.X, bounds.Y + 10), bsodFont);
                    break;
                case 3:
                    var num = GuiData.ActiveFontConfig.tinyFontCharHeight + 1f;
                    spriteBatch.Draw(Utils.white, bounds, os.darkBackgroundColor);
                    Math.Min((int) ((bounds.Height - 10 - (double) num)/num), bootTextCount);
                    var index = bootTextCount;
                    var dpos = new Vector2(bounds.X + 10, (float) (index*(double) num + 10.0));
                    if (dpos.Y > (double) (bounds.Y + bounds.Height - 14))
                        dpos.Y = bounds.Y + bounds.Height - 14;
                    for (; index >= 0 && dpos.Y > (double) num; --index)
                    {
                        drawString(bootText[index], dpos, GuiData.tinyfont);
                        dpos.Y -= num;
                    }
                    break;
                default:
                    spriteBatch.Draw(Utils.white, bounds, os.darkBackgroundColor);
                    break;
            }
        }

        private Vector2 drawString(string text, Vector2 dpos, SpriteFont font)
        {
            var vector2 = font.MeasureString(text);
            var flag = text.StartsWith("ERROR:");
            spriteBatch.DrawString(font, text, dpos, flag ? Color.Red : Color.White, 0.0f, Vector2.Zero, 1f,
                SpriteEffects.None, 0.0f);
            return vector2;
        }

        public string checkOSBootFiles(string bootString)
        {
            BootLoadErrors = "";
            var folder = os.thisComputer.files.root.searchForFolder("sys");
            var flag = true;
            var newValue1 = "ERROR: Unable to Load System file os-config.sys\n";
            if (folder.containsFile("os-config.sys"))
            {
                newValue1 = "Loaded os-config.sys : System Config Initialized";
            }
            else
            {
                os.failBoot();
                flag = false;
                var crashModule = this;
                var str = crashModule.BootLoadErrors + newValue1 + " \n";
                crashModule.BootLoadErrors = str;
            }
            bootString = bootString.Replace("[OSBoot1]", newValue1);
            var newValue2 = "ERROR: Unable to Load System file bootcfg.dll\n";
            if (folder.containsFile("bootcfg.dll"))
            {
                newValue2 = "Loaded bootcfg.dll : Boot Config Module Loaded";
            }
            else
            {
                os.failBoot();
                flag = false;
                var crashModule = this;
                var str = crashModule.BootLoadErrors + newValue2 + " \n";
                crashModule.BootLoadErrors = str;
            }
            bootString = bootString.Replace("[OSBoot2]", newValue2);
            var newValue3 = "ERROR: Unable to Load System file netcfgx.dll\n";
            if (folder.containsFile("netcfgx.dll"))
            {
                newValue3 = "Loaded netcfgx.dll : Network Config Module Loaded";
            }
            else
            {
                os.failBoot();
                flag = false;
                var crashModule = this;
                var str = crashModule.BootLoadErrors + newValue3 + " \n";
                crashModule.BootLoadErrors = str;
            }
            bootString = bootString.Replace("[OSBoot3]", newValue3);
            var newValue4 =
                "ERROR: Unable to Load System file x-server.sys\nERROR: Locate and restore a valid x-server file in ~/sys/ folder to restore UX functionality\nERROR: Consider examining reports in ~/log/ for problem cause and source\nERROR: System UX resources unavailable -- defaulting to terminal mode\n .\n .\n .\n";
            if (folder.containsFile("x-server.sys"))
            {
                newValue4 = "Loaded x-server.sys : UX Graphics Module Loaded";
                ThemeManager.switchTheme(os,
                    ThemeManager.getThemeForDataString(folder.searchForFile("x-server.sys").data));
                graphicsErrorsDetected = false;
            }
            else
            {
                os.graphicsFailBoot();
                flag = false;
                graphicsErrorsDetected = true;
                var crashModule = this;
                var str = crashModule.BootLoadErrors + newValue4 + " \n";
                crashModule.BootLoadErrors = str;
            }
            bootString = bootString.Replace("[OSBootTheme]", newValue4);
            if (flag)
            {
                if (os.Flags.HasFlag("BootFailure") && !os.Flags.HasFlag("BootFailureThemeSongChange") &&
                    ThemeManager.currentTheme != OSTheme.HacknetBlue)
                {
                    os.Flags.AddFlag("BootFailureThemeSongChange");
                    if (MusicManager.isPlaying)
                        MusicManager.stop();
                    MusicManager.loadAsCurrentSong("Music\\The_Quickening");
                }
                os.sucsesfulBoot();
            }
            else
                os.Flags.AddFlag("BootFailure");
            return bootString;
        }

        public void reset()
        {
            elapsedTime = 0.0f;
            state = 0;
            bootTextCount = 0;
            bootTextTimer = 0.0f;
            bootTextErrorDelay = 0.0f;
            hasPlayedBeep = false;
            loadBootText();
            MusicManager.stop();
        }

        public void completeReboot()
        {
            os.terminal.reset();
            os.execute("FirstTimeInitdswhupwnemfdsiuoewnmdsmffdjsklanfeebfjkalnbmsdakj Init");
            os.inputEnabled = false;
            if (!graphicsErrorsDetected)
            {
                os.setMouseVisiblity(true);
                MusicManager.playSong();
            }
            os.connectedComp = null;
            if (os.thisComputer.files.root.searchForFolder("sys").searchForFile("Notes_Reopener.bat") != null)
                os.runCommand("notes");
            try
            {
                os.threadedSaveExecute();
                os.gameSavedTextAlpha = -1f;
            }
            catch (Exception ex)
            {
                var num = 1 - 1;
            }
        }
    }
}
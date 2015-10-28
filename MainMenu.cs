// Decompiled with JetBrains decompiler
// Type: Hacknet.MainMenu
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;
using System.Threading;
using System.Xml;
using Hacknet.Effects;
using Hacknet.Gui;
using Hacknet.Misc;
using Hacknet.PlatformAPI.Storage;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MainMenu : GameScreen
    {
        public static string OSVersion = "v3.017";
        public static string AccumErrors = "";
        private readonly AttractModeMenuScreen attractModeScreen = new AttractModeMenuScreen();
        private Color buttonColor;
        private bool canLoad;
        private Color exitButtonColor;
        private int framecount;
        private bool hasSentErrorEmail;
        private HexGridBackground hexBackground;
        private readonly SavefileLoginScreen loginScreen = new SavefileLoginScreen();
        private MainMenuState State;
        private string testSuiteResult;
        private Color titleColor;
        private SpriteFont titleFont;

        public MainMenu()
        {
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            buttonColor = new Color(124, 137, 149);
            exitButtonColor = new Color(105, 82, 82);
            titleColor = new Color(190, 190, 190, 0);
            titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Kremlin");
            canLoad = SaveFileManager.StorageMethods[0].GetSaveManifest().LastLoggedInUser.Username != null;
            hexBackground = new HexGridBackground(ScreenManager.Game.Content);
            HookUpCreationEvents();
        }

        private void HookUpCreationEvents()
        {
            loginScreen.RequestGoBack += () => State = MainMenuState.Normal;
            loginScreen.StartNewGameForUsernameAndPass += (username, pass) =>
            {
                if (SaveFileManager.AddUser(username, pass))
                {
                    var filePathForLogin = SaveFileManager.GetFilePathForLogin(username, pass);
                    ExitScreen();
                    resetOS();
                    if (!Settings.soundDisabled)
                        ScreenManager.playAlertSound();
                    try
                    {
                        ScreenManager.AddScreen(new OS
                        {
                            SaveGameUserName = filePathForLogin,
                            SaveUserAccountName = username
                        }, ScreenManager.controllingPlayer);
                    }
                    catch (Exception ex)
                    {
                        UpdateUIForSaveCreationFailed(ex);
                    }
                }
                else
                {
                    loginScreen.ResetForNewAccount();
                    loginScreen.WriteToHistory(" ERROR: Username invalid or already in use.");
                }
            };
            loginScreen.LoadGameForUserFileAndUsername += (userFile, username) =>
            {
                ExitScreen();
                resetOS();
                OS.WillLoadSave = SaveFileManager.StorageMethods[0].FileExists(userFile);
                var os = new OS();
                os.SaveGameUserName = userFile;
                os.SaveUserAccountName = username;
                try
                {
                    ScreenManager.AddScreen(os, ScreenManager.controllingPlayer);
                }
                catch (XmlException ex)
                {
                    UpdateUIForSaveCorruption(userFile, ex);
                }
                catch (FormatException ex)
                {
                    UpdateUIForSaveCorruption(userFile, ex);
                }
                catch (NullReferenceException ex)
                {
                    UpdateUIForSaveCorruption(userFile, ex);
                }
                catch (FileNotFoundException ex)
                {
                    UpdateUIForSaveMissing(userFile, ex);
                }
            };
            attractModeScreen.Start += () =>
            {
                try
                {
                    ExitScreen();
                    resetOS();
                    ScreenManager.playAlertSound();
                    ScreenManager.AddScreen(new OS(), ScreenManager.controllingPlayer);
                }
                catch (Exception ex)
                {
                    Utils.writeToFile("OS Load Error: " + ex + "\n\n" + ex.StackTrace, "crashLog.txt");
                }
            };
        }

        private void UpdateUIForSaveCorruption(string saveName, Exception ex)
        {
            State = MainMenuState.Normal;
            AccumErrors = AccumErrors + "ACCOUNT FILE CORRUPTION: Account " + saveName +
                          " appears to be corrupted, and will not load. Reported Error:\r\n" +
                          Utils.GenerateReportFromException(ex) + "\r\n";
            if (hasSentErrorEmail)
                return;
            new Thread(() => Utils.SendErrorEmail(ex, "Save Corruption ", ""))
            {
                IsBackground = true,
                Name = "SaveCorruptErrorReportThread"
            }.Start();
            hasSentErrorEmail = true;
        }

        private void UpdateUIForSaveMissing(string saveName, Exception ex)
        {
            State = MainMenuState.Normal;
            AccumErrors = AccumErrors + "ACCOUNT FILE NOT FOUND: Account " + saveName +
                          " appears to be missing. It may have been moved or deleted. Reported Error:\r\n" +
                          Utils.GenerateReportFromException(ex) + "\r\n";
            if (hasSentErrorEmail)
                return;
            new Thread(() => Utils.SendErrorEmail(ex, "Save Missing ", ""))
            {
                IsBackground = true,
                Name = "SaveMissingErrorReportThread"
            }.Start();
            hasSentErrorEmail = true;
        }

        private void UpdateUIForSaveCreationFailed(Exception ex)
        {
            State = MainMenuState.Normal;
            AccumErrors = AccumErrors + "CRITICAL ERROR CREATING ACCOUNT: Reported Error:\r\n" +
                          Utils.GenerateReportFromException(ex) + "\r\n";
            if (hasSentErrorEmail)
                return;
            new Thread(() => Utils.SendErrorEmail(ex, "Account Creation Error ", ""))
            {
                IsBackground = true,
                Name = "SaveAccCreationErrorReportThread"
            }.Start();
            hasSentErrorEmail = true;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            ++framecount;
            hexBackground.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void HandleInput(InputState input)
        {
            base.HandleInput(input);
            GuiData.doInput(input);
        }

        public static void resetOS()
        {
            if (Settings.isSpecialTestBuild)
                return;
            TextBox.cursorPosition = 0;
            Settings.initShowsTutorial = Settings.osStartsWithTutorial;
            Settings.IsInAdventureMode = false;
            ScrollablePanel.ClearCache();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            try
            {
                PostProcessor.begin();
                ScreenManager.FadeBackBufferToBlack(byte.MaxValue);
                GuiData.startDraw();
                var dest1 = new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height);
                var destinationRectangle = new Rectangle(-20, -20, ScreenManager.GraphicsDevice.Viewport.Width + 40,
                    ScreenManager.GraphicsDevice.Viewport.Height + 40);
                var dest2 = new Rectangle(dest1.X + dest1.Width/4, dest1.Height/4, dest1.Width/2, dest1.Height/4);
                GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, Color.Black);
                hexBackground.Draw(dest1, GuiData.spriteBatch, Color.Transparent,
                    Settings.lighterColorHexBackground ? new Color(20, 20, 20) : new Color(10, 10, 10, 0),
                    HexGridBackground.ColoringAlgorithm.NegaitiveSinWash, 0.0f);
                TextItem.DrawShadow = false;
                switch (State)
                {
                    case MainMenuState.NewUser:
                        DrawLoginScreen(dest2, true);
                        break;
                    case MainMenuState.Login:
                        DrawLoginScreen(dest2, false);
                        break;
                    default:
                        if (Settings.isLockedDemoMode)
                        {
                            attractModeScreen.Draw(dest1, GuiData.spriteBatch);
                            break;
                        }
                        FlickeringTextEffect.DrawLinedFlickeringText(new Rectangle(180, 120, 340, 100), "HACKNET", 7f,
                            0.55f, titleFont, null, titleColor, 2);
                        TextItem.doFontLabel(new Vector2(520f, 178f), "OS " + OSVersion, GuiData.smallfont,
                            titleColor*0.5f, 600f, 26f);
                        var canRun = true;
                        if (Settings.IsExpireLocked)
                        {
                            var timeSpan = Settings.ExpireTime - DateTime.Now;
                            string text;
                            if (timeSpan.TotalSeconds < 1.0)
                            {
                                text = "TEST BUILD EXPIRED - EXECUTION DISABLED";
                                canRun = false;
                            }
                            else
                                text = "Test Build : Expires in " + timeSpan;
                            TextItem.doFontLabel(new Vector2(180f, 105f), text, GuiData.smallfont, Color.Red*0.8f, 600f,
                                26f);
                        }
                        if (Settings.isLockedDemoMode)
                        {
                            drawDemoModeButtons(canRun);
                            break;
                        }
                        drawMainMenuButtons(canRun);
                        if (Settings.testingMenuItemsEnabled)
                        {
                            drawTestingMainMenuButtons(canRun);
                        }
                        break;
                }
                GuiData.endDraw();
                PostProcessor.end();
                ScreenManager.FadeBackBufferToBlack(byte.MaxValue - TransitionAlpha);
            }
            catch (ObjectDisposedException ex)
            {
                if (hasSentErrorEmail)
                    throw ex;
                var body =
                    string.Concat(
                        Utils.GenerateReportFromException(ex) + (object) "\r\n Font:" + titleFont +
                        (object) "\r\n White:" + Utils.white + (object) "\r\n WhiteDisposed:" + Utils.white.IsDisposed +
                        (object) "\r\n SmallFont:" + GuiData.smallfont + (object) "\r\n TinyFont:" + GuiData.tinyfont +
                        "\r\n LineEffectTarget:" + FlickeringTextEffect.GetReportString() + "\r\n PostProcessort stuff:" +
                        PostProcessor.GetStatusReportString(), "\r\nRESOLUTION:\r\n ",
                        Game1.getSingleton().GraphicsDevice.PresentationParameters.BackBufferWidth, "x") +
                    Game1.getSingleton().GraphicsDevice.PresentationParameters.BackBufferHeight + "\r\nFullscreen: " +
                    (Game1.getSingleton().graphics.IsFullScreen ? "true" : "false") + "\r\n Adapter: " +
                    Game1.getSingleton().GraphicsDevice.Adapter.Description + "\r\n Device Name: " +
                    Game1.getSingleton().GraphicsDevice.Adapter.DeviceName + (object) "\r\n Status: " +
                    Game1.getSingleton().GraphicsDevice.GraphicsDeviceStatus;
                Utils.SendRealWorldEmail(
                    "Hackent " + OSVersion + " Crash " + DateTime.Now.ToShortDateString() + " " +
                    DateTime.Now.ToShortTimeString(), "hacknetbugs+Hacknet@gmail.com", body);
                hasSentErrorEmail = true;
                SettingsLoader.writeStatusFile();
            }
        }

        private void DrawLoginScreen(Rectangle dest, bool needsNewUser = false)
        {
            loginScreen.Draw(GuiData.spriteBatch, dest);
        }

        private void drawDemoModeButtons(bool canRun)
        {
            if (Button.doButton(1, 180, 200, 450, 50, "New Session", buttonColor))
            {
                if (canRun)
                {
                    try
                    {
                        ExitScreen();
                        resetOS();
                        ScreenManager.playAlertSound();
                        ScreenManager.AddScreen(new OS(), ScreenManager.controllingPlayer);
                    }
                    catch (Exception ex)
                    {
                        Utils.writeToFile("OS Load Error: " + ex + "\n\n" + ex.StackTrace, "crashLog.txt");
                    }
                }
            }
            if (Button.doButton(11, 180, 265, 450, 50, "Load Session", canLoad ? buttonColor : Color.Black))
            {
                if (canLoad)
                {
                    try
                    {
                        if (canRun)
                        {
                            ExitScreen();
                            resetOS();
                            OS.WillLoadSave = true;
                            ScreenManager.AddScreen(new OS(), ScreenManager.controllingPlayer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.writeToFile("OS Load Error: " + ex + "\n\n" + ex.StackTrace, "crashLog.txt");
                    }
                }
            }
            if (!Button.doButton(15, 180, 330, 450, 28, "Exit", exitButtonColor))
                return;
            MusicManager.stop();
            Game1.threadsExiting = true;
            Game1.getSingleton().Exit();
        }

        private void drawTestingMainMenuButtons(bool canRun)
        {
            if (Button.doButton(8801, 634, 200, 225, 23, "New Test Session", buttonColor) && canRun)
            {
                ExitScreen();
                resetOS();
                if (!Settings.soundDisabled)
                    ScreenManager.playAlertSound();
                var os = new OS();
                ScreenManager.AddScreen(os, ScreenManager.controllingPlayer);
                os.delayer.RunAllDelayedActions();
                os.threadedSaveExecute();
                ScreenManager.RemoveScreen(os);
                OS.WillLoadSave = true;
                resetOS();
                ScreenManager.AddScreen(new OS(), ScreenManager.controllingPlayer);
            }
            if (Button.doButton(8803, 634, 225, 225, 23, "New Entropy Accelerated Session", buttonColor) && canRun)
            {
                ExitScreen();
                resetOS();
                if (!Settings.soundDisabled)
                    ScreenManager.playAlertSound();
                var os1 = new OS();
                os1.SaveGameUserName = "entropyTest";
                os1.SaveUserAccountName = "entropyTest";
                ScreenManager.AddScreen(os1, ScreenManager.controllingPlayer);
                os1.Flags.AddFlag("TutorialComplete");
                os1.delayer.RunAllDelayedActions();
                os1.threadedSaveExecute();
                ScreenManager.RemoveScreen(os1);
                OS.WillLoadSave = true;
                resetOS();
                Settings.initShowsTutorial = false;
                var os2 = new OS();
                ScreenManager.AddScreen(os2, ScreenManager.controllingPlayer);
                MissionFunctions.runCommand(0, "EntropyFastFowardSetup");
                os2.delayer.Post(ActionDelayer.Wait(1.0), () => Game1.getSingleton().IsMouseVisible = true);
            }
            if (Button.doButton(8806, 634, 250, 225, 23, "Run Test Suite", buttonColor))
                testSuiteResult = TestSuite.TestSaveLoadOnFile(ScreenManager);
            if (Button.doButton(8809, 634, 275, 225, 23, "Export Animation", buttonColor))
            {
                var TitleFill = new Rectangle(0, 0, 300, 100);
                AnimatedSpriteExporter.ExportAnimation("OutNowAnim", "OutNow", TitleFill.Width, TitleFill.Height, 24f,
                    40f, GuiData.spriteBatch.GraphicsDevice, t => new OS
                    {
                        highlightColor = new Color(166, byte.MaxValue, 215)
                    }.timer += t, (sb, dest) =>
                    {
                        sb.Draw(Utils.white, dest, new Color(13, 13, 13));
                        FlickeringTextEffect.DrawFlickeringText(TitleFill, "OUT NOW", 8f, 0.7f, titleFont, null,
                            new Color(216, 216, 216));
                    }, 1);
            }
            if (Button.doButton(8812, 634, 300, 225, 23, "New CSEC Accel Session", buttonColor) && canRun)
            {
                ExitScreen();
                resetOS();
                if (!Settings.soundDisabled)
                    ScreenManager.playAlertSound();
                var os1 = new OS();
                ScreenManager.AddScreen(os1, ScreenManager.controllingPlayer);
                os1.Flags.AddFlag("TutorialComplete");
                os1.delayer.RunAllDelayedActions();
                os1.threadedSaveExecute();
                ScreenManager.RemoveScreen(os1);
                OS.WillLoadSave = true;
                resetOS();
                Settings.initShowsTutorial = false;
                var os2 = new OS();
                ScreenManager.AddScreen(os2, ScreenManager.controllingPlayer);
                MissionFunctions.runCommand(0, "CSECFastFowardSetup");
                os2.delayer.Post(ActionDelayer.Wait(1.0), () => Game1.getSingleton().IsMouseVisible = true);
            }
            if (testSuiteResult == null)
                return;
            TextItem.doFontLabel(new Vector2(635f, 325f),
                Utils.SuperSmartTwimForWidth(testSuiteResult, 600, GuiData.tinyfont), GuiData.tinyfont,
                testSuiteResult.Length > 250 ? Utils.AddativeRed : Utils.AddativeWhite, float.MaxValue, float.MaxValue);
        }

        private void drawMainMenuButtons(bool canRun)
        {
            var num1 = 135;
            int num2;
            if (Button.doButton(1, 180, num2 = num1 + 65, 450, 50, "New Session", buttonColor) && canRun)
            {
                State = MainMenuState.NewUser;
                loginScreen.ResetForNewAccount();
            }
            int num3;
            if (
                Button.doButton(1102, 180, num3 = num2 + 65, 450, 28,
                    canLoad
                        ? "Continue with account [" + SaveFileManager.LastLoggedInUser.Username + "]"
                        : "No Accounts", canLoad ? buttonColor : Color.Black) && canLoad)
                loginScreen.LoadGameForUserFileAndUsername(SaveFileManager.LastLoggedInUser.FileUsername,
                    SaveFileManager.LastLoggedInUser.Username);
            int num4;
            if (Button.doButton(11, 180, num4 = num3 + 39, 450, 50, "Login", canLoad ? buttonColor : Color.Black))
            {
                if (canLoad)
                {
                    try
                    {
                        State = MainMenuState.Login;
                        loginScreen.ResetForLogin();
                    }
                    catch (Exception ex)
                    {
                        Utils.writeToFile("OS Load Error: " + ex + "\n\n" + ex.StackTrace, "crashLog.txt");
                    }
                }
            }
            int num5;
            if (Button.doButton(3, 180, num5 = num4 + 65, 450, 50, "Settings", buttonColor))
                ScreenManager.AddScreen(new OptionsMenu(), ScreenManager.controllingPlayer);
            int num6;
            var y = num6 = num5 + 65;
            if (Settings.isServerMode)
            {
                if (Button.doButton(4, 180, y, 450, 50, "Start Relay Server", buttonColor))
                    ScreenManager.AddScreen(new ServerScreen(), ScreenManager.controllingPlayer);
                y += 65;
            }
            if (Settings.AllowAdventureMode)
            {
                if (Button.doButton(5, 180, y, 450, 50, "Adventure Session", buttonColor))
                {
                    ExitScreen();
                    resetOS();
                    Settings.IsInAdventureMode = true;
                    if (!Settings.soundDisabled)
                        ScreenManager.playAlertSound();
                    ScreenManager.AddScreen(new OS(), ScreenManager.controllingPlayer);
                }
                y += 65;
            }
            if (Button.doButton(15, 180, y, 450, 28, "Exit", exitButtonColor))
            {
                MusicManager.stop();
                Game1.threadsExiting = true;
                Game1.getSingleton().Exit();
            }
            var num7 = y + 30;
            if (!PlatformAPISettings.RemoteStorageRunning)
            {
                TextItem.doFontLabel(new Vector2(180f, num7), "WARNING: Error connecting to Steam Cloud",
                    GuiData.smallfont, Color.DarkRed, float.MaxValue, float.MaxValue);
                num7 += 20;
            }
            if (string.IsNullOrWhiteSpace(AccumErrors))
                return;
            TextItem.doFontLabel(new Vector2(180f, num7), AccumErrors, GuiData.smallfont, Color.DarkRed, float.MaxValue,
                float.MaxValue);
            var num8 = num7 + 20;
        }

        private enum MainMenuState
        {
            Normal,
            NewUser,
            Login
        }
    }
}
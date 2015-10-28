using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    internal class TraceDangerSequence
    {
        private const float WARNING_INTRO_TIME = 1f;
        private const float WARNING_EXIT_TIME = 13.9f;
        private const float DISCONNECT_REBOOT_TIME = 10f;
        private const float COUNTDOWN_TIME = 130f;
        private const float FLASH_FREQUENCY = 1.937667f;
        private static readonly Color DarkRed = new Color(105, 0, 0, 200);
        private static readonly Color BackgroundRed = new Color(120, 0, 0);
        private readonly SpriteFont bodyFont;
        private Rectangle fullscreen;
        public bool IsActive;
        private string oldSong;
        private float onBeatFlashTimer;
        private readonly OS os;
        private float percentComplete;
        public bool PreventOSRendering;
        private SpriteBatch scaleupSpriteBatch;
        private readonly SoundEffect spinDownSound;
        private readonly SoundEffect spinUpSound;
        private readonly SpriteBatch spriteBatch;
        private TraceDangerState state;
        private float timeThisState;
        private readonly SpriteFont titleFont;
        private bool warningScreenIsActivating;

        public TraceDangerSequence(ContentManager content, SpriteBatch sb, Rectangle fullscreenRect, OS os)
        {
            titleFont = GuiData.titlefont;
            bodyFont = GuiData.font;
            fullscreen = fullscreenRect;
            spriteBatch = sb;
            scaleupSpriteBatch = new SpriteBatch(sb.GraphicsDevice);
            this.os = os;
            spinDownSound = os.content.Load<SoundEffect>("Music/Ambient/spiral_gauge_down");
            spinUpSound = os.content.Load<SoundEffect>("Music/Ambient/spiral_gauge_up");
        }

        public void BeginTraceDangerSequence()
        {
            timeThisState = 0.0f;
            state = TraceDangerState.WarningScrenIntro;
            IsActive = true;
            PreventOSRendering = true;
            oldSong = MusicManager.currentSongName;
            os.execute("dc");
            os.display.command = "";
            os.terminal.inputLocked = true;
            MusicManager.playSongImmediatley("Music/Ambient/dark_drone_008");
            spinDownSound.Play();
        }

        public void CompleteIPResetSucsesfully()
        {
            timeThisState = 0.0f;
            state = TraceDangerState.DisconnectedReboot;
            IsActive = true;
            PreventOSRendering = true;
            PostProcessor.dangerModeEnabled = false;
            PostProcessor.dangerModePercentComplete = 0.0f;
            MusicManager.stop();
            spinDownSound.Play(1f, -0.6f, 0.0f);
        }

        public void CancelTraceDangerSequence()
        {
            timeThisState = 0.0f;
            state = TraceDangerState.WarningScrenIntro;
            IsActive = false;
            PreventOSRendering = false;
            PostProcessor.dangerModeEnabled = false;
            MusicManager.stop();
        }

        public void Update(float t)
        {
            timeThisState += t;
            var num = float.MaxValue;
            PostProcessor.dangerModeEnabled = false;
            switch (state)
            {
                case TraceDangerState.WarningScrenIntro:
                    num = 1f;
                    if (timeThisState > (double) num)
                    {
                        timeThisState = 0.0f;
                        state = TraceDangerState.WarningScreen;
                        warningScreenIsActivating = false;
                    }
                    break;
                case TraceDangerState.WarningScreenExiting:
                    num = 13.9f;
                    if (timeThisState > (double) num)
                    {
                        timeThisState = 0.0f;
                        state = TraceDangerState.Countdown;
                        os.display.visible = true;
                        os.netMap.visible = true;
                        os.terminal.visible = true;
                        os.ram.visible = true;
                    }
                    break;
                case TraceDangerState.Countdown:
                    num = 130f;
                    if (timeThisState > (double) num)
                    {
                        timeThisState = 0.0f;
                        state = TraceDangerState.Gameover;
                        CancelTraceDangerSequence();
                        Game1.getSingleton().Exit();
                    }
                    if ((os.timer - (double) onBeatFlashTimer)%1.93766665458679 < 0.0500000007450581)
                        os.warningFlash();
                    PostProcessor.dangerModePercentComplete = Math.Min(timeThisState/(num*0.85f), 1f);
                    PostProcessor.dangerModeEnabled = true;
                    PreventOSRendering = false;
                    break;
                case TraceDangerState.DisconnectedReboot:
                    num = 10f;
                    if (timeThisState > (double) num)
                    {
                        CancelTraceDangerSequence();
                        MusicManager.loadAsCurrentSong(oldSong);
                        os.rebootThisComputer();
                    }
                    break;
            }
            percentComplete = timeThisState/num;
        }

        public void Draw()
        {
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            switch (state)
            {
                case TraceDangerState.WarningScrenIntro:
                    DrawFlashingRedBackground();
                    var destinationRectangle1 = new Rectangle(10, fullscreen.Height/2 - 2, fullscreen.Width - 20, 4);
                    spriteBatch.Draw(Utils.white, destinationRectangle1, Color.Black);
                    destinationRectangle1.Width = (int) (destinationRectangle1.Width*(1.0 - percentComplete));
                    spriteBatch.Draw(Utils.white, destinationRectangle1, Color.Red);
                    break;
                case TraceDangerState.WarningScreen:
                    DrawWarningScreen();
                    break;
                case TraceDangerState.WarningScreenExiting:
                    DrawFlashingRedBackground();
                    var destinationRectangle2 = new Rectangle(10, fullscreen.Height/2 - 2, fullscreen.Width - 20, 4);
                    if (percentComplete > 0.5)
                    {
                        var num =
                            (int)
                                (os.fullscreen.Height*0.7f*
                                 (double) Utils.QuadraticOutCurve((float) ((percentComplete - 0.5)*2.0)));
                        destinationRectangle2.Y = fullscreen.Height/2 - 2 - num/2;
                        destinationRectangle2.Height = num;
                    }
                    spriteBatch.Draw(Utils.white, destinationRectangle2, Color.Black);
                    destinationRectangle2.Width =
                        (int)
                            (destinationRectangle2.Width*
                             (double) Math.Min(1f, Utils.QuadraticOutCurve(percentComplete*2f)));
                    spriteBatch.Draw(Utils.white, destinationRectangle2, Color.DarkRed);
                    var num1 = Utils.QuadraticOutCurve((float) ((percentComplete - 0.5)*2.0));
                    if (percentComplete > 0.5)
                        new ThinBarcode(destinationRectangle2.Width, destinationRectangle2.Height).Draw(spriteBatch,
                            destinationRectangle2.X, destinationRectangle2.Y,
                            Utils.randm(1f) > (double) num1
                                ? Color.Black
                                : Utils.randm(1f) <= (double) num1 || Utils.randm(1f) <= 0.800000011920929
                                    ? Utils.VeryDarkGray
                                    : Utils.AddativeWhite);
                    TextItem.doFontLabel(new Vector2(fullscreen.Width/2 - 250, destinationRectangle2.Y - 70),
                        "INITIALIZING FAILSAFE", GuiData.titlefont, Color.White, 500f, 70f);
                    break;
                case TraceDangerState.Countdown:
                    PreventOSRendering = false;
                    var num2 = timeThisState*0.5f;
                    if (num2 < 1.0)
                    {
                        os.display.visible = num2 > (double) Utils.randm(1f);
                        os.netMap.visible = num2 > (double) Utils.randm(1f);
                        os.terminal.visible = num2 > (double) Utils.randm(1f);
                        os.ram.visible = num2 > (double) Utils.randm(1f);
                    }
                    else
                    {
                        os.display.visible = true;
                        os.netMap.visible = true;
                        os.terminal.visible = true;
                        os.ram.visible = true;
                    }
                    DrawCountdownOverlay();
                    break;
                case TraceDangerState.DisconnectedReboot:
                    DrawDisconnectedScreen();
                    break;
            }
            TextItem.DrawShadow = flag;
        }

        private void DrawDisconnectedScreen()
        {
            spriteBatch.Draw(Utils.white, fullscreen, Color.Black);
            var destinationRectangle = new Rectangle();
            destinationRectangle.X = fullscreen.X + 2;
            destinationRectangle.Width = fullscreen.Width - 4;
            destinationRectangle.Y = fullscreen.Y + fullscreen.Height/6*2;
            destinationRectangle.Height = fullscreen.Height/3;
            spriteBatch.Draw(Utils.white, destinationRectangle, os.indentBackgroundColor);
            var vector2 = GuiData.titlefont.MeasureString("DISCONNECTED");
            var position = new Vector2(destinationRectangle.X + fullscreen.Width/2 - vector2.X/2f,
                fullscreen.Y + fullscreen.Height/2 - 50);
            spriteBatch.DrawString(GuiData.titlefont, "DISCONNECTED", position, os.subtleTextColor);
            DrawFlashInString("Rebooting",
                DrawFlashInString("Preparing for system reboot",
                    DrawFlashInString("Forigen trace averted",
                        DrawFlashInString("IP Address successfully reset",
                            new Vector2(200f, destinationRectangle.Y + destinationRectangle.Height + 20), 4f, 0.2f, true,
                            0.2f), 5f, 0.2f, true, 0.2f), 6f, 0.2f, true, 0.8f), 9f, 0.2f, true, 0.2f);
        }

        private void DrawWarningScreen()
        {
            if (warningScreenIsActivating)
                spriteBatch.Draw(Utils.white, fullscreen, Color.White);
            else
                DrawFlashingRedBackground();
            var text = "WARNING";
            var vector2_1 = titleFont.MeasureString(text);
            var widthTo = fullscreen.Width*0.65f;
            var scale = widthTo/vector2_1.X;
            var vector2_2 = new Vector2(20f, -10f);
            spriteBatch.DrawString(titleFont, text, vector2_2, Color.Black, 0.0f, Vector2.Zero, scale,
                SpriteEffects.None, 0.5f);
            vector2_2.Y += (float) (vector2_1.Y*(double) scale - 55.0);
            TextItem.doFontLabel(vector2_2, "COMPLETED TRACE DETECTED : EMERGENCY RECOVERY MODE ACTIVE", titleFont,
                Color.Black, widthTo, float.MaxValue);
            vector2_2.Y += 40f;
            vector2_2 = DrawFlashInString("Unsyndicated foreign connection detected during active trace", vector2_2,
                0.0f, 0.2f, false, 0.2f);
            vector2_2 = DrawFlashInString(" :: Emergency recovery mode activated", vector2_2, 0.1f, 0.2f, false, 0.2f);
            vector2_2 = DrawFlashInString("-----------------------------------------------------------------------",
                vector2_2, 0.2f, 0.2f, false, 0.2f);
            vector2_2 = DrawFlashInString(" ", vector2_2, 0.5f, 0.2f, false, 0.2f);
            vector2_2 = DrawFlashInString(
                "Automated screening procedures will divert incoming connections temporarily", vector2_2, 0.5f, 0.2f,
                false, 0.2f);
            vector2_2 = DrawFlashInString("This window is a final oppourtunity to regain anonymity.", vector2_2, 0.6f,
                0.2f, false, 0.2f);
            vector2_2 = DrawFlashInString("As your current IP Address is known, it must be changed -", vector2_2, 0.7f,
                0.2f, false, 0.2f);
            vector2_2 = DrawFlashInString("This can ony be done on your currently active ISP's routing server",
                vector2_2, 0.8f, 0.2f, false, 0.2f);
            vector2_2 =
                DrawFlashInString("Reverse tracerouting has located this ISP server's ip address as 68.144.93.18",
                    vector2_2, 0.9f, 0.2f, false, 0.2f);
            vector2_2 = DrawFlashInString(
                "Your local ip : " + os.thisComputer.ip + " must be tracked here and changed.", vector2_2, 1f, 0.2f,
                false, 0.2f);
            vector2_2 = DrawFlashInString(" ", vector2_2, 1.1f, 0.2f, false, 0.2f);
            vector2_2 =
                DrawFlashInString("Failure to complete this while active diversion holds will result in complete",
                    vector2_2, 1.1f, 0.2f, false, 0.2f);
            vector2_2 =
                DrawFlashInString(
                    "and permenant loss of all account data - THIS IS NOT REPEATABLE AND CANNOT BE DELAYED", vector2_2,
                    1.2f, 0.2f, false, 0.2f);
            if (warningScreenIsActivating || timeThisState < 1.20000004768372 ||
                !Button.doButton(789798001, 20, (int) (vector2_2.Y + 10.0), 400, 40, "BEGIN", Color.Black))
                return;
            timeThisState = 0.0f;
            state = TraceDangerState.WarningScreenExiting;
            PreventOSRendering = true;
            onBeatFlashTimer = os.timer;
            warningScreenIsActivating = true;
            spinUpSound.Play(1f, 0.0f, 0.0f);
            os.terminal.inputLocked = false;
            os.delayer.Post(ActionDelayer.Wait(0.1), () => spinUpSound.Play(1f, 0.0f, 0.0f));
            os.delayer.Post(ActionDelayer.Wait(0.4), () => spinUpSound.Play(0.4f, 0.0f, 0.0f));
            os.delayer.Post(ActionDelayer.Wait(0.8), () => spinUpSound.Play(0.2f, 0.1f, 0.0f));
            os.delayer.Post(ActionDelayer.Wait(1.3), () => spinUpSound.Play(0.1f, 0.2f, 0.0f));
            os.delayer.Post(ActionDelayer.Wait(0.01), () => MusicManager.playSongImmediatley("Music/Traced"));
        }

        private Vector2 DrawFlashInString(string text, Vector2 pos, float offset, float transitionInTime = 0.2f,
            bool hasDots = false, float dotsDelayer = 0.2f)
        {
            var vector2 = new Vector2(40f, 0.0f);
            if (timeThisState >= (double) offset)
            {
                var point = Math.Min((timeThisState - offset)/transitionInTime, 1f);
                var position = pos + vector2*(1f - Utils.QuadraticOutCurve(point));
                var str = "";
                if (hasDots)
                {
                    for (var num = timeThisState - offset; num > 0.0 && str.Length < 5; str += ".")
                        num -= dotsDelayer;
                }
                spriteBatch.DrawString(bodyFont, text + str, position, Color.White*point, 0.0f, Vector2.Zero, 0.5f,
                    SpriteEffects.None, 0.4f);
                pos.Y += 17f;
            }
            return pos;
        }

        private void DrawCountdownOverlay()
        {
            var height = 110;
            var destinationRectangle = new Rectangle(0, fullscreen.Height - height - 20, 520, height);
            spriteBatch.Draw(Utils.white, destinationRectangle,
                Color.Lerp(DarkRed, Color.Transparent, Utils.randm(0.2f)));
            var pos = new Vector2(destinationRectangle.X + 6, destinationRectangle.Y + 4);
            TextItem.doFontLabel(pos, "EMERGENCY TRACE AVERSION SEQUENCE", titleFont, Color.White,
                destinationRectangle.Width - 12, 35f);
            pos.Y += 32f;
            TextItem.doFontLabel(pos, "Reset Assigned Ip Address on ISP Mainframe", bodyFont, Color.White,
                destinationRectangle.Width - 10, 20f);
            pos.Y += 16f;
            TextItem.doFontLabel(pos, "ISP Mainframe IP:   68.144.93.18", bodyFont, Color.White,
                destinationRectangle.Width - 10, 20f);
            pos.Y += 16f;
            TextItem.doFontLabel(pos, "YOUR Assigned IP: " + os.thisComputer.ip, bodyFont, Color.White,
                destinationRectangle.Width - 10, 20f);
        }

        private void DrawFlashingRedBackground()
        {
            spriteBatch.Draw(Utils.white, fullscreen, Color.Lerp(BackgroundRed, Color.Black, Utils.randm(0.22f)));
        }

        private enum TraceDangerState
        {
            WarningScrenIntro,
            WarningScreen,
            WarningScreenExiting,
            Countdown,
            Gameover,
            DisconnectedReboot
        }
    }
}
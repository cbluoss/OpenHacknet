// Decompiled with JetBrains decompiler
// Type: Hacknet.EndingSequenceModule
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Effects;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Hacknet
{
    internal class EndingSequenceModule : Module
    {
        private const float SpeechTextHashDelay = 1f;
        private const float SpeechTextPercDelay = 0.5f;
        private const float SpeechTextCharDelay = 0.05f;
        private readonly string BitSpeechText;
        private string[] CreditsData;
        private readonly float creditsPixelsScrollPerSecond = 65f;
        private float creditsScroll;
        private float elapsedTime;
        private readonly float HacknetTitleFreezeTime = 10f;
        public bool IsActive;
        private bool IsInCredits;
        private SoundEffect speech;
        private SoundEffectInstance speechinstance;
        private int SpeechTextIndex;
        private float SpeechTextTimer;
        private SoundEffect spinUpEffect;
        private readonly SoundEffect traceDownEffect;
        private WaveformRenderer waveRender;

        public EndingSequenceModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            bounds = location;
            creditsScroll = os.fullscreen.Height/2;
            spinUpEffect = os.content.Load<SoundEffect>("Music/Ambient/spiral_gauge_up");
            traceDownEffect = os.content.Load<SoundEffect>("SFX/TraceKill");
            BitSpeechText = Utils.readEntireFile("Content/Post/BitSpeech.txt");
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (!IsActive)
                return;
            if (!IsInCredits)
            {
                if (waveRender == null)
                {
                    traceDownEffect.Play();
                    speech = os.content.Load<SoundEffect>("SFX/Ending/EndingSpeech");
                    waveRender = new WaveformRenderer("Content/SFX/Ending/EndingSpeech.wav");
                    speechinstance = speech.CreateInstance();
                    speechinstance.IsLooped = false;
                    CreditsData = Utils.readEntireFile("Content/Post/CreditsData.txt")
                        .Split(Utils.newlineDelim, StringSplitOptions.None);
                    MusicManager.stop();
                }
                if (speechinstance.State == SoundState.Playing)
                {
                    elapsedTime += t;
                    if (elapsedTime > speech.Duration.TotalSeconds)
                    {
                        RollCredits();
                    }
                    else
                    {
                        SpeechTextTimer += t;
                        if (SpeechTextIndex >= BitSpeechText.Length)
                            return;
                        if (BitSpeechText[SpeechTextIndex] == 35)
                        {
                            if (SpeechTextTimer < 1.0)
                                return;
                            --SpeechTextTimer;
                            ++SpeechTextIndex;
                        }
                        else if (BitSpeechText[SpeechTextIndex] == 37)
                        {
                            if (SpeechTextTimer < 0.5)
                                return;
                            SpeechTextTimer -= 0.5f;
                            ++SpeechTextIndex;
                        }
                        else
                        {
                            if (SpeechTextTimer < 0.0500000007450581)
                                return;
                            SpeechTextTimer -= 0.05f;
                            ++SpeechTextIndex;
                        }
                    }
                }
                else
                    speechinstance.Play();
            }
            else
            {
                elapsedTime += t;
                if (elapsedTime <= (double) HacknetTitleFreezeTime)
                    return;
                var num = Math.Min(1f, (float) ((elapsedTime - 10.0)/8.0));
                creditsScroll -= t*creditsPixelsScrollPerSecond*num;
            }
        }

        private void RollCredits()
        {
            IsInCredits = true;
            speechinstance.Stop();
            Settings.soundDisabled = false;
            elapsedTime = 0.0f;
            os.delayer.Post(ActionDelayer.Wait(1.0), () =>
            {
                MusicManager.playSongImmediatley("Music\\Bit(Ending)");
                MediaPlayer.IsRepeating = false;
                AchievementsManager.Unlock("progress_complete", false);
            });
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            var destinationRectangle = os.fullscreen;
            spriteBatch.Draw(Utils.white, destinationRectangle, Color.Black);
            if (waveRender == null)
                return;
            if (!IsInCredits)
            {
                var width = os.fullscreen.Width;
                var height = os.fullscreen.Height;
                waveRender.RenderWaveform(elapsedTime, speech.Duration.TotalSeconds, spriteBatch,
                    new Rectangle(0, os.fullscreen.Height/2 - height/2, width, height));
                var strArray =
                    BitSpeechText.Substring(0, SpeechTextIndex)
                        .Replace("#", "")
                        .Replace("%", "")
                        .Split(Utils.newlineDelim, StringSplitOptions.None);
                var position = new Vector2(bounds.X + 150f, bounds.Y + bounds.Height - 100f);
                var num = 1f;
                for (var index = strArray.Length - 1; index >= 0 && index > strArray.Length - 5; --index)
                {
                    spriteBatch.DrawString(GuiData.smallfont, strArray[index], position, Utils.AddativeWhite*num);
                    num *= 0.6f;
                    position.Y -= GuiData.ActiveFontConfig.tinyFontCharHeight + 8f;
                }
            }
            else
                DrawCredits();
        }

        private void DrawCredits()
        {
            var vector2_1 = new Vector2(0.0f, creditsScroll);
            var width = (int) (os.fullscreen.Width*0.5);
            var height = (int) (os.fullscreen.Height*0.400000005960464);
            var dest = new Rectangle(os.fullscreen.Width/2 - width/2, (int) (vector2_1.Y - (double) (height/2)), width,
                height);
            var destinationRectangle = new Rectangle(os.fullscreen.X, dest.Y + 65, os.fullscreen.Width,
                dest.Height - 135);
            if (elapsedTime >= 1.71000003814697)
            {
                spriteBatch.Draw(Utils.white, destinationRectangle,
                    Color.Lerp(Utils.AddativeRed, Color.Red, 0.2f + Utils.randm(0.05f))*0.5f);
                FlickeringTextEffect.DrawLinedFlickeringText(dest, "HACKNET", 16f, 0.4f, GuiData.titlefont, os,
                    Color.White, 5);
            }
            vector2_1.Y += os.fullscreen.Height/2f;
            for (var index = 0; index < CreditsData.Length; ++index)
            {
                var num = 20f;
                var str = CreditsData[index];
                if (!string.IsNullOrEmpty(str))
                {
                    var text = str;
                    var spriteFont = GuiData.font;
                    var color = Color.White*0.7f;
                    if (str.StartsWith("^"))
                    {
                        text = str.Substring(1);
                        color = Color.Gray*0.6f;
                    }
                    else if (str.StartsWith("%"))
                    {
                        text = str.Substring(1);
                        spriteFont = GuiData.titlefont;
                        num = 90f;
                    }
                    if (str.StartsWith("$"))
                    {
                        text = str.Substring(1);
                        color = Color.Gray*0.6f;
                        spriteFont = GuiData.smallfont;
                    }
                    var vector2_2 = spriteFont.MeasureString(text);
                    var position = vector2_1 + new Vector2(os.fullscreen.Width/2 - vector2_2.X/2f, 0.0f);
                    spriteBatch.DrawString(spriteFont, text, position, color);
                    vector2_1.Y += num;
                }
                vector2_1.Y += num;
            }
            if (vector2_1.Y >= -500.0)
                return;
            CompleteAndReturnToMenu();
        }

        private void CompleteAndReturnToMenu()
        {
            os.Flags.AddFlag("Victory");
            Programs.disconnect(new string[0], os);
            var computer = Programs.getComputer(os, "porthackHeart");
            os.netMap.visibleNodes.Remove(os.netMap.nodes.IndexOf(computer));
            computer.disabled = true;
            computer.daemons.Clear();
            computer.ip = NetworkMap.generateRandomIP();
            os.terminal.inputLocked = false;
            os.ram.inputLocked = false;
            os.netMap.inputLocked = false;
            os.DisableTopBarButtons = false;
            os.canRunContent = true;
            IsActive = false;
            ComputerLoader.loadMission("Content/Missions/CreditsMission.xml");
            os.threadedSaveExecute();
            MediaPlayer.IsRepeating = true;
            MusicManager.playSongImmediatley("Music\\Bit(Ending)");
        }
    }
}
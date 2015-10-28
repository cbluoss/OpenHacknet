// Decompiled with JetBrains decompiler
// Type: Hacknet.PorthackHeartDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class PorthackHeartDaemon : Daemon
    {
        private readonly float FadeoutDelay = 1f;
        private readonly float FadeoutDuration = 10f;
        private float flashOutTime;
        private readonly SoundEffect glowSoundEffect;
        private bool IsFlashingOut;
        private readonly PortHackCubeSequence pcs = new PortHackCubeSequence();
        private bool PlayingHeartbreak;
        private float playTimeExpended;
        private RenderTarget2D rendertarget;
        private SpriteBatch rtSpritebatch;
        private readonly SoundEffect SpinDownEffect;

        public PorthackHeartDaemon(Computer c, OS os)
            : base(c, "Porthack.Heart", os)
        {
            name = "Porthack.Heart";
            SpinDownEffect = os.content.Load<SoundEffect>("SFX/TraceKill");
            glowSoundEffect = os.content.Load<SoundEffect>("SFX/Ending/PorthackSpindown");
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
        }

        private void UpdateForTime(Rectangle bounds, SpriteBatch sb)
        {
            if (playTimeExpended > (double) FadeoutDelay)
            {
                var fade = Math.Min(1f, (playTimeExpended - FadeoutDelay)/FadeoutDuration);
                var correctedbounds = new Rectangle(bounds.X, bounds.Y - Module.PANEL_HEIGHT, bounds.Width,
                    bounds.Height + Module.PANEL_HEIGHT);
                os.postFXDrawActions +=
                    () => Utils.FillEverywhereExcept(correctedbounds, os.fullscreen, sb, Color.Black*fade*0.8f);
            }
            if (pcs.HeartFadeSequenceComplete)
            {
                IsFlashingOut = true;
                flashOutTime += (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
                var num = 3.8f;
                if (flashOutTime > (double) num)
                {
                    flashOutTime = num;
                    os.canRunContent = false;
                    os.endingSequence.IsActive = true;
                    PostProcessor.EndingSequenceFlashOutActive = false;
                    PostProcessor.EndingSequenceFlashOutPercentageComplete = 0.0f;
                    return;
                }
                PostProcessor.EndingSequenceFlashOutPercentageComplete = flashOutTime/num;
            }
            else
                IsFlashingOut = false;
            PostProcessor.EndingSequenceFlashOutActive = IsFlashingOut;
        }

        public void BreakHeart()
        {
            PlayingHeartbreak = true;
            os.terminal.inputLocked = true;
            os.netMap.inputLocked = true;
            os.ram.inputLocked = true;
            os.DisableTopBarButtons = true;
            MusicManager.transitionToSong("Music/Ambient/AmbientDrone_Clipped");
            SpinDownEffect.Play();
            os.delayer.Post(ActionDelayer.Wait(18.0), () => glowSoundEffect.Play());
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            var width = bounds.Width;
            var height = bounds.Height;
            if (rendertarget == null || rendertarget.Width != width || rendertarget.Height != height)
            {
                if (rtSpritebatch == null)
                    rtSpritebatch = new SpriteBatch(sb.GraphicsDevice);
                if (rendertarget != null)
                    rendertarget.Dispose();
                rendertarget = new RenderTarget2D(sb.GraphicsDevice, width, height);
            }
            if (!PlayingHeartbreak)
            {
                TextItem.DrawShadow = false;
                TextItem.doFontLabel(new Vector2(bounds.X + 6, bounds.Y + 2),
                    Utils.FlipRandomChars("PortHack.Heart", 0.003), GuiData.font, Utils.AddativeWhite*0.6f,
                    bounds.Width - 10, 100f);
                TextItem.doFontLabel(new Vector2(bounds.X + 6, bounds.Y + 2),
                    Utils.FlipRandomChars("PortHack.Heart", 0.1), GuiData.font, Utils.AddativeWhite*0.2f,
                    bounds.Width - 10, 100f);
            }
            if (PlayingHeartbreak)
                playTimeExpended += (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
            UpdateForTime(bounds, sb);
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            sb.GraphicsDevice.SetRenderTarget(rendertarget);
            sb.GraphicsDevice.Clear(Color.Transparent);
            rtSpritebatch.Begin();
            var dest = new Rectangle(0, 0, bounds.Width, bounds.Height);
            var vector3_1 = new Vector3(MathHelper.ToRadians(35.4f), MathHelper.ToRadians(45f),
                MathHelper.ToRadians(0.0f));
            var vector3_2 = new Vector3(1f, 1f, 0.0f)*os.timer*0.2f + new Vector3(os.timer*0.1f, os.timer*-0.4f, 0.0f);
            var num = 2.5f;
            if (PlayingHeartbreak)
            {
                if (playTimeExpended < (double) num)
                    Cube3D.RenderWireframe(Vector3.Zero, 2.6f,
                        Vector3.Lerp(Utils.NormalizeRotationVector(vector3_2), vector3_1,
                            Utils.QuadraticOutCurve(playTimeExpended/num)), Color.White);
                else
                    pcs.DrawHeartSequence(dest, (float) os.lastGameTime.ElapsedGameTime.TotalSeconds, 30f);
            }
            else
                Cube3D.RenderWireframe(new Vector3(0.0f, 0.0f, 0.0f), 2.6f, vector3_2, Color.White);
            rtSpritebatch.End();
            sb.GraphicsDevice.SetRenderTarget(currentRenderTarget);
            var rectangle = new Rectangle(bounds.X + (bounds.Width - width)/2, bounds.Y + (bounds.Height - height)/2,
                width, height);
            var rarity = Math.Min(1f, (float) (playTimeExpended/(double) num*0.800000011920929 + 0.200000002980232));
            FlickeringTextEffect.DrawFlickeringSprite(sb, rectangle, rendertarget, 4f, rarity, os, Color.White);
            sb.Draw(rendertarget, rectangle, Utils.AddativeWhite*0.7f);
        }

        public override string getSaveString()
        {
            return "<porthackheart name=\"" + name + "\"/>";
        }
    }
}
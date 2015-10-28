// Decompiled with JetBrains decompiler
// Type: Hacknet.TraceTracker
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class TraceTracker
    {
        private static SpriteFont font;
        private static SoundEffect beep;
        public bool active;
        private readonly string drawtext;
        private float lastFrameTime;
        private readonly OS os;
        private float startingTimer;
        private Computer target;
        public float timeDilation = 1f;
        private float timer;
        private readonly Color timerColor;
        public float timeSinceFreezeRequest;

        public TraceTracker(OS _os)
        {
            os = _os;
            timerColor = new Color(170, 0, 0);
            timer = 0.0f;
            active = false;
            if (font == null)
            {
                font = os.content.Load<SpriteFont>("Kremlin");
                font.Spacing = 11f;
                beep = os.content.Load<SoundEffect>("SFX/beep");
                SoundEffect.MasterVolume = 0.2f;
            }
            drawtext = "TRACE :";
        }

        public void Update(float t)
        {
            var flag = false;
            timeSinceFreezeRequest += t;
            if (timeSinceFreezeRequest < 0.0)
                timeSinceFreezeRequest = 1f;
            if (timeSinceFreezeRequest < 0.200000002980232)
                flag = true;
            if (!active)
                return;
            if (os.connectedComp == null || !os.connectedComp.ip.Equals(target.ip))
            {
                active = false;
                if (timer < 0.5)
                    AchievementsManager.Unlock("trace_close", false);
            }
            else if (!flag)
            {
                timer -= t*timeDilation;
                if (timer <= 0.0)
                {
                    timer = 0.0f;
                    active = false;
                    os.timerExpired();
                }
            }
            var num1 = (float) (timer/(double) startingTimer*100.0);
            var num2 = num1 < 45.0 ? num1 < 15.0 ? 1f : 5f : 10f;
            if (num1%(double) num2 > lastFrameTime%(double) num2)
            {
                beep.Play();
                os.warningFlash();
            }
            lastFrameTime = num1;
        }

        public void start(float t)
        {
            if (active)
                return;
            startingTimer = t;
            timer = t;
            active = true;
            os.warningFlash();
            target = os.connectedComp == null ? os.thisComputer : os.connectedComp;
            Console.WriteLine("Warning flash");
        }

        public void stop()
        {
            active = false;
        }

        public void Draw(SpriteBatch sb)
        {
            if (!active)
                return;
            var text = ((float) (timer/(double) startingTimer*100.0)).ToString("00.00");
            var vector2 = font.MeasureString(text);
            var position = new Vector2(10f, sb.GraphicsDevice.Viewport.Height - vector2.Y);
            sb.DrawString(font, text, position, timerColor);
            position.Y -= 25f;
            sb.DrawString(font, drawtext, position, timerColor, 0.0f, Vector2.Zero, new Vector2(0.3f),
                SpriteEffects.None, 0.5f);
        }
    }
}
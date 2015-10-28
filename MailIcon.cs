// Decompiled with JetBrains decompiler
// Type: Hacknet.MailIcon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MailIcon : MailResponder
    {
        private static readonly float ALERT_TIME = 2.4f;
        private static readonly float SCALE = 1f;
        private static readonly Color uncheckedMailPulseColor = new Color(110, 110, 110, 0);
        private float alertTimer;
        private readonly float bigTexScaleMod = 1f;
        public bool isEnabled = true;
        private bool mailUnchecked;
        private readonly SoundEffect newMailSound;
        private readonly OS os;
        public Vector2 pos;
        private readonly Texture2D tex;
        private readonly Texture2D texBig;

        public MailIcon(OS operatingSystem, Vector2 position)
        {
            os = operatingSystem;
            pos = position;
            alertTimer = 0.0f;
            tex = os.content.Load<Texture2D>("UnopenedMail");
            texBig = os.content.Load<Texture2D>("MailIconBig");
            bigTexScaleMod = tex.Width/(float) texBig.Width;
            newMailSound = os.content.Load<SoundEffect>("SFX/EmailSound");
            if (!os.multiplayer)
                ((MailServer) os.netMap.mailServer.daemons[0]).addResponder(this);
            mailUnchecked = false;
        }

        public void mailSent(string mail, string userTo)
        {
        }

        public void mailReceived(string mail, string userTo)
        {
            if (!userTo.Equals(os.defaultUser.name))
                return;
            alertTimer = ALERT_TIME;
            if (!Settings.soundDisabled)
                newMailSound.Play();
            if (os.connectedComp != null && os.connectedComp.Equals(os.netMap.mailServer))
                return;
            mailUnchecked = true;
        }

        public void Update(float t)
        {
            var num = alertTimer;
            alertTimer -= t;
            if (alertTimer <= 0.0)
                alertTimer = 0.0f;
            if (num <= 0.0 || alertTimer > 0.0)
                return;
            SFX.addCircle(
                pos + new Vector2((float) (tex.Width*(double) SCALE/2.0), (float) (tex.Height*(double) SCALE/2.0)),
                uncheckedMailPulseColor, 280f);
        }

        public void Draw()
        {
            if (
                Button.doButton(45687, (int) pos.X, (int) pos.Y, (int) (tex.Width*(double) SCALE),
                    (int) (tex.Height*(double) SCALE), "", os.topBarIconsColor, tex) && isEnabled)
                connectToMail();
            var percent = alertTimer/ALERT_TIME;
            percent *= percent;
            percent *= percent;
            percent = (float) Math.Pow(percent, 1.0 - percent);
            var iconPositionOffset = new Vector2(-100f, 20f);
            if (alertTimer > 0.0)
            {
                os.postFXDrawActions += () =>
                {
                    var origin = new Vector2((float) (texBig.Width*(double) SCALE/2.0),
                        (float) (texBig.Height*(double) SCALE/2.0));
                    GuiData.spriteBatch.Draw(texBig, pos + origin*bigTexScaleMod + iconPositionOffset*percent,
                        new Rectangle?(), Color.White*(1f - percent), 0.0f, origin,
                        (Vector2.One + Vector2.One*percent*25f)*bigTexScaleMod, SpriteEffects.None, 0.5f);
                };
            }
            else
            {
                if (!mailUnchecked || os.timer%2.0 >= 0.0322580635547638)
                    return;
                SFX.addCircle(
                    pos + new Vector2((float) (tex.Width*(double) SCALE/2.0), (float) (tex.Height*(double) SCALE/2.0)),
                    uncheckedMailPulseColor, 40f);
            }
        }

        public int getWidth()
        {
            return (int) (tex.Width*(double) SCALE);
        }

        public void connectToMail()
        {
            if (os.terminal.preventingExecution)
                return;
            if (!os.netMap.visibleNodes.Contains(os.netMap.nodes.IndexOf(os.netMap.mailServer)))
                os.netMap.discoverNode(os.netMap.mailServer);
            var mailServer = (MailServer) os.netMap.mailServer.daemons[0];
            os.connectedComp = os.netMap.mailServer;
            os.netMap.mailServer.userLoggedIn = true;
            os.display.command = mailServer.name;
            mailServer.viewInbox(os.defaultUser);
            mailUnchecked = false;
        }
    }
}
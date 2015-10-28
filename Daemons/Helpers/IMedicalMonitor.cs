// Decompiled with JetBrains decompiler
// Type: Hacknet.Daemons.Helpers.IMedicalMonitor
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Daemons.Helpers
{
    public interface IMedicalMonitor
    {
        void HeartBeat(float beatTime);

        void Update(float dt);

        void Draw(Rectangle bounds, SpriteBatch sb, Color c, float timeRollback);
    }
}
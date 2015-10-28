// Decompiled with JetBrains decompiler
// Type: Hacknet.ObjectState
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;

namespace Hacknet
{
    public struct ObjectState
    {
        public Vector2 Position { get; set; }

        public float VX { get; set; }

        public float VY { get; set; }

        public int state { get; set; }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.LoadedTexture
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public struct LoadedTexture
    {
        public string path { get; set; }

        public Texture2D tex { get; set; }

        public int retainCount { get; set; }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.BackgroundObject
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public struct BackgroundObject
    {
        public Vector2 pos { get; set; }

        public float Alpha { get; set; }

        public float Rotation { get; set; }

        public float VX { get; set; }

        public float VY { get; set; }

        public string SpritePath { get; set; }

        public float XScale { get; set; }

        public float YScale { get; set; }

        public Texture2D Texture { get; set; }

        public Color Colour { get; set; }

        public float Z { get; set; }
    }
}
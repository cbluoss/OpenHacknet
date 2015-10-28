// Decompiled with JetBrains decompiler
// Type: Hacknet.SpritePlacementData
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;

namespace Hacknet
{
    public struct SpritePlacementData
    {
        public Vector2 pos;
        public float depth;
        public Vector2 scale;
        public int spriteIndex;

        public BackgroundObject BackgroundObjectVersion
        {
            get
            {
                return new BackgroundObject
                {
                    Alpha = 1f,
                    Colour = Color.White,
                    pos = pos,
                    Rotation = 0.0f,
                    SpritePath = "",
                    Texture = null,
                    VX = 0.0f,
                    VY = 0.0f,
                    XScale = scale.X,
                    YScale = scale.Y,
                    Z = depth
                };
            }
        }

        public SpritePlacementData(Vector2 position, Vector2 scales, float layerDepth)
        {
            pos = position;
            scale = scales;
            depth = layerDepth;
            spriteIndex = 0;
        }

        public SpritePlacementData(Vector2 position, Vector2 scales, float layerDepth, int SpriteIndex)
        {
            pos = position;
            scale = scales;
            depth = layerDepth;
            spriteIndex = SpriteIndex;
        }
    }
}
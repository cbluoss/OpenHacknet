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
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
// Decompiled with JetBrains decompiler
// Type: Hacknet.CollidableRectangle
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;

namespace Hacknet
{
    public struct CollidableRectangle
    {
        private static readonly CollidableRectangle z = new CollidableRectangle(0.0f, 0.0f, Vector2.Zero);
        public Vector2 position;
        private float b_width;
        private float b_height;
        public Rectangle ck;

        public static CollidableRectangle Zero
        {
            get { return z; }
        }

        public Vector2 pos
        {
            get { return position; }
            set
            {
                position = value;
                ck.X = (int) pos.X;
                ck.Y = (int) pos.Y;
            }
        }

        public float Width
        {
            get { return b_width; }
            set
            {
                b_width = value;
                ck.Width = (int) value;
            }
        }

        public float Height
        {
            get { return b_height; }
            set
            {
                b_height = value;
                ck.Height = (int) value;
            }
        }

        public Rectangle checkable
        {
            get { return ck; }
        }

        public CollidableRectangle(float w, float h, Vector2 p)
        {
            b_width = w;
            b_height = h;
            position = p;
            ck = new Rectangle();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CollidableRectangle))
                return false;
            return ((CollidableRectangle) obj).checkable.Equals(checkable);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
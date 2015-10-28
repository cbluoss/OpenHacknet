// Decompiled with JetBrains decompiler
// Type: Hacknet.ConnectedNodeEffect
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class ConnectedNodeEffect
    {
        private const int NUMBER_OF_SEGMENTS = 7;
        private const float MIN_DISTANCE = 18f;
        private const float MAX_DISTANCE = 30f;
        private static List<Texture2D> textures;
        public Color color;
        private float[] distance;
        public bool Intense;
        private float[] offset;
        private Vector2 origin;
        private readonly OS os;
        public float ScaleFactor = 1f;
        private int[] tex;
        private float[] timescale;

        public ConnectedNodeEffect(OS os)
        {
            this.os = os;
            init(false);
        }

        public ConnectedNodeEffect(OS os, bool intense)
        {
            this.os = os;
            Intense = intense;
            init(intense);
        }

        private void init(bool intesne = false)
        {
            if (textures == null)
            {
                textures = new List<Texture2D>();
                textures.Add(os.content.Load<Texture2D>("rotator"));
                textures.Add(os.content.Load<Texture2D>("rotator2"));
                textures.Add(os.content.Load<Texture2D>("rotator3"));
                textures.Add(os.content.Load<Texture2D>("rotator4"));
                textures.Add(os.content.Load<Texture2D>("rotator5"));
            }
            origin = new Vector2(textures[0].Width/2, textures[0].Height/2);
            var length = (intesne ? 2 : 1)*7;
            tex = new int[length];
            distance = new float[length];
            offset = new float[length];
            timescale = new float[length];
            color = new Color(140, 12, 12, 50);
            reset();
        }

        public void reset()
        {
            var num = Intense ? 1.5f : 1f;
            for (var index = 0; index < 7; ++index)
            {
                tex[index] = Utils.random.Next(textures.Count - 1);
                distance[index] = (float) (Utils.random.NextDouble()*(30.0*num - 18.0/num) + 18.0/num);
                offset[index] = (float) (Utils.random.NextDouble()*(2.0*Math.PI));
                timescale[index] = (float) Utils.random.NextDouble();
            }
        }

        public void draw(SpriteBatch sb, Vector2 pos)
        {
            for (var index = 0; index < 7; ++index)
            {
                var origin = this.origin + new Vector2(0.0f, distance[index]);
                var rotation =
                    (float) (os.timer*timescale[index]*0.200000002980232%1.0*(2.0*Math.PI)*(index%2 == 0 ? 1.0 : -1.0)) +
                    offset[index];
                var scale =
                    new Vector2(
                        (float) (distance[index]/(30.0*(Intense ? 1.5 : 1.0))*0.699999988079071 + 0.300000011920929), 1f);
                scale *= ScaleFactor;
                sb.Draw(textures[tex[index]], pos, new Rectangle?(), color, rotation, origin, scale, SpriteEffects.None,
                    0.5f);
            }
        }
    }
}
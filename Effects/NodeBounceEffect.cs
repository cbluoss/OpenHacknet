using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    public class NodeBounceEffect
    {
        private float delayTillNextBounce;
        private readonly List<Vector2> locations = new List<Vector2>();
        public int maxNodes = 200;
        public float NodeHitDelay = 0.2f;
        public float TimeBetweenBounces = 0.07f;
        private float timeToNextBounce;

        public NodeBounceEffect()
        {
            locations.Add(new Vector2(Utils.rand(), Utils.rand()));
            locations.Add(new Vector2(Utils.rand(), Utils.rand()));
        }

        public void Update(float t, Action<Vector2> nodeHitAction = null)
        {
            timeToNextBounce -= t;
            if (timeToNextBounce > 0.0)
                return;
            if (delayTillNextBounce <= 0.0)
            {
                if (nodeHitAction != null)
                    nodeHitAction(locations[locations.Count - 1]);
                locations.Add(new Vector2(Utils.rand(), Utils.rand()));
                timeToNextBounce = TimeBetweenBounces;
                delayTillNextBounce = NodeHitDelay;
                while (locations.Count > maxNodes)
                    locations.RemoveAt(0);
            }
            else
            {
                delayTillNextBounce -= t;
                timeToNextBounce = 0.0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle bounds, Color lineColor, Color nodeColor)
        {
            var vector2_1 = locations[0];
            var vector2_2 = new Vector2(bounds.X + 2f, bounds.Y + 26f);
            var vector2_3 = new Vector2(bounds.Width - 4f, bounds.Height - 30f);
            if (vector2_3.X <= 0.0 || vector2_3.Y <= 0.0)
                return;
            for (var index = 1; index < locations.Count; ++index)
            {
                var vector2_4 = locations[index];
                if (index == locations.Count - 1)
                    vector2_4 = Vector2.Lerp(vector2_1, vector2_4,
                        (float) (1.0 - timeToNextBounce/(double) TimeBetweenBounces));
                Utils.drawLine(spriteBatch, vector2_2 + vector2_1*vector2_3, vector2_2 + vector2_4*vector2_3,
                    Vector2.Zero, lineColor*(index/(float) locations.Count), 0.4f);
                vector2_1 = locations[index];
            }
            for (var index = 1; index < locations.Count; ++index)
                spriteBatch.Draw(Utils.white, locations[index]*vector2_3 + vector2_2, nodeColor);
        }
    }
}
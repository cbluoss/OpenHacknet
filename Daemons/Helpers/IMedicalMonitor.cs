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
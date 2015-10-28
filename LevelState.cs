using System.Collections.Generic;

namespace Hacknet
{
    public struct LevelState
    {
        public double TimeTaken { get; set; }

        public List<ObjectState> BackgroundObjects { get; set; }

        public List<ObjectState> UpdatableObjects { get; set; }

        public ObjectState PlayerState { get; set; }
    }
}
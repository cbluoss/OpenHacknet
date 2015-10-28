// Decompiled with JetBrains decompiler
// Type: Hacknet.LevelState
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

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
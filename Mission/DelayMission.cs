// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.DelayMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class DelayMission : MisisonGoal
    {
        private DateTime? firstRequest = new DateTime?();
        public float time;

        public DelayMission(float time)
        {
            this.time = time;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            if (firstRequest.HasValue)
                return (DateTime.Now - firstRequest.Value).TotalSeconds >= time;
            firstRequest = DateTime.Now;
            return false;
        }
    }
}
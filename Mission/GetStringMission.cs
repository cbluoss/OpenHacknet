// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.GetStringMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class GetStringMission : MisisonGoal
    {
        public string target;

        public GetStringMission(string targetData)
        {
            target = targetData;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            if (additionalDetails == null)
                return false;
            for (var index = 0; index < additionalDetails.Count; ++index)
            {
                if (additionalDetails[index].Contains(target))
                    return true;
            }
            return false;
        }
    }
}
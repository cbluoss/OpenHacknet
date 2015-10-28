// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.CheckFlagSetMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class CheckFlagSetMission : MisisonGoal
    {
        private readonly OS os;
        public string target;

        public CheckFlagSetMission(string targetFlagName, OS _os)
        {
            target = targetFlagName;
            os = _os;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return os.Flags.HasFlag(target);
        }
    }
}
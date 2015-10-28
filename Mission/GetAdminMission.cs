// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.GetAdminMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class GetAdminMission : MisisonGoal
    {
        public OS os;
        public Computer target;

        public GetAdminMission(string compIP, OS _os)
        {
            os = _os;
            target = Programs.getComputer(os, compIP);
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return target.adminIP.Equals(os.thisComputer.ip);
        }

        public override void reset()
        {
        }
    }
}
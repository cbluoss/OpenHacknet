// Decompiled with JetBrains decompiler
// Type: Hacknet.SaveData
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;

namespace Hacknet
{
    [Serializable]
    public class SaveData
    {
        private ActiveMission mission;
        private List<Computer> nodes;

        public void addNodes(object nodesToAdd)
        {
            nodes = (List<Computer>) nodesToAdd;
        }

        public void setMission(object missionToAdd)
        {
            mission = (ActiveMission) missionToAdd;
        }

        public object getNodes()
        {
            return nodes;
        }

        public object getMission()
        {
            return mission;
        }
    }
}
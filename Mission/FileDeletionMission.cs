// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.FileDeletionMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class FileDeletionMission : MisisonGoal
    {
        public Folder container;
        public OS os;
        public string target;
        public Computer targetComp;
        public string targetData;

        public FileDeletionMission(string path, string filename, string computerIP, OS _os)
        {
            target = filename;
            os = _os;
            var computer = Programs.getComputer(os, computerIP);
            targetComp = computer;
            container = computer.getFolderFromPath(path, false);
            for (var index = 0; index < container.files.Count; ++index)
            {
                if (container.files[index].name.Equals(target))
                    targetData = container.files[index].data;
            }
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            for (var index = 0; index < container.files.Count; ++index)
            {
                if (container.files[index].name.Equals(target) &&
                    (container.files[index].data.Equals(targetData) || targetData == null))
                    return false;
            }
            return true;
        }

        public override string TestCompletable()
        {
            var str = "";
            if (container.searchForFile(target) == null)
                str = str + "File to delete (" + container.name + "/" + target + ") does not exist!";
            return str;
        }
    }
}
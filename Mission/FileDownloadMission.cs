// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.FileDownloadMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class FileDownloadMission : FileDeletionMission
    {
        public FileDownloadMission(string path, string filename, string computerIP, OS os)
            : base(path, filename, computerIP, os)
        {
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            var folder = os.thisComputer.files.root;
            for (var index = 0; index < folder.folders.Count; ++index)
            {
                if (folder.folders[index].containsFile(target, targetData))
                    return true;
            }
            return false;
        }

        public override string TestCompletable()
        {
            var str = "";
            if (container.searchForFile(target) == null)
                str = str + "File to download (" + container.name + "/" + target + ") does not exist!";
            return str;
        }
    }
}
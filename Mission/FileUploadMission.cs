// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.FileUploadMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class FileUploadMission : MisisonGoal
    {
        public Folder container;
        public Folder destinationFolder;
        public OS os;
        public string target;
        public Computer targetComp;
        public string targetData;
        public Computer uploadTargetComp;

        public FileUploadMission(string path, string filename, string computerWithFileIP, string computerToUploadToIP,
            string destToUploadToPath, OS _os, bool needsDecrypt = false, string decryptPass = "")
        {
            target = filename;
            os = _os;
            var computer1 = Programs.getComputer(os, computerWithFileIP);
            targetComp = computer1;
            container = computer1.getFolderFromPath(path, false);
            for (var index = 0; index < container.files.Count; ++index)
            {
                if (container.files[index].name.Equals(target))
                {
                    targetData = container.files[index].data;
                    if (needsDecrypt)
                        targetData = FileEncrypter.DecryptString(targetData, decryptPass)[2];
                }
            }
            var computer2 = Programs.getComputer(os, computerToUploadToIP);
            uploadTargetComp = computer2;
            destinationFolder = computer2.getFolderFromPath(destToUploadToPath, false);
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            for (var index = 0; index < destinationFolder.files.Count; ++index)
            {
                if (destinationFolder.files[index].data.Equals(targetData))
                    return true;
            }
            return false;
        }

        public override string TestCompletable()
        {
            var str = "";
            if (container.searchForFile(target) == null)
                str = str + "File to upload (" + container.name + "/" + target + ") does not exist!";
            return str;
        }
    }
}
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
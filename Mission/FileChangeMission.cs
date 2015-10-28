using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class FileChangeMission : MisisonGoal
    {
        public Folder container;
        public bool isRemoval;
        public OS os;
        public string target;
        public Computer targetComp;
        public string targetData;
        public string targetKeyword;

        public FileChangeMission(string path, string filename, string computerIP, string targetKeyword, OS _os,
            bool isRemoval = false)
        {
            target = filename;
            this.targetKeyword = targetKeyword.ToLower();
            this.isRemoval = isRemoval;
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
                    container.files[index].data.ToLower().Contains(targetKeyword))
                    return !isRemoval;
            }
            return isRemoval;
        }

        public override string TestCompletable()
        {
            var str = "";
            if (container.searchForFile(target) != null)
                str = str + "File to change (" + container.name + "/" + target + ") does not exist!";
            return str;
        }
    }
}
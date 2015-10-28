using System.Collections.Generic;

namespace Hacknet
{
    internal static class ProgramList
    {
        public static List<string> programs;

        public static void init()
        {
            programs = new List<string>();
            programs.Add("ls");
            programs.Add("cd");
            programs.Add("probe");
            programs.Add("scan");
            programs.Add("ps");
            programs.Add("kill");
            programs.Add("connect");
            programs.Add("dc");
            programs.Add("disconnect");
            programs.Add("help");
            programs.Add("exe");
            programs.Add("cat");
            programs.Add("scp");
            programs.Add("rm");
            programs.Add("openCDTray");
            programs.Add("closeCDTray");
            programs.Add("login");
            programs.Add("reboot");
            programs.Add("mv");
            programs.Add("upload");
            programs.Add("analyze");
            programs.Add("solve");
            programs.Add("addNote");
        }

        public static List<string> getExeList(OS os)
        {
            var list = new List<string>();
            list.Add("PortHack");
            list.Add("ForkBomb");
            list.Add("Shell");
            list.Add("Tutorial");
            list.Add("Notes");
            var folder = os.thisComputer.files.root.folders[2];
            for (var index = 0; index < folder.files.Count; ++index)
                list.Add(folder.files[index].name.Replace(".exe", ""));
            return list;
        }
    }
}
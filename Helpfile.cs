using System.Collections.Generic;
using System.Text;

namespace Hacknet
{
    internal class Helpfile
    {
        private static readonly int ITEMS_PER_PAGE = 10;

        public static string prefix =
            "---------------------------------\nCommand List - Page [PAGENUM] of [TOTALPAGES]:\n";

        private static readonly string postfix =
            "help [PAGE NUMBER]\n Displays the specified page of commands.\n---------------------------------\n";

        public static List<string> help;

        public static void init()
        {
            help = new List<string>();
            var str = "\n    ";
            help.Add("help [PAGE NUMBER]" + str + "Displays the specified page of commands.");
            help.Add("scp [filename] [OPTIONAL: destination]" + str +
                     "Copies file named [filename] from remote machine to specified local folder (/bin default)");
            help.Add("scan" + str + "Scans for links on the connected machine and adds them to the Map");
            help.Add("rm [filename (or use * for all files in folder)]" + str + "Deletes specified file(s)");
            help.Add("ps" + str + "Lists currently running processes");
            help.Add("kill [PID]" + str + "Kills Process number [PID]");
            help.Add("ls" + str + "Lists all files in current directory");
            help.Add("cd [foldername]" + str + "Moves current working directory to the specified folder");
            help.Add("mv [FILE] [DESTINATION]" + str + "Moves or renames [FILE] to [DESTINATION]" + str +
                     "(i.e: mv hi.txt ../bin/hi.txt)");
            help.Add("connect [ip]" + str + "Connect to an External Computer");
            help.Add("probe" + str + "Scans the connected machine for" + str + "active ports and security level");
            help.Add("exe" + str +
                     "Lists all available executables in the local /bin/ folder (Includes hidden and embedded executables)");
            help.Add("disconnect" + str + "Terminate the current open connection. ALT: \"dc\"");
            help.Add("cat [filename]" + str + "Displays contents of file");
            help.Add("openCDTray" + str + "Opens the connected Computer's CD Tray");
            help.Add("closeCDTray" + str + "Closes the connected Computer's CD Tray");
            help.Add("reboot [OPTIONAL: -i]" + str + "Reboots the connected computer. The -i flag reboots instantly");
            help.Add("replace [filename] \"target\" \"replacement\"" + str +
                     "Replaces the target text in the file with the replacement");
            help.Add("analyze" + str + "Performs an analysis pass on the firewall of the target machine");
            help.Add("solve [FIREWALL SOLUTION]" + str +
                     "Attempts to solve the firewall of target machine to allow UDP Traffic");
            help.Add("login" + str + "Requests a username and password to log in to the connected system");
            help.Add("upload [LOCAL FILE PATH]" + str +
                     "Uploads the indicated file on your local machine to the current connected directory");
            help.Add("clear" + str + "Clears the terminal");
        }

        public static void writeHelp(OS os, int page = 0)
        {
            if (page == 0)
                page = 1;
            var num = (page - 1)*ITEMS_PER_PAGE;
            if (num >= help.Count)
                num = 0;
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(
                prefix.Replace("[PAGENUM]", string.Concat(page))
                    .Replace("[TOTALPAGES]", string.Concat(getNumberOfPages())) + "\n");
            for (var index = num; index < help.Count && index < num + ITEMS_PER_PAGE; ++index)
                stringBuilder.Append((index == 0 ? " " : "") + help[index] + "\n  \n ");
            os.write(stringBuilder.ToString() + (object) "\n" + postfix);
        }

        public static int getNumberOfPages()
        {
            return help.Count/ITEMS_PER_PAGE + 1;
        }
    }
}
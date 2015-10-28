using System;
using System.Collections.Generic;
using System.Threading;

namespace Hacknet
{
    public class HackerScriptExecuter
    {
        public const string splitDelimiter = " $#%#$\r\n";

        public static void runScript(string scriptName, object os)
        {
            var separator = new string[1]
            {
                " $#%#$\r\n"
            };
            var script = Utils.readEntireFile("Content/HackerScripts/" + scriptName)
                .Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var thread = new Thread(() => executeThreadedScript(script, (OS) os))
            {
                IsBackground = true,
                CurrentCulture = Game1.culture,
                CurrentUICulture = Game1.culture
            };
            thread.Name = "OpposingHackerThread";
            thread.Start();
        }

        private static void executeThreadedScript(string[] script, OS os)
        {
            var nullable = new KeyValuePair<string, string>?();
            var flag1 = false;
            var target = os.thisComputer;
            Computer source = null;
            var timeout = TimeSpan.FromSeconds(0.5);
            for (var index = 0; index < script.Length; ++index)
            {
                if (source != null && source.disabled)
                {
                    Multiplayer.parseInputMessage(getBasicNetworkCommand("cDisconnect", target, source), os);
                    Console.WriteLine("Early Script Exit on Source Disable");
                    return;
                }
                var strArray = script[index].Split(Utils.spaceDelim, StringSplitOptions.RemoveEmptyEntries);
                var flag2 = true;
                switch (strArray[0])
                {
                    case "config":
                        target = (Computer) ComputerLoader.findComputer(strArray[1]);
                        source = (Computer) ComputerLoader.findComputer(strArray[2]);
                        timeout = TimeSpan.FromSeconds(Convert.ToDouble(strArray[3]));
                        flag2 = false;
                        nullable = new KeyValuePair<string, string>(source.ip, target.ip);
                        os.ActiveHackers.Add(nullable.Value);
                        break;
                    case "delay":
                        Thread.Sleep(TimeSpan.FromSeconds(Convert.ToDouble(strArray[1])));
                        break;
                    case "connect":
                        Multiplayer.parseInputMessage(getBasicNetworkCommand("cConnection", target, source), os);
                        break;
                    case "openPort":
                        Multiplayer.parseInputMessage(
                            getBasicNetworkCommand("cPortOpen", target, source) + " " + strArray[1], os);
                        break;
                    case "delete":
                        var pathString = getPathString(strArray[1], os, target.files.root);
                        Multiplayer.parseInputMessage(
                            "cDelete #" + target.ip + "#" + source.ip + "#" + strArray[2] + pathString, os);
                        break;
                    case "reboot":
                        os.runCommand("reboot");
                        break;
                    case "forkbomb":
                        Multiplayer.parseInputMessage(getBasicNetworkCommand("eForkBomb", target, source), os);
                        break;
                    case "disconnect":
                        Multiplayer.parseInputMessage(getBasicNetworkCommand("cDisconnect", target, source), os);
                        break;
                }
                try
                {
                    if (flag2)
                    {
                        if (!os.thisComputer.disabled)
                        {
                            os.beepSound.Play();
                            if (!flag1)
                            {
                                os.IncConnectionOverlay.Activate();
                                flag1 = true;
                            }
                        }
                    }
                }
                catch
                {
                    return;
                }
                Thread.Sleep(timeout);
            }
            if (!nullable.HasValue)
                return;
            os.ActiveHackers.Remove(nullable.Value);
        }

        private static string getBasicNetworkCommand(string targetCommand, Computer target, Computer source)
        {
            return targetCommand + " " + target.ip + " " + source.ip;
        }

        private static string getPathString(string fPath, OS os, Folder f)
        {
            var navigationPathAtPath = Programs.getNavigationPathAtPath(fPath, os, f);
            var str = "";
            for (var index = 0; index < navigationPathAtPath.Count; ++index)
                str = str + (object) "#" + navigationPathAtPath[index];
            return str;
        }
    }
}
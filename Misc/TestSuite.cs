using System;
using System.Collections.Generic;
using System.IO;
using Hacknet.PlatformAPI.Storage;

namespace Hacknet.Misc
{
    public class TestSuite
    {
        public static string TestSaveLoadOnFile(ScreenManager screenMan)
        {
            var text = "__hacknettestaccount";
            var pass = "__testingpassword";
            SaveFileManager.AddUser(text, pass);
            var saveFileNameForUsername = SaveFileManager.GetSaveFileNameForUsername(text);
            OS.TestingPassOnly = true;
            var text2 = "";
            var oS = new OS();
            oS.SaveGameUserName = saveFileNameForUsername;
            oS.SaveUserAccountName = text;
            screenMan.AddScreen(oS, screenMan.controllingPlayer);
            oS.threadedSaveExecute();
            var nodes = oS.netMap.nodes;
            screenMan.RemoveScreen(oS);
            OS.WillLoadSave = true;
            oS = new OS();
            oS.SaveGameUserName = saveFileNameForUsername;
            oS.SaveUserAccountName = text;
            screenMan.AddScreen(oS, screenMan.controllingPlayer);
            var text3 = "Serialization and Integrity Test Report:\r\n";
            Console.WriteLine(text3);
            text2 += text3;
            new List<string>();
            new List<string>();
            var num = 0;
            text2 += getTestingReportForLoadComparison(oS, nodes, num, out num);
            text2 = text2 + "\r\n" + TestMissions(oS);
            for (var i = 0; i < oS.netMap.nodes.Count; i++)
            {
                var root = oS.netMap.nodes[i].files.root;
                DeleteAllFilesRecursivley(root);
            }
            oS.SaveGameUserName = saveFileNameForUsername;
            oS.SaveUserAccountName = text;
            oS.threadedSaveExecute();
            nodes = oS.netMap.nodes;
            screenMan.RemoveScreen(oS);
            OS.WillLoadSave = true;
            oS = new OS();
            oS.SaveGameUserName = saveFileNameForUsername;
            oS.SaveUserAccountName = text;
            screenMan.AddScreen(oS, screenMan.controllingPlayer);
            var num2 = num;
            var testingReportForLoadComparison = getTestingReportForLoadComparison(oS, nodes, num, out num);
            if (num2 != num)
            {
                text2 = text2 + "\r\nAll Files Deleted pass:\r\n" + testingReportForLoadComparison;
            }
            screenMan.RemoveScreen(oS);
            OS.TestingPassOnly = false;
            SaveFileManager.DeleteUser(text);
            var text4 = string.Concat("\r\nTest complete - ", num, " errors found.\r\nTested ", nodes.Count,
                " generated nodes vs ", oS.netMap.nodes.Count, " loaded nodes");
            text2 += text4;
            Console.WriteLine(text4);
            MusicManager.stop();
            return text2;
        }

        private static void DeleteAllFilesRecursivley(Folder f)
        {
            f.files.Clear();
            for (var i = 0; i < f.folders.Count; i++)
            {
                DeleteAllFilesRecursivley(f.folders[i]);
            }
        }

        private static string getTestingReportForLoadComparison(OS os, List<Computer> oldComps, int currentErrorCount,
            out int errorCount)
        {
            var list = new List<string>();
            var list2 = new List<string>();
            var text = "";
            errorCount = currentErrorCount;
            for (var i = 0; i < oldComps.Count; i++)
            {
                var computer = oldComps[i];
                var computer2 = Programs.getComputer(os, computer.ip);
                Assert(computer.name, computer2.name);
                var text2 = computer2.files.TestEquals(computer.files);
                if (list.Contains(computer2.idName))
                {
                    var num = list.IndexOf(computer2.idName);
                    object obj = text2;
                    text2 = string.Concat(obj, "Duplicate ID Found - \"", computer2.idName, "\" from ", list2[num], "@",
                        num, " current: ", computer2.name, "@", i);
                }
                list.Add(computer2.idName);
                list2.Add(computer2.name);
                if (!Utils.FloatEquals(computer.startingOverloadTicks, computer2.startingOverloadTicks))
                {
                    object obj2 = text2;
                    text2 = string.Concat(obj2, "Proxy timer difference - \"", computer2.name, "\" from Base:",
                        computer.startingOverloadTicks, " to Current: ", computer2.startingOverloadTicks);
                }
                if ((computer.firewall != null || computer2.firewall != null) &&
                    !computer.firewall.Equals(computer2.firewall))
                {
                    var text3 = text2;
                    text2 = string.Concat(text3, "Firewall difference - \"", computer2.name, "\" from Base:",
                        computer.firewall.ToString(), "\r\n to Current: ", computer2.firewall.ToString(), "\r\n");
                }
                if (computer.icon != computer2.icon)
                {
                    var text4 = text2;
                    text2 = string.Concat(text4, "Icon difference - \"", computer2.name, "\" from Base:", computer.icon,
                        " to Current: ", computer2.icon);
                }
                if (computer.portsNeededForCrack != computer2.portsNeededForCrack)
                {
                    object obj3 = text2;
                    text2 = string.Concat(obj3, "Port for crack difference - \"", computer2.name, "\" from Base:",
                        computer.portsNeededForCrack, " to Current: ", computer2.portsNeededForCrack);
                }
                for (var j = 0; j < computer.links.Count; j++)
                {
                    if (computer.links[j] != computer2.links[j])
                    {
                        object obj4 = text2;
                        text2 = string.Concat(obj4, "Link difference - \"", computer2.name, "\" @", j, " ",
                            computer.links[j], " vs ", computer2.links[j]);
                    }
                }
                if (!Utils.FloatEquals(computer.location.X, computer2.location.X) ||
                    !Utils.FloatEquals(computer.location.Y, computer2.location.Y))
                {
                    object obj5 = text2;
                    text2 = string.Concat(obj5, "Location difference - \"", computer2.name, "\" from Base:",
                        computer.location, " to Current: ", computer2.location);
                }
                if (!Utils.FloatEquals(computer.traceTime, computer2.traceTime))
                {
                    object obj6 = text2;
                    text2 = string.Concat(obj6, "Trace timer difference - \"", computer2.name, "\" from Base:",
                        computer.traceTime, " to Current: ", computer2.traceTime);
                }
                if (computer.adminPass != computer2.adminPass)
                {
                    var text5 = text2;
                    text2 = string.Concat(text5, "Password Difference: expected \"", computer.adminPass, "\" but got \"",
                        computer2.adminPass, "\"\r\n");
                }
                if (computer.adminIP != computer2.adminIP)
                {
                    var text6 = text2;
                    text2 = string.Concat(text6, "Admin IP Difference: expected \"", computer.adminIP, "\" but got \"",
                        computer2.adminIP, "\"\r\n");
                }
                for (var k = 0; k < computer.users.Count; k++)
                {
                    if (!computer.users[k].Equals(computer2.users[k]))
                    {
                        object obj7 = text2;
                        text2 = string.Concat(obj7, "User difference - \"", computer2.name, "\" @", k, " ",
                            computer.users[k], " vs ", computer2.users[k]);
                    }
                }
                for (var l = 0; l < computer.daemons.Count; l++)
                {
                    if (computer.daemons[l].getSaveString() != computer2.daemons[l].getSaveString())
                    {
                        var text7 = text2;
                        text2 = string.Concat(text7, "Daemon Difference: expected \r\n-----\r\n",
                            computer.daemons[l].getSaveString(), "\r\n----- but got -----\r\n",
                            computer2.daemons[l].getSaveString(), "\r\n-----\r\n");
                    }
                }
                if (computer.hasProxy != computer2.hasProxy)
                {
                    object obj8 = text2;
                    text2 = string.Concat(obj8, "Proxy Difference: OldProxy:", computer.hasProxy, " vs NewProxy:",
                        computer2.hasProxy);
                }
                if (computer.proxyActive != computer2.proxyActive)
                {
                    object obj9 = text2;
                    text2 = string.Concat(obj9, "Proxy Active Difference: OldProxy:", computer.proxyActive,
                        " vs NewProxy:", computer2.proxyActive);
                }
                if (computer.portsNeededForCrack != computer2.portsNeededForCrack)
                {
                    object obj10 = text2;
                    text2 = string.Concat(obj10, "Ports for crack Difference: Old:", computer.portsNeededForCrack,
                        " vs New:", computer2.portsNeededForCrack);
                }
                if (computer.admin != null && computer2.admin != null &&
                    computer.admin.GetType() != computer2.admin.GetType())
                {
                    object obj11 = text2;
                    text2 = string.Concat(obj11, "SecAdmin Difference: Old:", computer.admin, " vs New:",
                        computer2.admin);
                }
                if (computer.admin != null && computer.admin.IsSuper != computer2.admin.IsSuper)
                {
                    object obj12 = text2;
                    text2 = string.Concat(obj12, "SecAdmin Super Difference: Old:", computer.admin.IsSuper, " vs New:",
                        computer2.admin.IsSuper);
                }
                if (computer.admin != null && computer.admin.ResetsPassword != computer2.admin.ResetsPassword)
                {
                    object obj13 = text2;
                    text2 = string.Concat(obj13, "SecAdmin PassReset Difference: Old:", computer.admin.ResetsPassword,
                        " vs New:", computer2.admin.ResetsPassword);
                }
                if (text2 != null)
                {
                    var text8 = string.Concat("\r\nErrors in ", computer.idName, " - \"", computer.name, "\"\r\n", text2,
                        "\r\n");
                    text = text + text8 + "\r\n";
                    Console.WriteLine(text8);
                    errorCount++;
                }
                else
                {
                    Console.Write(".");
                    text += ".";
                }
            }
            return text;
        }

        public static string TestMissions(object os_obj)
        {
            var os = (OS) os_obj;
            var str = "";
            str += TestMission("Content/Missions/BitMission0.xml", os);
            var text = "Content/Missions/Entropy/StartingSet/";
            var directoryInfo = new DirectoryInfo(text);
            var files = directoryInfo.GetFiles("*.xml");
            for (var i = 0; i < files.Length; i++)
            {
                var missionName = text + files[i].Name;
                str += TestMission(missionName, os);
            }
            str += TestMission("Content/Missions/MainHub/Intro/Intro01.xml", os);
            text = "Content/Missions/MainHub/FirstSet/";
            directoryInfo = new DirectoryInfo(text);
            files = directoryInfo.GetFiles("*.xml");
            for (var j = 0; j < files.Length; j++)
            {
                var missionName2 = text + files[j].Name;
                str += TestMission(missionName2, os);
            }
            str += TestMission("Content/Missions/MainHub/BitSet/Missions/BitHubSet01.xml", os);
            return str + TestMission("Content/Missions/BitPath/BitAdv_Intro.xml", os);
        }

        private static string TestMission(string missionName, OS os)
        {
            var text = "";
            try
            {
                var activeMission = (ActiveMission) ComputerLoader.readMission(missionName);
                var text2 = "";
                for (var i = 0; i < activeMission.goals.Count; i++)
                {
                    var text3 = activeMission.goals[i].TestCompletable();
                    if (text3 != null && text3.Length > 0)
                    {
                        object obj = text2;
                        text2 = string.Concat(obj, missionName, " Goal[", i, "] ", activeMission.goals[i].ToString(),
                            " :: ", text3, "\r\n");
                    }
                }
                if (text2.Length > 0)
                {
                    text = text + text2 + "--------------\r\n";
                }
                if (activeMission.nextMission != null && activeMission.nextMission.ToLower() != "none")
                {
                    text += TestMission("Content/Missions/" + activeMission.nextMission, os);
                }
            }
            catch (Exception ex)
            {
                var text4 = text;
                text = string.Concat(text4, "Error Loading ", missionName, "\r\n", ex.ToString());
            }
            return text;
        }

        private static void Assert(string first, string second)
        {
            if (first != second)
            {
                throw new InvalidProgramException();
            }
        }
    }
}
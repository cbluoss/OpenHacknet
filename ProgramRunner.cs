// Decompiled with JetBrains decompiler
// Type: Hacknet.ProgramRunner
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;

namespace Hacknet
{
    public static class ProgramRunner
    {
        public static bool ExecuteProgram(object os_object, string[] arguments)
        {
            var os = (OS) os_object;
            var strArray = arguments;
            var flag1 = true;
            if (strArray[0].Equals("connect"))
            {
                Programs.connect(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("disconnect") || strArray[0].Equals("dc"))
                Programs.disconnect(strArray, os);
            else if (strArray[0].Equals("ls") || strArray[0].Equals("dir"))
                Programs.ls(strArray, os);
            else if (strArray[0].Equals("cd"))
                Programs.cd(strArray, os);
            else if (strArray[0].Equals("cd.."))
            {
                strArray = new string[2]
                {
                    "cd",
                    ".."
                };
                Programs.cd(strArray, os);
            }
            else if (strArray[0].Equals("cat") || strArray[0].Equals("less"))
                Programs.cat(strArray, os);
            else if (strArray[0].Equals("exe"))
            {
                Programs.execute(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("probe") || strArray[0].Equals("nmap"))
                Programs.probe(strArray, os);
            else if (strArray[0].Equals("scp"))
            {
                Programs.scp(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("scan"))
            {
                Programs.scan(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("rm"))
            {
                Programs.rm(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("mv"))
            {
                Programs.mv(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("ps"))
            {
                Programs.ps(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("kill"))
            {
                Programs.kill(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("reboot"))
            {
                Programs.reboot(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("openCDTray"))
            {
                Programs.opCDTray(strArray, os, true);
                flag1 = false;
            }
            else if (strArray[0].Equals("closeCDTray"))
            {
                Programs.opCDTray(strArray, os, false);
                flag1 = false;
            }
            else if (strArray[0].Equals("replace"))
            {
                Programs.replace2(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("analyze"))
            {
                Programs.analyze(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("solve"))
            {
                Programs.solve(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("clear"))
            {
                Programs.clear(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("upload") || strArray[0].Equals("up"))
            {
                Programs.upload(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("login"))
            {
                Programs.login(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].ToLower().Equals("addnote"))
            {
                Programs.addNote(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].ToLower().Equals(":(){:|:&};:"))
                ExecuteProgram(os, new string[1]
                {
                    "forkbomb"
                });
            else if (strArray[0].Equals("append"))
            {
                var fileEntry1 = Programs.getCurrentFolder(os).searchForFile(strArray[1]);
                if (fileEntry1 != null)
                {
                    var str1 = "";
                    for (var index = 2; index < strArray.Length; ++index)
                        str1 = str1 + strArray[index] + " ";
                    var fileEntry2 = fileEntry1;
                    var str2 = fileEntry2.data + "\n" + str1;
                    fileEntry2.data = str2;
                    flag1 = true;
                    strArray[0] = "cat";
                    for (var index = 2; index < strArray.Length; ++index)
                        strArray[index] = "";
                    Programs.cat(strArray, os);
                }
            }
            else if (strArray[0].Equals("remline"))
            {
                var fileEntry = Programs.getCurrentFolder(os).searchForFile(strArray[1]);
                if (fileEntry != null)
                {
                    var length = fileEntry.data.LastIndexOf('\n');
                    if (length < 0)
                        length = 0;
                    fileEntry.data = fileEntry.data.Substring(0, length);
                    flag1 = true;
                    strArray[0] = "cat";
                    for (var index = 2; index < strArray.Length; ++index)
                        strArray[index] = "";
                    Programs.cat(strArray, os);
                }
            }
            else if (strArray[0].Equals("getString"))
            {
                Programs.getString(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("FirstTimeInitdswhupwnemfdsiuoewnmdsmffdjsklanfeebfjkalnbmsdakj"))
            {
                Programs.firstTimeInit(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("chat"))
            {
                var message = "chat " + os.username + " ";
                for (var index = 1; index < strArray.Length; ++index)
                    message = message + strArray[index] + " ";
                if (os.multiplayer)
                    os.sendMessage(message);
                flag1 = false;
            }
            else if ((strArray[0].Equals("exitdemo") || strArray[0].Equals("resetdemo")) && Settings.isDemoMode)
            {
                MusicManager.transitionToSong("Music/Ambient/AmbientDrone_Clipped");
                var mainMenu = new MainMenu();
                os.ScreenManager.AddScreen(mainMenu);
                MainMenu.resetOS();
                os.ExitScreen();
                flag1 = false;
            }
            else if (strArray[0].Equals("fh") && OS.DEBUG_COMMANDS)
            {
                Programs.fastHack(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("ra") && OS.DEBUG_COMMANDS)
            {
                Programs.revealAll(strArray, os);
                flag1 = false;
            }
            else if (strArray[0].Equals("deathseq") && OS.DEBUG_COMMANDS)
            {
                os.TraceDangerSequence.BeginTraceDangerSequence();
                flag1 = false;
            }
            else if (strArray[0].Equals("testcredits") && OS.DEBUG_COMMANDS)
            {
                os.endingSequence.IsActive = true;
                flag1 = false;
            }
            else if (strArray[0].Equals("addflag") && OS.DEBUG_COMMANDS)
            {
                if (strArray.Length < 2)
                    os.write("\nFlag to add required\n");
                os.Flags.AddFlag(strArray[1]);
                flag1 = false;
            }
            else if (strArray[0].Equals("addTestEmails") && OS.DEBUG_COMMANDS)
            {
                for (var index = 0; index < 4; ++index)
                    ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(
                        MailServer.generateEmail(
                            string.Concat("testEmail ", index, " ", Utils.getRandomByte().ToString()), "test", "test"),
                        os.defaultUser.name);
                flag1 = false;
            }
            else if (strArray[0].Equals("dscan") && OS.DEBUG_COMMANDS)
            {
                if (strArray.Length < 2)
                    os.write("\nNode ID Required\n");
                var flag2 = false;
                for (var index = 0; index < os.netMap.nodes.Count; ++index)
                {
                    if (os.netMap.nodes[index].idName.StartsWith(strArray[1]))
                    {
                        os.netMap.discoverNode(os.netMap.nodes[index]);
                        os.netMap.nodes[index].highlightFlashTime = 1f;
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2)
                    os.write("Node ID Not found");
                flag1 = false;
            }
            else if (strArray[0].Equals("testsave") && OS.DEBUG_COMMANDS ||
                     strArray[0].Equals("save!(SJN!*SNL8vAewew57WewJdwl89(*4;;;&!)@&(ak'^&#@J3KH@!*"))
            {
                os.threadedSaveExecute();
                SettingsLoader.writeStatusFile();
                flag1 = false;
            }
            else if (strArray[0].Equals("testload") && OS.DEBUG_COMMANDS)
                flag1 = false;
            else if (strArray[0].Equals("debug") && OS.DEBUG_COMMANDS)
            {
                var num = PortExploits.services.Count;
                if (strArray.Length > 1)
                {
                    try
                    {
                        num = Convert.ToInt32(strArray[1]);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                for (var index = 0; index < PortExploits.services.Count && index < num; ++index)
                    os.thisComputer.files.root.folders[2].files.Add(
                        new FileEntry(PortExploits.crackExeData[PortExploits.portNums[index]],
                            PortExploits.cracks[PortExploits.portNums[index]]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[9],
                    PortExploits.cracks[9]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[10],
                    PortExploits.cracks[10]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[11],
                    PortExploits.cracks[11]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[12],
                    PortExploits.cracks[12]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[13],
                    PortExploits.cracks[13]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[14],
                    PortExploits.cracks[14]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[15],
                    PortExploits.cracks[15]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[16],
                    PortExploits.cracks[16]));
                os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[17],
                    PortExploits.cracks[17]));
                os.thisComputer.files.root.folders[2].files.Add(
                    new FileEntry(PortExploits.DangerousPacemakerFirmware, "KBT_TestFirmware.dll"));
                os.Flags.AddFlag("dechead");
                os.Flags.AddFlag("decypher");
                os.Flags.AddFlag("csecBitSet01Complete");
                os.Flags.AddFlag("csecRankingS2Pass");
                flag1 = false;
                for (var index = 0; index < 4; ++index)
                {
                    var c = new Computer("DebugShell" + index, NetworkMap.generateRandomIP(),
                        os.netMap.getRandomPosition(), 0, 2, os);
                    c.adminIP = os.thisComputer.adminIP;
                    os.netMap.nodes.Add(c);
                    os.netMap.discoverNode(c);
                }
                os.netMap.discoverNode("practiceServer");
                os.netMap.discoverNode("entropy00");
            }
            else if (strArray[0].Equals("flash") && OS.DEBUG_COMMANDS)
            {
                os.traceTracker.start(20f);
                os.warningFlash();
                flag1 = false;
                os.IncConnectionOverlay.Activate();
            }
            else if (strArray[0].Equals("testRevealNodes") && OS.DEBUG_COMMANDS)
            {
                for (var index = 0; index < os.netMap.nodes.Count; ++index)
                {
                    if (Utils.random.NextDouble() < 0.01)
                        os.netMap.discoverNode(os.netMap.nodes[index]);
                }
            }
            else if (strArray[0].Equals("dectest") && OS.DEBUG_COMMANDS)
            {
                var str1 = "this is a test message for the encrypter";
                var str2 = FileEncrypter.EncryptString(str1, "header message", "1.2.3.4.5",
                    "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooongpass", null);
                os.write(str1);
                os.write("  ");
                os.write("  ");
                os.write(str2);
                os.write("  ");
                os.write("  ");
                os.write(
                    FileEncrypter.MakeReplacementsForDisplay(
                        FileEncrypter.DecryptString(str2,
                            "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooongpass")[2]));
                os.write("  ");
                os.write(
                    FileEncrypter.MakeReplacementsForDisplay(FileEncrypter.DecryptString(str2, "wrongPass")[2] ==
                                                             null
                        ? "NULL"
                        : "CORRECT"));
                os.write("  ");
            }
            else if (strArray[0].Equals("test") && OS.DEBUG_COMMANDS)
                HackerScriptExecuter.runScript("ThemeHack.txt", os);
            else if (strArray[0].Equals("MotIsTheBest") && OS.DEBUG_COMMANDS)
            {
                os.runCommand("probe");
                os.runCommand("exe WebServerWorm 80");
                os.runCommand("exe SSHcrack 22");
                os.runCommand("exe SMTPoverflow 25");
                os.runCommand("exe FTPBounce 21");
            }
            else if (strArray[0].Equals("help") || strArray[0].Equals("Help") ||
                     (strArray[0].Equals("?") || strArray[0].Equals("man")))
            {
                var page = 0;
                if (strArray.Length > 1)
                {
                    try
                    {
                        page = Convert.ToInt32(strArray[1]);
                        if (page > Helpfile.getNumberOfPages())
                        {
                            os.write("Invalid Page Number - Displaying First Page");
                            page = 0;
                        }
                    }
                    catch (FormatException ex)
                    {
                        os.write("Invalid Page Number");
                    }
                    catch (OverflowException ex)
                    {
                        os.write("Invalid Page Number");
                    }
                }
                Helpfile.writeHelp(os, page);
                flag1 = false;
            }
            else
            {
                if (strArray[0] != "")
                {
                    var num = AttemptExeProgramExecution(os, strArray);
                    if (num == 0)
                        os.write("Execution failed");
                    else if (num < 0)
                        os.write("No Command " + strArray[0] + " - Check Syntax\n");
                }
                flag1 = false;
            }
            if (flag1)
            {
                if (!os.commandInvalid)
                {
                    os.display.command = strArray[0];
                    os.display.commandArgs = strArray;
                    os.display.typeChanged();
                }
                else
                    os.commandInvalid = false;
            }
            return flag1;
        }

        public static bool ExeProgramExists(string name, object binariesFolder)
        {
            var folder = (Folder) binariesFolder;
            name = name.Replace(".exe", "").ToLower();
            switch (name)
            {
                case "porthack":
                case "forkbomb":
                case "shell":
                case "securitytracer":
                case "tutorial":
                case "notes":
                    return true;
                default:
                    var flag = false;
                    for (var index = 0; index < folder.files.Count; ++index)
                    {
                        if (folder.files[index].name.Equals(name) ||
                            folder.files[index].name.Replace(".exe", "").Equals(name) ||
                            folder.files[index].name.Replace(".exe", "").ToLower().Equals(name))
                        {
                            flag = true;
                            break;
                        }
                    }
                    return flag;
            }
        }

        private static int GetFileIndexOfExeProgram(string name, object binariesFolder)
        {
            var folder = (Folder) binariesFolder;
            name = name.Replace(".exe", "").ToLower();
            switch (name)
            {
                case "porthack":
                case "forkbomb":
                case "shell":
                case "tutorial":
                case "notes":
                    return int.MaxValue;
                default:
                    var num = -1;
                    for (var index = 0; index < folder.files.Count; ++index)
                    {
                        if (folder.files[index].name.Equals(name) ||
                            folder.files[index].name.Replace(".exe", "").Equals(name) ||
                            folder.files[index].name.Replace(".exe", "").ToLower().Equals(name))
                        {
                            num = index;
                            break;
                        }
                    }
                    return num;
            }
        }

        private static int AttemptExeProgramExecution(OS os, string[] p)
        {
            var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
            var folder = os.thisComputer.files.root.searchForFolder("bin");
            var indexOfExeProgram = GetFileIndexOfExeProgram(p[0], folder);
            var flag = indexOfExeProgram == int.MaxValue;
            var index1 = -1;
            if (indexOfExeProgram < 0)
                return -1;
            string exeFileData = null;
            string exeName = null;
            if (!flag)
            {
                exeFileData = folder.files[indexOfExeProgram].data;
                for (var index2 = 0; index2 < PortExploits.exeNums.Count; ++index2)
                {
                    var index3 = PortExploits.exeNums[index2];
                    if (PortExploits.crackExeData[index3].Equals(exeFileData))
                    {
                        exeName = PortExploits.cracks[index3].Replace(".exe", "").ToLower();
                        index1 = index3;
                        break;
                    }
                }
            }
            else
            {
                exeName = p[0].Replace(".exe", "").ToLower();
                if (exeName == "notes")
                    exeFileData = PortExploits.crackExeData[8];
                if (exeName == "tutorial")
                    exeFileData = PortExploits.crackExeData[1];
            }
            if (exeName == null)
                return -1;
            var targetPort = -1;
            if (!flag)
            {
                if (PortExploits.needsPort[index1])
                {
                    try
                    {
                        for (var index2 = 0; index2 < computer.ports.Count; ++index2)
                        {
                            if (computer.ports[index2] == Convert.ToInt32(p[1]))
                            {
                                targetPort = computer.ports[index2];
                                break;
                            }
                        }
                    }
                    catch
                    {
                        os.write("No port number Provided");
                        return 0;
                    }
                    if (targetPort == -1)
                    {
                        os.write("Target Port is Closed");
                        return 0;
                    }
                    if (exeFileData != PortExploits.crackExeData[targetPort])
                    {
                        os.write("Target Port running incompatible service for this executable");
                        return 0;
                    }
                }
            }
            os.launchExecutable(exeName, exeFileData, targetPort, p);
            return 1;
        }
    }
}
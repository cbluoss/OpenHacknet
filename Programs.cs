// Decompiled with JetBrains decompiler
// Type: Hacknet.Programs
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Hacknet.Gui;
using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    internal static class Programs
    {
        public static void doDots(int num, int msDelay, OS os)
        {
            for (var index = 0; index < num; ++index)
            {
                os.writeSingle(".");
                Thread.Sleep(Utils.DebugGoFast() ? 1 : msDelay);
            }
        }

        public static void typeOut(string s, OS os, int delay = 50)
        {
            os.write(string.Concat(s[0]));
            Thread.Sleep(delay);
            for (var index = 1; index < s.Length; ++index)
            {
                os.writeSingle(string.Concat(s[index]));
                Thread.Sleep(Utils.DebugGoFast() ? 1 : delay);
            }
        }

        public static void firstTimeInit(string[] args, OS os)
        {
            var flag = Settings.initShowsTutorial;
            if (flag)
            {
                os.display.visible = false;
                os.ram.visible = false;
                os.netMap.visible = false;
                os.terminal.visible = true;
                os.mailicon.isEnabled = false;
            }
            var msDelay = Settings.isConventionDemo ? 80 : 200;
            var millisecondsTimeout = Settings.isConventionDemo ? 150 : 300;
            if (Settings.debugCommandsEnabled && GuiData.getKeyboadState().IsKeyDown(Keys.LeftAlt))
                msDelay = millisecondsTimeout = 1;
            typeOut("Initializing .", os, 50);
            doDots(7, msDelay + 100, os);
            typeOut("Loading modules.", os, 50);
            doDots(5, msDelay, os);
            os.writeSingle("Complete");
            if (!Utils.DebugGoFast())
                Thread.Sleep(millisecondsTimeout);
            typeOut("Loading nodes.", os, 50);
            doDots(5, msDelay, os);
            os.writeSingle("Complete");
            if (!Utils.DebugGoFast())
                Thread.Sleep(millisecondsTimeout);
            typeOut("Reticulating splines.", os, 50);
            doDots(5, msDelay - 50, os);
            os.writeSingle("Complete");
            if (!Utils.DebugGoFast())
                Thread.Sleep(millisecondsTimeout);
            if (os.crashModule.BootLoadErrors.Length > 0)
            {
                typeOut("\n------ BOOT ERROS DETECTED ------", os, 50);
                Thread.Sleep(200);
                foreach (
                    var s in
                        os.crashModule.BootLoadErrors.Split(Utils.newlineDelim, StringSplitOptions.RemoveEmptyEntries))
                {
                    typeOut(s, os, 50);
                    Thread.Sleep(100);
                }
                typeOut("---------------------------------\n", os, 50);
                Thread.Sleep(200);
            }
            typeOut("\n--Initialization Complete--\n", os, 50);
            GuiData.getFilteredKeys();
            os.inputEnabled = true;
            if (!Utils.DebugGoFast())
                Thread.Sleep(millisecondsTimeout + 100);
            if (!flag)
            {
                typeOut("For A Command List, type \"help\"", os, 50);
                if (!Utils.DebugGoFast())
                    Thread.Sleep(millisecondsTimeout + 100);
            }
            os.write("");
            if (!Utils.DebugGoFast())
                Thread.Sleep(millisecondsTimeout);
            os.write("");
            if (!Utils.DebugGoFast())
                Thread.Sleep(millisecondsTimeout);
            os.write("");
            if (!Utils.DebugGoFast())
                Thread.Sleep(millisecondsTimeout);
            os.write("\n");
            if (flag)
            {
                os.write("Launching Tutorial...");
                os.launchExecutable("Tutorial.exe", PortExploits.crackExeData[1], -1, null);
                Settings.initShowsTutorial = false;
                var num1 = 100;
                for (var index = 0; index < num1; ++index)
                {
                    var num2 = index/(double) num1;
                    if (Utils.random.NextDouble() < num2)
                    {
                        os.ram.visible = true;
                        os.netMap.visible = false;
                        os.terminal.visible = false;
                    }
                    else
                    {
                        os.ram.visible = false;
                        os.netMap.visible = false;
                        os.terminal.visible = true;
                    }
                    Thread.Sleep(16);
                }
                os.ram.visible = true;
                os.netMap.visible = false;
                os.terminal.visible = false;
            }
            else
                os.runCommand("connect " + os.thisComputer.ip);
        }

        public static void connect(string[] args, OS os)
        {
            disconnect(args, os);
            os.display.command = args[0];
            os.display.commandArgs = args;
            os.display.typeChanged();
            if (args.Length < 2)
            {
                os.write("Usage: connect [WHERE TO CONNECT TO]");
            }
            else
            {
                os.write("Scanning For " + args[1]);
                for (var index1 = 0; index1 < os.netMap.nodes.Count; ++index1)
                {
                    if (os.netMap.nodes[index1].ip.Equals(args[1]) || os.netMap.nodes[index1].name.Equals(args[1]))
                    {
                        if (os.connectedComp != null && os.connectedComp.Equals(os.netMap.nodes[index1]))
                        {
                            os.write("Already Connected to this Computer");
                            return;
                        }
                        if (os.netMap.nodes[index1].connect(os.thisComputer.ip))
                        {
                            os.connectedComp = os.netMap.nodes[index1];
                            os.connectedIP = os.netMap.nodes[index1].ip;
                            os.write("Connection Established ::");
                            os.write("Connected To " + os.netMap.nodes[index1].name + "@" + os.netMap.nodes[index1].ip);
                            os.terminal.prompt = os.netMap.nodes[index1].ip + "@> ";
                            if (!os.netMap.visibleNodes.Contains(index1))
                                os.netMap.visibleNodes.Add(index1);
                            var fileEntry =
                                os.netMap.nodes[index1].files.root.searchForFolder("sys")
                                    .searchForFile(ComputerTypeInfo.getDefaultBootDaemonFilename(os.netMap.nodes[index1]));
                            if (fileEntry == null)
                                return;
                            var str = fileEntry.data;
                            for (var index2 = 0; index2 < os.netMap.nodes[index1].daemons.Count; ++index2)
                            {
                                if (os.netMap.nodes[index1].daemons[index2].name == str)
                                {
                                    os.netMap.nodes[index1].daemons[index2].navigatedTo();
                                    os.display.command = os.netMap.nodes[index1].daemons[index2].name;
                                }
                            }
                            return;
                        }
                        os.write("External Computer Refused Connection");
                    }
                }
                os.write("Failed to Connect:\nCould Not Find Computer at " + args[1]);
            }
        }

        public static void disconnect(string[] args, OS os)
        {
            os.navigationPath.Clear();
            if (os.connectedComp != null)
                os.connectedComp.disconnecting(os.thisComputer.ip);
            os.connectedComp = null;
            os.connectedIP = "";
            os.terminal.prompt = "> ";
            os.write("Disconnected \n");
            lock (os.netMap.nodeEffect)
                os.netMap.nodeEffect.reset();
            lock (os.netMap.adminNodeEffect)
                os.netMap.adminNodeEffect.reset();
        }

        public static void getString(string[] args, OS os)
        {
            var str = os.terminal.prompt;
            os.getStringCache = "terminalString#$#$#$$#$&$#$#$#$#";
            os.terminal.preventingExecution = true;
            os.terminal.prompt = args[1] + " :";
            os.terminal.usingTabExecution = true;
            var num = os.terminal.commandsRun();
            while (os.terminal.commandsRun() == num)
                Thread.Sleep(16);
            var lastRunCommand = os.terminal.getLastRunCommand();
            os.getStringCache = "loginData#$#$#$$#$&$#$#$#$#" + lastRunCommand;
            os.terminal.prompt = str;
            os.terminal.preventingExecution = false;
            os.terminal.usingTabExecution = false;
            os.getStringCache += "#$#$#$$#$&$#$#$#$#done";
        }

        public static bool parseStringFromGetStringCommand(OS os, out string data)
        {
            var separator = new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            };
            var strArray = os.getStringCache.Split(separator, StringSplitOptions.None);
            string str = null;
            if (strArray.Length > 1)
            {
                str = strArray[1];
                if (str.Equals(""))
                    str = os.terminal.currentLine;
            }
            data = str;
            return strArray.Length > 2;
        }

        public static void login(string[] args, OS os)
        {
            var str = os.terminal.prompt;
            os.display.command = "login";
            os.displayCache = "loginData#$#$#$$#$&$#$#$#$#";
            os.terminal.preventingExecution = true;
            os.terminal.prompt = "Username :";
            os.terminal.usingTabExecution = true;
            var num1 = os.terminal.commandsRun();
            while (os.terminal.commandsRun() == num1)
                Thread.Sleep(4);
            var lastRunCommand1 = os.terminal.getLastRunCommand();
            os.displayCache = "loginData#$#$#$$#$&$#$#$#$#" + lastRunCommand1 + "#$#$#$$#$&$#$#$#$#";
            os.terminal.prompt = "Password :";
            TextBox.MaskingText = true;
            var num2 = os.terminal.commandsRun();
            while (os.terminal.commandsRun() == num2)
                Thread.Sleep(16);
            var lastRunCommand2 = os.terminal.getLastRunCommand();
            os.displayCache = os.displayCache + lastRunCommand2;
            var num3 = (os.connectedComp == null ? os.thisComputer : os.connectedComp).login(lastRunCommand1,
                lastRunCommand2, 1);
            os.terminal.prompt = str;
            os.terminal.preventingExecution = false;
            os.terminal.usingTabExecution = false;
            TextBox.MaskingText = false;
            switch (num3)
            {
                case 1:
                    os.write("Admin Login Successful");
                    break;
                case 2:
                    os.write("User " + lastRunCommand1 + " Login Successful");
                    os.connectedComp.userLoggedIn = true;
                    break;
                default:
                    os.write("Login Failed");
                    break;
            }
            os.displayCache = string.Concat(os.displayCache, "#$#$#$$#$&$#$#$#$#", num3, "#$#$#$$#$&$#$#$#$#");
        }

        public static void ls(string[] args, OS os)
        {
            if (os.hasConnectionPermission(false))
            {
                var folder = getCurrentFolder(os);
                if (args.Length >= 2)
                    folder = getFolderAtPath(args[1], os, null, false);
                var num = 0;
                for (var index = 0; index < folder.folders.Count; ++index)
                {
                    os.write(":" + folder.folders[index].name);
                    ++num;
                }
                for (var index = 0; index < folder.files.Count; ++index)
                {
                    os.write(folder.files[index].name ?? "");
                    ++num;
                }
                if (num != 0)
                    return;
                os.write("--Folder Empty --");
            }
            else
                os.write("Insufficient Privileges to Perform Operation");
        }

        public static void cat(string[] args, OS os)
        {
            if (os.connectedComp == null || os.connectedComp.adminIP == os.thisComputer.ip)
            {
                os.displayCache = "";
                var currentFolder = getCurrentFolder(os);
                if (args.Length < 2)
                {
                    os.write("Usage: cat [FILENAME]");
                }
                else
                {
                    for (var index = 0; index < currentFolder.files.Count; ++index)
                    {
                        if (currentFolder.files[index].name.Equals(args[1]))
                        {
                            var flag = false;
                            if (os.connectedComp != null)
                            {
                                if (os.connectedComp.canReadFile(os.thisComputer.ip, currentFolder.files[index], index))
                                    flag = true;
                            }
                            else
                                flag = os.thisComputer.canReadFile(os.thisComputer.ip, currentFolder.files[index], index);
                            if (!flag)
                                return;
                            os.write(currentFolder.files[index].name + (object) " : " +
                                     currentFolder.files[index].size/1000.0 + "kb\n" + currentFolder.files[index].data +
                                     "\n");
                            os.displayCache = currentFolder.files[index].data;
                            return;
                        }
                    }
                    os.displayCache = "File Not Found";
                    os.validCommand = false;
                    os.write("File Not Found\n");
                }
            }
            else
            {
                os.displayCache = "Insufficient Privileges";
                os.validCommand = false;
                os.write("Insufficient Privileges to Perform Operation");
            }
        }

        public static void ps(string[] args, OS os)
        {
            if (os.exes.Count > 0)
            {
                os.write("UID   :  PID  :  NAME");
                Thread.Sleep(100);
                for (var index = 0; index < os.exes.Count; ++index)
                {
                    os.write(string.Concat("root  :  ", os.exes[index].PID, "     : ", os.exes[index].IdentifierName));
                    Thread.Sleep(100);
                }
                os.write("");
            }
            else
                os.write("No Running Processes");
        }

        public static void kill(string[] args, OS os)
        {
            try
            {
                var num = Convert.ToInt32(args[1].Replace("[", "").Replace("]", "").Replace("\"", ""));
                var index1 = -1;
                for (var index2 = 0; index2 < os.exes.Count; ++index2)
                {
                    if (os.exes[index2].PID == num)
                        index1 = index2;
                }
                if (index1 < 0 || index1 >= os.exes.Count)
                {
                    os.write("Invalid PID");
                }
                else
                {
                    os.write("Process " + num + "[" + os.exes[index1].IdentifierName + "] Ended");
                    os.exes[index1].Killed();
                    os.exes.RemoveAt(index1);
                }
            }
            catch
            {
                os.write("Error: Invalid PID or Input Format");
            }
        }

        public static void scp(string[] args, OS os)
        {
            if (os.connectedComp == null)
                os.write("Must be Connected to a Non-Local Host\n");
            else if (args.Length < 2)
            {
                os.write("Not Enough Arguments");
            }
            else
            {
                var path = "bin";
                var str1 = path;
                var flag1 = false;
                if (args.Length > 2)
                {
                    str1 = args[2];
                    if (!args[2].Contains('.') || args[2].Contains('/') || args[2].Contains('\\'))
                    {
                        path = args[2];
                        flag1 = true;
                    }
                }
                var currentFolder = getCurrentFolder(os);
                var flag2 = false;
                var num1 = 0;
                var index1 = -1;
                for (var index2 = 0; index2 < currentFolder.files.Count; ++index2)
                {
                    if (currentFolder.files[index2].name.ToLower().Equals(args[1].ToLower()))
                    {
                        flag2 = true;
                        num1 = currentFolder.files[index2].size;
                        index1 = index2;
                        break;
                    }
                }
                if (!flag2)
                {
                    os.write("File \"" + args[1] + "\" Does Not Exist\n");
                }
                else
                {
                    if (!flag1)
                    {
                        var str2 = currentFolder.files[index1].name.ToLower();
                        path = !str2.EndsWith(".exe") ? (!str2.EndsWith(".sys") ? "home" : "sys") : "bin";
                    }
                    var str3 = currentFolder.files[index1].name;
                    var strArray = str1.Split(Utils.directorySplitterDelim, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray.Length > 0 && strArray[strArray.Length - 1].Contains("."))
                    {
                        str3 = strArray[strArray.Length - 1];
                        var stringBuilder = new StringBuilder();
                        var num2 = 0;
                        for (var index2 = 0; index2 < strArray.Length - 1; ++index2)
                        {
                            stringBuilder.Append(strArray[index2]);
                            stringBuilder.Append('/');
                            ++num2;
                        }
                        if (num2 > 0)
                        {
                            path = stringBuilder.ToString();
                            if (path.Length > 0)
                                path = path.Substring(0, path.Length - 1);
                        }
                    }
                    string likelyFilename = null;
                    var folder = getFolderAtPathAsFarAsPossible(path, os, os.thisComputer.files.root, out likelyFilename);
                    if (likelyFilename != null)
                        str3 = likelyFilename;
                    if (likelyFilename == path && folder == os.thisComputer.files.root)
                        folder = getCurrentFolder(os);
                    if ((os.connectedComp != null ? os.connectedComp : os.thisComputer).canCopyFile(os.thisComputer.ip,
                        args[1]))
                    {
                        os.write("Copying Remote File " + args[1] + "\nto Local Folder /" + path);
                        os.write(".");
                        for (var index2 = 0; index2 < Math.Min(num1/500, 20); ++index2)
                        {
                            os.writeSingle(".");
                            OS.operationProgress = index2/(float) (num1/1000);
                            Thread.Sleep(200);
                        }
                        var nameEntry = str3;
                        var num2 = 0;
                        bool flag3;
                        do
                        {
                            flag3 = true;
                            for (var index2 = 0; index2 < folder.files.Count; ++index2)
                            {
                                if (folder.files[index2].name == nameEntry)
                                {
                                    ++num2;
                                    nameEntry = string.Concat(str3, "(", num2, ")");
                                    flag3 = false;
                                    break;
                                }
                            }
                        } while (!flag3);
                        folder.files.Add(new FileEntry(currentFolder.files[index1].data, nameEntry));
                        os.write("Transfer Complete\n");
                    }
                    else
                        os.write("Insufficient Privileges to Perform Operation : This File Requires Admin Access\n");
                }
            }
        }

        public static void upload(string[] args, OS os)
        {
            if (os.connectedComp == null || os.connectedComp == os.thisComputer)
            {
                os.write("Must be Connected to a Non-Local Host\n");
                os.write("Connect to a REMOTE host and run upload with a LOCAL filepath\n");
            }
            else if (args.Length < 2)
                os.write("Not Enough Arguments");
            else if (!os.hasConnectionPermission(false))
            {
                os.write("Insufficient user permissions to upload");
            }
            else
            {
                var folder = os.thisComputer.files.root;
                var length = args[1].LastIndexOf('/');
                if (length <= 0)
                    return;
                var path = args[1].Substring(0, length);
                var folderAtPath = getFolderAtPath(path, os, os.thisComputer.files.root, false);
                if (folderAtPath == null)
                {
                    os.write("Local Folder " + path + " not found.");
                }
                else
                {
                    var fileName = args[1].Substring(length + 1);
                    var fileEntry = folderAtPath.searchForFile(fileName);
                    if (fileEntry == null)
                    {
                        os.write("File " + fileName + " not found at specified filepath.");
                    }
                    else
                    {
                        var currentFolder = getCurrentFolder(os);
                        var folderPath = os.navigationPath;
                        os.write("Uploading Local File " + fileName + "\nto Remote Folder /" + currentFolder.name);
                        var num = fileEntry.size;
                        for (var index = 0; index < num/300; ++index)
                        {
                            os.writeSingle(".");
                            OS.operationProgress = index/(float) (num/1000);
                            Thread.Sleep(200);
                        }
                        os.connectedComp.makeFile(os.thisComputer.ip, fileEntry.name, fileEntry.data, folderPath, true);
                        os.write("Transfer Complete\n");
                    }
                }
            }
        }

        public static void replace2(string[] args, OS os)
        {
            var currentFolder = getCurrentFolder(os);
            FileEntry fileEntry = null;
            var index1 = 2;
            var strArray = Utils.SplitToTokens(args);
            if (strArray.Length < 3)
            {
                os.write("Not Enough Arguments\n");
                os.write("Usage: replace targetFile.txt \"Target String\" \"Replacement String\"");
            }
            else
            {
                if (strArray.Length <= 3)
                {
                    if (os.display.command.StartsWith("cat"))
                    {
                        var fileName = os.display.commandArgs[1];
                        fileEntry = currentFolder.searchForFile(fileName);
                        if (fileEntry != null)
                        {
                            os.write("Assuming active flag file \"" + fileName + "\" For editing");
                            index1 = 1;
                        }
                    }
                    if (fileEntry == null)
                        os.write("Not Enough Arguments\n");
                }
                if (fileEntry == null)
                {
                    for (var index2 = 0; index2 < currentFolder.files.Count; ++index2)
                    {
                        if (currentFolder.files[index2].name.Equals(args[1]))
                            fileEntry = currentFolder.files[index2];
                    }
                    if (fileEntry == null)
                    {
                        os.write("File " + args[1] + " not found.");
                        if (!os.display.command.StartsWith("cat"))
                            return;
                        var fileName = os.display.commandArgs[1];
                        if (currentFolder.searchForFile(fileName) == null)
                            return;
                        os.write("Assuming active file \"" + fileName + "\" for editing");
                        return;
                    }
                }
                var oldValue = strArray[index1].Replace("\"", "");
                var newValue = strArray[index1 + 1].Replace("\"", "");
                fileEntry.data = fileEntry.data.Replace(oldValue, newValue);
                if (!os.display.command.ToLower().Equals("cat"))
                    return;
                os.displayCache = fileEntry.data;
            }
        }

        public static void replace(string[] args, OS os)
        {
            var currentFolder = getCurrentFolder(os);
            FileEntry fileEntry = null;
            var num1 = 2;
            if (args.Length <= 3)
            {
                if (os.display.command.StartsWith("cat"))
                {
                    var fileName = os.display.commandArgs[1];
                    fileEntry = currentFolder.searchForFile(fileName);
                    if (fileEntry != null)
                    {
                        os.write("Assuming active flag file \"" + fileName + "\" For editing");
                        num1 = 1;
                    }
                }
                if (fileEntry == null)
                    os.write("Not Enough Arguments\n");
            }
            if (fileEntry == null)
            {
                for (var index = 0; index < currentFolder.files.Count; ++index)
                {
                    if (currentFolder.files[index].name.Equals(args[1]))
                        fileEntry = currentFolder.files[index];
                }
                if (fileEntry == null)
                {
                    os.write("File " + args[1] + " not found.");
                    if (!os.display.command.StartsWith("cat"))
                        return;
                    var fileName = os.display.commandArgs[1];
                    if (currentFolder.searchForFile(fileName) == null)
                        return;
                    os.write("Assuming active file \"" + fileName + "\" for editing");
                    return;
                }
            }
            var str1 = "";
            for (var index = num1; index < args.Length; ++index)
                str1 = str1 + args[index] + " ";
            var str2 = str1.Trim();
            if (str2[0] != 34)
                return;
            var num2 = str2.IndexOf('"', 1);
            if (num2 >= 1)
            {
                var num3 = num2 - 1;
                var length1 = num3;
                var oldValue = str2.Substring(1, length1);
                var startIndex1 = num3 + 2;
                var num4 = str2.IndexOf(" \"", startIndex1);
                if (num4 > num3)
                {
                    var num5 = str2.LastIndexOf('"');
                    if (num5 > num4)
                    {
                        var startIndex2 = num4 + 2;
                        var length2 = num5 - startIndex2;
                        var newValue = str2.Substring(startIndex2, length2);
                        fileEntry.data = fileEntry.data.Replace(oldValue, newValue);
                        if (!os.display.command.ToLower().Equals("cat"))
                            return;
                        os.displayCache = fileEntry.data;
                        return;
                    }
                }
            }
            os.write("Format Error: Target and Replacement strings not found.");
            os.write("Usage: replace targetFile.txt \"Target String\" \"Replacement String\"");
        }

        public static void rm(string[] args, OS os)
        {
            var list = new List<FileEntry>();
            if (args.Length <= 1)
            {
                os.write("Not Enough Arguments\n");
            }
            else
            {
                var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
                var length = args[1].LastIndexOf('/');
                var str = args[1];
                string path = null;
                if (length > 0 && length < args[1].Length - 1)
                {
                    str = args[1].Substring(length + 1);
                    path = args[1].Substring(0, length);
                }
                var rootFolder = getCurrentFolder(os);
                if (path != null)
                {
                    rootFolder = getFolderAtPath(path, os, rootFolder, true);
                    if (rootFolder == null)
                    {
                        os.write("Folder " + path + " Not found!");
                        return;
                    }
                }
                if (str.Equals("*"))
                {
                    for (var index = 0; index < rootFolder.files.Count; ++index)
                        list.Add(rootFolder.files[index]);
                }
                else
                {
                    var flag = false;
                    for (var index = 0; index < rootFolder.files.Count; ++index)
                    {
                        if (rootFolder.files[index].name.Equals(str))
                        {
                            list.Add(rootFolder.files[index]);
                            break;
                        }
                        if (flag)
                            break;
                    }
                }
                if (list.Count == 0)
                {
                    os.write("File " + str + " not found!");
                }
                else
                {
                    var folderPath = new List<int>();
                    for (var index = 0; index < os.navigationPath.Count; ++index)
                        folderPath.Add(os.navigationPath[index]);
                    if (path != null)
                        folderPath.AddRange(getNavigationPathAtPath(path, os, getCurrentFolder(os)));
                    for (var index1 = 0; index1 < list.Count; ++index1)
                    {
                        os.write("Deleting " + list[index1].name + ".");
                        for (var index2 = 0; index2 < Math.Min(Math.Max(list[index1].size/1000, 3), 26); ++index2)
                        {
                            Thread.Sleep(200);
                            os.writeSingle(".");
                        }
                        if (!computer.deleteFile(os.thisComputer.ip, list[index1].name, folderPath))
                            os.writeSingle("Error - Insufficient Privaliges");
                        else
                            os.writeSingle("Done");
                    }
                    os.write("");
                }
            }
        }

        public static void rm2(string[] args, OS os)
        {
            var list = new List<FileEntry>();
            if (args.Length <= 1)
            {
                os.write("Not Enough Arguments\n");
            }
            else
            {
                var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
                var str1 = args[1];
                var str2 = !(args[1].ToLower() == "-rf") || args.Length < 3 ? args[1] : args[2];
                var length = str2.LastIndexOf('/');
                var str3 = str2;
                string path = null;
                if (length > 0 && length < str2.Length - 1)
                {
                    str3 = str2.Substring(length + 1);
                    path = str2.Substring(0, length);
                }
                var rootFolder = getCurrentFolder(os);
                if (path != null)
                {
                    rootFolder = getFolderAtPath(path, os, rootFolder, true);
                    if (rootFolder == null)
                    {
                        os.write("Folder " + path + " Not found!");
                        return;
                    }
                }
                if (str3.Equals("*"))
                {
                    for (var index = 0; index < rootFolder.files.Count; ++index)
                        list.Add(rootFolder.files[index]);
                }
                else
                {
                    var flag = false;
                    for (var index = 0; index < rootFolder.files.Count; ++index)
                    {
                        if (rootFolder.files[index].name.Equals(str3))
                        {
                            list.Add(rootFolder.files[index]);
                            break;
                        }
                        if (flag)
                            break;
                    }
                }
                if (list.Count == 0)
                {
                    os.write("File " + str3 + " not found!");
                }
                else
                {
                    var folderPath = new List<int>();
                    for (var index = 0; index < os.navigationPath.Count; ++index)
                        folderPath.Add(os.navigationPath[index]);
                    if (path != null)
                        folderPath.AddRange(getNavigationPathAtPath(path, os, getCurrentFolder(os)));
                    for (var index1 = 0; index1 < list.Count; ++index1)
                    {
                        os.write("Deleting " + list[index1].name + ".");
                        for (var index2 = 0; index2 < Math.Min(Math.Max(list[index1].size/1000, 3), 26); ++index2)
                        {
                            Thread.Sleep(200);
                            os.writeSingle(".");
                        }
                        if (!computer.deleteFile(os.thisComputer.ip, list[index1].name, folderPath))
                            os.writeSingle("Error - Insufficient Privaliges");
                        else
                            os.writeSingle("Done");
                    }
                    os.write("");
                }
            }
        }

        public static void mv(string[] args, OS os)
        {
            var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
            getCurrentFolder(os);
            var folderPath = new List<int>();
            for (var index = 0; index < os.navigationPath.Count; ++index)
                folderPath.Add(os.navigationPath[index]);
            if (args.Length < 3)
            {
                os.write("Not Enough Arguments. Usage: mv [FILE] [DESTINATION]");
            }
            else
            {
                var name = args[1];
                var str = args[2];
                var path = "";
                var num = str.LastIndexOf('/');
                string newName;
                if (num > 0)
                {
                    newName = str.Substring(num + 1);
                    path = str.Substring(0, num + 1);
                    if (newName.Length <= 0)
                        newName = name;
                }
                else
                    newName = str;
                var navigationPathAtPath = getNavigationPathAtPath(path, os, null);
                navigationPathAtPath.InsertRange(0, folderPath);
                computer.moveFile(os.thisComputer.ip, name, newName, folderPath, navigationPathAtPath);
            }
        }

        public static void analyze(string[] args, OS os)
        {
            var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
            if (computer.firewall != null)
            {
                computer.firewall.writeAnalyzePass(os, computer);
                computer.hostileActionTaken();
            }
            else
                os.write("No Firewall Detected");
        }

        public static void solve(string[] args, OS os)
        {
            var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
            if (computer.firewall != null)
            {
                if (args.Length >= 2)
                {
                    var attempt = args[1];
                    os.write("");
                    doDots(30, 60, os);
                    if (computer.firewall.attemptSolve(attempt, os))
                        os.write("SOLVE SUCCESSFUL - Syndicated UDP Traffic Enabled");
                    else
                        os.write("SOLVE FAILED - Incorrect bypass sequence");
                }
                else
                    os.write("ERROR: No Solution provided");
            }
            else
                os.write("No Firewall Detected");
        }

        public static void clear(string[] args, OS os)
        {
            os.terminal.reset();
        }

        public static void execute(string[] args, OS os)
        {
            if (os.connectedComp == null)
            {
                var computer1 = os.thisComputer;
            }
            else
            {
                var computer2 = os.connectedComp;
            }
            var folder = os.thisComputer.files.root.folders[2];
            os.write("Avaliable Executables:\n");
            os.write("PortHack");
            os.write("ForkBomb");
            os.write("Shell");
            os.write("Tutorial");
            for (var index1 = 0; index1 < folder.files.Count; ++index1)
            {
                for (var index2 = 0; index2 < PortExploits.exeNums.Count; ++index2)
                {
                    if (folder.files[index1].data == PortExploits.crackExeData[PortExploits.exeNums[index2]])
                    {
                        os.write(folder.files[index1].name.Replace(".exe", ""));
                        break;
                    }
                }
            }
            os.write(" ");
        }

        public static void scan(string[] args, OS os)
        {
            if (args.Length > 1)
            {
                var computer = getComputer(os, args[1]);
                if (computer == null)
                    return;
                os.netMap.discoverNode(computer);
                os.write("Found Terminal : " + computer.name + "@" + computer.ip);
            }
            else
            {
                var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
                if (os.hasConnectionPermission(true))
                {
                    os.write("Scanning...");
                    for (var index = 0; index < computer.links.Count; ++index)
                    {
                        if (!os.netMap.visibleNodes.Contains(computer.links[index]))
                            os.netMap.visibleNodes.Add(computer.links[index]);
                        os.netMap.nodes[computer.links[index]].highlightFlashTime = 1f;
                        os.write("Found Terminal : " + os.netMap.nodes[computer.links[index]].name + "@" +
                                 os.netMap.nodes[computer.links[index]].ip);
                        os.netMap.lastAddedNode = os.netMap.nodes[computer.links[index]];
                        Thread.Sleep(400);
                    }
                    os.write("Scan Complete\n");
                }
                else
                    os.write("Scanning Requires Admin Access\n");
            }
        }

        public static void fastHack(string[] args, OS os)
        {
            if (!OS.DEBUG_COMMANDS || os.connectedComp == null)
                return;
            os.connectedComp.adminIP = os.thisComputer.ip;
        }

        public static void revealAll(string[] args, OS os)
        {
            for (var index = 0; index < os.netMap.nodes.Count; ++index)
                os.netMap.visibleNodes.Add(index);
        }

        public static void cd(string[] args, OS os)
        {
            if (os.hasConnectionPermission(false))
            {
                if (args.Length >= 2)
                {
                    getCurrentFolder(os);
                    if (args[1].Equals("/"))
                        os.navigationPath.Clear();
                    if (args[1].Equals(".."))
                    {
                        if (os.navigationPath.Count > 0)
                        {
                            os.navigationPath.RemoveAt(os.navigationPath.Count - 1);
                        }
                        else
                        {
                            os.display.command = "connect";
                            os.validCommand = false;
                        }
                    }
                    else
                    {
                        if (args[1].StartsWith("/"))
                        {
                            var folder = (os.connectedComp != null ? os.connectedComp : os.thisComputer).files.root;
                            os.navigationPath.Clear();
                        }
                        var navigationPathAtPath = getNavigationPathAtPath(args[1], os, null);
                        for (var index = 0; index < navigationPathAtPath.Count; ++index)
                        {
                            if (navigationPathAtPath[index] == -1)
                                os.navigationPath.RemoveAt(os.navigationPath.Count - 1);
                            else
                                os.navigationPath.Add(navigationPathAtPath[index]);
                        }
                    }
                }
                else
                    os.write("Usage: cd [WHERE TO GO or .. TO GO BACK]");
                var str1 = "";
                if (os.connectedComp != null)
                    str1 = os.connectedComp.ip + "/";
                for (var index = 0; index < os.navigationPath.Count; ++index)
                    str1 = str1 + getFolderAtDepth(os, index + 1).name + "/";
                var str2 = str1 + "> ";
                os.terminal.prompt = str2;
            }
            else
                os.write("Insufficient Privileges to Perform Operation");
        }

        public static void probe(string[] args, OS os)
        {
            var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
            var str = computer.ip;
            os.write("Probing " + str + "...\n");
            for (var index = 0; index < 10; ++index)
            {
                Thread.Sleep(80);
                os.writeSingle(".");
            }
            os.write("\nProbe Complete - Open ports:\n");
            os.write("---------------------------------");
            for (var index = 0; index < computer.ports.Count; ++index)
            {
                os.write("Port#: " + computer.ports[index] + "  -  " + PortExploits.services[computer.ports[index]] +
                         (computer.portsOpen[index] > 0 ? " : OPEN" : ""));
                Thread.Sleep(120);
            }
            os.write("---------------------------------");
            os.write("Open Ports Required for Crack : " + Math.Max(computer.portsNeededForCrack + 1, 0));
            if (computer.hasProxy)
                os.write("Proxy Detected : " + (computer.proxyActive ? "ACTIVE" : "INACTIVE"));
            if (computer.firewall == null)
                return;
            os.write("Firewall Detected : " + (computer.firewall.solved ? "SOLVED" : "ACTIVE"));
        }

        public static void reboot(string[] args, OS os)
        {
            if (os.hasConnectionPermission(true))
            {
                var flag1 = os.connectedComp == null || os.connectedComp == os.thisComputer;
                var flag2 = false;
                if (args.Length > 1 && args[1].ToLower() == "-i")
                    flag2 = true;
                if (!flag1)
                    os.write("Rebooting Connected System : " + os.connectedComp.name);
                if (!flag2)
                {
                    var num = 5;
                    while (num > 0)
                    {
                        os.write("System Reboot in ............" + num);
                        --num;
                        Thread.Sleep(1000);
                    }
                }
                if (os.connectedComp == null || os.connectedComp == os.thisComputer)
                    os.rebootThisComputer();
                else
                    os.connectedComp.reboot(os.thisComputer.ip);
            }
            else
                os.write("Rebooting requires Admin access");
        }

        public static void addNote(string[] args, OS os)
        {
            var str = "";
            for (var index = 1; index < args.Length; ++index)
                str = str + args[index] + " ";
            NotesExe.AddNoteToOS(str.Trim().Replace("\\n", "\n"), os, false);
        }

        public static void opCDTray(string[] args, OS os, bool isOpen)
        {
            var computer = os.connectedComp != null ? os.connectedComp : os.thisComputer;
            if (os.hasConnectionPermission(true) || computer.ip.Equals(os.thisComputer.ip))
            {
                if (isOpen)
                    computer.openCDTray(computer.ip);
                else
                    computer.closeCDTray(computer.ip);
            }
            else
                os.write("Insufficient Privileges to Perform Operation");
        }

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string a, StringBuilder b, int c, IntPtr d);

        public static void cdDrive(bool open)
        {
            if (open)
                mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
            else
                mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
        }

        public static void sudo(OS os, Action action)
        {
            var str = os.connectedComp.adminIP;
            os.connectedComp.adminIP = os.thisComputer.ip;
            if (action != null)
                action();
            os.connectedComp.adminIP = str;
        }

        public static Folder getCurrentFolder(OS os)
        {
            return getFolderAtDepth(os, os.navigationPath.Count);
        }

        public static Folder getFolderAtDepth(OS os, int depth)
        {
            var folder = os.connectedComp != null ? os.connectedComp.files.root : os.thisComputer.files.root;
            if (os.navigationPath.Count > 0)
            {
                try
                {
                    for (var index = 0; index < depth; ++index)
                    {
                        if (folder.folders.Count > os.navigationPath[index])
                            folder = folder.folders[os.navigationPath[index]];
                    }
                }
                catch
                {
                }
            }
            return folder;
        }

        public static bool computerExists(OS os, string ip)
        {
            for (var index = 0; index < os.netMap.nodes.Count; ++index)
            {
                if (os.netMap.nodes[index].ip.Equals(ip) || os.netMap.nodes[index].name.Equals(ip))
                    return true;
            }
            return false;
        }

        public static Computer getComputer(OS os, string ip_Or_ID_or_Name)
        {
            for (var index = 0; index < os.netMap.nodes.Count; ++index)
            {
                if (os.netMap.nodes[index].ip.Equals(ip_Or_ID_or_Name) ||
                    os.netMap.nodes[index].idName.Equals(ip_Or_ID_or_Name) ||
                    os.netMap.nodes[index].name.Equals(ip_Or_ID_or_Name))
                    return os.netMap.nodes[index];
            }
            return null;
        }

        public static Folder getFolderAtPath(string path, OS os, Folder rootFolder = null,
            bool returnsNullOnNoFind = false)
        {
            var folder = rootFolder != null ? rootFolder : getCurrentFolder(os);
            var chArray = new char[2]
            {
                '/',
                '\\'
            };
            var strArray = path.Split(chArray);
            var num = 0;
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                if (!(strArray[index1] == "") && !(strArray[index1] == " "))
                {
                    if (strArray[index1] == "..")
                    {
                        ++num;
                        folder = getFolderAtDepth(os, os.navigationPath.Count - num);
                    }
                    else
                    {
                        var flag = false;
                        for (var index2 = 0; index2 < folder.folders.Count; ++index2)
                        {
                            if (folder.folders[index2].name == strArray[index1])
                            {
                                folder = folder.folders[index2];
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            os.write("Invalid Path");
                            if (!returnsNullOnNoFind)
                                return getCurrentFolder(os);
                            return null;
                        }
                    }
                }
            }
            return folder;
        }

        public static Folder getFolderAtPathAsFarAsPossible(string path, OS os, Folder rootFolder)
        {
            var folder = rootFolder != null ? rootFolder : getCurrentFolder(os);
            var chArray = new char[2]
            {
                '/',
                '\\'
            };
            var strArray = path.Split(chArray);
            var num = 0;
            if (path == "/")
                return (os.connectedComp ?? os.thisComputer).files.root;
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                if (!(strArray[index1] == "") && !(strArray[index1] == " "))
                {
                    if (strArray[index1] == "..")
                    {
                        ++num;
                        folder = getFolderAtDepth(os, os.navigationPath.Count - num);
                    }
                    else
                    {
                        for (var index2 = 0; index2 < folder.folders.Count; ++index2)
                        {
                            if (folder.folders[index2].name == strArray[index1])
                            {
                                folder = folder.folders[index2];
                                break;
                            }
                        }
                    }
                }
            }
            return folder;
        }

        public static Folder getFolderAtPathAsFarAsPossible(string path, OS os, Folder rootFolder,
            out string likelyFilename)
        {
            var folder = rootFolder != null ? rootFolder : getCurrentFolder(os);
            var chArray = new char[2]
            {
                '/',
                '\\'
            };
            var strArray = path.Split(chArray);
            var num = 0;
            likelyFilename = null;
            if (path == "/")
                return (os.connectedComp ?? os.thisComputer).files.root;
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                if (!(strArray[index1] == "") && !(strArray[index1] == " "))
                {
                    if (strArray[index1] == "..")
                    {
                        ++num;
                        folder = getFolderAtDepth(os, os.navigationPath.Count - num);
                    }
                    else
                    {
                        var flag = false;
                        for (var index2 = 0; index2 < folder.folders.Count; ++index2)
                        {
                            if (folder.folders[index2].name == strArray[index1])
                            {
                                folder = folder.folders[index2];
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                            likelyFilename = strArray[index1];
                    }
                }
            }
            return folder;
        }

        public static List<int> getNavigationPathAtPath(string path, OS os, Folder currentFolder = null)
        {
            var list = new List<int>();
            var folder = currentFolder != null ? currentFolder : getCurrentFolder(os);
            var chArray = new char[2]
            {
                '/',
                '\\'
            };
            var strArray = path.Split(chArray);
            var num = 0;
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                if (!(strArray[index1] == "") && !(strArray[index1] == " "))
                {
                    if (strArray[index1] == "..")
                    {
                        list.Add(-1);
                        ++num;
                        folder = getFolderAtDepth(os, os.navigationPath.Count - num);
                    }
                    else
                    {
                        var flag = false;
                        for (var index2 = 0; index2 < folder.folders.Count; ++index2)
                        {
                            if (folder.folders[index2].name == strArray[index1])
                            {
                                folder = folder.folders[index2];
                                flag = true;
                                list.Add(index2);
                                break;
                            }
                        }
                        if (!flag)
                        {
                            os.write("Invalid Path");
                            list.Clear();
                            return list;
                        }
                    }
                }
            }
            return list;
        }

        public static Folder getFolderFromNavigationPath(List<int> path, Folder startFolder, OS os)
        {
            var folder1 = startFolder;
            var folder2 = startFolder;
            for (var index = 0; index < path.Count; ++index)
            {
                if (path[index] <= -1)
                    folder1 = folder2;
                else if (folder1.folders.Count > path[index])
                {
                    folder2 = folder1;
                    folder1 = folder1.folders[path[index]];
                }
                else
                {
                    os.write("Invalid Path");
                    return folder1;
                }
            }
            return folder1;
        }
    }
}
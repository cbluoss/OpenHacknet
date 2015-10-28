// Decompiled with JetBrains decompiler
// Type: Hacknet.Computer
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    [Serializable]
    internal class Computer
    {
        public const byte CORPORATE = 1;
        public const byte HOME = 2;
        public const byte SERVER = 3;
        public const byte EMPTY = 4;
        public const byte EOS = 5;
        public static float BASE_BOOT_TIME = Settings.isConventionDemo ? 15f : 25.5f;
        public static float BASE_REBOOT_TIME = 10.5f;
        public static float BASE_PROXY_TICKS = 30f;
        public static float BASE_TRACE_TIME = 15f;
        public Administrator admin;
        public string adminIP;
        public string adminPass;
        public bool AllowsDefaultBootModule = true;
        public string attatchedDeviceIDs;
        private float bootTimer;
        public UserDetail currentUser;
        public List<Daemon> daemons;
        public bool disabled;
        public ExternalCounterpart externalCounterpart;
        public FileSystem files;
        public Firewall firewall;
        public bool firewallAnalysisInProgress;
        public bool hasProxy;
        public float highlightFlashTime;
        public string icon;
        public string idName;
        public string ip;
        public List<int> links;
        public Vector2 location;
        public string name;
        private readonly OS os;
        public List<int> ports;
        public int portsNeededForCrack;
        public List<byte> portsOpen;
        public bool proxyActive;
        public float proxyOverloadTicks;
        public ShellExe reportingShell;
        public int securityLevel;
        public bool silent;
        public float startingOverloadTicks = -1f;
        private float timeLastPinged;
        public float traceTime;
        public byte type;
        public bool userLoggedIn;
        public List<UserDetail> users;

        public Computer(string compName, string compIP, Vector2 compLocation, int seclevel, byte compType, OS opSystem)
        {
            name = compName;
            ip = compIP;
            location = compLocation;
            type = compType;
            files = generateRandomFileSystem();
            idName = compName.Replace(" ", "_");
            os = opSystem;
            traceTime = -1f;
            securityLevel = seclevel;
            adminIP = NetworkMap.generateRandomIP();
            users = new List<UserDetail>();
            adminPass = PortExploits.getRandomPassword();
            users.Add(new UserDetail("admin", adminPass, 1));
            ports = new List<int>(seclevel);
            portsOpen = new List<byte>(seclevel);
            openPortsForSecurityLevel(seclevel);
            links = new List<int>();
            daemons = new List<Daemon>();
        }

        public void initDaemons()
        {
            for (var index = 0; index < daemons.Count; ++index)
            {
                daemons[index].initFiles();
                daemons[index].registerAsDefaultBootDaemon();
            }
        }

        public FileSystem generateRandomFileSystem()
        {
            var fileSystem = new FileSystem();
            if (type != 5)
            {
                var num1 = Utils.random.Next(6);
                for (var index1 = 0; index1 < num1; ++index1)
                {
                    var num2 = 0;
                    string str1;
                    do
                    {
                        str1 = (num2 > 10 ? "AA" : "") + generateFolderName(Utils.random.Next(100));
                        ++num2;
                    } while (fileSystem.root.folders[0].searchForFolder(str1) != null);
                    var folder = new Folder(str1);
                    var num3 = Utils.random.Next(3);
                    for (var index2 = 0; index2 < num3; ++index2)
                    {
                        if (Utils.random.NextDouble() > 0.8)
                        {
                            var num4 = 0;
                            string str2;
                            do
                            {
                                str2 = generateFileName(Utils.random.Next(300));
                                ++num4;
                                if (num4 > 3)
                                    str2 = (Utils.getRandomChar() + Utils.getRandomChar()).ToString() + (object) str2;
                            } while (folder.searchForFile(str2) != null);
                            folder.files.Add(
                                new FileEntry(
                                    Utils.flipCoin()
                                        ? generateFileData(Utils.random.Next(500))
                                        : generateBinaryString(500), str2));
                        }
                        else
                        {
                            var fileEntry = new FileEntry();
                            var str2 = fileEntry.name;
                            while (folder.searchForFile(fileEntry.name) != null)
                                fileEntry.name = Utils.getRandomChar().ToString() + (int) Utils.getRandomChar() +
                                                 (object) str2;
                            folder.files.Add(fileEntry);
                        }
                    }
                    fileSystem.root.folders[0].folders.Add(folder);
                }
            }
            else
                fileSystem.root.folders.Insert(0, EOSComp.GenerateEOSFolder());
            return fileSystem;
        }

        public void openPortsForSecurityLevel(int security)
        {
            portsNeededForCrack = security - 1;
            if (security >= 5)
            {
                --portsNeededForCrack;
                var time = 0.0f;
                for (var index = 4; index < security; ++index)
                    time += BASE_PROXY_TICKS/(index - 3);
                addProxy(time);
            }
            switch (security)
            {
                case 1:
                    openPorts(PortExploits.portNums.Count - 1);
                    break;
                default:
                    openPorts(PortExploits.portNums.Count);
                    break;
            }
            if (security >= 4)
                traceTime = Math.Max(10 - security, 3)*BASE_TRACE_TIME;
            if (security < 5)
                return;
            firewall = new Firewall(security - 5);
            admin = new BasicAdministrator();
        }

        private void openPorts(int n)
        {
            for (var index = 4 - 1; index >= 0; --index)
            {
                ports.Add(PortExploits.portNums[index]);
                portsOpen.Add(0);
            }
        }

        public void addProxy(float time)
        {
            if (time <= 0.0)
                return;
            hasProxy = true;
            proxyActive = true;
            proxyOverloadTicks = time;
            startingOverloadTicks = proxyOverloadTicks;
        }

        public void addFirewall(int level)
        {
            firewall = new Firewall(level);
        }

        public void addFirewall(int level, string solution)
        {
            firewall = new Firewall(level, solution);
        }

        public void addFirewall(int level, string solution, float additionalTime)
        {
            firewall = new Firewall(level, solution, additionalTime);
        }

        public void addMultiplayerTargetFile()
        {
            files.root.folders[0].files.Add(
                new FileEntry("#CRITICAL SYSTEM FILE - DO NOT MODIFY#\n\n" + generateBinaryString(2000), "system32.sys"));
        }

        private void sendNetworkMessage(string s)
        {
            if (os.multiplayer && !silent)
                os.sendMessage(s);
            if (externalCounterpart == null)
                return;
            externalCounterpart.writeMessage(s);
        }

        private void tryExternalCounterpartDisconnect()
        {
            if (externalCounterpart == null)
                return;
            externalCounterpart.disconnect();
        }

        public void hostileActionTaken()
        {
            if (os.connectedComp == null || !os.connectedComp.ip.Equals(ip))
                return;
            if (traceTime > 0.0)
                os.traceTracker.start(traceTime);
            if (os.timer - (double) timeLastPinged <= 0.349999994039536)
                return;
            SFX.addCircle(getScreenSpacePosition(), os.brightLockedColor, 25f);
            timeLastPinged = os.timer;
        }

        public void bootupTick(float t)
        {
            bootTimer -= t;
            if (bootTimer > 0.0)
                return;
            disabled = false;
        }

        public void log(string message)
        {
            if (disabled)
                return;
            if (reportingShell != null)
                reportingShell.reportedTo(message);
            message = string.Concat("@", (int) OS.currentElapsedTime, " ", message);
            var str1 = message;
            if (str1.Length > 256)
                str1 = str1.Substring(0, 256);
            var str2 = str1;
            var num = 0;
            bool flag;
            do
            {
                flag = false;
                var folder = files.root.searchForFolder("log");
                for (var index = 0; index < folder.files.Count; ++index)
                {
                    if (folder.files[index].name == str2)
                    {
                        flag = true;
                        ++num;
                        str2 = string.Concat(str1, "(", num, ")");
                    }
                }
            } while (flag);
            var nameEntry = str2;
            files.root.searchForFolder("log").files.Insert(0, new FileEntry(message, nameEntry));
        }

        public string generateFolderName(int seed)
        {
            return "NewFolder" + seed;
        }

        public string generateFileName(int seed)
        {
            return "Data" + seed;
        }

        public string generateFileData(int seed)
        {
            var str = "";
            for (var index = 0; index < seed; ++index)
                str = str + (object) " " + index;
            return str;
        }

        public bool connect(string ipFrom)
        {
            if (disabled)
                return false;
            log("Connection: from " + ipFrom);
            sendNetworkMessage("cConnection " + ip + " " + ipFrom);
            if (externalCounterpart != null)
                externalCounterpart.establishConnection();
            userLoggedIn = false;
            return true;
        }

        public void addNewUser(string ipFrom, string name, string pass, byte type)
        {
            addNewUser(ipFrom, new UserDetail(name, pass, type));
        }

        public void addNewUser(string ipFrom, UserDetail usr)
        {
            users.Add(usr);
            if (!silent)
                log("User Account Added: from " + ipFrom + " -Name: " + name);
            sendNetworkMessage("cAddUser #" + (object) ip + "#" + ipFrom + "#" + name + "#" + usr.pass + "#" + usr.type);
            for (var index = 0; index < daemons.Count; ++index)
                daemons[index].userAdded(usr.name, usr.pass, usr.type);
        }

        public void crash(string ipFrom)
        {
            if (os.connectedComp != null && os.connectedComp.Equals(this) && !os.connectedComp.Equals(os.thisComputer))
                os.connectedComputerCrashed(this);
            else if (os.thisComputer.Equals(this) || os.thisComputer.Equals(os.connectedComp))
                os.thisComputerCrashed();
            if (!silent)
                log("CRASH REPORT: Kernel Panic -- Fatal Trap");
            disabled = true;
            bootTimer = BASE_BOOT_TIME;
            tryExternalCounterpartDisconnect();
            sendNetworkMessage("cCrash " + ip + " " + ipFrom);
        }

        public void reboot(string ipFrom)
        {
            if (os.connectedComp != null && os.connectedComp.Equals(this) && !os.connectedComp.Equals(os.thisComputer))
                os.connectedComputerCrashed(this);
            else if (os.thisComputer.Equals(this) || os.thisComputer.Equals(os.connectedComp))
                os.rebootThisComputer();
            if (!silent)
                log("Rebooting system : " + ipFrom);
            disabled = true;
            bootTimer = BASE_REBOOT_TIME;
            tryExternalCounterpartDisconnect();
            sendNetworkMessage("cReboot " + ip + " " + ipFrom);
        }

        public bool canReadFile(string ipFrom, FileEntry f, int index)
        {
            var flag = false;
            if (ipFrom.Equals(adminIP))
                flag = true;
            for (var index1 = 0; index1 < users.Count; ++index1)
            {
                if (ipFrom.Equals(users[index1]))
                    flag = true;
            }
            if (!flag)
                return false;
            if (f.name[0] != 64)
            {
                log("FileRead: by " + ipFrom + " - file:" + f.name);
                sendNetworkMessage("cFile " + (object) ip + " " + ipFrom + " " + f.name + " " + index);
            }
            return true;
        }

        public bool canCopyFile(string ipFrom, string name)
        {
            if (!silent && !ipFrom.Equals(adminIP))
                return false;
            log("FileCopied: by " + ipFrom + " - file:" + name);
            sendNetworkMessage("cCopy " + ip + " " + ipFrom + " " + name);
            return true;
        }

        public bool deleteFile(string ipFrom, string name, List<int> folderPath)
        {
            if (!silent && !ipFrom.Equals(adminIP) && !ipFrom.Equals(ip))
                return false;
            if (name[0] != 64)
                log("FileDeleted: by " + ipFrom + " - file:" + name);
            var startFolder = files.root;
            if (folderPath.Count > 0)
                startFolder = Programs.getFolderFromNavigationPath(folderPath, startFolder, os);
            var str1 = name;
            if (name[0] == 64)
                str1 = name.Substring(name.IndexOf('_'));
            for (var index = 0; index < startFolder.files.Count; ++index)
            {
                var str2 = startFolder.files[index].name;
                if (name[0] != 64 ? str2.Equals(name) : str2.Substring(str2.IndexOf('_')).Equals(str1))
                {
                    startFolder.files.RemoveAt(index);
                    break;
                }
            }
            var s = "cDelete #" + ip + "#" + ipFrom + "#" + name;
            for (var index = 0; index < folderPath.Count; ++index)
                s = s + (object) "#" + folderPath[index];
            sendNetworkMessage(s);
            return true;
        }

        public bool moveFile(string ipFrom, string name, string newName, List<int> folderPath, List<int> destFolderPath)
        {
            if (!silent && !ipFrom.Equals(adminIP) && !ipFrom.Equals(ip))
                return false;
            var folder1 = files.root;
            var fromNavigationPath = Programs.getFolderFromNavigationPath(folderPath, files.root, os);
            var folder2 = Programs.getFolderFromNavigationPath(destFolderPath, files.root, os);
            if (newName.StartsWith("/"))
            {
                if (destFolderPath.Count == 0 ||
                    folderPath.Count > 0 && destFolderPath.Count == folderPath.Count &&
                    destFolderPath[0] == folderPath[0])
                {
                    folder2 = files.root;
                    newName = newName.Substring(1);
                    var folder3 = folder2.searchForFolder(newName);
                    if (folder3 != null)
                    {
                        folder2 = folder3;
                        newName = name;
                    }
                }
                else
                    newName = newName.Substring(1);
            }
            FileEntry fileEntry = null;
            for (var index = 0; index < fromNavigationPath.files.Count; ++index)
            {
                if (fromNavigationPath.files[index].name == name)
                {
                    fileEntry = fromNavigationPath.files[index];
                    fromNavigationPath.files.RemoveAt(index);
                    break;
                }
            }
            if (fileEntry == null)
            {
                os.write("File not Found");
                return false;
            }
            if (newName == "" || newName == " ")
                newName = name;
            fileEntry.name = newName;
            var str1 = fileEntry.name;
            var num = 1;
            while (folder2.searchForFile(fileEntry.name) != null)
            {
                fileEntry.name = string.Concat(str1, "(", num, ")");
                ++num;
            }
            folder2.files.Add(fileEntry);
            var str2 = "cMove #" + ip + "#" + ipFrom + "#" + name + "#" + newName + "#";
            for (var index = 0; index < folderPath.Count; ++index)
                str2 = str2 + (object) "%" + folderPath[index];
            var s = str2 + "#";
            for (var index = 0; index < destFolderPath.Count; ++index)
                s = s + (object) "%" + destFolderPath[index];
            sendNetworkMessage(s);
            log("File Moved: by " + ipFrom + " - file:" + name + " To: " + newName);
            return true;
        }

        public bool makeFile(string ipFrom, string name, string data, List<int> folderPath, bool isUpload = false)
        {
            if (!isUpload && !silent && (!ipFrom.Equals(adminIP) && !ipFrom.Equals(ip)))
                return false;
            if (name[0] != 64)
                log("FileCreated: by " + ipFrom + " - file:" + name);
            var folder = files.root;
            if (folderPath.Count > 0)
            {
                for (var index = 0; index < folderPath.Count; ++index)
                {
                    if (folder.folders.Count > folderPath[index])
                        folder = folder.folders[folderPath[index]];
                }
            }
            if (isUpload)
                folder.files.Insert(0, new FileEntry(data, name));
            else
                folder.files.Add(new FileEntry(data, name));
            var s = "cMake #" + ip + "#" + ipFrom + "#" + name + "#" + data;
            for (var index = 0; index < folderPath.Count; ++index)
                s = s + (object) "#" + folderPath[index];
            sendNetworkMessage(s);
            return true;
        }

        public bool makeFolder(string ipFrom, string name, List<int> folderPath)
        {
            if (!silent && !ipFrom.Equals(adminIP) && !ipFrom.Equals(ip))
                return false;
            if (name[0] != 64)
                log("FolderCreated: by " + ipFrom + " - folder:" + name);
            var folder = files.root;
            if (folderPath.Count > 0)
            {
                for (var index = 0; index < folderPath.Count; ++index)
                {
                    if (folder.folders.Count > folderPath[index])
                        folder = folder.folders[folderPath[index]];
                }
            }
            folder.folders.Add(new Folder(name));
            var s = "cMkDir #" + ip + "#" + ipFrom + "#" + name;
            for (var index = 0; index < folderPath.Count; ++index)
                s = s + (object) "#" + folderPath[index];
            sendNetworkMessage(s);
            return true;
        }

        public void disconnecting(string ipFrom)
        {
            log(ipFrom + " Disconnected");
            if (os.multiplayer && !silent)
                sendNetworkMessage("cDisconnect " + ip + " " + ipFrom);
            tryExternalCounterpartDisconnect();
        }

        public void giveAdmin(string ipFrom)
        {
            adminIP = ipFrom;
            log(ipFrom + " Became Admin");
            if (os.multiplayer && !silent)
                sendNetworkMessage("cAdmin " + ip + " " + ipFrom);
            var userDetail = users[0];
            userDetail.known = true;
            users[0] = userDetail;
        }

        public void openPort(int portNum, string ipFrom)
        {
            var index1 = -1;
            for (var index2 = 0; index2 < ports.Count; ++index2)
            {
                if (ports[index2] == portNum)
                    index1 = index2;
            }
            if (index1 != -1 && !silent)
                portsOpen[index1] = 1;
            log(ipFrom + (object) " Opened Port#" + portNum);
            sendNetworkMessage("cPortOpen " + (object) ip + " " + ipFrom + " " + portNum);
        }

        public void closePort(int portNum, string ipFrom)
        {
            var index1 = -1;
            for (var index2 = 0; index2 < ports.Count; ++index2)
            {
                if (ports[index2] == portNum)
                    index1 = index2;
            }
            var flag = false;
            if (index1 != -1)
            {
                flag = portsOpen[index1] != 0;
                if (!silent)
                    portsOpen[index1] = 0;
            }
            if (flag)
                log(ipFrom + (object) " Closed Port#" + portNum);
            sendNetworkMessage("cPortClose " + (object) ip + " " + ipFrom + " " + portNum);
        }

        public void openCDTray(string ipFrom)
        {
            if (os.thisComputer.ip.Equals(ip))
                Programs.cdDrive(true);
            sendNetworkMessage("cCDDrive " + ip + " open");
        }

        public void closeCDTray(string ipFrom)
        {
            if (os.thisComputer.ip.Equals(ip))
                Programs.cdDrive(false);
            sendNetworkMessage("cCDDrive " + ip + " close");
        }

        public void forkBombClients(string ipFrom)
        {
            sendNetworkMessage("cFBClients " + ip + " " + ipFrom);
            if (os.multiplayer)
                return;
            for (var index = 0; index < os.ActiveHackers.Count; ++index)
            {
                if (os.ActiveHackers[index].Value == ip)
                    Programs.getComputer(os, os.ActiveHackers[index].Key).crash(ip);
            }
        }

        public virtual int login(string username, string password, byte type = 1)
        {
            if (username.ToLower().Equals("admin") && password.Equals(adminPass))
            {
                giveAdmin(os.thisComputer.ip);
                return 1;
            }
            for (var index = 0; index < users.Count; ++index)
            {
                if (users[index].name.Equals(username) && users[index].pass.Equals(password) &&
                    (users[index].type == type || type == 1))
                {
                    currentUser = users[index];
                    return 2;
                }
            }
            return 0;
        }

        public void setAdminPassword(string newPass)
        {
            adminPass = newPass;
            for (var index = 0; index < users.Count; ++index)
            {
                if (users[index].name.ToLower().Equals("admin"))
                    users[index] = new UserDetail("admin", newPass, 0);
            }
        }

        public string getSaveString()
        {
            var str1 = "none";
            if (os.netMap.mailServer.Equals(this))
                str1 = "mail";
            if (os.thisComputer.Equals(this))
                str1 = "player";
            var str2 = attatchedDeviceIDs == null ? "" : " devices=\"" + attatchedDeviceIDs + "\"";
            var str3 = "<computer name=\"" + (object) name + "\" ip=\"" + ip + "\" type=\"" + type + "\" spec=\"" + str1 +
                       "\" id=\"" + idName + "\" " + (icon == null ? "" : "icon=\"" + icon + "\"") + str2 + " >\n" +
                       "<location x=\"" + location.X.ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                       location.Y.ToString(CultureInfo.InvariantCulture) + "\" />" + (object) "<security level=\"" +
                       securityLevel + "\" traceTime=\"" + traceTime.ToString(CultureInfo.InvariantCulture) +
                       (startingOverloadTicks > 0.0
                           ? "\" proxyTime=\"" +
                             (hasProxy ? startingOverloadTicks.ToString(CultureInfo.InvariantCulture) : "-1")
                           : "") + "\" portsToCrack=\"" + (string) (object) portsNeededForCrack + "\" adminIP=\"" +
                       adminIP + "\" />" + "<admin type=\"" +
                       (admin == null ? "none" : (admin is FastBasicAdministrator ? "fast" : "basic")) +
                       "\" resetPass=\"" + (admin == null || !admin.ResetsPassword ? "false" : "true") + "\"" +
                       " isSuper=\"" + (admin == null || !admin.IsSuper ? "false" : "true") + "\" />";
            var str4 = "";
            for (var index = 0; index < links.Count; ++index)
                str4 = str4 + (object) " " + links[index];
            var str5 = str3 + "<links>" + str4 + "</links>\n";
            if (firewall != null)
                str5 = str5 + firewall.getSaveString() + "\n";
            var str6 = "";
            for (var index = 0; index < portsOpen.Count; ++index)
                str6 = str6 + (object) " " + ports[index];
            var str7 = str5 + "<portsOpen>" + str6 + "</portsOpen>\n" + "<users>\n";
            for (var index = 0; index < users.Count; ++index)
                str7 = str7 + users[index].getSaveString() + "\n";
            var str8 = str7 + "</users>\n" + "<daemons>\n";
            for (var index = 0; index < daemons.Count; ++index)
                str8 = str8 + daemons[index].getSaveString() + "\n";
            return str8 + "</daemons>\n" + files.getSaveString() + "</computer>\n";
        }

        public static Computer load(XmlReader reader, OS os)
        {
            while (reader.Name != "computer")
                reader.Read();
            reader.MoveToAttribute("name");
            var compName = reader.ReadContentAsString();
            reader.MoveToAttribute("ip");
            var compIP = reader.ReadContentAsString();
            reader.MoveToAttribute("type");
            var compType = (byte) reader.ReadContentAsInt();
            reader.MoveToAttribute("spec");
            var str1 = reader.ReadContentAsString();
            reader.MoveToAttribute("id");
            var str2 = reader.ReadContentAsString();
            string str3 = null;
            if (reader.MoveToAttribute("devices"))
                str3 = reader.ReadContentAsString();
            string str4 = null;
            if (reader.MoveToAttribute("icon"))
                str4 = reader.ReadContentAsString();
            while (reader.Name != "location")
                reader.Read();
            reader.MoveToAttribute("x");
            var x = reader.ReadContentAsFloat();
            reader.MoveToAttribute("y");
            var y = reader.ReadContentAsFloat();
            while (reader.Name != "security")
                reader.Read();
            reader.MoveToAttribute("level");
            var seclevel = reader.ReadContentAsInt();
            reader.MoveToAttribute("traceTime");
            var num1 = reader.ReadContentAsFloat();
            reader.MoveToAttribute("portsToCrack");
            var num2 = reader.ReadContentAsInt();
            reader.MoveToAttribute("adminIP");
            var str5 = reader.ReadContentAsString();
            var time = -1f;
            if (reader.MoveToAttribute("proxyTime"))
                time = reader.ReadContentAsFloat();
            while (reader.Name != "admin")
                reader.Read();
            reader.MoveToAttribute("type");
            var str6 = reader.ReadContentAsString();
            reader.MoveToAttribute("resetPass");
            var flag1 = reader.ReadContentAsBoolean();
            reader.MoveToAttribute("isSuper");
            var flag2 = reader.ReadContentAsBoolean();
            Administrator administrator = null;
            switch (str6)
            {
                case "fast":
                    administrator = new FastBasicAdministrator();
                    break;
                case "basic":
                    administrator = new BasicAdministrator();
                    break;
            }
            if (administrator != null)
                administrator.ResetsPassword = flag1;
            if (administrator != null)
                administrator.IsSuper = flag2;
            while (reader.Name != "links")
                reader.Read();
            var strArray = reader.ReadElementContentAsString().Split();
            Firewall firewall = null;
            while (reader.Name != "portsOpen" && reader.Name != "firewall")
                reader.Read();
            if (reader.Name == "firewall")
                firewall = Firewall.load(reader);
            while (reader.Name != "portsOpen")
                reader.Read();
            var portsList = reader.ReadElementContentAsString();
            var computer = new Computer(compName, compIP, new Vector2(x, y), seclevel, compType, os);
            computer.firewall = firewall;
            computer.admin = administrator;
            if (time > 0.0)
            {
                computer.addProxy(time);
            }
            else
            {
                computer.hasProxy = false;
                computer.proxyActive = false;
            }
            while (reader.Name != "users")
                reader.Read();
            computer.users.Clear();
            while (!(reader.Name == "users") || reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.Name == "user")
                {
                    var userDetail = UserDetail.loadUserDetail(reader);
                    if (userDetail.name.ToLower() == "admin")
                        computer.adminPass = userDetail.pass;
                    computer.users.Add(userDetail);
                }
                reader.Read();
            }
            while (reader.Name != "daemons")
                reader.Read();
            reader.Read();
            while (!(reader.Name == "daemons") || reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.Name == "MailServer")
                {
                    reader.MoveToAttribute("name");
                    var name = reader.ReadContentAsString();
                    var mailServer = new MailServer(computer, name, os);
                    computer.daemons.Add(mailServer);
                    if (reader.MoveToAttribute("color"))
                    {
                        var newThemeColor = Utils.convertStringToColor(reader.ReadContentAsString());
                        mailServer.setThemeColor(newThemeColor);
                    }
                }
                else if (reader.Name == "MissionListingServer")
                {
                    reader.MoveToAttribute("name");
                    var serviceName = reader.ReadContentAsString();
                    reader.MoveToAttribute("group");
                    var group = reader.ReadContentAsString();
                    reader.MoveToAttribute("public");
                    var _isPublic = reader.ReadContentAsString().ToLower().Equals("true");
                    reader.MoveToAttribute("assign");
                    var _isAssigner = reader.ReadContentAsString().ToLower().Equals("true");
                    var missionListingServer = new MissionListingServer(computer, serviceName, group, os, _isPublic,
                        _isAssigner);
                    computer.daemons.Add(missionListingServer);
                }
                else if (reader.Name == "AddEmailServer")
                {
                    reader.MoveToAttribute("name");
                    var serviceName = reader.ReadContentAsString();
                    var addEmailDaemon = new AddEmailDaemon(computer, serviceName, os);
                    computer.daemons.Add(addEmailDaemon);
                }
                else if (reader.Name == "MessageBoard")
                {
                    reader.MoveToAttribute("name");
                    var str7 = reader.ReadContentAsString();
                    var messageBoardDaemon = new MessageBoardDaemon(computer, os);
                    messageBoardDaemon.name = str7;
                    computer.daemons.Add(messageBoardDaemon);
                }
                else if (reader.Name == "WebServer")
                {
                    reader.MoveToAttribute("name");
                    var serviceName = reader.ReadContentAsString();
                    reader.MoveToAttribute("url");
                    var pageFileLocation = reader.ReadContentAsString();
                    var webServerDaemon = new WebServerDaemon(computer, serviceName, os, pageFileLocation);
                    computer.daemons.Add(webServerDaemon);
                }
                else if (reader.Name == "OnlineWebServer")
                {
                    reader.MoveToAttribute("name");
                    var serviceName = reader.ReadContentAsString();
                    reader.MoveToAttribute("url");
                    var url = reader.ReadContentAsString();
                    var onlineWebServerDaemon = new OnlineWebServerDaemon(computer, serviceName, os);
                    onlineWebServerDaemon.setURL(url);
                    computer.daemons.Add(onlineWebServerDaemon);
                }
                else if (reader.Name == "AcademicDatabse")
                {
                    reader.MoveToAttribute("name");
                    var serviceName = reader.ReadContentAsString();
                    var academicDatabaseDaemon = new AcademicDatabaseDaemon(computer, serviceName, os);
                    computer.daemons.Add(academicDatabaseDaemon);
                }
                else if (reader.Name == "MissionHubServer")
                {
                    var missionHubServer = new MissionHubServer(computer, "unknown", "unknown", os);
                    computer.daemons.Add(missionHubServer);
                }
                else if (reader.Name == "DeathRowDatabase")
                {
                    var rowDatabaseDaemon = new DeathRowDatabaseDaemon(computer, "Death Row Database", os);
                    computer.daemons.Add(rowDatabaseDaemon);
                }
                else if (reader.Name == "MedicalDatabase")
                {
                    var medicalDatabaseDaemon = new MedicalDatabaseDaemon(computer, os);
                    computer.daemons.Add(medicalDatabaseDaemon);
                }
                else if (reader.Name == "HeartMonitor")
                {
                    var str7 = "UNKNOWN";
                    if (reader.MoveToAttribute("patient"))
                        str7 = reader.ReadContentAsString();
                    computer.daemons.Add(new HeartMonitorDaemon(computer, os)
                    {
                        PatientID = str7
                    });
                }
                else if (reader.Name.Equals("PointClicker"))
                {
                    var pointClickerDaemon = new PointClickerDaemon(computer, "Point Clicker!", os);
                    computer.daemons.Add(pointClickerDaemon);
                }
                else if (reader.Name.Equals("ispSystem"))
                {
                    var ispDaemon = new ISPDaemon(computer, os);
                    computer.daemons.Add(ispDaemon);
                }
                else if (reader.Name.Equals("porthackheart"))
                {
                    var porthackHeartDaemon = new PorthackHeartDaemon(computer, os);
                    computer.daemons.Add(porthackHeartDaemon);
                }
                else if (reader.Name.Equals("SongChangerDaemon"))
                {
                    var songChangerDaemon = new SongChangerDaemon(computer, os);
                    computer.daemons.Add(songChangerDaemon);
                }
                else if (reader.Name == "UploadServerDaemon")
                {
                    string str7;
                    var input = str7 = "";
                    var foldername = str7;
                    var serviceName = str7;
                    if (reader.MoveToAttribute("name"))
                        serviceName = reader.ReadContentAsString();
                    if (reader.MoveToAttribute("foldername"))
                        foldername = reader.ReadContentAsString();
                    if (reader.MoveToAttribute("color"))
                        input = reader.ReadContentAsString();
                    var needsAuthentication = false;
                    if (reader.MoveToAttribute("needsAuh"))
                        needsAuthentication = reader.ReadContentAsBoolean();
                    var themeColor = Color.White;
                    if (input != "")
                        themeColor = Utils.convertStringToColor(input);
                    var uploadServerDaemon = new UploadServerDaemon(computer, serviceName, themeColor, os, foldername,
                        needsAuthentication);
                    computer.daemons.Add(uploadServerDaemon);
                }
                reader.Read();
            }
            computer.files = FileSystem.load(reader);
            computer.traceTime = num1;
            computer.portsNeededForCrack = num2;
            computer.adminIP = str5;
            computer.idName = str2;
            computer.icon = str4;
            computer.attatchedDeviceIDs = str3;
            for (var index = 0; index < strArray.Length; ++index)
            {
                if (strArray[index] != "")
                    computer.links.Add(Convert.ToInt32(strArray[index]));
            }
            if (portsList.Length > 0)
                ComputerLoader.loadPortsIntoComputer(portsList, computer);
            if (str1 == "mail")
                os.netMap.mailServer = computer;
            else if (str1 == "player")
                os.thisComputer = computer;
            return computer;
        }

        public string getTooltipString()
        {
            return name + "\n" + ip;
        }

        public Vector2 getScreenSpacePosition()
        {
            var nodeDrawPos = os.netMap.GetNodeDrawPos(location);
            return new Vector2(os.netMap.bounds.X + nodeDrawPos.X + NetworkMap.NODE_SIZE/2,
                os.netMap.bounds.Y + nodeDrawPos.Y + NetworkMap.NODE_SIZE/2);
        }

        public Daemon getDaemon(Type t)
        {
            for (var index = 0; index < daemons.Count; ++index)
            {
                if (daemons[index].GetType().Equals(t))
                    return daemons[index];
            }
            return null;
        }

        public static string generateBinaryString(int length)
        {
            var buffer = new byte[length/8];
            Utils.random.NextBytes(buffer);
            var str = "";
            for (var index = 0; index < buffer.Length; ++index)
                str += Convert.ToString(buffer[index], 2);
            return str;
        }

        public static Folder getFolderAtDepth(Computer c, int depth, List<int> path)
        {
            var folder = c.files.root;
            if (path.Count > 0)
            {
                for (var index = 0; index < depth; ++index)
                {
                    if (folder.folders.Count > path[index])
                        folder = folder.folders[path[index]];
                }
            }
            return folder;
        }

        public override string ToString()
        {
            return "Comp : " + idName;
        }

        public static Computer loadFromFile(string filename)
        {
            return (Computer) ComputerLoader.loadComputer(filename);
        }

        public Folder getFolderFromPath(string path, bool createFoldersThatDontExist = false)
        {
            var folderPath = getFolderPath(path, createFoldersThatDontExist);
            return getFolderAtDepth(this, folderPath.Count, folderPath);
        }

        public List<int> getFolderPath(string path, bool createFoldersThatDontExist = false)
        {
            var list = new List<int>();
            var chArray = new char[2]
            {
                '/',
                '\\'
            };
            var strArray = path.Split(chArray);
            var folder = files.root;
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                var flag = false;
                for (var index2 = 0; index2 < folder.folders.Count; ++index2)
                {
                    if (folder.folders[index2].name.Equals(strArray[index1]))
                    {
                        list.Add(index2);
                        folder = folder.folders[index2];
                        flag = true;
                        break;
                    }
                }
                if (!flag && createFoldersThatDontExist)
                {
                    folder.folders.Add(new Folder(strArray[index1]));
                    var index2 = folder.folders.Count - 1;
                    folder = folder.folders[index2];
                    list.Add(index2);
                }
            }
            return list;
        }
    }
}
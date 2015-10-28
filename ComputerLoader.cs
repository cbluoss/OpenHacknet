// Decompiled with JetBrains decompiler
// Type: Hacknet.ComputerLoader
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Xml;
using Hacknet.Mission;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    public static class ComputerLoader
    {
        public static Action MissionPreLoadComplete;
        public static Action postAllLoadedActions;
        private static OS os;

        public static void init(object opsys)
        {
            os = (OS) opsys;
        }

        public static object loadComputer(string filename)
        {
            var rdr = XmlReader.Create(TitleContainer.OpenStream(filename));
            var str1 = "UNKNOWN";
            var compName = "UNKNOWN";
            string str2 = null;
            var seclevel = 0;
            byte compType = 1;
            var flag1 = true;
            var compIP = NetworkMap.generateRandomIP();
            while (rdr.Name != "Computer")
                rdr.Read();
            if (rdr.MoveToAttribute("id"))
                str1 = rdr.ReadContentAsString();
            if (rdr.MoveToAttribute("name"))
                compName = rdr.ReadContentAsString();
            if (rdr.MoveToAttribute("security"))
                seclevel = rdr.ReadContentAsInt();
            if (rdr.MoveToAttribute("type"))
                compType = (byte) rdr.ReadContentAsInt();
            if (rdr.MoveToAttribute("ip"))
                compIP = rdr.ReadContentAsString();
            if (rdr.MoveToAttribute("icon"))
                str2 = rdr.ReadContentAsString();
            if (rdr.MoveToAttribute("allowsDefaultBootModule"))
                flag1 = rdr.ReadContentAsBoolean();
            var computer1 = new Computer(compName, compIP, os.netMap.getRandomPosition(), seclevel, compType, os);
            computer1.idName = str1;
            computer1.AllowsDefaultBootModule = flag1;
            computer1.icon = str2;
            if (computer1.type == 4)
            {
                var folder = computer1.files.root.searchForFolder("home");
                if (folder != null)
                {
                    folder.files.Clear();
                    folder.folders.Clear();
                }
            }
            while (rdr.Name != "Computer")
            {
                if (rdr.Name.Equals("file"))
                {
                    var path = !rdr.MoveToAttribute("path") ? "home" : rdr.ReadContentAsString();
                    var str3 = filter(!rdr.MoveToAttribute("name") ? "Data" : rdr.ReadContentAsString());
                    var num = (int) rdr.MoveToContent();
                    var s = rdr.ReadElementContentAsString();
                    if (s.Equals(""))
                        s = Computer.generateBinaryString(500);
                    var dataEntry = filter(s);
                    var folderFromPath = computer1.getFolderFromPath(path, true);
                    if (folderFromPath.searchForFile(str3) != null)
                        folderFromPath.searchForFile(str3).data = dataEntry;
                    else
                        folderFromPath.files.Add(new FileEntry(dataEntry, str3));
                }
                if (rdr.Name.Equals("encryptedFile"))
                {
                    var flag2 = false;
                    var path = !rdr.MoveToAttribute("path") ? "home" : rdr.ReadContentAsString();
                    var s1 = !rdr.MoveToAttribute("name") ? "Data" : rdr.ReadContentAsString();
                    var header = !rdr.MoveToAttribute("header") ? "ERROR" : rdr.ReadContentAsString();
                    var ipLink = !rdr.MoveToAttribute("ip") ? "ERROR" : rdr.ReadContentAsString();
                    var pass = !rdr.MoveToAttribute("pass") ? "" : rdr.ReadContentAsString();
                    var fileExtension = !rdr.MoveToAttribute("extension") ? null : rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("double"))
                        flag2 = rdr.ReadContentAsBoolean();
                    var str3 = filter(s1);
                    var num = (int) rdr.MoveToContent();
                    var s2 = rdr.ReadElementContentAsString();
                    if (s2.Equals(""))
                        s2 = Computer.generateBinaryString(500);
                    var data = filter(s2);
                    if (flag2)
                        data = FileEncrypter.EncryptString(data, header, ipLink, pass, fileExtension);
                    var dataEntry = FileEncrypter.EncryptString(data, header, ipLink, pass,
                        flag2 ? "_LAYER2.dec" : fileExtension);
                    var folderFromPath = computer1.getFolderFromPath(path, true);
                    if (folderFromPath.searchForFile(str3) != null)
                        folderFromPath.searchForFile(str3).data = dataEntry;
                    else
                        folderFromPath.files.Add(new FileEntry(dataEntry, str3));
                }
                else if (rdr.Name.Equals("ports"))
                {
                    var num = (int) rdr.MoveToContent();
                    loadPortsIntoComputer(rdr.ReadElementContentAsString(), computer1);
                }
                else if (rdr.Name.Equals("positionNear"))
                {
                    var ip_Or_ID_or_Name = "";
                    if (rdr.MoveToAttribute("target"))
                        ip_Or_ID_or_Name = rdr.ReadContentAsString();
                    var num = 0;
                    var total = 3;
                    if (rdr.MoveToAttribute("position"))
                        num = rdr.ReadContentAsInt();
                    var positionNumber = num + 1;
                    if (rdr.MoveToAttribute("total"))
                        total = rdr.ReadContentAsInt();
                    var computer2 = Programs.getComputer(os, ip_Or_ID_or_Name);
                    if (computer2 != null)
                        computer1.location = computer2.location +
                                             Corporation.getNearbyNodeOffset(computer2.location, positionNumber, total,
                                                 os.netMap);
                }
                else if (rdr.Name.Equals("proxy"))
                {
                    var num = 1f;
                    if (rdr.MoveToAttribute("time"))
                        num = rdr.ReadContentAsFloat();
                    if (num > 0.0)
                    {
                        computer1.addProxy(Computer.BASE_PROXY_TICKS*num);
                    }
                    else
                    {
                        computer1.hasProxy = false;
                        computer1.proxyActive = false;
                    }
                }
                else if (rdr.Name.Equals("portsForCrack"))
                {
                    var num = -1;
                    if (rdr.MoveToAttribute("val"))
                        num = rdr.ReadContentAsInt();
                    if (num != -1)
                        computer1.portsNeededForCrack = num - 1;
                }
                else if (rdr.Name.Equals("firewall"))
                {
                    var level = 1;
                    if (rdr.MoveToAttribute("level"))
                        level = rdr.ReadContentAsInt();
                    if (level > 0)
                    {
                        string solution = null;
                        var additionalTime = 0.0f;
                        if (rdr.MoveToAttribute("solution"))
                            solution = rdr.ReadContentAsString();
                        if (rdr.MoveToAttribute("additionalTime"))
                            additionalTime = rdr.ReadContentAsFloat();
                        if (solution != null)
                            computer1.addFirewall(level, solution, additionalTime);
                        else
                            computer1.addFirewall(level);
                    }
                    else
                        computer1.firewall = null;
                }
                else if (rdr.Name.Equals("link"))
                {
                    var ip_Or_ID_or_Name = "";
                    if (rdr.MoveToAttribute("target"))
                        ip_Or_ID_or_Name = rdr.ReadContentAsString();
                    var computer2 = Programs.getComputer(os, ip_Or_ID_or_Name);
                    if (computer2 != null)
                        computer1.links.Add(os.netMap.nodes.IndexOf(computer2));
                }
                else if (rdr.Name.Equals("dlink"))
                {
                    var comp = "";
                    if (rdr.MoveToAttribute("target"))
                        comp = rdr.ReadContentAsString();
                    var local = computer1;
                    postAllLoadedActions += () =>
                    {
                        var computer2 = Programs.getComputer(os, comp);
                        if (computer2 == null)
                            return;
                        local.links.Add(os.netMap.nodes.IndexOf(computer2));
                    };
                }
                else if (rdr.Name.Equals("trace"))
                {
                    var num = 1f;
                    if (rdr.MoveToAttribute("time"))
                        num = rdr.ReadContentAsFloat();
                    computer1.traceTime = num;
                }
                else if (rdr.Name.Equals("adminPass"))
                {
                    string newPass = null;
                    if (rdr.MoveToAttribute("pass"))
                        newPass = rdr.ReadContentAsString();
                    if (newPass == null)
                        newPass = PortExploits.getRandomPassword();
                    computer1.setAdminPassword(newPass);
                }
                else if (rdr.Name.Equals("admin"))
                {
                    var str3 = "basic";
                    var flag2 = true;
                    var flag3 = false;
                    if (rdr.MoveToAttribute("type"))
                        str3 = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("resetPassword"))
                        flag2 = rdr.ReadContentAsBoolean();
                    if (rdr.MoveToAttribute("isSuper"))
                        flag3 = rdr.ReadContentAsBoolean();
                    switch (str3)
                    {
                        case "fast":
                            computer1.admin = new FastBasicAdministrator();
                            break;
                        default:
                            computer1.admin = new BasicAdministrator();
                            break;
                    }
                    computer1.admin.ResetsPassword = flag2;
                    computer1.admin.IsSuper = flag3;
                }
                else if (rdr.Name.Equals("ExternalCounterpart"))
                {
                    var serverName = "";
                    var idName = "";
                    if (rdr.MoveToAttribute("id"))
                        serverName = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("name"))
                        idName = rdr.ReadContentAsString();
                    var externalCounterpart = new ExternalCounterpart(idName,
                        ExternalCounterpart.getIPForServerName(serverName));
                    computer1.externalCounterpart = externalCounterpart;
                }
                else if (rdr.Name.Equals("account"))
                {
                    byte accountType = 0;
                    string s1;
                    var s2 = s1 = "ERROR";
                    if (rdr.MoveToAttribute("username"))
                        s2 = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("password"))
                        s1 = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("type"))
                        accountType = (byte) rdr.ReadContentAsInt();
                    var user = filter(s2);
                    var password = filter(s1);
                    var flag2 = false;
                    for (var index = 0; index < computer1.users.Count; ++index)
                    {
                        var userDetail = computer1.users[index];
                        if (userDetail.name.Equals(user))
                        {
                            userDetail.pass = password;
                            userDetail.type = accountType;
                            computer1.users[index] = userDetail;
                            if (user.Equals("admin"))
                                computer1.adminPass = password;
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        var userDetail = new UserDetail(user, password, accountType);
                        computer1.users.Add(userDetail);
                    }
                }
                else if (rdr.Name.Equals("missionListingServer"))
                {
                    var flag2 = false;
                    var _isPublic = false;
                    string serviceName;
                    var group = serviceName = "ERROR";
                    if (rdr.MoveToAttribute("name"))
                        serviceName = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("group"))
                        group = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("assigner"))
                        flag2 = rdr.ReadContentAsBoolean();
                    if (rdr.MoveToAttribute("public"))
                        _isPublic = rdr.ReadContentAsBoolean();
                    computer1.daemons.Add(new MissionListingServer(computer1, serviceName, group, os, _isPublic, false)
                    {
                        missionAssigner = flag2
                    });
                }
                else if (rdr.Name.Equals("mailServer"))
                {
                    var name = "Mail Server";
                    if (rdr.MoveToAttribute("name"))
                        name = rdr.ReadContentAsString();
                    var ms = new MailServer(computer1, name, os);
                    if (rdr.MoveToAttribute("color"))
                        ms.setThemeColor(Utils.convertStringToColor(rdr.ReadContentAsString()));
                    while (!(rdr.Name == "mailServer") || rdr.IsStartElement())
                    {
                        if (rdr.Name == "email")
                        {
                            var sender = "UNKNOWN";
                            string str3 = null;
                            var subject = "UNKNOWN";
                            if (rdr.MoveToAttribute("sender"))
                                sender = rdr.ReadContentAsString();
                            if (rdr.MoveToAttribute("recipient"))
                                str3 = rdr.ReadContentAsString();
                            if (rdr.MoveToAttribute("subject"))
                                subject = rdr.ReadContentAsString();
                            var num = (int) rdr.MoveToContent();
                            var body = rdr.ReadElementContentAsString();
                            if (str3 != null)
                            {
                                var email = MailServer.generateEmail(subject, body, sender);
                                var recp = str3;
                                ms.setupComplete += () => ms.addMail(email, recp);
                            }
                        }
                        rdr.Read();
                    }
                    computer1.daemons.Add(ms);
                }
                else if (rdr.Name.Equals("addEmailDaemon"))
                {
                    var addEmailDaemon = new AddEmailDaemon(computer1, "Final Task", os);
                    computer1.daemons.Add(addEmailDaemon);
                }
                else if (rdr.Name.Equals("deathRowDatabase"))
                {
                    var rowDatabaseDaemon = new DeathRowDatabaseDaemon(computer1, "Death Row Database", os);
                    computer1.daemons.Add(rowDatabaseDaemon);
                }
                else if (rdr.Name.Equals("ispSystem"))
                {
                    var ispDaemon = new ISPDaemon(computer1, os);
                    computer1.daemons.Add(ispDaemon);
                }
                else if (rdr.Name.Equals("messageBoard"))
                {
                    var messageBoardDaemon = new MessageBoardDaemon(computer1, os);
                    var str3 = "Anonymous";
                    if (rdr.MoveToAttribute("name"))
                        str3 = rdr.ReadContentAsString();
                    messageBoardDaemon.name = str3;
                    while (!(rdr.Name == "messageBoard") || rdr.IsStartElement())
                    {
                        if (rdr.Name == "thread")
                        {
                            var num = (int) rdr.MoveToContent();
                            var filename1 = rdr.ReadElementContentAsString();
                            if (filename1 != null)
                                messageBoardDaemon.AddThread(Utils.readEntireFile(filename1));
                        }
                        rdr.Read();
                    }
                    computer1.daemons.Add(messageBoardDaemon);
                }
                else if (rdr.Name.Equals("addAvconDemoEndDaemon"))
                {
                    var avconDemoEndDaemon = new AvconDemoEndDaemon(computer1, "Demo End", os);
                    computer1.daemons.Add(avconDemoEndDaemon);
                }
                else if (rdr.Name.Equals("addWebServer"))
                {
                    var serviceName = "Web Server";
                    string pageFileLocation = null;
                    if (rdr.MoveToAttribute("name"))
                        serviceName = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("url"))
                        pageFileLocation = rdr.ReadContentAsString();
                    var webServerDaemon = new WebServerDaemon(computer1, serviceName, os, pageFileLocation);
                    webServerDaemon.registerAsDefaultBootDaemon();
                    computer1.daemons.Add(webServerDaemon);
                }
                else if (rdr.Name.Equals("addOnlineWebServer"))
                {
                    var serviceName = "Web Server";
                    string url = null;
                    if (rdr.MoveToAttribute("name"))
                        serviceName = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("url"))
                        url = rdr.ReadContentAsString();
                    var onlineWebServerDaemon = new OnlineWebServerDaemon(computer1, serviceName, os);
                    if (url != null)
                        onlineWebServerDaemon.setURL(url);
                    onlineWebServerDaemon.registerAsDefaultBootDaemon();
                    computer1.daemons.Add(onlineWebServerDaemon);
                }
                else if (rdr.Name.Equals("uploadServerDaemon"))
                {
                    var serviceName = "File Upload Server";
                    string foldername = null;
                    var input = "0,94,38";
                    var needsAuthentication = false;
                    if (rdr.MoveToAttribute("name"))
                        serviceName = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("folder"))
                        foldername = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("color"))
                        input = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("needsAuth"))
                        needsAuthentication = rdr.ReadContentAsString().ToLower() == "true";
                    var themeColor = Utils.convertStringToColor(input);
                    var uploadServerDaemon = new UploadServerDaemon(computer1, serviceName, themeColor, os, foldername,
                        needsAuthentication);
                    uploadServerDaemon.registerAsDefaultBootDaemon();
                    computer1.daemons.Add(uploadServerDaemon);
                }
                else if (rdr.Name.Equals("MedicalDatabase"))
                {
                    var medicalDatabaseDaemon = new MedicalDatabaseDaemon(computer1, os);
                    computer1.daemons.Add(medicalDatabaseDaemon);
                }
                else if (rdr.Name.Equals("HeartMonitor"))
                {
                    var str3 = "UNKNOWN";
                    if (rdr.MoveToAttribute("patient"))
                        str3 = rdr.ReadContentAsString();
                    computer1.daemons.Add(new HeartMonitorDaemon(computer1, os)
                    {
                        PatientID = str3
                    });
                }
                else if (rdr.Name.Equals("PointClicker"))
                {
                    var pointClickerDaemon = new PointClickerDaemon(computer1, "Point Clicker!", os);
                    computer1.daemons.Add(pointClickerDaemon);
                }
                else if (rdr.Name.Equals("PorthackHeart"))
                {
                    var porthackHeartDaemon = new PorthackHeartDaemon(computer1, os);
                    computer1.daemons.Add(porthackHeartDaemon);
                }
                else if (rdr.Name.Equals("SongChangerDaemon"))
                {
                    var songChangerDaemon = new SongChangerDaemon(computer1, os);
                    computer1.daemons.Add(songChangerDaemon);
                }
                else if (rdr.Name.Equals("eosDevice"))
                    EOSComp.AddEOSComp(rdr, computer1, os);
                rdr.Read();
            }
            computer1.initDaemons();
            os.netMap.nodes.Add(computer1);
            return computer1;
        }

        public static void loadPortsIntoComputer(string portsList, object computer_obj)
        {
            var computer = (Computer) computer_obj;
            var separator = new char[2]
            {
                ' ',
                ','
            };
            var strArray = portsList.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            computer.ports.Clear();
            computer.portsOpen.Clear();
            for (var index = 0; index < strArray.Length; ++index)
            {
                try
                {
                    var num = Convert.ToInt32(strArray[index]);
                    if (PortExploits.portNums.Contains(num))
                    {
                        computer.ports.Add(num);
                        computer.portsOpen.Add(0);
                        continue;
                    }
                }
                catch (OverflowException ex)
                {
                }
                catch (FormatException ex)
                {
                }
                var num1 = -1;
                foreach (var keyValuePair in PortExploits.cracks)
                {
                    if (keyValuePair.Value.ToLower().Equals(strArray[index].ToLower()))
                        num1 = keyValuePair.Key;
                }
                if (num1 != -1)
                {
                    computer.ports.Add(num1);
                    computer.portsOpen.Add(0);
                }
            }
        }

        public static object readMission(string filename)
        {
            var xmlReader = XmlReader.Create(TitleContainer.OpenStream(filename));
            var _goals = new List<MisisonGoal>();
            var val1 = 0;
            var name1 = "";
            var name2 = "";
            var val2 = 0;
            var flag1 = false;
            var flag2 = false;
            while (xmlReader.Name != "mission")
                xmlReader.Read();
            if (xmlReader.MoveToAttribute("activeCheck"))
                flag1 = xmlReader.ReadContentAsBoolean();
            if (xmlReader.MoveToAttribute("shouldIgnoreSenderVerification"))
                flag2 = xmlReader.ReadContentAsBoolean();
            while (xmlReader.Name != "goals" && xmlReader.Name != "generationKeys")
                xmlReader.Read();
            if (xmlReader.Name == "generationKeys")
            {
                var keys = new Dictionary<string, string>();
                while (xmlReader.MoveToNextAttribute())
                {
                    var name3 = xmlReader.Name;
                    var str = xmlReader.Value;
                    keys.Add(name3, str);
                }
                var num = (int) xmlReader.MoveToContent();
                var str1 = xmlReader.ReadElementContentAsString();
                if (str1 != null & str1.Length >= 1)
                    keys.Add("Data", str1);
                MissionGenerator.setMissionGenerationKeys(keys);
                while (xmlReader.Name != "goals")
                    xmlReader.Read();
            }
            xmlReader.Read();
            if (MissionPreLoadComplete != null)
                MissionPreLoadComplete();
            while (xmlReader.Name != "goals")
            {
                if (xmlReader.Name.Equals("goal"))
                {
                    var str1 = "UNKNOWN";
                    if (xmlReader.MoveToAttribute("type"))
                        str1 = xmlReader.ReadContentAsString();
                    if (str1.ToLower().Equals("filedeletion"))
                    {
                        string str2;
                        var path = str2 = "";
                        var filename1 = str2;
                        var target = str2;
                        if (xmlReader.MoveToAttribute("target"))
                            target = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("file"))
                            filename1 = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("path"))
                            path = filter(xmlReader.ReadContentAsString());
                        var fileDeletionMission = new FileDeletionMission(path, filename1, findComp(target).ip, os);
                        _goals.Add(fileDeletionMission);
                    }
                    else if (str1.ToLower().Equals("filedownload"))
                    {
                        string str2;
                        var path = str2 = "";
                        var filename1 = str2;
                        var target = str2;
                        if (xmlReader.MoveToAttribute("target"))
                            target = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("file"))
                            filename1 = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("path"))
                            path = filter(xmlReader.ReadContentAsString());
                        var fileDownloadMission = new FileDownloadMission(path, filename1, findComp(target).ip, os);
                        _goals.Add(fileDownloadMission);
                    }
                    else if (str1.ToLower().Equals("filechange"))
                    {
                        string str2;
                        var targetKeyword = str2 = "";
                        var path = str2;
                        var filename1 = str2;
                        var target = str2;
                        if (xmlReader.MoveToAttribute("target"))
                            target = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("file"))
                            filename1 = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("path"))
                            path = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("keyword"))
                            targetKeyword = filter(xmlReader.ReadContentAsString());
                        var isRemoval = false;
                        if (xmlReader.MoveToAttribute("removal"))
                            isRemoval = xmlReader.ReadContentAsBoolean();
                        var fileChangeMission = new FileChangeMission(path, filename1, findComp(target).ip,
                            targetKeyword, os, isRemoval);
                        _goals.Add(fileChangeMission);
                    }
                    else if (str1.ToLower().Equals("getadmin"))
                    {
                        var target = "";
                        if (xmlReader.MoveToAttribute("target"))
                            target = filter(xmlReader.ReadContentAsString());
                        var getAdminMission = new GetAdminMission(findComp(target).ip, os);
                        _goals.Add(getAdminMission);
                    }
                    else if (str1.ToLower().Equals("getstring"))
                    {
                        var targetData = "";
                        if (xmlReader.MoveToAttribute("target"))
                            targetData = filter(xmlReader.ReadContentAsString());
                        var getStringMission = new GetStringMission(targetData);
                        _goals.Add(getStringMission);
                    }
                    else if (str1.ToLower().Equals("delay"))
                    {
                        var time = 1f;
                        if (xmlReader.MoveToAttribute("time"))
                            time = xmlReader.ReadContentAsFloat();
                        var delayMission = new DelayMission(time);
                        _goals.Add(delayMission);
                    }
                    else if (str1.ToLower().Equals("hasflag"))
                    {
                        var targetFlagName = "";
                        if (xmlReader.MoveToAttribute("target"))
                            targetFlagName = filter(xmlReader.ReadContentAsString());
                        var checkFlagSetMission = new CheckFlagSetMission(targetFlagName, os);
                        _goals.Add(checkFlagSetMission);
                    }
                    if (str1.ToLower().Equals("fileupload"))
                    {
                        var needsDecrypt = false;
                        string str2;
                        var decryptPass = str2 = "";
                        var destToUploadToPath = str2;
                        var computerToUploadToIP = str2;
                        var path = str2;
                        var filename1 = str2;
                        var computerWithFileIP = str2;
                        if (xmlReader.MoveToAttribute("target"))
                            computerWithFileIP = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("file"))
                            filename1 = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("path"))
                            path = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("destTarget"))
                            computerToUploadToIP = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("destPath"))
                            destToUploadToPath = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("decryptPass"))
                            decryptPass = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("decrypt"))
                            needsDecrypt = xmlReader.ReadContentAsBoolean();
                        var fileUploadMission = new FileUploadMission(path, filename1, computerWithFileIP,
                            computerToUploadToIP, destToUploadToPath, os, needsDecrypt, decryptPass);
                        _goals.Add(fileUploadMission);
                    }
                    if (str1.ToLower().Equals("adddegree"))
                    {
                        string str2;
                        var degreeName = str2 = "";
                        var uniName = str2;
                        var targetName = str2;
                        var desiredGPA = -1f;
                        if (xmlReader.MoveToAttribute("owner"))
                            targetName = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("degree"))
                            degreeName = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("uni"))
                            uniName = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("gpa"))
                            desiredGPA = xmlReader.ReadContentAsFloat();
                        var addDegreeMission = new AddDegreeMission(targetName, degreeName, uniName, desiredGPA, os);
                        _goals.Add(addDegreeMission);
                    }
                    if (str1.ToLower().Equals("wipedegrees"))
                    {
                        var targetName = "";
                        if (xmlReader.MoveToAttribute("owner"))
                            targetName = filter(xmlReader.ReadContentAsString());
                        var wipeDegreesMission = new WipeDegreesMission(targetName, os);
                        _goals.Add(wipeDegreesMission);
                    }
                    if (str1.ToLower().Equals("removeDeathRowRecord".ToLower()))
                    {
                        var firstName = "UNKNOWN";
                        var lastName = "UNKNOWN";
                        if (xmlReader.MoveToAttribute("name"))
                        {
                            var strArray = filter(xmlReader.ReadContentAsString()).Split(' ');
                            firstName = strArray[0];
                            lastName = strArray[1];
                        }
                        if (xmlReader.MoveToAttribute("fname"))
                            firstName = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("lname"))
                            lastName = filter(xmlReader.ReadContentAsString());
                        var recordRemovalMission = new DeathRowRecordRemovalMission(firstName, lastName, os);
                        _goals.Add(recordRemovalMission);
                    }
                    if (str1.ToLower().Equals("modifyDeathRowRecord".ToLower()))
                    {
                        var firstName = "UNKNOWN";
                        var lastName = "UNKNOWN";
                        if (xmlReader.MoveToAttribute("name"))
                        {
                            var strArray = filter(xmlReader.ReadContentAsString()).Split(' ');
                            firstName = strArray[0];
                            lastName = strArray[1];
                        }
                        if (xmlReader.MoveToAttribute("fname"))
                            firstName = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("lname"))
                            lastName = filter(xmlReader.ReadContentAsString());
                        var num = (int) xmlReader.MoveToContent();
                        var lastWords = xmlReader.ReadElementContentAsString();
                        var recordModifyMission = new DeathRowRecordModifyMission(firstName, lastName, lastWords, os);
                        _goals.Add(recordModifyMission);
                    }
                    else if (str1.ToLower().Equals("sendemail"))
                    {
                        string proposedEmailSubject;
                        var mailRecipient = proposedEmailSubject = "";
                        var mailServerID = "jmail";
                        if (xmlReader.MoveToAttribute("mailServer"))
                            mailServerID = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("recipient"))
                            mailRecipient = filter(xmlReader.ReadContentAsString());
                        if (xmlReader.MoveToAttribute("subject"))
                            proposedEmailSubject = filter(xmlReader.ReadContentAsString());
                        var sendEmailMission = new SendEmailMission(mailServerID, mailRecipient, proposedEmailSubject,
                            os);
                        _goals.Add(sendEmailMission);
                    }
                }
                xmlReader.Read();
            }
            while (xmlReader.Name != "nextMission" && xmlReader.Name != "missionEnd" && xmlReader.Name != "missionStart")
                xmlReader.Read();
            if (xmlReader.Name.Equals("missionStart"))
            {
                var num1 = 1;
                var flag3 = false;
                if (xmlReader.MoveToAttribute("val"))
                    num1 = xmlReader.ReadContentAsInt();
                if (xmlReader.MoveToAttribute("suppress"))
                    flag3 = xmlReader.ReadContentAsBoolean();
                var num2 = (int) xmlReader.MoveToContent();
                var name3 = xmlReader.ReadElementContentAsString();
                if (flag3)
                {
                    name2 = name3;
                    val2 = num1;
                }
                else
                    MissionFunctions.runCommand(num1, name3);
                xmlReader.Read();
            }
            if (xmlReader.Name.Equals("missionEnd"))
            {
                var num1 = 1;
                if (xmlReader.MoveToAttribute("val"))
                    num1 = xmlReader.ReadContentAsInt();
                var num2 = (int) xmlReader.MoveToContent();
                name1 = xmlReader.ReadElementContentAsString();
                val1 = num1;
            }
            var flag4 = true;
            while (!xmlReader.Name.Equals("nextMission"))
                xmlReader.Read();
            if (xmlReader.MoveToAttribute("IsSilent"))
                flag4 = xmlReader.ReadContentAsString().ToLower().Equals("false");
            var num3 = (int) xmlReader.MoveToContent();
            var next = xmlReader.ReadElementContentAsString();
            if (os.branchMissions != null)
                os.branchMissions.Clear();
            while (xmlReader.Name != "posting" && xmlReader.Name != "email")
            {
                if (xmlReader.Name.Equals("branchMissions"))
                {
                    xmlReader.Read();
                    var list = new List<ActiveMission>();
                    while (!xmlReader.Name.Equals("branchMissions") || xmlReader.IsStartElement())
                    {
                        if (xmlReader.Name == "branch")
                        {
                            var num1 = (int) xmlReader.MoveToContent();
                            var str = xmlReader.ReadElementContentAsString();
                            list.Add((ActiveMission) readMission("Content/Missions/" + str));
                        }
                        xmlReader.Read();
                    }
                    os.branchMissions = list;
                }
                xmlReader.Read();
            }
            int num4;
            var num5 = num4 = 0;
            string str3;
            var str4 = str3 = "UNKNOWN";
            var str5 = str3;
            var s1 = str3;
            var str6 = str3;
            string str7 = null;
            while (xmlReader.Name != "posting" && xmlReader.Name != "email")
                xmlReader.Read();
            if (xmlReader.Name.Equals("posting"))
            {
                if (xmlReader.MoveToAttribute("title"))
                    s1 = xmlReader.ReadContentAsString();
                if (xmlReader.MoveToAttribute("reqs"))
                    str7 = xmlReader.ReadContentAsString();
                if (xmlReader.MoveToAttribute("requiredRank"))
                    num5 = xmlReader.ReadContentAsInt();
                if (xmlReader.MoveToAttribute("difficulty"))
                    num4 = xmlReader.ReadContentAsInt();
                if (xmlReader.MoveToAttribute("client"))
                    str5 = xmlReader.ReadContentAsString();
                if (xmlReader.MoveToAttribute("target"))
                    str4 = xmlReader.ReadContentAsString();
                var num1 = (int) xmlReader.MoveToContent();
                var s2 = xmlReader.ReadElementContentAsString();
                s1 = filter(s1);
                str6 = filter(s2);
            }
            while (xmlReader.Name != "email")
                xmlReader.Read();
            while (xmlReader.Name != "sender")
                xmlReader.Read();
            var s3 = xmlReader.ReadElementContentAsString();
            while (xmlReader.Name != "subject")
                xmlReader.Read();
            var s4 = xmlReader.ReadElementContentAsString();
            while (xmlReader.Name != "body")
                xmlReader.Read();
            var s5 = xmlReader.ReadElementContentAsString();
            s5.Trim();
            var bod = filter(s5);
            var subj = filter(s4);
            var sendr = filter(s3);
            while (xmlReader.Name != "attachments")
                xmlReader.Read();
            xmlReader.Read();
            var _attachments = new List<string>();
            while (xmlReader.Name != "attachments")
            {
                if (xmlReader.Name.Equals("link"))
                {
                    var str1 = "";
                    if (xmlReader.MoveToAttribute("comp"))
                        str1 = filter(xmlReader.ReadContentAsString());
                    Computer computer = null;
                    for (var index = 0; index < os.netMap.nodes.Count; ++index)
                    {
                        if (os.netMap.nodes[index].idName.Equals(str1))
                            computer = os.netMap.nodes[index];
                    }
                    if (computer != null)
                        _attachments.Add("link#%#" + computer.name + "#%#" + computer.ip);
                }
                if (xmlReader.Name.Equals("account"))
                {
                    var str1 = "";
                    if (xmlReader.MoveToAttribute("comp"))
                        str1 = filter(xmlReader.ReadContentAsString());
                    Computer computer = null;
                    for (var index = 0; index < os.netMap.nodes.Count; ++index)
                    {
                        if (os.netMap.nodes[index].idName.Equals(str1))
                            computer = os.netMap.nodes[index];
                    }
                    string s2;
                    var s6 = s2 = "UNKNOWN";
                    if (xmlReader.MoveToAttribute("user"))
                        s6 = xmlReader.ReadContentAsString();
                    if (xmlReader.MoveToAttribute("pass"))
                        s2 = xmlReader.ReadContentAsString();
                    var str2 = filter(s6);
                    var str8 = filter(s2);
                    if (computer != null)
                        _attachments.Add("account#%#" + computer.name + "#%#" + computer.ip + "#%#" + str2 + "#%#" +
                                         str8);
                }
                if (xmlReader.Name.Equals("note"))
                {
                    var str1 = "Data";
                    if (xmlReader.MoveToAttribute("title"))
                        str1 = filter(xmlReader.ReadContentAsString());
                    var num1 = (int) xmlReader.MoveToContent();
                    var str2 = xmlReader.ReadElementContentAsString();
                    _attachments.Add("note#%#" + str1 + "#%#" + str2);
                }
                xmlReader.Read();
            }
            var _email = new MailServer.EMailData(sendr, bod, subj, _attachments);
            var activeMission = new ActiveMission(_goals, next, _email);
            activeMission.activeCheck = flag1;
            activeMission.ShouldIgnoreSenderVerification = flag2;
            activeMission.postingBody = str6;
            activeMission.postingTitle = s1;
            activeMission.requiredRank = num5;
            activeMission.difficulty = num4;
            activeMission.client = str5;
            activeMission.target = str4;
            activeMission.reloadGoalsSourceFile = filename;
            if (str7 != null)
                activeMission.postingAcceptFlagRequirements = str7.Split(Utils.commaDelim,
                    StringSplitOptions.RemoveEmptyEntries);
            activeMission.willSendEmail = flag4;
            if (!name1.Equals(""))
                activeMission.addEndFunction(val1, name1);
            if (!name2.Equals(""))
                activeMission.addStartFunction(val2, name2);
            return activeMission;
        }

        public static void loadMission(string filename)
        {
            var activeMission = (ActiveMission) readMission(filename);
            os.currentMission = activeMission;
            activeMission.sendEmail(os);
        }

        public static string filter(string s)
        {
            return
                MissionGenerationParser.parse(
                    s.Replace("#BINARY#", Computer.generateBinaryString(2000))
                        .Replace("#BINARYSMALL#", Computer.generateBinaryString(800))
                        .Replace("#PLAYERNAME#", os.defaultUser.name)
                        .Replace("#PLAYER_IP#", os.thisComputer.ip)
                        .Replace("#SSH_CRACK#", PortExploits.crackExeData[22])
                        .Replace("#FTP_CRACK#", PortExploits.crackExeData[21])
                        .Replace("#WEB_CRACK#", PortExploits.crackExeData[80])
                        .Replace("#DECYPHER_PROGRAM#", PortExploits.crackExeData[9])
                        .Replace("#DECHEAD_PROGRAM#", PortExploits.crackExeData[10])
                        .Replace("#CLOCK_PROGRAM#", PortExploits.crackExeData[11])
                        .Replace("#MEDICAL_PROGRAM#", PortExploits.crackExeData[104])
                        .Replace("#SMTP_CRACK#", PortExploits.crackExeData[25])
                        .Replace("#SQL_CRACK#", PortExploits.crackExeData[1433])
                        .Replace("#SECURITYTRACER_PROGRAM#", PortExploits.crackExeData[4])
                        .Replace("#HACKNET_EXE#", PortExploits.crackExeData[15])
                        .Replace("#HEXCLOCK_EXE#", PortExploits.crackExeData[16])
                        .Replace("#SEQUENCER_EXE#", PortExploits.crackExeData[17])
                        .Replace("#GREEN_THEME#", ThemeManager.getThemeDataString(OSTheme.HackerGreen))
                        .Replace("#WHITE_THEME#", ThemeManager.getThemeDataString(OSTheme.HacknetWhite))
                        .Replace("#YELLOW_THEME#", ThemeManager.getThemeDataString(OSTheme.HacknetYellow))
                        .Replace("#TEAL_THEME#", ThemeManager.getThemeDataString(OSTheme.HacknetTeal))
                        .Replace("#BASE_THEME#", ThemeManager.getThemeDataString(OSTheme.HacknetBlue))
                        .Replace("#PURPLE_THEME#", ThemeManager.getThemeDataString(OSTheme.HacknetPurple))
                        .Replace("#MINT_THEME#", ThemeManager.getThemeDataString(OSTheme.HacknetMint))
                        .Replace("#PACEMAKER_FW_WORKING#", PortExploits.ValidPacemakerFirmware)
                        .Replace("#PACEMAKER_FW_DANGER#", PortExploits.DangerousPacemakerFirmware)
                        .Replace("\t", "    "));
        }

        private static Computer findComp(string target)
        {
            for (var index = 0; index < os.netMap.nodes.Count; ++index)
            {
                if (os.netMap.nodes[index].idName.Equals(target))
                    return os.netMap.nodes[index];
            }
            return null;
        }

        public static object findComputer(string target)
        {
            return findComp(target);
        }
    }
}
using System;
using System.Xml;

namespace Hacknet
{
    internal class EOSComp
    {
        public static void AddEOSComp(XmlReader rdr, Computer compAttatchedTo, object osObj)
        {
            var os = (OS) osObj;
            var compName = "Unregistered eOS Device";
            var str1 = compAttatchedTo.idName + "_eos";
            if (rdr.MoveToAttribute("name"))
                compName = rdr.ReadContentAsString();
            if (rdr.MoveToAttribute("id"))
                str1 = rdr.ReadContentAsString();
            var device = new Computer(compName, NetworkMap.generateRandomIP(), os.netMap.getRandomPosition(), 0, 5, os);
            device.idName = str1;
            var str2 = "ePhone";
            if (rdr.MoveToAttribute("icon"))
                str2 = rdr.ReadContentAsString();
            device.icon = str2;
            device.location = compAttatchedTo.location +
                              Corporation.getNearbyNodeOffset(compAttatchedTo.location, Utils.random.Next(12), 12,
                                  os.netMap);
            device.setAdminPassword("alpine");
            ComputerLoader.loadPortsIntoComputer("22,3659", device);
            device.portsNeededForCrack = 2;
            GenerateEOSFilesystem(device);
            rdr.Read();
            var folder1 = device.files.root.searchForFolder("eos");
            var folder2 = folder1.searchForFolder("notes");
            var folder3 = folder1.searchForFolder("mail");
            while (!(rdr.Name == "eosDevice") || rdr.IsStartElement())
            {
                if (rdr.Name.ToLower() == "note" && rdr.IsStartElement())
                {
                    string nameEntry = null;
                    if (rdr.MoveToAttribute("filename"))
                        nameEntry = rdr.ReadContentAsString();
                    var num = (int) rdr.MoveToContent();
                    var dataEntry = rdr.ReadElementContentAsString().TrimStart();
                    if (nameEntry == null)
                    {
                        var length = dataEntry.IndexOf("\n");
                        if (length == -1)
                            length = dataEntry.IndexOf("\n");
                        if (length == -1)
                            length = dataEntry.Length;
                        nameEntry = dataEntry.Substring(0, length).Replace(" ", "_").ToLower().Trim() + ".txt";
                    }
                    var fileEntry = new FileEntry(dataEntry, nameEntry);
                    folder2.files.Add(fileEntry);
                }
                if (rdr.Name.ToLower() == "mail" && rdr.IsStartElement())
                {
                    string str3 = null;
                    string str4 = null;
                    if (rdr.MoveToAttribute("username"))
                        str3 = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("pass"))
                        str4 = rdr.ReadContentAsString();
                    var dataEntry = "MAIL ACCOUNT : " + str3 + "\nAccount   :" + str3 + "\nPassword :" + str4 +
                                    "\nLast Sync :" + DateTime.Now + "\n\n" + Computer.generateBinaryString(512);
                    var nameEntry = str3 + ".act";
                    folder3.files.Add(new FileEntry(dataEntry, nameEntry));
                }
                rdr.Read();
                if (rdr.EOF)
                    break;
            }
            os.netMap.nodes.Add(device);
            ComputerLoader.postAllLoadedActions += () => device.links.Add(os.netMap.nodes.IndexOf(compAttatchedTo));
            if (compAttatchedTo.attatchedDeviceIDs != null)
                compAttatchedTo.attatchedDeviceIDs += ",";
            compAttatchedTo.attatchedDeviceIDs += device.idName;
        }

        public static Folder GenerateEOSFolder()
        {
            var folder1 = new Folder("eos");
            var folder2 = new Folder("apps");
            var folder3 = new Folder("system");
            var folder4 = new Folder("notes");
            var folder5 = new Folder("mail");
            folder1.folders.Add(folder2);
            folder1.folders.Add(folder4);
            folder1.folders.Add(folder5);
            folder1.folders.Add(folder3);
            folder3.files.Add(new FileEntry(Computer.generateBinaryString(1024), "core.sys"));
            folder3.files.Add(new FileEntry(Computer.generateBinaryString(1024), "runtime.bin"));
            var num = 4 + Utils.random.Next(8);
            for (var index = 0; index < num; ++index)
                folder2.folders.Add(EOSAppGenerator.GetAppFolder());
            return folder1;
        }

        public static void GenerateEOSFilesystem(Computer device)
        {
            if (device.files.root.searchForFolder("eos") != null)
                return;
            device.files.root.folders.Insert(0, GenerateEOSFolder());
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class Multiplayer
    {
        private static int generatedComputerCount;
        public static int PORT = 3020;

        public static char[] delims = new char[2]
        {
            ' ',
            '\n'
        };

        public static char[] specSplitDelims = new char[1]
        {
            '#'
        };

        public static void parseInputMessage(string message, OS os)
        {
            if (message.Equals(""))
                return;
            var strArray1 = message.Split(delims);
            if (strArray1.Length == 0)
                return;
            if (os.thisComputer != null && os.thisComputer.ip.Equals(strArray1[1]))
                os.warningFlash();
            if (strArray1[0].Equals("init"))
            {
                var Seed = Convert.ToInt32(strArray1[1]);
                Utils.random = new Random(Seed);
                os.canRunContent = true;
                os.LoadContent();
                os.write("Seed Established :" + Seed);
            }
            else if (strArray1[0].Equals("chat"))
            {
                var s = "";
                for (var index = 2; index < strArray1.Length; ++index)
                    s = s + strArray1[index] + " ";
                os.write(strArray1[1] + ": " + DisplayModule.splitForWidth(s, 350));
            }
            else if (strArray1[0].Equals("clientConnect"))
                os.write("Connection Established");
            else if (strArray1[0].Equals("cConnection"))
            {
                var comp = getComp(strArray1[1], os);
                if (comp == null)
                {
                    os.write("Error in Message : " + message);
                }
                else
                {
                    comp.silent = true;
                    comp.connect(strArray1[2]);
                    comp.silent = false;
                    os.opponentLocation = strArray1[1];
                }
            }
            else if (strArray1[0].Equals("cDisconnect"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                comp.disconnecting(strArray1[2]);
                comp.silent = false;
                os.opponentLocation = "";
            }
            else if (strArray1[0].Equals("cAdmin"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                comp.giveAdmin(strArray1[2]);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cPortOpen"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                comp.openPort(Convert.ToInt32(strArray1[3]), strArray1[2]);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cPortClose"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                comp.closePort(Convert.ToInt32(strArray1[3]), strArray1[2]);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cFile"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                var f = new FileEntry("", strArray1[3]);
                comp.canReadFile(strArray1[2], f, Convert.ToInt32(strArray1[4]));
                comp.silent = false;
            }
            else if (strArray1[0].Equals("newComp"))
            {
                var strArray2 = message.Split(specSplitDelims);
                var compLocation = new Vector2(Convert.ToInt32(strArray2[2]), Convert.ToInt32(strArray2[3]));
                var computer = new Computer(strArray2[5], strArray2[1], compLocation, Convert.ToInt32(strArray2[4]), 1,
                    os);
                computer.idName = "opponent#" + generatedComputerCount;
                ++generatedComputerCount;
                computer.addMultiplayerTargetFile();
                os.netMap.nodes.Add(computer);
                os.opponentComputer = computer;
            }
            else if (strArray1[0].Equals("cDelete"))
            {
                var strArray2 = message.Split(specSplitDelims);
                var comp = getComp(strArray2[1], os);
                var folderPath = new List<int>();
                for (var index = 4; index < strArray2.Length; ++index)
                {
                    if (strArray2[index] != "")
                        folderPath.Add(Convert.ToInt32(strArray2[index]));
                }
                comp.silent = true;
                comp.deleteFile(strArray2[2], strArray2[3], folderPath);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cMake"))
            {
                var strArray2 = message.Split(specSplitDelims);
                var comp = getComp(strArray2[1], os);
                var folderPath = new List<int>();
                for (var index = 4; index < strArray2.Length; ++index)
                {
                    if (strArray2[index] != "")
                        folderPath.Add(Convert.ToInt32(strArray2[index]));
                }
                comp.silent = true;
                comp.makeFile(strArray2[2], strArray2[3], strArray2[4], folderPath, false);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cMove"))
            {
                var strArray2 = message.Split(specSplitDelims);
                var comp = getComp(strArray2[1], os);
                var separator = new char[1]
                {
                    '%'
                };
                var folderPath = new List<int>();
                var strArray3 = strArray2[5].Split(separator, 500, StringSplitOptions.RemoveEmptyEntries);
                for (var index = 0; index < strArray3.Length; ++index)
                {
                    if (strArray2[index] != "")
                        folderPath.Add(Convert.ToInt32(strArray2[index]));
                }
                var destFolderPath = new List<int>();
                var strArray4 = strArray2[6].Split(separator, 500, StringSplitOptions.RemoveEmptyEntries);
                for (var index = 0; index < strArray4.Length; ++index)
                {
                    if (strArray2[index] != "")
                        destFolderPath.Add(Convert.ToInt32(strArray2[index]));
                }
                comp.silent = true;
                comp.moveFile(strArray2[2], strArray2[3], strArray2[4], folderPath, destFolderPath);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cMkDir"))
            {
                var strArray2 = message.Split(specSplitDelims);
                var comp = getComp(strArray2[1], os);
                var folderPath = new List<int>();
                for (var index = 4; index < strArray2.Length; ++index)
                {
                    if (strArray2[index] != "")
                        folderPath.Add(Convert.ToInt32(strArray2[index]));
                }
                comp.silent = true;
                comp.makeFolder(strArray2[2], strArray2[3], folderPath);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cAddUser"))
            {
                var strArray2 = message.Split(specSplitDelims);
                var comp = getComp(strArray2[1], os);
                var name = strArray2[3];
                var pass = strArray2[4];
                var type = Convert.ToByte(strArray2[5]);
                comp.silent = true;
                comp.addNewUser(strArray2[2], name, pass, type);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cCopy"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                comp.canCopyFile(strArray1[2], strArray1[3]);
                comp.silent = false;
                FileEntry fileEntry1 = null;
                for (var index = 0; index < comp.files.root.folders[2].files.Count; ++index)
                {
                    if (comp.files.root.folders[2].files[index].name.Equals(strArray1[3]))
                        fileEntry1 = comp.files.root.folders[2].files[index];
                }
                var fileEntry2 = new FileEntry(fileEntry1.data, fileEntry1.name);
                getComp(strArray1[2], os).files.root.folders[2].files.Add(fileEntry2);
            }
            else if (strArray1[0].Equals("cCDDrive"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                if (strArray1[2].Equals("open"))
                    comp.openCDTray(strArray1[1]);
                else
                    comp.closeCDTray(strArray1[1]);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cCrash"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                comp.crash(strArray1[2]);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cReboot"))
            {
                var comp = getComp(strArray1[1], os);
                comp.silent = true;
                comp.reboot(strArray1[2]);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("cFBClients"))
            {
                var comp = getComp(strArray1[1], os);
                if (os.connectedComp != null && os.connectedComp.ip.Equals(strArray1[1]))
                    os.exes.Add(new ForkBombExe(os.getExeBounds(), os));
                comp.silent = true;
                comp.forkBombClients(strArray1[2]);
                comp.silent = false;
            }
            else if (strArray1[0].Equals("eForkBomb"))
            {
                if (!os.thisComputer.ip.Equals(strArray1[1]))
                    return;
                var forkBombExe = new ForkBombExe(os.getExeBounds(), os);
                forkBombExe.LoadContent();
                os.exes.Add(forkBombExe);
            }
            else if (strArray1[0].Equals("mpOpponentWin"))
            {
                os.endMultiplayerMatch(false);
            }
            else
            {
                if (strArray1[0].Equals("stayAlive"))
                    return;
                os.write("MSG: " + message);
            }
        }

        private static Computer getComp(string ip, OS os)
        {
            for (var index = 0; index < os.netMap.nodes.Count; ++index)
            {
                if (os.netMap.nodes[index].ip.Equals(ip))
                    return os.netMap.nodes[index];
            }
            return null;
        }
    }
}
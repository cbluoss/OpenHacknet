using System.Collections.Generic;

namespace Hacknet.Factions
{
    internal class HubFaction : Faction
    {
        public HubFaction(string _name, int _neededValue)
            : base(_name, _neededValue)
        {
            PlayerLosesValueOnAbandon = false;
        }

        public override void addValue(int value, object os)
        {
            var oldValue = playerValue;
            base.addValue(value, os);
            if (valuePassedPoint(oldValue, 1) && !((OS) os).Flags.HasFlag("themeChangerAdded"))
            {
                var folder1 = Programs.getComputer((OS) os, "mainHubAssets").files.root.searchForFolder("bin");
                var folder2 = new Folder("ThemeChanger");
                folder2.files.Add(new FileEntry(PortExploits.crackExeData[14], "ThemeChanger.exe"));
                var dataEntry =
                    "\n-- Theme Changer Readme --\n\nThis program allows for fast hot-swapping of x-server theme files from local and remote conneced sources.\nFiles in the \"Remote\" row are remotely hosted valid theme files in the currently nagivated folder on a connected machine.\nFiles in the \"Local\" row are locally hosted theme files in the home or sys folder.\nThemeChanger allows a user to select any of these, and will automatically download it's contents into x-server.sys\nin the system folder, and activate the system theme without the need for a reboot.\nThe program will also automatically back up existing themes so that all known styles are preserved for future use.";
                folder2.files.Add(new FileEntry(dataEntry, "info.txt"));
                folder1.folders.Add(folder2);
                ((OS) os).delayer.Post(ActionDelayer.Wait(1.0), () =>
                {
                    SendAssetAddedNotification(os);
                    ((OS) os).Flags.AddFlag("themeChangerAdded");
                    ((OS) os).saveGame();
                });
            }
            if (valuePassedPoint(oldValue, 4))
                ((OS) os).delayer.Post(ActionDelayer.Wait(2.0), () =>
                {
                    ((MissionHubServer) Programs.getComputer((OS) os, "mainHub").getDaemon(typeof (MissionHubServer)))
                        .AddMissionToListings("Content/Missions/MainHub/BitSet/Missions/BitHubSet01.xml");
                    ((OS) os).saveGame();
                });
            else if (playerValue >= 7 && ((OS) os).Flags.HasFlag("decypher") &&
                     (((OS) os).Flags.HasFlag("dechead") && !((OS) os).Flags.HasFlag("csecRankingS2Pass")))
            {
                SendNotification(os, "Project Junebug");
                ((OS) os).Flags.AddFlag("csecRankingS2Pass");
                ((OS) os).saveGame();
            }
            else
            {
                if (playerValue < 10 || ((OS) os).Flags.HasFlag("bitPathStarted"))
                    return;
                ((OS) os).Flags.AddFlag("bitPathStarted");
                ((OS) os).delayer.Post(ActionDelayer.Wait(1.6),
                    () => ComputerLoader.loadMission("Content/Missions/BitPath/BitAdv_Intro.xml"));
                Programs.getComputer((OS) os, "mainHubAssets")
                    .files.root.searchForFolder("bin")
                    .folders.Add(new Folder("Misc")
                    {
                        files =
                        {
                            new FileEntry(PortExploits.crackExeData[9], "Decypher.exe"),
                            new FileEntry(PortExploits.crackExeData[10], "DECHead.exe"),
                            new FileEntry(PortExploits.crackExeData[104], "KBT_PortTest.exe"),
                            new FileEntry("Kellis BioTech medical port cycler - target 104-103.", "kbt_readme.txt")
                        }
                    });
                SendNotification(os,
                    "Agent,\nAdditional resources have been added to the CSEC members asset pool, for your free use. Find them in the misc folder on the asset server.\n\nThankyou,\n -" +
                    name, name + " Admins :: Asset Uploads");
            }
        }

        private void SendNotification(object osIn, string body, string subject)
        {
            var os = (OS) osIn;
            var sender = name + " ReplyBot";
            var mail = MailServer.generateEmail(subject, body, sender);
            ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(mail, os.defaultUser.name);
        }

        private void SendNotification(object osIn, string contractName)
        {
            var os = (OS) osIn;
            var mail = MailServer.generateEmail(name + " Admins :: Flagged for Critical Contract",
                "Agent,\nAdministrators have flagged your account with permissions to accept a critical contract.\n" +
                "This is listed as \"" + contractName + "\" on the listings page.\n" +
                "\nBe aware the accepting this contract may be either time consuming, or require confidentiality and a seperation from other projects for a time\n" +
                "and should be undertaken only with the expectation that other contracts, and possibly " + name +
                " services may be unavaliable for it's duration.\n" + "\nThankyou,\n -" + name, name + " ReplyBot");
            ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(mail, os.defaultUser.name);
        }

        private void SendAssetAddedNotification(object osIn)
        {
            var os = (OS) osIn;
            var subject = name + " Admins :: New asset added";
            var body =
                "Agent,\nA new tool has been added to the asset server for your use.\nIt's an x-server changing utility named \"ThemeChanger.exe\"- you can find it under:\n\n/bin/ThemeChanger/\n\nFeel free to download a copy and use it however it's useful to you.\n" +
                "\nThankyou,\n -" + name;
            var sender = name + " ReplyBot";
            var computer = Programs.getComputer(os, "mainHubAssets");
            var str = "link#%#" + computer.name + "#%#" + computer.ip;
            var mail = MailServer.generateEmail(subject, body, sender, new List<string>(new string[1]
            {
                str
            }));
            ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(mail, os.defaultUser.name);
        }
    }
}
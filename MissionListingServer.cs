// Decompiled with JetBrains decompiler
// Type: Hacknet.MissionListingServer
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.IO;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MissionListingServer : AuthenticatingDaemon
    {
        private const int NEED_LOGIN = 0;
        private const int BOARD = 1;
        private const int MESSAGE = 2;
        private const int LOGIN = 3;
        private readonly List<List<ActiveMission>> branchMissions = new List<List<ActiveMission>>();
        private Folder closedMissionsFolder;
        private readonly Texture2D corner;
        private readonly string groupName;
        public bool isPublic;
        private readonly string listingTitle;
        private readonly Texture2D logo;
        private Rectangle logoRect;
        public bool missionAssigner;
        private Folder missionFolder;
        private List<int> missionFolderPath;
        private readonly List<ActiveMission> missions;
        private Rectangle panelRect;
        private Folder root;
        private List<int> rootPath;
        private int state;
        private FileEntry sysFile;
        private int targetIndex;
        private readonly Color themeColor;
        private readonly Texture2D topBar;

        public MissionListingServer(Computer c, string serviceName, string group, OS _os, bool _isPublic = false,
            bool _isAssigner = false)
            : base(c, serviceName, _os)
        {
            groupName = group;
            topBar = os.content.Load<Texture2D>("Panel");
            corner = os.content.Load<Texture2D>("Corner");
            if (group.Equals("Entropy"))
            {
                themeColor = new Color(3, 102, 49);
                logo = os.content.Load<Texture2D>("EntropyLogo");
            }
            else if (group.Equals("NetEdu"))
            {
                themeColor = new Color(119, 104, 160);
                logo = os.content.Load<Texture2D>("Sprites/Academic_logo");
            }
            else if (group.Equals("Kellis Biotech"))
            {
                themeColor = new Color(106, 176, byte.MaxValue);
                logo = os.content.Load<Texture2D>("Sprites/KellisLogo");
            }
            else
            {
                themeColor = new Color(204, 163, 27);
                logo = os.content.Load<Texture2D>("SlashbotLogo");
            }
            logoRect = new Rectangle(0, 0, 64, 64);
            isPublic = _isPublic;
            missionAssigner = _isAssigner;
            state = !isPublic ? 0 : 1;
            missions = new List<ActiveMission>();
            branchMissions = new List<List<ActiveMission>>();
            if (isPublic)
                listingTitle = group + " News";
            else
                listingTitle = "Available Contracts";
        }

        public override void initFiles()
        {
            base.initFiles();
            initFilesystem();
            addListingsForGroup();
        }

        public override void loadInit()
        {
            base.loadInit();
            root = comp.files.root.searchForFolder("MsgBoard");
            missionFolder = root.searchForFolder("listings");
            closedMissionsFolder = root.searchForFolder("closed");
            if (closedMissionsFolder == null)
            {
                closedMissionsFolder = new Folder("closed");
                root.folders.Add(closedMissionsFolder);
            }
            if (!missionAssigner)
            {
                for (var index = 0; index < missionFolder.files.Count; ++index)
                {
                    try
                    {
                        var str1 = missionFolder.files[index].name.Replace("_", " ");
                        var str2 = missionFolder.files[index].data;
                        missions.Add(new ActiveMission(null, null, new MailServer.EMailData())
                        {
                            postingTitle = str1,
                            postingBody = str2
                        });
                    }
                    catch (Exception ex)
                    {
                        var num = 0 + 1;
                    }
                }
            }
            else
            {
                var list1 = os.branchMissions;
                for (var index = 0; index < missionFolder.files.Count; ++index)
                {
                    try
                    {
                        var data = missionFolder.files[index].data;
                        var contractRegistryNumber = 0;
                        os.branchMissions.Clear();
                        var activeMission =
                            (ActiveMission) MissionSerializer.restoreMissionFromFile(data, out contractRegistryNumber);
                        var list2 = new List<ActiveMission>();
                        list2.AddRange(os.branchMissions.ToArray());
                        branchMissions.Add(list2);
                        missions.Add(activeMission);
                    }
                    catch (Exception ex)
                    {
                        var num = 0 + 1;
                    }
                }
                os.branchMissions = list1;
            }
        }

        public override string getSaveString()
        {
            return "<MissionListingServer name=\"" + (object) name + "\" group=\"" + groupName + "\" public=\"" +
                   isPublic + "\" assign=\"" + missionAssigner + "\"/>";
        }

        public void addMisison(ActiveMission m)
        {
            var dataEntry = m.postingBody;
            if (missionAssigner)
                dataEntry = MissionSerializer.generateMissionFile(m, 0, groupName);
            missionFolder.files.Add(new FileEntry(dataEntry, m.postingTitle));
            missions.Add(m);
            branchMissions.Add(os.branchMissions);
        }

        public void removeMission(int index)
        {
            var flag = false;
            for (var index1 = 0; index1 < missionFolder.files.Count; ++index1)
            {
                if (missionFolder.files[index1].name.Equals(missions[index].postingTitle.Replace(" ", "_")))
                {
                    var fileEntry = missionFolder.files[index1];
                    missionFolder.files.RemoveAt(index1);
                    closedMissionsFolder.files.Add(fileEntry);
                    flag = true;
                    break;
                }
            }
            var num = flag ? 1 : 0;
            missions.RemoveAt(index);
        }

        public void initFilesystem()
        {
            rootPath = new List<int>();
            missionFolderPath = new List<int>();
            root = new Folder("MsgBoard");
            rootPath.Add(comp.files.root.folders.IndexOf(root));
            missionFolderPath.Add(comp.files.root.folders.IndexOf(root));
            missionFolder = new Folder("listings");
            missionFolderPath.Add(0);
            root.folders.Add(missionFolder);
            closedMissionsFolder = new Folder("closed");
            root.folders.Add(closedMissionsFolder);
            sysFile = new FileEntry(Computer.generateBinaryString(1024), "config.sys");
            root.files.Add(sysFile);
            root.files.Add(
                new FileEntry(
                    "  ----- WARNING -----  \n\nconfig.sys in this folder is a critical system file.\nDO NOT DELETE OR RENAME IT\nDoing so will crash the board and bring the host program down\nChanges to the config should be made only during scheduled outages to avoid this issue\n",
                    "Config_CAUTION.txt"));
            comp.files.root.folders.Add(root);
        }

        public void addListingsForGroup()
        {
            var list = os.branchMissions;
            os.branchMissions = null;
            if (groupName.ToLower().Equals("entropy"))
            {
                foreach (
                    FileSystemInfo fileSystemInfo in
                        new DirectoryInfo("Content/Missions/Entropy/StartingSet/").GetFiles("*.xml"))
                {
                    var filename = "Content/Missions/Entropy/StartingSet/" + fileSystemInfo.Name;
                    os.branchMissions = new List<ActiveMission>();
                    addMisison((ActiveMission) ComputerLoader.readMission(filename));
                }
                for (var index = 0; index < 2; ++index)
                {
                    os.branchMissions = new List<ActiveMission>();
                    addMisison((ActiveMission) MissionGenerator.generate(2));
                }
            }
            else if (groupName.ToLower().Equals("netedu"))
            {
                addMisison(
                    (ActiveMission)
                        ComputerLoader.readMission("Content/Missions/Misc/EducationArticles/Education4.0.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Misc/EducationArticles/Education2.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Misc/EducationArticles/Education3.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Misc/EducationArticles/Education1.xml"));
                addMisison(
                    (ActiveMission)
                        ComputerLoader.readMission("Content/Missions/Misc/EducationArticles/Education4.1.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Misc/EducationArticles/Education5.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Misc/EducationArticles/Education6.xml"));
            }
            else if (groupName.ToLower().Equals("slashbot"))
            {
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Entropy/NewsArticles/EntropyNews1.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Entropy/NewsArticles/EntropyNews3.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Entropy/NewsArticles/EntropyNews2.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Entropy/NewsArticles/EntropyNews4.xml"));
                addMisison(
                    (ActiveMission) ComputerLoader.readMission("Content/Missions/Entropy/NewsArticles/EntropyNews5.xml"));
            }
            else if (groupName.ToLower().Equals("kellis biotech"))
            {
                addMisison(
                    (ActiveMission)
                        ComputerLoader.readMission(
                            "Content/Missions/MainHub/PacemakerSet/HardwareArticles/Hardware1.xml"));
                addMisison(
                    (ActiveMission)
                        ComputerLoader.readMission(
                            "Content/Missions/MainHub/PacemakerSet/HardwareArticles/Hardware2.xml"));
                addMisison(
                    (ActiveMission)
                        ComputerLoader.readMission(
                            "Content/Missions/MainHub/PacemakerSet/HardwareArticles/Hardware3.xml"));
                addMisison(
                    (ActiveMission)
                        ComputerLoader.readMission(
                            "Content/Missions/MainHub/PacemakerSet/HardwareArticles/Hardware4.xml"));
                addMisison(
                    (ActiveMission)
                        ComputerLoader.readMission(
                            "Content/Missions/MainHub/PacemakerSet/HardwareArticles/Hardware5.xml"));
            }
            os.branchMissions = list;
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            if (isPublic)
                state = 1;
            else
                state = 0;
        }

        public override void loginGoBack()
        {
            base.loginGoBack();
            state = 0;
        }

        public override void userLoggedIn()
        {
            base.userLoggedIn();
            if (!user.name.Equals(""))
                state = 1;
            else
                state = 0;
        }

        public bool hasSysfile()
        {
            for (var index = 0; index < root.files.Count; ++index)
            {
                if (root.files[index].name.Equals("config.sys"))
                    return true;
            }
            return false;
        }

        public bool hasListingFile(string name)
        {
            name = name.Replace(" ", "_");
            for (var index = 0; index < missionFolder.files.Count; ++index)
            {
                if (missionFolder.files[index].name == name)
                    return true;
            }
            return false;
        }

        public void drawTopBar(Rectangle bounds, SpriteBatch sb)
        {
            panelRect = new Rectangle(bounds.X, bounds.Y, bounds.Width - corner.Width, topBar.Height);
            sb.Draw(topBar, panelRect, themeColor);
            sb.Draw(corner, new Vector2(bounds.X + bounds.Width - corner.Width, bounds.Y), themeColor);
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            PatternDrawer.draw(new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2), 0.28f,
                Color.Transparent, themeColor*0.1f, sb, PatternDrawer.thinStripe);
            drawTopBar(bounds, sb);
            if (!hasSysfile())
            {
                if (Button.doButton(800003, bounds.X + 10, bounds.Y + topBar.Height + 10, 300, 30, "Exit", themeColor))
                    os.display.command = "connect";
                PatternDrawer.draw(
                    new Rectangle(bounds.X + 1, bounds.Y + 1 + 64, bounds.Width - 2, bounds.Height - 2 - 64), 1f,
                    Color.Transparent, os.lockedColor, sb, PatternDrawer.errorTile);
                var num1 = bounds.X + 20;
                var num2 = bounds.Y + bounds.Height/2 - 20;
                TextItem.doLabel(new Vector2(num1, num2), "CRITICAL ERROR", new Color?());
                var num3 = num2 + 40;
                TextItem.doSmallLabel(new Vector2(num1, num3),
                    "ERROR #4040408 - NULL_SYSFILE\nUnhandled Exception - IOException@L 2217 :R 28\nSystem Files Corrupted and/or Destroyed\nContact the System Administrator",
                    new Color?());
            }
            else
            {
                switch (state)
                {
                    case 0:
                        if (Button.doButton(800003, bounds.X + 10, bounds.Y + topBar.Height + 10, 300, 30, "Exit",
                            themeColor))
                            os.display.command = "connect";
                        sb.Draw(logo, new Rectangle(bounds.X + 30, bounds.Y + 115, 128, 128), Color.White);
                        TextItem.doFontLabel(new Vector2(bounds.X + 40 + 128, bounds.Y + 115),
                            groupName + " Group\nMessage Board", GuiData.font, new Color?(), bounds.Width - 40, 60f);
                        if (
                            !Button.doButton(800004, bounds.X + 30, bounds.Y + bounds.Height/2, 300, 40, "Login",
                                themeColor))
                            break;
                        startLogin();
                        state = 3;
                        break;
                    case 1:
                        if (Button.doButton(800003, bounds.X + 10, bounds.Y + topBar.Height + 10, 300, 30, "Exit",
                            themeColor))
                            os.display.command = "connect";
                        var num4 = bounds.X + 10;
                        var num5 = bounds.Y + topBar.Height + 50;
                        logoRect.X = num4;
                        logoRect.Y = num5;
                        sb.Draw(logo, logoRect, Color.White);
                        var x = num4 + (logoRect.Width + 5);
                        TextItem.doLabel(new Vector2(x, num5), listingTitle, new Color?());
                        var y = num5 + 40;
                        for (var index = 0; index < missions.Count; ++index)
                        {
                            if (hasListingFile(missions[index].postingTitle))
                            {
                                if (Button.doButton(87654 + index, x, y, (int) (bounds.Width*0.800000011920929), 30,
                                    missions[index].postingTitle, new Color?()))
                                {
                                    state = 2;
                                    targetIndex = index;
                                }
                                y += 35;
                            }
                        }
                        break;
                    case 2:
                        if (Button.doButton(800003, bounds.X + 10, bounds.Y + topBar.Height + 10, 300, 30, "Back",
                            themeColor))
                            state = 1;
                        var num6 = 60;
                        var num7 = 84;
                        var destinationRectangle = new Rectangle(bounds.X + 30, bounds.Y + topBar.Height + num6, num7,
                            num7);
                        sb.Draw(logo, destinationRectangle, themeColor);
                        var num8 = num6 + 30;
                        TextItem.doFontLabel(new Vector2(bounds.X + 34 + num7, bounds.Y + topBar.Height + num8),
                            missions[targetIndex].postingTitle, GuiData.font, new Color?(),
                            bounds.Width - (36 + num7 + 6), 40f);
                        var num9 = num8 + 40;
                        PatternDrawer.draw(
                            new Rectangle(destinationRectangle.X + destinationRectangle.Width + 2,
                                bounds.Y + topBar.Height + num9 - 8,
                                bounds.Width - (destinationRectangle.X - bounds.X + destinationRectangle.Width + 10),
                                PatternDrawer.warningStripe.Height/2), 1f, Color.Transparent, themeColor, sb,
                            PatternDrawer.warningStripe);
                        var num10 = num9 + 36;
                        var text = Utils.SuperSmartTwimForWidth(missions[targetIndex].postingBody, bounds.Width - 60,
                            GuiData.tinyfont);
                        TextItem.doFontLabel(new Vector2(bounds.X + 30, bounds.Y + topBar.Height + num10), text,
                            GuiData.tinyfont, new Color?(), bounds.Width, bounds.Height - num10 - topBar.Height - 10);
                        var flag = os.currentFaction.idName.ToLower() == groupName.ToLower();
                        if (missionAssigner && os.currentMission == null &&
                            (flag &&
                             Button.doButton(800005, bounds.X + bounds.Width/2 - 10, bounds.Y + bounds.Height - 35,
                                 bounds.Width/2, 30, "Accept", os.highlightColor)))
                        {
                            os.currentMission = missions[targetIndex];
                            var activeMission =
                                (ActiveMission) ComputerLoader.readMission(missions[targetIndex].reloadGoalsSourceFile);
                            missions[targetIndex].sendEmail(os);
                            missions[targetIndex].ActivateSuppressedStartFunctionIfPresent();
                            removeMission(targetIndex);
                            state = 1;
                            break;
                        }
                        if (missionAssigner && os.currentMission != null)
                        {
                            if (os.currentMission.wasAutoGenerated &&
                                Button.doButton(8000105, bounds.X + 6, bounds.Y + bounds.Height - 29, 210, 25,
                                    "Abandon Current Contract", os.lockedColor))
                            {
                                os.currentMission = null;
                                os.currentFaction.contractAbbandoned(os);
                            }
                            TextItem.doFontLabel(new Vector2(bounds.X + 10, bounds.Y + bounds.Height - 52),
                                "Mission Unavaliable : " +
                                (flag ? "Complete Existing Contracts" : "user ID Assigned to Different Faction "),
                                GuiData.smallfont, new Color?(), bounds.Width - 20, 30f);
                            break;
                        }
                        if (!missionAssigner || flag)
                            break;
                        TextItem.doFontLabel(new Vector2(bounds.X + 10, bounds.Y + bounds.Height - 52),
                            "Mission Unavaliable : User ID Assigned to Different Faction ", GuiData.smallfont,
                            new Color?(), bounds.Width - 20, 30f);
                        break;
                    case 3:
                        doLoginDisplay(bounds, sb);
                        break;
                }
            }
        }
    }
}
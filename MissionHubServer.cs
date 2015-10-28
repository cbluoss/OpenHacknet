using System;
using System.Collections.Generic;
using System.IO;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MissionHubServer : AuthenticatingDaemon
    {
        public const string ROOT_FOLDERNAME = "ContractHub";
        public const string CONFIG_FILENAME = "settings.sys";
        public const string CRITICAL_FILE_FILENAME = "net64.sys";
        public const double BUTTON_TRANSITION_OFFSET = 40.0;
        public const float TRANSITION_TIME = 0.3f;
        public const double TRANSITION_ELEMENT_INCREASE = 0.1;
        private DateTime activeUserLoginTime = DateTime.Now;
        private string activeUserName = "UNKNOWN USER";
        private BarcodeEffect barcode;
        private int contractRegistryNumber = 256;
        private readonly Texture2D decorationPanel;
        private readonly Texture2D decorationPanelSide;
        private string groupName = "UNKNOWN";
        private Folder listingArchivesFolder;
        private readonly Dictionary<string, ActiveMission> listingMissions = new Dictionary<string, ActiveMission>();
        private Folder listingsFolder;
        private readonly Texture2D lockIcon;
        private int missionListDisplayed = 1;
        private int missionListPageNumber;
        private Folder missionsFolder;
        private Folder root;
        private float screenTransition;
        private int selectedElementIndex;
        private HubState state;
        private Color themeColor = Color.PaleTurquoise;
        private readonly Color themeColorBackground = new Color(10, 15, 25, 200);
        private readonly Color themeColorLine = new Color(20, 25, 45, 200);
        private ThinBarcode thinBarcodeBot;
        private ThinBarcode thinBarcodeTop;
        private int userListPageNumber;
        private Folder usersFolder;

        public MissionHubServer(Computer c, string serviceName, string group, OS _os)
            : base(c, serviceName, _os)
        {
            groupName = group;
            decorationPanel = TextureBank.load("Sprites/HubDecoration", os.content);
            decorationPanelSide = TextureBank.load("Sprites/HubDecorationSide", os.content);
            lockIcon = TextureBank.load("Lock", os.content);
        }

        public override void initFiles()
        {
            root = new Folder("ContractHub");
            missionsFolder = new Folder("Contracts");
            listingsFolder = new Folder("Listings");
            listingArchivesFolder = new Folder("Archives");
            missionsFolder.folders.Add(listingsFolder);
            missionsFolder.folders.Add(listingArchivesFolder);
            usersFolder = new Folder("Users");
            root.folders.Add(missionsFolder);
            root.folders.Add(usersFolder);
            root.files.Add(generateConfigFile());
            root.files.Add(new FileEntry(Computer.generateBinaryString(1024), "net64.sys"));
            populateUserList();
            loadInitialContracts();
            comp.files.root.folders.Add(root);
        }

        private void populateUserList()
        {
            initializeUsers();
        }

        private void loadInitialContracts()
        {
            var num = 0;
            foreach (
                FileSystemInfo fileSystemInfo in
                    new DirectoryInfo("Content/Missions/MainHub/FirstSet/").GetFiles("*.xml"))
            {
                addMission(
                    (ActiveMission)
                        ComputerLoader.readMission("Content/Missions/MainHub/FirstSet/" + fileSystemInfo.Name), false);
                ++num;
            }
        }

        public void AddMissionToListings(string missionFilename)
        {
            addMission((ActiveMission) ComputerLoader.readMission(missionFilename), true);
        }

        private void addMission(ActiveMission mission, bool insertAtTop = false)
        {
            contractRegistryNumber += Utils.getRandomByte() + 1;
            listingMissions.Add(string.Concat(contractRegistryNumber), mission);
            var fileEntry = new FileEntry(
                MissionSerializer.generateMissionFile(mission, contractRegistryNumber, "CSEC"),
                "Contract#" + contractRegistryNumber);
            if (insertAtTop)
                listingsFolder.files.Insert(0, fileEntry);
            else
                listingsFolder.files.Add(fileEntry);
        }

        private FileEntry generateConfigFile()
        {
            return
                new FileEntry(
                    string.Concat(
                        "//Contract Hub Setup File\n" + "ThemeColor = " +
                        Utils.convertColorToParseableString(themeColor) + "\n" + "ServiceName = " + name + "\n" +
                        "GroupName = " + groupName + "\n", "ContractListingIndexes = ", contractRegistryNumber, "\n") +
                    "\n", "settings.sys");
        }

        private void loadFromConfigFileData(string config)
        {
            var strArray = config.Split(Utils.newlineDelim, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < strArray.Length; ++index)
            {
                if (!strArray[index].StartsWith("//"))
                {
                    var line = strArray[index];
                    if (line.StartsWith("ThemeColor"))
                        themeColor = Utils.convertStringToColor(getDataFromConfigLine(line, "= "));
                    else if (line.StartsWith("ServiceName"))
                        name = getDataFromConfigLine(line, "= ");
                    else if (line.StartsWith("GroupName"))
                        groupName = getDataFromConfigLine(line, "= ");
                    else if (line.StartsWith("ContractListingIndexes"))
                    {
                        try
                        {
                            contractRegistryNumber = Convert.ToInt32(getDataFromConfigLine(line, "= "));
                        }
                        catch (FormatException ex)
                        {
                            contractRegistryNumber = 0;
                        }
                        catch (OverflowException ex)
                        {
                            contractRegistryNumber = 0;
                        }
                    }
                }
            }
        }

        private string getDataFromConfigLine(string line, string sentinel = "= ")
        {
            return line.Substring(line.IndexOf(sentinel) + 2);
        }

        public override void loadInit()
        {
            base.loadInit();
            root = comp.files.root.searchForFolder("ContractHub");
            missionsFolder = root.searchForFolder("Contracts");
            listingsFolder = missionsFolder.searchForFolder("Listings");
            listingArchivesFolder = missionsFolder.searchForFolder("Archives");
            usersFolder = root.searchForFolder("Users");
            var fileEntry = root.searchForFile("settings.sys");
            if (fileEntry != null)
                loadFromConfigFileData(fileEntry.data);
            loadListingMissionsFromFiles();
        }

        private void loadListingMissionsFromFiles()
        {
            for (var index = 0; index < listingsFolder.files.Count; ++index)
            {
                var data = listingsFolder.files[index].data;
                var stringForContractFile = getIDStringForContractFile(listingsFolder.files[index]);
                try
                {
                    listingMissions.Add(stringForContractFile,
                        (ActiveMission) MissionSerializer.restoreMissionFromFile(data, out contractRegistryNumber));
                }
                catch (FormatException ex)
                {
                }
            }
        }

        public override string getSaveString()
        {
            return "<MissionHubServer />";
        }

        private void initializeUsers()
        {
            var list = new List<int>();
            var num1 = 0;
            for (var index = 0; index < People.hubAgents.Count; ++index)
            {
                num1 = 0;
                int num2;
                do
                {
                    num2 = Utils.random.Next(9999);
                } while (list.Contains(num2));
                usersFolder.files.Add(
                    new FileEntry(
                        "USER: " + num2 + "\n" + "Handle: " + People.hubAgents[index].handle + "\n" + "Date Joined : " +
                        (DateTime.Now - TimeSpan.FromDays(Utils.random.NextDouble()*200.0)).ToString()
                            .Replace('/', '-')
                            .Replace(' ', '_') + "\n" + "Status : " + generateUserState(People.hubAgents[index].handle) +
                        "\n" + "Rank : " + generateUserRank(People.hubAgents[index].handle) + "\n",
                        People.hubAgents[index].handle + (object) "#" + num2));
            }
            num1 = 0;
            int num3;
            do
            {
                num3 = Utils.random.Next(9999);
            } while (list.Contains(num3));
            usersFolder.files.Insert(0,
                new FileEntry(
                    "USER: " + num3 + "\n" + "Handle: Bit\n" + "Date Joined : " +
                    (DateTime.Now - TimeSpan.FromDays(411.0)).ToString().Replace('/', '-').Replace(' ', '_') + "\n" +
                    "Status : " + generateUserState("Bit") + "\n" + "Rank : " + generateUserRank("Bit") + "\n",
                    "Bit#" + num3));
        }

        public void addUser(UserDetail newUser)
        {
            var num = Utils.random.Next(9999);
            usersFolder.files.Add(
                new FileEntry(
                    string.Concat(
                        "USER: " + num + "\n" + "Handle: " + newUser.name + "\n" + "Date Joined : " +
                        DateTime.Now.ToString().Replace('/', '-').Replace(' ', '_') + "\n" + "Status : Active\n",
                        "Rank : ", 0, "\n"), newUser.name + (object) "#" + num));
        }

        private string generateUserRank(string username)
        {
            switch (username)
            {
                case "Bit":
                    return "2116";
                default:
                    return string.Concat((int) (Utils.random.NextDouble()*2500.0));
            }
        }

        private string generateUserState(string username)
        {
            switch (username)
            {
                case "Bit":
                    return "UNKNOWN";
                default:
                    return username.GetHashCode() < 1073741823 ? "Active" : "Passive";
            }
        }

        private string getIDStringForContractFile(FileEntry file)
        {
            var num1 = file.name.IndexOf('#');
            if (num1 <= -1)
                return "";
            var num2 = num1 + 1;
            return file.name.Substring(file.name.IndexOf('#') + 1);
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            screenTransition = 1f;
            state = HubState.Welcome;
            missionListPageNumber = 0;
            if (thinBarcodeTop != null)
                thinBarcodeTop.regenerate();
            if (thinBarcodeBot == null)
                return;
            thinBarcodeBot.regenerate();
        }

        public override void loginGoBack()
        {
            base.loginGoBack();
            state = HubState.Welcome;
            screenTransition = 1f;
        }

        public override void userLoggedIn()
        {
            base.userLoggedIn();
            activeUserName = user.name;
            state = HubState.Menu;
            screenTransition = 1f;
            activeUserLoginTime = DateTime.Now;
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            updateScreenTransition();
            switch (state)
            {
                case HubState.Welcome:
                    doBarcodeEffect(bounds, sb);
                    drawWelcomeScreen(bounds, sb);
                    break;
                case HubState.Menu:
                    doMenuScreen(bounds, sb);
                    doLoggedInScreenDetailing(bounds, sb);
                    break;
                case HubState.Login:
                    doBarcodeEffect(bounds, sb);
                    doLoginDisplay(bounds, sb);
                    break;
                case HubState.Listing:
                    doListingScreen(bounds, sb);
                    break;
                case HubState.ContractPreview:
                    doLoggedInScreenDetailing(bounds, sb);
                    doContractPreviewScreen(bounds, sb);
                    break;
                case HubState.UserList:
                    doLoggedInScreenDetailing(bounds, sb);
                    doUserListScreen(bounds, sb);
                    break;
                case HubState.CancelContract:
                    doCancelContractScreen(bounds, sb);
                    doLoggedInScreenDetailing(bounds, sb);
                    break;
            }
        }

        private void updateScreenTransition()
        {
            screenTransition -= (float) (os.lastGameTime.ElapsedGameTime.TotalSeconds/0.300000011920929);
            screenTransition = Math.Max(screenTransition, 0.0f);
        }

        private int getTransitionOffset(int position)
        {
            return (int) (Math.Pow(Math.Min(screenTransition + position*0.1, 1.0), 1.0)*40.0*screenTransition);
        }

        private void doMenuScreen(Rectangle bounds, SpriteBatch sb)
        {
            var rectangle = new Rectangle(bounds.X + 10, bounds.Y + bounds.Height/3 + 10, bounds.Width/2, 40);
            if (Button.doButton(101010, rectangle.X + getTransitionOffset(0), rectangle.Y, rectangle.Width,
                rectangle.Height, "Contract Listing", themeColor))
            {
                state = HubState.Listing;
                screenTransition = 1f;
            }
            rectangle.Y += rectangle.Height + 5;
            if (Button.doButton(101015, rectangle.X + getTransitionOffset(1), rectangle.Y, rectangle.Width,
                rectangle.Height, "User List", themeColor))
            {
                state = HubState.UserList;
                screenTransition = 1f;
            }
            rectangle.Y += rectangle.Height + 5;
            if (
                Button.doButton(101017, rectangle.X + getTransitionOffset(1), rectangle.Y, rectangle.Width,
                    rectangle.Height/2, "Abort Current Contract", os.currentMission == null ? Color.Black : themeColor) &&
                os.currentMission != null)
            {
                state = HubState.CancelContract;
                screenTransition = 1f;
            }
            rectangle.Y += rectangle.Height/2 + 5;
            if (Button.doButton(102015, rectangle.X + getTransitionOffset(3), rectangle.Y, rectangle.Width,
                rectangle.Height/2, "Exit", os.lockedColor))
                os.display.command = "connect";
            rectangle.Y += rectangle.Height + 5;
        }

        private void doCancelContractScreen(Rectangle bounds, SpriteBatch sb)
        {
            var rectangle = new Rectangle(bounds.X + 10, bounds.Y + bounds.Height/3 + 10, bounds.Width/2, 40);
            TextItem.doFontLabel(new Vector2(rectangle.X, rectangle.Y),
                "Are you sure you with to abandon your current contract?\nThis cannot be reversed.", GuiData.font,
                Color.White, bounds.Width - 30, rectangle.Height);
            rectangle.Y += rectangle.Height + 4;
            if (
                Button.doButton(142011, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, "Abandon Contract",
                    Color.Red) && os.currentMission != null)
            {
                os.currentMission = null;
                os.currentFaction.contractAbbandoned(os);
                screenTransition = 0.0f;
                state = HubState.Menu;
            }
            rectangle.Y += rectangle.Height + 10;
            if (
                !Button.doButton(142015, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, "Back", themeColor))
                return;
            screenTransition = 1f;
            state = HubState.Menu;
        }

        private void doListingScreen(Rectangle bounds, SpriteBatch sb)
        {
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            var rectangle1 = doListingScreenBackground(bounds, sb);
            var rectangle2 = new Rectangle(bounds.X + 10, rectangle1.Y, bounds.Width/2, 30);
            var height = 36;
            for (var index = missionListPageNumber*missionListDisplayed;
                index < listingsFolder.files.Count && rectangle2.Y + height < rectangle1.Y + rectangle1.Height;
                ++index)
            {
                var stringForContractFile = getIDStringForContractFile(listingsFolder.files[index]);
                if (listingMissions.ContainsKey(stringForContractFile))
                {
                    var mission = listingMissions[stringForContractFile];
                    drawMissionEntry(new Rectangle(bounds.X + 1, rectangle2.Y, bounds.Width - 2, height), sb, mission,
                        index);
                    rectangle2.Y += height;
                }
            }
            var num = 0;
            rectangle2 = new Rectangle(bounds.X + 10, rectangle1.Y, bounds.Width/2, 30);
            while (rectangle2.Y + height < rectangle1.Y + rectangle1.Height)
            {
                rectangle2.Y += height;
                ++num;
            }
            missionListDisplayed = num;
            TextItem.DrawShadow = flag;
        }

        private void drawMissionEntry(Rectangle bounds, SpriteBatch sb, ActiveMission mission, int index)
        {
            var flag1 = false;
            if (mission.postingAcceptFlagRequirements != null)
            {
                for (var index1 = 0; index1 < mission.postingAcceptFlagRequirements.Length; ++index1)
                {
                    if (!os.Flags.HasFlag(mission.postingAcceptFlagRequirements[index1]))
                        flag1 = true;
                }
            }
            var myID = index*139284 + 984275 + index;
            var flag2 = Button.outlineOnly;
            var flag3 = Button.drawingOutline;
            Button.outlineOnly = true;
            Button.drawingOutline = false;
            if (GuiData.active == myID)
                sb.Draw(Utils.white, bounds, Color.Black);
            else if (GuiData.hot == myID)
            {
                sb.Draw(Utils.white, bounds, themeColor*0.12f);
            }
            else
            {
                var color = index%2 == 0 ? themeColorLine : themeColorBackground;
                if (flag1)
                    color = Color.Lerp(color, Color.Gray, 0.25f);
                sb.Draw(Utils.white, bounds, color);
            }
            if (flag1)
            {
                var destinationRectangle = bounds;
                destinationRectangle.Height -= 6;
                destinationRectangle.Y += 3;
                destinationRectangle.X += bounds.Width - bounds.Height - 6;
                destinationRectangle.Width = destinationRectangle.Height;
                sb.Draw(lockIcon, destinationRectangle, Color.White*0.2f);
            }
            if (!flag1 && Button.doButton(myID, bounds.X, bounds.Y, bounds.Width, bounds.Height, "", Color.Transparent))
            {
                selectedElementIndex = index;
                state = HubState.ContractPreview;
                screenTransition = 1f;
            }
            var text1 = mission.postingTitle ?? "";
            TextItem.doFontLabel(new Vector2(bounds.X + 1 + getTransitionOffset(index), bounds.Y + 3), text1,
                GuiData.smallfont, Color.White, bounds.Width, float.MaxValue);
            var text2 = "Target: " + (object) mission.target + " -- Client: " + mission.client + " -- Key: " +
                        mission.generationKeys;
            TextItem.doFontLabel(new Vector2(bounds.X + 1, bounds.Y + bounds.Height - 16), text2, GuiData.detailfont,
                Color.White*0.3f, bounds.Width, 13f);
            bounds.Y += bounds.Height - 1;
            bounds.Height = 1;
            sb.Draw(Utils.white, bounds, themeColor*0.2f);
            Button.outlineOnly = flag2;
            Button.drawingOutline = flag3;
        }

        private Rectangle doListingScreenBackground(Rectangle bounds, SpriteBatch sb)
        {
            var destinationRectangle1 = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
            sb.Draw(Utils.white, destinationRectangle1, themeColorBackground);
            doLoggedInScreenDetailing(bounds, sb);
            destinationRectangle1.Height = 5;
            destinationRectangle1.Width = 0;
            destinationRectangle1.Y += 12;
            if (thinBarcodeTop == null)
                thinBarcodeTop = new ThinBarcode(bounds.Width - 4, 5);
            if (thinBarcodeBot == null)
                thinBarcodeBot = new ThinBarcode(bounds.Width - 4, 5);
            thinBarcodeTop.Draw(sb, destinationRectangle1.X, destinationRectangle1.Y, themeColor);
            TextItem.doFontLabel(new Vector2(bounds.X + 20, destinationRectangle1.Y + 10), "MISSION LISTING",
                GuiData.titlefont, themeColor, bounds.Width - 40, 35f);
            destinationRectangle1.Y += 45;
            destinationRectangle1.X = bounds.X + 1;
            thinBarcodeBot.Draw(sb, destinationRectangle1.X, destinationRectangle1.Y, themeColor);
            destinationRectangle1.Y += 10;
            destinationRectangle1.X = bounds.X + 1;
            destinationRectangle1.Width = decorationPanel.Width;
            destinationRectangle1.Height = decorationPanel.Height;
            sb.Draw(decorationPanel, destinationRectangle1, themeColor);
            destinationRectangle1.X += destinationRectangle1.Width;
            destinationRectangle1.Width = bounds.Width - 2 - destinationRectangle1.Width;
            sb.Draw(decorationPanelSide, destinationRectangle1, themeColor);
            var pos = new Vector2(bounds.X + 6, destinationRectangle1.Y + decorationPanel.Height/2 - 8);
            var height = (int) (decorationPanel.Height*0.300000011920929);
            var num1 = (float) ((decorationPanel.Height - (double) height)/2.0);
            var destinationRectangle2 = new Rectangle(bounds.X + 1, (int) (destinationRectangle1.Y + (double) num1),
                decorationPanel.Width, height);
            var rectangle = new Rectangle(0, (int) ((decorationPanel.Height - (double) height)/2.0),
                decorationPanel.Width, height);
            var flag1 = Button.outlineOnly;
            var flag2 = Button.drawingOutline;
            Button.outlineOnly = true;
            Button.drawingOutline = false;
            var myID = 974748322;
            var color = GuiData.active == myID
                ? Color.Black
                : (GuiData.hot == myID ? themeColorLine : themeColorBackground);
            sb.Draw(decorationPanel, destinationRectangle2, rectangle, color, 0.0f, Vector2.Zero, SpriteEffects.None,
                0.6f);
            if (Button.doButton(myID, bounds.X + 1, destinationRectangle1.Y + decorationPanel.Height/6 + 4,
                decorationPanel.Width - 30, (int) (decorationPanel.Height - decorationPanel.Height/6*3.20000004768372),
                "Back", Color.Transparent))
            {
                screenTransition = 1f;
                state = HubState.Menu;
            }
            Button.outlineOnly = flag1;
            Button.drawingOutline = flag2;
            pos.X += decorationPanel.Width - 10;
            pos.Y += 4f;
            var text1 = "CSEC secure contract listing panel : Verified Connection : Token last verfied " + DateTime.Now;
            TextItem.doFontLabel(pos, text1, GuiData.detailfont, themeColor, destinationRectangle1.Width - 10f, 14f);
            pos.Y += 12f;
            if (missionListPageNumber > 0 &&
                Button.doButton(188278101, (int) pos.X, (int) pos.Y, 45, 20, "<", new Color?()))
            {
                --missionListPageNumber;
                screenTransition = 1f;
            }
            destinationRectangle1.X += 50;
            var num2 = listingsFolder.files.Count/missionListDisplayed + 1;
            var text2 = (missionListPageNumber + 1).ToString() + (object) "/" + num2;
            var num3 = (float) (50.0 - GuiData.smallfont.MeasureString(text2).X/2.0);
            sb.DrawString(GuiData.smallfont, text2, new Vector2(destinationRectangle1.X + num3, (int) pos.Y + 1),
                Color.White);
            destinationRectangle1.X += 100;
            if (missionListPageNumber < num2 - 1 &&
                Button.doButton(188278102, destinationRectangle1.X, (int) pos.Y, 45, 20, ">", new Color?()))
            {
                ++missionListPageNumber;
                screenTransition = 1f;
            }
            destinationRectangle1.Y += decorationPanel.Height + 4;
            destinationRectangle1.Width = bounds.Width - 2;
            destinationRectangle1.X = bounds.X + 1;
            destinationRectangle1.Height = 7;
            sb.Draw(Utils.white, destinationRectangle1, themeColor);
            destinationRectangle1.Y += destinationRectangle1.Height;
            return new Rectangle(bounds.X, destinationRectangle1.Y, bounds.Width,
                bounds.Height - bounds.Height/12 - (destinationRectangle1.Y - bounds.Y));
        }

        private void doContractPreviewScreen(Rectangle bounds, SpriteBatch sb)
        {
            var stringForContractFile = getIDStringForContractFile(listingsFolder.files[selectedElementIndex]);
            if (!listingMissions.ContainsKey(stringForContractFile))
                return;
            var mission = listingMissions[stringForContractFile];
            var vector2 = new Vector2(bounds.X + 20, bounds.Y + 20);
            TextItem.doFontLabel(vector2 + new Vector2(getTransitionOffset(0), 0.0f),
                "CONTRACT:" + stringForContractFile, GuiData.titlefont, new Color?(), bounds.Width/2, 40f);
            vector2.Y += 40f;
            TextItem.doFontLabel(vector2 + new Vector2(getTransitionOffset(1), 0.0f), mission.postingTitle, GuiData.font,
                new Color?(), bounds.Width - 30, float.MaxValue);
            vector2.Y += 30f;
            var text = DisplayModule.cleanSplitForWidth(mission.postingBody, bounds.Width - 110);
            TextItem.doFontLabel(vector2 + new Vector2(getTransitionOffset(2), 0.0f), text, GuiData.smallfont,
                new Color?(), bounds.Width - 20, float.MaxValue);
            var num = Math.Max(135, bounds.Height/6);
            if (Button.doButton(2171618, bounds.X + 20 + getTransitionOffset(3), bounds.Y + bounds.Height - num,
                bounds.Width/5, 30, "Back", new Color?()))
            {
                state = HubState.Listing;
                screenTransition = 1f;
            }
            if (os.currentMission == null)
            {
                if (
                    !Button.doButton(2171615, bounds.X + 20 + getTransitionOffset(4),
                        bounds.Y + bounds.Height - num - 40, bounds.Width/5, 30, "Accept", os.highlightColor))
                    return;
                acceptMission(mission, selectedElementIndex, stringForContractFile);
                state = HubState.Listing;
                screenTransition = 1f;
            }
            else
                TextItem.doFontLabelToSize(
                    new Rectangle(bounds.X + 20 + getTransitionOffset(4), bounds.Y + bounds.Height - num - 40,
                        bounds.Width/2, 30), "Abort current contract to accept new ones.", GuiData.smallfont,
                    Color.White);
        }

        private void doUserListScreen(Rectangle bounds, SpriteBatch sb)
        {
            if (Button.doButton(101801, bounds.X + 2, bounds.Y + 20, bounds.Width/4, 22, "Back", themeColor))
                state = HubState.Menu;
            var rectangle = new Rectangle(bounds.X + 30, bounds.Y + 50, bounds.Width/2, 30);
            var num1 = 18 + rectangle.Height - 8;
            var num2 = (bounds.Height - 90)/num1;
            var num3 = num2*userListPageNumber;
            var num4 = 0;
            for (var index1 = num2*userListPageNumber;
                index1 < usersFolder.files.Count && index1 < num3 + num2;
                ++index1)
            {
                var strArray = usersFolder.files[index1].data.Split(Utils.newlineDelim);
                string str1;
                var str2 = str1 = "";
                var str3 = str1;
                var str4 = str1;
                var str5 = str1;
                for (var index2 = 0; index2 < strArray.Length; ++index2)
                {
                    if (strArray[index2].StartsWith("USER"))
                        str5 = getDataFromConfigLine(strArray[index2], ": ");
                    if (strArray[index2].StartsWith("Rank"))
                        str2 = getDataFromConfigLine(strArray[index2], ": ");
                    if (strArray[index2].StartsWith("Handle"))
                        str4 = getDataFromConfigLine(strArray[index2], ": ");
                    if (strArray[index2].StartsWith("Date"))
                        str3 = getDataFromConfigLine(strArray[index2], ": ");
                }
                var destinationRectangle = new Rectangle(rectangle.X + (bounds.Width - 60) - 10, rectangle.Y + 2,
                    rectangle.Height + 1, bounds.Width - 60);
                GuiData.spriteBatch.Draw(Utils.gradient, destinationRectangle, new Rectangle?(), themeColorLine,
                    1.570796f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0.8f);
                TextItem.doSmallLabel(new Vector2(rectangle.X, rectangle.Y),
                    "#" + str5 + " - \"" + str4 + "\" Rank:" + str2, new Color?());
                rectangle.Y += 18;
                TextItem.doFontLabel(new Vector2(rectangle.X, rectangle.Y), "Joined " + str3, GuiData.detailfont,
                    new Color?(), float.MaxValue, float.MaxValue);
                rectangle.Y += rectangle.Height - 10;
                num4 = index1;
            }
            rectangle.Y += 16;
            if (userListPageNumber > 0 &&
                Button.doButton(101005, rectangle.X, rectangle.Y, rectangle.Width/2 - 20, 15, "Previous Page",
                    new Color?()))
                --userListPageNumber;
            TextItem.doTinyLabel(new Vector2(rectangle.X + rectangle.Width/2 - 8, rectangle.Y),
                string.Concat(userListPageNumber), new Color?());
            if (usersFolder.files.Count <= num4 + 1 ||
                !Button.doButton(101010, rectangle.X + rectangle.Width/2 + 10, rectangle.Y, rectangle.Width/2 - 10, 15,
                    "Next Page", new Color?()))
                return;
            ++userListPageNumber;
        }

        private void acceptMission(ActiveMission mission, int index, string id)
        {
            os.currentMission = mission;
            var activeMission = (ActiveMission) ComputerLoader.readMission(mission.reloadGoalsSourceFile);
            mission.sendEmail(os);
            mission.ActivateSuppressedStartFunctionIfPresent();
            var fileEntry = listingsFolder.files[index];
            listingsFolder.files.RemoveAt(index);
            listingArchivesFolder.files.Add(
                new FileEntry(
                    "Contract Archive:\nAccepted : " + DateTime.Now + "\nUser : " + activeUserName + "\nActive Since : " +
                    activeUserLoginTime + "\n\n" + fileEntry.data, "Contract#" + id + "Archive"));
        }

        private void drawWelcomeScreen(Rectangle bounds, SpriteBatch sb)
        {
            var rectangle = new Rectangle(bounds.X + 10, bounds.Y + bounds.Height/3 - 10, bounds.Width/2, 30);
            var text = (groupName + " Contract Hub").ToUpper();
            TextItem.doFontLabel(new Vector2(rectangle.X, rectangle.Y), text, GuiData.titlefont, new Color?(),
                bounds.Width/0.6f, 50f);
            rectangle.Y += 50;
            if (Button.doButton(11005, rectangle.X + getTransitionOffset(0), rectangle.Y, rectangle.Width,
                rectangle.Height, "Login", themeColor))
            {
                startLogin();
                state = HubState.Login;
            }
            rectangle.Y += rectangle.Height + 5;
            if (
                !Button.doButton(12010, rectangle.X + getTransitionOffset(1), rectangle.Y, rectangle.Width,
                    rectangle.Height, "Exit", themeColor))
                return;
            os.display.command = "connect";
        }

        private void doBarcodeEffect(Rectangle bounds, SpriteBatch sb)
        {
            if (barcode == null || barcode.maxWidth != bounds.Width - 2 || barcode.leftRightBias)
                barcode = new BarcodeEffect(bounds.Width - 2, true, false);
            barcode.Update((float) os.lastGameTime.ElapsedGameTime.TotalSeconds);
            barcode.Draw(bounds.X + 1, bounds.Y + 5*(bounds.Height/6) - 1, bounds.Width - 2, bounds.Height/6, sb,
                themeColor);
            barcode.isInverted = false;
            barcode.Draw(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height/6, sb, themeColor);
            barcode.isInverted = true;
        }

        private void doLoggedInScreenDetailing(Rectangle bounds, SpriteBatch sb)
        {
            var text = "Authenticated User: " + activeUserName + " -- Token Active Since: " + activeUserLoginTime;
            TextItem.doFontLabel(new Vector2(bounds.X + 1, bounds.Y + 1), text, GuiData.detailfont, new Color?(),
                bounds.Width - 2, float.MaxValue);
            doBaseBarcodeEffect(bounds, sb);
        }

        private void doBaseBarcodeEffect(Rectangle bounds, SpriteBatch sb)
        {
            if (barcode == null || barcode.maxWidth != bounds.Width - 2 || !barcode.leftRightBias)
                barcode = new BarcodeEffect(bounds.Width - 2, true, true);
            barcode.Update((float) os.lastGameTime.ElapsedGameTime.TotalSeconds);
            var y = bounds.Y + 11*(bounds.Height/12) - 1;
            barcode.Draw(bounds.X + 1, y, bounds.Width - 2, bounds.Height/12, sb, themeColor);
        }

        private enum HubState
        {
            Welcome,
            Menu,
            Login,
            Listing,
            ContractPreview,
            UserList,
            CancelContract
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.OS
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using Hacknet.Effects;
using Hacknet.Factions;
using Hacknet.Gui;
using Hacknet.Mission;
using Hacknet.Modules.Overlays;
using Hacknet.PlatformAPI.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    internal class OS : GameScreen
    {
        public static bool DEBUG_COMMANDS = Settings.debugCommandsEnabled;
        public static float EXE_MODULE_HEIGHT = 250f;
        public static float TCP_STAYALIVE_TIMER = 10f;
        public static float WARNING_FLASH_TIME = 2f;
        public static int TOP_BAR_HEIGHT = 21;
        public static bool WillLoadSave;
        public static bool TestingPassOnly = false;
        public static double currentElapsedTime;
        public static float operationProgress = 0.0f;
        public static object displayObjectCache = null;
        public static OS currentInstance;
        private readonly AudioVisualizer audioVisualizer = new AudioVisualizer();
        private readonly TcpClient client;
        private readonly ASCIIEncoding encoder;
        private readonly byte[] inBuffer;
        private readonly NetworkStream netStream;

        private readonly char[] trimChars = new char[3]
        {
            ' ',
            '\n',
            char.MinValue
        };

        public List<KeyValuePair<string, string>> ActiveHackers = new List<KeyValuePair<string, string>>();
        public AllFactions allFactions;
        public SoundEffect beepSound;
        private bool bootingUp;
        public List<ActiveMission> branchMissions = new List<ActiveMission>();
        public Color brightLockedColor = new Color(160, 0, 0);
        public Color brightUnlockedColor = new Color(0, 160, 0);
        public bool canRunContent = true;
        private Texture2D cog;
        public bool commandInvalid;
        public Computer connectedComp;
        public string connectedIP = "";
        public string connectedIPLastFrame = "";
        public Color connectedNodeHighlight = new Color(222, 0, 0, 195);
        public ContentManager content;
        public CrashModule crashModule;
        private Texture2D cross;
        public Faction currentFaction;
        public ActiveMission currentMission;
        public int currentPID;
        public Color darkBackgroundColor = new Color(8, 8, 8);
        public Color defaultHighlightColor = new Color(0, 139, 199, byte.MaxValue);
        public Color defaultTopBarColor = new Color(130, 65, 27);
        public UserDetail defaultUser;
        public ActionDelayer delayer;
        private bool DestroyThreads;
        public bool DisableTopBarButtons;
        public DisplayModule display;
        public string displayCache = "";
        public Color displayModuleExtraLayerBackingColor = new Color(0, 0, 0, 0);
        private int drawErrorCount;
        public EndingSequenceModule endingSequence;
        public Color exeModuleTitleText = new Color(155, 85, 37, 0);
        public Color exeModuleTopBar = new Color(130, 65, 27, 80);
        public List<ExeModule> exes;
        public bool FirstTimeStartup = Settings.slowOSStartup;
        public ProgressionFlags Flags = new ProgressionFlags();
        public Rectangle fullscreen;
        public float gameSavedTextAlpha = -1f;
        public string getStringCache = "";
        public Color highlightColor = new Color(0, 139, 199, byte.MaxValue);
        public string homeAssetServerID = "entropy01";
        public string homeNodeID = "entropy00";
        public IncomingConnectionOverlay IncConnectionOverlay;
        public Color indentBackgroundColor = new Color(12, 12, 12);
        public bool initShowsTutorial = Settings.initShowsTutorial;
        public bool inputEnabled;
        private IntroTextModule introTextModule;
        public bool isLoaded;
        public bool isServer;
        public GameTime lastGameTime;
        public Color lightGray = new Color(180, 180, 180);
        private Thread listenerThread;
        private string locationString = "";
        public Color lockedColor = new Color(65, 16, 16, 200);
        public MailIcon mailicon;
        public Color moduleColorBacking = new Color(5, 6, 7, 10);
        public Color moduleColorSolid = new Color(50, 59, 90, byte.MaxValue);
        public Color moduleColorSolidDefault = new Color(50, 59, 90, byte.MaxValue);
        public Color moduleColorStrong = new Color(14, 28, 40, 80);
        private List<Module> modules;
        public bool multiplayer;
        private bool multiplayerMissionLoaded;
        public List<int> navigationPath = new List<int>();
        public NetworkMap netMap;
        public Color netmapToolTipBackground = new Color(0, 0, 0, 150);
        public Color netmapToolTipColor = new Color(213, 245, byte.MaxValue, 0);
        public Computer opponentComputer;
        public string opponentLocation = "";
        private byte[] outBuffer;
        public Color outlineColor = new Color(68, 68, 68);
        public Action postFXDrawActions;
        public RamModule ram;
        public int ramAvaliable = 800 - (TOP_BAR_HEIGHT + 2);
        public string SaveGameUserName = "";
        private Texture2D saveIcon;
        public string SaveUserAccountName;
        private Texture2D scanLines;
        public Color scanlinesColor = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
        public Color semiTransText = new Color(120, 120, 120, 0);
        public Color shellButtonColor = new Color(105, 167, 188);
        public Color shellColor = new Color(222, 201, 24);
        public List<string> shellIPs;
        public List<ShellExe> shells;
        public float stayAliveTimer = TCP_STAYALIVE_TIMER;
        public Color subtleTextColor = new Color(90, 90, 90);
        public Color superLightWhite = new Color(2, 2, 2, 30);
        public Terminal terminal;
        public bool terminalOnlyMode;
        public Color terminalTextColor = new Color(213, 245, byte.MaxValue);
        public Computer thisComputer;
        public Color thisComputerNode = new Color(95, 220, 83);
        public float timer;
        private Rectangle topBar;
        public Color topBarColor = new Color(0, 139, 199, byte.MaxValue);
        public Color topBarIconsColor = Color.White;
        public Color topBarTextColor = new Color(126, 126, 126, 100);
        public int totalRam = 800 - (TOP_BAR_HEIGHT + 2) - RamModule.contentStartOffset;
        public TraceDangerSequence TraceDangerSequence;
        public TraceTracker traceTracker;

        public Color unlockedColor = new Color(39, 65, 36);
        private int updateErrorCount;
        public string username = "";
        public bool validCommand;
        public Color warningColor = Color.Red;
        public float warningFlashTimer;

        public OS()
        {
            multiplayer = false;
            currentInstance = this;
        }

        public OS(TcpClient socket, NetworkStream stream, bool actingServer, ScreenManager sman)
        {
            ScreenManager = sman;
            multiplayer = true;
            client = socket;
            isServer = actingServer;
            netStream = stream;
            inBuffer = new byte[4096];
            outBuffer = new byte[4096];
            encoder = new ASCIIEncoding();
            canRunContent = false;
            TextBox.cursorPosition = 0;
            currentInstance = this;
        }

        public override void LoadContent()
        {
            if (canRunContent)
            {
                delayer = new ActionDelayer();
                ComputerLoader.init(this);
                content = ScreenManager.Game.Content;
                username = SaveUserAccountName == null
                    ? (Settings.isConventionDemo ? Settings.ConventionLoginName : Environment.UserName)
                    : SaveUserAccountName;
                username = FileSanitiser.purifyStringForDisplay(username);
                var compLocation = new Vector2(0.1f, 0.5f);
                if (multiplayer && !isServer)
                    compLocation = new Vector2(0.8f, 0.8f);
                ramAvaliable = totalRam;
                var str = !multiplayer || !isServer ? NetworkMap.generateRandomIP() : NetworkMap.generateRandomIP();
                thisComputer = new Computer(username + " PC", NetworkMap.generateRandomIP(), compLocation, 5, 4, this);
                thisComputer.adminIP = thisComputer.ip;
                thisComputer.idName = "playerComp";
                var folder = thisComputer.files.root.searchForFolder("home");
                folder.folders.Add(new Folder("stash"));
                folder.folders.Add(new Folder("misc"));
                var userDetail = thisComputer.users[0];
                userDetail.known = true;
                thisComputer.users[0] = userDetail;
                defaultUser = new UserDetail(username, "password", 1);
                defaultUser.known = true;
                var theme = OSTheme.HacknetBlue;
                if (Settings.isConventionDemo)
                {
                    var num = Utils.random.NextDouble();
                    if (num < 0.33)
                        theme = OSTheme.HacknetMint;
                    else if (num < 0.66)
                        theme = OSTheme.HackerGreen;
                }
                ThemeManager.setThemeOnComputer(thisComputer, theme);
                if (multiplayer)
                {
                    thisComputer.addMultiplayerTargetFile();
                    sendMessage("newComp #" + (object) thisComputer.ip + "#" + compLocation.X + "#" + compLocation.Y +
                                "#" + 5 + "#" + thisComputer.name);
                    multiplayerMissionLoaded = false;
                }
                if (!WillLoadSave)
                    People.init();
                modules = new List<Module>();
                exes = new List<ExeModule>();
                shells = new List<ShellExe>();
                shellIPs = new List<string>();
                var viewport = ScreenManager.GraphicsDevice.Viewport;
                var width1 = RamModule.MODULE_WIDTH;
                var height1 = 205;
                var width2 = (int) ((viewport.Width - width1 - 6)*0.4442);
                var num1 = (int) ((viewport.Width - width1 - 6)*0.5558);
                var height2 = viewport.Height - height1 - TOP_BAR_HEIGHT - 6;
                terminal =
                    new Terminal(
                        new Rectangle(viewport.Width - 2 - width2, TOP_BAR_HEIGHT, width2,
                            viewport.Height - TOP_BAR_HEIGHT - 2), this);
                terminal.name = "TERMINAL";
                modules.Add(terminal);
                netMap = new NetworkMap(new Rectangle(width1 + 4, viewport.Height - height1 - 2, num1 - 1, height1),
                    this);
                netMap.name = "netMap v1.7";
                modules.Add(netMap);
                display = new DisplayModule(new Rectangle(width1 + 4, TOP_BAR_HEIGHT, num1 - 2, height2), this);
                display.name = "DISPLAY";
                modules.Add(display);
                ram =
                    new RamModule(
                        new Rectangle(2, TOP_BAR_HEIGHT, width1, ramAvaliable + RamModule.contentStartOffset), this);
                ram.name = "RAM";
                modules.Add(ram);
                for (var index = 0; index < modules.Count; ++index)
                    modules[index].LoadContent();
                for (var index = 0; index < 2; ++index)
                {
                    if (isServer || !multiplayer)
                        thisComputer.links.Add(index);
                    else
                        thisComputer.links.Add(netMap.nodes.Count - 1 - index);
                }
                var flag1 = false;
                if (!WillLoadSave)
                {
                    netMap.nodes.Insert(0, thisComputer);
                    netMap.visibleNodes.Add(0);
                    MusicManager.loadAsCurrentSong("Music\\Revolve");
                }
                else
                {
                    loadSaveFile();
                    flag1 = true;
                    Settings.initShowsTutorial = false;
                }
                if (!multiplayer && !flag1)
                {
                    MailServer.shouldGenerateJunk = false;
                    netMap.mailServer.addNewUser(thisComputer.ip, defaultUser);
                }
                mailicon = new MailIcon(this, new Vector2(0.0f, 0.0f));
                mailicon.pos.X = viewport.Width - mailicon.getWidth() - 2;
                topBar = new Rectangle(0, 0, viewport.Width, TOP_BAR_HEIGHT - 1);
                crashModule =
                    new CrashModule(
                        new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width,
                            ScreenManager.GraphicsDevice.Viewport.Height), this);
                crashModule.LoadContent();
                introTextModule =
                    new IntroTextModule(
                        new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width,
                            ScreenManager.GraphicsDevice.Viewport.Height), this);
                introTextModule.LoadContent();
                traceTracker = new TraceTracker(this);
                IncConnectionOverlay = new IncomingConnectionOverlay(this);
                if (allFactions == null)
                {
                    allFactions = new AllFactions();
                    allFactions.init();
                }
                currentFaction = allFactions.factions[allFactions.currentFaction];
                scanLines = content.Load<Texture2D>("ScanLines");
                cross = content.Load<Texture2D>("Cross");
                cog = content.Load<Texture2D>("Cog");
                saveIcon = content.Load<Texture2D>("SaveIcon");
                beepSound = content.Load<SoundEffect>("SFX/beep");
                if (!multiplayer & !flag1)
                    loadMissionNodes();
                if (!flag1)
                    MusicManager.playSong();
                if (flag1 || !Settings.slowOSStartup)
                {
                    initShowsTutorial = false;
                    introTextModule.complete = true;
                }
                inputEnabled = true;
                isLoaded = true;
                fullscreen = new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height);
                TraceDangerSequence = new TraceDangerSequence(content, ScreenManager.SpriteBatch, fullscreen, this);
                endingSequence = new EndingSequenceModule(fullscreen, this);
                var flag2 = Settings.slowOSStartup && !flag1;
                var flag3 = Settings.osStartsWithTutorial && (!flag1 || !Flags.HasFlag("TutorialComplete"));
                if (flag2)
                {
                    rebootThisComputer();
                    if (!Settings.initShowsTutorial)
                        return;
                    display.visible = false;
                    ram.visible = false;
                    netMap.visible = false;
                    terminal.visible = true;
                }
                else if (flag3)
                {
                    display.visible = false;
                    ram.visible = false;
                    netMap.visible = false;
                    terminal.visible = true;
                    terminal.reset();
                    Settings.initShowsTutorial = true;
                    initShowsTutorial = true;
                    if (TestingPassOnly)
                        return;
                    execute("FirstTimeInitdswhupwnemfdsiuoewnmdsmffdjsklanfeebfjkalnbmsdakj Init");
                }
                else
                {
                    if (!TestingPassOnly)
                        runCommand("connect " + thisComputer.ip);
                    if (thisComputer.files.root.searchForFolder("sys").searchForFile("Notes_Reopener.bat") == null)
                        return;
                    runCommand("notes");
                }
            }
            else
            {
                if (!multiplayer)
                    return;
                initializeNetwork();
            }
        }

        public void loadMultiplayerMission()
        {
            currentMission = (ActiveMission) ComputerLoader.readMission("Content/Missions/MultiplayerMission.xml");
        }

        public void loadMissionNodes()
        {
            if (multiplayer)
                return;
            if (Settings.IsInAdventureMode)
            {
                var adventureList = BootLoadList.getAdventureList();
                for (var index = 0; index < adventureList.Count; ++index)
                    Computer.loadFromFile(adventureList[index]);
                if (ComputerLoader.postAllLoadedActions != null)
                    ComputerLoader.postAllLoadedActions();
                var computer = Programs.getComputer(this, "advStartPoint");
                if (computer != null)
                    netMap.discoverNode(computer);
            }
            else
            {
                var list = BootLoadList.getList();
                for (var index = 0; index < list.Count; ++index)
                {
                    try
                    {
                        Computer.loadFromFile(list[index]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        throw ex;
                    }
                }
                if (ComputerLoader.postAllLoadedActions != null)
                    ComputerLoader.postAllLoadedActions();
                if (Settings.isDemoMode)
                {
                    var demoList = BootLoadList.getDemoList();
                    for (var index = 0; index < demoList.Count; ++index)
                        Computer.loadFromFile(demoList[index]);
                }
            }
            if (!initShowsTutorial && !Settings.IsInAdventureMode)
            {
                if (Settings.isSpecialTestBuild)
                    ComputerLoader.loadMission("Content/Missions/Misc/TesterCSECIntroMission.xml");
                else
                    ComputerLoader.loadMission("Content/Missions/BitMissionIntro.xml");
            }
            else
            {
                if (Settings.IsInAdventureMode)
                    return;
                currentMission = (ActiveMission) ComputerLoader.readMission("Content/Missions/BitMissionIntro.xml");
            }
        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            try
            {
                lastGameTime = gameTime;
                base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
                var t = (float) gameTime.ElapsedGameTime.TotalSeconds;
                SFX.Update(t);
                timer += t;
                if (isLoaded && bootingUp)
                {
                    canRunContent = false;
                    thisComputer.bootupTick(t);
                    crashModule.Update(t);
                    if (!thisComputer.disabled)
                    {
                        if (FirstTimeStartup && !introTextModule.complete)
                            introTextModule.Update(t);
                        if (introTextModule.complete)
                        {
                            FirstTimeStartup = false;
                            bootingUp = false;
                            canRunContent = true;
                            crashModule.completeReboot();
                        }
                    }
                }
                delayer.Pump();
                if (canRunContent)
                {
                    if (isLoaded)
                    {
                        try
                        {
                            if (TraceDangerSequence.IsActive)
                                TraceDangerSequence.Update(t);
                            if (multiplayer)
                            {
                                if (!multiplayerMissionLoaded && opponentComputer != null)
                                {
                                    loadMultiplayerMission();
                                    multiplayerMissionLoaded = true;
                                }
                                stayAliveTimer -= t;
                                if (stayAliveTimer <= 0.0)
                                {
                                    stayAliveTimer = TCP_STAYALIVE_TIMER;
                                    sendMessage("stayAlive " + (int) currentElapsedTime);
                                }
                            }
                            else
                                mailicon.Update(t);
                            for (var index = 0; index < modules.Count; ++index)
                                modules[index].Update(t);
                            if (connectedComp == null)
                            {
                                if (connectedIPLastFrame != null)
                                {
                                    handleDisconnection();
                                    connectedIPLastFrame = null;
                                }
                            }
                            else if (connectedIPLastFrame != connectedComp.ip)
                                handleDisconnection();
                            ramAvaliable = totalRam;
                            if (exes.Count > 0)
                                exes[0].bounds.Y = ram.bounds.Y + RamModule.contentStartOffset;
                            for (var index = 0; index < exes.Count; ++index)
                            {
                                exes[index].bounds.X = ram.bounds.X;
                                if (index > 0 && index < exes.Count)
                                    exes[index].bounds.Y = exes[index - 1].bounds.Y + exes[index - 1].bounds.Height;
                                if (exes[index].needsRemoval)
                                {
                                    exes.RemoveAt(index);
                                    --index;
                                }
                                else
                                    ramAvaliable -= exes[index].ramCost;
                            }
                            for (var index = 0; index < exes.Count; ++index)
                                exes[index].Update(t);
                            if (currentMission != null)
                                currentMission.Update(t);
                            for (var index = 0; index < branchMissions.Count; ++index)
                                branchMissions[index].Update(t);
                            traceTracker.Update(t);
                            if (gameSavedTextAlpha > 0.0)
                                gameSavedTextAlpha -= t;
                            if (warningFlashTimer > 0.0)
                            {
                                warningFlashTimer -= t;
                                if (warningFlashTimer <= 0.0)
                                {
                                    highlightColor = defaultHighlightColor;
                                }
                                else
                                {
                                    highlightColor = Color.Lerp(defaultHighlightColor, warningColor,
                                        warningFlashTimer/WARNING_FLASH_TIME);
                                    moduleColorSolid = Color.Lerp(moduleColorSolidDefault, warningColor,
                                        warningFlashTimer/WARNING_FLASH_TIME);
                                }
                            }
                            IncConnectionOverlay.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
                            currentElapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
                            connectedIPLastFrame = connectedComp != null ? connectedComp.ip : null;
                            return;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            ++updateErrorCount;
                            if (updateErrorCount >= 5)
                                return;
                            Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex));
                            return;
                        }
                    }
                }
                endingSequence.Update(t);
            }
            catch (Exception ex)
            {
                ++updateErrorCount;
                DebugLog.add(ex.ToString());
                if (updateErrorCount < 3)
                    return;
                handleUpdateError();
            }
        }

        private void handleDisconnection()
        {
            var computer = Programs.getComputer(this, connectedIPLastFrame);
            if (computer == null)
                return;
            var administrator = computer.admin;
            if (administrator == null)
                return;
            administrator.disconnectionDetected(computer, this);
        }

        public override void HandleInput(InputState input)
        {
            GuiData.doInput(input);
            if (!Utils.keyPressed(input, Keys.NumLock, ScreenManager.controllingPlayer))
                return;
            PostProcessor.bloomEnabled = !PostProcessor.bloomEnabled;
        }

        public void drawBackground()
        {
            ThemeManager.drawBackgroundImage(GuiData.spriteBatch, fullscreen);
        }

        public void drawScanlines()
        {
            if (!PostProcessor.scanlinesEnabled)
                return;
            var position = new Vector2(0.0f, 0.0f);
            while (position.X < (double) ScreenManager.GraphicsDevice.Viewport.Width)
            {
                while (position.Y < (double) ScreenManager.GraphicsDevice.Viewport.Height)
                {
                    GuiData.spriteBatch.Draw(scanLines, position, scanlinesColor);
                    position.Y += scanLines.Height;
                }
                position.Y = 0.0f;
                position.X += scanLines.Width;
            }
        }

        public void drawModules(GameTime gameTime)
        {
            var zero = Vector2.Zero;
            GuiData.spriteBatch.Draw(Utils.white, topBar, topBarColor);
            var t = (float) gameTime.ElapsedGameTime.TotalSeconds;
            try
            {
                if (connectedComp != null)
                    locationString = "Location: " + connectedComp.name + "@" + connectedIP + " ";
                else
                    locationString = "Location: Not Connected ";
                var vector2_1 = GuiData.UITinyfont.MeasureString(locationString);
                zero.X = topBar.Width - vector2_1.X - mailicon.getWidth();
                zero.Y -= 3f;
                GuiData.spriteBatch.DrawString(GuiData.UITinyfont, locationString, zero, topBarTextColor);
                var text = "Home IP: " + thisComputer.ip + " ";
                zero.Y += topBar.Height/2;
                var vector2_2 = GuiData.UITinyfont.MeasureString(text);
                zero.X = topBar.Width - vector2_2.X - mailicon.getWidth();
                GuiData.spriteBatch.DrawString(GuiData.UITinyfont, text, zero, topBarTextColor);
                zero.Y = 0.0f;
            }
            catch (Exception ex)
            {
            }
            zero.X = 110f;
            if (!Settings.isLockedDemoMode && !DisableTopBarButtons)
            {
                if (Button.doButton(3827178, 3, 0, 20, topBar.Height - 1, "", topBarIconsColor, cross))
                {
                    var messageBoxScreen = new MessageBoxScreen("Quit HackNetOS\nVirtual Machine?\n", false, true);
                    messageBoxScreen.Accepted += quitGame;
                    ScreenManager.AddScreen(messageBoxScreen);
                }
                if (Button.doButton(3827179, 26, 0, 20, topBar.Height - 1, "", topBarIconsColor, cog))
                {
                    saveGame();
                    ScreenManager.AddScreen(new OptionsMenu(), ScreenManager.controllingPlayer);
                }
                if (!initShowsTutorial &&
                    Button.doButton(3827180, 49, 0, 20, topBar.Height - 1, "", topBarIconsColor, saveIcon))
                    saveGame();
                if (!initShowsTutorial && Settings.debugCommandsEnabled &&
                    Button.doButton(3827190, 72, 0, 20, topBar.Height - 1, "", topBarIconsColor, cog))
                {
                    if (ThemeManager.currentTheme == OSTheme.HackerGreen)
                        ThemeManager.switchTheme(this, OSTheme.HacknetBlue);
                    else if (ThemeManager.currentTheme == OSTheme.HacknetBlue)
                        ThemeManager.switchTheme(this, OSTheme.HacknetTeal);
                    else if (ThemeManager.currentTheme == OSTheme.HacknetTeal)
                        ThemeManager.switchTheme(this, OSTheme.HacknetWhite);
                    else if (ThemeManager.currentTheme == OSTheme.HacknetWhite)
                        ThemeManager.switchTheme(this, OSTheme.HacknetYellow);
                    else if (ThemeManager.currentTheme == OSTheme.HacknetYellow)
                        ThemeManager.switchTheme(this, OSTheme.HacknetPurple);
                    else if (ThemeManager.currentTheme == OSTheme.HacknetPurple)
                        ThemeManager.switchTheme(this, OSTheme.HacknetMint);
                    else if (ThemeManager.currentTheme == OSTheme.HacknetMint)
                        ThemeManager.switchTheme(this, OSTheme.HackerGreen);
                }
            }
            else
                zero.X = 2f;
            zero.Y = -2f;
            GuiData.spriteBatch.DrawString(GuiData.UITinyfont,
                string.Concat((int) (1.0/gameTime.ElapsedGameTime.TotalSeconds + 0.5)), zero, topBarTextColor);
            zero.Y = 0.0f;
            if (!multiplayer && !DisableTopBarButtons)
                mailicon.Draw();
            var num1 = ram.bounds.Height + topBar.Height + 16;
            if (num1 < fullscreen.Height && ram.visible)
                audioVisualizer.Draw(
                    new Rectangle(ram.bounds.X, num1 + 1, ram.bounds.Width - 2, fullscreen.Height - num1 - 4),
                    GuiData.spriteBatch);
            for (var index = 0; index < modules.Count; ++index)
            {
                if (modules[index].visible)
                {
                    modules[index].PreDrawStep();
                    modules[index].Draw(t);
                    modules[index].PostDrawStep();
                }
            }
            if (ram.visible)
            {
                for (var index = 0; index < exes.Count; ++index)
                    exes[index].Draw(t);
            }
            IncConnectionOverlay.Draw(fullscreen, GuiData.spriteBatch);
            traceTracker.Draw(GuiData.spriteBatch);
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            var num2 = 1f - gameSavedTextAlpha;
            var num3 = 45;
            TextItem.doFontLabel(
                new Vector2(0.0f, ScreenManager.GraphicsDevice.Viewport.Height - num3) - num2*new Vector2(0.0f, 200f),
                "SESSION SAVED", GuiData.titlefont, thisComputerNode*gameSavedTextAlpha, float.MaxValue, num3);
            TextItem.DrawShadow = flag;
        }

        public void quitGame(object sender, PlayerIndexEventArgs e)
        {
            Game1.getSingleton().Exit();
        }

        public override void Draw(GameTime gameTime)
        {
            try
            {
                var t = (float) gameTime.ElapsedGameTime.TotalSeconds;
                if (canRunContent && isLoaded)
                {
                    PostProcessor.begin();
                    GuiData.startDraw();
                    try
                    {
                        if (!TraceDangerSequence.PreventOSRendering)
                        {
                            drawBackground();
                            if (terminalOnlyMode)
                                terminal.Draw(t);
                            else
                                drawModules(gameTime);
                            SFX.Draw(GuiData.spriteBatch);
                        }
                        if (TraceDangerSequence.IsActive)
                            TraceDangerSequence.Draw();
                    }
                    catch (Exception ex)
                    {
                        ++drawErrorCount;
                        if (drawErrorCount < 5)
                            Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex) + "\r\n\r\n");
                    }
                    GuiData.endDraw();
                    PostProcessor.end();
                    GuiData.startDraw();
                    if (postFXDrawActions != null)
                    {
                        postFXDrawActions();
                        postFXDrawActions = null;
                    }
                    drawScanlines();
                    GuiData.endDraw();
                }
                else if (endingSequence.IsActive)
                {
                    PostProcessor.begin();
                    ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    endingSequence.Draw(t);
                    drawScanlines();
                    ScreenManager.SpriteBatch.End();
                    PostProcessor.end();
                }
                else if (bootingUp)
                {
                    PostProcessor.begin();
                    ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    if (thisComputer.disabled)
                        crashModule.Draw(t);
                    else
                        introTextModule.Draw(t);
                    ScreenManager.SpriteBatch.End();
                    PostProcessor.end();
                }
                else
                {
                    GuiData.startDraw();
                    TextItem.doSmallLabel(new Vector2(0.0f, 700f), "Loading...", new Color?());
                    GuiData.endDraw();
                }
            }
            catch (Exception ex)
            {
                ++drawErrorCount;
                DebugLog.add(ex.ToString());
                if (drawErrorCount < 3)
                    return;
                handleDrawError();
            }
        }

        private void handleUpdateError()
        {
            DebugLog.data.Add("----------------------Handling Update error");
            connectedComp = null;
        }

        private void handleDrawError()
        {
            connectedComp = null;
            ScreenManager.GraphicsDevice.SetRenderTarget(null);
        }

        public void endMultiplayerMatch(bool won)
        {
            if (won)
                sendMessage("mpOpponentWin " + timer);
            ScreenManager.AddScreen(new MultiplayerGameOverScreen(won));
        }

        private void initializeNetwork()
        {
            if (isServer)
            {
                var message = "init " + Utils.random.Next();
                sendMessage(message);
                Multiplayer.parseInputMessage(message, this);
            }
            else
                sendMessage("clientConnect Hallo");
            listenerThread = new Thread(listenThread);
            listenerThread.Name = "OS Listener Thread";
            listenerThread.Start();
            Console.WriteLine("Created Network Listener Thread");
        }

        public void sendMessage(string message)
        {
            if (!netStream.CanWrite)
                return;
            outBuffer = encoder.GetBytes(message + "#&#");
            netStream.Write(outBuffer, 0, outBuffer.Length);
            netStream.Flush();
            for (var index = 0; index < message.Length; ++index)
                outBuffer[index] = 0;
        }

        private void listenThread()
        {
            while (!DestroyThreads)
            {
                try
                {
                    if (netStream.Read(inBuffer, 0, inBuffer.Length) == 0)
                    {
                        clientDisconnected();
                    }
                    else
                    {
                        var str = encoder.GetString(inBuffer).Trim(trimChars);
                        var separator = new string[1]
                        {
                            "#&#"
                        };
                        foreach (var message in str.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                            Multiplayer.parseInputMessage(message, this);
                        for (var index = 0; index < inBuffer.Length; ++index)
                            inBuffer[index] = 0;
                    }
                }
                catch (IOException ex)
                {
                    if (ex.Message.Contains("Unable to read"))
                    {
                        write("Opponent Disconnected - Victory");
                        ScreenManager.AddScreen(new MultiplayerGameOverScreen(true));
                        netStream.Close();
                        return;
                    }
                    write("NetError: " + DisplayModule.splitForWidth(ex.ToString(), terminal.bounds.Width - 20));
                }
                catch (Exception ex)
                {
                    write("NetError: " + DisplayModule.splitForWidth(ex.ToString(), terminal.bounds.Width - 20));
                    break;
                }
                if (Game1.threadsExiting)
                    return;
            }
            Console.WriteLine("Listener Thread Exiting");
        }

        public void clientDisconnected()
        {
            write("DISCONNECTION detected");
            DestroyThreads = true;
            netStream.Close();
            client.Close();
        }

        public void timerExpired()
        {
            if (Flags.HasFlag("CSEC_Member"))
                TraceDangerSequence.BeginTraceDangerSequence();
            else
                thisComputer.crash(thisComputer.ip);
        }

        public void loadSaveFile()
        {
            var saveReadStream = SaveFileManager.GetSaveReadStream(SaveGameUserName);
            if (saveReadStream == null)
                return;
            var xmlReader = XmlReader.Create(saveReadStream);
            loadTitleSaveData(xmlReader);
            Flags.Load(xmlReader);
            netMap.load(xmlReader);
            currentMission = (ActiveMission) ActiveMission.load(xmlReader);
            if (currentMission != null && !(currentMission.reloadGoalsSourceFile == "Missions/BitMissionIntro.xml"))
            {
                var activeMission = (ActiveMission) ComputerLoader.readMission(currentMission.reloadGoalsSourceFile);
            }
            Console.WriteLine(branchMissions.Count);
            allFactions = AllFactions.loadFromSave(xmlReader);
            loadOtherSaveData(xmlReader);
            WillLoadSave = false;
            saveReadStream.Flush();
            saveReadStream.Close();
        }

        public void saveGame()
        {
            if (initShowsTutorial)
                return;
            execute("save!(SJN!*SNL8vAewew57WewJdwl89(*4;;;&!)@&(ak'^&#@J3KH@!*");
        }

        public void threadedSaveExecute()
        {
            lock (SaveFileManager.CurrentlySaving)
            {
                gameSavedTextAlpha = 1f;
                writeSaveGame(SaveUserAccountName);
                StatsManager.SaveStatProgress();
            }
        }

        private void writeSaveGame(string filename)
        {
            var str1 = "<?xml version =\"1.0\" encoding =\"UTF-8\" ?>\n" +
                       (object) "<HacknetSave generatedMissionCount=\"" + MissionGenerator.generationCount +
                       "\" Username=\"" + username + "\">\n" + Flags.GetSaveString() + netMap.getSaveString();
            var str2 = (currentMission == null
                ? str1 + "<mission next=\"NULL_MISSION\" goals=\"none\" activeCheck=\"none\">\n</mission>"
                : str1 + currentMission.getSaveString()) + "<branchMissions>/n";
            for (var index = 0; index < branchMissions.Count; ++index)
                str2 += branchMissions[index].getSaveString();
            SaveFileManager.WriteSaveData(
                str2 + "</branchMissions>" + allFactions.getSaveString() + "<other music=\"" +
                MusicManager.currentSongName + "\" homeNode=\"" + homeNodeID + "\" homeAssetsNode=\"" +
                homeAssetServerID + "\" />" + "</HacknetSave>", filename);
        }

        public void loadTitleSaveData(XmlReader reader)
        {
            while (reader.Name != "HacknetSave")
            {
                if (reader.EOF)
                    return;
                reader.Read();
            }
            MissionGenerator.generationCount = !reader.MoveToAttribute("generatedMissionCount")
                ? 100
                : reader.ReadContentAsInt();
            if (!reader.MoveToAttribute("Username"))
                return;
            username = reader.ReadContentAsString();
            defaultUser.name = username;
        }

        public void setMouseVisiblity(bool mouseIsVisible)
        {
            delayer.Post(ActionDelayer.NextTick(), () => Game1.getSingleton().IsMouseVisible = mouseIsVisible);
        }

        public void loadBranchMissionsSaveData(XmlReader reader)
        {
            while (reader.Name != "branchMissions")
            {
                if (reader.EOF)
                    return;
                reader.Read();
                if (reader.Name == "other")
                    return;
            }
            if (!reader.IsStartElement())
                return;
            branchMissions.Clear();
            reader.Read();
            while (true)
            {
                while (reader.IsStartElement() && reader.Name == "mission" ||
                       !reader.IsStartElement() && reader.Name == "branchMissions")
                {
                    if (reader.Name == "branchMissions")
                        return;
                    branchMissions.Add((ActiveMission) ActiveMission.load(reader));
                }
                reader.Read();
            }
        }

        public void loadOtherSaveData(XmlReader reader)
        {
            while (reader.Name != "other")
            {
                if (reader.EOF)
                    return;
                reader.Read();
            }
            reader.MoveToAttribute("music");
            MusicManager.playSongImmediatley(reader.ReadContentAsString());
            if (reader.MoveToAttribute("homeNode"))
                homeNodeID = reader.ReadContentAsString();
            if (!reader.MoveToAttribute("homeAssetsNode"))
                return;
            homeAssetServerID = reader.ReadContentAsString();
        }

        public override void inputMethodChanged(bool usingGamePad)
        {
        }

        public void write(string text)
        {
            if (terminal == null || text.Length <= 0)
                return;
            var text1 = DisplayModule.cleanSplitForWidth(text, terminal.bounds.Width - 40);
            if (text1[text1.Length - 1] == 10)
                text1 = text1.Substring(0, text1.Length - 1);
            terminal.writeLine(text1);
        }

        public void writeSingle(string text)
        {
            terminal.write(text);
        }

        public void runCommand(string text)
        {
            if (terminal.preventingExecution)
                return;
            write("\n" + terminal.prompt + text);
            terminal.lastRunCommand = text;
            execute(text);
        }

        public void execute(string text)
        {
            var strArray = text.Split(' ');
            var thread = new Thread(threadExecute);
            thread.Name = "exe" + thread.Name;
            if (!text.StartsWith("save"))
                thread.IsBackground = true;
            thread.CurrentCulture = Game1.culture;
            thread.CurrentUICulture = Game1.culture;
            Console.WriteLine("Spawning thread for command " + text);
            thread.Start(strArray);
        }

        public void connectedComputerCrashed(Computer c)
        {
            connectedComp = null;
            display.command = "crash";
        }

        public void thisComputerCrashed()
        {
            display.command = "";
            connectedComp = null;
            bootingUp = true;
            exes.Clear();
            shellIPs.Clear();
            crashModule.reset();
            setMouseVisiblity(false);
        }

        public void thisComputerIPReset()
        {
            if (traceTracker.active)
                traceTracker.active = false;
            if (!TraceDangerSequence.IsActive)
                return;
            TraceDangerSequence.CompleteIPResetSucsesfully();
        }

        public void rebootThisComputer()
        {
            crashModule.reset();
            setMouseVisiblity(false);
            crashModule.elapsedTime = CrashModule.BLUESCREEN_TIME + 0.5f;
            inputEnabled = false;
            bootingUp = true;
            thisComputer.disabled = true;
            canRunContent = false;
            thisComputer.silent = true;
            thisComputer.crash(thisComputer.ip);
            thisComputer.silent = false;
            thisComputer.bootupTick(CrashModule.BLUESCREEN_TIME + 0.5f);
            crashModule.Update(CrashModule.BLUESCREEN_TIME + 0.5f);
        }

        private void threadExecute(object threadText)
        {
            try
            {
                validCommand = ProgramRunner.ExecuteProgram(this, (string[]) threadText);
            }
            catch (Exception ex)
            {
                var num = 0 + 1;
            }
        }

        public bool hasConnectionPermission(bool admin)
        {
            if (admin)
            {
                if (connectedComp != null)
                    return connectedComp.adminIP == thisComputer.ip;
                return true;
            }
            var flag = !admin;
            if (flag && connectedComp != null && !connectedComp.userLoggedIn)
                flag = false;
            if (connectedComp != null && !(connectedComp.adminIP == thisComputer.ip))
                return flag;
            return true;
        }

        public void takeAdmin()
        {
            if (connectedComp == null)
                return;
            connectedComp.giveAdmin(thisComputer.ip);
            runCommand("connect " + connectedComp.ip);
        }

        public void takeAdmin(string ip)
        {
            var computer = Programs.getComputer(this, ip);
            if (computer == null)
                return;
            computer.giveAdmin(thisComputer.ip);
            runCommand("connect " + computer.ip);
        }

        public void warningFlash()
        {
            warningFlashTimer = WARNING_FLASH_TIME;
        }

        public Rectangle getExeBounds()
        {
            var y = ram.bounds.Y + RamModule.contentStartOffset;
            for (var index = 0; index < exes.Count; ++index)
                y += exes[index].bounds.Height;
            return new Rectangle(ram.bounds.X, y, 252, (int) EXE_MODULE_HEIGHT);
        }

        public void launchExecutable(string exeName, string exeFileData, int targetPort, string[] allParams = null)
        {
            var y = ram.bounds.Y + RamModule.contentStartOffset;
            for (var index = 0; index < exes.Count; ++index)
                y += exes[index].bounds.Height;
            var location = new Rectangle(ram.bounds.X, y, RamModule.MODULE_WIDTH, (int) EXE_MODULE_HEIGHT);
            exeName = exeName.ToLower();
            if (exeName.Equals("porthack"))
            {
                var flag1 = false;
                var flag2 = false;
                if (connectedComp != null)
                {
                    var num = 0;
                    for (var index = 0; index < connectedComp.portsOpen.Count; ++index)
                        num += connectedComp.portsOpen[index];
                    if (num > connectedComp.portsNeededForCrack)
                        flag1 = true;
                    if (connectedComp.firewall != null && !connectedComp.firewall.solved)
                    {
                        if (flag1)
                            flag2 = true;
                        flag1 = false;
                    }
                }
                if (flag1)
                    addExe(new PortHackExe(location, this));
                else if (flag2)
                    write(
                        "Target Machine Rejecting Syndicated UDP Traffic -\nBypass Firewall to allow unrestricted traffic");
                else
                    write("Too Few Open Ports to Run - \nOpen Additional Ports on Target Machine\n");
            }
            else if (exeName.Equals("forkbomb"))
            {
                if (hasConnectionPermission(true))
                {
                    if (connectedComp == null || connectedComp.ip.Equals(thisComputer.ip))
                        addExe(new ForkBombExe(location, this, thisComputer.ip));
                    else if (multiplayer && connectedComp.ip.Equals(opponentComputer.ip))
                        sendMessage("eForkBomb " + connectedComp.ip);
                    else
                        connectedComp.crash(thisComputer.ip);
                }
                else
                    write("Requires Administrator Access to Run");
            }
            else if (exeName.Equals("shell"))
            {
                if (hasConnectionPermission(true))
                {
                    var flag = false;
                    var str = connectedComp == null ? thisComputer.ip : connectedComp.ip;
                    for (var index = 0; index < exes.Count; ++index)
                    {
                        if (exes[index] is ShellExe && exes[index].targetIP.Equals(str))
                            flag = true;
                    }
                    if (!flag)
                        addExe(new ShellExe(location, this));
                    else
                        write("This computer is already running a shell.");
                }
                else
                    write("Requires Administrator Access to Run");
            }
            else
            {
                string str1 = null;
                for (var index1 = 0; index1 < PortExploits.exeNums.Count; ++index1)
                {
                    var index2 = PortExploits.exeNums[index1];
                    if (PortExploits.crackExeData[index2].Equals(exeFileData))
                    {
                        str1 = PortExploits.cracks[index2];
                        var str2 = PortExploits.crackExeData[index2];
                    }
                }
                if (str1 != null)
                {
                    switch (str1)
                    {
                        case "SSHcrack.exe":
                            addExe(new SSHCrackExe(location, this));
                            break;
                        case "FTPBounce.exe":
                            addExe(new FTPBounceExe(location, this));
                            break;
                        case "SMTPoverflow.exe":
                            addExe(new SMTPoverflowExe(location, this));
                            break;
                        case "WebServerWorm.exe":
                            addExe(new HTTPExploitExe(location, this));
                            break;
                        case "Tutorial.exe":
                            addExe(new AdvancedTutorial(location, this));
                            break;
                        case "Notes.exe":
                            addExe(new NotesExe(location, this));
                            break;
                        case "SecurityTracer.exe":
                            addExe(new SecurityTraceExe(location, this));
                            break;
                        case "SQL_MemCorrupt.exe":
                            addExe(new SQLExploitExe(location, this));
                            break;
                        case "Decypher.exe":
                            addExe(new DecypherExe(location, this, allParams));
                            break;
                        case "DECHead.exe":
                            addExe(new DecypherTrackExe(location, this, allParams));
                            break;
                        case "Clock.exe":
                            addExe(new ClockExe(location, this, allParams));
                            break;
                        case "KBT_PortTest.exe":
                            addExe(new MedicalPortExe(location, this, allParams));
                            break;
                        case "TraceKill.exe":
                            addExe(new TraceKillExe(location, this, allParams));
                            break;
                        case "eosDeviceScan.exe":
                            addExe(new EOSDeviceScannerExe(location, this, allParams));
                            break;
                        case "themeSwitch.exe":
                            addExe(new ThemeChangerExe(location, this, allParams));
                            break;
                        case "HexClock.exe":
                            addExe(new HexClockExe(location, this, allParams));
                            break;
                        case "Sequencer.exe":
                            addExe(new SequencerExe(location, this, allParams));
                            break;
                        case "hacknet.exe":
                            write(" ");
                            write(" ----- Error ----- ");
                            write(" ");
                            write("Program \"hacknet.exe\" is already running!");
                            write(" ");
                            write(" ----------------- ");
                            write(" ");
                            break;
                    }
                }
                else
                    write("Program not Found");
            }
        }

        public void addExe(ExeModule exe)
        {
            var computer = connectedComp == null ? thisComputer : connectedComp;
            if (exe.needsProxyAccess && computer.proxyActive)
                write("Proxy Active -- Cannot Execute");
            else if (ramAvaliable >= exe.ramCost)
            {
                exe.LoadContent();
                exes.Add(exe);
            }
            else
            {
                ram.FlashMemoryWarning();
                write("Insufficient Memory");
            }
        }

        public void failBoot()
        {
            graphicsFailBoot();
        }

        public void graphicsFailBoot()
        {
            ThemeManager.switchTheme(this, OSTheme.TerminalOnlyBlack);
            topBar.Y = -100000;
            terminalOnlyMode = true;
        }

        public void sucsesfulBoot()
        {
            topBar.Y = 0;
            terminalOnlyMode = false;
            connectedComp = null;
        }
    }
}
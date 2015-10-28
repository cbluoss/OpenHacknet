using System;
using System.Collections.Generic;
using System.Xml;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class NetworkMap : CoreModule
    {
        public static int NODE_SIZE = 26;
        public static float ADMIN_CIRCLE_SCALE = 0.62f;
        public static float PULSE_DECAY = 0.5f;
        public static float PULSE_FREQUENCY = 0.8f;
        public Computer academicDatabase;
        private Texture2D adminCircle;
        private Texture2D adminNodeCircle;
        public ConnectedNodeEffect adminNodeEffect;
        private Texture2D assetServerNodeOverlay;
        private Texture2D circle;
        private Vector2 circleOrigin;
        private Texture2D circleOutline;
        public List<Corporation> corporations;
        public bool DimNonConnectedNodes;
        private Texture2D homeNodeCircle;
        private string label;
        public Computer lastAddedNode;
        public Computer mailServer;
        private Texture2D nodeCircle;
        public ConnectedNodeEffect nodeEffect;
        private Texture2D nodeGlow;
        public List<Computer> nodes;
        private float pulseFade = 1f;
        private float pulseTimer = PULSE_FREQUENCY;
        private float rotation;
        private Texture2D targetNodeCircle;
        public List<int> visibleNodes;

        public NetworkMap(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
        }

        public override void LoadContent()
        {
            label = "Network Map";
            visibleNodes = new List<int>();
            if (OS.WillLoadSave)
            {
                nodes = new List<Computer>();
                corporations = new List<Corporation>();
            }
            else if (os.multiplayer)
            {
                nodes = generateNetwork(os);
                corporations = new List<Corporation>();
            }
            else
            {
                nodes = new List<Computer>();
                var list = generateGameNodes();
                nodes.Clear();
                nodes.AddRange(list);
                if (Settings.isDemoMode)
                    nodes.AddRange(generateDemoNodes());
                nodes.Insert(0, generateSPNetwork(os)[0]);
                corporations = generateCorporations();
            }
            nodeEffect = new ConnectedNodeEffect(os);
            adminNodeEffect = new ConnectedNodeEffect(os);
            adminNodeEffect.color = new Color(60, 65, 75, 19);
            circle = TextureBank.load("Circle", os.content);
            nodeCircle = TextureBank.load("NodeCircle", os.content);
            adminNodeCircle = TextureBank.load("AdminNodeCircle", os.content);
            homeNodeCircle = TextureBank.load("HomeNodeCircle", os.content);
            targetNodeCircle = TextureBank.load("TargetNodeCircle", os.content);
            assetServerNodeOverlay = TextureBank.load("AssetServerNodeOverlay", os.content);
            circleOutline = TextureBank.load("CircleOutline", os.content);
            adminCircle = TextureBank.load("AdminCircle", os.content);
            nodeGlow = TextureBank.load("RadialGradient", os.content);
            circleOrigin = new Vector2(circleOutline.Width/2, circleOutline.Height/2);
        }

        public override void Update(float t)
        {
            rotation += t/2f;
            if (pulseFade > 0.0)
            {
                pulseFade -= t*PULSE_DECAY;
            }
            else
            {
                pulseTimer -= t;
                if (pulseTimer <= 0.0)
                {
                    pulseFade = 1f;
                    pulseTimer = PULSE_FREQUENCY;
                }
            }
            for (var index = 0; index < nodes.Count; ++index)
            {
                if (nodes[index].disabled)
                    nodes[index].bootupTick(t);
            }
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            doGui(t);
        }

        public string getSaveString()
        {
            var str = "<NetworkMap>\n" + getVisibleNodesString() + "\n" + "<network>\n";
            for (var index = 0; index < nodes.Count; ++index)
                str += nodes[index].getSaveString();
            return str + "</network>\n" + "</NetworkMap>";
        }

        public string getVisibleNodesString()
        {
            var str = "<visible>";
            for (var index = 0; index < visibleNodes.Count; ++index)
                str = str + visibleNodes[index] + (index != visibleNodes.Count - 1 ? " " : "");
            return str + "</visible>";
        }

        public void load(XmlReader reader)
        {
            nodes.Clear();
            while (reader.Name != "visible")
                reader.Read();
            foreach (var str in reader.ReadElementContentAsString().Split())
                visibleNodes.Add(Convert.ToInt32(str));
            while (reader.Name != "network")
                reader.Read();
            reader.Read();
            label_13:
            while (reader.Name != "network")
            {
                while (reader.Name == "computer" && reader.NodeType != XmlNodeType.EndElement)
                    nodes.Add(Computer.load(reader, os));
                while (true)
                {
                    if ((!(reader.Name == "computer") || reader.NodeType == XmlNodeType.EndElement) &&
                        reader.Name != "network")
                        reader.Read();
                    else
                        goto label_13;
                }
            }
            for (var index1 = 0; index1 < nodes.Count; ++index1)
            {
                var computer = nodes[index1];
                for (var index2 = 0; index2 < computer.daemons.Count; ++index2)
                    computer.daemons[index2].loadInit();
            }
            loadAssignGameNodes();
            Console.WriteLine("Done loading");
        }

        private void loadAssignGameNodes()
        {
            mailServer = Programs.getComputer(os, "jmail");
            academicDatabase = Programs.getComputer(os, "academic");
        }

        public List<Corporation> generateCorporations()
        {
            var list = new List<Corporation>();
            var num = 0;
            for (var index = 0; index < num; ++index)
                list.Add(new Corporation(os));
            return list;
        }

        public List<Computer> generateNetwork(OS os)
        {
            var list1 = new List<Computer>();
            var list2 = new List<Computer>();
            var list3 = new List<Computer>();
            var num1 = 2;
            var num2 = 0.5f;
            var num3 = 4;
            var num4 = 0;
            float num5 = num1;
            var flag1 = false;
            float x = (bounds.Width - 40)/(num3*2 + 3);
            float num6 = bounds.Height - 30;
            var y = 10f;
            var compLocation = new Vector2(x, y);
            var flag2 = false;
            while (num4 >= 0 || !flag1)
            {
                var num7 = num6/(num5 + 1f);
                compLocation.Y = num7;
                for (var index1 = 0; index1 < (int) num5; ++index1)
                {
                    var computer = new Computer(NameGenerator.generateName(), generateRandomIP(), compLocation, num4,
                        Utils.flipCoin() ? (byte) 1 : (byte) 2, os);
                    Utils.random.NextDouble();
                    var index2 = Math.Min(Math.Max(num4, 0), PortExploits.services.Count - 1);
                    computer.files.root.folders[2].files.Add(
                        new FileEntry(PortExploits.crackExeData[PortExploits.portNums[index2]],
                            PortExploits.cracks[PortExploits.portNums[index2]]));
                    list3.Add(computer);
                    list1.Add(computer);
                    compLocation.Y += num7;
                }
                for (var index = 0; index < list2.Count; ++index)
                {
                    var flag3 = index - 1 >= 0 && index < list2.Count - 1;
                    var flag4 = index < list3.Count && index < list2.Count;
                    var flag5 = index + 1 < list3.Count && index + 1 < list2.Count - 1;
                    if (flag3)
                    {
                        list3[index - 1].links.Add(list1.IndexOf(list2[index]));
                        list2[index - 1].links.Add(list1.IndexOf(list3[index]));
                    }
                    if (flag4)
                    {
                        list3[index].links.Add(list1.IndexOf(list2[index]));
                        list2[index].links.Add(list1.IndexOf(list3[index]));
                    }
                    if (flag5)
                    {
                        list3[index + 1].links.Add(list1.IndexOf(list2[index]));
                        list2[index + 1].links.Add(list1.IndexOf(list3[index]));
                    }
                }
                list2.Clear();
                for (var index = 0; index < list3.Count; ++index)
                    list2.Add(list3[index]);
                list3.Clear();
                compLocation.X += x;
                if (flag1)
                {
                    --num4;
                    num5 -= num2;
                }
                else
                {
                    ++num4;
                    num5 += num2;
                }
                if (num4 > num3)
                {
                    --num4;
                    flag1 = true;
                    num5 += num2;
                }
                flag2 = false;
            }
            if (!os.multiplayer)
                list1.AddRange(generateGameNodes());
            if (Settings.isDemoMode)
                list1.AddRange(generateDemoNodes());
            return list1;
        }

        public List<Computer> generateSPNetwork(OS os)
        {
            var list = new List<Computer>();
            var computer = new Computer(NameGenerator.generateName(), generateRandomIP(), getRandomPosition(), 0, 2, os);
            computer.idName = "firstGeneratedNode";
            computer.files.root.searchForFolder("bin")
                .files.Add(new FileEntry(Utils.readEntireFile("Content/Files/config.txt"), "config.txt"));
            ThemeManager.setThemeOnComputer(computer, OSTheme.HackerGreen);
            list.Add(computer);
            os.thisComputer.files.root.folders[2].files.Add(new FileEntry(PortExploits.crackExeData[4],
                "SecurityTracer.exe"));
            return list;
        }

        public List<Computer> generateGameNodes()
        {
            var list = new List<Computer>();
            var computer1 = (Computer) ComputerLoader.loadComputer("Content/Missions/CoreServers/JMailServer.xml");
            computer1.location = new Vector2(0.7f, 0.2f);
            mailServer = computer1;
            list.Add(computer1);
            var c1 = new Computer("boatmail.com", "65.55.72.183", new Vector2(0.6f, 0.9f), 4, 3, os);
            c1.idName = "boatmail";
            c1.daemons.Add(new BoatMail(c1, "Boatmail", os));
            c1.initDaemons();
            list.Add(c1);
            var c2 =
                (Computer) ComputerLoader.loadComputer("Content/Missions/CoreServers/InternationalAcademicDatabase.xml");
            var academicDatabaseDaemon = new AcademicDatabaseDaemon(c2, "Academic Databse", os);
            c2.daemons.Add(academicDatabaseDaemon);
            c2.initDaemons();
            academicDatabaseDaemon.initFilesFromPeople(People.all);
            academicDatabase = c2;
            list.Add(c2);
            var computer2 =
                (Computer) ComputerLoader.loadComputer("Content/Missions/CoreServers/ContractHubAssetsComp.xml");
            list.Add(computer2);
            var ch = (Computer) ComputerLoader.loadComputer("Content/Missions/CoreServers/ContractHubComp.xml");
            os.delayer.Post(ActionDelayer.NextTick(), () =>
            {
                ch.daemons.Add(new MissionHubServer(ch, "CSEC Contract Database", "CSEC", os));
                ch.initDaemons();
            });
            list.Add(ch);
            computer2.location = ch.location + Corporation.getNearbyNodeOffset(ch.location, 1, 1, this);
            list.Add(new Computer("Cheater's Stash", "1337.1337.1337.1337", getRandomPosition(), 0, 2, os)
            {
                idName = "haxServer",
                files =
                {
                    root =
                    {
                        files =
                        {
                            new FileEntry(PortExploits.crackExeData[PortExploits.portNums[0]],
                                PortExploits.cracks[PortExploits.portNums[0]]),
                            new FileEntry(PortExploits.crackExeData[PortExploits.portNums[1]],
                                PortExploits.cracks[PortExploits.portNums[1]]),
                            new FileEntry(PortExploits.crackExeData[PortExploits.portNums[2]],
                                PortExploits.cracks[PortExploits.portNums[2]]),
                            new FileEntry(PortExploits.crackExeData[PortExploits.portNums[3]],
                                PortExploits.cracks[PortExploits.portNums[3]])
                        }
                    }
                }
            });
            return list;
        }

        public List<Computer> generateDemoNodes()
        {
            return new List<Computer>
            {
                new Computer("AvCon Hatland Demo PC", "192.168.1.3", getRandomPosition(), 1, 2, os)
                {
                    idName = "avcon1",
                    externalCounterpart =
                        new ExternalCounterpart("avcon1", ExternalCounterpart.getIPForServerName("avconServer"))
                }
            };
        }

        public void discoverNode(Computer c)
        {
            if (!visibleNodes.Contains(nodes.IndexOf(c)))
                visibleNodes.Add(nodes.IndexOf(c));
            c.highlightFlashTime = 1f;
            lastAddedNode = c;
        }

        public void discoverNode(string cName)
        {
            for (var index = 0; index < nodes.Count; ++index)
            {
                if (nodes[index].idName.Equals(cName))
                {
                    discoverNode(nodes[index]);
                    break;
                }
            }
        }

        public Vector2 getRandomPosition()
        {
            for (var index = 0; index < 50; ++index)
            {
                var location = generatePos();
                if (!collides(location, -1f))
                    return location;
            }
            Console.WriteLine("TOO MANY COLLISIONS");
            return generatePos();
        }

        private Vector2 generatePos()
        {
            var num = NODE_SIZE;
            return new Vector2((float) Utils.random.NextDouble(), (float) Utils.random.NextDouble());
        }

        public bool collides(Vector2 location, float minSeperation = -1f)
        {
            if (nodes == null)
                return false;
            var num = 0.075f;
            if (minSeperation > 0.0)
                num = minSeperation;
            for (var index = 0; index < nodes.Count; ++index)
            {
                if (Vector2.Distance(location, nodes[index].location) <= (double) num)
                    return true;
            }
            return false;
        }

        public void randomizeNetwork()
        {
            for (var index = 0; index < 10; ++index)
                nodes.Add(new Computer(NameGenerator.generateName(), generateRandomIP(),
                    new Vector2((float) Utils.random.NextDouble()*bounds.Width,
                        (float) Utils.random.NextDouble()*bounds.Height), Utils.random.Next(0, 4),
                    Utils.randomCompType(), os));
            var num1 = 2;
            for (var index1 = 0; index1 < nodes.Count; ++index1)
            {
                var num2 = num1 + Utils.random.Next(0, 2);
                for (var index2 = 0; index2 < num2; ++index2)
                    nodes[index1].links.Add(Utils.random.Next(0, nodes.Count - 1));
            }
        }

        public void doGui(float t)
        {
            var num1 = -1;
            var color1 = os.highlightColor;
            for (var index1 = 0; index1 < nodes.Count; ++index1)
            {
                var nodeDrawPos = GetNodeDrawPos(nodes[index1].location);
                if (visibleNodes.Contains(index1) && !nodes[index1].disabled)
                {
                    for (var index2 = 0; index2 < nodes[index1].links.Count; ++index2)
                    {
                        if (visibleNodes.Contains(nodes[index1].links[index2]) &&
                            !nodes[nodes[index1].links[index2]].disabled)
                            drawLine(GetNodeDrawPos(nodes[index1].location),
                                GetNodeDrawPos(nodes[nodes[index1].links[index2]].location),
                                new Vector2(bounds.X, bounds.Y));
                    }
                    if (pulseFade > 0.0)
                    {
                        var color2 = os.highlightColor*(pulseFade*pulseFade);
                        if (DimNonConnectedNodes)
                            color2 = Utils.AddativeRed*0.5f*(pulseFade*pulseFade);
                        spriteBatch.Draw(circleOutline,
                            new Vector2(bounds.X + (int) nodeDrawPos.X + NODE_SIZE/2,
                                bounds.Y + (int) nodeDrawPos.Y + NODE_SIZE/2), new Rectangle?(), color2, rotation,
                            circleOrigin, new Vector2(ADMIN_CIRCLE_SCALE*(float) (2.0 - 2.0*pulseFade)),
                            SpriteEffects.None, 0.5f);
                    }
                    if (nodes[index1].idName == os.homeNodeID && !nodes[index1].disabled)
                        spriteBatch.Draw(homeNodeCircle,
                            new Vector2(bounds.X + (int) nodeDrawPos.X + NODE_SIZE/2,
                                bounds.Y + (int) nodeDrawPos.Y + NODE_SIZE/2), new Rectangle?(),
                            os.connectedComp == null || os.connectedComp != nodes[index1]
                                ? (nodes[index1].Equals(os.thisComputer) ? os.thisComputerNode : os.highlightColor)
                                : (nodes[index1].adminIP == os.thisComputer.ip
                                    ? Utils.AddativeWhite
                                    : Utils.AddativeWhite), -1f*rotation, circleOrigin, new Vector2(ADMIN_CIRCLE_SCALE),
                            SpriteEffects.None, 0.5f);
                    if (nodes[index1] == lastAddedNode && !nodes[index1].disabled)
                    {
                        var num2 = nodes[index1].adminIP == os.thisComputer.ip ? 1f : 4f;
                        spriteBatch.Draw(targetNodeCircle,
                            new Vector2(bounds.X + (int) nodeDrawPos.X + NODE_SIZE/2,
                                bounds.Y + (int) nodeDrawPos.Y + NODE_SIZE/2), new Rectangle?(),
                            os.connectedComp == null || os.connectedComp != nodes[index1]
                                ? (nodes[index1].Equals(os.thisComputer)
                                    ? os.thisComputerNode
                                    : (nodes[index1].adminIP == os.thisComputer.ip
                                        ? os.highlightColor
                                        : Utils.AddativeWhite))
                                : (nodes[index1].adminIP == os.thisComputer.ip ? os.highlightColor : Utils.AddativeWhite),
                            -1f*rotation, circleOrigin,
                            new Vector2(ADMIN_CIRCLE_SCALE) +
                            Vector2.One*((float) Math.Sin(os.timer*3.0)*num2)/targetNodeCircle.Height,
                            SpriteEffects.None, 0.5f);
                    }
                }
                if (nodes[index1].adminIP == os.thisComputer.ip && !nodes[index1].disabled)
                    spriteBatch.Draw(nodes[index1].ip.Equals(os.thisComputer.ip) ? adminCircle : nodeGlow,
                        new Vector2(bounds.X + (int) nodeDrawPos.X + NODE_SIZE/2,
                            bounds.Y + (int) nodeDrawPos.Y + NODE_SIZE/2), new Rectangle?(),
                        nodes[index1].Equals(os.thisComputer) ? os.thisComputerNode : os.highlightColor, rotation,
                        circleOrigin, new Vector2(ADMIN_CIRCLE_SCALE), SpriteEffects.None, 0.5f);
            }
            lock (nodes)
            {
                for (var local_7 = 0; local_7 < nodes.Count; ++local_7)
                {
                    if (visibleNodes.Contains(local_7) && !nodes[local_7].disabled)
                    {
                        Color local_1_1;
                        if (os.thisComputer.ip == nodes[local_7].ip)
                        {
                            local_1_1 = os.thisComputerNode;
                        }
                        else
                        {
                            local_1_1 = os.connectedComp == null
                                ? (!(nodes[local_7].adminIP == os.thisComputer.ip) ||
                                   !(nodes[local_7].ip == os.opponentLocation)
                                    ? (!os.shellIPs.Contains(nodes[local_7].ip) ? os.highlightColor : os.shellColor)
                                    : Color.DarkRed)
                                : (!(os.connectedComp.ip == nodes[local_7].ip)
                                    ? (!(nodes[local_7].adminIP == os.thisComputer.ip) ||
                                       !(nodes[local_7].ip == os.opponentLocation)
                                        ? (!os.shellIPs.Contains(nodes[local_7].ip) ? os.highlightColor : os.shellColor)
                                        : Color.DarkRed)
                                    : Color.White);
                            if (nodes[local_7].highlightFlashTime > 0.0)
                            {
                                nodes[local_7].highlightFlashTime -= t;
                                local_1_1 = Color.Lerp(local_1_1, Utils.AddativeWhite,
                                    Utils.QuadraticOutCurve(nodes[local_7].highlightFlashTime));
                            }
                        }
                        var local_8 = GetNodeDrawPos(nodes[local_7].location);
                        if (DimNonConnectedNodes && os.connectedComp != null && os.connectedComp.ip != nodes[local_7].ip)
                            local_1_1 *= 0.3f;
                        if (
                            Button.doButton(2000 + local_7, bounds.X + (int) local_8.X, bounds.Y + (int) local_8.Y,
                                NODE_SIZE, NODE_SIZE, "", local_1_1,
                                nodes[local_7].adminIP == os.thisComputer.ip ? adminNodeCircle : nodeCircle) &&
                            os.inputEnabled)
                        {
                            var local_9 = false;
                            if (os.terminal.preventingExecution && os.terminal.executionPreventionIsInteruptable)
                            {
                                os.terminal.executeLine();
                                local_9 = true;
                            }
                            var nodeindex = local_7;
                            Action local_10 = () => os.runCommand("connect " + nodes[nodeindex].ip);
                            if (local_9)
                                os.delayer.Post(ActionDelayer.NextTick(), local_10);
                            else
                                local_10();
                        }
                        if (GuiData.hot == 2000 + local_7)
                            num1 = local_7;
                        if (nodes[local_7].idName == os.homeAssetServerID && !nodes[local_7].disabled)
                            spriteBatch.Draw(assetServerNodeOverlay,
                                new Vector2(bounds.X + (int) local_8.X + NODE_SIZE/2,
                                    bounds.Y + (int) local_8.Y + NODE_SIZE/2), new Rectangle?(),
                                num1 == local_7
                                    ? GuiData.Default_Lit_Backing_Color
                                    : (os.connectedComp == null || os.connectedComp != nodes[local_7]
                                        ? (nodes[local_7].Equals(os.thisComputer)
                                            ? os.thisComputerNode
                                            : os.highlightColor)
                                        : Color.Black), -0.5f*rotation, new Vector2(assetServerNodeOverlay.Width/2),
                                new Vector2(ADMIN_CIRCLE_SCALE - 0.22f), SpriteEffects.None, 0.5f);
                    }
                }
            }
            if (os.connectedComp != null && !os.connectedComp.Equals(os.thisComputer) &&
                !os.connectedComp.adminIP.Equals(os.thisComputer.ip))
            {
                GuiData.spriteBatch.Draw(nodeGlow,
                    GetNodeDrawPos(os.connectedComp.location) - new Vector2(nodeGlow.Width/2, nodeGlow.Height/2) +
                    new Vector2(bounds.X, bounds.Y) + new Vector2(NODE_SIZE/2), os.connectedNodeHighlight);
                if (nodeEffect != null && os.connectedComp != null)
                    nodeEffect.draw(spriteBatch,
                        GetNodeDrawPos(os.connectedComp.location) + new Vector2(NODE_SIZE/2) +
                        new Vector2(bounds.X, bounds.Y));
            }
            else if (os.connectedComp != null && !os.connectedComp.Equals(os.thisComputer) && adminNodeEffect != null)
                adminNodeEffect.draw(spriteBatch,
                    GetNodeDrawPos(os.connectedComp.location) + new Vector2(NODE_SIZE/2) +
                    new Vector2(bounds.X, bounds.Y));
            if (num1 != -1)
            {
                try
                {
                    var index = num1;
                    var nodeDrawPos = GetNodeDrawPos(nodes[index].location);
                    var ttpos = new Vector2(bounds.X + (int) nodeDrawPos.X + NODE_SIZE, bounds.Y + (int) nodeDrawPos.Y);
                    var text = nodes[index].getTooltipString();
                    var textSize = GuiData.tinyfont.MeasureString(text);
                    os.postFXDrawActions += () =>
                    {
                        GuiData.spriteBatch.Draw(Utils.white,
                            new Rectangle((int) ttpos.X, (int) ttpos.Y, (int) textSize.X, (int) textSize.Y),
                            os.netmapToolTipBackground);
                        TextItem.doFontLabel(ttpos, text, GuiData.tinyfont, os.netmapToolTipColor, float.MaxValue,
                            float.MaxValue);
                    };
                }
                catch (Exception ex)
                {
                    DebugLog.add(ex.ToString());
                }
            }
            if (!Settings.debugDrawEnabled)
                return;
            for (var index = 0; index < Corporation.TestedPositions.Count; ++index)
            {
                var nodeDrawPos = GetNodeDrawPos(Corporation.TestedPositions[index]);
                var position = new Vector2(bounds.X + (int) nodeDrawPos.X + NODE_SIZE, bounds.Y + (int) nodeDrawPos.Y);
                GuiData.spriteBatch.Draw(Utils.white, position, new Rectangle?(), Utils.AddativeRed, 0.0f, Vector2.Zero,
                    Vector2.One, SpriteEffects.None, 0.8f);
            }
        }

        public Vector2 GetNodeDrawPos(Vector2 nodeLocation)
        {
            var num1 = 3;
            nodeLocation = Utils.Clamp(nodeLocation, 0.0f, 1f);
            var num2 = bounds.Width - NODE_SIZE*1f;
            var num3 = bounds.Height - NODE_SIZE*1f;
            var num4 = num2 - 2*num1;
            var num5 = num3 - 2*num1;
            return new Vector2((float) (nodeLocation.X*(double) num4 + NODE_SIZE/4.0),
                (float) (nodeLocation.Y*(double) num5 + NODE_SIZE/4.0));
        }

        public static string generateRandomIP()
        {
            return Utils.random.Next(byte.MaxValue) + "." + Utils.random.Next(byte.MaxValue) + "." +
                   Utils.random.Next(byte.MaxValue) + "." + Utils.random.Next(byte.MaxValue);
        }

        public void drawLine(Vector2 origin, Vector2 dest, Vector2 offset)
        {
            var vector2 = new Vector2(NODE_SIZE/2);
            origin += vector2;
            dest += vector2;
            var y = Vector2.Distance(origin, dest);
            var rotation = (float) Math.Atan2(dest.Y - (double) origin.Y, dest.X - (double) origin.X) + 4.712389f;
            spriteBatch.Draw(Utils.white, origin + offset, new Rectangle?(), os.outlineColor, rotation, Vector2.Zero,
                new Vector2(1f, y), SpriteEffects.None, 0.5f);
        }
    }
}
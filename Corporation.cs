// Decompiled with JetBrains decompiler
// Type: Hacknet.Corporation
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class Corporation
    {
        private static readonly float COMPUTER_SEPERATION = 0.066f;
        private static readonly float COMPUTER_SEPERATION_ADD_PER_CYCLE = 0.04f;
        private static readonly float Y_ASPECT_RATIO_BIAS = 1.9f;
        public static List<Vector2> TestedPositions = new List<Vector2>();
        private bool altRotation;
        public Computer backupServer;
        private readonly string baseID;
        private readonly Vector2 basePosition;
        private readonly int baseSecurityLevel;
        public Computer fileServer;
        public Computer internalServices;
        private string ipSubstring;
        public Computer mailServer;
        public Computer mainframe;
        private string name;
        private readonly OS os;
        private string postfix;
        private readonly int serverCount;
        public List<Computer> servers;
        public Computer webServer;

        public Corporation(OS _os)
        {
            os = _os;
            baseSecurityLevel = 3;
            serverCount = 5;
            baseID = "corp" + GenerationStatics.CorportationsGenerated + "#";
            ++GenerationStatics.CorportationsGenerated;
            servers = new List<Computer>();
            altRotation = Utils.flipCoin();
            do
            {
                basePosition = os.netMap.getRandomPosition();
            } while (locationCollides(basePosition));
            generate();
        }

        private void generate()
        {
            generateName();
            generateServers();
        }

        private void generateName()
        {
            var strArray = NameGenerator.generateCompanyName();
            name = strArray[0];
            postfix = strArray[1];
            ipSubstring = NetworkMap.generateRandomIP();
            ipSubstring = ipSubstring.Substring(ipSubstring.Length - ipSubstring.LastIndexOf('.'));
            ipSubstring += ".";
        }

        private void generateServers()
        {
            generateMainframe();
            os.netMap.nodes.Add(servers[servers.Count - 1]);
            generateMailServer();
            os.netMap.nodes.Add(servers[servers.Count - 1]);
            generateWebServer();
            os.netMap.nodes.Add(servers[servers.Count - 1]);
            generateInternalServices();
            os.netMap.nodes.Add(servers[servers.Count - 1]);
            generateFileServer();
            os.netMap.nodes.Add(servers[servers.Count - 1]);
            generateBackupMachine();
            os.netMap.nodes.Add(servers[servers.Count - 1]);
            linkServers();
        }

        private void generateMainframe()
        {
            mainframe = new Computer(getFullName() + " Central Mainframe", getAddress(), getLocation(),
                baseSecurityLevel + 2, 3, os);
            mainframe.idName = baseID + "MF";
            servers.Add(mainframe);
        }

        private void generateMailServer()
        {
            mailServer = new Computer(name + " Mail Server", getAddress(), getLocation(), baseSecurityLevel, 3, os);
            mailServer.daemons.Add(new MailServer(mailServer, name + " Mail", os));
            mailServer.initDaemons();
            mailServer.idName = baseID + "MS";
            servers.Add(mailServer);
        }

        private void generateWebServer()
        {
            webServer = new Computer(name + " Web Server", getAddress(), getLocation(), baseSecurityLevel, 3, os);
            var webServerDaemon = new WebServerDaemon(webServer, name + " Web Server", os,
                "Content/Web/BaseImageWebPage.html");
            webServer.daemons.Add(webServerDaemon);
            webServer.initDaemons();
            webServerDaemon.generateBaseCorporateSite(getFullName(), "Content/Web/BaseCorporatePage.html");
            webServer.idName = baseID + "WS";
            servers.Add(webServer);
        }

        private void generateInternalServices()
        {
            internalServices = new Computer(name + " Internal Services Machine", getAddress(), getLocation(),
                baseSecurityLevel - 1, 1, os);
            internalServices.idName = baseID + "IS";
            servers.Add(internalServices);
        }

        private void generateFileServer()
        {
            fileServer = new Computer(name + " File Server", getAddress(), getLocation(), baseSecurityLevel, 3, os);
            fileServer.daemons.Add(new MissionListingServer(fileServer, "Mission Board", name, os, false, false));
            fileServer.initDaemons();
            fileServer.idName = baseID + "FS";
            servers.Add(fileServer);
        }

        private void generateBackupMachine()
        {
            backupServer = new Computer(getFullName() + " Backup Server", getAddress(), getLocation(),
                baseSecurityLevel - 1, 1, os);
            backupServer.idName = baseID + "BU";
            servers.Add(backupServer);
        }

        private void linkServers()
        {
            for (var index1 = 0; index1 < servers.Count; ++index1)
            {
                for (var index2 = 0; index2 < servers.Count; ++index2)
                {
                    if (index2 != index1)
                        servers[index1].links.Add(os.netMap.nodes.IndexOf(servers[index2]) + 1);
                }
            }
        }

        private void addServersToInternet()
        {
            for (var index = 0; index < servers.Count; ++index)
                os.netMap.nodes.Add(servers[index]);
        }

        private string getAddress()
        {
            string str;
            bool flag;
            do
            {
                str = ipSubstring + Utils.random.Next()%253 + 1;
                flag = true;
                for (var index = 0; index < servers.Count; ++index)
                {
                    if (servers[index].ip.Equals(str))
                        flag = false;
                }
            } while (!flag);
            return str;
        }

        private Vector2 getLocation()
        {
            return basePosition + getNearbyNodeOffset(basePosition, servers.Count, serverCount, os.netMap);
        }

        public static Vector2 GetOffsetPositionFromCycle(int pos, int total)
        {
            var num1 = Math.Max(0, pos/total - 1);
            var num2 = pos%total;
            var magnitude = COMPUTER_SEPERATION + num1*COMPUTER_SEPERATION_ADD_PER_CYCLE;
            var vector2 = Utils.PolarToCartesian((float) (num2/(double) total*6.28318548202515), magnitude);
            vector2.Y *= Y_ASPECT_RATIO_BIAS;
            return vector2;
        }

        private static bool GeneratedPositionIsValid(Vector2 position, NetworkMap netMap)
        {
            if (position.X < 0.0 || position.X > 1.0 || (position.Y < 0.0 || position.Y > 1.0))
                return false;
            return !netMap.collides(position, -1f);
        }

        public static Vector2 getNearbyNodeOffset(Vector2 basePos, int positionNumber, int total, NetworkMap map)
        {
            var total1 = total;
            var num1 = positionNumber;
            if (total < 20)
            {
                var num2 = 30;
                var num3 = positionNumber/(float) total;
                total1 = num2;
                num1 = (int) (num3*(double) num2);
            }
            var num4 = 300;
            for (var index = 0; index < num4; ++index)
            {
                var positionFromCycle = GetOffsetPositionFromCycle(num1 + index, total1);
                if (GeneratedPositionIsValid(positionFromCycle + basePos, map))
                    return positionFromCycle;
                TestedPositions.Add(positionFromCycle + basePos);
            }
            Console.WriteLine("Node had to resort to random");
            return map.getRandomPosition() - basePos;
        }

        public static Vector2 getNearbyNodeOffsetOld(Vector2 basePos, int positionNumber, int total, NetworkMap map,
            float ExtraSeperationDistance = 0.0f)
        {
            var num1 = 60;
            var num2 = 0;
            var vector2 = Vector2.Zero;
            Vector2 location;
            do
            {
                var num3 = positionNumber + num2;
                var num4 = total;
                while (num3 >= num4)
                {
                    num3 -= num4;
                    num4 += total;
                }
                if (num3 > 0)
                    vector2 = Utils.PolarToCartesian(num3/(float) num4*6.283185f, 1f);
                else
                    vector2.X = 1f;
                vector2.Y *= Y_ASPECT_RATIO_BIAS;
                double num5 = COMPUTER_SEPERATION;
                vector2 = new Vector2(vector2.X*COMPUTER_SEPERATION, vector2.Y*COMPUTER_SEPERATION);
                location = basePos + vector2;
                ++num2;
                TestedPositions.Add(vector2);
            } while ((location.X < 0.0 || location.X > 1.0 || (location.Y < 0.0 || location.Y > 1.0) ||
                      map.collides(location, 0.075f)) && num2 < num1);
            if (num2 >= num1)
            {
                if (ExtraSeperationDistance <= 0.0)
                    return getNearbyNodeOffsetOld(basePos, positionNumber, total, map, COMPUTER_SEPERATION);
                if (ExtraSeperationDistance <= (double) COMPUTER_SEPERATION)
                    return getNearbyNodeOffsetOld(basePos, positionNumber, total, map,
                        COMPUTER_SEPERATION + COMPUTER_SEPERATION);
                Console.WriteLine("Node had to resort to random");
                vector2 = map.getRandomPosition() - basePos;
            }
            return vector2;
        }

        private bool locationCollides(Vector2 loc)
        {
            for (var index = 0; index < servers.Count; ++index)
            {
                if (Vector2.Distance(loc, servers[index].location) <= 0.0799999982118607)
                    return true;
            }
            return loc.X < 0.0 || loc.X > 1.0 || (loc.Y < 0.0 || loc.Y > 1.0);
        }

        public string getName()
        {
            return name;
        }

        public string getFullName()
        {
            return name + postfix;
        }
    }
}
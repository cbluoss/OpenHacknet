// Decompiled with JetBrains decompiler
// Type: Hacknet.Faction
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Xml;
using Hacknet.Factions;

namespace Hacknet
{
    public class Faction
    {
        public string idName = "";
        public string name = "unknown";
        public int neededValue;
        public bool playerHasPassedValue;
        public bool PlayerLosesValueOnAbandon;
        public int playerValue;

        public Faction(string _name, int _neededValue)
        {
            playerValue = 0;
            neededValue = _neededValue;
            name = _name;
        }

        public virtual void addValue(int value, object os)
        {
            playerValue += value;
            if (playerValue < neededValue || playerHasPassedValue)
                return;
            playerPassedValue(os);
        }

        public void contractAbbandoned(object osIn)
        {
            var os = (OS) osIn;
            if (PlayerLosesValueOnAbandon)
            {
                playerValue -= 10;
                if (playerValue < 0)
                    playerValue = 0;
            }
            var mail = MailServer.generateEmail("Contract Abandoned",
                "Agent,\nYou have abandoned your current contract, and as such are no longer eligable to complete it.\n" +
                "You are now free to accept further contracts from the contact database.\n" +
                (object) "\nYour Current Ranking is " + getRank() + " of " + getMaxRank() + ".\n" + "\nThankyou,\n -" +
                name, name + " ReplyBot");
            ((MailServer) os.netMap.mailServer.getDaemon(typeof (MailServer))).addMail(mail, os.defaultUser.name);
        }

        public int getRank()
        {
            return Math.Max(1, Math.Abs((int) ((1.0 - playerValue/(double) neededValue)*getMaxRank())));
        }

        public string getSaveString()
        {
            var str = "Faction";
            if (this is EntropyFaction)
                str = "EntropyFaction";
            if (this is HubFaction)
                str = "HubFaction";
            return "<" + (object) str + " name=\"" + name + "\" id=\"" + idName + "\" neededVal=\"" + neededValue +
                   "\" playerVal=\"" + (string) (object) playerValue + "\" playerHasPassed=\"" + playerHasPassedValue +
                   "\" />";
        }

        public int getMaxRank()
        {
            return 100;
        }

        public virtual void playerPassedValue(object os)
        {
            playerHasPassedValue = true;
        }

        public static Faction loadFromSave(XmlReader xmlRdr)
        {
            var _name = "UNKNOWN";
            var str = "";
            var flag = false;
            var _neededValue = 100;
            var num = 0;
            while (xmlRdr.Name != "Faction" && xmlRdr.Name != "EntropyFaction" && xmlRdr.Name != "HubFaction")
                xmlRdr.Read();
            var name = xmlRdr.Name;
            if (xmlRdr.MoveToAttribute("name"))
                _name = xmlRdr.ReadContentAsString();
            if (xmlRdr.MoveToAttribute("id"))
                str = xmlRdr.ReadContentAsString();
            if (xmlRdr.MoveToAttribute("neededVal"))
                _neededValue = xmlRdr.ReadContentAsInt();
            if (xmlRdr.MoveToAttribute("playerVal"))
                num = xmlRdr.ReadContentAsInt();
            if (xmlRdr.MoveToAttribute("playerHasPassed"))
                flag = xmlRdr.ReadContentAsString().ToLower() == "true";
            Faction faction;
            switch (name)
            {
                case "HubFaction":
                    faction = new HubFaction(_name, _neededValue);
                    break;
                case "EntropyFaction":
                    faction = new EntropyFaction(_name, _neededValue);
                    break;
                default:
                    faction = new Faction(_name, _neededValue);
                    break;
            }
            faction.playerValue = num;
            faction.idName = str;
            faction.playerHasPassedValue = flag;
            return faction;
        }

        public bool valuePassedPoint(int oldValue, int neededValue)
        {
            if (playerValue >= neededValue)
                return oldValue < neededValue;
            return false;
        }
    }
}
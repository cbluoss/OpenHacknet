// Decompiled with JetBrains decompiler
// Type: Hacknet.Factions.AllFactions
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;
using System.Xml;

namespace Hacknet.Factions
{
    internal class AllFactions
    {
        public string currentFaction;
        public Dictionary<string, Faction> factions = new Dictionary<string, Faction>();

        public AllFactions()
        {
            currentFaction = "entropy";
        }

        public void init()
        {
            var dictionary1 = factions;
            var key1 = "entropy";
            var entropyFaction1 = new EntropyFaction("Entropy", 5);
            entropyFaction1.idName = "entropy";
            var entropyFaction2 = entropyFaction1;
            dictionary1.Add(key1, entropyFaction2);
            factions.Add("lelzSec", new Faction("lelzSec", 1000)
            {
                idName = "lelzSec"
            });
            var dictionary2 = factions;
            var key2 = "hub";
            var hubFaction1 = new HubFaction("CSEC", 10);
            hubFaction1.idName = "hub";
            var hubFaction2 = hubFaction1;
            dictionary2.Add(key2, hubFaction2);
        }

        public string getSaveString()
        {
            var str = "<AllFactions current=\"" + currentFaction + "\">\n";
            foreach (var keyValuePair in factions)
                str = str + "\t" + keyValuePair.Value.getSaveString();
            return str + "</AllFactions>";
        }

        public void setCurrentFaction(string newFaction, OS os)
        {
            currentFaction = newFaction;
            os.currentFaction = factions[currentFaction];
        }

        public static AllFactions loadFromSave(XmlReader xmlRdr)
        {
            var allFactions = new AllFactions();
            while (xmlRdr.Name != "AllFactions")
                xmlRdr.Read();
            if (xmlRdr.MoveToAttribute("current"))
                allFactions.currentFaction = xmlRdr.ReadContentAsString();
            while (!(xmlRdr.Name == "AllFactions") || xmlRdr.IsStartElement())
            {
                var faction = Faction.loadFromSave(xmlRdr);
                allFactions.factions.Add(faction.idName, faction);
                xmlRdr.Read();
            }
            return allFactions;
        }
    }
}
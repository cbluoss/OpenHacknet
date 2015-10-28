// Decompiled with JetBrains decompiler
// Type: Hacknet.Factions.EntropyFaction
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet.Factions
{
    internal class EntropyFaction : Faction
    {
        public EntropyFaction(string _name, int _neededValue)
            : base(_name, _neededValue)
        {
        }

        public override void addValue(int value, object os)
        {
            var oldValue = playerValue;
            base.addValue(value, os);
            if (!valuePassedPoint(oldValue, 3))
                return;
            ((OS) os).Flags.AddFlag("eosPathStarted");
            ComputerLoader.loadMission("Content/Missions/Entropy/StartingSet/eosMissions/eosIntroDelayer.xml");
        }

        public override void playerPassedValue(object os)
        {
            base.playerPassedValue(os);
            if (Settings.isAlphaDemoMode)
                ComputerLoader.loadMission("Content/Missions/Entropy/EntropyMission3.xml");
            else
                ((OS) os).delayer.Post(ActionDelayer.Wait(1.7),
                    () => ComputerLoader.loadMission("Content/Missions/Entropy/ThemeHackTransitionMission.xml"));
        }
    }
}
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
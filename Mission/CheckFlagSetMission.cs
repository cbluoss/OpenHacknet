using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class CheckFlagSetMission : MisisonGoal
    {
        private readonly OS os;
        public string target;

        public CheckFlagSetMission(string targetFlagName, OS _os)
        {
            target = targetFlagName;
            os = _os;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return os.Flags.HasFlag(target);
        }
    }
}
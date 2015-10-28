using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class GetStringMission : MisisonGoal
    {
        public string target;

        public GetStringMission(string targetData)
        {
            target = targetData;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            if (additionalDetails == null)
                return false;
            for (var index = 0; index < additionalDetails.Count; ++index)
            {
                if (additionalDetails[index].Contains(target))
                    return true;
            }
            return false;
        }
    }
}
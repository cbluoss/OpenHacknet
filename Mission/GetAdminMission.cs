using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class GetAdminMission : MisisonGoal
    {
        public OS os;
        public Computer target;

        public GetAdminMission(string compIP, OS _os)
        {
            os = _os;
            target = Programs.getComputer(os, compIP);
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return target.adminIP.Equals(os.thisComputer.ip);
        }

        public override void reset()
        {
        }
    }
}
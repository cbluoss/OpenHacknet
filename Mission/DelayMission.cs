using System;
using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class DelayMission : MisisonGoal
    {
        private DateTime? firstRequest = new DateTime?();
        public float time;

        public DelayMission(float time)
        {
            this.time = time;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            if (firstRequest.HasValue)
                return (DateTime.Now - firstRequest.Value).TotalSeconds >= time;
            firstRequest = DateTime.Now;
            return false;
        }
    }
}
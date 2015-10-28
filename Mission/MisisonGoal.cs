using System.Collections.Generic;

namespace Hacknet.Mission
{
    public class MisisonGoal
    {
        public virtual bool isComplete(List<string> additionalDetails = null)
        {
            return true;
        }

        public virtual void reset()
        {
        }

        public virtual string TestCompletable()
        {
            return "";
        }
    }
}
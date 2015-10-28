using System;
using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class WipeDegreesMission : MisisonGoal
    {
        public AcademicDatabaseDaemon database;
        private readonly string ownerName;

        public WipeDegreesMission(string targetName, OS _os)
        {
            var wipeDegreesMission = this;
            ownerName = targetName;
            Action init = null;
            init = () =>
            {
                if (_os.netMap.academicDatabase == null)
                    _os.delayer.Post(ActionDelayer.NextTick(), init);
                else
                    database =
                        (AcademicDatabaseDaemon) _os.netMap.academicDatabase.getDaemon(typeof (AcademicDatabaseDaemon));
            };
            init();
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return !database.hasDegrees(ownerName);
        }
    }
}
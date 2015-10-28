// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.AddDegreeMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class AddDegreeMission : MisisonGoal
    {
        public AcademicDatabaseDaemon database;
        private readonly string degreeName;
        private readonly float desiredGPA;
        private readonly string ownerName;
        private readonly string uniName;

        public AddDegreeMission(string targetName, string degreeName, string uniName, float desiredGPA, OS _os)
        {
            var addDegreeMission = this;
            ownerName = targetName;
            this.degreeName = degreeName;
            this.uniName = uniName;
            this.desiredGPA = desiredGPA;
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
            return database.doesDegreeExist(ownerName, degreeName, uniName, desiredGPA);
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.DeathRowRecordRemovalMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class DeathRowRecordRemovalMission : MisisonGoal
    {
        public Folder container;
        public Computer deathRowDatabase;
        public string fname;
        public string lname;
        public OS os;

        public DeathRowRecordRemovalMission(string firstName, string lastName, OS _os)
        {
            os = _os;
            fname = firstName;
            lname = lastName;
            var computer = Programs.getComputer(os, "deathRow");
            deathRowDatabase = computer;
            container = computer.getFolderFromPath("dr_database/Records", false);
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            return
                !((DeathRowDatabaseDaemon) deathRowDatabase.getDaemon(typeof (DeathRowDatabaseDaemon)))
                    .ContainsRecordForName(fname, lname);
        }
    }
}
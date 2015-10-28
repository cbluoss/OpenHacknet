// Decompiled with JetBrains decompiler
// Type: Hacknet.Mission.SendEmailMission
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet.Mission
{
    internal class SendEmailMission : MisisonGoal
    {
        private readonly string mailRecipient;
        private readonly string mailSubject;
        private readonly MailServer server;

        public SendEmailMission(string mailServerID, string mailRecipient, string proposedEmailSubject, OS _os)
        {
            server = (MailServer) Programs.getComputer(_os, mailServerID).getDaemon(typeof (MailServer));
            mailSubject = proposedEmailSubject;
            this.mailRecipient = mailRecipient;
        }

        public override bool isComplete(List<string> additionalDetails = null)
        {
            if (server == null)
                return true;
            return server.MailWithSubjectExists(mailRecipient, mailSubject);
        }

        public override void reset()
        {
        }
    }
}
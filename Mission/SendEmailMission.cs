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
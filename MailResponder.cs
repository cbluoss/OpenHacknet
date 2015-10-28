namespace Hacknet
{
    internal interface MailResponder
    {
        void mailSent(string mail, string userTo);

        void mailReceived(string mail, string userTo);
    }
}
namespace Hacknet
{
    internal class Administrator
    {
        public bool IsSuper;
        public bool ResetsPassword;

        public virtual void disconnectionDetected(Computer c, OS os)
        {
        }
    }
}
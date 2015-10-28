using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class Daemon
    {
        public Computer comp;
        public bool isListed;
        public string name;
        public OS os;

        public Daemon(Computer computer, string serviceName, OS opSystem)
        {
            name = serviceName;
            isListed = true;
            comp = computer;
            os = opSystem;
        }

        public virtual void initFiles()
        {
        }

        public virtual void draw(Rectangle bounds, SpriteBatch sb)
        {
        }

        public virtual void navigatedTo()
        {
        }

        public virtual void userAdded(string name, string pass, byte type)
        {
        }

        public virtual string getSaveString()
        {
            return "";
        }

        public virtual void loadInit()
        {
        }

        public static bool validUser(byte type)
        {
            if (type != 1)
                return type == 0;
            return true;
        }

        public void registerAsDefaultBootDaemon()
        {
            if (!comp.AllowsDefaultBootModule)
                return;
            var fileEntry =
                comp.files.root.searchForFolder("sys")
                    .searchForFile(ComputerTypeInfo.getDefaultBootDaemonFilename(this));
            if (fileEntry != null)
            {
                if (!(fileEntry.data != "[Locked]"))
                    return;
                fileEntry.data = name;
            }
            else
                comp.files.root.searchForFolder("sys")
                    .files.Add(new FileEntry(name, ComputerTypeInfo.getDefaultBootDaemonFilename(this)));
        }
    }
}
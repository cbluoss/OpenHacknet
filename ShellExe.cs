using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class ShellExe : ExeModule
    {
        private const int IDLE_STATE = 0;
        private const int CLOSING_STATE = -1;
        private const int PROXY_OVERLOAD_STATE = 1;
        private const int FORKBOMB_TRAP_STATE = 2;
        public static int INFOBAR_HEIGHT = 16;
        public static int BASE_RAM_COST = 40;
        public static float RAM_CHANGE_PS = 200f;
        public static int TRAP_RAM_USE = 100;
        private Computer compThisShellIsRunningOn;
        private Computer destComp;
        private int destCompIndex = -1;
        public string destinationIP = "";
        private Rectangle infoBar;
        private int state;
        private int targetRamUse = BASE_RAM_COST;

        public ShellExe(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            ramCost = BASE_RAM_COST;
            IdentifierName = "Shell@" + targetIP;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            infoBar = new Rectangle(bounds.X, bounds.Y, bounds.Width, INFOBAR_HEIGHT);
            os.shells.Add(this);
            os.shellIPs.Add(targetIP);
            compThisShellIsRunningOn = Programs.getComputer(os, targetIP);
            compThisShellIsRunningOn.log(os.thisComputer.ip + "_Opened_#SHELL");
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (!Programs.getComputer(os, targetIP).adminIP.Equals(os.thisComputer.ip) ||
                Programs.getComputer(os, targetIP).disabled)
                Completed();
            if (targetRamUse != ramCost)
            {
                if (targetRamUse < ramCost)
                {
                    ramCost -= (int) (t*(double) RAM_CHANGE_PS);
                    if (ramCost < targetRamUse)
                        ramCost = targetRamUse;
                }
                else
                {
                    var num = (int) (t*(double) RAM_CHANGE_PS);
                    if (os.ramAvaliable >= num)
                    {
                        ramCost += num;
                        if (ramCost > targetRamUse)
                            ramCost = targetRamUse;
                    }
                }
            }
            switch (state)
            {
                case 1:
                    if (!destComp.hasProxy)
                        break;
                    destComp.proxyOverloadTicks -= t;
                    if (destComp.proxyOverloadTicks <= 0.0)
                    {
                        destComp.proxyOverloadTicks = 0.0f;
                        destComp.proxyActive = false;
                        completedAction(1);
                        break;
                    }
                    destComp.hostileActionTaken();
                    break;
            }
        }

        public override void Draw(float t)
        {
            var str = IdentifierName;
            IdentifierName = "@" + targetIP;
            base.Draw(t);
            drawOutline();
            drawTarget("Shell");
            IdentifierName = str;
            doGui();
        }

        public void doGui()
        {
            if (state == 2 && ramCost == targetRamUse)
            {
                var color = os.highlightColor;
                if (os.opponentLocation.Equals(targetIP))
                    color = os.lockedColor;
                if (Button.doButton(95000 + os.exes.IndexOf(this), bounds.X + 10, bounds.Y + 20, bounds.Width - 20, 50,
                    "Trigger", color))
                {
                    destComp.forkBombClients(targetIP);
                    completedAction(2);
                    compThisShellIsRunningOn.log("#SHELL_TrapActivate_:_ConnectionsFlooded");
                }
            }
            doControlButtons();
        }

        public void doControlButtons()
        {
            if (Button.doButton(89100 + os.exes.IndexOf(this), bounds.X + 1, bounds.Y + bounds.Height - 19, 40, 17,
                "Close", os.lockedColor*fade))
                Completed();
            if (Button.doButton(89200 + os.exes.IndexOf(this), bounds.X + 45, bounds.Y + bounds.Height - 19, 65, 17,
                "Overload", os.shellButtonColor*fade))
            {
                state = 1;
                targetRamUse = BASE_RAM_COST;
                destinationIP = os.connectedComp == null ? os.thisComputer.ip : os.connectedComp.ip;
                if (destComp == null || destComp.ip != destinationIP)
                    compThisShellIsRunningOn.log("#SHELL_Overload_@_" + destinationIP);
                destComp = Programs.getComputer(os, destinationIP);
                destCompIndex = os.netMap.nodes.IndexOf(destComp);
            }
            if (Button.doButton(89300 + os.exes.IndexOf(this), bounds.X + 115, bounds.Y + bounds.Height - 19, 40, 17,
                "Trap", os.shellButtonColor*fade))
            {
                state = 2;
                targetRamUse = TRAP_RAM_USE;
                destinationIP = os.connectedComp == null ? os.thisComputer.ip : os.connectedComp.ip;
                if (destComp == null || destComp.ip != destinationIP)
                    compThisShellIsRunningOn.log("#SHELL_TrapAcive");
                destComp = Programs.getComputer(os, destinationIP);
                destCompIndex = os.netMap.nodes.IndexOf(destComp);
            }
            if (
                !Button.doButton(89400 + os.exes.IndexOf(this), bounds.X + 160, bounds.Y + bounds.Height - 19, 50, 17,
                    "Cancel", os.shellButtonColor*fade))
                return;
            cancelTarget();
        }

        public void completedAction(int action)
        {
            cancelTarget();
        }

        public void cancelTarget()
        {
            state = 0;
            destinationIP = "";
            destComp = null;
            destCompIndex = -1;
            targetRamUse = BASE_RAM_COST;
        }

        public override void Completed()
        {
            base.Completed();
            cancelTarget();
            state = -1;
            os.shells.Remove(this);
            os.shellIPs.Remove(targetIP);
            if (!isExiting)
                compThisShellIsRunningOn.log("#SHELL_Closed");
            isExiting = true;
        }

        public void reportedTo(string data)
        {
        }
    }
}
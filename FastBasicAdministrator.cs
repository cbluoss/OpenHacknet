using System;

namespace Hacknet
{
    internal class FastBasicAdministrator : Administrator
    {
        public override void disconnectionDetected(Computer c, OS os)
        {
            base.disconnectionDetected(c, os);
            for (var index = 0; index < c.ports.Count; ++index)
                c.closePort(c.ports[index], "LOCAL_ADMIN");
            if (c.firewall != null)
            {
                c.firewall.resetSolutionProgress();
                c.firewall.solved = false;
            }
            if (c.hasProxy)
            {
                c.proxyActive = true;
                c.proxyOverloadTicks = c.startingOverloadTicks;
            }
            var time = 20.0*Utils.random.NextDouble();
            Action action = () =>
            {
                if (os.connectedComp != null && !(os.connectedComp.ip != c.ip))
                    return;
                for (var index = 0; index < c.ports.Count; ++index)
                    c.closePort(c.ports[index], "LOCAL_ADMIN");
                if (ResetsPassword)
                    c.setAdminPassword(PortExploits.getRandomPassword());
                c.adminIP = c.ip;
                if (c.firewall == null)
                    return;
                c.firewall.resetSolutionProgress();
            };
            if (IsSuper)
                action();
            else
                os.delayer.Post(ActionDelayer.Wait(time), action);
        }
    }
}
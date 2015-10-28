// Decompiled with JetBrains decompiler
// Type: Hacknet.BasicAdministrator
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet
{
    internal class BasicAdministrator : Administrator
    {
        public override void disconnectionDetected(Computer c, OS os)
        {
            base.disconnectionDetected(c, os);
            var time = 20.0*Utils.random.NextDouble();
            os.delayer.Post(ActionDelayer.Wait(time), () =>
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
                c.firewall.solved = false;
                c.firewall.resetSolutionProgress();
            });
        }
    }
}
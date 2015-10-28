// Decompiled with JetBrains decompiler
// Type: Hacknet.MailResponder
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet
{
    internal interface MailResponder
    {
        void mailSent(string mail, string userTo);

        void mailReceived(string mail, string userTo);
    }
}
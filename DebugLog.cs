// Decompiled with JetBrains decompiler
// Type: Hacknet.DebugLog
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;

namespace Hacknet
{
    public static class DebugLog
    {
        public static char[] delimiters = new char[2]
        {
            '\n',
            '\r'
        };

        public static List<string> data = new List<string>(64);

        public static void add(string s)
        {
            var strArray = s.Split(delimiters);
            for (var index = 0; index < strArray.Length; ++index)
            {
                if (!strArray[index].Equals(""))
                    data.Add(strArray[index]);
            }
        }

        public static string GetDump()
        {
            var str = "";
            for (var index = 0; index < data.Count; ++index)
                str = str + data[index] + "\r\n";
            return str;
        }
    }
}
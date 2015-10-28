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
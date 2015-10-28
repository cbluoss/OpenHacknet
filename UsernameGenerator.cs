using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    public static class UsernameGenerator
    {
        private static readonly string[] delims = new string[1]
        {
            "\r\n\r\n"
        };

        public static string[] names;
        private static int nameIndex;

        public static void init()
        {
            var streamReader = new StreamReader(TitleContainer.OpenStream("Content/Usernames.txt"));
            var str = streamReader.ReadToEnd();
            streamReader.Close();
            names = str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            nameIndex = (int) (Utils.random.NextDouble()*(names.Length - 1));
        }

        public static string getName()
        {
            nameIndex = (nameIndex + 1)%(names.Length - 1);
            return names[nameIndex];
        }
    }
}
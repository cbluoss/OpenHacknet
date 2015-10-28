using System.Collections.Generic;

namespace Hacknet
{
    internal static class NameGenerator
    {
        public static List<string> main;
        public static List<string> postfix;

        public static void init()
        {
            main = new List<string>();
            main.Add("Holopoint");
            main.Add("Ascendant");
            main.Add("Enabled");
            main.Add("Subversion");
            main.Add("Introversion");
            main.Add("Photonic");
            main.Add("Enlightened");
            main.Add("Software");
            main.Add("Facespace");
            main.Add("Mott");
            main.Add("Starchip");
            main.Add("Macrosoft");
            main.Add("Oppol");
            main.Add("Octovision");
            main.Add("tijital");
            main.Add("Valence");
            main.Add("20%Cooler");
            main.Add("Celestia");
            main.Add("Manic");
            main.Add("Dengler");
            main.Add("Beagle");
            main.Add("Warden");
            main.Add("Phoenix");
            main.Add("Banished Stallion");
            postfix = new List<string>();
            postfix.Add(" Inc");
            postfix.Add(" Interactive");
            postfix.Add(".com");
            postfix.Add(" Internal");
            postfix.Add(" Software");
            postfix.Add(" Technologies");
            postfix.Add(" Tech");
            postfix.Add(" Solutions");
            postfix.Add(" Enterprises");
            postfix.Add(" Studios");
            postfix.Add(" Consortium");
            postfix.Add(" Communications");
        }

        public static string generateName()
        {
            return "" + main[Utils.random.Next(0, main.Count)] + postfix[Utils.random.Next(0, postfix.Count)];
        }

        public static string[] generateCompanyName()
        {
            return new string[2]
            {
                main[Utils.random.Next(0, main.Count)],
                postfix[Utils.random.Next(0, postfix.Count)]
            };
        }

        public static string getRandomMain()
        {
            return main[Utils.random.Next(0, main.Count)];
        }
    }
}
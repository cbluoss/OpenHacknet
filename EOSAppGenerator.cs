// Decompiled with JetBrains decompiler
// Type: Hacknet.EOSAppGenerator
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Text;

namespace Hacknet
{
    public class EOSAppGenerator
    {
        public static string[] Name1 = new string[18]
        {
            "Candy",
            "Super",
            "Angry",
            "Extreme",
            "Crazy",
            "Delicious",
            "Zombie",
            "Spirits",
            "Ghost",
            "Spooky",
            "Skate",
            "Smash",
            "Tiny",
            "Mini",
            "Small",
            "Cute",
            "Baby",
            "Die"
        };

        public static string[] Name2 = new string[21]
        {
            "Ultraviolence",
            "Violence",
            "Crush",
            "Ninja",
            "Samurai",
            "Birds",
            "Zombie",
            "Rope",
            "Crowds",
            "Mecha",
            "Racer",
            "Point",
            "Fighter",
            "Warrior",
            "Gem",
            "Treasure",
            "Gold",
            "Booty",
            "Loot",
            "Fart",
            "Hard"
        };

        public static string[] Name3 = new string[18]
        {
            "Quest",
            "Saga",
            "Trilogy",
            "1",
            "2",
            "II",
            "IV",
            "8",
            "XXX",
            "Adventure",
            "Crusher",
            "Builder",
            "Extreme",
            "Ultra",
            "MK2",
            "Season Pass",
            "DLC",
            "Free"
        };

        public static string[] Postfix = new string[16]
        {
            "Revengance",
            "The Third",
            "Payback Time",
            "Die Harder",
            "No Survivors",
            "No Prisoners",
            "Bird's Revenge",
            "Edgy Reboot",
            "Justice Edition",
            "Get Naked",
            "Pay2Win Edition",
            "Enemy Unknown",
            "The Crushening",
            "Elucidator",
            "Blood on the sand",
            "The Long War"
        };

        public static string[] SaveData1 = new string[22]
        {
            "Sacred",
            "Holy",
            "Shiny",
            "Gold",
            "Precious",
            "Outlawed",
            "Dirty",
            "Mysterious",
            "Paid",
            "Blood",
            "Culturally Significant",
            "Irreplacable",
            "Priceless",
            "Rare",
            "Normal",
            "Beloved",
            "Evil",
            "Really Evil",
            "Chaotic Good",
            "Huge",
            "DLC",
            "Dramatic"
        };

        public static string[] SaveData2 = new string[30]
        {
            "Urns",
            "Ruins",
            "Coins",
            "Objects",
            "Grandparents",
            "Mermaids",
            "Slaves",
            "Artifacts",
            "Artworks",
            "Planets",
            "Sunglasses",
            "Weapons",
            "Ninja Techniques",
            "Family Members",
            "Spaceships",
            "Racing Vehicles",
            "Tamed Animals",
            "Animals",
            "Wild Panthers",
            "Flowers",
            "Candies",
            "Gems",
            "Birds",
            "Anime Girls",
            "Gains",
            "Reps",
            "Catchphrases",
            "One-Liners",
            "Kanye West Tweets",
            "Buisness Cards"
        };

        public static string[] SaveData3 = new string[30]
        {
            "Desecrated",
            "Ruined",
            "Toppled",
            "Punched",
            "Bought",
            "Collected",
            "Found",
            "Upended",
            "Ruined",
            "Soiled",
            "Violated",
            "Rescued",
            "Witnessed",
            "Gathered",
            "Lost",
            "Murdured",
            "Ultraviolenced",
            "Found",
            "Taken",
            "Liberated",
            "Tamed",
            "Touched",
            "Unlocked",
            "Beaten",
            "Conquered",
            "Hacked",
            "Rescued",
            "Kidnapped",
            "Hoarded",
            "Dodged"
        };

        public static string[] SaveDataWildcards = new string[13]
        {
            "Killed in a single punch",
            "Donated to charity",
            "Stolen form orphans",
            "Crushed in fist",
            "Thrown into the sun",
            "Loved tenderly",
            "Shown off in mirror",
            "Posted on the internet",
            "Thrown into hammerspace",
            "Forgotten thanks to alcohol",
            "Kickflipped over",
            "Brought to justice",
            "Flaunted in rap video"
        };

        private static string[] GenerateNames()
        {
            var strArray = new string[2];
            var stringBuilder = new StringBuilder("");
            var num = 0;
            if (Utils.flipCoin())
            {
                stringBuilder.Append(Utils.RandomFromArray(Name1) + " ");
                ++num;
            }
            stringBuilder.Append(Utils.RandomFromArray(Name2));
            if (Utils.flipCoin() || num == 0)
            {
                stringBuilder.Append(" " + Utils.RandomFromArray(Name3) + " ");
                ++num;
            }
            strArray[1] = stringBuilder.ToString().Trim();
            var flag = Utils.flipCoin();
            if (num <= 1 && flag || flag && Utils.flipCoin())
            {
                var str = Utils.RandomFromArray(Postfix);
                if (Utils.random.NextDouble() < 0.13)
                    str = Utils.RandomFromArray(Name3);
                if (Utils.flipCoin())
                    str = str.ToUpper();
                stringBuilder.Append(": " + str);
            }
            strArray[0] = stringBuilder.ToString().Trim();
            return strArray;
        }

        public static string GenerateName()
        {
            return GenerateNames()[0];
        }

        public static string GenerateAppSaveLine()
        {
            var stringBuilder = new StringBuilder("");
            if (Utils.random.NextDouble() < 0.7)
            {
                stringBuilder.Append(Utils.RandomFromArray(SaveData1));
                stringBuilder.Append(" ");
            }
            stringBuilder.Append(Utils.RandomFromArray(SaveData2));
            stringBuilder.Append(" ");
            if (Utils.random.NextDouble() < 0.08)
                stringBuilder.Append(Utils.RandomFromArray(SaveDataWildcards) + " ");
            else
                stringBuilder.Append(Utils.RandomFromArray(SaveData3) + " ");
            stringBuilder.Append(": ");
            var num = 0;
            if (Utils.random.NextDouble() < 0.7)
                num = Utils.getRandomByte();
            if (Utils.random.NextDouble() < 0.2)
                num = Utils.random.Next();
            stringBuilder.Append(string.Concat(num));
            return stringBuilder.ToString();
        }

        public static Folder GetAppFolder()
        {
            var strArray = GenerateNames();
            var str = strArray[0];
            var foldername = strArray[1].ToLower().Replace(" ", "_").Trim();
            var data = str.Replace(" ", "_").Trim();
            var folder = new Folder(foldername);
            folder.files.Add(new FileEntry(Computer.generateBinaryString(1024), "app.pkg"));
            var stringBuilder = new StringBuilder("----- [" + str + "] Save Data -----\n\n");
            var num = 8 + Utils.random.Next(8);
            for (var index = 0; index < num; ++index)
            {
                stringBuilder.Append(GenerateAppSaveLine());
                stringBuilder.Append("\n\n");
            }
            folder.files.Add(new FileEntry(stringBuilder.ToString(), FileSanitiser.purifyStringForDisplay(data) + ".sav"));
            return folder;
        }
    }
}
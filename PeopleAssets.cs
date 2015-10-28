// Decompiled with JetBrains decompiler
// Type: Hacknet.PeopleAssets
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;

namespace Hacknet
{
    internal class PeopleAssets
    {
        private static readonly string[] degreeTitles = new string[3]
        {
            "Bachelor of ",
            "Masters in ",
            "PHD in "
        };

        private static readonly string[] degreeNames = new string[12]
        {
            "Computer Science",
            "Buisness",
            "Electrical Engineering",
            "Finance",
            "Marketing",
            "The Arts",
            "Computer Graphics",
            "Design",
            "Medicine",
            "Pharmacy",
            "Information Technology",
            "Psychology"
        };

        private static readonly string[] hackerDegreeNames = new string[5]
        {
            "Computer Science",
            "Digital Security",
            "Computer Networking",
            "Information Technology",
            "Computer Graphics"
        };

        public static Degree getRandomDegree(WorldLocation origin)
        {
            var degree = new Degree();
            degree.name = randOf(degreeTitles) + randOf(degreeNames);
            degree.GPA = (float) (3.0 + 3.0*(Utils.random.NextDouble() - 0.5)*0.5);
            while (degree.GPA > 4.0)
                degree.GPA -= Utils.randm(0.4f);
            degree.uni = "University of " + origin.name;
            if (Utils.flipCoin())
                degree.uni = origin.name + " University";
            return degree;
        }

        public static Degree getRandomHackerDegree(WorldLocation origin)
        {
            var degree = new Degree();
            degree.name = randOf(degreeTitles) + randOf(hackerDegreeNames);
            degree.GPA = (float) (3.0 + 5.0*(Utils.random.NextDouble() - 0.5)*0.5);
            degree.uni = "University of " + origin.name;
            if (Utils.flipCoin())
                degree.uni = origin.name + " University";
            return degree;
        }

        public static string randOf(string[] array)
        {
            var index = (int) (Math.Max(0.0001, Utils.random.NextDouble())*array.Length - 1.0);
            return array[index];
        }
    }
}
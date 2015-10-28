using System;
using System.Collections.Generic;

namespace Hacknet
{
    public static class WorldLocationLoader
    {
        public static List<WorldLocation> locations;

        public static void init()
        {
            var strArray1 = Utils.readEntireFile("Content/PersonData/LocationData.txt").Split(Utils.newlineDelim);
            var chArray = new char[1]
            {
                '#'
            };
            locations = new List<WorldLocation>(strArray1.Length);
            for (var index = 0; index < strArray1.Length; ++index)
            {
                var strArray2 = strArray1[index].Split(chArray);
                locations.Add(new WorldLocation(strArray2[1], strArray2[0], (float) Convert.ToDouble(strArray2[2]),
                    (float) Convert.ToDouble(strArray2[3]), (float) Convert.ToDouble(strArray2[4]),
                    (float) Convert.ToDouble(strArray2[5])));
            }
        }

        public static WorldLocation getRandomLocation()
        {
            return locations[Utils.random.Next(locations.Count)];
        }

        public static WorldLocation getClosestOrCreate(string name)
        {
            for (var index = 0; index < locations.Count; ++index)
            {
                if (locations[index].name.ToLower().Equals(name.ToLower()))
                    return locations[index];
            }
            var worldLocation = new WorldLocation(name, name, (float) Utils.random.NextDouble(),
                (float) Utils.random.NextDouble(), (float) Utils.random.NextDouble(), (float) Utils.random.NextDouble());
            locations.Add(worldLocation);
            return worldLocation;
        }
    }
}
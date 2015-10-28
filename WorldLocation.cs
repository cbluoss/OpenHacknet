// Decompiled with JetBrains decompiler
// Type: Hacknet.WorldLocation
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet
{
    public struct WorldLocation
    {
        public string country;
        public string name;
        public float educationLevel;
        public float lifeLevel;
        public float employerLevel;
        public float affordabilityLevel;

        public WorldLocation(string countryName, string locName, float education, float life, float employer,
            float affordability)
        {
            country = countryName;
            name = locName;
            educationLevel = education;
            lifeLevel = life;
            employerLevel = employer;
            affordabilityLevel = affordability;
        }

        public new string ToString()
        {
            return name + ", " + country;
        }
    }
}
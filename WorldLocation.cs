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
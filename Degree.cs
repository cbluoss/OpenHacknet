namespace Hacknet
{
    public struct Degree
    {
        public string name;
        public string uni;
        public float GPA;

        public Degree(string degreeName, string uniName, float degreeGPA)
        {
            name = degreeName;
            uni = uniName;
            GPA = degreeGPA;
        }

        public override string ToString()
        {
            return name + " from " + uni + ". GPA: " + GPA;
        }
    }
}
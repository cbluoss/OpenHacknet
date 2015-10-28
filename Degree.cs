// Decompiled with JetBrains decompiler
// Type: Hacknet.Degree
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

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
            return name + (object) " from " + uni + ". GPA: " + GPA;
        }
    }
}
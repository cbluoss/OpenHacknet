// Decompiled with JetBrains decompiler
// Type: Hacknet.Person
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;

namespace Hacknet
{
    public class Person
    {
        public WorldLocation birthplace;
        public DateTime DateOfBirth = DateTime.Now;
        public List<Degree> degrees;
        public string firstName;
        public string handle;
        public bool isHacker;
        public bool isMale = true;
        public string lastName;
        public MedicalRecord medicalRecord;
        public List<VehicleRegistration> vehicles;

        public Person(string fName, string lName, bool male, bool isHacker = false, string handle = null)
        {
            firstName = fName;
            lastName = lName;
            isMale = male;
            this.handle = handle;
            this.isHacker = isHacker;
            birthplace = WorldLocationLoader.getRandomLocation();
            vehicles = new List<VehicleRegistration>();
            degrees = new List<Degree>();
            addRandomDegrees();
            addRandomVehicles();
            var num1 = 18;
            var num2 = 72;
            if (isHacker)
                num2 = 45;
            DateOfBirth = DateTime.Now -
                          TimeSpan.FromDays(num1*365 + (int) (Utils.random.NextDouble()*(num2 - num1)*365.0));
            medicalRecord = new MedicalRecord(birthplace, DateOfBirth);
        }

        public string FullName
        {
            get { return firstName + " " + lastName; }
        }

        public void addRandomDegrees()
        {
            var num = 0.6;
            if (isHacker)
                num = 0.9;
            while (Utils.random.NextDouble() < num)
            {
                if (isHacker)
                    degrees.Add(PeopleAssets.getRandomHackerDegree(birthplace));
                else
                    degrees.Add(PeopleAssets.getRandomDegree(birthplace));
                num *= num;
                if (isHacker)
                    num *= 0.36;
            }
        }

        public void addRandomVehicles()
        {
            var num = 0.7;
            while (Utils.random.NextDouble() < num)
            {
                vehicles.Add(VehicleInfo.getRandomRegistration());
                num *= num;
                if (isHacker)
                    num *= num;
            }
        }

        public override string ToString()
        {
            return firstName + " " + lastName + "\n Gender: " + (isMale ? "Male  " : "Female  ") + "Born: " +
                   birthplace.ToString() + "\n" + getDegreeString() + " " + getVehicleRegString() + "\n" + medicalRecord;
        }

        private string getDegreeString()
        {
            var str = "Degrees:\n";
            for (var index = 0; index < degrees.Count; ++index)
                str = str + " -" + degrees[index] + "\n";
            return str;
        }

        private string getVehicleRegString()
        {
            var str = "Vehicle Registrations:\n";
            for (var index = 0; index < vehicles.Count; ++index)
                str = str + " -" + vehicles[index].ToString() + "\n";
            return str;
        }
    }
}
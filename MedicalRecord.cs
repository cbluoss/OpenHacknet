// Decompiled with JetBrains decompiler
// Type: Hacknet.MedicalRecord
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Xml;

namespace Hacknet
{
    public class MedicalRecord
    {
        private static List<string> Allergants;
        public List<string> Allergies = new List<string>();
        public string BloodType = "AB";
        public DateTime DateofBirth;
        public int Height = 172;
        public string Notes = "N/A";
        public List<string> Perscriptions = new List<string>();
        public List<string> Visits = new List<string>();

        public MedicalRecord(WorldLocation location, DateTime dob)
        {
            DateofBirth = dob;
            var days = (DateTime.Now - dob).Days;
            var num1 = 155;
            var num2 = 220;
            Height = num1 + (int) (Utils.random.NextDouble()*Utils.random.NextDouble()*(num2 - num1));
            AddRandomVists(days, location);
            BloodType = (Utils.flipCoin() ? 'A' : (Utils.flipCoin() ? 'B' : 'O')) +
                        (Utils.flipCoin() ? 'A' : (Utils.flipCoin() ? 'B' : 'O')).ToString();
            if (Allergants == null)
                LoadStatics();
            AddAllergies();
        }

        private void LoadStatics()
        {
            Allergants = new List<string>();
            Allergants.AddRange(Utils.readEntireFile("Content/PersonData/Allergies.txt")
                .Split(Utils.robustNewlineDelim, StringSplitOptions.RemoveEmptyEntries));
        }

        private void AddAllergies()
        {
            var num1 = 4;
            var num2 = Math.Max(0, Utils.random.Next(0, num1 + 2) - 2);
            for (var index = 0; index < num2; ++index)
            {
                string str;
                do
                {
                    str = Allergants[Utils.random.Next(0, Allergants.Count - 1)];
                } while (Allergies.Contains(str));
                Allergies.Add(str);
            }
        }

        private void AddRandomVists(int daysOld, WorldLocation loc)
        {
            var num1 = (int) (Utils.random.NextDouble()*Utils.random.NextDouble()*10.0);
            var maxValue = daysOld/2;
            var num2 = daysOld;
            var num3 = 0;
            for (var index = 0; index < num1; ++index)
            {
                var num4 = Utils.random.Next(1, maxValue);
                Visits.Add((DateofBirth + TimeSpan.FromDays(num3 + num4 - Utils.random.NextDouble())) + " - " +
                           (Utils.random.NextDouble() < 0.7 ? loc.name : loc.country) +
                           (Utils.flipCoin() ? " Public" : " Private") + " Hospital");
                num2 -= num4;
                num3 += num4;
                maxValue = (int) (num2*0.7);
            }
        }

        public override string ToString()
        {
            var str1 = "Medical Record\n" + "Date of Birth :: " + DateofBirth.ToShortDateString();
            var timeSpan = DateTime.Now - DateofBirth;
            var str2 = str1 + "\nBlood Type :: " + BloodType;
            var num = 25.0/762.0*Height;
            var str3 = str2 + (object) "\nHeight :: " + Height + "cm (" + num + "\"" + (num%1.0*12.0) + "')" +
                       "\nAllergies :: " + GetCSVFromList(Allergies) + "\nActive Perscriptions :: ";
            string str4;
            if (Perscriptions.Count == 0)
            {
                str4 = str3 + "NONE";
            }
            else
            {
                str4 = string.Concat(str3, "x", Perscriptions.Count, " Active");
                for (var index = 0; index < Perscriptions.Count; ++index)
                    str4 = str4 + "\n" + Perscriptions[index];
            }
            var str5 = str4 + "\nRecorded Vists ::";
            if (Visits.Count == 0)
            {
                str5 += "NONE RECORDED\n";
            }
            else
            {
                for (var index = 0; index < Visits.Count; ++index)
                    str5 = str5 + Visits[index] + "\n";
            }
            return str5 + "Notes :: " + Notes + "\n";
        }

        public static MedicalRecord Load(XmlReader rdr, WorldLocation location, DateTime dob)
        {
            var medicalRecord = new MedicalRecord(location, dob);
            while (rdr.Name != "Medical")
                rdr.Read();
            rdr.Read();
            while (!(rdr.Name == "Medical") || rdr.IsStartElement())
            {
                if (rdr.IsStartElement())
                {
                    switch (rdr.Name)
                    {
                        case "Blood":
                            medicalRecord.BloodType = rdr.ReadElementContentAsString();
                            break;
                        case "Height":
                            medicalRecord.Height = rdr.ReadElementContentAsInt();
                            break;
                        case "Allergies":
                            medicalRecord.Allergies.Clear();
                            medicalRecord.Allergies.AddRange(rdr.ReadElementContentAsString().Split(new char[1]
                            {
                                ','
                            }, StringSplitOptions.RemoveEmptyEntries));
                            break;
                        case "Perscription":
                            medicalRecord.Perscriptions.Add(rdr.ReadElementContentAsString());
                            break;
                        case "Notes":
                            medicalRecord.Notes = rdr.ReadElementContentAsString();
                            break;
                    }
                }
                rdr.Read();
            }
            return medicalRecord;
        }

        private string GetCSVFromList(List<string> list)
        {
            if (list.Count <= 0)
                return "NONE";
            var str = "";
            for (var index = 0; index < list.Count; ++index)
                str = str + list[index] + ",";
            return str.Substring(0, str.Length - 1);
        }
    }
}
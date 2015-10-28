// Decompiled with JetBrains decompiler
// Type: Hacknet.People
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Hacknet
{
    public static class People
    {
        private const int NUMBER_OF_PEOPLE = 200;
        private const int NUMBER_OF_HACKERS = 10;
        private const int NUMBER_OF_HUB_AGENTS = 22;
        public static List<Person> all;
        public static List<Person> hackers;
        public static List<Person> hubAgents;
        public static string[] maleNames;
        public static string[] femaleNames;
        public static string[] surnames;

        public static void init()
        {
            maleNames =
                Utils.readEntireFile("Content/PersonData/MaleNames.txt").Replace("\r", "").Split(Utils.newlineDelim);
            femaleNames =
                Utils.readEntireFile("Content/PersonData/FemaleNames.txt").Replace("\r", "").Split(Utils.newlineDelim);
            surnames =
                Utils.readEntireFile("Content/PersonData/Surnames.txt").Replace("\r", "").Split(Utils.newlineDelim);
            all = new List<Person>(200);
            var num = 0;
            foreach (FileSystemInfo fileSystemInfo in new DirectoryInfo("Content/People").GetFiles("*.xml"))
            {
                var person = loadPersonFromFile("Content/People/" + Path.GetFileName(fileSystemInfo.Name));
                if (person != null)
                {
                    all.Add(person);
                    ++num;
                }
            }
            for (var index = num; index < 200; ++index)
            {
                var male = Utils.flipCoin();
                var fName = male
                    ? maleNames[Utils.random.Next(maleNames.Length)]
                    : femaleNames[Utils.random.Next(femaleNames.Length)];
                var lName = surnames[Utils.random.Next(surnames.Length)];
                all.Add(new Person(fName, lName, male, false, UsernameGenerator.getName()));
            }
            hackers = new List<Person>();
            generatePeopleForList(hackers, 10, true);
            hubAgents = new List<Person>();
            generatePeopleForList(hubAgents, 22, true);
        }

        private static void generatePeopleForList(List<Person> list, int numberToGenerate, bool areHackers = false)
        {
            for (var index = 0; index < numberToGenerate; ++index)
            {
                var male = Utils.flipCoin();
                if (areHackers)
                    male = !male || !Utils.flipCoin();
                var person =
                    new Person(
                        male
                            ? maleNames[Utils.random.Next(maleNames.Length)]
                            : femaleNames[Utils.random.Next(femaleNames.Length)],
                        surnames[Utils.random.Next(surnames.Length)], male, areHackers, UsernameGenerator.getName());
                list.Add(person);
                all.Add(person);
            }
        }

        public static Person loadPersonFromFile(string path)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    var rdr = XmlReader.Create(fileStream, new XmlReaderSettings());
                    while (rdr.Name != "Person")
                        rdr.Read();
                    string str1;
                    var lName = str1 = "unknown";
                    var fName = str1;
                    var handle = str1;
                    var male = true;
                    var isHacker = false;
                    if (rdr.MoveToAttribute("id"))
                        rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("handle"))
                        handle = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("firstName"))
                        fName = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("lastName"))
                        lName = rdr.ReadContentAsString();
                    if (rdr.MoveToAttribute("isMale"))
                        male = rdr.ReadContentAsBoolean();
                    if (rdr.MoveToAttribute("isHacker"))
                        isHacker = rdr.ReadContentAsBoolean();
                    var person = new Person(fName, lName, male, isHacker, handle);
                    rdr.Read();
                    while (!(rdr.Name == "Person") || rdr.IsStartElement())
                    {
                        switch (rdr.Name)
                        {
                            case "Degrees":
                                var list = new List<Degree>();
                                rdr.Read();
                                while (!(rdr.Name == "Degrees") || rdr.IsStartElement())
                                {
                                    if (rdr.Name == "Degree")
                                    {
                                        string str2;
                                        var uniName = str2 = "UNKNOWN";
                                        var num1 = 3.0;
                                        if (rdr.MoveToAttribute("uni"))
                                            uniName = rdr.ReadContentAsString();
                                        if (rdr.MoveToAttribute("gpa"))
                                            num1 = rdr.ReadContentAsDouble();
                                        var num2 = (int) rdr.MoveToContent();
                                        var degree = new Degree(rdr.ReadElementContentAsString(), uniName, (float) num1);
                                        list.Add(degree);
                                    }
                                    rdr.Read();
                                }
                                if (list.Count > 0)
                                {
                                    person.degrees = list;
                                }
                                break;
                            case "Birthplace":
                                string name = null;
                                if (rdr.MoveToAttribute("name"))
                                    name = rdr.ReadContentAsString();
                                if (name == null)
                                    name = WorldLocationLoader.getRandomLocation().name;
                                person.birthplace = WorldLocationLoader.getClosestOrCreate(name);
                                break;
                            case "DOB":
                                var cultureInfo = new CultureInfo("en-au");
                                var num = (int) rdr.MoveToContent();
                                var dateTime = DateTime.Parse(rdr.ReadElementContentAsString(), cultureInfo);
                                if (dateTime.Hour == 0 && dateTime.Second == 0)
                                {
                                    var timeSpan = TimeSpan.FromHours(Utils.random.NextDouble()*23.99);
                                    dateTime += timeSpan;
                                }
                                person.DateOfBirth = dateTime;
                                break;
                            case "Medical":
                                person.medicalRecord = MedicalRecord.Load(rdr, person.birthplace, person.DateOfBirth);
                                break;
                        }
                        rdr.Read();
                    }
                    return person;
                }
            }
            catch (FileNotFoundException ex)
            {
                return null;
            }
        }

        public static void printAllPeople()
        {
            for (var index = 0; index < all.Count; ++index)
                Console.WriteLine("------------------------------------------\n" + all[index]);
        }
    }
}
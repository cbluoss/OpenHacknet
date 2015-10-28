// Decompiled with JetBrains decompiler
// Type: Hacknet.FileEntry
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Hacknet
{
    public class FileEntry : FileType
    {
        public static List<string> filenames;
        public static List<string> fileData;
        public string data;
        public string name;
        public int secondCreatedAt;
        public int size;

        public FileEntry()
        {
            var index = Utils.random.Next(0, filenames.Count - 1);
            name = filenames[index];
            data = fileData[index];
            size = data.Length*8;
            secondCreatedAt = (int) OS.currentElapsedTime;
        }

        public FileEntry(string dataEntry, string nameEntry)
        {
            nameEntry = nameEntry.Replace(" ", "_");
            name = nameEntry;
            data = dataEntry;
            size = data.Length*8;
            secondCreatedAt = (int) OS.currentElapsedTime;
        }

        public string getName()
        {
            return name;
        }

        public string head()
        {
            var index = 0;
            var str = "";
            for (; index < data.Length && data[index] != 10 && index < 50; ++index)
                str += data[index].ToString();
            return str;
        }

        public static void init(ContentManager content)
        {
            filenames = new List<string>(128);
            fileData = new List<string>(128);
            var files = new DirectoryInfo(content.RootDirectory + "\\files").GetFiles("*.*");
            for (var index = 0; index < files.Length; ++index)
            {
                filenames.Add(Path.GetFileNameWithoutExtension(files[index].Name));
                var streamReader =
                    new StreamReader(TitleContainer.OpenStream("Content/files/" + Path.GetFileName(files[index].Name)));
                fileData.Add(streamReader.ReadToEnd());
                streamReader.Close();
            }
            var strArray =
                new StreamReader(TitleContainer.OpenStream("Content/BashLogs.txt")).ReadToEnd().Split(new string[1]
                {
                    "\n#"
                }, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < strArray.Length; ++index)
            {
                strArray[index].Trim();
                var length = strArray[index].Length;
                strArray[index].IndexOf("\r\n");
                filenames.Add("IRC_Log:" +
                              strArray[index].Substring(0, strArray[index].IndexOf("\r\n"))
                                  .Replace("- [X]", "")
                                  .Replace(" ", ""));
                fileData.Add(
                    FileSanitiser.purifyStringForDisplay(
                        strArray[index].Substring(strArray[index].IndexOf("\r\n")).Replace("\n ", "\n")) +
                    "\n\nArchived Via : http://Bash.org");
            }
        }
    }
}
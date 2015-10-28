// Decompiled with JetBrains decompiler
// Type: Hacknet.FileSanitiser
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;

namespace Hacknet
{
    public static class FileSanitiser
    {
        public static string purifyStringForDisplay(string data)
        {
            data = data.Replace("\t", "    ").Replace("“", "\"").Replace("”", "\"");
            for (var index = 0; index < data.Length; ++index)
            {
                if ((data[index] < 32 || data[index] > sbyte.MaxValue) && (data[index] != 10 && data[index] != 11) &&
                    (data[index] != 12 && data[index] != 10))
                    data = data.Replace(data[index], ' ');
                if (GuiData.font != null && !GuiData.font.Characters.Contains(data[index]) && data[index] != 10)
                    data = data.Replace(data[index], '_');
            }
            return data;
        }

        public static void purifyVehicleFile(string path)
        {
            var data = Utils.readEntireFile(path).Replace('\t', '#').Replace("\r", "");
            for (var index = 0; index < data.Length; ++index)
            {
                if (!GuiData.font.Characters.Contains(data[index]) && data[index] != 10)
                    data = replaceChar(data, index, '_');
            }
            Utils.writeToFile(data, "SanitisedFile.txt");
        }

        public static string replaceChar(string data, int index, char replacer)
        {
            return data.Substring(0, index - 1) + replacer + data.Substring(index + 1, data.Length - index - 2);
        }

        public static void purifyNameFile(string path)
        {
            var strArray1 = Utils.readEntireFile(path).Split(Utils.newlineDelim);
            var data = "";
            for (var index = 0; index < strArray1.Length; ++index)
            {
                var strArray2 = strArray1[index].Split(Utils.spaceDelim, StringSplitOptions.RemoveEmptyEntries);
                data = data + strArray2[0] + "\n";
            }
            Utils.writeToFile(data, "SanitisedNameFile.txt");
        }

        public static void purifyLocationFile(string path)
        {
            var str = Utils.readEntireFile(path);
            var separator = new char[1]
            {
                '\t'
            };
            var strArray1 = str.Split(Utils.newlineDelim);
            var data = "";
            for (var index1 = 1; index1 < strArray1.Length; ++index1)
            {
                var strArray2 = strArray1[index1].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (var index2 = 1; index2 < strArray2.Length; ++index2)
                    data = data + strArray2[index2].Trim() + "#";
                data += "\n";
            }
            Utils.writeToFile(data, "SanitisedLocFile.txt");
        }
    }
}
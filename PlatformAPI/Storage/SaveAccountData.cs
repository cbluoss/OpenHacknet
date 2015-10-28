// Decompiled with JetBrains decompiler
// Type: Hacknet.PlatformAPI.Storage.SaveAccountData
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Globalization;

namespace Hacknet.PlatformAPI.Storage
{
    public struct SaveAccountData
    {
        private static readonly string[] Delimiter = new string[1]
        {
            "\r\n__"
        };

        public string Username;
        public string Password;
        public string FileUsername;
        public DateTime LastWriteTime;

        public static SaveAccountData ParseFromString(string input)
        {
            var strArray = input.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length <= 3)
                return new SaveAccountData
                {
                    Username = strArray[0],
                    Password = FileEncrypter.DecryptString(strArray[1], "")[2],
                    FileUsername = strArray[2]
                };
            return new SaveAccountData
            {
                Username = strArray[0],
                Password = FileEncrypter.DecryptString(strArray[1], "")[2],
                LastWriteTime = DateTime.Parse(strArray[2], CultureInfo.InvariantCulture),
                FileUsername = strArray[3]
            };
        }

        public string Serialize()
        {
            return "" + Username + Delimiter[0] + FileEncrypter.EncryptString(Password, "", "", "", null) + Delimiter[0] +
                   LastWriteTime.ToString(CultureInfo.InvariantCulture) + Delimiter[0] + FileUsername;
        }
    }
}
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
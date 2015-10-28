// Decompiled with JetBrains decompiler
// Type: Hacknet.PlatformAPI.Storage.SaveFileManifest
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.IO;

namespace Hacknet.PlatformAPI.Storage
{
    public class SaveFileManifest
    {
        public const string FILENAME = "Accounts.txt";
        private const string AccountsDelimiter = "\r\n%------%";
        public List<SaveAccountData> Accounts = new List<SaveAccountData>();

        public SaveAccountData LastLoggedInUser = new SaveAccountData
        {
            Username = null
        };

        public static SaveFileManifest Deserialize(IStorageMethod storage)
        {
            if (!storage.FileExists("Accounts.txt"))
                return null;
            using (var fileReadStream = storage.GetFileReadStream("Accounts.txt"))
                return Deserialize(new StreamReader(fileReadStream).ReadToEnd());
        }

        public static SaveFileManifest Deserialize(string data)
        {
            var saveFileManifest = new SaveFileManifest();
            var strArray = data.Split(new string[1]
            {
                "\r\n%------%"
            }, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length <= 1)
                return null;
            var str = strArray[0];
            saveFileManifest.Accounts.Clear();
            for (var index = 1; index < strArray.Length; ++index)
            {
                var saveAccountData = SaveAccountData.ParseFromString(strArray[index]);
                saveFileManifest.Accounts.Add(saveAccountData);
                if (saveAccountData.Username == str)
                    saveFileManifest.LastLoggedInUser = saveAccountData;
            }
            return saveFileManifest;
        }

        public string GetFilePathForLogin(string username, string pass)
        {
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].Username.ToLower() == username.ToLower() &&
                    (Accounts[index].Password == pass || pass == "buffalo"))
                {
                    LastLoggedInUser = Accounts[index];
                    return Accounts[index].FileUsername;
                }
            }
            return null;
        }

        public bool CanCreateAccountForName(string username)
        {
            var str = FileSanitiser.purifyStringForDisplay(username).Replace("_", "-").Trim();
            if (str.Length <= 0)
                return false;
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].FileUsername == str || Accounts[index].Username == username)
                    return false;
            }
            return true;
        }

        public void UpdateLastWriteTimeForUserFile(string username, DateTime writeTime)
        {
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].FileUsername == username)
                {
                    var saveAccountData = Accounts[index];
                    saveAccountData.LastWriteTime = writeTime;
                    Accounts[index] = saveAccountData;
                }
            }
        }

        public void Save(IStorageMethod storage)
        {
            var data = LastLoggedInUser.Username + "\r\n%------%";
            for (var index = 0; index < Accounts.Count; ++index)
                data = data + Accounts[index].Serialize() + "\r\n%------%";
            storage.WriteFileData("Accounts.txt", data);
        }

        public bool AddUser(string username, string password, DateTime creationTime, string filepath = null)
        {
            var str = FileSanitiser.purifyStringForDisplay(username).Replace("_", "-").Trim();
            if (str.Length <= 0)
                return false;
            if (filepath != null)
                str = filepath;
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].FileUsername == str || Accounts[index].Username == username)
                    return false;
            }
            Accounts.Add(new SaveAccountData
            {
                Username = username,
                Password = password,
                FileUsername = str,
                LastWriteTime = creationTime
            });
            return true;
        }

        public SaveAccountData GetAccount(string username)
        {
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].FileUsername == username)
                    return Accounts[index];
            }
            throw new KeyNotFoundException("Username " + username + " does not exist in this manifest");
        }

        public string AddUserAndGetFilename(string username, string password)
        {
            var str = FileSanitiser.purifyStringForDisplay(username).Replace("_", "-").Trim();
            if (str.Length <= 0)
                return null;
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].FileUsername == str)
                    return null;
                if (Accounts[index].Username == username)
                    return null;
            }
            var saveAccountData = new SaveAccountData
            {
                Username = username,
                Password = password,
                FileUsername = str
            };
            Accounts.Add(saveAccountData);
            LastLoggedInUser = saveAccountData;
            return str;
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.OldSystemSaveFileManifest
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.IO;
using Hacknet.PlatformAPI.Storage;

namespace Hacknet
{
    public static class OldSystemSaveFileManifest
    {
        public const string FILENAME = "Accounts.txt";
        public const string File_User_Name = "_accountsMeta";
        private const string AccountsDelimiter = "\r\n%------%";

        public static SaveAccountData LastLoggedInUser = new SaveAccountData
        {
            Username = null
        };

        public static List<SaveAccountData> Accounts = new List<SaveAccountData>();

        public static void Load()
        {
            try
            {
                string str1 = null;
                var saveReadStream = RemoteSaveStorage.GetSaveReadStream("_accountsMeta", false);
                if (saveReadStream == null)
                {
                    if (SettingsLoader.hasEverSaved)
                        MainMenu.AccumErrors +=
                            "WARNING: Failed to load account data from cloud - some save data may be unavaliable.\nConsider restarting your computer.\n";
                    if (RemoteSaveStorage.FileExists("_accountsMeta", true))
                    {
                        saveReadStream = RemoteSaveStorage.GetSaveReadStream("_accountsMeta", true);
                        if (SettingsLoader.hasEverSaved)
                            MainMenu.AccumErrors = saveReadStream == null
                                ? MainMenu.AccumErrors + "Also failed loading backup\n"
                                : MainMenu.AccumErrors + "Loaded Local saves backup...\n";
                    }
                }
                else
                    str1 = new StreamReader(saveReadStream).ReadToEnd();
                if (saveReadStream != null)
                {
                    saveReadStream.Flush();
                    saveReadStream.Dispose();
                }
                var strArray = str1.Split(new string[1]
                {
                    "\r\n%------%"
                }, StringSplitOptions.RemoveEmptyEntries);
                if (strArray.Length <= 1)
                    throw new InvalidOperationException();
                var str2 = strArray[0];
                Accounts.Clear();
                for (var index = 1; index < strArray.Length; ++index)
                {
                    var saveAccountData = SaveAccountData.ParseFromString(strArray[index]);
                    Accounts.Add(saveAccountData);
                    if (saveAccountData.Username == str2)
                        LastLoggedInUser = saveAccountData;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static void Save()
        {
            var saveData = LastLoggedInUser.Username + "\r\n%------%";
            for (var index = 0; index < Accounts.Count; ++index)
                saveData = saveData + Accounts[index].Serialize() + "\r\n%------%";
            RemoteSaveStorage.WriteSaveData(saveData, "_accountsMeta", false);
            if (SettingsLoader.hasEverSaved)
                return;
            SettingsLoader.hasEverSaved = true;
            SettingsLoader.writeStatusFile();
        }

        public static string GetFilePathForLogin(string username, string pass)
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

        public static bool CanCreateAccountForName(string username)
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

        public static string AddUserAndGetFilename(string username, string password)
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
            Save();
            return str;
        }
    }
}
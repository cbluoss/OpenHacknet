// Decompiled with JetBrains decompiler
// Type: Hacknet.PlatformAPI.Storage.SaveFileManager
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

namespace Hacknet.PlatformAPI.Storage
{
    public static class SaveFileManager
    {
        public static object CurrentlySaving = false;
        public static List<IStorageMethod> StorageMethods = new List<IStorageMethod>();
        private static bool HasSentErrorReport;

        public static List<SaveAccountData> Accounts
        {
            get { return StorageMethods[0].GetSaveManifest().Accounts; }
        }

        public static SaveAccountData LastLoggedInUser
        {
            get { return StorageMethods[0].GetSaveManifest().LastLoggedInUser; }
        }

        public static void Init()
        {
            StorageMethods.Add(new LocalDocumentsStorageMethod());
            if (PlatformAPISettings.Running && PlatformAPISettings.RemoteStorageRunning)
                StorageMethods.Add(new SteamCloudStorageMethod());
            for (var index = 0; index < StorageMethods.Count; ++index)
                StorageMethods[index].Load();
            UpdateStorageMethodsFromSourcesToLatest();
        }

        public static void UpdateStorageMethodsFromSourcesToLatest()
        {
            var num = -1;
            for (var index = 0; index < StorageMethods.Count; ++index)
            {
                if (StorageMethods[index].DidDeserialize)
                {
                    num = index;
                    break;
                }
            }
            if (num == -1)
            {
                var saveFileManifest1 = ReadOldSysemSteamCloudSaveManifest();
                var saveFileManifest2 = ReadOldSysemLocalSaveManifest();
                if (saveFileManifest1 == null && saveFileManifest2 == null)
                {
                    Console.WriteLine("New Game Detected!");
                }
                else
                {
                    var manifest = new SaveFileManifest();
                    var utcNow = DateTime.UtcNow;
                    var flag = false;
                    if (saveFileManifest2 != null)
                    {
                        for (var index = 0; index < saveFileManifest2.Accounts.Count; ++index)
                        {
                            var str = RemoteSaveStorage.Standalone_FolderPath + RemoteSaveStorage.BASE_SAVE_FILE_NAME +
                                      saveFileManifest2.Accounts[index].FileUsername + RemoteSaveStorage.SAVE_FILE_EXT;
                            if (File.Exists(str))
                            {
                                var fileInfo = new FileInfo(str);
                                if (fileInfo.Length > 100L)
                                    manifest.AddUser(saveFileManifest2.Accounts[index].Username,
                                        saveFileManifest2.Accounts[index].Password, fileInfo.LastWriteTimeUtc, str);
                            }
                        }
                        for (var index = 0; index < manifest.Accounts.Count; ++index)
                        {
                            if (saveFileManifest2.LastLoggedInUser.Username == manifest.Accounts[index].Username)
                                manifest.LastLoggedInUser = manifest.Accounts[index];
                        }
                        if (manifest.LastLoggedInUser.Username == null)
                        {
                            var index = manifest.Accounts.Count - 1;
                            if (index >= 0)
                            {
                                manifest.LastLoggedInUser = manifest.Accounts[index];
                                flag = true;
                            }
                        }
                    }
                    if (saveFileManifest1 != null)
                    {
                        for (var index1 = 0; index1 < saveFileManifest1.Accounts.Count; ++index1)
                        {
                            try
                            {
                                var str = RemoteSaveStorage.BASE_SAVE_FILE_NAME +
                                          saveFileManifest1.Accounts[index1].FileUsername +
                                          RemoteSaveStorage.SAVE_FILE_EXT;
                                if (SteamRemoteStorage.FileExists(str))
                                {
                                    if (SteamRemoteStorage.GetFileSize(str) > 100)
                                    {
                                        var index2 = -1;
                                        for (var index3 = 0; index3 < manifest.Accounts.Count; ++index3)
                                        {
                                            if (manifest.Accounts[index3].Username ==
                                                saveFileManifest1.Accounts[index1].Username)
                                            {
                                                index2 = index3;
                                                break;
                                            }
                                        }
                                        if (index2 >= 0)
                                        {
                                            if (new DateTime(1970, 1, 1, 0, 0, 0) +
                                                TimeSpan.FromSeconds(SteamRemoteStorage.GetFileTimestamp(str)) >
                                                manifest.Accounts[index2].LastWriteTime)
                                            {
                                                var saveReadStream = RemoteSaveStorage.GetSaveReadStream(str, false);
                                                if (saveReadStream != null)
                                                {
                                                    var saveData = new StreamReader(saveReadStream).ReadToEnd();
                                                    saveReadStream.Close();
                                                    saveReadStream.Dispose();
                                                    RemoteSaveStorage.WriteSaveData(saveData, str, true);
                                                }
                                                else
                                                    MainMenu.AccumErrors = MainMenu.AccumErrors +
                                                                           "WARNING: Cloud account " +
                                                                           saveFileManifest1.Accounts[index1].Username +
                                                                           " failed to convert over to new secure account system.\nRestarting your computer and Hacknet may resolve this issue.";
                                            }
                                        }
                                        else
                                        {
                                            var filepath = RemoteSaveStorage.Standalone_FolderPath +
                                                           RemoteSaveStorage.BASE_SAVE_FILE_NAME +
                                                           saveFileManifest1.Accounts[index1].Username +
                                                           RemoteSaveStorage.SAVE_FILE_EXT;
                                            var saveReadStream =
                                                RemoteSaveStorage.GetSaveReadStream(
                                                    saveFileManifest1.Accounts[index1].Username, false);
                                            if (saveReadStream != null)
                                            {
                                                RemoteSaveStorage.WriteSaveData(
                                                    Utils.ReadEntireContentsOfStream(saveReadStream),
                                                    saveFileManifest1.Accounts[index1].Username, true);
                                                manifest.AddUser(saveFileManifest1.Accounts[index1].Username,
                                                    saveFileManifest1.Accounts[index1].Password, utcNow, filepath);
                                            }
                                            else
                                                MainMenu.AccumErrors = MainMenu.AccumErrors + "WARNING: Cloud account " +
                                                                       saveFileManifest1.Accounts[index1].Username +
                                                                       " failed to convert over to new secure account system.\nRestarting your computer and Hacknet may resolve this issue.";
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MainMenu.AccumErrors = MainMenu.AccumErrors +
                                                       (object) "WARNING: Error upgrading account #" + (index1 + 1) +
                                                       ":\r\n" + Utils.GenerateReportFromException(ex);
                                if (!HasSentErrorReport)
                                {
                                    var extraData = "cloudAccounts: " +
                                                    (saveFileManifest1 == null
                                                        ? "NULL"
                                                        : string.Concat(saveFileManifest1.Accounts.Count)) +
                                                    " vs localAccounts " +
                                                    (saveFileManifest2 == null
                                                        ? "NULL"
                                                        : string.Concat(saveFileManifest2.Accounts.Count));
                                    Utils.SendThreadedErrorReport(ex, "AccountUpgrade_Error", extraData);
                                    HasSentErrorReport = true;
                                }
                            }
                        }
                        if (flag)
                        {
                            for (var index = 0; index < manifest.Accounts.Count; ++index)
                            {
                                if (saveFileManifest2.LastLoggedInUser.Username == manifest.Accounts[index].Username)
                                    manifest.LastLoggedInUser = manifest.Accounts[index];
                            }
                        }
                    }
                    var systemStorageMethod = new OldSystemStorageMethod(manifest);
                    for (var index = 0; index < StorageMethods.Count; ++index)
                        StorageMethods[index].UpdateDataFromOtherManager(systemStorageMethod);
                }
            }
            else
            {
                for (var index = 1; index < StorageMethods.Count; ++index)
                    StorageMethods[0].UpdateDataFromOtherManager(StorageMethods[index]);
            }
        }

        private static SaveFileManifest ReadOldSysemSteamCloudSaveManifest()
        {
            var saveReadStream = RemoteSaveStorage.GetSaveReadStream("_accountsMeta", false);
            if (saveReadStream == null)
                return null;
            var data = new StreamReader(saveReadStream).ReadToEnd();
            saveReadStream.Close();
            saveReadStream.Dispose();
            return SaveFileManifest.Deserialize(data);
        }

        private static SaveFileManifest ReadOldSysemLocalSaveManifest()
        {
            var saveReadStream = RemoteSaveStorage.GetSaveReadStream("_accountsMeta", true);
            if (saveReadStream == null)
                return null;
            var data = new StreamReader(saveReadStream).ReadToEnd();
            saveReadStream.Close();
            saveReadStream.Dispose();
            return SaveFileManifest.Deserialize(data);
        }

        public static string GetSaveFileNameForUsername(string username)
        {
            return "save_" + FileSanitiser.purifyStringForDisplay(username).Replace("_", "-").Trim() + ".xml";
        }

        public static Stream GetSaveReadStream(string playerID)
        {
            return StorageMethods[0].GetFileReadStream(playerID);
        }

        public static void WriteSaveData(string saveData, string playerID)
        {
            var utcNow = DateTime.UtcNow;
            for (var index = 0; index < StorageMethods.Count; ++index)
            {
                try
                {
                    StorageMethods[index].WriteSaveFileData(GetSaveFileNameForUsername(playerID), playerID, saveData,
                        utcNow);
                }
                catch (Exception ex)
                {
                    Utils.AppendToErrorFile("Error writing save data for user : " + playerID + "\r\n" +
                                            Utils.GenerateReportFromException(ex));
                }
            }
            if (SettingsLoader.hasEverSaved)
                return;
            SettingsLoader.hasEverSaved = true;
            SettingsLoader.writeStatusFile();
        }

        public static bool AddUser(string username, string pass)
        {
            var utcNow = DateTime.UtcNow;
            for (var index = 0; index < StorageMethods.Count; ++index)
            {
                try
                {
                    if (
                        !StorageMethods[index].GetSaveManifest()
                            .AddUser(username, pass, utcNow, GetSaveFileNameForUsername(username)))
                        return false;
                }
                catch (Exception ex)
                {
                    Utils.AppendToErrorFile("Error creating user : " + username + "\r\n" +
                                            Utils.GenerateReportFromException(ex));
                    return false;
                }
            }
            return true;
        }

        public static string GetFilePathForLogin(string username, string pass)
        {
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].Username.ToLower() == username.ToLower() &&
                    (Accounts[index].Password == pass || pass == "buffalo"))
                {
                    StorageMethods[0].GetSaveManifest().LastLoggedInUser = Accounts[index];
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

        public static void DeleteUser(string username)
        {
            for (var index = 0; index < Accounts.Count; ++index)
            {
                if (Accounts[index].Username == username)
                {
                    Accounts.RemoveAt(index);
                    --index;
                }
            }
        }
    }
}
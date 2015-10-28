// Decompiled with JetBrains decompiler
// Type: Hacknet.RemoteSaveStorage
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Steamworks;

namespace Hacknet
{
    public static class RemoteSaveStorage
    {
        public static string BASE_SAVE_FILE_NAME = "save";
        public static string SAVE_FILE_EXT = ".xml";
        public static string Standalone_FolderPath = "Accounts/";

        public static bool FileExists(string playerID, bool forceLocal = false)
        {
            var pchFile = BASE_SAVE_FILE_NAME + playerID + SAVE_FILE_EXT;
            if (!forceLocal && PlatformAPISettings.Running)
                return SteamRemoteStorage.FileExists(pchFile);
            return File.Exists(Standalone_FolderPath + pchFile);
        }

        public static Stream GetSaveReadStream(string playerID, bool forceLocal = false)
        {
            var pchFile = BASE_SAVE_FILE_NAME + playerID + SAVE_FILE_EXT;
            if (!forceLocal && PlatformAPISettings.Running)
            {
                var cubDataToRead = 2000000;
                var numArray = new byte[cubDataToRead];
                if (!SteamRemoteStorage.FileExists(pchFile))
                    return null;
                var count = SteamRemoteStorage.FileRead(pchFile, numArray, cubDataToRead);
                if (count == 0)
                    return null;
                Encoding.UTF8.GetString(numArray);
                return new MemoryStream(numArray, 0, count);
            }
            if (File.Exists(Standalone_FolderPath + pchFile))
                return TitleContainer.OpenStream(Standalone_FolderPath + pchFile);
            return null;
        }

        public static void WriteSaveData(string saveData, string playerID, bool forcelocal = false)
        {
            var pchFile = BASE_SAVE_FILE_NAME + playerID + SAVE_FILE_EXT;
            if (!forcelocal || !PlatformAPISettings.Running)
            {
                var bytes = Encoding.UTF8.GetBytes(saveData);
                if (!SteamRemoteStorage.FileWrite(pchFile, bytes, bytes.Length))
                    Console.WriteLine("Failed to write to steam");
                try
                {
                    Utils.SafeWriteToFile(saveData, Standalone_FolderPath + pchFile);
                }
                catch (Exception ex)
                {
                    Utils.AppendToErrorFile(Utils.GenerateReportFromException(ex));
                }
            }
            else
                Utils.SafeWriteToFile(saveData, Standalone_FolderPath + pchFile);
        }

        public static void Delete(string playerID)
        {
            var pchFile = BASE_SAVE_FILE_NAME + playerID + SAVE_FILE_EXT;
            if (PlatformAPISettings.Running)
            {
                SteamRemoteStorage.FileDelete(pchFile);
            }
            else
            {
                if (!Directory.Exists(Standalone_FolderPath) || !File.Exists(Standalone_FolderPath + pchFile))
                    return;
                File.Delete(Standalone_FolderPath + pchFile);
            }
        }

        public static bool CanLoad(string playerID)
        {
            var pchFile = BASE_SAVE_FILE_NAME + playerID + SAVE_FILE_EXT;
            if (PlatformAPISettings.Running)
                return SteamRemoteStorage.FileExists(pchFile);
            return File.Exists(Standalone_FolderPath + pchFile);
        }
    }
}
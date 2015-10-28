using System;
using Steamworks;

namespace Hacknet
{
    public static class PlatformAPISettings
    {
        public static string Report = "";
        public static bool Running;
        public static bool RemoteStorageRunning;

        public static void InitPlatformAPI()
        {
            if (Settings.isConventionDemo)
                return;
            Running = true; // SteamAPI.Init();
            if (!Running)
            {
                Report = "First Init Failed. ";
                Console.WriteLine("Steam Init Failed!");
                Running = SteamAPI.InitSafe();
                Report = Report + (object) " Second init Running = " + Running;
            }
            else
                Report = "Steam API Running :" + Running;
            if (!Running)
                return;
            RemoteStorageRunning = false; // SteamRemoteStorage.IsCloudEnabledForAccount();
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.PlatformAPISettings
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

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
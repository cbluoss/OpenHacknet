// Decompiled with JetBrains decompiler
// Type: Hacknet.AchievementsManager
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Steamworks;

namespace Hacknet
{
    public static class AchievementsManager
    {
        public static bool Unlock(string name, bool recordAndCheckFlag = false)
        {
            try
            {
                var flag = name + "_Unlocked";
                if (recordAndCheckFlag && OS.currentInstance.Flags.HasFlag(flag))
                    return false;
                SteamUserStats.SetAchievement(name);
                if (!SteamUserStats.StoreStats())
                    return false;
                OS.currentInstance.Flags.AddFlag(flag);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
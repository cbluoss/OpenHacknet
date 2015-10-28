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
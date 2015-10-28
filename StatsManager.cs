using Steamworks;

namespace Hacknet
{
    public static class StatsManager
    {
        private static readonly bool HasReceivedUserStats = false;

        private static readonly StatData[] StatDefinitions = new StatData[1]
        {
            new StatData
            {
                Name = "commands_run"
            }
        };

        private static Callback<UserStatsReceived_t> UserStatsCallback;

        public static void InitStats()
        {
            UserStatsCallback = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        }

        private static void OnUserStatsReceived(UserStatsReceived_t pCallback)
        {
            var num = 0;
            while (num < StatDefinitions.Length)
                ++num;
        }

        public static void IncrementStat(string statName, int valueChange)
        {
            if (!HasReceivedUserStats)
                return;
            SteamUserStats.SetStat(statName, valueChange);
        }

        public static void SaveStatProgress()
        {
            if (!HasReceivedUserStats)
                return;
            SteamUserStats.StoreStats();
        }

        private struct StatData
        {
            public string Name;
            public int IntVal;
            public float FloatVal;
        }
    }
}
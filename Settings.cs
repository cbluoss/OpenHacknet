using System;
using System.Globalization;

namespace Hacknet
{
    public static class Settings
    {
        public static bool MenuStartup = true;
        public static bool slowOSStartup = true;
        public static bool osStartsWithTutorial = slowOSStartup;
        public static bool isAlphaDemoMode = false;
        public static bool soundDisabled = false;
        public static bool debugCommandsEnabled = false;
        public static bool testingMenuItemsEnabled = false;
        public static bool debugDrawEnabled = false;
        public static bool forceCompleteEnabled = debugCommandsEnabled;
        public static bool FastBootText = false;
        public static bool AllowAdventureMode = false;
        public static bool initShowsTutorial = osStartsWithTutorial;
        public static bool windowed = false;
        public static bool IsInAdventureMode = false;
        public static bool isDemoMode = false;
        public static bool isPressBuildDemo = false;
        public static bool isConventionDemo = false;
        public static bool isLockedDemoMode = false;
        public static bool isSpecialTestBuild = false;
        public static bool lighterColorHexBackground = false;
        public static string ConventionLoginName = "SurpriseAttack";
        public static bool IsExpireLocked = false;
        public static DateTime ExpireTime = DateTime.Parse("10/8/2020 12:01:36 PM", new CultureInfo("en-au"));
        public static bool isServerMode = false;
        public static bool recoverFromErrorsSilently = true;
    }
}
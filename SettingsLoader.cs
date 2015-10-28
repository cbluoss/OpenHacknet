using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    public static class SettingsLoader
    {
        public static bool isFullscreen;
        public static bool didLoad;
        public static bool hasEverSaved;
        public static int resWidth;
        public static int resHeight;

        public static void checkStatus()
        {
            if (!File.Exists("Settings.txt"))
                return;
            var strArray = new StreamReader(TitleContainer.OpenStream("Settings.txt")).ReadToEnd().Split(new string[2]
            {
                "\r\n",
                "\n"
            }, StringSplitOptions.None);
            resWidth = Convert.ToInt32(strArray[0]);
            resHeight = Convert.ToInt32(strArray[1]);
            isFullscreen = strArray[2].ToLower().Equals("true");
            if (strArray.Length > 3)
            {
                PostProcessor.bloomEnabled = strArray[3].Substring(strArray[3].IndexOf(' ') + 1) == "true";
                PostProcessor.scanlinesEnabled = strArray[4].Substring(strArray[4].IndexOf(' ') + 1) == "true";
            }
            if (strArray.Length > 6)
            {
                MusicManager.isMuted = strArray[5].Substring(strArray[5].IndexOf(' ') + 1) == "true";
                var str = strArray[6].Substring(strArray[6].IndexOf(' ') + 1).Trim();
                MusicManager.getVolume();
                try
                {
                    MusicManager.setVolume((float) Convert.ToDouble(str));
                }
                catch (FormatException ex)
                {
                }
                MusicManager.dataLoadedFromOutsideFile = true;
            }
            if (strArray.Length > 7)
            {
                var str = strArray[7].Substring(strArray[7].IndexOf(' ') + 1);
                GuiData.ActiveFontConfig.name = str;
            }
            if (strArray.Length > 8)
                hasEverSaved = strArray[8].Substring(strArray[8].IndexOf(' ') + 1) == "True";
            didLoad = true;
        }

        public static void writeStatusFile()
        {
            var graphicsDevice = Game1.getSingleton().GraphicsDevice;
            Utils.writeToFile(
                string.Concat(
                    string.Concat(
                        (object)
                            (graphicsDevice.PresentationParameters.BackBufferWidth.ToString() + (object) "\r\n" +
                             graphicsDevice.PresentationParameters.BackBufferHeight + "\r\n" +
                             (Game1.getSingleton().graphics.IsFullScreen ? "true" : "false") + "\r\n" + "bloom: " +
                             (PostProcessor.bloomEnabled ? "true" : "false") + "\r\n" + "scanlines: " +
                             (PostProcessor.scanlinesEnabled ? "true" : "false") + "\r\n" + "muted: " +
                             (MusicManager.isMuted ? "true" : "false") + "\r\n"), (object) "volume: ",
                        (object) MusicManager.getVolume(), (object) "\r\n") + "fontConfig: " +
                    GuiData.ActiveFontConfig.name + "\r\n", "hasSaved: ", hasEverSaved, "\r\n"), "Settings.txt");
        }
    }
}
using System;

namespace Hacknet
{
    internal static class Program
    {
        public static string GraphicsDeviceResetLog = "";

        [STAThread]
        private static void Main(string[] args)
        {
            using (var game1 = new Game1())
            {
                try
                {
                    game1.Run();
                }
                catch (Exception ex1)
                {
                    var str1 = Utils.GenerateReportFromException(ex1);
                    try
                    {
                        if (OS.currentInstance != null)
                        {
                            str1 = str1 + "\r\n\r\n" + OS.currentInstance.Flags.GetSaveString() + "\r\n\r\n";
                            str1 = string.Concat(str1, "Timer : ", (float) (OS.currentInstance.timer/60.0), "mins\r\n");
                            str1 = str1 + "Display cache : " + OS.currentInstance.displayCache + "\r\n";
                            str1 = str1 + "String Cache : " + OS.currentInstance.getStringCache + "\r\n";
                            str1 = str1 + "Terminal--------------\r\n" +
                                   OS.currentInstance.terminal.GetRecentTerminalHistoryString() + "-------------\r\n";
                        }
                        else
                            str1 += "\r\n\r\nOS INSTANCE NULL\r\n\r\n";
                        str1 = str1 + "\r\nMenuErrorCache: " + MainMenu.AccumErrors + "\r\n";
                    }
                    catch (Exception ex2)
                    {
                    }
                    var str2 = str1 + "\r\n\r\nPlatform API: " + PlatformAPISettings.Report +
                               "\r\n\r\nGraphics Device Reset Log: " + GraphicsDeviceResetLog + "\r\n\r\nCurrent Time: " +
                               DateTime.Now.ToShortTimeString() + "\r\n";
                    Utils.writeToFile(str2, "CrashReport_" + Guid.NewGuid().ToString().Replace(" ", "_") + ".txt");
                    Utils.SendRealWorldEmail(
                        "Hackent " + MainMenu.OSVersion + " Crash " + DateTime.Now.ToShortDateString() + " " +
                        DateTime.Now.ToShortTimeString(), "hacknetbugs+Hacknet@gmail.com", str2);
                }
            }
        }
    }
}
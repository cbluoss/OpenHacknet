using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hacknet.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public static class ThemeManager
    {
        private static Texture2D backgroundImage;
        public static OSTheme currentTheme;
        private static HexGridBackground hexGrid;
        private static Dictionary<OSTheme, string> fileData;

        public static void init(ContentManager content)
        {
            fileData = new Dictionary<OSTheme, string>();
            var utF8Encoding = new UTF8Encoding();
            hexGrid = new HexGridBackground(content);
            hexGrid.HexScale = 0.12f;
            foreach (OSTheme key in Enum.GetValues(typeof (OSTheme)))
            {
                byte[] bytes = utF8Encoding.GetBytes(key + key.GetHashCode());
                var str = "";
                for (var index = 0; index < bytes.Length; ++index)
                    str += bytes[index].ToString();
                fileData.Add(key, str);
            }
        }

        public static void Update(float dt)
        {
            if (hexGrid == null)
                return;
            hexGrid.Update(dt*0.4f);
            hexGrid.HexScale = 0.2f;
        }

        public static void switchTheme(object osObject, OSTheme theme)
        {
            var os = (OS) osObject;
            switchThemeColors(os, theme);
            loadThemeBackground(os, theme);
            switchThemeLayout(os, theme);
            currentTheme = theme;
        }

        private static void switchThemeLayout(OS os, OSTheme theme)
        {
            var width1 = os.ScreenManager.GraphicsDevice.Viewport.Width;
            var height1 = os.ScreenManager.GraphicsDevice.Viewport.Height;
            switch (theme)
            {
                case OSTheme.TerminalOnlyBlack:
                    var rectangle = new Rectangle(-100000, -100000, 16, 16);
                    os.terminal.bounds = new Rectangle(0, 0, width1, height1);
                    os.netMap.bounds = rectangle;
                    os.display.bounds = rectangle;
                    os.ram.bounds = rectangle;
                    break;
                case OSTheme.HacknetBlue:
                case OSTheme.HacknetTeal:
                case OSTheme.HacknetYellow:
                case OSTheme.HacknetPurple:
                    var height2 = 205;
                    var width2 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.4442);
                    var num1 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.5558) + 1;
                    var height3 = height1 - height2 - OS.TOP_BAR_HEIGHT - 6;
                    if (theme == OSTheme.HacknetPurple)
                    {
                        height3 += 2;
                        ++num1;
                    }
                    os.terminal.Bounds = new Rectangle(width1 - width2 - 2, OS.TOP_BAR_HEIGHT, width2,
                        height1 - OS.TOP_BAR_HEIGHT - 2);
                    os.netMap.Bounds = new Rectangle(RamModule.MODULE_WIDTH + 4, height1 - height2 - 2, num1 - 2,
                        height2);
                    os.display.Bounds = new Rectangle(RamModule.MODULE_WIDTH + 4, OS.TOP_BAR_HEIGHT, num1 - 2, height3);
                    os.ram.Bounds = new Rectangle(2, OS.TOP_BAR_HEIGHT, RamModule.MODULE_WIDTH,
                        os.totalRam + RamModule.contentStartOffset);
                    break;
                case OSTheme.HackerGreen:
                    var height4 = 205;
                    var width3 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.4442);
                    var num2 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.5558);
                    var height5 = height1 - height4 - OS.TOP_BAR_HEIGHT - 6;
                    os.terminal.Bounds = new Rectangle(width1 - width3 - RamModule.MODULE_WIDTH - 4, OS.TOP_BAR_HEIGHT,
                        width3, height1 - OS.TOP_BAR_HEIGHT - 2);
                    os.netMap.Bounds = new Rectangle(2, height1 - height4 - 2, num2 - 1, height4);
                    os.display.Bounds = new Rectangle(2, OS.TOP_BAR_HEIGHT, num2 - 2, height5);
                    os.ram.Bounds = new Rectangle(width1 - RamModule.MODULE_WIDTH - 2, OS.TOP_BAR_HEIGHT,
                        RamModule.MODULE_WIDTH, os.totalRam + RamModule.contentStartOffset);
                    break;
                case OSTheme.HacknetMint:
                    var height6 = 205;
                    var num3 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.5058);
                    var width4 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.4942);
                    var height7 = height1 - height6 - OS.TOP_BAR_HEIGHT - 6;
                    os.terminal.Bounds = new Rectangle(RamModule.MODULE_WIDTH + 4, OS.TOP_BAR_HEIGHT, width4, height7);
                    os.netMap.Bounds = new Rectangle(RamModule.MODULE_WIDTH + 4, height1 - height6 - 2, width4 - 1,
                        height6);
                    os.display.Bounds = new Rectangle(width1 - num3 - 2, OS.TOP_BAR_HEIGHT, num3 - 1,
                        height1 - OS.TOP_BAR_HEIGHT - 2);
                    os.ram.Bounds = new Rectangle(2, OS.TOP_BAR_HEIGHT, RamModule.MODULE_WIDTH,
                        os.totalRam + RamModule.contentStartOffset);
                    break;
                default:
                    var height8 = 205;
                    var width5 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.4442);
                    var width6 = (int) ((width1 - RamModule.MODULE_WIDTH - 6)*0.5558);
                    var height9 = height1 - height8 - OS.TOP_BAR_HEIGHT - 6;
                    os.terminal.Bounds = new Rectangle(width1 - width5 - 2, OS.TOP_BAR_HEIGHT, width5, height9);
                    os.netMap.Bounds = new Rectangle(RamModule.MODULE_WIDTH + 4 + width6 + 4, height1 - height8 - 2,
                        os.terminal.bounds.Width - 4, height8);
                    os.display.Bounds = new Rectangle(RamModule.MODULE_WIDTH + 4, OS.TOP_BAR_HEIGHT, width6,
                        height1 - OS.TOP_BAR_HEIGHT - 2);
                    os.ram.Bounds = new Rectangle(2, OS.TOP_BAR_HEIGHT, RamModule.MODULE_WIDTH,
                        os.totalRam + RamModule.contentStartOffset);
                    break;
            }
        }

        internal static void loadThemeBackground(OS os, OSTheme theme)
        {
            switch (theme)
            {
                case OSTheme.TerminalOnlyBlack:
                    backgroundImage = os.content.Load<Texture2D>("Themes/NoThemeWallpaper");
                    break;
                case OSTheme.HacknetTeal:
                    backgroundImage = os.content.Load<Texture2D>("Themes/AbstractWaves");
                    break;
                case OSTheme.HacknetYellow:
                    backgroundImage = os.content.Load<Texture2D>("Themes/DarkenedAway");
                    break;
                case OSTheme.HackerGreen:
                    backgroundImage = os.content.Load<Texture2D>("Themes/GreenAbstract");
                    break;
                case OSTheme.HacknetWhite:
                    backgroundImage = os.content.Load<Texture2D>("Themes/AwayTooLong");
                    break;
                case OSTheme.HacknetPurple:
                    backgroundImage = os.content.Load<Texture2D>("Themes/BlueCirclesBackground");
                    break;
                case OSTheme.HacknetMint:
                    backgroundImage = os.content.Load<Texture2D>("Themes/WaterWashGreen2");
                    break;
                default:
                    backgroundImage = os.content.Load<Texture2D>("Themes/Razzamataz2");
                    break;
            }
        }

        internal static void switchThemeColors(OS os, OSTheme theme)
        {
            os.displayModuleExtraLayerBackingColor = Color.Transparent;
            switch (theme)
            {
                case OSTheme.TerminalOnlyBlack:
                    var color = new Color(0, 0, 0, 0);
                    os.defaultHighlightColor = new Color(0, 0, 0, 200);
                    os.defaultTopBarColor = new Color(0, 0, 0, 0);
                    os.highlightColor = os.defaultHighlightColor;
                    os.shellColor = color;
                    os.shellButtonColor = color;
                    os.moduleColorSolid = new Color(0, 0, 0, 0);
                    os.moduleColorStrong = new Color(0, 0, 0, 0);
                    os.moduleColorSolidDefault = new Color(0, 0, 0, 0);
                    os.moduleColorBacking = color;
                    os.topBarColor = os.defaultTopBarColor;
                    os.terminalTextColor = new Color(byte.MaxValue, 254, 235);
                    os.exeModuleTopBar = color;
                    os.exeModuleTitleText = color;
                    break;
                case OSTheme.HacknetBlue:
                    os.defaultHighlightColor = new Color(0, 139, 199, byte.MaxValue);
                    os.defaultTopBarColor = new Color(130, 65, 27);
                    os.warningColor = Color.Red;
                    os.highlightColor = os.defaultHighlightColor;
                    os.subtleTextColor = new Color(90, 90, 90);
                    os.darkBackgroundColor = new Color(8, 8, 8);
                    os.indentBackgroundColor = new Color(12, 12, 12);
                    os.outlineColor = new Color(68, 68, 68);
                    os.lockedColor = new Color(65, 16, 16, 200);
                    os.brightLockedColor = new Color(160, 0, 0);
                    os.brightUnlockedColor = new Color(0, 160, 0);
                    os.unlockedColor = new Color(39, 65, 36);
                    os.lightGray = new Color(180, 180, 180);
                    os.shellColor = new Color(222, 201, 24);
                    os.shellButtonColor = new Color(105, 167, 188);
                    os.moduleColorSolid = new Color(50, 59, 90, byte.MaxValue);
                    os.moduleColorSolidDefault = new Color(50, 59, 90, byte.MaxValue);
                    os.moduleColorStrong = new Color(14, 28, 40, 80);
                    os.moduleColorBacking = new Color(5, 6, 7, 10);
                    os.topBarColor = os.defaultTopBarColor;
                    os.semiTransText = new Color(120, 120, 120, 0);
                    os.terminalTextColor = new Color(213, 245, byte.MaxValue);
                    os.topBarTextColor = new Color(126, 126, 126, 100);
                    os.superLightWhite = new Color(2, 2, 2, 30);
                    os.connectedNodeHighlight = new Color(222, 0, 0, 195);
                    os.exeModuleTopBar = new Color(130, 65, 27, 80);
                    os.exeModuleTitleText = new Color(155, 85, 37, 0);
                    os.netmapToolTipColor = new Color(213, 245, byte.MaxValue, 0);
                    os.netmapToolTipBackground = new Color(0, 0, 0, 70);
                    os.topBarIconsColor = Color.White;
                    os.thisComputerNode = new Color(95, 220, 83);
                    os.scanlinesColor = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
                    break;
                case OSTheme.HacknetTeal:
                    os.defaultHighlightColor = new Color(59, 134, 134, byte.MaxValue);
                    os.defaultTopBarColor = new Color(11, 72, 107);
                    os.warningColor = Color.Red;
                    os.highlightColor = os.defaultHighlightColor;
                    os.subtleTextColor = new Color(90, 90, 90);
                    os.darkBackgroundColor = new Color(8, 8, 8);
                    os.indentBackgroundColor = new Color(12, 12, 12);
                    os.outlineColor = new Color(68, 68, 68);
                    os.lockedColor = new Color(65, 16, 16, 200);
                    os.brightLockedColor = new Color(160, 0, 0);
                    os.brightUnlockedColor = new Color(0, 160, 0);
                    os.unlockedColor = new Color(39, 65, 36);
                    os.lightGray = new Color(180, 180, 180);
                    os.shellColor = new Color(121, 189, 154);
                    os.shellButtonColor = new Color(207, 240, 158);
                    os.moduleColorSolid = new Color(59, 134, 134);
                    os.moduleColorSolidDefault = new Color(59, 134, 134);
                    os.moduleColorStrong = new Color(14, 28, 40, 80);
                    os.moduleColorBacking = new Color(5, 7, 6, 200);
                    os.topBarColor = os.defaultTopBarColor;
                    os.semiTransText = new Color(120, 120, 120, 0);
                    os.terminalTextColor = new Color(213, 245, byte.MaxValue);
                    os.topBarTextColor = new Color(126, 126, 126, 100);
                    os.superLightWhite = new Color(2, 2, 2, 30);
                    os.connectedNodeHighlight = new Color(222, 0, 0, 195);
                    os.exeModuleTopBar = new Color(12, 33, 33, 80);
                    os.exeModuleTitleText = new Color(11, 72, 107, 0);
                    os.netmapToolTipColor = new Color(213, 245, byte.MaxValue, 0);
                    os.netmapToolTipBackground = new Color(0, 0, 0, 70);
                    os.topBarIconsColor = Color.White;
                    os.thisComputerNode = new Color(95, 220, 83);
                    os.scanlinesColor = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
                    break;
                case OSTheme.HacknetYellow:
                    os.defaultHighlightColor = new Color(186, 98, 9, byte.MaxValue);
                    os.defaultTopBarColor = new Color(89, 48, 6);
                    os.highlightColor = os.defaultHighlightColor;
                    os.shellColor = new Color(60, 107, 85);
                    os.shellButtonColor = new Color(207, 240, 158);
                    os.moduleColorSolid = new Color(186, 170, 67, 10);
                    os.moduleColorStrong = new Color(201, 154, 10, 10);
                    os.moduleColorBacking = new Color(5, 7, 6, 200);
                    os.topBarColor = os.defaultTopBarColor;
                    os.exeModuleTopBar = new Color(12, 33, 33, 80);
                    os.exeModuleTitleText = new Color(11, 72, 107, 0);
                    break;
                case OSTheme.HackerGreen:
                    os.defaultHighlightColor = new Color(135, 222, 109, 200);
                    os.defaultTopBarColor = new Color(6, 40, 16);
                    os.highlightColor = os.defaultHighlightColor;
                    os.shellColor = new Color(135, 222, 109);
                    os.shellButtonColor = new Color(207, 240, 158);
                    os.moduleColorSolid = new Color(60, 222, 10, 100);
                    os.moduleColorSolidDefault = new Color(60, 222, 10, 100);
                    os.moduleColorStrong = new Color(10, 80, 20, 50);
                    os.moduleColorBacking = new Color(6, 6, 6, 200);
                    os.topBarColor = os.defaultTopBarColor;
                    os.terminalTextColor = new Color(95, byte.MaxValue, 70);
                    os.exeModuleTopBar = new Color(12, 33, 33, 80);
                    os.exeModuleTitleText = new Color(11, 107, 20, 0);
                    os.topBarIconsColor = Color.White;
                    break;
                case OSTheme.HacknetWhite:
                    os.defaultHighlightColor = new Color(185, 219, byte.MaxValue, byte.MaxValue);
                    os.defaultTopBarColor = new Color(20, 20, 20);
                    os.highlightColor = os.defaultHighlightColor;
                    os.shellColor = new Color(156, 185, 190);
                    os.shellButtonColor = new Color(159, 220, 231);
                    os.moduleColorSolid = new Color(220, 222, 220, 100);
                    os.moduleColorSolidDefault = new Color(220, 222, 220, 100);
                    os.moduleColorStrong = new Color(71, 71, 71, 50);
                    os.moduleColorBacking = new Color(6, 6, 6, 205);
                    os.topBarColor = os.defaultTopBarColor;
                    os.terminalTextColor = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue);
                    os.exeModuleTopBar = new Color(20, 20, 20, 80);
                    os.exeModuleTitleText = new Color(200, 200, 200, 0);
                    os.displayModuleExtraLayerBackingColor = new Color(0, 0, 0, 140);
                    break;
                case OSTheme.HacknetPurple:
                    os.defaultHighlightColor = new Color(35, 158, 121);
                    os.defaultTopBarColor = new Color(0, 0, 0, 60);
                    os.highlightColor = os.defaultHighlightColor;
                    os.highlightColor = os.defaultHighlightColor;
                    os.moduleColorSolid = new Color(154, 119, 189, byte.MaxValue);
                    os.moduleColorSolidDefault = new Color(154, 119, 189, byte.MaxValue);
                    os.moduleColorStrong = new Color(27, 14, 40, 80);
                    os.moduleColorBacking = new Color(6, 5, 7, 205);
                    os.topBarColor = os.defaultTopBarColor;
                    os.exeModuleTopBar = new Color(32, 22, 40, 80);
                    os.exeModuleTitleText = new Color(91, 132, 207, 0);
                    os.netmapToolTipColor = new Color(213, 245, byte.MaxValue, 0);
                    os.netmapToolTipBackground = new Color(0, 0, 0, 70);
                    os.displayModuleExtraLayerBackingColor = new Color(0, 0, 0, 60);
                    os.thisComputerNode = new Color(95, 220, 83);
                    os.scanlinesColor = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
                    break;
                case OSTheme.HacknetMint:
                    os.defaultHighlightColor = new Color(35, 158, 121);
                    os.defaultTopBarColor = new Color(0, 0, 0, 40);
                    os.topBarColor = os.defaultTopBarColor;
                    os.highlightColor = os.defaultHighlightColor;
                    os.moduleColorSolid = new Color(150, 150, 150, byte.MaxValue);
                    os.moduleColorSolidDefault = new Color(150, 150, 150, byte.MaxValue);
                    os.moduleColorStrong = new Color(43, 43, 43, 80);
                    os.moduleColorBacking = new Color(6, 6, 6, 145);
                    os.displayModuleExtraLayerBackingColor = new Color(0, 0, 0, 70);
                    os.thisComputerNode = new Color(95, 220, 83);
                    os.topBarIconsColor = new Color(49, 224, 172);
                    os.exeModuleTopBar = new Color(20, 20, 20, 80);
                    os.exeModuleTitleText = new Color(49, 224, 172, 0);
                    os.scanlinesColor = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
                    break;
            }
        }

        public static Color GetRepresentativeColorForTheme(OSTheme theme)
        {
            switch (theme)
            {
                case OSTheme.TerminalOnlyBlack:
                    return Utils.VeryDarkGray;
                case OSTheme.HacknetTeal:
                    return new Color(59, 134, 134, byte.MaxValue);
                case OSTheme.HacknetYellow:
                    return new Color(186, 98, 9, byte.MaxValue);
                case OSTheme.HackerGreen:
                    return new Color(135, 222, 109, 200);
                case OSTheme.HacknetWhite:
                    return new Color(185, 219, byte.MaxValue, byte.MaxValue);
                case OSTheme.HacknetPurple:
                    return new Color(111, 89, 171, byte.MaxValue);
                case OSTheme.HacknetMint:
                    return new Color(35, 158, 121);
                default:
                    return new Color(0, 139, 199, byte.MaxValue);
            }
        }

        public static void drawBackgroundImage(SpriteBatch sb, Rectangle area)
        {
            switch (currentTheme)
            {
                case OSTheme.HacknetYellow:
                    sb.Draw(Utils.white, area, new Color(51, 38, 0, byte.MaxValue));
                    var dest = new Rectangle(area.X - 30, area.Y - 20, area.Width + 60, area.Height + 40);
                    hexGrid.Draw(dest, sb, Color.Transparent, new Color(byte.MaxValue, 217, 105)*0.2f,
                        HexGridBackground.ColoringAlgorithm.CorrectedSinWash, 0.0f);
                    break;
                default:
                    sb.Draw(backgroundImage, area,
                        currentTheme == OSTheme.HackerGreen ? new Color(100, 150, 100) : Color.White);
                    break;
            }
        }

        public static string getThemeDataString(OSTheme theme)
        {
            return fileData[theme];
        }

        public static OSTheme getThemeForDataString(string data)
        {
            return fileData.FirstOrDefault(x => x.Value == data).Key;
        }

        public static void setThemeOnComputer(object computerObject, OSTheme theme)
        {
            var folder = ((Computer) computerObject).files.root.searchForFolder("sys");
            if (folder.containsFile("x-server.sys"))
            {
                folder.searchForFile("x-server.sys").data = getThemeDataString(theme);
            }
            else
            {
                var fileEntry = new FileEntry(getThemeDataString(theme), "x-server.sys");
                folder.files.Add(fileEntry);
            }
        }
    }
}
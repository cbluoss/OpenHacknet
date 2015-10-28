// Decompiled with JetBrains decompiler
// Type: Hacknet.HexClockExe
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class HexClockExe : ExeModule
    {
        private OSTheme theme;

        public HexClockExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            name = "HexClock";
            ramCost = 55;
            IdentifierName = "HexClock";
            targetIP = os.thisComputer.ip;
            theme = ThemeManager.currentTheme;
        }

        public override void Killed()
        {
            base.Killed();
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            var now = DateTime.Now;
            var str = "#" + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
            var color = Utils.ColorFromHexString(str);
            AutoUpdateTheme(color);
            var contentAreaDest = GetContentAreaDest();
            spriteBatch.Draw(Utils.white, contentAreaDest, color);
            var width = (int) (bounds.Width*0.800000011920929);
            var height = 30;
            TextItem.doFontLabelToSize(
                new Rectangle(contentAreaDest.X + (contentAreaDest.Width/2 - width/2),
                    contentAreaDest.Y + (contentAreaDest.Height/2 - height/2), width, height), str, GuiData.font,
                Utils.AddativeWhite);
        }

        private void AutoUpdateTheme(Color c)
        {
            var c1 = c;
            double h1;
            double s;
            double l1;
            Utils.RGB2HSL(c, out h1, out s, out l1);
            var sl = Math.Min(0.7, s);
            var num = Math.Max(0.2, l1);
            c = Utils.HSL2RGB(h1, sl, num);
            var l2 = Math.Max(0.35, num);
            var h2 = h1 - 0.5;
            if (h2 < 0.0)
                ++h2;
            Utils.HSL2RGB(h2, sl, l2);
            os.defaultHighlightColor = Utils.GetComplimentaryColor(c1);
            os.defaultTopBarColor = new Color(0, 0, 0, 60);
            os.highlightColor = os.defaultHighlightColor;
            os.highlightColor = os.defaultHighlightColor;
            os.moduleColorSolid = Color.Lerp(c, Utils.AddativeWhite, 0.5f);
            os.moduleColorSolidDefault = os.moduleColorSolid;
            os.moduleColorStrong = c;
            os.moduleColorStrong.A = 80;
            os.topBarColor = os.defaultTopBarColor;
            os.exeModuleTopBar = new Color(32, 22, 40, 80);
            os.exeModuleTitleText = new Color(91, 132, 207, 0);
            os.netmapToolTipColor = new Color(213, 245, byte.MaxValue, 0);
            os.netmapToolTipBackground = new Color(0, 0, 0, 70);
            os.displayModuleExtraLayerBackingColor = new Color(0, 0, 0, 60);
            os.thisComputerNode = new Color(95, 220, 83);
            os.scanlinesColor = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 15);
        }
    }
}
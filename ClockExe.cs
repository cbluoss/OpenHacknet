using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class ClockExe : ExeModule
    {
        public ClockExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            name = "Clock";
            ramCost = 60;
            IdentifierName = "Clock";
            targetIP = os.thisComputer.ip;
            AchievementsManager.Unlock("clock_run", false);
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            var now = DateTime.Now;
            TextItem.doFontLabel(new Vector2(bounds.X + 2, bounds.Y + 12),
                (now.Hour%12).ToString("00") + " : " + now.Minute.ToString("00") + " : " + now.Second.ToString("00"),
                GuiData.titlefont, RamModule.USED_RAM_COLOR, bounds.Width - 15, bounds.Height - 10);
            TextItem.doFontLabel(new Vector2(bounds.X + bounds.Width - 28, bounds.Y + bounds.Height - 38),
                now.Hour > 12 ? "PM" : "AM", GuiData.titlefont, RamModule.USED_RAM_COLOR, 30f, 26f);
            var width = bounds.Width - 2;
            var destinationRectangle = new Rectangle(bounds.X + 1, bounds.Y + bounds.Height - 1 - 6, width, 1);
            var num1 = now.Millisecond/1000f;
            var num2 = 0.0f;
            if (num1 < 0.5)
                num2 = (float) (1.0 - num1*2.0);
            spriteBatch.Draw(Utils.white, destinationRectangle, os.moduleColorSolidDefault*0.2f*num2);
            destinationRectangle.Width = (int) (width*(double) num1);
            spriteBatch.Draw(Utils.white, destinationRectangle, os.moduleColorSolidDefault*0.2f);
            var num3 = (now.Second + num1)/60f;
            destinationRectangle.Width = (int) (width*(double) num3);
            destinationRectangle.Y += 2;
            spriteBatch.Draw(Utils.white, destinationRectangle, os.moduleColorStrong);
            var num4 = now.Minute/60f;
            var num5 = now.Hour/60.0;
            destinationRectangle.Width = (int) (width*(double) num4);
            destinationRectangle.Y += 2;
            spriteBatch.Draw(Utils.white, destinationRectangle, os.moduleColorSolid);
        }
    }
}
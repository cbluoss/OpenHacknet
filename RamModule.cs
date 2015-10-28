// Decompiled with JetBrains decompiler
// Type: Hacknet.RamModule
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class RamModule : CoreModule
    {
        public static int contentStartOffset = 16;
        public static int MODULE_WIDTH = 252;
        public static Color USED_RAM_COLOR = new Color(60, 60, 67);
        public static float FLASH_TIME = 3f;
        private Rectangle infoBar;
        private Rectangle infoBarUsedRam;
        private string infoString = "";
        private Vector2 infoStringPos;
        private float OutOfMemoryFlashTime;

        public RamModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
        }

        public override void LoadContent()
        {
            base.LoadContent();
            infoBar = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, contentStartOffset);
            infoBarUsedRam = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, contentStartOffset);
            infoStringPos = new Vector2(infoBar.X, infoBar.Y);
        }

        public override void Update(float t)
        {
            base.Update(t);
            infoBar = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, contentStartOffset);
            infoString = "USED RAM: " + (os.totalRam - os.ramAvaliable) + "mb / " + os.totalRam + "mb";
            infoBarUsedRam = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, contentStartOffset);
            if (OutOfMemoryFlashTime <= 0.0)
                return;
            OutOfMemoryFlashTime -= t;
        }

        public void FlashMemoryWarning()
        {
            OutOfMemoryFlashTime = FLASH_TIME;
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            spriteBatch.Draw(Utils.white, infoBar, os.indentBackgroundColor);
            infoBarUsedRam.Width = (int) (infoBar.Width*(1.0 - os.ramAvaliable/(double) os.totalRam));
            spriteBatch.Draw(Utils.white, infoBarUsedRam, USED_RAM_COLOR);
            spriteBatch.DrawString(GuiData.detailfont, infoString, new Vector2(infoBar.X, infoBar.Y), Color.White);
            spriteBatch.DrawString(GuiData.detailfont, string.Concat(os.exes.Count),
                new Vector2(bounds.X + bounds.Width - (os.exes.Count >= 10 ? 24 : 12), infoBar.Y), Color.White);
            if (OutOfMemoryFlashTime <= 0.0)
                return;
            var num = Math.Min(1f, OutOfMemoryFlashTime);
            var amount = Math.Max(0.0f, OutOfMemoryFlashTime - (FLASH_TIME - 1f));
            PatternDrawer.draw(bounds, 0.0f, Color.Transparent,
                Color.Lerp(os.lockedColor, Utils.AddativeRed, amount)*num, spriteBatch, PatternDrawer.errorTile);
            var height = 40;
            var rectangle = new Rectangle(bounds.X, bounds.Y + bounds.Height - height - 1, bounds.Width, height);
            spriteBatch.Draw(Utils.white, Utils.InsetRectangle(rectangle, 4), Color.Black*0.75f);
            --rectangle.X;
            var text = " ^ INSUFFICIENT MEMORY ^ ";
            TextItem.doFontLabelToSize(rectangle, text, GuiData.font, Color.Black*num);
            rectangle.X += 2;
            TextItem.doFontLabelToSize(rectangle, text, GuiData.font, Color.Black*num);
            --rectangle.X;
            TextItem.doFontLabelToSize(rectangle, text, GuiData.font, Color.White*num);
        }

        public virtual void drawOutline()
        {
            var destinationRectangle = bounds;
            spriteBatch.Draw(Utils.white, destinationRectangle, os.outlineColor);
            ++destinationRectangle.X;
            ++destinationRectangle.Y;
            destinationRectangle.Width -= 2;
            destinationRectangle.Height -= 2;
            spriteBatch.Draw(Utils.white, destinationRectangle, os.darkBackgroundColor);
        }
    }
}
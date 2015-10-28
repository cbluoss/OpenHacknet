// Decompiled with JetBrains decompiler
// Type: Hacknet.ThemeChangerExe
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class ThemeChangerExe : ExeModule
    {
        private const float START_LOADING_TIME = 25.5f;
        private BarcodeEffect barcodeEffect;
        private readonly Texture2D circle;
        private SpriteBatch internalSB;
        private float loadingTimeRemaining = 25.5f;
        private int localScroll;
        private int localsSelected = -1;
        private int remoteScroll;
        private int remotesSelected = -1;
        private readonly ThemeChangerState state = ThemeChangerState.List;
        private RenderTarget2D target;
        private Color themeColor;

        public ThemeChangerExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            name = "ThemeSwitch";
            ramCost = 320;
            IdentifierName = "Theme Switch";
            targetIP = os.thisComputer.ip;
            circle = TextureBank.load("Circle", os.content);
            AchievementsManager.Unlock("themeswitch_run", false);
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            themeColor = os.highlightColor;
            var dest = new Rectangle(bounds.X + 2, bounds.Y + PANEL_HEIGHT, bounds.Width - 4,
                bounds.Height - 2 - PANEL_HEIGHT);
            switch (state)
            {
                case ThemeChangerState.Startup:
                    loadingTimeRemaining -= t;
                    DrawLoading(loadingTimeRemaining, 25.5f, dest, spriteBatch);
                    break;
                case ThemeChangerState.LoadItem:
                    break;
                case ThemeChangerState.Activating:
                    break;
                default:
                    spriteBatch.GraphicsDevice.Clear(Color.Transparent);
                    DrawListing(dest, spriteBatch);
                    break;
            }
        }

        public void DrawWithSeperateRT(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            themeColor = os.highlightColor;
            var destinationRectangle = new Rectangle(bounds.X + 2, bounds.Y + PANEL_HEIGHT + 2, bounds.Width - 4,
                bounds.Height - 3 - PANEL_HEIGHT);
            var flag = false;
            if (target == null)
            {
                target = new RenderTarget2D(this.spriteBatch.GraphicsDevice, destinationRectangle.Width,
                    destinationRectangle.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                    RenderTargetUsage.PreserveContents);
                internalSB = new SpriteBatch(this.spriteBatch.GraphicsDevice);
                flag = true;
            }
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            this.spriteBatch.GraphicsDevice.SetRenderTarget(target);
            var dest = new Rectangle(0, 0, destinationRectangle.Width, destinationRectangle.Height);
            var spriteBatch = GuiData.spriteBatch;
            GuiData.spriteBatch = internalSB;
            internalSB.Begin();
            if (flag)
                internalSB.GraphicsDevice.Clear(Color.Transparent);
            switch (state)
            {
                case ThemeChangerState.Startup:
                    loadingTimeRemaining -= t;
                    DrawLoading(loadingTimeRemaining, 25.5f, dest, internalSB);
                    goto case 2;
                case ThemeChangerState.LoadItem:
                case ThemeChangerState.Activating:
                    internalSB.End();
                    GuiData.spriteBatch = spriteBatch;
                    this.spriteBatch.GraphicsDevice.SetRenderTarget(currentRenderTarget);
                    this.spriteBatch.Draw(target, destinationRectangle, Color.White);
                    break;
                default:
                    this.spriteBatch.GraphicsDevice.Clear(Color.Transparent);
                    DrawListing(dest, internalSB);
                    goto case 2;
            }
        }

        private void DrawListing(Rectangle dest, SpriteBatch sb)
        {
            DrawHeaders(dest, sb);
            if (isExiting)
                return;
            var pos = new Vector2(dest.X + 2, dest.Y + 60f);
            TextItem.doFontLabel(pos, "Remote", GuiData.smallfont, themeColor, this.bounds.Width - 20, 20f);
            pos.Y += 18f;
            sb.Draw(Utils.white, new Rectangle(this.bounds.X + 2, (int) pos.Y, this.bounds.Width - 6, 1),
                Utils.AddativeWhite);
            var list1 = new List<string>();
            var currentFolder = Programs.getCurrentFolder(os);
            for (var index = 0; index < currentFolder.files.Count; ++index)
            {
                if (ThemeManager.getThemeForDataString(currentFolder.files[index].data) != OSTheme.TerminalOnlyBlack)
                    list1.Add(currentFolder.files[index].name);
            }
            string str = null;
            string selectedFileData = null;
            var color = Color.Lerp(os.topBarColor, Utils.AddativeWhite, 0.2f);
            var num = SelectableTextList.scrollOffset;
            var rectangle1 = new Rectangle((int) pos.X, (int) pos.Y, this.bounds.Width - 6, 54);
            if (list1.Count > 0)
            {
                SelectableTextList.scrollOffset = remoteScroll;
                remotesSelected = SelectableTextList.doFancyList(8139191 + PID, rectangle1.X, rectangle1.Y,
                    rectangle1.Width, rectangle1.Height, list1.ToArray(), remotesSelected, color, true);
                if (SelectableTextList.selectionWasChanged)
                    localsSelected = -1;
                remoteScroll = SelectableTextList.scrollOffset;
                if (remotesSelected >= 0)
                {
                    if (remotesSelected >= list1.Count)
                    {
                        remotesSelected = -1;
                    }
                    else
                    {
                        str = list1[remotesSelected];
                        selectedFileData = currentFolder.searchForFile(str).data;
                    }
                }
            }
            else
            {
                sb.Draw(Utils.white, rectangle1, Utils.VeryDarkGray);
                TextItem.doFontLabelToSize(rectangle1, "    -- No Valid Files --    ", GuiData.smallfont,
                    Utils.AddativeWhite);
            }
            pos.Y += rectangle1.Height + 6;
            TextItem.doFontLabel(pos, "Local Theme Files", GuiData.smallfont, themeColor, this.bounds.Width - 20, 20f);
            pos.Y += 18f;
            sb.Draw(Utils.white, new Rectangle(this.bounds.X + 2, (int) pos.Y, this.bounds.Width - 6, 1),
                Utils.AddativeWhite);
            list1.Clear();
            var list2 = new List<string>();
            var folder1 = os.thisComputer.files.root.searchForFolder("sys");
            for (var index = 0; index < folder1.files.Count; ++index)
            {
                if (ThemeManager.getThemeForDataString(folder1.files[index].data) != OSTheme.TerminalOnlyBlack)
                {
                    list1.Add(folder1.files[index].name);
                    list2.Add(folder1.files[index].data);
                }
            }
            var folder2 = os.thisComputer.files.root.searchForFolder("home");
            for (var index = 0; index < folder2.files.Count; ++index)
            {
                if (ThemeManager.getThemeForDataString(folder2.files[index].data) != OSTheme.TerminalOnlyBlack)
                {
                    list1.Add(folder2.files[index].name);
                    list2.Add(folder2.files[index].data);
                }
            }
            var rectangle2 = new Rectangle((int) pos.X, (int) pos.Y, this.bounds.Width - 6, 72);
            if (list1.Count > 0)
            {
                SelectableTextList.scrollOffset = localScroll;
                localsSelected = SelectableTextList.doFancyList(839192 + PID, rectangle2.X, rectangle2.Y,
                    rectangle2.Width, rectangle2.Height, list1.ToArray(), localsSelected, color, true);
                if (SelectableTextList.selectionWasChanged)
                    remotesSelected = -1;
                localScroll = SelectableTextList.scrollOffset;
                if (localsSelected >= 0)
                {
                    str = list1[localsSelected];
                    selectedFileData = list2[localsSelected];
                }
            }
            else
            {
                sb.Draw(Utils.white, rectangle2, Utils.VeryDarkGray);
                TextItem.doFontLabelToSize(rectangle2, "    -- No Valid Files --    ", GuiData.smallfont,
                    Utils.AddativeWhite);
            }
            SelectableTextList.scrollOffset = num;
            pos.Y += rectangle2.Height + 2;
            var bounds = new Rectangle(this.bounds.X + 4, (int) pos.Y + 2, this.bounds.Width - 8,
                (int) (dest.Height - (pos.Y - (double) dest.Y)) - 4);
            DrawApplyField(str, selectedFileData, bounds, sb);
        }

        private void DrawApplyField(string selectedFilename, string selectedFileData, Rectangle bounds, SpriteBatch sb)
        {
            sb.Draw(Utils.white, bounds, Utils.VeryDarkGray);
            sb.Draw(Utils.white, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), Utils.AddativeWhite);
            if (selectedFileData == null || selectedFilename == null)
                return;
            var representativeColorForTheme =
                ThemeManager.GetRepresentativeColorForTheme(ThemeManager.getThemeForDataString(selectedFileData));
            var destinationRectangle = new Rectangle(bounds.X + bounds.Width/4*3, bounds.Y + 2, bounds.Width/4,
                bounds.Height - 2);
            sb.Draw(Utils.white, destinationRectangle, representativeColorForTheme);
            destinationRectangle.X += destinationRectangle.Width - 15;
            destinationRectangle.Width = 15;
            sb.Draw(Utils.white, destinationRectangle, Color.Black*0.6f);
            TextItem.doFontLabel(new Vector2(bounds.X, bounds.Y + 2), selectedFilename, GuiData.smallfont,
                Utils.AddativeWhite, bounds.Width/4*3, 25f);
            if (
                !Button.doButton(3837791 + PID, bounds.X, bounds.Y + 25, bounds.Width/6*4, 30, "Activate Theme",
                    representativeColorForTheme))
                return;
            ApplyTheme(selectedFilename, selectedFileData);
        }

        private void ApplyTheme(string themeFilename, string fileData)
        {
            var filename = "x-serverBACKUP";
            var f = os.thisComputer.files.root.searchForFolder("sys");
            var fileEntry = f.searchForFile("x-server.sys");
            if (fileEntry != null)
            {
                var flag = true;
                for (var index = 0; index < f.files.Count; ++index)
                {
                    if (f.files[index].name.StartsWith(filename) && f.files[index].data == fileEntry.data)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    var repeatingFilename = Utils.GetNonRepeatingFilename(filename, ".sys", f);
                    f.files.Add(new FileEntry(fileEntry.data, repeatingFilename));
                }
            }
            fileEntry.data = fileData;
            ThemeManager.switchTheme(os, ThemeManager.getThemeForDataString(fileData));
        }

        private void DrawHeaders(Rectangle dest, SpriteBatch sb)
        {
            if (barcodeEffect == null)
                barcodeEffect = new BarcodeEffect(bounds.Width - 4, false, false);
            barcodeEffect.Update((float) os.lastGameTime.ElapsedGameTime.TotalSeconds);
            barcodeEffect.Draw(dest.X, dest.Y, dest.Width - 4, Math.Min(30, bounds.Height - PANEL_HEIGHT), sb,
                themeColor);
            if (bounds.Height <= PANEL_HEIGHT + 60)
                return;
            TextItem.doFontLabel(new Vector2(dest.X + 2, dest.Y + 32), "Themechanger", GuiData.font,
                Utils.AddativeWhite*0.8f, bounds.Width - 80, 30f);
            var width = 60;
            if (
                !Button.doButton(PID + 228173, bounds.X + bounds.Width - width - 2, dest.Y + 38, width, 20, "Exit",
                    os.lockedColor))
                return;
            isExiting = true;
        }

        private void DrawLoading(float timeRemaining, float totalTime, Rectangle dest, SpriteBatch sb)
        {
            var loaderRadius = 20f;
            var loaderCentre = new Vector2(dest.X + dest.Width/2f, dest.Y + dest.Height/2f);
            var num1 = totalTime - timeRemaining;
            var num2 = num1/totalTime;
            sb.Draw(Utils.white, dest, Color.Black*0.05f);
            DrawLoadingCircle(timeRemaining, totalTime, dest, loaderCentre, loaderRadius, num1*0.2f, 1f, sb);
            DrawLoadingCircle(timeRemaining, totalTime, dest, loaderCentre, loaderRadius + 20f*num2, num1*-0.4f, 0.7f,
                sb);
            DrawLoadingCircle(timeRemaining, totalTime, dest, loaderCentre, loaderRadius + 35f*num2, num1*0.5f, 0.52f,
                sb);
        }

        private void DrawLoadingCircle(float timeRemaining, float totalTime, Rectangle dest, Vector2 loaderCentre,
            float loaderRadius, float baseRotationAdd, float rotationRateRPS, SpriteBatch sb)
        {
            var num1 = totalTime - timeRemaining;
            var num2 = 10;
            for (var index = 0; index < num2; ++index)
            {
                var num3 = index/(float) num2;
                var num4 = 2f;
                var num5 = 1f;
                var num6 = 6.283185f;
                var num7 = num6 + num5;
                var num8 = num3*num4;
                if (num1 > (double) num8)
                {
                    var num9 = num1/num8*rotationRateRPS%num7;
                    if (num9 >= (double) num6)
                        num9 = 0.0f;
                    var angle = num6*Utils.QuadraticOutCurve(num9/num6) + baseRotationAdd;
                    var vector2_1 = loaderCentre + Utils.PolarToCartesian(angle, loaderRadius);
                    sb.Draw(circle, vector2_1, new Rectangle?(), Utils.AddativeWhite, 0.0f, Vector2.Zero,
                        (float) (0.100000001490116*(loaderRadius/120.0)), SpriteEffects.None, 0.3f);
                    if (Utils.random.NextDouble() < 0.001)
                    {
                        var vector2_2 = loaderCentre + Utils.PolarToCartesian(angle, 20f + Utils.randm(45f));
                        sb.Draw(Utils.white, vector2_1, Utils.AddativeWhite);
                        Utils.drawLine(sb, vector2_1, vector2_2, Vector2.Zero, Utils.AddativeWhite*0.4f, 0.1f);
                    }
                }
            }
        }

        private enum ThemeChangerState
        {
            Startup,
            List,
            LoadItem,
            Activating
        }
    }
}
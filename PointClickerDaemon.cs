using System;
using System.Collections.Generic;
using Hacknet.Effects;
using Hacknet.Gui;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class PointClickerDaemon : Daemon
    {
        private PointClickerGameState activeState;
        private string ActiveStory = "";
        private Texture2D background1;
        private Texture2D background2;
        private float currentRate;
        private int hoverIndex;
        private Texture2D logoBase;
        private SpriteBatch logoBatch;
        private Texture2D logoOverlay1;
        private Texture2D logoOverlay2;
        private RenderTarget2D logoRenderBase;
        private Texture2D logoStar;
        private float pointOverflow;
        private Folder rootFolder;
        private Folder savesFolder;
        private Texture2D scanlinesTextBackground;
        private ScrollableSectionedPanel scrollerPanel;
        private readonly List<PointClickerStar> Stars = new List<PointClickerStar>();
        private PointClickerScreenState state;
        private readonly List<long> storyBeatChangers = new List<long>();
        private readonly List<string> storyBeats = new List<string>();
        private readonly Color ThemeColor = new Color(133, 239, byte.MaxValue, 0);
        private readonly Color ThemeColorBacking = new Color(13, 59, 74, 250);
        private readonly Color ThemeColorHighlight = new Color(227, 0, 121, 200);
        private float timeSinceLastSave;
        private readonly float UpgradeCostMultiplier = 13f;
        private readonly List<float> upgradeCosts = new List<float>();
        private readonly List<string> upgradeNames = new List<string>();
        private readonly List<UpgradeNotifier> UpgradeNotifiers = new List<UpgradeNotifier>();
        private readonly List<float> upgradeValues = new List<float>();
        private string userFilePath;

        public PointClickerDaemon(Computer computer, string serviceName, OS opSystem)
            : base(computer, serviceName, opSystem)
        {
            InitGameSettings();
            InitLogoSettings();
            InitRest();
        }

        private void InitRest()
        {
        }

        private void InitLogoSettings()
        {
            background1 = os.content.Load<Texture2D>("EffectFiles/PointClicker/Background1");
            background2 = os.content.Load<Texture2D>("EffectFiles/PointClicker/Background2");
            logoBase = os.content.Load<Texture2D>("EffectFiles/PointClicker/BaseLogo");
            logoOverlay1 = os.content.Load<Texture2D>("EffectFiles/PointClicker/LogoOverlay1");
            logoOverlay2 = os.content.Load<Texture2D>("EffectFiles/PointClicker/LogoOverlay2");
            logoStar = os.content.Load<Texture2D>("EffectFiles/PointClicker/Star");
            scanlinesTextBackground = os.content.Load<Texture2D>("EffectFiles/ScanlinesTextBackground");
            logoRenderBase = new RenderTarget2D(GuiData.spriteBatch.GraphicsDevice, 768, 384);
            logoBatch = new SpriteBatch(GuiData.spriteBatch.GraphicsDevice);
            for (var index = 0; index < 40; ++index)
                AddRandomLogoStar(true);
        }

        private void UpdateRate()
        {
            currentRate = 0.0f;
            for (var index = 0; index < upgradeNames.Count; ++index)
                currentRate += upgradeValues[index]*activeState.upgradeCounts[index];
        }

        private void UpdatePoints()
        {
            if (activeState == null)
                return;
            if (currentRate > 0.0 || currentRate < -1.0)
            {
                var num1 = currentRate*os.lastGameTime.ElapsedGameTime.TotalSeconds;
                activeState.points += (int) num1;
                pointOverflow += (float) num1 - (int) num1;
                if (pointOverflow > 1.0)
                {
                    var num2 = (int) pointOverflow;
                    activeState.points += num2;
                    pointOverflow -= num2;
                }
            }
            UpdateStory();
            if (ActiveStory == null)
                ActiveStory = "";
            if (activeState.points <= -1L)
                AchievementsManager.Unlock("pointclicker_expert", true);
            timeSinceLastSave += (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastSave <= 10.0)
                return;
            SaveProgress();
        }

        private void UpdateStory()
        {
            if (activeState.currentStoryElement < 0)
            {
                activeState.currentStoryElement = 0;
                ActiveStory = storyBeats[activeState.currentStoryElement] ?? "";
            }
            else
            {
                if (storyBeatChangers.Count <= activeState.currentStoryElement + 1 ||
                    activeState.points < storyBeatChangers[activeState.currentStoryElement + 1])
                    return;
                ++activeState.currentStoryElement;
                ActiveStory = storyBeats[activeState.currentStoryElement] ?? "";
            }
        }

        private void PurchaseUpgrade(int index)
        {
            if (activeState == null || activeState.points < (double) upgradeCosts[index])
                return;
            List<int> list;
            int index1;
            (list = activeState.upgradeCounts)[index1 = index] = list[index1] + 1;
            activeState.points -= (int) upgradeCosts[index];
            pointOverflow -= upgradeCosts[index] - (int) upgradeCosts[index];
            UpdateRate();
            UpgradeNotifiers.Add(new UpgradeNotifier
            {
                text = "+" + upgradeValues[index],
                timer = 1f
            });
            SaveProgress();
            if (index < upgradeNames.Count - 1)
                return;
            AchievementsManager.Unlock("pointclicker_basic", true);
        }

        private void SaveProgress()
        {
            for (var index = 0; index < savesFolder.files.Count; ++index)
            {
                if (savesFolder.files[index].name == userFilePath)
                {
                    savesFolder.files[index].data = activeState.ToSaveString();
                    timeSinceLastSave = 0.0f;
                    break;
                }
            }
        }

        private void AddRandomLogoStar(bool randomStartLife = false)
        {
            var pointClickerStar = new PointClickerStar
            {
                Pos = new Vector2(Utils.randm(1f), Utils.randm(1f)),
                life = randomStartLife ? Utils.randm(1f) : 1f,
                rot = Utils.randm(6.48f),
                scale = 0.2f + Utils.rand(1.3f),
                timescale = 0.3f + Utils.randm(1.35f)
            };
            pointClickerStar.color = Utils.AddativeWhite;
            var num = Utils.randm(1f);
            var maxValue = 80;
            if (num < 0.300000011920929)
                pointClickerStar.color.R = (byte) (byte.MaxValue - Utils.random.Next(maxValue));
            else if (num < 0.600000023841858)
                pointClickerStar.color.G = (byte) (byte.MaxValue - Utils.random.Next(maxValue));
            else if (num < 0.899999976158142)
                pointClickerStar.color.B = (byte) (byte.MaxValue - Utils.random.Next(maxValue));
            Stars.Add(pointClickerStar);
        }

        private void InitGameSettings()
        {
            upgradeNames.Add("Click Me!");
            upgradeNames.Add("Autoclicker v1");
            upgradeNames.Add("Autoclicker v2");
            upgradeNames.Add("Pointereiellion");
            upgradeValues.Add(0.04f);
            upgradeValues.Add(1f);
            upgradeValues.Add(10f);
            upgradeValues.Add(200f);
            storyBeats.Add("Your glorious ClickPoints empire begins");
            storyBeats.Add(
                "The hard days of manual button clicking labor seem endless, but a better future is in sight.");
            storyBeats.Add("The investment is returned - you finally turn a profit.");
            storyBeats.Add("Your long days of labor to gather the initial 12 points are a fast-fading memory.");
            storyBeats.Add("You reach international acclaim as a prominent and incredibly wealthy point collector.");
            storyBeats.Add("Your enormous pile of points is now larger than everest");
            storyBeats.Add("The ClickPoints continent is declared : a landmass made entirely of your insane wealth.");
            storyBeatChangers.Add(0L);
            storyBeatChangers.Add(5L);
            storyBeatChangers.Add(15L);
            storyBeatChangers.Add(200L);
            storyBeatChangers.Add(100000L);
            storyBeatChangers.Add(1000000000000L);
            storyBeatChangers.Add(11111000000000000L);
            for (var index = 3; index < 50; ++index)
            {
                upgradeNames.Add("Upgrade " + index + 1);
                upgradeValues.Add((float) Math.Max(index*index*index*index*index, 0.01));
            }
            for (var index = 0; index < upgradeValues.Count; ++index)
                upgradeCosts.Add(
                    (float)
                        (upgradeValues[index]*(double) (1 + index/50*5)*UpgradeCostMultiplier*
                         (1.0 + UpgradeCostMultiplier*(index + 1)/upgradeValues.Count*5.0)));
            upgradeCosts[0] = 0.0f;
            upgradeCosts[1] = 12f;
        }

        public override void initFiles()
        {
            base.initFiles();
            rootFolder = new Folder("PointClicker");
            savesFolder = new Folder("Saves");
            var num1 = 50;
            var num2 = 0;
            for (var index = 0; index < num1; ++index)
            {
                string name;
                do
                {
                    name = People.all[index + num2].handle;
                    if (name == null)
                        ++num2;
                } while (index + num2 < People.all.Count && name == null);
                if (index == 22)
                    name = "Mengsk";
                if (index == 28)
                    name = "Bit";
                if (name != null)
                    AddSaveForName(name, index == 22 || index == 28);
            }
            rootFolder.folders.Add(savesFolder);
            comp.files.root.folders.Add(rootFolder);
            rootFolder.files.Add(new FileEntry(Computer.generateBinaryString(1000), "config.ini"));
            rootFolder.files.Add(
                new FileEntry(
                    "IMPORTANT : NEVER DELETE OR RE-NAME \"config.ini\"\n IT IS SYSTEM CRITICAL! Removing it causes instant crash. DO NOT TEST THIS",
                    "IMPORTANT_README_DONT_CRASH.txt"));
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            state = PointClickerScreenState.Welcome;
        }

        public override void loadInit()
        {
            base.loadInit();
            rootFolder = comp.files.root.searchForFolder("PointClicker");
            savesFolder = rootFolder.searchForFolder("Saves");
        }

        private void AddSaveForName(string name, bool isSuperHighScore = false)
        {
            var clickerGameState = new PointClickerGameState(upgradeValues.Count);
            for (var index = 0; index < clickerGameState.upgradeCounts.Count; ++index)
            {
                clickerGameState.upgradeCounts[index] =
                    (int) (10.0*Utils.randm(1f)*(index/(double) clickerGameState.upgradeCounts.Count));
                if (isSuperHighScore)
                    clickerGameState.upgradeCounts[index] = 900 + (int) (Utils.randm(1f)*99.9000015258789);
            }
            clickerGameState.points = Utils.random.Next();
            clickerGameState.currentStoryElement = Utils.random.Next(storyBeats.Count);
            savesFolder.files.Add(new FileEntry(clickerGameState.ToSaveString(), name + ".pcsav"));
        }

        public override string getSaveString()
        {
            return "<PointClicker />";
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            switch (state)
            {
                case PointClickerScreenState.Error:
                    TextItem.DrawShadow = flag;
                    break;
                case PointClickerScreenState.Main:
                    UpdatePoints();
                    DrawMainScreen(bounds, sb);
                    goto case 1;
                default:
                    DrawWelcome(bounds, sb);
                    goto case 1;
            }
        }

        private void DrawLogo(Rectangle dest, SpriteBatch sb)
        {
            var num1 = (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
            for (var index = 0; index < Stars.Count; ++index)
            {
                var pointClickerStar = Stars[index];
                pointClickerStar.life -= num1*2f*pointClickerStar.timescale;
                if (Stars[index].life <= 0.0)
                {
                    Stars.RemoveAt(index);
                    --index;
                    AddRandomLogoStar(false);
                }
                else
                    Stars[index] = pointClickerStar;
            }
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            sb.GraphicsDevice.SetRenderTarget(logoRenderBase);
            sb.GraphicsDevice.Clear(Color.Transparent);
            logoBatch.Begin();
            var num2 = (float) (Math.Sin(os.timer/2.20000004768372) + 1.0)/2f;
            logoBatch.Draw(background1, Vector2.Zero, Utils.AddativeWhite*num2);
            logoBatch.Draw(background2, Vector2.Zero, Utils.AddativeWhite*(1f - num2));
            var dest1 = new Rectangle(0, 0, logoBase.Width, logoBase.Height);
            logoBatch.Draw(logoBase, Vector2.Zero, Color.White);
            FlickeringTextEffect.DrawFlickeringSprite(logoBatch, dest1, logoBase, 4f, 0.25f, os, Color.White);
            var num3 = (float) (0.439999997615814 + (Math.Sin(os.timer*0.823000013828278) + 1.0)/2.0);
            logoBatch.Draw(logoOverlay1, Vector2.Zero, Utils.AddativeWhite*num3);
            logoBatch.Draw(logoOverlay2, Vector2.Zero, Utils.AddativeWhite*(1f - num3));
            logoBatch.End();
            sb.GraphicsDevice.SetRenderTarget(currentRenderTarget);
            for (var index = 0; index < Stars.Count; ++index)
                DrawStar(dest, sb, Stars[index]);
            FlickeringTextEffect.DrawFlickeringSpriteAltWeightings(sb, dest, logoRenderBase, 4f, 0.01f, os,
                Utils.AddativeWhite);
        }

        private void DrawStar(Rectangle logoDest, SpriteBatch sb, PointClickerStar star)
        {
            var position = new Vector2((float) (star.Pos.X*(double) logoDest.Width*0.5) + logoDest.X,
                (float) (star.Pos.Y*(double) logoDest.Height*0.5) + logoDest.Y);
            position.X += logoDest.Width*0.25f;
            position.Y += logoDest.Height*0.25f;
            var num1 = star.life >= 0.899999976158142
                ? (float) (1.0 - (star.life - 0.899999976158142)/0.100000001490116)
                : (float) ((star.life - 0.100000001490116)/0.899999976158142);
            var num2 = Vector2.Distance(star.Pos, new Vector2(0.5f));
            var num3 = 0.9f;
            if (num2 > (double) num3)
                num1 = (float) (1.0 - (num2 - (double) num3)/1.0)*num1;
            sb.Draw(logoStar, position, new Rectangle?(), star.color*num1, star.rot*(star.life*0.5f),
                new Vector2(logoStar.Width/2, logoStar.Height/2), star.scale*num1, SpriteEffects.None, 0.4f);
        }

        private void DrawMainScreen(Rectangle bounds, SpriteBatch sb)
        {
            var points = activeState.points.ToString();
            var rectangle1 = new Rectangle(bounds.X + 1, bounds.Y + 10, bounds.Width - 2, 100);
            DrawMonospaceString(rectangle1, sb, points);
            rectangle1.Y -= 4;
            sb.Draw(scanlinesTextBackground, rectangle1, ThemeColor*0.2f);
            for (var index = 0; index < Stars.Count; ++index)
                DrawStar(rectangle1, sb, Stars[index]);
            var bounds1 = new Rectangle(bounds.X + 2, rectangle1.Y + rectangle1.Height + 12, bounds.Width/2 - 4,
                bounds.Height - 12 - (rectangle1.Y + rectangle1.Height - bounds.Y));
            DrawUpgrades(bounds1, sb);
            var num1 = logoRenderBase.Height/(float) logoRenderBase.Width;
            var num2 = 45;
            var width = bounds.Width/2 + num2;
            var height = (int) (width*(double) num1);
            DrawHoverTooltip(
                new Rectangle(bounds.X + bounds1.Width + 4, bounds1.Y, bounds.Width - bounds1.Width - 8,
                    bounds1.Height - height + 30), sb);
            var rectangle2 = new Rectangle(bounds.X + bounds.Width - width + 42, bounds.Y + bounds.Height - height + 10,
                width - 50, 50);
            var text = Utils.SuperSmartTwimForWidth(ActiveStory, rectangle2.Width, GuiData.smallfont);
            TextItem.doFontLabel(new Vector2(rectangle2.X, rectangle2.Y), text, GuiData.smallfont, ThemeColor,
                rectangle2.Width, rectangle2.Height);
            DrawLogo(
                new Rectangle(bounds.X + bounds.Width - width + 35, bounds.Y + bounds.Height - height + 20, width,
                    height), sb);
            if (
                !Button.doButton(3032113, bounds.X + bounds.Width - 22, bounds.Y + bounds.Height - 22, 20, 20, "X",
                    os.lockedColor))
                return;
            state = PointClickerScreenState.Welcome;
        }

        private void DrawHoverTooltip(Rectangle bounds, SpriteBatch sb)
        {
            var announcerWidth = 80f;
            var num1 = bounds.Height - 80;
            Rectangle rectangle1;
            if (hoverIndex > -1 && hoverIndex < upgradeNames.Count && activeState != null)
            {
                var bounds1 = bounds;
                bounds1.Height = num1;
                var flag = upgradeCosts[hoverIndex] <= (double) activeState.points;
                var cornerCut = 20f;
                FancyOutlines.DrawCornerCutOutline(bounds1, sb, cornerCut, ThemeColor);
                var dest = new Rectangle((int) (bounds.X + (double) cornerCut + 4.0), bounds.Y + 3,
                    (int) (bounds.Width - (cornerCut + 4.0)*2.0), 40);
                TextItem.doFontLabelToSize(dest, upgradeNames[hoverIndex], GuiData.font, ThemeColorHighlight);
                rectangle1 = new Rectangle(bounds.X + 2, bounds.Y + dest.Height + 4, bounds.Width - 4, 20);
                var text = flag ? "UPGRADE AVALIABLE" : "INSUFFICIENT POINTS";
                TextItem.doFontLabelToSize(rectangle1, text, GuiData.font, flag ? ThemeColorHighlight*0.8f : Color.Gray);
                rectangle1.Y += rectangle1.Height;
                rectangle1.Height = 50;
                rectangle1.X += 4;
                rectangle1.Width -= 4;
                var f = activeState.points == 0L ? 1f : upgradeCosts[hoverIndex]/activeState.points;
                DrawStatsTextBlock("COST", string.Concat(upgradeCosts[hoverIndex]),
                    (!float.IsNaN(f) ? f*100f : 100f).ToString("00.0") + "% of current Points", rectangle1, sb,
                    announcerWidth);
                rectangle1.Y += rectangle1.Height;
                DrawStatsTextBlock("+PPS", string.Concat(upgradeValues[hoverIndex]),
                    (currentRate <= 0.0 ? 100f : (float) (upgradeValues[hoverIndex]/(double) currentRate*100.0))
                        .ToString("00.0") + "% of current Points Per Second", rectangle1, sb, announcerWidth);
                var rectangle2 = new Rectangle((int) (bounds.X + (double) cornerCut + 4.0),
                    rectangle1.Y + rectangle1.Height + 4, (int) (bounds.Width - (cornerCut + 4.0)*2.0), 50);
                if (flag)
                {
                    sb.Draw(scanlinesTextBackground, rectangle2, Utils.makeColorAddative(ThemeColorHighlight)*0.6f);
                    FlickeringTextEffect.DrawFlickeringText(rectangle2, "CLICK TO UPGRADE", 3f, 0.1f, GuiData.titlefont,
                        os, Utils.AddativeWhite);
                }
                else
                {
                    sb.Draw(scanlinesTextBackground, rectangle2,
                        Color.Lerp(os.brightLockedColor, Utils.makeColorAddative(Color.Red), 0.2f + Utils.randm(0.8f))*
                        0.4f);
                    FlickeringTextEffect.DrawFlickeringText(rectangle2, "INSUFFICIENT POINTS", 3f, 0.1f,
                        GuiData.titlefont, os, Utils.AddativeWhite);
                }
            }
            rectangle1 = new Rectangle(bounds.X + 2, bounds.Y + num1 + 4, bounds.Width - 4, 50);
            var f1 = currentRate <= 0.0 ? 0.0f : activeState.points/currentRate;
            if (float.IsNaN(f1))
                f1 = float.PositiveInfinity;
            DrawStatsTextBlock("PPS", currentRate.ToString("000.0") ?? "",
                f1.ToString("00.0") + " seconds to double current points", rectangle1, sb, announcerWidth);
            var num2 = (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
            for (var index = 0; index < UpgradeNotifiers.Count; ++index)
            {
                var upgradeNotifier = UpgradeNotifiers[index];
                upgradeNotifier.timer -= num2*4f;
                if (upgradeNotifier.timer <= 0.0)
                {
                    UpgradeNotifiers.RemoveAt(index);
                    --index;
                }
                else
                {
                    var vector2 = GuiData.font.MeasureString(upgradeNotifier.text);
                    sb.DrawString(GuiData.font, upgradeNotifier.text,
                        new Vector2(
                            rectangle1.X + (float) ((rectangle1.Width - (double) announcerWidth)/2.0) + announcerWidth,
                            rectangle1.Y + 10), ThemeColorHighlight*upgradeNotifier.timer, 0.0f, vector2/2f,
                        (float) (0.5 + (1.0 - upgradeNotifier.timer)*2.20000004768372), SpriteEffects.None, 0.9f);
                    UpgradeNotifiers[index] = upgradeNotifier;
                }
            }
        }

        private void DrawStatsTextBlock(string anouncer, string main, string secondary, Rectangle bounds, SpriteBatch sb,
            float announcerWidth)
        {
            var pos = new Vector2(bounds.X, bounds.Y);
            TextItem.doFontLabel(pos, anouncer, GuiData.font, Utils.AddativeWhite, announcerWidth, bounds.Height);
            pos.X += announcerWidth + 2f;
            TextItem.doFontLabel(new Vector2((float) (bounds.X + (double) announcerWidth - 12.0), bounds.Y), ":",
                GuiData.font, Utils.AddativeWhite, 22f, bounds.Height);
            TextItem.doFontLabel(pos, main, GuiData.font, ThemeColorHighlight, bounds.Width - announcerWidth,
                bounds.Height);
            pos.Y += 29f;
            pos.X = bounds.X;
            TextItem.doFontLabel(pos, "[" + secondary + "]", GuiData.smallfont, Color.Gray, bounds.Width, bounds.Height);
        }

        private void DrawUpgrades(Rectangle bounds, SpriteBatch sb)
        {
            var panelHeight = 28;
            if (scrollerPanel == null)
                scrollerPanel = new ScrollableSectionedPanel(panelHeight, sb.GraphicsDevice);
            scrollerPanel.NumberOfPanels = upgradeNames.Count;
            Button.outlineOnly = true;
            Button.drawingOutline = false;
            var drawnThisCycle = 0;
            var needsButtonChecks = bounds.Contains(GuiData.getMousePoint());
            scrollerPanel.Draw((index, drawAreaFull, sprBatch) =>
            {
                var destinationRectangle = new Rectangle(drawAreaFull.X, drawAreaFull.Y, drawAreaFull.Width - 12,
                    drawAreaFull.Height);
                var myID = 115700 + index*111;
                if (needsButtonChecks &&
                    Button.doButton(myID, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width,
                        destinationRectangle.Height, "", Color.Transparent))
                    PurchaseUpgrade(index);
                else if (!needsButtonChecks && GuiData.hot == myID)
                    GuiData.hot = -1;
                var flag1 = upgradeCosts[index] <= (double) activeState.points;
                var flag2 = flag1 && GuiData.hot == myID;
                if (GuiData.hot == myID)
                    hoverIndex = index;
                if (flag2)
                {
                    var num1 = destinationRectangle.Height;
                    var num2 = 0;
                    var num3 = 0;
                    if (drawAreaFull.X == 0 && drawAreaFull.Y == 0)
                    {
                        if (drawnThisCycle == 0)
                        {
                            num2 = bounds.X;
                            num3 = bounds.Y;
                        }
                        else
                        {
                            num2 = bounds.X;
                            num3 = bounds.Y + bounds.Height - panelHeight/2;
                        }
                    }
                    var rectangle = new Rectangle(num2 + destinationRectangle.X - num1,
                        num3 + destinationRectangle.Y - num1, destinationRectangle.Width + 2*num1,
                        destinationRectangle.Height + 2*num1);
                    for (var index1 = 0; index1 < Stars.Count; ++index1)
                        DrawStar(rectangle, sb, Stars[index1]);
                    sb.Draw(scanlinesTextBackground, rectangle, ThemeColor*(GuiData.active == myID ? 0.6f : 0.3f));
                }
                sprBatch.Draw(scanlinesTextBackground, destinationRectangle,
                    new Rectangle(scanlinesTextBackground.Width/2, scanlinesTextBackground.Height/9*4,
                        scanlinesTextBackground.Width/2, scanlinesTextBackground.Height/4),
                    flag1 ? ThemeColor*0.2f : Utils.AddativeWhite*0.08f);
                if (GuiData.hot == myID)
                    RenderedRectangle.doRectangle(destinationRectangle.X + 1, destinationRectangle.Y + 1,
                        destinationRectangle.Width - 2, destinationRectangle.Height - 2,
                        flag2 ? (GuiData.active == myID ? Color.Black : ThemeColorBacking) : Color.Black);
                if (index == 0)
                    Utils.drawLine(sprBatch, new Vector2(destinationRectangle.X + 1, destinationRectangle.Y + 1),
                        new Vector2(destinationRectangle.X + destinationRectangle.Width - 2, destinationRectangle.Y + 1),
                        Vector2.Zero, ThemeColor, 0.8f);
                Utils.drawLine(sprBatch,
                    new Vector2(destinationRectangle.X + 1, destinationRectangle.Y + destinationRectangle.Height - 2),
                    new Vector2(destinationRectangle.X + destinationRectangle.Width - 2,
                        destinationRectangle.Y + destinationRectangle.Height - 2), Vector2.Zero, ThemeColor, 0.8f);
                if (flag1)
                    sprBatch.Draw(Utils.white,
                        new Rectangle(destinationRectangle.X, destinationRectangle.Y + 1, 8,
                            destinationRectangle.Height - 2), ThemeColor);
                var text = "[" + activeState.upgradeCounts[index].ToString("000") + "] " + upgradeNames[index];
                TextItem.doFontLabel(
                    new Vector2(destinationRectangle.X + 4 + (flag1 ? 10 : 0), destinationRectangle.Y + 4), text,
                    GuiData.UISmallfont,
                    flag2
                        ? (GuiData.active == myID ? ThemeColor : (flag1 ? Color.White : Color.Gray))
                        : (flag1 ? Utils.AddativeWhite : Color.Gray), destinationRectangle.Width - 6, float.MaxValue);
                ++drawnThisCycle;
            }, sb, bounds);
            Button.outlineOnly = false;
            Button.drawingOutline = true;
        }

        private void DrawMonospaceString(Rectangle bounds, SpriteBatch sb, string points)
        {
            points = points.Trim();
            var num1 = 65f;
            var num2 = (points.Length + 1f)*num1;
            var scale = 1f;
            if (num2 > (double) bounds.Width)
                scale = bounds.Width/num2;
            var num3 = num2*scale;
            var num4 = (float) ((bounds.Width - (double) num3)/2.0);
            var vector2_1 = new Vector2(bounds.X + num4, bounds.Y);
            for (var index = 0; index < points.Length; ++index)
            {
                var text = string.Concat(points[index]);
                var vector2_2 = GuiData.titlefont.MeasureString(text);
                var x = (float) (num1*(double) scale - vector2_2.X*(double) scale/2.0);
                sb.DrawString(GuiData.titlefont, text, vector2_1 + new Vector2(x, 0.0f), Color.White, 0.0f, Vector2.Zero,
                    scale, SpriteEffects.None, 0.67f);
                vector2_1.X += num1*scale;
            }
        }

        private void DrawWelcome(Rectangle bounds, SpriteBatch sb)
        {
            var num1 = logoRenderBase.Height/(float) logoRenderBase.Width;
            var num2 = 45;
            var dest = new Rectangle(bounds.X - num2 + 20, bounds.Y, bounds.Width + num2,
                (int) ((bounds.Width + 2*num2)*(double) num1));
            DrawLogo(dest, sb);
            var rectangle = new Rectangle(bounds.X, dest.Y + dest.Height, bounds.Width, 60);
            sb.Draw(scanlinesTextBackground, rectangle, Utils.AddativeWhite*0.2f);
            for (var index = 0; index < Stars.Count; ++index)
                DrawStar(rectangle, sb, Stars[index]);
            rectangle.X += 100;
            rectangle.Width = bounds.Width - 200;
            rectangle.Y += 13;
            rectangle.Height = 35;
            if (Button.doButton(98373721, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, "GO!",
                Utils.AddativeWhite))
            {
                activeState = null;
                var str = os.defaultUser.name;
                for (var index = 0; index < savesFolder.files.Count; ++index)
                {
                    if (savesFolder.files[index].name.StartsWith(str))
                    {
                        userFilePath = savesFolder.files[index].name;
                        activeState = PointClickerGameState.LoadFromString(savesFolder.files[index].data);
                        break;
                    }
                }
                if (activeState == null)
                {
                    activeState = new PointClickerGameState(upgradeNames.Count);
                    var fileEntry = new FileEntry(activeState.ToSaveString(), str + ".pcsav");
                    savesFolder.files.Add(fileEntry);
                    userFilePath = fileEntry.name;
                }
                state = PointClickerScreenState.Main;
                currentRate = 0.0f;
                ActiveStory = "";
                UpdateRate();
                UpdateStory();
                UpdatePoints();
            }
            if (
                !Button.doButton(98373732, bounds.X + 2, bounds.Y + bounds.Height - 19, 180, 18, "Exit  :<",
                    os.lockedColor))
                return;
            os.display.command = "connect";
        }

        private struct PointClickerStar
        {
            public Vector2 Pos;
            public float scale;
            public float life;
            public float rot;
            public float timescale;
            public Color color;
        }

        private struct UpgradeNotifier
        {
            public string text;
            public float timer;
        }

        private class PointClickerGameState
        {
            public int currentStoryElement;
            public long points;
            public readonly List<int> upgradeCounts;

            public PointClickerGameState(int upgradesTotal)
            {
                currentStoryElement = -1;
                points = 0L;
                upgradeCounts = new List<int>();
                for (var index = 0; index < upgradesTotal; ++index)
                    upgradeCounts.Add(0);
            }

            public string ToSaveString()
            {
                var str = string.Concat(points, "\n", currentStoryElement, "\n");
                for (var index = 0; index < upgradeCounts.Count; ++index)
                    str = str + upgradeCounts[index] + ",";
                return str;
            }

            public static PointClickerGameState LoadFromString(string save)
            {
                var clickerGameState = new PointClickerGameState(0);
                var strArray = save.Split(Utils.newlineDelim);
                clickerGameState.points = Convert.ToInt64(strArray[0]);
                clickerGameState.currentStoryElement = Convert.ToInt32(strArray[1]);
                foreach (var str in strArray[2].Split(Utils.commaDelim, StringSplitOptions.RemoveEmptyEntries))
                    clickerGameState.upgradeCounts.Add(Convert.ToInt32(str));
                return clickerGameState;
            }
        }

        private enum PointClickerScreenState
        {
            Welcome,
            Error,
            Main
        }
    }
}
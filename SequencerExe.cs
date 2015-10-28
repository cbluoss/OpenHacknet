using System;
using System.Collections.Generic;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace Hacknet
{
    internal class SequencerExe : ExeModule
    {
        public static int ACTIVATING_RAM_COST = 170;
        public static int BASE_RAM_COST = 60;
        public static float RAM_CHANGE_PS = 100f;
        public static double Song_Length = 186.0;
        public static float SPIN_UP_TIME = 17f;
        private static readonly float TimeBetweenBeats = 1.832061f;
        private readonly MovingBarsEffect bars = new MovingBarsEffect();
        private readonly double beatDropTime = 16.64;
        private readonly float beatHits = 0.15f;
        private string flagForProgressionName;
        private bool HasBeenKilled;
        private readonly List<ConnectedNodeEffect> nodeeffects = new List<ConnectedNodeEffect>();
        private string oldSongName;
        private OSTheme originalTheme;
        private SequencerExeState state;
        private float stateTimer;
        private Computer targetComp;
        private string targetID;
        private int targetRamUse = ACTIVATING_RAM_COST;
        private OSTheme targetTheme = OSTheme.HacknetWhite;

        public SequencerExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            name = "Sequencer";
            ramCost = ACTIVATING_RAM_COST;
            IdentifierName = "Sequencer";
            targetIP = os.thisComputer.ip;
            bars.MinLineChangeTime = 1f;
            bars.MaxLineChangeTime = 3f;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            nodeeffects.Add(new ConnectedNodeEffect(os, true));
            nodeeffects.Add(new ConnectedNodeEffect(os, true));
            nodeeffects.Add(new ConnectedNodeEffect(os, true));
            nodeeffects.Add(new ConnectedNodeEffect(os, true));
            nodeeffects.Add(new ConnectedNodeEffect(os, true));
            if (Settings.isPressBuildDemo)
            {
                targetID = "finalNodeDemo";
                flagForProgressionName = "DemoSequencerEnabled";
            }
            else
            {
                targetID = "EnTechOfflineBackup";
                flagForProgressionName = "VaporSequencerEnabled";
            }
            if (ThemeManager.currentTheme != OSTheme.HacknetWhite)
                return;
            targetTheme = OSTheme.HacknetBlue;
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (HasBeenKilled)
                return;
            bars.Update(t);
            UpdateRamCost(t);
            stateTimer += t;
            switch (state)
            {
                case SequencerExeState.Unavaliable:
                    if (os.Flags.HasFlag(flagForProgressionName) || Settings.debugCommandsEnabled)
                    {
                        state = SequencerExeState.AwaitingActivation;
                        break;
                    }
                    bars.MinLineChangeTime = 1f;
                    bars.MaxLineChangeTime = 3f;
                    break;
                case SequencerExeState.SpinningUp:
                    if (MediaPlayer.State == MediaState.Playing)
                    {
                        if (MediaPlayer.PlayPosition.TotalSeconds < beatDropTime || stateTimer <= 10.0)
                            break;
                        MoveToActiveState();
                        break;
                    }
                    if (stateTimer <= (double) SPIN_UP_TIME)
                        break;
                    MoveToActiveState();
                    break;
                case SequencerExeState.Active:
                    var num = 2.5f;
                    if (stateTimer < (double) num)
                    {
                        if (Utils.randm(1f) < 0.300000011920929 + stateTimer/(double) num*0.699999988079071)
                        {
                            ThemeManager.switchThemeColors(os, targetTheme);
                            ThemeManager.loadThemeBackground(os, targetTheme);
                            ThemeManager.currentTheme = targetTheme;
                        }
                        else
                        {
                            ThemeManager.switchThemeColors(os, originalTheme);
                            ThemeManager.loadThemeBackground(os, originalTheme);
                            ThemeManager.currentTheme = originalTheme;
                        }
                        if ((MediaPlayer.PlayPosition.TotalSeconds - beatDropTime)%beatHits < 0.00999999977648258)
                            os.warningFlash();
                    }
                    ActiveStateUpdate(t);
                    break;
            }
        }

        private void ActiveStateUpdate(float t)
        {
            PostProcessor.dangerModeEnabled = true;
            double num = stateTimer;
            if (MediaPlayer.State == MediaState.Playing && !Settings.soundDisabled)
                num = MediaPlayer.PlayPosition.TotalSeconds;
            PostProcessor.dangerModePercentComplete = (float) ((num - SPIN_UP_TIME)/(Song_Length - SPIN_UP_TIME));
            if (PostProcessor.dangerModePercentComplete >= 1.0)
            {
                if (Settings.isDemoMode)
                {
                    MissionFunctions.runCommand(0, "demoFinalMissionEnd");
                }
                else
                {
                    os.netMap.visibleNodes.Remove(os.netMap.nodes.IndexOf(targetComp));
                    PostProcessor.dangerModeEnabled = false;
                    PostProcessor.dangerModePercentComplete = 0.0f;
                    os.thisComputerCrashed();
                }
            }
            if (os.connectedComp == null && stateTimer > 1.0 && !Settings.isDemoMode)
            {
                isExiting = true;
                if (oldSongName != null)
                {
                    MusicManager.transitionToSong("Music/Ambient/AmbientDrone_Clipped");
                    MediaPlayer.IsRepeating = true;
                }
                os.netMap.visibleNodes.Remove(os.netMap.nodes.IndexOf(targetComp));
                PostProcessor.dangerModeEnabled = false;
                PostProcessor.dangerModePercentComplete = 0.0f;
            }
            if ((num - beatDropTime)%TimeBetweenBeats >= t)
                return;
            os.warningFlash();
        }

        private void UpdateRamCost(float t)
        {
            if (targetRamUse == ramCost)
                return;
            if (targetRamUse < ramCost)
            {
                ramCost -= (int) (t*(double) RAM_CHANGE_PS);
                if (ramCost >= targetRamUse)
                    return;
                ramCost = targetRamUse;
            }
            else
            {
                var num = (int) (t*(double) RAM_CHANGE_PS);
                if (os.ramAvaliable < num)
                    return;
                ramCost += num;
                if (ramCost <= targetRamUse)
                    return;
                ramCost = targetRamUse;
            }
        }

        public override void Killed()
        {
            base.Killed();
            HasBeenKilled = true;
            PostProcessor.dangerModeEnabled = false;
            PostProcessor.dangerModePercentComplete = 0.0f;
            if (oldSongName != null)
            {
                MusicManager.transitionToSong("Music/Ambient/AmbientDrone_Clipped");
                MediaPlayer.IsRepeating = true;
            }
            os.netMap.DimNonConnectedNodes = false;
            os.runCommand("disconnect");
            if (targetComp == null)
                return;
            os.netMap.visibleNodes.Remove(os.netMap.nodes.IndexOf(targetComp));
        }

        private void MoveToActiveState()
        {
            state = SequencerExeState.Active;
            stateTimer = 0.0f;
            targetRamUse = BASE_RAM_COST;
            os.warningFlashTimer = OS.WARNING_FLASH_TIME;
            os.netMap.DimNonConnectedNodes = true;
            os.netMap.discoverNode(targetComp);
            os.runCommand("connect " + targetComp.ip);
            os.delayer.Post(ActionDelayer.Wait(0.05), () => os.runCommand("probe"));
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            var rectangle = Utils.InsetRectangle(GetContentAreaDest(), 1);
            var amount = os.warningFlashTimer/OS.WARNING_FLASH_TIME;
            var minHeight = 2f;
            if (amount > 0.0)
                minHeight += amount*(rectangle.Height - minHeight);
            var drawColor = Color.Lerp(Utils.AddativeWhite*0.5f, Utils.AddativeRed, amount);
            bars.Draw(spriteBatch, GetContentAreaDest(), minHeight, 4f, 1f, drawColor);
            switch (state)
            {
                case SequencerExeState.Unavaliable:
                    spriteBatch.Draw(Utils.white, rectangle, Color.Black*0.5f);
                    var dest = Utils.InsetRectangle(rectangle, 6);
                    if (!isExiting)
                        TextItem.doFontLabelToSize(dest, "LINK UNAVAILABLE", GuiData.titlefont, Utils.AddativeWhite);
                    var destinationRectangle1 = dest;
                    destinationRectangle1.Y += destinationRectangle1.Height - 20;
                    destinationRectangle1.Height = 20;
                    if (isExiting)
                        break;
                    GuiData.spriteBatch.Draw(Utils.white, destinationRectangle1, Color.Black*0.5f);
                    if (
                        !Button.doButton(32711803, destinationRectangle1.X, destinationRectangle1.Y,
                            destinationRectangle1.Width, destinationRectangle1.Height, "Exit", os.lockedColor))
                        break;
                    isExiting = true;
                    break;
                case SequencerExeState.AwaitingActivation:
                    var height = 30;
                    var destinationRectangle2 = new Rectangle(this.bounds.X + 1,
                        this.bounds.Y + this.bounds.Height/2 - height, this.bounds.Width - 2, height*2);
                    spriteBatch.Draw(Utils.white, destinationRectangle2, Color.Black*0.92f);
                    if (
                        !Button.doButton(8310101, this.bounds.X + 10, this.bounds.Y + this.bounds.Height/2 - height/2,
                            this.bounds.Width - 20, height, "ACTIVATE", os.highlightColor))
                        break;
                    stateTimer = 0.0f;
                    state = SequencerExeState.SpinningUp;
                    bars.MinLineChangeTime = 0.1f;
                    bars.MaxLineChangeTime = 1f;
                    originalTheme = ThemeManager.currentTheme;
                    MusicManager.FADE_TIME = 0.6f;
                    oldSongName = MusicManager.currentSongName;
                    MusicManager.transitionToSong("Music\\Roller_Mobster_Clipped");
                    MediaPlayer.IsRepeating = false;
                    targetComp = Programs.getComputer(os, targetID);
                    var webServerDaemon = (WebServerDaemon) targetComp.getDaemon(typeof (WebServerDaemon));
                    if (webServerDaemon == null)
                        break;
                    webServerDaemon.LoadWebPage("index.html");
                    break;
                case SequencerExeState.SpinningUp:
                    var bounds = rectangle;
                    bounds.Height = (int) (bounds.Height*(stateTimer/(double) SPIN_UP_TIME));
                    bounds.Y = rectangle.Y + rectangle.Height - bounds.Height + 1;
                    bounds.Width += 4;
                    bars.Draw(spriteBatch, bounds, minHeight, 4f, 1f, os.brightLockedColor);
                    break;
                case SequencerExeState.Active:
                    spriteBatch.Draw(Utils.white, GetContentAreaDest(), Color.Black*0.5f);
                    TextItem.doFontLabelToSize(GetContentAreaDest(), " G O   G O   G O ", GuiData.titlefont,
                        Color.Lerp(Utils.AddativeRed, os.brightLockedColor, Math.Min(1f, stateTimer/2f)));
                    DrawActiveState();
                    break;
            }
        }

        private void DrawActiveState()
        {
            var val1 = 5.2f;
            var point = Math.Min(val1, stateTimer)/val1;
            var num1 = 30f;
            var vector2 = new Vector2(os.netMap.bounds.X, os.netMap.bounds.Y) + new Vector2(NetworkMap.NODE_SIZE/2f);
            for (var index = 0; index < nodeeffects.Count; ++index)
            {
                var num2 = (index + 1)/(float) (nodeeffects.Count + 1);
                var num3 = 3f*num2;
                var num4 = 1f - Utils.QuadraticOutCurve(Utils.QuadraticOutCurve(Utils.QuadraticOutCurve(point)));
                nodeeffects[index].ScaleFactor = num2*(num1*num4) + num3;
                nodeeffects[index].draw(spriteBatch, vector2 + os.netMap.GetNodeDrawPos(targetComp.location));
            }
            DrawCountdownOverlay();
        }

        private void DrawCountdownOverlay()
        {
            var height = 110;
            var destinationRectangle = new Rectangle(0, os.fullscreen.Height - height - 20, 400, height);
            var color = new Color(100, 0, 0, 200);
            spriteBatch.Draw(Utils.white, destinationRectangle, Color.Lerp(color, Color.Transparent, Utils.randm(0.2f)));
            var pos = new Vector2(destinationRectangle.X + 6, destinationRectangle.Y + 4);
            TextItem.doFontLabel(pos, "ENTECH SEQUENCER ATTACK", GuiData.titlefont, Color.White,
                destinationRectangle.Width - 12, 35f);
            pos.Y += 32f;
            TextItem.doFontLabel(pos,
                Settings.isDemoMode
                    ? "Analyze security countermeasures with \"Probe\""
                    : "Break active security on target", GuiData.smallfont, Color.White, destinationRectangle.Width - 10,
                20f);
            pos.Y += 16f;
            TextItem.doFontLabel(pos,
                Settings.isDemoMode
                    ? "Break active security and gain access (Programs + Porthack)"
                    : "Delete all Hacknet related files", GuiData.smallfont, Color.White,
                destinationRectangle.Width - 10, 20f);
            pos.Y += 16f;
            TextItem.doFontLabel(pos,
                Settings.isDemoMode ? "Delete all files in directories /sys/ and /log/" : "Disconnect",
                GuiData.smallfont, Color.White, destinationRectangle.Width - 10, 20f);
        }

        private enum SequencerExeState
        {
            Unavaliable,
            AwaitingActivation,
            SpinningUp,
            Active
        }
    }
}
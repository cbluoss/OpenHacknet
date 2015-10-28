using System;
using System.Collections.Generic;
using Hacknet.Daemons.Helpers;
using Hacknet.Gui;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class HeartMonitorDaemon : Daemon
    {
        private const string ActiveFirmwareFilename = "LiveFirmware.dll";
        private const string FolderName = "KBT_Pacemaker";
        private const string LiveFolderName = "Active";
        private const string SubLoginUsername = "EAdmin";
        private const string SubLoginPass = "tens86";
        private const float MinFirmwareLoadTime = 10f;
        private const float TimeToDeathInDanger = 21f;
        private const float DyingDangerTime = 6f;
        private const float DeadBeepSustainFadeOut = 16f;
        private const float DeadBeepSustainFadeoutStartDelay = 5f;
        private float alarmHeartOKTimer;
        private float averageSPO2;
        private readonly float beatTime = 0.18f;
        private readonly SoundEffect beepSound;
        private readonly SoundEffectInstance beepSustainSound;
        private readonly Color bloomColor = new Color(10, 10, 10, 0);
        private RenderTarget2D bloomTarget;
        private readonly Effect blufEffect;
        private SpriteBatch BlurContentSpritebatch;
        private BasicMedicalMonitor BPMonitor;
        private float currentSPO2;
        private float firmwareLoadTime;
        private bool HasSecondaryLogin;
        private readonly Texture2D Heart;
        private readonly Color heartColor = new Color(247, 237, 125);
        private BasicMedicalMonitor HeartMonitor;
        private int HeartRate;
        private bool isConfirmingSelection;
        private string loginPass;
        private string loginUsername;
        private readonly List<IMedicalMonitor> Monitors = new List<IMedicalMonitor>();
        private bool opOpening;
        private float opTransition;
        private readonly Texture2D OxyIcon;
        private bool PatientDead;
        public string PatientID = "UNKNOWN";
        private bool PatientInCardiacArrest;
        private float PatientTimeInDanger;

        private readonly List<Action<int, int, SpriteBatch>> PostBloomDrawCalls =
            new List<Action<int, int, SpriteBatch>>();

        private RenderTarget2D priorTarget;
        private float projectionFowardsTime = 0.3f;
        private int reportedSP02 = 95;
        private RenderTarget2D secondaryBloomTarget;
        private string selectedFirmwareData = "";
        private int selectedFirmwareIndex;
        private string selectedFirmwareName = "";
        private BasicMedicalMonitor SPMonitor;
        private HeartMonitorState State;
        private readonly float timeBetweenHeartbeats = 0.8823529f;
        private float timeDead;
        private float timeSinceLastHeartBeat = float.MaxValue;
        private float timeSinceNormalHeartRate;
        private float timeThisState;
        private float timeTillNextHeartbeat;
        private readonly float volume = 0.4f;
        private readonly Texture2D WarnIcon;

        public HeartMonitorDaemon(Computer c, OS os)
            : base(c, "Remote Monitor", os)
        {
            blufEffect = os.content.Load<Effect>("Shaders/DOFBlur");
            blufEffect.CurrentTechnique = blufEffect.Techniques["SmoothGaussBlur"];
            Heart = os.content.Load<Texture2D>("Sprites/Icons/Heart");
            OxyIcon = os.content.Load<Texture2D>("Sprites/Icons/O2Img");
            WarnIcon = os.content.Load<Texture2D>("Sprites/Icons/CautionIcon");
            beepSound = os.content.Load<SoundEffect>("SFX/HeartMonitorBeep");
            beepSustainSound = os.content.Load<SoundEffect>("SFX/HeartMonitorSustain").CreateInstance();
            beepSustainSound.IsLooped = true;
            beepSustainSound.Volume = volume;
            SetUpMonitors();
        }

        public override string getSaveString()
        {
            return "<HeartMonitor patient=\"" + PatientID + "\" />";
        }

        public override void initFiles()
        {
            base.initFiles();
            var folder1 = comp.files.root.searchForFolder("KBT_Pacemaker");
            if (folder1 == null)
            {
                folder1 = new Folder("KBT_Pacemaker");
                comp.files.root.folders.Add(folder1);
            }
            var folder2 = folder1.searchForFolder("Active") ?? new Folder("Active");
            folder1.folders.Add(folder2);
            var fileEntry1 = new FileEntry(PortExploits.ValidPacemakerFirmware, "KBT_Firmware_v1.2.dll");
            folder1.files.Add(fileEntry1);
            var fileEntry2 = new FileEntry(PortExploits.ValidPacemakerFirmware, "LiveFirmware.dll");
            folder2.files.Add(fileEntry1);
        }

        private void SetUpMonitors()
        {
            HeartMonitor = new BasicMedicalMonitor((lastVal, dt) =>
            {
                var list = new List<BasicMedicalMonitor.MonitorRecordKeypoint>();
                var monitorRecordKeypoint = new BasicMedicalMonitor.MonitorRecordKeypoint();
                monitorRecordKeypoint.timeOffset = dt;
                monitorRecordKeypoint.value = lastVal;
                var num = PatientDead ? 0.05f : 0.25f;
                if (lastVal > (double) num)
                    monitorRecordKeypoint.value -= dt*0.5f;
                else if (lastVal < -1.0*num)
                    monitorRecordKeypoint.value +=
                        (float) (dt*0.5*(PatientDead ? 0.5*Math.Max((float) (1.0 - timeDead/16.0), 0.0f) : 1.0));
                else
                    monitorRecordKeypoint.value += (float) ((Utils.randm(0.2f) - 0.100000001490116)*0.300000011920929);
                list.Add(monitorRecordKeypoint);
                return list;
            }, (lastVal, dt) =>
            {
                var list = new List<BasicMedicalMonitor.MonitorRecordKeypoint>();
                if (!PatientDead)
                {
                    list.Add(new BasicMedicalMonitor.MonitorRecordKeypoint
                    {
                        timeOffset = dt/3f,
                        value = (float) (Utils.randm(0.2f) - 0.100000001490116 - 0.800000011920929)
                    });
                    list.Add(new BasicMedicalMonitor.MonitorRecordKeypoint
                    {
                        timeOffset = dt/3f,
                        value = (float) (0.899999976158142 + (Utils.randm(0.1f) - 0.0500000007450581))
                    });
                    list.Add(new BasicMedicalMonitor.MonitorRecordKeypoint
                    {
                        timeOffset = dt/3f,
                        value = Utils.randm(0.1f*dt) - 0.05f
                    });
                }
                return list;
            });
            Monitors.Add(HeartMonitor);
            BPMonitor = new BasicMedicalMonitor((lastVal, dt) =>
            {
                var list = new List<BasicMedicalMonitor.MonitorRecordKeypoint>();
                var monitorRecordKeypoint = new BasicMedicalMonitor.MonitorRecordKeypoint();
                monitorRecordKeypoint.timeOffset = dt;
                monitorRecordKeypoint.value = lastVal;
                if (lastVal > 0.25)
                    monitorRecordKeypoint.value -= dt*0.5f;
                else if (lastVal < -0.25)
                    monitorRecordKeypoint.value += dt*0.5f;
                else
                    monitorRecordKeypoint.value +=
                        (float)
                            ((Utils.randm(0.2f) - 0.100000001490116)*0.300000011920929*
                             (PatientDead ? 0.5*Math.Max((float) (1.0 - timeDead/16.0), 0.0f) : 1.0));
                list.Add(monitorRecordKeypoint);
                return list;
            }, (lastVal, dt) => new List<BasicMedicalMonitor.MonitorRecordKeypoint>
            {
                new BasicMedicalMonitor.MonitorRecordKeypoint
                {
                    timeOffset = dt/3f,
                    value = (float) (Utils.randm(0.2f) - 0.100000001490116 - 0.800000011920929)
                },
                new BasicMedicalMonitor.MonitorRecordKeypoint
                {
                    timeOffset = dt/3f,
                    value = (float) (0.899999976158142 + (Utils.randm(0.1f) - 0.0500000007450581))
                },
                new BasicMedicalMonitor.MonitorRecordKeypoint
                {
                    timeOffset = dt/3f,
                    value = Utils.randm(0.1f*dt) - 0.05f
                }
            });
            Monitors.Add(BPMonitor);
            SPMonitor = new BasicMedicalMonitor((lastVal, dt) =>
            {
                var list = new List<BasicMedicalMonitor.MonitorRecordKeypoint>();
                var monitorRecordKeypoint = new BasicMedicalMonitor.MonitorRecordKeypoint();
                monitorRecordKeypoint.timeOffset = dt;
                monitorRecordKeypoint.value = lastVal;
                if (lastVal > 0.800000011920929)
                    monitorRecordKeypoint.value += (float) ((Utils.randm(0.2f) - 0.100000001490116)*0.300000011920929);
                else
                    monitorRecordKeypoint.value +=
                        (float)
                            (0.150000005960464*(Utils.random.NextDouble()*Utils.random.NextDouble())*
                             (PatientDead ? 0.5*Math.Max((float) (1.0 - timeDead/16.0), 0.0f) : 1.0));
                list.Add(monitorRecordKeypoint);
                return list;
            }, (lastVal, dt) =>
            {
                var list = new List<BasicMedicalMonitor.MonitorRecordKeypoint>();
                var monitorRecordKeypoint = new BasicMedicalMonitor.MonitorRecordKeypoint();
                monitorRecordKeypoint.timeOffset = dt*0.7f;
                monitorRecordKeypoint.value = -0.6f - Utils.randm(0.4f);
                list.Add(monitorRecordKeypoint);
                list.Add(new BasicMedicalMonitor.MonitorRecordKeypoint
                {
                    timeOffset = dt*0.3f,
                    value = (float) (monitorRecordKeypoint.value - (double) Utils.randm(0.15f) - 0.0500000007450581)
                });
                return list;
            });
            Monitors.Add(SPMonitor);
        }

        private void UpdateReports(float dt)
        {
            timeSinceLastHeartBeat += dt;
            currentSPO2 = 1f - SPMonitor.GetCurrentValue(projectionFowardsTime);
            averageSPO2 = (float) (averageSPO2*0.959999978542328 + currentSPO2*0.0399999991059303);
            reportedSP02 = Math.Min(100, 90 + (int) (5.0*averageSPO2 + 0.5));
            if (HeartRate > 120 || HeartRate < 50)
            {
                timeSinceNormalHeartRate += dt;
            }
            else
            {
                alarmHeartOKTimer += dt;
                if (alarmHeartOKTimer <= 10.0)
                    return;
                timeSinceNormalHeartRate = 0.0f;
                alarmHeartOKTimer = 0.0f;
            }
        }

        private void UpdateReportsForHeartbeat()
        {
            HeartRate = (int) (60.0/timeSinceLastHeartBeat + 0.5);
            timeSinceLastHeartBeat = 0.0f;
        }

        private void ChangeState(HeartMonitorState newState)
        {
            if (State == HeartMonitorState.MainDisplay && newState == HeartMonitorState.MainDisplay)
                return;
            if (newState == HeartMonitorState.MainDisplay)
            {
                if (State != HeartMonitorState.Welcome)
                {
                    opOpening = false;
                    opTransition = 0.0f;
                }
                else
                {
                    opOpening = false;
                    opTransition = 2f;
                }
            }
            else if (State == HeartMonitorState.MainDisplay)
            {
                opOpening = true;
                opTransition = 0.0f;
            }
            State = newState;
            timeThisState = 0.0f;
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            ChangeState(HeartMonitorState.Welcome);
            if (!os.Flags.HasFlag(PatientID + ":DEAD"))
                return;
            PatientDead = true;
        }

        private void UpdateStates(float dt)
        {
            timeThisState += dt;
            switch (State)
            {
                case HeartMonitorState.MainDisplay:
                    opTransition += (opOpening ? 1f : 2f)*dt;
                    break;
                case HeartMonitorState.SecondaryLogin:
                case HeartMonitorState.SecondaryLoginRunning:
                case HeartMonitorState.Error:
                case HeartMonitorState.FirmwareScreen:
                case HeartMonitorState.FirmwareScreenConfirm:
                    opTransition += dt;
                    break;
                case HeartMonitorState.FirmwareScreenLoading:
                    opTransition += dt;
                    firmwareLoadTime += dt*(float) Utils.random.NextDouble();
                    if (firmwareLoadTime <= 10.0)
                        break;
                    EnactFirmwareChange();
                    ChangeState(HeartMonitorState.FirmwareScreenComplete);
                    firmwareLoadTime = 0.0f;
                    break;
                case HeartMonitorState.FirmwareScreenComplete:
                    opTransition += dt;
                    firmwareLoadTime += dt;
                    if (firmwareLoadTime <= 3.33333325386047)
                        break;
                    ChangeState(HeartMonitorState.MainDisplay);
                    break;
            }
        }

        public void ForceStopBeepSustainSound()
        {
            if (beepSustainSound == null)
                return;
            beepSustainSound.Stop();
        }

        private void Update(float dt)
        {
            os.delayer.Post(ActionDelayer.Wait(os.lastGameTime.ElapsedGameTime.TotalSeconds*1.999), () =>
            {
                if (!(os.display.command != name) || !PatientDead)
                    return;
                beepSustainSound.Stop(true);
            });
            if (PatientInCardiacArrest)
            {
                PatientTimeInDanger += dt;
                if (PatientTimeInDanger > 21.0)
                {
                    PatientDead = true;
                    HeartRate = 0;
                    os.Flags.AddFlag(PatientID + ":DEAD");
                    beepSustainSound.Play();
                }
                else
                {
                    var num = 3.5f;
                    if (PatientTimeInDanger - (double) dt < num && PatientTimeInDanger >= (double) num &&
                        os.currentMission.postingTitle == "Project Junebug")
                    {
                        MusicManager.FADE_TIME = 10f;
                        MusicManager.transitionToSong("Music/Ambient/dark_drone_008");
                    }
                }
            }
            else
                PatientTimeInDanger = 0.0f;
            timeTillNextHeartbeat -= dt;
            if (timeTillNextHeartbeat <= 0.0 && !PatientDead)
            {
                timeTillNextHeartbeat = timeBetweenHeartbeats + (Utils.randm(0.1f) - 0.05f);
                if (PatientInCardiacArrest)
                    timeTillNextHeartbeat = PatientTimeInDanger <= 15.0
                        ? Utils.randm(timeBetweenHeartbeats*2f)
                        : (float) (0.360000014305115 - 0.25*(PatientTimeInDanger/21.0));
                projectionFowardsTime += beatTime + dt;
                for (var index = 0; index < Monitors.Count; ++index)
                    Monitors[index].HeartBeat(beatTime);
                UpdateReportsForHeartbeat();
                if (State != HeartMonitorState.Welcome)
                    os.delayer.Post(ActionDelayer.Wait(projectionFowardsTime + beatTime/4.0),
                        () => beepSound.Play(volume, 0.0f, 0.0f));
            }
            else if (projectionFowardsTime > 0.300000011920929)
            {
                projectionFowardsTime -= dt;
            }
            else
            {
                for (var index = 0; index < Monitors.Count; ++index)
                    Monitors[index].Update(dt);
            }
            if (PatientDead)
            {
                timeDead += dt;
                if (timeDead > 5.0)
                    beepSustainSound.Volume = Math.Max(0.0f, (float) (1.0 - (timeDead - 5.0)/16.0)*volume);
            }
            UpdateStates(dt);
            UpdateReports(dt);
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            Update((float) os.lastGameTime.ElapsedGameTime.TotalSeconds);
            if (bloomTarget == null || bloomTarget.Width != bounds.Width || bloomTarget.Height != bounds.Height)
            {
                bloomTarget = new RenderTarget2D(sb.GraphicsDevice, bounds.Width, bounds.Height);
                secondaryBloomTarget = new RenderTarget2D(sb.GraphicsDevice, bounds.Width, bounds.Height);
            }
            if (BlurContentSpritebatch == null)
                BlurContentSpritebatch = new SpriteBatch(sb.GraphicsDevice);
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            PostBloomDrawCalls.Clear();
            StartBloomDraw(BlurContentSpritebatch);
            var rectangle = bounds;
            rectangle.X = rectangle.Y = 0;
            var spriteBatch = GuiData.spriteBatch;
            GuiData.spriteBatch = BlurContentSpritebatch;
            DrawStates(rectangle, BlurContentSpritebatch);
            GuiData.spriteBatch = spriteBatch;
            EndBloomDraw(bounds, rectangle, sb, BlurContentSpritebatch);
            for (var index = 0; index < PostBloomDrawCalls.Count; ++index)
                PostBloomDrawCalls[index](bounds.X, bounds.Y, sb);
            TextItem.DrawShadow = flag;
        }

        private void DrawStates(Rectangle bounds, SpriteBatch sb)
        {
            switch (State)
            {
                case HeartMonitorState.Welcome:
                    DrawWelcomeScreen(bounds, sb);
                    break;
                default:
                    DrawSegments(bounds, sb);
                    break;
            }
            if (State == HeartMonitorState.Welcome)
                return;
            DrawOptionsPanel(bounds, sb);
        }

        private void StartBloomDraw(SpriteBatch sb)
        {
            priorTarget = (RenderTarget2D) sb.GraphicsDevice.GetRenderTargets()[0].RenderTarget;
            sb.GraphicsDevice.SetRenderTarget(bloomTarget);
            sb.GraphicsDevice.Clear(Color.Transparent);
            sb.Begin();
        }

        private void EndBloomDraw(Rectangle bounds, Rectangle zeroedBounds, SpriteBatch mainSB,
            SpriteBatch bloomContentSpritebatch)
        {
            bloomContentSpritebatch.End();
            mainSB.GraphicsDevice.SetRenderTarget(secondaryBloomTarget);
            mainSB.GraphicsDevice.Clear(Color.Transparent);
            bloomContentSpritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                DepthStencilState.Default, RasterizerState.CullNone, blufEffect);
            zeroedBounds.X -= 2;
            bloomContentSpritebatch.Draw(bloomTarget, zeroedBounds, bloomColor);
            zeroedBounds.X += 4;
            bloomContentSpritebatch.Draw(bloomTarget, zeroedBounds, bloomColor);
            zeroedBounds.X -= 2;
            zeroedBounds.Y -= 2;
            bloomContentSpritebatch.Draw(bloomTarget, zeroedBounds, bloomColor);
            zeroedBounds.Y += 4;
            bloomContentSpritebatch.Draw(bloomTarget, zeroedBounds, bloomColor);
            bloomContentSpritebatch.End();
            mainSB.GraphicsDevice.SetRenderTarget(priorTarget);
            mainSB.Draw(bloomTarget, bounds, Color.White);
            mainSB.Draw(secondaryBloomTarget, bounds, Color.White);
        }

        public void DrawSegments(Rectangle bounds, SpriteBatch sb)
        {
            var num = 26;
            var dest1 = bounds;
            dest1.Height = bounds.Height/4 - 4;
            DrawGraph(HeartMonitor, dest1, heartColor, sb, true);
            var rectangle = dest1;
            rectangle.Y += rectangle.Height + 2;
            var HRM_DisplayBounds = rectangle;
            HRM_DisplayBounds.Width = rectangle.Width/3;
            var showingHeartIcon = timeSinceLastHeartBeat > beatTime*1.60000002384186 &&
                                   timeSinceLastHeartBeat < 4.0*beatTime;
            PostBloomDrawCalls.Add((x, y, sprBatch) =>
            {
                var bounds1 = HRM_DisplayBounds;
                bounds1.X += x;
                bounds1.Y += y;
                DrawMonitorNumericalDisplay(bounds1, "HR", string.Concat(HeartRate), sprBatch, heartColor,
                    showingHeartIcon ? Heart : null);
            });
            var statusMonitorBounds = rectangle;
            statusMonitorBounds.Width -= HRM_DisplayBounds.Width + 8 + num;
            statusMonitorBounds.X += HRM_DisplayBounds.Width + 4 + num;
            PostBloomDrawCalls.Add((x, y, sprBatch) =>
            {
                var bounds1 = statusMonitorBounds;
                bounds1.X += x;
                bounds1.Y += y;
                var str = "OK";
                var col = Color.Gray;
                Texture2D icon = null;
                if (timeSinceNormalHeartRate > 0.5)
                {
                    if (timeSinceNormalHeartRate > 4.0)
                    {
                        str = "DANGER";
                        col = Color.Red;
                        if (os.timer%0.300000011920929 < 0.100000001490116)
                            icon = WarnIcon;
                    }
                    else
                    {
                        str = "WARN";
                        col = Color.Yellow;
                        if (os.timer%0.5 < 0.200000002980232)
                            icon = WarnIcon;
                    }
                }
                DrawMonitorStatusPanelDisplay(bounds1, "ALARM", str, sprBatch, col, icon);
            });
            var BPColor = new Color(148, 231, 243);
            var BP_DisplayBounds = HRM_DisplayBounds;
            BP_DisplayBounds.Y += BP_DisplayBounds.Height + 4;
            PostBloomDrawCalls.Add((x, y, sprBatch) =>
            {
                var bounds1 = BP_DisplayBounds;
                bounds1.X += x;
                bounds1.Y += y;
                DrawMonitorNumericalDisplay(bounds1, "BP", "133 : 97\n\n  (109)", sprBatch, BPColor, null);
            });
            var dest2 = statusMonitorBounds;
            dest2.Y += dest2.Height + 4;
            DrawGraph(BPMonitor, dest2, BPColor, sb, true);
            var SPColor = new Color(165, 241, 138);
            var SP_DisplayBounds = BP_DisplayBounds;
            SP_DisplayBounds.Y += SP_DisplayBounds.Height + 4;
            PostBloomDrawCalls.Add((x, y, sprBatch) =>
            {
                var bounds1 = SP_DisplayBounds;
                bounds1.X += x;
                bounds1.Y += y;
                DrawMonitorNumericalDisplay(bounds1, "Sp02", string.Concat(reportedSP02), sprBatch, SPColor,
                    reportedSP02 >= 91 ? OxyIcon : null);
            });
            var dest3 = dest2;
            dest3.Y += dest3.Height + 4;
            DrawGraph(SPMonitor, dest3, SPColor, sb, true);
        }

        private void DrawMonitorNumericalDisplay(Rectangle bounds, string display, string value, SpriteBatch sb,
            Color col, Texture2D icon = null)
        {
            var heightTo = 45f;
            var vector2 = TextItem.doMeasuredFontLabel(new Vector2(bounds.X + 2f, bounds.Y + 2f), display, GuiData.font,
                col, bounds.Width - 12f, heightTo);
            var num = 8;
            var destinationRectangle1 = new Rectangle((int) (bounds.X + (double) vector2.X + 9.0), bounds.Y + num, 8,
                (int) heightTo - 2*num);
            sb.Draw(Utils.white, destinationRectangle1, Color.DarkRed);
            if (icon != null)
                sb.Draw(icon,
                    new Rectangle(bounds.X + 2, (int) (bounds.Y + (double) vector2.Y + 2.0), (int) heightTo,
                        (int) heightTo), col);
            TextItem.doFontLabelToSize(
                new Rectangle(bounds.X + bounds.Width/3, bounds.Y + bounds.Height/3, (int) (bounds.Width*0.6),
                    (int) (bounds.Height*0.6)), value, GuiData.titlefont, col);
            var destinationRectangle2 = new Rectangle(bounds.X + 4, bounds.Y + bounds.Height + 2, bounds.Width - 8, 1);
            sb.Draw(Utils.white, destinationRectangle2, Color.Gray);
        }

        private void DrawMonitorStatusPanelDisplay(Rectangle bounds, string display, string value, SpriteBatch sb,
            Color col, Texture2D icon = null)
        {
            var heightTo = 45f;
            var vector2 = TextItem.doMeasuredFontLabel(new Vector2(bounds.X + 2f, bounds.Y + 2f), display, GuiData.font,
                col, bounds.Width - 12f, heightTo);
            var num = 8;
            var destinationRectangle1 = new Rectangle((int) (bounds.X + (double) vector2.X + 9.0), bounds.Y + num, 8,
                (int) heightTo - 2*num);
            sb.Draw(Utils.white, destinationRectangle1, Color.DarkRed);
            if (icon != null)
                sb.Draw(icon,
                    new Rectangle(bounds.X + 2, (int) (bounds.Y + (double) vector2.Y + 2.0), (int) heightTo,
                        (int) heightTo), col);
            TextItem.doFontLabelToSize(
                new Rectangle(bounds.X + (int) (heightTo*1.5), bounds.Y + bounds.Height/3, (int) (bounds.Width*0.3),
                    (int) (bounds.Height*0.3)), value, GuiData.titlefont, col);
            var destinationRectangle2 = new Rectangle(bounds.X + 4, bounds.Y + bounds.Height + 2, bounds.Width - 8, 1);
            sb.Draw(Utils.white, destinationRectangle2, Color.Gray);
        }

        private void DrawGraph(IMedicalMonitor monitor, Rectangle dest, Color col, SpriteBatch sb,
            bool drawUnderline = true)
        {
            monitor.Draw(dest, sb, col, projectionFowardsTime);
            if (!drawUnderline)
                return;
            PostBloomDrawCalls.Add((x, y, sprBatch) =>
            {
                var destinationRectangle = new Rectangle(dest.X + 4 + x, dest.Y + dest.Height + 2 + y, dest.Width - 8, 1);
                sprBatch.Draw(Utils.white, destinationRectangle, Color.Gray);
            });
        }

        private void EnactFirmwareChange()
        {
            PatientInCardiacArrest = selectedFirmwareData == PortExploits.DangerousPacemakerFirmware;
        }

        public void DrawWelcomeScreen(Rectangle bounds, SpriteBatch sb)
        {
            var hasAdmin = comp.adminIP == os.thisComputer.ip;
            var graphColor = Color.Red;
            graphColor.A = 100;
            if (hasAdmin)
                graphColor = new Color(20, 200, 20, 100);
            var heartRateDisplay = new Rectangle(bounds.X + 10, bounds.Y + bounds.Height/4, bounds.Width - 20,
                bounds.Height/4);
            DrawGraph(HeartMonitor, heartRateDisplay, graphColor, sb, false);
            heartRateDisplay.Y += heartRateDisplay.Height + 10;
            var adminMsg = hasAdmin ? "Admin Access Granted" : "Admin Access Required";
            adminMsg = adminMsg.ToUpper();
            heartRateDisplay.Height = 20;
            DrawLinedMessage(adminMsg, graphColor*0.2f, heartRateDisplay, sb);
            var nextToButonsLineThing = new Rectangle(heartRateDisplay.X + heartRateDisplay.Width/4 - 18,
                heartRateDisplay.Y + heartRateDisplay.Height + 10, 14, bounds.Height/3);
            sb.Draw(Utils.white, nextToButonsLineThing, graphColor*0.2f);
            PostBloomDrawCalls.Add((x, y, sprBatch) =>
            {
                var dest = heartRateDisplay;
                dest.X += x;
                dest.Y += y;
                DrawLinedMessage(adminMsg, graphColor, dest, sprBatch);
                nextToButonsLineThing.X += x;
                nextToButonsLineThing.Y += y;
                sprBatch.Draw(Utils.white, nextToButonsLineThing, graphColor);
                dest.Y += dest.Height + 10;
                dest.Width = dest.Width/2;
                dest.X += dest.Width/2;
                dest.Height = 28;
                dest.Height *= 2;
                sprBatch.DrawString(GuiData.font, comp.name, new Vector2(dest.X, dest.Y), Color.White);
                sprBatch.DrawString(GuiData.detailfont, "Kellis Biotech\nB-Type Pacemaker v2.44",
                    new Vector2(dest.X + 2, dest.Y + 30), Color.White, 0.0f, Vector2.Zero, 0.8f, SpriteEffects.None,
                    0.5f);
                dest.Y += dest.Height + 6;
                dest.Height /= 2;
                if (
                    Button.doButton(686868001, dest.X, dest.Y, dest.Width, dest.Height + 6, "View Monitor",
                        hasAdmin ? os.highlightColor : Color.Gray) && hasAdmin)
                    ChangeState(HeartMonitorState.MainDisplay);
                dest.Y += dest.Height + 10 + 6;
                if (
                    Button.doButton(686868003, dest.X, dest.Y, dest.Width, dest.Height, "Admin Login",
                        !hasAdmin ? os.highlightColor : Color.Gray) && !hasAdmin)
                    os.runCommand("login");
                dest.Y += dest.Height + 10;
                if (!Button.doButton(686868005, dest.X, dest.Y, dest.Width, dest.Height, "Exit", os.brightLockedColor))
                    return;
                os.display.command = "connect";
            });
        }

        private void DrawLinedMessage(string message, Color col, Rectangle dest, SpriteBatch sb)
        {
            var num = 16;
            var spriteFont = GuiData.smallfont;
            var vector2 = spriteFont.MeasureString(message);
            vector2.X += num;
            dest.Height = (int) (vector2.Y + 0.5);
            var destinationRectangle = new Rectangle(dest.X, dest.Y + dest.Height/2 - 1,
                dest.Width/2 - (int) (vector2.X/2.0), 2);
            sb.Draw(Utils.white, destinationRectangle, col);
            sb.DrawString(spriteFont, message,
                new Vector2(destinationRectangle.X + destinationRectangle.Width + num/2,
                    destinationRectangle.Y - dest.Height/2), col);
            destinationRectangle.X = dest.X + dest.Width - destinationRectangle.Width;
            sb.Draw(Utils.white, destinationRectangle, col);
        }

        private void DrawOptionsPanel(Rectangle bounds, SpriteBatch spritebatch)
        {
            var buttonWidth = 120;
            var num1 = (bounds.Width - buttonWidth)*0.7f;
            var point = Math.Min(1f, opTransition);
            if (!opOpening)
                point = 1f - point;
            var num2 = Utils.QuadraticOutCurve(point);
            var ShowingContent = num2 >= 0.980000019073486;
            var contentFade = 0.0f;
            var num3 = 0.5f;
            if (ShowingContent)
                contentFade = Math.Min(1f + num3, opTransition) - num3;
            var width = (int) (num2*(double) num1) + buttonWidth;
            var panelArea = new Rectangle(bounds.X + bounds.Width - width, bounds.Y + bounds.Height/10, width,
                bounds.Height - bounds.Height/5);
            var buttonSourceRect = panelArea;
            PostBloomDrawCalls.Add((x, y, sprBatch) =>
            {
                var rectangle = buttonSourceRect;
                rectangle.X += x;
                rectangle.Y += y;
                var num4 = bounds.Height/4 + 12;
                rectangle.Y += num4 - bounds.Height/10;
                var height = 30;
                var destinationRectangle = rectangle;
                destinationRectangle.Width = buttonWidth;
                destinationRectangle.Height = height;
                destinationRectangle.X -= 2;
                sprBatch.Draw(Utils.white, destinationRectangle, Utils.VeryDarkGray);
                if (Button.doButton(83838001, destinationRectangle.X, destinationRectangle.Y, buttonWidth, height,
                    "Login", State != HeartMonitorState.SecondaryLogin ? os.highlightColor : Color.Black))
                    ChangeState(HeartMonitorState.SecondaryLogin);
                destinationRectangle.Y += height + 2;
                sprBatch.Draw(Utils.white, destinationRectangle, Utils.VeryDarkGray);
                if (Button.doButton(83838003, destinationRectangle.X, destinationRectangle.Y, buttonWidth, height,
                    "Firmware", State != HeartMonitorState.FirmwareScreen ? os.highlightColor : Color.Black))
                    ChangeState(HeartMonitorState.FirmwareScreen);
                destinationRectangle.Y += height + 2;
                sprBatch.Draw(Utils.white, destinationRectangle, Utils.VeryDarkGray);
                if (Button.doButton(83838006, destinationRectangle.X, destinationRectangle.Y, buttonWidth, height,
                    "Monitor", State != HeartMonitorState.MainDisplay ? os.highlightColor : Color.Black))
                    ChangeState(HeartMonitorState.MainDisplay);
                destinationRectangle.Y += height + 2;
                sprBatch.Draw(Utils.white, destinationRectangle, Utils.VeryDarkGray);
                if (Button.doButton(83838009, destinationRectangle.X, destinationRectangle.Y, buttonWidth, height,
                    "Exit", os.lockedColor))
                    ChangeState(HeartMonitorState.Welcome);
                destinationRectangle.Y += height + 2;
                panelArea.X += x;
                panelArea.Y += y;
                panelArea.X += buttonWidth - 3;
                panelArea.Width -= buttonWidth;
                if (opOpening || opTransition < 1.0)
                    panelArea.Width += 2;
                sprBatch.Draw(Utils.white, panelArea, os.outlineColor);
                var num5 = 3;
                panelArea.X += num5;
                panelArea.Width -= 2*num5;
                panelArea.Y += num5;
                panelArea.Height -= 2*num5;
                sprBatch.Draw(Utils.white, panelArea, os.indentBackgroundColor);
                if (!ShowingContent)
                    return;
                DrawOptionsPanelContent(panelArea, sprBatch, contentFade);
            });
        }

        private Rectangle DrawOptionsPanelHeaders(Rectangle bounds, SpriteBatch sb, float contentFade)
        {
            var rectangle = bounds;
            rectangle.Height = 30;
            rectangle.X += 2;
            rectangle.Width -= 4;
            rectangle.Y += 2;
            var message = "Firmware Config";
            DrawLinedMessage(message, os.highlightColor*contentFade, rectangle, sb);
            rectangle.X -= 2;
            DrawLinedMessage(message, os.highlightColor*contentFade*0.2f, rectangle, sb);
            rectangle.X += 4;
            DrawLinedMessage(message, os.highlightColor*contentFade*0.2f, rectangle, sb);
            rectangle.X -= 2;
            rectangle.Y += rectangle.Height - 10;
            rectangle.Height = 54;
            var str = "Firmware Administration\nAccess : ";
            var color1 = os.brightLockedColor;
            string text;
            if (HasSecondaryLogin)
            {
                text = str + "GRANTED";
                color1 = os.brightUnlockedColor;
            }
            else
                text = str + " DENIED";
            var color2 = color1*contentFade;
            sb.DrawString(GuiData.font, text, new Vector2(rectangle.X, rectangle.Y), color2, 0.0f, Vector2.Zero, 0.8f,
                SpriteEffects.None, 0.6f);
            rectangle.Y += rectangle.Height;
            sb.DrawString(GuiData.detailfont, "Secondary security layer for firmware read/write access",
                new Vector2(rectangle.X, rectangle.Y), color2*0.7f, 0.0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0.6f);
            rectangle.Y += 15;
            rectangle.Height = 1;
            sb.Draw(Utils.white, rectangle,
                Color.Gray*contentFade*(float) (Utils.random.NextDouble()*0.15 + 0.280000001192093));
            return new Rectangle(bounds.X, rectangle.Y, bounds.Width,
                bounds.Y + bounds.Height - rectangle.Y - rectangle.Height);
        }

        private void DrawOptionsPanelContent(Rectangle bounds, SpriteBatch sb, float contentFade)
        {
            var bounds1 = DrawOptionsPanelHeaders(bounds, sb, contentFade);
            switch (State)
            {
                case HeartMonitorState.Error:
                    break;
                case HeartMonitorState.FirmwareScreen:
                case HeartMonitorState.FirmwareScreenConfirm:
                case HeartMonitorState.FirmwareScreenLoading:
                case HeartMonitorState.FirmwareScreenComplete:
                    DrawOptionsPanelFirmwareContent(bounds1, sb, contentFade);
                    break;
                default:
                    DrawOptionsPanelLoginContent(bounds1, sb, contentFade);
                    break;
            }
        }

        private void DrawOptionsPanelFirmwareContent(Rectangle bounds, SpriteBatch sb, float contentFade)
        {
            var color = heartColor;
            color.A = 0;
            if (!HasSecondaryLogin)
            {
                TextItem.doFontLabelToSize(
                    new Rectangle(bounds.X + bounds.Width/4, bounds.Y, bounds.Width/2, bounds.Height),
                    "Firmware Administration\nAccess Required\nLog In First", GuiData.font, color);
            }
            else
            {
                switch (State)
                {
                    case HeartMonitorState.FirmwareScreenConfirm:
                        break;
                    case HeartMonitorState.FirmwareScreenLoading:
                        var bounds1 = bounds;
                        ++bounds1.X;
                        bounds1.Width -= 2;
                        bounds1.Height = 110;
                        DrawSelectedFirmwareFileDetails(bounds1, sb, selectedFirmwareData, selectedFirmwareName);
                        bounds1.Y += bounds1.Height + 2;
                        var rectangle1 = bounds;
                        var num1 = bounds1.Height + 10;
                        rectangle1.Height -= num1;
                        rectangle1.Y += num1;
                        var destinationRectangle = rectangle1;
                        destinationRectangle.Height = 1;
                        var num2 = firmwareLoadTime/10f;
                        color.A = 0;
                        var num3 = 0;
                        while (num3 < rectangle1.Height)
                        {
                            var num4 = num3/(float) rectangle1.Height;
                            if (num2 > (double) num4)
                            {
                                destinationRectangle.Y = rectangle1.Y + num3;
                                sb.Draw(Utils.white, destinationRectangle, color);
                            }
                            num3 += 3;
                        }
                        break;
                    case HeartMonitorState.FirmwareScreenComplete:
                        var rectangle2 = bounds;
                        rectangle2.X += 2;
                        rectangle2.Width -= 4;
                        rectangle2.Y += 2;
                        rectangle2.Height -= 4;
                        sb.Draw(Utils.white, rectangle2, os.brightLockedColor*0.3f*contentFade);
                        rectangle2.X += 40;
                        rectangle2.Width -= 80;
                        TextItem.doFontLabelToSize(rectangle2, "FIRMWARE UPDATE COMPLETE", GuiData.font, Color.White);
                        break;
                    default:
                        var folder = comp.files.root.searchForFolder("KBT_Pacemaker");
                        var text = new string[folder.files.Count + 1];
                        text[0] = "Currently Active Firmware";
                        for (var index = 0; index < folder.files.Count; ++index)
                            text[index + 1] = folder.files[index].name;
                        selectedFirmwareIndex = SelectableTextList.doFancyList(8937001, bounds.X + 2, bounds.Y + 10,
                            (int) (bounds.Width - 4.0), bounds.Height/3, text, selectedFirmwareIndex, os.topBarColor,
                            false);
                        var rectangle3 = new Rectangle(bounds.X + 2, bounds.Y + 10 + bounds.Height/3 + 4,
                            bounds.Width - 4, bounds.Height/4);
                        var data = selectedFirmwareIndex != 0 ? folder.files[selectedFirmwareIndex - 1].data : null;
                        var filename = selectedFirmwareIndex != 0
                            ? folder.files[selectedFirmwareIndex - 1].name
                            : "Currently Active Firmware";
                        var flag = DrawSelectedFirmwareFileDetails(rectangle3, sb, data, filename);
                        rectangle3.Y += rectangle3.Height + 6;
                        if (!flag || selectedFirmwareIndex == 0)
                            break;
                        rectangle3.Height = 30;
                        if (!isConfirmingSelection)
                        {
                            if (
                                !Button.doButton(8937004, rectangle3.X, rectangle3.Y, rectangle3.Width,
                                    rectangle3.Height, "Activate This Firmware", os.highlightColor))
                                break;
                            isConfirmingSelection = true;
                            break;
                        }
                        DrawLinedMessage("Confirm Firmware Activation", os.brightLockedColor, rectangle3, sb);
                        rectangle3.Y += rectangle3.Height;
                        if (Button.doButton(8937008, rectangle3.X, rectangle3.Y, rectangle3.Width, rectangle3.Height,
                            "Confirm Activation", os.highlightColor))
                        {
                            selectedFirmwareName = filename;
                            selectedFirmwareData = data;
                            ChangeState(HeartMonitorState.FirmwareScreenLoading);
                            firmwareLoadTime = 0.0f;
                            isConfirmingSelection = false;
                        }
                        rectangle3.Y += rectangle3.Height + 4;
                        rectangle3.Height = 20;
                        if (
                            !Button.doButton(8937009, rectangle3.X, rectangle3.Y, rectangle3.Width, rectangle3.Height,
                                "Cancel", os.lockedColor))
                            break;
                        ChangeState(HeartMonitorState.FirmwareScreen);
                        isConfirmingSelection = false;
                        break;
                }
            }
        }

        private bool DrawSelectedFirmwareFileDetails(Rectangle bounds, SpriteBatch sb, string data, string filename)
        {
            var flag = data == null || IsValidFirmwareData(data);
            var dest = bounds;
            dest.Height = 28;
            DrawLinedMessage(flag ? "Valid Firmware File" : "Invalid Firmware File",
                flag ? heartColor : os.brightLockedColor, dest, sb);
            dest.Y += dest.Height - 4;
            var color = Color.White;
            if (!flag)
                color = Color.Gray;
            TextItem.doFontLabel(new Vector2(dest.X + 6f, dest.Y), filename, GuiData.font, color, dest.Width,
                dest.Height);
            dest.Y += dest.Height + 2;
            dest.Height = 14;
            var text =
                "Invalid binary package\nValid firmware packages must be\ndigitally signed by an authorized manufacturer";
            if (flag)
                text = "Valid binary package\nSigned by : KELLIS BIOTECH\nCompiled by : EIDOLON SOFT";
            TextItem.doFontLabel(new Vector2(dest.X + 6f, dest.Y), text, GuiData.detailfont, color*0.7f, dest.Width - 12,
                float.MaxValue);
            var destinationRectangle = bounds;
            destinationRectangle.Y += destinationRectangle.Height - 1;
            destinationRectangle.Height = 1;
            sb.Draw(Utils.white, destinationRectangle, Color.Gray);
            return flag;
        }

        private bool IsValidFirmwareData(string data)
        {
            if (!(data == PortExploits.DangerousPacemakerFirmware))
                return data == PortExploits.ValidPacemakerFirmware;
            return true;
        }

        private void DrawOptionsPanelLoginContent(Rectangle bounds, SpriteBatch sb, float contentFade)
        {
            var data =
                "A secondary login is required to review and modify running firmware. Personal login details are provided for each chip. If you have lost your login details, connect your support program to our content server (111.105.22.1)";
            var rectangle1 = new Rectangle(bounds.X + 2, bounds.Y + 10, bounds.Width - 4, 24);
            var text = Utils.SuperSmartTwimForWidth(data, rectangle1.Width, GuiData.detailfont);
            sb.DrawString(GuiData.detailfont, text, new Vector2(rectangle1.X, rectangle1.Y), Color.Gray*contentFade);
            var vector2 = GuiData.detailfont.MeasureString(text);
            rectangle1.Y += (int) (vector2.Y + 10.0);
            if (State == HeartMonitorState.SecondaryLoginRunning || HasSecondaryLogin)
            {
                var rectangle2 = rectangle1;
                rectangle2.Height = bounds.Height/4;
                var num = rectangle2.Height;
                if (loginUsername != null)
                {
                    rectangle2.Height = 60;
                    GetStringUIControl.DrawGetStringControlInactive("Username : ", loginUsername, rectangle2, sb, os, "");
                    rectangle2.Y += rectangle2.Height + 2;
                }
                if (loginPass != null)
                {
                    rectangle2.Height = 60;
                    GetStringUIControl.DrawGetStringControlInactive("Password : ", loginPass, rectangle2, sb, os, "");
                    rectangle2.Y += rectangle2.Height + 2;
                }
                rectangle2.Height = num;
                if (loginPass == null || loginUsername == null)
                {
                    var stringControl =
                        GetStringUIControl.DrawGetStringControl(loginUsername == null ? "Username : " : "Password : ",
                            rectangle2, () => loginUsername = loginPass = "", () => loginUsername = loginPass = "", sb,
                            os, os.highlightColor, os.lockedColor, "", new Color?());
                    if (stringControl != null)
                    {
                        if (loginUsername == null)
                        {
                            loginUsername = stringControl;
                            GetStringUIControl.StartGetString("Password", os);
                        }
                        else
                            loginPass = stringControl;
                    }
                    rectangle2.Y += rectangle2.Height + 10;
                }
                else
                {
                    rectangle2.Y += 20;
                    HasSecondaryLogin = loginUsername == "EAdmin" && loginPass == "tens86";
                    rectangle2.Height = 20;
                    DrawLinedMessage(HasSecondaryLogin ? "Login Complete" : "Login Failed",
                        HasSecondaryLogin ? os.brightUnlockedColor : os.brightLockedColor, rectangle2, sb);
                    rectangle2.Y += rectangle2.Height + 20;
                    if (!HasSecondaryLogin)
                    {
                        if (
                            !Button.doButton(92923008, rectangle2.X + 4, rectangle2.Y, rectangle2.Width - 80, 24,
                                "Retry Login", new Color?()))
                            return;
                        ChangeState(HeartMonitorState.SecondaryLogin);
                    }
                    else
                    {
                        if (
                            !Button.doButton(92923009, rectangle2.X + 4, rectangle2.Y, rectangle2.Width - 80, 24,
                                "Administrate Firmware", os.highlightColor))
                            return;
                        ChangeState(HeartMonitorState.FirmwareScreen);
                    }
                }
            }
            else
            {
                if (HasSecondaryLogin || !ButtonFlashForContentFade(contentFade) ||
                    !Button.doButton(92923001, rectangle1.X, rectangle1.Y, (int) (rectangle1.Width*0.699999988079071),
                        30, "Login", os.highlightColor))
                    return;
                loginUsername = loginPass = null;
                GetStringUIControl.StartGetString("Username", os);
                ChangeState(HeartMonitorState.SecondaryLoginRunning);
            }
        }

        private bool ButtonFlashForContentFade(float contentFade)
        {
            if (contentFade > 0.949999988079071)
                return true;
            return Utils.random.NextDouble() > contentFade;
        }

        private enum HeartMonitorState
        {
            Welcome,
            MainDisplay,
            SecondaryLogin,
            SecondaryLoginRunning,
            Error,
            FirmwareScreen,
            FirmwareScreenConfirm,
            FirmwareScreenLoading,
            FirmwareScreenComplete
        }
    }
}
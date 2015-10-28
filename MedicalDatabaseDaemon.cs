using System;
using System.Collections.Generic;
using System.Linq;
using Hacknet.Gui;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MedicalDatabaseDaemon : Daemon
    {
        private const string FOLDER_NAME = "Medical";
        private const float SEARCH_TIME = 1.6f;
        private FileEntry currentFile;
        private FileMedicalRecord currentRecord;
        private ScrollableSectionedPanel displayPanel;
        private float elapsedTimeThisState;
        private string emailRecipientAddress = "";
        private string errorMessage = "UNKNOWN ERROR";
        private readonly Texture2D logo;
        private Folder recordsFolder;
        private string searchName = "";
        private MedicalDatabaseState state;
        private readonly Color theme_back = new Color(20, 20, 20);
        private readonly Color theme_deep = new Color(8, 78, 90);
        private readonly Color theme_light = new Color(165, 237, 249);
        private readonly Color theme_strong = new Color(64, 157, 174);
        private readonly GridSpot[,] themeGrid = new GridSpot[20, 100];
        private float totalTimeThisState = 1f;

        public MedicalDatabaseDaemon(Computer c, OS os)
            : base(c, "Universal Medical Database", os)
        {
            logo = os.content.Load<Texture2D>("Sprites/MedicalLogo");
        }

        public override void initFiles()
        {
            base.initFiles();
            recordsFolder = new Folder("Medical");
            comp.files.root.folders.Add(recordsFolder);
            for (var index = 0; index < People.all.Count; ++index)
            {
                var fileMedicalRecord = new FileMedicalRecord(People.all[index]);
                recordsFolder.files.Add(new FileEntry(fileMedicalRecord.ToString(), fileMedicalRecord.GetFileName()));
            }
            var dataEntry = Utils.readEntireFile("Content/Post/MedicalDatabaseInfo.txt");
            var folder = comp.files.root.searchForFolder("home");
            if (folder == null)
                return;
            folder.files.Add(new FileEntry(dataEntry, "MedicalDatabaseInfo.txt"));
        }

        public override void loadInit()
        {
            base.loadInit();
            recordsFolder = comp.files.root.searchForFolder("Medical");
        }

        public override string getSaveString()
        {
            return "<MedicalDatabase />";
        }

        public void ResetThemeGrid()
        {
            for (var y = 0; y < themeGrid.GetLength(0); ++y)
            {
                for (var x = 0; x < themeGrid.GetLength(1); ++x)
                    ResetGridPoint(x, y);
            }
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            elapsedTimeThisState = 0.0f;
            state = MedicalDatabaseState.MainMenu;
        }

        public void ResetGridPoint(int x, int y)
        {
            themeGrid[y, x] = new GridSpot
            {
                from = themeGrid[y, x].to,
                to = Utils.randm(2f),
                time = 0f,
                totalTime = Utils.randm(3f) + 1.2f
            };
        }

        private void LookupEntry()
        {
            var list = new List<string>();
            list.Add(searchName.Trim().ToLower().Replace(" ", "_"));
            if (searchName.Contains(" "))
            {
                var str1 =
                    (searchName.Substring(searchName.IndexOf(" ")) + searchName.Substring(0, searchName.IndexOf(" ")))
                        .Trim().ToLower().Replace(" ", "_");
                list.Add(str1);
                var str2 =
                    (searchName.Substring(searchName.IndexOf(" ")) + "_" +
                     searchName.Substring(0, searchName.IndexOf(" "))).Trim().ToLower().Replace(" ", "_");
                list.Add(str2);
            }
            FileEntry fileEntry = null;
            for (var index1 = 0; index1 < list.Count; ++index1)
            {
                for (var index2 = 0; index2 < recordsFolder.files.Count; ++index2)
                {
                    if (recordsFolder.files[index2].name.ToLower().StartsWith(list[index1]))
                    {
                        fileEntry = recordsFolder.files[index2];
                        break;
                    }
                }
                if (fileEntry != null)
                    break;
            }
            if (fileEntry == null)
            {
                state = MedicalDatabaseState.Error;
                errorMessage = "No entry found for name " + searchName + "\nPermutations tested:\n";
                for (var index = 0; index < list.Count; ++index)
                {
                    var medicalDatabaseDaemon = this;
                    var str = medicalDatabaseDaemon.errorMessage + list[index] + "\n";
                    medicalDatabaseDaemon.errorMessage = str;
                }
                elapsedTimeThisState = 0.0f;
            }
            else
            {
                currentFile = fileEntry;
                var record = new FileMedicalRecord();
                if (FileMedicalRecord.RecordFromString(currentFile.data, out record))
                {
                    currentRecord = record;
                    state = MedicalDatabaseState.Entry;
                    elapsedTimeThisState = 0.0f;
                }
                else
                {
                    elapsedTimeThisState = 0.0f;
                    state = MedicalDatabaseState.Error;
                    errorMessage = "Corrupt record --\nUnable to parse record " + currentFile.name;
                }
            }
        }

        private void UpdateStates(float t)
        {
            elapsedTimeThisState += t;
            if (elapsedTimeThisState < (double) totalTimeThisState || state != MedicalDatabaseState.Searching)
                return;
            LookupEntry();
        }

        private void SendReportEmail(FileMedicalRecord record, string emailAddress)
        {
            try
            {
                var mail = MailServer.generateEmail("MedicalRecord - " + record.Lastname + "_" + record.Firstname,
                    record.ToEmailString(), "records@meddb.org");
                var userTo = emailAddress;
                if (emailAddress.Contains('@'))
                    userTo = emailAddress.Substring(0, emailAddress.IndexOf('@'));
                var computer = Programs.getComputer(os, "jmail");
                if (computer == null)
                    return;
                var mailServer = (MailServer) computer.getDaemon(typeof (MailServer));
                if (mailServer == null)
                    return;
                mailServer.addMail(mail, userTo);
            }
            catch (Exception ex)
            {
            }
        }

        private void updateGrid()
        {
            var num = (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
            for (var y = 0; y < themeGrid.GetLength(0); ++y)
            {
                for (var x = 0; x < themeGrid.GetLength(1); ++x)
                {
                    var gridSpot = themeGrid[y, x];
                    gridSpot.time += num;
                    if (gridSpot.time >= (double) gridSpot.totalTime)
                        ResetGridPoint(x, y);
                    else
                        themeGrid[y, x] = gridSpot;
                }
            }
        }

        private void drawGrid(Rectangle bounds, SpriteBatch sb, int width)
        {
            var num1 = 12;
            var x = bounds.X + bounds.Width - num1 - 1;
            var destinationRectangle = new Rectangle(x, bounds.Y + 1, num1, num1);
            int num2;
            var num3 = num2 = 0;
            while (destinationRectangle.Y + 1 < bounds.Y + bounds.Height)
            {
                if (destinationRectangle.Y + num1 + 1 >= bounds.Y + bounds.Height)
                    destinationRectangle.Height = bounds.Y + bounds.Height - (destinationRectangle.Y + 2);
                while (destinationRectangle.X - num1 > bounds.X + bounds.Width - width - 1)
                {
                    var gridSpot = themeGrid[num3%themeGrid.GetLength(0), num2%themeGrid.GetLength(1)];
                    var num4 = gridSpot.time/gridSpot.totalTime;
                    var amount = gridSpot.from + num4*(gridSpot.to - gridSpot.from);
                    var color1 = theme_deep;
                    var color2 = theme_strong;
                    if (amount >= 1.0)
                    {
                        --amount;
                        color1 = theme_strong;
                        color2 = theme_light;
                    }
                    var color3 = Color.Lerp(color1, color2, amount);
                    sb.Draw(Utils.white, destinationRectangle, color3);
                    ++num3;
                    destinationRectangle.X -= num1 + 1;
                }
                ++num2;
                destinationRectangle.X = x;
                destinationRectangle.Y += num1 + 1;
            }
        }

        public void DrawError(Rectangle bounds, SpriteBatch sb)
        {
            var rectangle = new Rectangle(bounds.X + 2, bounds.Y + 12, (int) (bounds.Width/1.6) - 4, bounds.Height/2);
            rectangle.Height = (int) (logo.Height/(double) logo.Width*rectangle.Height);
            sb.Draw(logo, rectangle, theme_deep*0.4f);
            var num = rectangle.Width;
            rectangle.Y += 100;
            rectangle.Height = 35;
            rectangle.Width = (int) (num*(double) Utils.QuadraticOutCurve(Math.Min(elapsedTimeThisState*2f, 1f)));
            DrawMessage("Error", true, sb, rectangle, os.lockedColor, Color.White);
            rectangle.Y += rectangle.Height + 2;
            rectangle.Height = 100;
            rectangle.Width = (int) (num*(double) Utils.QuadraticOutCurve(Math.Min(elapsedTimeThisState*1.5f, 1f)));
            DrawMessageBot(errorMessage, false, sb, rectangle, theme_back, theme_light);
            rectangle.Width = num;
            rectangle.Y += rectangle.Height + 2;
            if (!Button.doButton(444402002, rectangle.X, rectangle.Y, rectangle.Width, 24, "Back to menu", theme_strong))
                return;
            elapsedTimeThisState = 0.0f;
            state = MedicalDatabaseState.MainMenu;
        }

        public void DrawSendReport(Rectangle bounds, SpriteBatch sb)
        {
            var rectangle = new Rectangle(bounds.X + 2, bounds.Y + 12, (int) (bounds.Width/1.6) - 4, bounds.Height/2);
            rectangle.Height = (int) (logo.Height/(double) logo.Width*rectangle.Height);
            sb.Draw(logo, rectangle, theme_deep*0.4f);
            var num = rectangle.Width;
            rectangle.Y += 100;
            rectangle.Height = 35;
            rectangle.Width = (int) (num*(double) Utils.QuadraticOutCurve(Math.Min(elapsedTimeThisState*2f, 1f)));
            DrawMessage("Send Record Copy", true, sb, rectangle, theme_deep, Color.White);
            rectangle.Y += rectangle.Height + 2;
            rectangle.Height = 22;
            rectangle.Width = (int) (num*(double) Utils.QuadraticOutCurve(Math.Min(elapsedTimeThisState*1.5f, 1f)));
            DrawMessageBot("Record for " + currentRecord.Firstname + " " + currentRecord.Lastname, false, sb, rectangle,
                theme_back, theme_light);
            rectangle.Width = num;
            rectangle.Y += rectangle.Height + 12;
            rectangle.Height = 35;
            rectangle.Width = num;
            DrawMessage("Recipient Address", true, sb, rectangle, theme_deep, Color.White);
            rectangle.Y += rectangle.Height + 2;
            var upperPrompt = " ---------";
            rectangle.Height = 130;
            if (state == MedicalDatabaseState.SendReportSearch)
            {
                emailRecipientAddress = GetStringUIControl.DrawGetStringControl("Recipient Address (Case Sensitive): ",
                    rectangle, () =>
                    {
                        elapsedTimeThisState = 0.0f;
                        state = MedicalDatabaseState.Error;
                        errorMessage = "Error getting recipient email";
                    }, () => state = MedicalDatabaseState.SendReport, sb, os, theme_strong, os.lockedColor, upperPrompt,
                    new Color?());
                rectangle.Y += 26;
                if (emailRecipientAddress != null)
                {
                    state = MedicalDatabaseState.SendReportSending;
                    elapsedTimeThisState = 1f;
                }
            }
            else
                GetStringUIControl.DrawGetStringControlInactive("Recpient Address: ",
                    emailRecipientAddress == null ? "Undefined" : emailRecipientAddress, rectangle, sb, os, upperPrompt);
            rectangle.Y += rectangle.Height + 2;
            rectangle.Height = 24;
            if (state == MedicalDatabaseState.SendReport)
            {
                if (Button.doButton(444402023, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
                    "Specify Address", theme_strong))
                {
                    GetStringUIControl.StartGetString("Recipient_Address", os);
                    state = MedicalDatabaseState.SendReportSearch;
                }
            }
            else if (state == MedicalDatabaseState.SendReportSending || state == MedicalDatabaseState.SendReportComplete)
            {
                var point = (float) ((elapsedTimeThisState - 1.0)/3.0);
                if (state == MedicalDatabaseState.SendReportComplete)
                    point = 1f;
                if (point >= 1.0 && state != MedicalDatabaseState.SendReportComplete)
                {
                    state = MedicalDatabaseState.SendReportComplete;
                    SendReportEmail(currentRecord, emailRecipientAddress);
                }
                sb.Draw(Utils.white, rectangle, theme_back);
                var destinationRectangle = rectangle;
                destinationRectangle.Width =
                    (int) (destinationRectangle.Width*(double) Utils.QuadraticOutCurve(point));
                sb.Draw(Utils.white, destinationRectangle, theme_light);
                sb.DrawString(GuiData.smallfont,
                    state == MedicalDatabaseState.SendReportComplete ? "COMPLETE" : "SENDING ...",
                    new Vector2(rectangle.X, rectangle.Y + 2), Color.Black);
            }
            rectangle.Y += rectangle.Height + 2;
            if (state == MedicalDatabaseState.SendReportComplete &&
                Button.doButton(444402001, rectangle.X, rectangle.Y, rectangle.Width, 24, "Send to different address",
                    theme_light))
                state = MedicalDatabaseState.SendReport;
            rectangle.Y += rectangle.Height + 2;
            if (!Button.doButton(444402002, rectangle.X, rectangle.Y, rectangle.Width, 24, "Back to menu", theme_strong))
                return;
            elapsedTimeThisState = 0.0f;
            state = MedicalDatabaseState.MainMenu;
        }

        public void DrawAbout(Rectangle bounds, SpriteBatch sb)
        {
            string data = null;
            var folder = comp.files.root.searchForFolder("home");
            if (folder != null)
            {
                var fileEntry = folder.searchForFile("MedicalDatabaseInfo.txt");
                if (fileEntry != null)
                    data = fileEntry.data;
            }
            if (data == null)
            {
                state = MedicalDatabaseState.Error;
                errorMessage =
                    "DatabaseInfo file not found\n~/home/MedicalDatabaseInfo.txt\nCould not be found or opened";
                elapsedTimeThisState = 0.0f;
            }
            else
            {
                var rectangle = new Rectangle(bounds.X + 2, bounds.Y + 12, (int) (bounds.Width/1.6) - 4, bounds.Height/2);
                rectangle.Height = (int) (logo.Height/(double) logo.Width*rectangle.Height);
                sb.Draw(logo, rectangle, theme_deep*0.4f);
                var num = rectangle.Width;
                rectangle.Y += 100;
                rectangle.Height = 35;
                rectangle.Width = (int) (num*(double) Utils.QuadraticOutCurve(Math.Min(elapsedTimeThisState*2f, 1f)));
                DrawMessage("Info", true, sb, rectangle, theme_deep, Color.White);
                var msg = Utils.SuperSmartTwimForWidth(data, rectangle.Width - 12, GuiData.tinyfont);
                rectangle.Y += rectangle.Height + 2;
                rectangle.Height = Math.Min(bounds.Height - 200, 420);
                rectangle.Width = (int) (num*(double) Utils.QuadraticOutCurve(Math.Min(elapsedTimeThisState*1.5f, 1f)));
                DrawMessageBot(msg, false, sb, rectangle, theme_back, theme_light);
                rectangle.Width = num;
                rectangle.Y += rectangle.Height + 2;
                if (
                    !Button.doButton(444402002, rectangle.X, rectangle.Y, rectangle.Width, 24, "Back to menu",
                        theme_strong))
                    return;
                elapsedTimeThisState = 0.0f;
                state = MedicalDatabaseState.MainMenu;
            }
        }

        public void DrawEntry(Rectangle bounds, SpriteBatch sb)
        {
            var num1 = 34;
            if (displayPanel == null)
                displayPanel = new ScrollableSectionedPanel(26, sb.GraphicsDevice);
            var drawCalls = new List<Action<int, Rectangle, SpriteBatch>>();
            var rectangle1 = new Rectangle(bounds.X + 2, bounds.Y + 12, (int) (bounds.Width/1.6) - 4, bounds.Height/2);
            var allTextBounds = rectangle1;
            allTextBounds.Width += 2;
            allTextBounds.Y += num1;
            allTextBounds.Height = bounds.Height - num1 - 2 - 40 - 28;
            rectangle1.Height = logo.Height/logo.Width*rectangle1.Width;
            sb.Draw(logo, rectangle1, theme_deep*0.4f);
            rectangle1.Height = num1;
            DrawMessage(currentRecord.Lastname + ", " + currentRecord.Firstname, true, sb, rectangle1, theme_light,
                theme_back);
            rectangle1.Y += num1;
            var num2 = 22;
            rectangle1.Height = num2;
            var lines = currentRecord.record.Split(Utils.newlineDelim);
            var separator = new string[5]
            {
                " :: ",
                ":: ",
                " ::",
                "::",
                "\n"
            };
            var flag = false;
            for (var index1 = 0; index1 < lines.Length; ++index1)
            {
                var sections =
                    Utils.SuperSmartTwimForWidth(lines[index1], rectangle1.Width - 12, GuiData.tinyfont)
                        .Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length > 1)
                {
                    for (var index2 = 0; index2 < sections.Length; ++index2)
                    {
                        if (index2 == 0 && !flag)
                        {
                            if (sections[index2] == "Notes")
                                flag = true;
                            var secID = index2;
                            drawCalls.Add((index, drawPos, sprBatch) =>
                            {
                                var dest = drawPos;
                                ++dest.Y;
                                dest.Height -= 2;
                                DrawMessage(sections[secID] + " :", false, sprBatch, dest, theme_deep, theme_light);
                            });
                            rectangle1.Y += num2 + 2;
                        }
                        else if (sections[index2].Trim().Length > 0)
                        {
                            var subSecID = index2;
                            drawCalls.Add((index, drawPos, sprBatch) =>
                            {
                                var dest = drawPos;
                                ++dest.Y;
                                dest.Height -= 2;
                                DrawMessage(sections[subSecID], false, sprBatch, dest);
                            });
                            rectangle1.Y += num2 + 2;
                        }
                    }
                }
                else if (lines[index1].Trim().Length > 0)
                {
                    var idx = index1;
                    drawCalls.Add((index, drawPos, sprBatch) =>
                    {
                        var rectangle2 = drawPos;
                        ++rectangle2.Y;
                        rectangle2.Height -= 2;
                        DrawMessage(lines[idx], false, sprBatch, drawPos);
                    });
                    rectangle1.Y += num2 + 2;
                }
            }
            drawCalls.Add((index, drawPos, sprBatch) =>
            {
                var dest = drawPos;
                dest.Y += 2;
                dest.Height -= 4;
                DrawMessage(" ", false, sprBatch, dest);
            });
            rectangle1.Y += num2 + 2;
            displayPanel.NumberOfPanels = drawCalls.Count;
            displayPanel.Draw((idx, rect, sprBatch) =>
            {
                if ((drawCalls.Count + 1)*displayPanel.PanelHeight >= allTextBounds.Height)
                    rect.Width -= 10;
                drawCalls[idx](idx, rect, sprBatch);
            }, sb, allTextBounds);
            rectangle1.Y += 2;
            if (Button.doButton(444402033, rectangle1.X, bounds.Y + bounds.Height - 26, rectangle1.Width, 24,
                "Back to menu", theme_strong))
            {
                elapsedTimeThisState = 0.0f;
                state = MedicalDatabaseState.MainMenu;
            }
            if (Button.doButton(444402035, rectangle1.X, bounds.Y + bounds.Height - 26 - 2 - 26, rectangle1.Width, 24,
                "e-mail this record", theme_light))
            {
                elapsedTimeThisState = 0.0f;
                state = MedicalDatabaseState.SendReport;
            }
            var dest1 = new Rectangle(rectangle1.X + rectangle1.Width + 2, bounds.Y + 34 + 12,
                (int) (bounds.Width/6.5) - 2, bounds.Height - 4);
            var num3 = 33;
            var num4 = 22;
            dest1.Height = num3;
            DrawMessage("Age", true, sb, dest1);
            dest1.Y += dest1.Height + 2;
            var timeSpan = DateTime.Now - currentRecord.DOB;
            var num5 = (int) (timeSpan.Days/365.0);
            DrawMessage(string.Concat(timeSpan.Days/365), true, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height;
            dest1.Height = num4;
            DrawMessage("Years", false, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height + 2;
            dest1.Height = num3;
            DrawMessage(string.Concat(timeSpan.Days - num5*365), true, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height;
            dest1.Height = num4;
            DrawMessage("Days", false, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height + 2;
            dest1.Height = num3;
            DrawMessage(string.Concat(timeSpan.Hours), true, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height;
            dest1.Height = num4;
            DrawMessage("Hours", false, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height + 2;
            dest1.Height = num3;
            DrawMessage(string.Concat(timeSpan.Minutes), true, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height;
            dest1.Height = num4;
            DrawMessage("Minutes", false, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height + 2;
            dest1.Height = num3;
            DrawMessage(string.Concat(timeSpan.Seconds), true, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height;
            dest1.Height = num4;
            DrawMessage("Seconds", false, sb, dest1, Color.Transparent, theme_light);
            dest1.Y += dest1.Height + 2;
            dest1.Height = num3;
        }

        public void DrawMenu(Rectangle bounds, SpriteBatch sb)
        {
            var height = 34;
            var rectangle = new Rectangle(bounds.X + 2, bounds.Y + 12, bounds.Width/2 - 4, height);
            DrawMessage("Universal Medical", true, sb, rectangle, theme_light, Color.Black);
            rectangle.Y += height + 2;
            rectangle.Height = 20;
            DrawMessage("Records & Monitoring Services", false, sb, rectangle);
            var destinationRectangle1 = new Rectangle(bounds.X + bounds.Width/2 + 10, bounds.Y + 12, bounds.Width/4 - 12,
                (int) (logo.Height/(double) logo.Width*(bounds.Width/4.0)));
            sb.Draw(logo, destinationRectangle1, theme_light);
            var destinationRectangle2 = new Rectangle(rectangle.X + 10, rectangle.Y + 40, rectangle.Width - 20, 1);
            sb.Draw(Utils.white, destinationRectangle2, Utils.SlightlyDarkGray*0.5f);
            destinationRectangle2.Y += 4;
            sb.Draw(Utils.white, destinationRectangle2, Utils.SlightlyDarkGray*0.5f);
            rectangle.Y += 90;
            if (!(comp.adminIP == os.thisComputer.ip))
            {
                rectangle.Height = bounds.Y + bounds.Height - rectangle.Y;
                DrawNoAdminMenuSection(rectangle, sb);
            }
            else
            {
                rectangle.Height = 80;
                DrawMessageBot("Information", true, sb, rectangle, theme_light, Color.Black);
                rectangle.Y += rectangle.Height + 2;
                rectangle.Height = 20;
                DrawMessage("Details and Administration", false, sb, rectangle);
                rectangle.Y += rectangle.Height + 2;
                if (Button.doButton(444402000, rectangle.X + 1, rectangle.Y, rectangle.Width, 24, "Info", theme_strong))
                {
                    state = MedicalDatabaseState.AboutScreen;
                    elapsedTimeThisState = 0.0f;
                }
                rectangle.Y += 60;
                rectangle.Height = 80;
                DrawMessageBot("Database", true, sb, rectangle, theme_light, Color.Black);
                rectangle.Y += rectangle.Height + 2;
                rectangle.Height = 20;
                DrawMessage("Records Lookup", false, sb, rectangle);
                rectangle.Y += rectangle.Height + 2;
                if (state == MedicalDatabaseState.MainMenu)
                {
                    if (Button.doButton(444402005, rectangle.X + 1, rectangle.Y, rectangle.Width, 24, "Search",
                        theme_strong))
                    {
                        state = MedicalDatabaseState.Search;
                        elapsedTimeThisState = 0.0f;
                        GetStringUIControl.StartGetString("Patient_Name", os);
                    }
                    rectangle.Y += 26;
                    if (Button.doButton(444402007, rectangle.X + 1, rectangle.Y, rectangle.Width, 24, "Random Entry",
                        theme_strong))
                    {
                        searchName = recordsFolder.files[Utils.random.Next(recordsFolder.files.Count)].name;
                        elapsedTimeThisState = 0.0f;
                        state = MedicalDatabaseState.Searching;
                        totalTimeThisState = 1.6f;
                    }
                }
                else if (state == MedicalDatabaseState.Search)
                {
                    var num = Utils.QuadraticOutCurve(Math.Min(1f, elapsedTimeThisState*2f));
                    var bounds1 = new Rectangle(rectangle.X, rectangle.Y - 10, rectangle.Width, (int) (num*72.0));
                    var destinationRectangle3 = new Rectangle(bounds1.X, rectangle.Y + 2, rectangle.Width,
                        (int) (num*32.0));
                    sb.Draw(Utils.white, destinationRectangle3, os.darkBackgroundColor);
                    var stringControl = GetStringUIControl.DrawGetStringControl("Enter patient name :", bounds1, () =>
                    {
                        elapsedTimeThisState = 0.0f;
                        state = MedicalDatabaseState.Error;
                        errorMessage = "Error in name input";
                    }, () =>
                    {
                        elapsedTimeThisState = 0.0f;
                        state = MedicalDatabaseState.MainMenu;
                        os.terminal.executeLine();
                    }, sb, os, theme_strong, theme_back, "", Color.Transparent);
                    if (stringControl != null)
                    {
                        searchName = stringControl;
                        elapsedTimeThisState = 0.0f;
                        state = MedicalDatabaseState.Searching;
                        totalTimeThisState = 1.6f;
                    }
                }
                else if (state == MedicalDatabaseState.Searching)
                {
                    var destinationRectangle3 = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 24);
                    sb.Draw(Utils.white, destinationRectangle3, theme_deep);
                    destinationRectangle3.Width =
                        (int)
                            (destinationRectangle3.Width*
                             (double) Utils.QuadraticOutCurve(elapsedTimeThisState/totalTimeThisState));
                    sb.Draw(Utils.white, destinationRectangle3, theme_light);
                    destinationRectangle3.Y += destinationRectangle3.Height/2 - 2;
                    destinationRectangle3.Height = 4;
                    sb.Draw(Utils.white, destinationRectangle3, theme_deep);
                }
                if (
                    !Button.doButton(444402800, rectangle.X + 1, bounds.Y + bounds.Height - 28, rectangle.Width, 24,
                        "Exit Database View", os.lockedColor))
                    return;
                os.display.command = "connect";
            }
        }

        private void DrawNoAdminMenuSection(Rectangle bounds, SpriteBatch sb)
        {
            bounds.Height -= 2;
            ++bounds.X;
            bounds.Width -= 2;
            bounds.Height -= 30;
            PatternDrawer.draw(bounds, 0.2f, Color.Transparent, os.brightLockedColor, sb, PatternDrawer.errorTile);
            bounds.Height += 30;
            var dest = bounds;
            dest.Height = 36;
            dest.Y = bounds.Y + bounds.Height/2 - dest.Height/2;
            DrawMessage("Admin Access", true, sb, dest, os.brightLockedColor*0.8f, Color.Black);
            dest.Y += dest.Height + 2;
            dest.Height = 22;
            DrawMessage("Required for use", false, sb, dest, theme_back, os.brightLockedColor);
            if (
                !Button.doButton(444402800, bounds.X + 1, bounds.Y + bounds.Height - 28, bounds.Width, 24,
                    "Exit Database View", os.lockedColor))
                return;
            os.display.command = "connect";
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            UpdateStates((float) os.lastGameTime.ElapsedGameTime.TotalSeconds);
            updateGrid();
            drawGrid(bounds, sb, bounds.Width/4);
            switch (state)
            {
                case MedicalDatabaseState.MainMenu:
                case MedicalDatabaseState.Search:
                case MedicalDatabaseState.Searching:
                    DrawMenu(bounds, sb);
                    break;
                case MedicalDatabaseState.Entry:
                    DrawEntry(bounds, sb);
                    break;
                case MedicalDatabaseState.Error:
                    DrawError(bounds, sb);
                    break;
                case MedicalDatabaseState.AboutScreen:
                    DrawAbout(bounds, sb);
                    break;
                case MedicalDatabaseState.SendReport:
                case MedicalDatabaseState.SendReportSearch:
                case MedicalDatabaseState.SendReportSending:
                case MedicalDatabaseState.SendReportComplete:
                    DrawSendReport(bounds, sb);
                    break;
            }
        }

        private void DrawMessage(string msg, bool big, SpriteBatch sb, Rectangle dest)
        {
            DrawMessage(msg, big, sb, dest, theme_back, theme_light);
        }

        private void DrawMessage(string msg, bool big, SpriteBatch sb, Rectangle dest, Color back, Color front)
        {
            sb.Draw(Utils.white, dest, back);
            var spriteFont = big ? GuiData.font : GuiData.tinyfont;
            var vector2 = spriteFont.MeasureString(msg);
            var position = new Vector2(dest.X + dest.Width - 4 - vector2.X,
                (float) (dest.Y + dest.Height/2.0 - vector2.Y/2.0));
            sb.DrawString(spriteFont, msg, position, front);
        }

        private void DrawMessageBot(string msg, bool big, SpriteBatch sb, Rectangle dest, Color back, Color front)
        {
            sb.Draw(Utils.white, dest, back);
            var spriteFont = big ? GuiData.font : GuiData.tinyfont;
            var vector2 = spriteFont.MeasureString(msg);
            var position = new Vector2(dest.X + dest.Width - 4 - vector2.X,
                (float) (dest.Y + dest.Height - (double) vector2.Y - 4.0));
            sb.DrawString(spriteFont, msg, position, front);
        }

        private class FileMedicalRecord
        {
            private const string DELIMITER = "\n-----------------\n";

            private static readonly string[] SPLIT_DELIM = new string[1]
            {
                "\n-----------------\n"
            };

            public DateTime DOB;
            public string Firstname;
            public bool IsMale = true;
            public string Lastname;
            public string record;

            public FileMedicalRecord()
            {
            }

            public FileMedicalRecord(Person p)
            {
                Firstname = p.firstName;
                Lastname = p.lastName;
                IsMale = p.isMale;
                record = MedicalRecordToReport(p.medicalRecord);
                DOB = p.medicalRecord.DateofBirth;
            }

            public string MedicalRecordToReport(MedicalRecord rec)
            {
                return rec.ToString();
            }

            public override string ToString()
            {
                return Firstname + "\n-----------------\n" + Lastname + "\n-----------------\n" +
                       (IsMale ? "male" : "female") + "\n-----------------\n" + DOB.ToShortDateString() +
                       "\n-----------------\n" + record;
            }

            public string ToEmailString()
            {
                return ToString()
                    .Replace("tions ::", "tions::\n")
                    .Replace("Visits ::", "Visits::\n")
                    .Replace("Notes ::", "\nNotes ::\n");
            }

            public string GetFileName()
            {
                return Lastname.ToLower() + "_" + Firstname.ToLower() + ".rec";
            }

            public static bool RecordFromString(string rec, out FileMedicalRecord record)
            {
                var strArray = rec.Split(SPLIT_DELIM, StringSplitOptions.RemoveEmptyEntries);
                if (strArray.Length >= 5)
                {
                    record = new FileMedicalRecord();
                    record.Firstname = strArray[0];
                    record.Lastname = strArray[1];
                    record.IsMale = strArray[2] == "male";
                    record.DOB = DateTime.Parse(strArray[3]);
                    record.record = strArray[4];
                    return true;
                }
                record = null;
                return false;
            }
        }

        private enum MedicalDatabaseState
        {
            MainMenu,
            Search,
            Searching,
            Entry,
            Error,
            AboutScreen,
            SendReport,
            SendReportSearch,
            SendReportSending,
            SendReportComplete
        }

        private struct GridSpot
        {
            public float from;
            public float to;
            public float time;
            public float totalTime;
        }
    }
}
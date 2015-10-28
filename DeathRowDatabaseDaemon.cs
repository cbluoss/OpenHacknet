// Decompiled with JetBrains decompiler
// Type: Hacknet.DeathRowDatabaseDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class DeathRowDatabaseDaemon : Daemon
    {
        public const string ROOT_FOLDERNAME = "dr_database";
        public const string RECORDS_FOLDERNAME = "records";
        public const string SERVER_INFO_FILENAME = "ServerDetails.txt";
        private static Texture2D Logo;
        private static Texture2D Circle;
        private Folder records;
        private Vector2 recordScrollPosition = Vector2.Zero;
        private Folder root;
        private int SelectedIndex = -1;
        private readonly Color themeColor = new Color(207, 44, 19);

        public DeathRowDatabaseDaemon(Computer c, string serviceName, OS os)
            : base(c, serviceName, os)
        {
            if (Logo == null)
                Logo = os.content.Load<Texture2D>("Sprites/DeathRowLogo");
            if (Circle != null)
                return;
            Circle = os.content.Load<Texture2D>("Sprites/ThinCircleOutline");
        }

        public override void initFiles()
        {
            base.initFiles();
            root = new Folder("dr_database");
            records = new Folder("records");
            root.folders.Add(records);
            comp.files.root.folders.Add(root);
            root.files.Add(new FileEntry(
                Utils.readEntireFile("Content/Post/DeathRowServerInfo.txt").Replace('\t', ' '), "ServerDetails.txt"));
            LoadRecords(null);
        }

        private void LoadRecords(string data = null)
        {
            var str1 = Utils.readEntireFile("Content/Post/DeathRow.txt");
            if (data != null)
                str1 = data;
            var str2 = Utils.readEntireFile("Content/Post/DeathRowSpecials.txt");
            var strArray = (str1 + str2).Split(new string[1]
            {
                "\r\n###%%##%%##%%##\r\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < strArray.Length; ++index)
            {
                var deathRowEntry = ConvertStringToRecord(strArray[index]);
                if (deathRowEntry.FName != null)
                {
                    var nameEntry = deathRowEntry.LName + "_" + deathRowEntry.FName + "[" + deathRowEntry.RecordNumber +
                                    "]";
                    records.files.Add(new FileEntry(strArray[index].Replace("#", "#\n"), nameEntry));
                }
            }
            records.files.Sort((f1, f2) => string.Compare(f1.name, f2.name));
        }

        private DeathRowEntry ConvertStringToRecord(string data)
        {
            var strArray = data.Split(new string[2]
            {
                "#\n",
                "#"
            }, StringSplitOptions.None);
            var deathRowEntry = new DeathRowEntry();
            if (strArray.Length >= 9)
            {
                deathRowEntry.FName = strArray[0];
                deathRowEntry.LName = strArray[1];
                deathRowEntry.RecordNumber = strArray[2];
                deathRowEntry.Age = strArray[3];
                deathRowEntry.Date = strArray[4];
                deathRowEntry.Country = strArray[5];
                deathRowEntry.PriorRecord = strArray[6];
                deathRowEntry.IncidentReport = strArray[7];
                deathRowEntry.Statement = strArray[8];
            }
            return deathRowEntry;
        }

        public override void loadInit()
        {
            base.loadInit();
            root = comp.files.root.searchForFolder("dr_database");
            records = root.searchForFolder("records");
        }

        public override string getSaveString()
        {
            return "<DeathRowDatabase />";
        }

        private string ConvertFilesToOutput()
        {
            var str1 = "\r\n###%%##%%##%%##\r\n";
            var str2 = "#";
            var str3 = "";
            records.files.Sort((f1, f2) => string.Compare(f1.name, f2.name));
            for (var index = 0; index < records.files.Count; ++index)
            {
                var deathRowEntry = ConvertStringToRecord(records.files[index].data);
                if (deathRowEntry.RecordNumber != null)
                {
                    var str4 = deathRowEntry.FName + str2 + deathRowEntry.LName + str2 + deathRowEntry.RecordNumber +
                               str2 + deathRowEntry.Age + str2 + deathRowEntry.Date + str2 + deathRowEntry.Country +
                               str2 + deathRowEntry.PriorRecord + str2 + deathRowEntry.IncidentReport + str2 +
                               deathRowEntry.Statement + str1;
                    str3 += str4;
                }
            }
            return str3;
        }

        private void TestStringConversion()
        {
            var data = ConvertFilesToOutput();
            records.files.Clear();
            LoadRecords(data);
        }

        public bool ContainsRecordForName(string fName, string lName)
        {
            var str = lName + "_" + fName;
            for (var index = 0; index < records.files.Count; ++index)
            {
                if (records.files[index].name.StartsWith(str))
                    return true;
            }
            return false;
        }

        public DeathRowEntry GetRecordForName(string fName, string lName)
        {
            var str = lName + "_" + fName;
            for (var index = 0; index < records.files.Count; ++index)
            {
                if (records.files[index].name.StartsWith(str))
                    return ConvertStringToRecord(records.files[index].data);
                try
                {
                    var deathRowEntry = ConvertStringToRecord(records.files[index].data);
                    if (deathRowEntry.FName.ToLower() == fName.ToLower())
                    {
                        if (deathRowEntry.LName.ToLower() == lName.ToLower())
                            return deathRowEntry;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return new DeathRowEntry();
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            var text = new string[records.files.Count];
            for (var index = 0; index < records.files.Count; ++index)
            {
                try
                {
                    text[index] =
                        records.files[index].name.Substring(0, records.files[index].name.IndexOf('['))
                            .Replace("_", ", ");
                }
                catch (Exception ex)
                {
                    text[index] = "UNKNOWN" + index;
                }
            }
            var width = bounds.Width/3;
            var num = SelectedIndex;
            SelectedIndex = SelectableTextList.doFancyList(832190831, bounds.X + 1, bounds.Y + 4, width,
                bounds.Height - 8, text, SelectedIndex, themeColor, true);
            if (SelectedIndex != num)
                recordScrollPosition = Vector2.Zero;
            sb.Draw(Utils.white, new Rectangle(bounds.X + width - 1, bounds.Y + 1, 2, bounds.Height - 2), themeColor);
            var entry = new DeathRowEntry();
            var bounds1 = bounds;
            bounds1.X += width;
            bounds1.Width -= width + 1;
            if (SelectedIndex >= 0 && SelectedIndex < records.files.Count)
                entry = ConvertStringToRecord(records.files[SelectedIndex].data);
            if (entry.RecordNumber != null)
                DrawRecord(bounds1, sb, entry);
            else
                DrawTitleScreen(bounds1, sb);
        }

        private void DrawTitleScreen(Rectangle bounds, SpriteBatch sb)
        {
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            var destinationRectangle = new Rectangle(bounds.X + 12, bounds.Y + 12, Logo.Width, Logo.Height);
            sb.Draw(Logo, destinationRectangle, Color.White);
            var num1 = 1f;
            var num2 = 1.4f;
            var point = os.timer%3f;
            if (point > 1.0)
                point = 0.0f;
            var num3 = (1f - point)*0.4f;
            if (point == 0.0)
                num3 = 0.0f;
            var num4 = Utils.QuadraticOutCurve(point)*(num2 - num1);
            if (num4 > 0.0)
                num4 += num1;
            destinationRectangle =
                new Rectangle((int) (destinationRectangle.X - destinationRectangle.Width/2*(double) num4),
                    (int) (destinationRectangle.Y - destinationRectangle.Height/2*(double) num4),
                    (int) (destinationRectangle.Width*(double) num4), (int) (destinationRectangle.Height*(double) num4));
            destinationRectangle.X += Logo.Width/2;
            destinationRectangle.Y += Logo.Height/2;
            sb.Draw(Circle, destinationRectangle, Color.White*num3);
            var pos = new Vector2(bounds.X + Logo.Width + 22, bounds.Y + 14);
            TextItem.doFontLabel(pos, "DEATH ROW", GuiData.titlefont, themeColor, bounds.Width - Logo.Width - 26f, 50f);
            pos.Y += 45f;
            TextItem.doFontLabel(pos, "EXECUTED OFFENDERS LISTING", GuiData.titlefont, Color.White,
                bounds.Width - Logo.Width - 26f, 40f);
            pos.Y += Logo.Height - 40;
            pos.X = bounds.X + 12;
            var fileEntry = root.searchForFile("ServerDetails.txt");
            if (fileEntry != null)
            {
                var text = Utils.SmartTwimForWidth(fileEntry.data, bounds.Width - 30, GuiData.tinyfont);
                TextItem.doFontLabel(pos, text, GuiData.tinyfont, Color.White, bounds.Width - 20, bounds.Height - 200f);
            }
            if (Button.doButton(166261601, bounds.X + 6, bounds.Y + bounds.Height - 26, bounds.Width - 12, 22, "Exit",
                Color.Black))
                os.display.command = "connect";
            TextItem.DrawShadow = flag;
        }

        private void DrawRecord(Rectangle bounds, SpriteBatch sb, DeathRowEntry entry)
        {
            bounds.X += 2;
            bounds.Width -= 2;
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            var height = 850;
            ScrollablePanel.beginPanel(98302836, new Rectangle(bounds.X, bounds.Y, bounds.Width, height),
                recordScrollPosition);
            var num1 = bounds.Width - 16;
            var vector2_1 = new Vector2(5f, 5f);
            GuiData.spriteBatch.Draw(Logo, new Rectangle((int) vector2_1.X, (int) vector2_1.Y, 60, 60), Color.White);
            vector2_1.X += 70f;
            TextItem.doFontLabel(vector2_1, "DEATH ROW : EXECUTED OFFENDERS LISTING", GuiData.titlefont, themeColor,
                num1 - 80, 45f);
            vector2_1.Y += 22f;
            if (Button.doButton(98102855, (int) vector2_1.X, (int) vector2_1.Y, bounds.Width/2, 25, "Return",
                Color.Black))
                SelectedIndex = -1;
            vector2_1.X = 5f;
            vector2_1.Y += 55f;
            TextItem.doFontLabel(vector2_1, "RECORD " + entry.RecordNumber, GuiData.titlefont, Color.White, num1 - 4,
                60f);
            vector2_1.Y += 70f;
            var seperatorHeight = 18;
            var margin = 12;
            var drawPos = DrawCompactLabel("Name:", entry.LName + ", " + entry.FName, vector2_1, margin, seperatorHeight,
                num1);
            var pos = DrawCompactLabel("Age:", entry.Age, drawPos, margin, seperatorHeight, num1);
            var num2 = 20;
            var num3 = 20;
            var zero = Vector2.Zero;
            TextItem.doFontLabel(pos, "Incident Report:", GuiData.smallfont, themeColor, num1, float.MaxValue);
            pos.Y += num2;
            TextItem.DrawShadow = false;
            var vector2_2 = TextItem.doMeasuredSmallLabel(pos,
                Utils.SmartTwimForWidth(entry.IncidentReport, num1, GuiData.smallfont), Color.White);
            pos.Y += Math.Max(vector2_2.Y, num2);
            pos.Y += num3;
            TextItem.doFontLabel(pos, "Final Statement:", GuiData.smallfont, themeColor, num1, float.MaxValue);
            pos.Y += num2;
            vector2_2 = TextItem.doMeasuredSmallLabel(pos,
                Utils.SmartTwimForWidth(entry.Statement, num1, GuiData.smallfont), Color.White);
            pos.Y += num3;
            recordScrollPosition = ScrollablePanel.endPanel(98302836, recordScrollPosition, bounds,
                height - bounds.Height, true);
            TextItem.DrawShadow = flag;
        }

        private Vector2 DrawCompactLabel(string label, string value, Vector2 drawPos, int margin, int seperatorHeight,
            int textWidth)
        {
            TextItem.doFontLabel(drawPos, label, GuiData.smallfont, themeColor, textWidth, float.MaxValue);
            drawPos.Y += seperatorHeight;
            TextItem.doFontLabel(drawPos, value, GuiData.smallfont, Color.White, textWidth, float.MaxValue);
            drawPos.Y += seperatorHeight;
            drawPos.Y += margin;
            return drawPos;
        }

        public struct DeathRowEntry
        {
            public string FName;
            public string LName;
            public string RecordNumber;
            public string Age;
            public string Date;
            public string Country;
            public string PriorRecord;
            public string IncidentReport;
            public string Statement;
        }
    }
}
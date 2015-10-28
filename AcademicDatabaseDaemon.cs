// Decompiled with JetBrains decompiler
// Type: Hacknet.AcademicDatabaseDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class AcademicDatabaseDaemon : Daemon
    {
        public const string ROOT_FOLDERNAME = "academic_data";
        public const string ENTRIES_FOLDERNAME = "entry_cache";
        public const string CONFIG_FILENAME = "config.sys";
        public const string INFO_FILENAME = "info.txt";
        private const float SEARCH_TIME = 3.6f;
        private const float MULTI_MATCH_SEARCH_TIME = 0.7f;
        public const string DEGREE_SPLIT_DELIM = "--------------------";
        private List<Vector2> backBars;
        private readonly Color backThemeColor;
        private readonly Color darkThemeColor;
        private ADDEditField editedField;
        private int editedIndex;
        private Folder entries;
        private string foundFileName;
        private string infoText;
        private Texture2D loadingCircle;
        private bool needsDeletionConfirmation;
        private Folder root;
        private List<Degree> searchedDegrees;
        private string searchedName;
        private List<string> searchResultsNames;
        private float searchStartTime;
        private ADDState state;
        private readonly Color themeColor;
        private List<Vector2> topBars;

        public AcademicDatabaseDaemon(Computer c, string serviceName, OS os)
            : base(c, serviceName, os)
        {
            themeColor = new Color(53, 96, 156);
            backThemeColor = new Color(27, 58, 102);
            darkThemeColor = new Color(12, 20, 40, 100);
            init();
        }

        private void init()
        {
            backBars = new List<Vector2>();
            topBars = new List<Vector2>();
            searchedDegrees = new List<Degree>();
            searchResultsNames = new List<string>();
            for (var index = 0; index < 5; ++index)
            {
                backBars.Add(new Vector2(0.5f, Utils.randm(1f)));
                topBars.Add(new Vector2(0.5f, Utils.randm(1f)));
            }
            loadingCircle = os.content.Load<Texture2D>("Sprites/Spinner");
        }

        public override void initFiles()
        {
            root = comp.files.root.searchForFolder("academic_data");
            if (root == null)
            {
                root = new Folder("academic_data");
                comp.files.root.folders.Add(root);
            }
            entries = new Folder("entry_cache");
            root.folders.Add(entries);
            var dataEntry = "Welcome to the International Academic Database\n" +
                            "This public access server provides centralized access to academic records" +
                            " from every internationally recognized University in the world.\n" +
                            "Our database is managed and maintained by the administrators of this secure terminal, " +
                            "with additions from select leading universities that contribute to the database.\n" +
                            "If you are the Vice Chancellor of a university that wishes to contribute to this database " +
                            "and be internationally recognized, email our administrators at admin@mail.academic.org";
            root.files.Add(new FileEntry(dataEntry, "info.txt"));
            infoText = dataEntry;
        }

        public override void loadInit()
        {
            root = comp.files.root.searchForFolder("academic_data");
            entries = root.searchForFolder("entry_cache");
            var fileEntry = root.searchForFile("info.txt");
            infoText = fileEntry != null ? fileEntry.data : "DESCRIPTION FILE NOT FOUND";
        }

        public void initFilesFromPeople(List<Person> people)
        {
            for (var index = 0; index < people.Count; ++index)
            {
                if (people[index].degrees.Count > 0)
                    addFileForPerson(people[index]);
            }
        }

        private void addFileForPerson(Person p)
        {
            entries.files.Add(getFileForPerson(p));
        }

        private FileEntry getFileForPerson(Person p)
        {
            var fileEntry = new FileEntry();
            fileEntry.name = convertNameToFileNameStart(p.FullName);
            var num = 0;
            while (entries.searchForFile(fileEntry.name) != null)
            {
                if (num == 0)
                    fileEntry.name = fileEntry.name.Substring(0, fileEntry.name.Length - 1) + num;
                else
                    fileEntry.name += "1";
                ++num;
            }
            var str = p.FullName + "\n--------------------\n";
            for (var index = 0; index < p.degrees.Count; ++index)
                str = str + (object) p.degrees[index].name + "\n" + p.degrees[index].uni + "\n" + p.degrees[index].GPA +
                      "\n--------------------";
            fileEntry.data = str;
            return fileEntry;
        }

        public string convertNameToFileNameStart(string name)
        {
            return name.ToLower().Replace(" ", "_");
        }

        public FileEntry findFileForName(string name)
        {
            FileEntry fileEntry = null;
            searchResultsNames.Clear();
            var str1 = convertNameToFileNameStart(name);
            for (var index = 0; index < entries.files.Count; ++index)
            {
                if (entries.files[index].name.StartsWith(str1))
                {
                    var str2 = entries.files[index].data;
                    if (fileEntry == null)
                        fileEntry = entries.files[index];
                    searchResultsNames.Add(str2.Substring(0, str2.IndexOf('\n')));
                }
            }
            if (searchResultsNames.Count > 1)
                return null;
            if (fileEntry == null)
                return null;
            foundFileName = fileEntry.name;
            return fileEntry;
        }

        private void setDegreesFromFileEntryData(string file)
        {
            var separator = new string[1]
            {
                "--------------------"
            };
            var strArray1 = file.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            searchedName = strArray1[0];
            searchedDegrees.Clear();
            for (var index = 1; index < strArray1.Length; ++index)
            {
                var strArray2 = strArray1[index].Split(Utils.newlineDelim, StringSplitOptions.RemoveEmptyEntries);
                if (strArray2.Length >= 3)
                    searchedDegrees.Add(new Degree(strArray2[0], strArray2[1], (float) Convert.ToDouble(strArray2[2])));
            }
        }

        public bool doesDegreeExist(string owner_name, string degree_name, string uni_name, float gpaMin)
        {
            var list = searchedDegrees;
            var fileForName = findFileForName(owner_name);
            if (fileForName == null)
                return false;
            setDegreesFromFileEntryData(fileForName.data);
            var flag = false;
            for (var index = 0; index < searchedDegrees.Count; ++index)
            {
                if (searchedDegrees[index].GPA >= (double) gpaMin &&
                    (degree_name == null || searchedDegrees[index].name.ToLower().Equals(degree_name.ToLower())) &&
                    (uni_name == null || searchedDegrees[index].uni.ToLower().Equals(uni_name.ToLower())))
                {
                    flag = true;
                    break;
                }
            }
            searchedDegrees = list;
            return flag;
        }

        public bool hasDegrees(string owner_name)
        {
            var fileForName = findFileForName(owner_name);
            if (fileForName == null)
                return false;
            var list = searchedDegrees;
            setDegreesFromFileEntryData(fileForName.data);
            var flag = true;
            if (searchedDegrees.Count <= 0)
                flag = false;
            searchedDegrees = list;
            return flag;
        }

        private void doPreEntryViewSearch()
        {
            var fileForName = findFileForName(searchedName);
            if (fileForName != null)
                setDegreesFromFileEntryData(fileForName.data);
            else if (searchResultsNames.Count > 1)
                state = ADDState.MultipleEntriesFound;
            else
                state = ADDState.EntryNotFound;
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            drawSideBar(bounds, sb);
            var num1 = (int) (bounds.Width/5.0);
            bounds.Width -= num1;
            bounds.X += num1;
            drawTitle(bounds, sb);
            bounds.Y += 30;
            bounds.Height -= 30;
            switch (state)
            {
                case ADDState.Welcome:
                    var flag = comp.adminIP == os.thisComputer.ip;
                    var destinationRectangle = bounds;
                    destinationRectangle.Y = bounds.Y + 60 - 20;
                    destinationRectangle.Height = 22;
                    sb.Draw(Utils.white, destinationRectangle, flag ? themeColor : darkThemeColor);
                    var text1 = "Valid Administrator Account Detected";
                    if (!flag)
                        text1 = "Non-Admin Account Active";
                    var vector2 = GuiData.smallfont.MeasureString(text1);
                    var position1 = new Vector2(destinationRectangle.X + destinationRectangle.Width/2 - vector2.X/2f,
                        destinationRectangle.Y);
                    sb.DrawString(GuiData.smallfont, text1, position1, Color.Black);
                    if (Button.doButton(456011, bounds.X + 30, bounds.Y + bounds.Height/2 - 15, bounds.Width/2, 40,
                        "About This Server", themeColor))
                        state = ADDState.InfoPanel;
                    if (Button.doButton(456001, bounds.X + 30, bounds.Y + bounds.Height/2 - 15 + 50, bounds.Width/2, 40,
                        "Search Entries", themeColor))
                    {
                        state = ADDState.Seach;
                        os.execute("getString Name");
                    }
                    if (
                        !Button.doButton(456005, bounds.X + 30, bounds.Y + bounds.Height/2 - 15 + 100, bounds.Width/2,
                            40, "Exit", themeColor))
                        break;
                    os.display.command = "connect";
                    break;
                case ADDState.Seach:
                    drawSearchState(bounds, sb);
                    break;
                case ADDState.MultiMatchSearch:
                case ADDState.PendingResult:
                    if (Button.doButton(456010, bounds.X + 2, bounds.Y + 2, 160, 30, "Back", darkThemeColor))
                        state = ADDState.Welcome;
                    var position2 = new Vector2(bounds.X + bounds.Width/2, bounds.Y = bounds.Height/2);
                    var origin = new Vector2(loadingCircle.Width/2, loadingCircle.Height/2);
                    sb.Draw(loadingCircle, position2, new Rectangle?(), Color.White, (float) (os.timer%Math.PI*3.0),
                        origin, Vector2.One, SpriteEffects.None, 0.5f);
                    var num2 = os.timer - searchStartTime;
                    if ((state != ADDState.PendingResult || num2 <= 3.59999990463257) &&
                        (state != ADDState.MultiMatchSearch || num2 <= 0.699999988079071))
                        break;
                    state = ADDState.Entry;
                    needsDeletionConfirmation = true;
                    doPreEntryViewSearch();
                    break;
                case ADDState.Entry:
                case ADDState.EditPerson:
                    drawEntryState(bounds, sb);
                    break;
                case ADDState.EntryNotFound:
                    if (Button.doButton(456010, bounds.X + 2, bounds.Y + 20, 160, 30, "Back", darkThemeColor))
                        state = ADDState.Welcome;
                    if (Button.doButton(456015, bounds.X + 2, bounds.Y + 55, 160, 30, "Search Again", darkThemeColor))
                    {
                        state = ADDState.Seach;
                        os.execute("getString Name");
                    }
                    TextItem.doFontLabel(new Vector2(bounds.X + 2, bounds.Y + 90), "No Entries Found", GuiData.font,
                        new Color?(), float.MaxValue, float.MaxValue);
                    break;
                case ADDState.MultipleEntriesFound:
                    drawMultipleEntriesState(bounds, sb);
                    break;
                case ADDState.InfoPanel:
                    if (Button.doButton(456010, bounds.X + 2, bounds.Y + 30, 160, 30, "Back", darkThemeColor))
                        state = ADDState.Welcome;
                    var pos = new Vector2(bounds.X + 20, bounds.Y + 70);
                    TextItem.doFontLabel(pos, "Information", GuiData.font, new Color?(), float.MaxValue, float.MaxValue);
                    pos.Y += 40f;
                    var fileEntry = root.searchForFile("info.txt");
                    var text2 = "ERROR: Unhandled System.IO.FileNotFoundException\nFile \"info.txt\" was not found";
                    if (fileEntry != null)
                        text2 = DisplayModule.cleanSplitForWidth(fileEntry.data, bounds.Width - 80);
                    TextItem.DrawShadow = false;
                    TextItem.doFontLabel(pos, text2, GuiData.smallfont, new Color?(), bounds.Width - 40, float.MaxValue);
                    break;
                case ADDState.EditEntry:
                    drawEditDegreeState(bounds, sb);
                    break;
            }
        }

        private void drawSearchState(Rectangle bounds, SpriteBatch sb)
        {
            if (Button.doButton(456010, bounds.X + 2, bounds.Y + 2, 160, 30, "Back", darkThemeColor))
                state = ADDState.Welcome;
            var str = "";
            var x = bounds.X + 6;
            var num1 = bounds.Y + 100;
            var vector2_1 = TextItem.doMeasuredSmallLabel(new Vector2(x, num1),
                "Please enter the name you with to search for:", new Color?());
            var y1 = num1 + ((int) vector2_1.Y + 10);
            var strArray = os.getStringCache.Split(new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            }, StringSplitOptions.None);
            if (strArray.Length > 1)
            {
                str = strArray[1];
                if (str.Equals(""))
                    str = os.terminal.currentLine;
            }
            var destinationRectangle = new Rectangle(x, y1, bounds.Width - 12, 80);
            sb.Draw(Utils.white, destinationRectangle, os.darkBackgroundColor);
            var num2 = y1 + 28;
            var vector2_2 = TextItem.doMeasuredSmallLabel(new Vector2(x, num2), "Name: " + str, new Color?());
            destinationRectangle.X = x + (int) vector2_2.X + 2;
            destinationRectangle.Y = num2;
            destinationRectangle.Width = 7;
            destinationRectangle.Height = 20;
            if (os.timer%1.0 < 0.300000011920929)
                sb.Draw(Utils.white, destinationRectangle, os.outlineColor);
            var y2 = num2 + 122;
            if (strArray.Length <= 2 && !Button.doButton(30, x, y2, 300, 22, "Search", os.highlightColor))
                return;
            if (strArray.Length <= 2)
                os.terminal.executeLine();
            if (str.Length > 0)
            {
                state = ADDState.PendingResult;
                searchedName = str;
                searchStartTime = os.timer;
                comp.log("ACADEMIC_DATABASE::RecordSearch_:_" + str);
            }
            else
                state = ADDState.EntryNotFound;
        }

        private void drawMultipleEntriesState(Rectangle bounds, SpriteBatch sb)
        {
            var num1 = 22f;
            var num2 = 2f;
            var pos = new Vector2(bounds.X + 20f, bounds.Y + 10f);
            var num3 =
                (int)
                    Math.Min(searchResultsNames.Count,
                        (float) ((bounds.Height - 40.0 - 40.0 - 80.0)/(num1 + (double) num2)));
            TextItem.doFontLabel(pos, "Multiple Matches", GuiData.font, new Color?(), float.MaxValue, float.MaxValue);
            pos.Y += 30f;
            if (num3 > searchResultsNames.Count)
                TextItem.doFontLabel(new Vector2(pos.X, pos.Y - 18f), "Some Results Omitted", GuiData.tinyfont,
                    new Color?(), float.MaxValue, float.MaxValue);
            sb.Draw(Utils.white, new Rectangle((int) pos.X, (int) pos.Y, (int) (bounds.Width - (double) pos.X - 5.0), 2),
                Color.White);
            pos.Y += 12f;
            for (var index = 0; index < num3; ++index)
            {
                if (Button.doButton(1237000 + index, (int) pos.X, (int) pos.Y, (int) (bounds.Width*0.666), (int) num1,
                    searchResultsNames[index], darkThemeColor))
                {
                    searchedName = searchResultsNames[index];
                    state = ADDState.MultiMatchSearch;
                    searchStartTime = os.timer;
                }
                pos.Y += num1 + num2;
            }
            pos.Y += 5f;
            sb.Draw(Utils.white, new Rectangle((int) pos.X, (int) pos.Y, (int) (bounds.Width - (double) pos.X - 5.0), 2),
                Color.White);
            pos.Y += 10f;
            if (Button.doButton(12346080, (int) pos.X, (int) pos.Y, 160, 25, "Refine Search", themeColor))
            {
                state = ADDState.Seach;
                os.execute("getString Name");
            }
            if (!Button.doButton(12346085, (int) pos.X + 170, (int) pos.Y, 160, 25, "Go Back", darkThemeColor))
                return;
            state = ADDState.Welcome;
        }

        private void drawEntryState(Rectangle bounds, SpriteBatch sb)
        {
            if (state == ADDState.Entry && os.hasConnectionPermission(true))
                state = ADDState.EditPerson;
            if (Button.doButton(456010, bounds.X + 2, bounds.Y + 2, 160, 30, "Back", darkThemeColor))
                state = ADDState.Welcome;
            var x = bounds.X + 20f;
            var y1 = bounds.Y + 50f;
            TextItem.doFontLabel(new Vector2(x, y1), searchedName, GuiData.font, new Color?(),
                bounds.Width - (x - bounds.X), 60f);
            var y2 = y1 + 30f;
            if (searchedDegrees.Count == 0)
                TextItem.doFontLabel(new Vector2(x, y2), " -No Degrees Found", GuiData.smallfont, new Color?(),
                    float.MaxValue, float.MaxValue);
            for (var index = 0; index < searchedDegrees.Count; ++index)
            {
                var text = "Degree :" + searchedDegrees[index].name + "\nUni      :" + searchedDegrees[index].uni +
                           (object) "\nGPA      :" + searchedDegrees[index].GPA;
                TextItem.doFontLabel(new Vector2(x, y2), text, GuiData.smallfont, new Color?(),
                    bounds.Width - (bounds.X - x), 50f);
                y2 += 60f;
                if (state == ADDState.EditPerson)
                {
                    var num = y2 - 10f;
                    if (Button.doButton(457900 + index, (int) x, (int) num, 100, 20, "Edit", new Color?()))
                    {
                        state = ADDState.EditEntry;
                        editedField = ADDEditField.None;
                        editedIndex = index;
                    }
                    if (Button.doButton(456900 + index, (int) x + 105, (int) num, 100, 20,
                        needsDeletionConfirmation ? "Delete" : "Confirm?",
                        needsDeletionConfirmation ? Color.Gray : Color.Red))
                    {
                        if (needsDeletionConfirmation)
                        {
                            needsDeletionConfirmation = false;
                        }
                        else
                        {
                            comp.log(string.Concat("ACADEMIC_DATABASE::RecordDeletion_:_#", index, "_: ",
                                searchedName.Replace(" ", "_")));
                            searchedDegrees.RemoveAt(index);
                            saveChangesToEntry();
                            --index;
                            needsDeletionConfirmation = true;
                        }
                    }
                    y2 = num + 35f;
                }
            }
            var num1 = y2 + 10f;
            if (state != ADDState.EditPerson ||
                !Button.doButton(458009, (int) x, (int) num1, 170, 30, "Add Degree", themeColor))
                return;
            var degree = new Degree("UNKNOWN", "UNKNOWN", 0.0f);
            searchedDegrees.Add(degree);
            editedIndex = searchedDegrees.IndexOf(degree);
            state = ADDState.EditEntry;
            comp.log(string.Concat("ACADEMIC_DATABASE::RecordAdd_:_#", editedIndex, "_: ",
                searchedName.Replace(" ", "_")));
        }

        private void drawEditDegreeState(Rectangle bounds, SpriteBatch sb)
        {
            if (Button.doButton(456010, bounds.X + 2, bounds.Y + 2, 160, 30, "Back", darkThemeColor))
                state = ADDState.Entry;
            var pos = new Vector2(bounds.X + 10f, bounds.Y + 60f);
            var flag = editedField == ADDEditField.None;
            TextItem.doSmallLabel(pos, "University:", new Color?());
            pos.X += 110f;
            TextItem.doSmallLabel(pos, searchedDegrees[editedIndex].uni,
                editedField == ADDEditField.Uni ? themeColor : Color.White);
            pos.X -= 110f;
            pos.Y += 20f;
            if (flag && Button.doButton(46700, (int) pos.X, (int) pos.Y, 80, 20, "Edit", darkThemeColor))
            {
                editedField = ADDEditField.Uni;
                os.execute("getString University");
            }
            pos.Y += 30f;
            TextItem.doSmallLabel(pos, "Degree:", new Color?());
            pos.X += 110f;
            TextItem.doSmallLabel(pos, searchedDegrees[editedIndex].name,
                editedField == ADDEditField.Degree ? themeColor : Color.White);
            pos.X -= 110f;
            pos.Y += 20f;
            if (flag && Button.doButton(46705, (int) pos.X, (int) pos.Y, 80, 20, "Edit", darkThemeColor))
            {
                editedField = ADDEditField.Degree;
                os.execute("getString Degree");
            }
            pos.Y += 30f;
            TextItem.doSmallLabel(pos, "GPA:", new Color?());
            pos.X += 110f;
            TextItem.doSmallLabel(pos, string.Concat(searchedDegrees[editedIndex].GPA),
                editedField == ADDEditField.GPA ? themeColor : Color.White);
            pos.X -= 110f;
            pos.Y += 20f;
            if (flag && Button.doButton(46710, (int) pos.X, (int) pos.Y, 80, 20, "Edit", darkThemeColor))
            {
                editedField = ADDEditField.GPA;
                os.execute("getString GPA");
            }
            pos.Y += 30f;
            if (editedField != ADDEditField.None)
            {
                if (!doEditField())
                    return;
                editedField = ADDEditField.None;
                os.getStringCache = "";
                saveChangesToEntry();
            }
            else
            {
                if (
                    !Button.doButton(486012, bounds.X + 2, bounds.Y + bounds.Height - 40, 180, 30, "Save And Return",
                        backThemeColor))
                    return;
                state = ADDState.Entry;
                comp.log(string.Concat("ACADEMIC_DATABASE::RecordEdit_:_#", editedIndex, "_: ",
                    searchedName.Replace(" ", "_")));
            }
        }

        private bool doEditField()
        {
            var str = "";
            var strArray = os.getStringCache.Split(new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            }, StringSplitOptions.None);
            if (strArray.Length > 1)
            {
                str = strArray[1];
                if (str.Equals(""))
                    str = os.terminal.currentLine;
                setEditedFieldValue(str);
            }
            if (strArray.Length > 2)
            {
                if (strArray.Length <= 2)
                    os.terminal.executeLine();
                if (str.Length > 0)
                {
                    setEditedFieldValue(str);
                    return true;
                }
            }
            return false;
        }

        private void setEditedFieldValue(string value)
        {
            switch (editedField)
            {
                case ADDEditField.Uni:
                    searchedDegrees[editedIndex] = new Degree(searchedDegrees[editedIndex].name, value,
                        searchedDegrees[editedIndex].GPA);
                    break;
                case ADDEditField.Degree:
                    searchedDegrees[editedIndex] = new Degree(value, searchedDegrees[editedIndex].uni,
                        searchedDegrees[editedIndex].GPA);
                    break;
                case ADDEditField.GPA:
                    var degreeGPA = 2f;
                    try
                    {
                        if (value.Length > 0)
                            degreeGPA = (float) Convert.ToDouble(value);
                    }
                    catch
                    {
                    }
                    searchedDegrees[editedIndex] = new Degree(searchedDegrees[editedIndex].name,
                        searchedDegrees[editedIndex].uni, degreeGPA);
                    break;
            }
        }

        private void drawTitle(Rectangle bounds, SpriteBatch sb)
        {
            TextItem.doFontLabel(new Vector2(bounds.X, bounds.Y), "International Academic Database", GuiData.font,
                new Color?(), bounds.Width - 6, float.MaxValue);
        }

        private void drawSideBar(Rectangle bounds, SpriteBatch sb)
        {
            updateSideBar();
            var rectangle = new Rectangle(bounds.X + 2, bounds.Y + 1, bounds.Width/8, bounds.Height - 2);
            var num = bounds.Width/15;
            var destinationRectangle = rectangle;
            destinationRectangle.Width = (int) (rectangle.Width*1.5);
            sb.Draw(Utils.white, destinationRectangle, darkThemeColor);
            for (var index = 0; index < backBars.Count; ++index)
            {
                destinationRectangle.X = (int) (rectangle.X + backBars[index].X*(double) rectangle.Width);
                destinationRectangle.Width = (int) (num*(double) backBars[index].Y);
                sb.Draw(Utils.white, destinationRectangle, backThemeColor);
                destinationRectangle.X = (int) (rectangle.X + topBars[index].X*(double) rectangle.Width);
                destinationRectangle.Width = (int) (num*(double) topBars[index].Y*0.5);
                sb.Draw(Utils.white, destinationRectangle, themeColor);
            }
            destinationRectangle.X = rectangle.X;
            destinationRectangle.Width = (int) (rectangle.Width*1.5);
            sb.Draw(Utils.gradient, destinationRectangle, Color.Black);
        }

        private void updateSideBar()
        {
            var num = os.timer*0.4f;
            for (var index = 0; index < backBars.Count; ++index)
            {
                backBars[index] = new Vector2((float) (0.5 + Math.Sin(num*(double) index)*0.5),
                    (float) Math.Abs(Math.Sin(num*(double) index)));
                topBars[index] = new Vector2((float) (0.5 + Math.Sin(-num*(double) (backBars.Count - index))*0.5),
                    (float) Math.Abs(Math.Sin(-num/2.0*index)));
            }
        }

        private void saveChangesToEntry()
        {
            var strArray = searchedName.Split(Utils.spaceDelim);
            entries.searchForFile(foundFileName).data =
                getFileForPerson(new Person(strArray[0], strArray[1], true, false, null)
                {
                    degrees = searchedDegrees
                }).data;
        }

        public override void navigatedTo()
        {
            state = ADDState.Welcome;
        }

        public override void userAdded(string name, string pass, byte type)
        {
        }

        public override string getSaveString()
        {
            return "<AcademicDatabse name=\"" + name + "\"/>";
        }

        private enum ADDEditField
        {
            None,
            Uni,
            Degree,
            GPA
        }

        private enum ADDState
        {
            Welcome,
            Seach,
            MultiMatchSearch,
            Entry,
            PendingResult,
            EntryNotFound,
            MultipleEntriesFound,
            InfoPanel,
            EditPerson,
            EditEntry
        }
    }
}
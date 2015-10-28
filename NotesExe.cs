using System;
using System.Collections.Generic;
using System.Text;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class NotesExe : ExeModule
    {
        private const float RAM_CHANGE_PS = 350f;
        private const int BASE_PANEL_HEIGHT = 22;
        private const string NotesSaveFilename = "Notes.txt";
        public const string NotesReopenOnLoadFile = "Notes_Reopener.bat";
        private const string NotesSaveFileDelimiter = "\n\n----------\n\n";
        private static readonly int noteBaseCost = 8;
        private static readonly int noteCostPerLine = 14;
        private static Texture2D crossTexture;
        private readonly int baseRamCost = 100;
        private bool gettingNewNote;
        private readonly List<string> notes = new List<string>();
        private int targetRamUse = 100;

        public NotesExe(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            IdentifierName = "Notes";
            ramCost = PANEL_HEIGHT + 22;
            baseRamCost = ramCost;
            targetRamUse = ramCost;
            targetIP = os.thisComputer.ip;
            if (crossTexture != null)
                return;
            crossTexture = os.content.Load<Texture2D>("cross");
        }

        public override void LoadContent()
        {
            base.LoadContent();
            LoadNotesFromDrive();
            var folder = os.thisComputer.files.root.searchForFolder("sys");
            if (folder.searchForFile("Notes_Reopener.bat") != null)
                return;
            folder.files.Add(new FileEntry("true", "Notes_Reopener.bat"));
        }

        public override void Killed()
        {
            base.Killed();
            RemoveReopnener();
        }

        private void RemoveReopnener()
        {
            var folder = os.thisComputer.files.root.searchForFolder("sys");
            for (var index = 0; index < folder.files.Count; ++index)
            {
                if (folder.files[index].name == "Notes_Reopener.bat")
                {
                    folder.files.RemoveAt(index);
                    break;
                }
            }
        }

        public static bool NoteExists(string note, OS os)
        {
            for (var index = 0; index < os.exes.Count; ++index)
            {
                var notesExe = os.exes[index] as NotesExe;
                if (notesExe != null)
                    return notesExe.HasNote(note);
            }
            return false;
        }

        public static void AddNoteToOS(string note, OS os, bool isRecursiveSelfAdd = false)
        {
            for (var index = 0; index < os.exes.Count; ++index)
            {
                var notesExe = os.exes[index] as NotesExe;
                if (notesExe != null)
                {
                    notesExe.AddNote(note);
                    return;
                }
            }
            if (!isRecursiveSelfAdd)
                os.runCommand("notes");
            Action action = () => AddNoteToOS(note, os, true);
            os.delayer.Post(ActionDelayer.NextTick(), action);
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (targetRamUse != ramCost)
            {
                if (targetRamUse < ramCost)
                {
                    ramCost -= (int) (t*350.0);
                    if (ramCost < targetRamUse)
                        ramCost = targetRamUse;
                }
                else
                {
                    var num = (int) (t*350.0);
                    if (os.ramAvaliable >= num)
                    {
                        ramCost += num;
                        if (ramCost > targetRamUse)
                            ramCost = targetRamUse;
                    }
                }
            }
            if (!gettingNewNote)
                return;
            string data = null;
            if (!Programs.parseStringFromGetStringCommand(os, out data))
                return;
            gettingNewNote = false;
            AddNote(data);
        }

        public void AddNote(string note)
        {
            var str = Utils.SuperSmartTwimForWidth(note, os.ram.bounds.Width - 8, GuiData.UITinyfont);
            for (var index = 0; index < notes.Count; ++index)
            {
                if (notes[index] == str)
                    return;
            }
            notes.Add(str);
            recalcualteRamCost();
            SaveNotesToDrive();
        }

        public bool HasNote(string note)
        {
            var str = Utils.SuperSmartTwimForWidth(note, os.ram.bounds.Width - 8, GuiData.UITinyfont);
            for (var index = 0; index < notes.Count; ++index)
            {
                if (notes[index] == str)
                    return true;
            }
            return false;
        }

        private void LoadNotesFromDrive()
        {
            var fileEntry = os.thisComputer.files.root.searchForFolder("home").searchForFile("Notes.txt");
            if (fileEntry == null)
                return;
            var strArray = fileEntry.data.Split(new string[1]
            {
                "\n\n----------\n\n"
            }, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length < 0)
                return;
            notes.AddRange(strArray);
            recalcualteRamCost();
        }

        private void SaveNotesToDrive()
        {
            var folder = os.thisComputer.files.root.searchForFolder("home");
            var fileEntry = folder.searchForFile("Notes.txt");
            if (fileEntry == null)
            {
                fileEntry = new FileEntry("", "Notes.txt");
                folder.files.Add(fileEntry);
            }
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < notes.Count; ++index)
            {
                if (index != 0)
                    stringBuilder.Append("\n\n----------\n\n");
                stringBuilder.Append(notes[index]);
            }
            fileEntry.data = stringBuilder.ToString();
        }

        private void recalcualteRamCost()
        {
            targetRamUse = baseRamCost;
            for (var index = 0; index < notes.Count; ++index)
                targetRamUse += noteBaseCost + notes[index].Split(Utils.newlineDelim).Length*noteCostPerLine;
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            if (ramCost >= PANEL_HEIGHT)
            {
                drawOutline();
                drawTarget("");
            }
            else
                drawFrame();
            DrawNotes(new Rectangle(bounds.X + 2, bounds.Y + 2 + PANEL_HEIGHT, bounds.Width - 4,
                ramCost - PANEL_HEIGHT - 22));
            if (ramCost < PANEL_HEIGHT + 22)
                return;
            DrawBasePanel(new Rectangle(bounds.X + 2, bounds.Y + bounds.Height - 22, bounds.Width - 4, 22));
        }

        private void DrawBasePanel(Rectangle dest)
        {
            var width = Math.Min(120, (dest.Width - 8)/2);
            if (Button.doButton(631012 + os.exes.IndexOf(this), dest.X + dest.Width - width, dest.Y + 4, width,
                dest.Height - 6, "Close", os.lockedColor*fade))
            {
                if (gettingNewNote)
                {
                    os.terminal.executeLine();
                    gettingNewNote = false;
                }
                Completed();
                RemoveReopnener();
                isExiting = true;
            }
            if (!gettingNewNote &&
                Button.doButton(631014 + os.exes.IndexOf(this), dest.X, dest.Y + 4, width, dest.Height - 6, "Add Note",
                    os.highlightColor*fade))
            {
                os.runCommand("getString Note");
                os.getStringCache = "";
                gettingNewNote = true;
            }
            else
            {
                if (!gettingNewNote)
                    return;
                var dest1 = new Rectangle(dest.X, dest.Y + 4, width, dest.Height - 6);
                PatternDrawer.draw(dest1, 1f, Color.Transparent, os.highlightColor*0.5f, spriteBatch,
                    PatternDrawer.thinStripe);
                TextItem.doFontLabelToSize(dest1, "Type In Terminal...", GuiData.smallfont, Color.White);
            }
        }

        private void DrawNotes(Rectangle dest)
        {
            var num1 = dest.Y;
            for (var index1 = 0; index1 < notes.Count; ++index1)
            {
                var strArray = notes[index1].Split(Utils.newlineDelim);
                var num2 = noteBaseCost + strArray.Length*noteCostPerLine;
                if (num1 - dest.Y + num2 > dest.Height + 1)
                    break;
                var destinationRectangle = new Rectangle(dest.X, num1 + noteBaseCost/2 - 2, dest.Width,
                    num2 - noteBaseCost/2);
                spriteBatch.Draw(Utils.white, destinationRectangle, os.highlightColor*0.2f);
                var num3 = num1 + noteBaseCost/2;
                for (var index2 = 0; index2 < strArray.Length; ++index2)
                {
                    spriteBatch.DrawString(GuiData.UITinyfont, strArray[index2],
                        new Vector2(destinationRectangle.X + 2, num3), Color.White);
                    num3 += noteCostPerLine;
                }
                var num4 = 13;
                if (Button.doButton(539261 + index1*100 + os.exes.IndexOf(this)*2000,
                    destinationRectangle.X + destinationRectangle.Width - num4 - 1, num1 + noteBaseCost/2 + 1, num4,
                    num4, "", Color.White*0.5f, crossTexture))
                {
                    notes.RemoveAt(index1);
                    recalcualteRamCost();
                    SaveNotesToDrive();
                    --index1;
                }
                num1 += num2;
            }
        }
    }
}
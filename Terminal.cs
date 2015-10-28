// Decompiled with JetBrains decompiler
// Type: Hacknet.Terminal
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class Terminal : CoreModule
    {
        public static float PROMPT_OFFSET;
        private Color backColor = new Color(8, 8, 8);
        private int commandHistoryOffset;
        public string currentLine;
        private readonly Color currentTextColor = Color.White;
        public bool executionPreventionIsInteruptable;
        private List<string> history;
        private Color historyTextColor = new Color(220, 220, 220);
        public string lastRunCommand;
        private Color outlineColor = new Color(68, 68, 68);
        public bool preventingExecution;
        public string prompt;
        private List<string> runCommands = new List<string>();
        public bool usingTabExecution;

        public Terminal(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
        }

        public override void LoadContent()
        {
            history = new List<string>(512);
            runCommands = new List<string>(512);
            commandHistoryOffset = 0;
            currentLine = "";
            lastRunCommand = "";
            prompt = "> ";
        }

        public override void Update(float t)
        {
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            var num1 = GuiData.ActiveFontConfig.tinyFontCharHeight;
            spriteBatch.Draw(Utils.white, bounds, os.displayModuleExtraLayerBackingColor);
            var num2 = Math.Min((int) ((bounds.Height - 12)/(num1 + 1.0)) - 3, history.Count);
            var position = new Vector2(bounds.X + 4, bounds.Y + bounds.Height - num1*5f);
            if (num2 > 0)
            {
                for (var count = history.Count; count > history.Count - num2; --count)
                {
                    try
                    {
                        spriteBatch.DrawString(GuiData.tinyfont, history[count - 1], position, os.terminalTextColor);
                        position.Y -= num1 + 1f;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            doGui();
        }

        public void executeLine()
        {
            var str = currentLine;
            if (TextBox.MaskingText)
            {
                str = "";
                for (var index = 0; index < currentLine.Length; ++index)
                    str += "*";
            }
            history.Add(prompt + str);
            lastRunCommand = currentLine;
            runCommands.Add(currentLine);
            if (!preventingExecution)
            {
                commandHistoryOffset = 0;
                os.execute(currentLine);
                if (currentLine.Length > 0)
                    StatsManager.IncrementStat("commands_run", 1);
            }
            currentLine = "";
            TextBox.cursorPosition = 0;
            TextBox.textDrawOffsetPosition = 0;
            executionPreventionIsInteruptable = false;
        }

        public string GetRecentTerminalHistoryString()
        {
            var str = "";
            for (var index = history.Count - 1; index > history.Count - 30 && history.Count > index; --index)
                str = str + history[index] + "\r\n";
            return str;
        }

        public void NonThreadedInstantExecuteLine()
        {
            var str = currentLine;
            if (TextBox.MaskingText)
            {
                str = "";
                for (var index = 0; index < currentLine.Length; ++index)
                    str += "*";
            }
            history.Add(prompt + str);
            lastRunCommand = currentLine;
            runCommands.Add(currentLine);
            if (!preventingExecution)
            {
                commandHistoryOffset = 0;
                ProgramRunner.ExecuteProgram(os, currentLine.Split(' '));
            }
            currentLine = "";
            TextBox.cursorPosition = 0;
            TextBox.textDrawOffsetPosition = 0;
            executionPreventionIsInteruptable = false;
        }

        public void doGui()
        {
            var spriteFont = GuiData.tinyfont;
            var num1 = GuiData.ActiveFontConfig.tinyFontCharHeight;
            var num2 = -4;
            var num3 = (int) (bounds.Y + bounds.Height - 16 - (double) num1 - num2);
            int num4;
            for (num4 = (int) spriteFont.MeasureString(prompt).X;
                num4 >= (int) (bounds.Width*0.7);
                num4 = (int) spriteFont.MeasureString(prompt).X)
                prompt = prompt.Substring(1);
            spriteBatch.DrawString(spriteFont, prompt, new Vector2(bounds.X + 3, num3), currentTextColor);
            var y = num3 + num2;
            if (!os.inputEnabled || inputLocked)
                return;
            TextBox.LINE_HEIGHT = (int) (num1 + 15.0);
            currentLine = TextBox.doTerminalTextField(7001,
                bounds.X + 3 + (int) PROMPT_OFFSET + (int) spriteFont.MeasureString(prompt).X, y,
                bounds.Width - num4 - 4, bounds.Height, 1, currentLine, spriteFont);
            if (TextBox.BoxWasActivated)
                executeLine();
            if (TextBox.UpWasPresed && runCommands.Count > 0)
            {
                ++commandHistoryOffset;
                if (commandHistoryOffset > runCommands.Count)
                    commandHistoryOffset = runCommands.Count;
                currentLine = runCommands[runCommands.Count - commandHistoryOffset];
                TextBox.cursorPosition = currentLine.Length;
            }
            if (TextBox.DownWasPresed && commandHistoryOffset > 0)
            {
                --commandHistoryOffset;
                if (commandHistoryOffset < 0)
                    commandHistoryOffset = 0;
                currentLine = commandHistoryOffset > 0 ? runCommands[runCommands.Count - commandHistoryOffset] : "";
                TextBox.cursorPosition = currentLine.Length;
            }
            if (!TextBox.TabWasPresed)
                return;
            if (usingTabExecution)
                executeLine();
            else
                doTabComplete();
        }

        public void doTabComplete()
        {
            var list1 = new List<string>();
            if (currentLine.Length == 0)
                return;
            var length1 = currentLine.IndexOf(' ');
            if (length1 >= 1)
            {
                var path = currentLine.Substring(length1 + 1);
                var str1 = currentLine.Substring(0, length1);
                if (path == null || (path.Equals("") || path.Length < 1) && !str1.Equals("exe"))
                    return;
                if (str1.Equals("upload") || str1.Equals("up"))
                {
                    var length2 = path.LastIndexOf('/');
                    if (length2 < 0)
                        length2 = 0;
                    var str2 = path.Substring(0, length2) + "/";
                    if (str2.StartsWith("/"))
                        str2 = str2.Substring(1);
                    var str3 = path.Substring(length2 + (length2 == 0 ? 0 : 1));
                    var folder = Programs.getFolderAtPathAsFarAsPossible(path, os, os.thisComputer.files.root) ??
                                 os.thisComputer.files.root;
                    for (var index = 0; index < folder.folders.Count; ++index)
                    {
                        if (folder.folders[index].name.StartsWith(str3, StringComparison.InvariantCultureIgnoreCase))
                            list1.Add(str1 + " " + str2 + folder.folders[index].name + "/");
                    }
                    for (var index = 0; index < folder.files.Count; ++index)
                    {
                        if (folder.files[index].name.StartsWith(str3))
                            list1.Add(str1 + " " + str2 + folder.files[index].name);
                    }
                }
                else
                {
                    var currentFolder = Programs.getCurrentFolder(os);
                    for (var index = 0; index < currentFolder.folders.Count; ++index)
                    {
                        if (currentFolder.folders[index].name.StartsWith(path,
                            StringComparison.InvariantCultureIgnoreCase))
                            list1.Add(str1 + " " + currentFolder.folders[index].name + "/");
                    }
                    for (var index = 0; index < currentFolder.files.Count; ++index)
                    {
                        if (currentFolder.files[index].name.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                            list1.Add(str1 + " " + currentFolder.files[index].name);
                    }
                    if (list1.Count == 0)
                    {
                        for (var index = 0; index < currentFolder.files.Count; ++index)
                        {
                            if (currentFolder.files[index].name.StartsWith(path,
                                StringComparison.InvariantCultureIgnoreCase))
                                list1.Add(str1 + " " + currentFolder.files[index].name);
                        }
                    }
                }
            }
            else
            {
                var list2 = new List<string>();
                list2.AddRange(ProgramList.programs);
                list2.AddRange(ProgramList.getExeList(os));
                for (var index = 0; index < list2.Count; ++index)
                {
                    if (list2[index].ToLower().StartsWith(currentLine.ToLower()))
                        list1.Add(list2[index]);
                }
            }
            if (list1.Count == 1)
            {
                currentLine = list1[0];
                TextBox.moveCursorToEnd(currentLine);
            }
            else
            {
                if (list1.Count <= 1)
                    return;
                os.write(prompt + currentLine);
                var str = list1[0];
                for (var index = 0; index < list1.Count; ++index)
                {
                    os.write(list1[index]);
                    for (var length2 = 0; length2 < str.Length; ++length2)
                    {
                        if (list1[index].Length <= length2 || str[length2] != list1[index][length2])
                        {
                            str = str.Substring(0, length2);
                            break;
                        }
                    }
                    currentLine = str;
                    TextBox.moveCursorToEnd(currentLine);
                }
            }
        }

        public void writeLine(string text)
        {
            text = Utils.SuperSmartTwimForWidth(text, bounds.Width - 6, GuiData.tinyfont);
            var str1 = text;
            var chArray = new char[1]
            {
                '\n'
            };
            foreach (var str2 in str1.Split(chArray))
                history.Add(str2);
        }

        public void write(string text)
        {
            if (GuiData.tinyfont.MeasureString(history[history.Count - 1] + text).X > (double) (bounds.Width - 6))
            {
                writeLine(text);
            }
            else
            {
                List<string> list;
                int index;
                (list = history)[index = history.Count - 1] = list[index] + text;
            }
        }

        public void clearCurrentLine()
        {
            currentLine = "";
            TextBox.cursorPosition = 0;
            TextBox.textDrawOffsetPosition = 0;
        }

        public void reset()
        {
            history.Clear();
            clearCurrentLine();
        }

        public int commandsRun()
        {
            return runCommands.Count;
        }

        public string getLastRunCommand()
        {
            return lastRunCommand;
        }
    }
}
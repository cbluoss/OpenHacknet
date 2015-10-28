// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.TextBox
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet.Gui
{
    public static class TextBox
    {
        private const float DELAY_BEFORE_KEY_REPEAT_START = 0.44f;
        private const float KEY_REPEAT_DELAY = 0.04f;
        private const int OUTLINE_WIDTH = 2;
        private static float keyRepeatDelay = 0.44f;
        public static int LINE_HEIGHT = 25;
        public static int cursorPosition;
        public static int textDrawOffsetPosition;
        private static int FramesSelected;
        public static bool MaskingText = false;
        public static bool BoxWasActivated;
        public static bool UpWasPresed;
        public static bool DownWasPresed;
        public static bool TabWasPresed;
        private static Keys lastHeldKey;

        public static string doTextBox(int myID, int x, int y, int width, int lines, string str, SpriteFont font)
        {
            var str1 = str;
            if (font == null)
                font = GuiData.smallfont;
            BoxWasActivated = false;
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = lines*LINE_HEIGHT;
            if (destinationRectangle.Contains(GuiData.getMousePoint()))
                GuiData.hot = myID;
            else if (GuiData.hot == myID)
                GuiData.hot = -1;
            if (GuiData.mouseWasPressed())
            {
                if (GuiData.hot == myID)
                {
                    if (GuiData.active == myID)
                    {
                        var num = GuiData.mouse.X - x;
                        var flag = false;
                        for (var length = 1; length <= str.Length; ++length)
                        {
                            if (font.MeasureString(str.Substring(0, length)).X > (double) num)
                            {
                                cursorPosition = length - 1;
                                break;
                            }
                            if (!flag)
                                cursorPosition = str.Length;
                        }
                    }
                    else
                    {
                        GuiData.active = myID;
                        cursorPosition = str.Length;
                    }
                }
                else if (GuiData.active == myID)
                    GuiData.active = -1;
            }
            if (GuiData.active == myID)
            {
                GuiData.willBlockTextInput = true;
                str1 = getStringInput(str1, GuiData.getKeyboadState(), GuiData.getLastKeyboadState());
                if (GuiData.getKeyboadState().IsKeyDown(Keys.Enter) &&
                    GuiData.getLastKeyboadState().IsKeyDown(Keys.Enter))
                {
                    BoxWasActivated = true;
                    GuiData.active = -1;
                }
            }
            ++FramesSelected;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = lines*LINE_HEIGHT;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                GuiData.active == myID
                    ? GuiData.Default_Lit_Backing_Color
                    : (GuiData.hot == myID ? GuiData.Default_Selected_Color : GuiData.Default_Dark_Background_Color));
            destinationRectangle.X += 2;
            destinationRectangle.Y += 2;
            destinationRectangle.Width -= 4;
            destinationRectangle.Height -= 4;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, GuiData.Default_Light_Backing_Color);
            var num1 = (float) ((LINE_HEIGHT - (double) font.MeasureString(str1).Y)/2.0);
            GuiData.spriteBatch.DrawString(font, str1, new Vector2(x + 2, y + num1), Color.White);
            if (GuiData.active == myID)
            {
                destinationRectangle.X = (int) (x + (double) font.MeasureString(str1.Substring(0, cursorPosition)).X) +
                                         3;
                destinationRectangle.Y = y + 2;
                destinationRectangle.Width = 1;
                destinationRectangle.Height = LINE_HEIGHT - 4;
                GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                    FramesSelected%60 < 40 ? Color.White : Color.Gray);
            }
            return str1;
        }

        public static string doTerminalTextField(int myID, int x, int y, int width, int selectionHeight, int lines,
            string str, SpriteFont font)
        {
            var s = str;
            if (font == null)
                font = GuiData.smallfont;
            BoxWasActivated = false;
            UpWasPresed = false;
            DownWasPresed = false;
            TabWasPresed = false;
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = 0;
            if (destinationRectangle.Contains(GuiData.getMousePoint()))
                GuiData.hot = myID;
            else if (GuiData.hot == myID)
                GuiData.hot = -1;
            if (GuiData.mouseWasPressed())
            {
                if (GuiData.hot == myID)
                {
                    if (GuiData.active == myID)
                    {
                        var num = GuiData.mouse.X - x;
                        var flag = false;
                        for (var length = 1; length <= str.Length; ++length)
                        {
                            if (font.MeasureString(str.Substring(0, length)).X > (double) num)
                            {
                                cursorPosition = length - 1;
                                break;
                            }
                            if (!flag)
                                cursorPosition = str.Length;
                        }
                    }
                    else
                    {
                        GuiData.active = myID;
                        cursorPosition = str.Length;
                    }
                }
                else if (GuiData.active == myID)
                    GuiData.active = -1;
            }
            var num1 = GuiData.active;
            var filteredStringInput = getFilteredStringInput(s, GuiData.getKeyboadState(), GuiData.getLastKeyboadState());
            if (GuiData.getKeyboadState().IsKeyDown(Keys.Enter) && !GuiData.getLastKeyboadState().IsKeyDown(Keys.Enter))
            {
                BoxWasActivated = true;
                cursorPosition = 0;
                textDrawOffsetPosition = 0;
            }
            destinationRectangle.Height = lines*LINE_HEIGHT;
            ++FramesSelected;
            destinationRectangle.X = x;
            destinationRectangle.Y = y;
            destinationRectangle.Width = width;
            destinationRectangle.Height = 10;
            destinationRectangle.X += 2;
            destinationRectangle.Y += 2;
            destinationRectangle.Width -= 4;
            destinationRectangle.Height -= 4;
            var num2 = (float) ((LINE_HEIGHT - (double) font.MeasureString(filteredStringInput).Y)/2.0);
            var str1 = filteredStringInput;
            var num3 = 0;
            var startIndex = 0;
            int length1;
            for (var text = str1;
                font.MeasureString(text).X > (double) (width - 5);
                text = str1.Substring(startIndex, length1))
            {
                ++num3;
                length1 = str1.Length - startIndex - (num3 - startIndex);
            }
            if (cursorPosition < textDrawOffsetPosition)
                textDrawOffsetPosition = Math.Max(0, textDrawOffsetPosition - 1);
            while (cursorPosition > textDrawOffsetPosition + (str1.Length - num3))
                ++textDrawOffsetPosition;
            if (str1.Length <= num3 || textDrawOffsetPosition < 0)
                textDrawOffsetPosition = textDrawOffsetPosition > str1.Length - num3 ? 0 : str1.Length - num3;
            else if (textDrawOffsetPosition > num3)
                num3 = textDrawOffsetPosition;
            var text1 = str1.Substring(textDrawOffsetPosition, str1.Length - num3);
            if (MaskingText)
            {
                var str2 = "";
                for (var index = 0; index < filteredStringInput.Length; ++index)
                    str2 += "*";
                text1 = str2;
            }
            GuiData.spriteBatch.DrawString(font, text1, new Vector2(x + 2, y + num2), Color.White);
            var num4 = GuiData.active;
            if (filteredStringInput != "")
            {
                var length2 = Math.Min(cursorPosition - textDrawOffsetPosition, text1.Length);
                destinationRectangle.X = (int) (x + (double) font.MeasureString(text1.Substring(0, length2)).X) + 3;
            }
            else
                destinationRectangle.X = x + 3;
            destinationRectangle.Y = y + 2;
            destinationRectangle.Width = 1;
            destinationRectangle.Height = LINE_HEIGHT - 4;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle,
                FramesSelected%60 < 40 ? Color.White : Color.Gray);
            return filteredStringInput;
        }

        private static string getStringInput(string s, KeyboardState input, KeyboardState lastInput)
        {
            var pressedKeys = input.GetPressedKeys();
            for (var index = 0; index < pressedKeys.Length; ++index)
            {
                if (!lastInput.IsKeyDown(pressedKeys[index]))
                {
                    if (!IsSpecialKey(pressedKeys[index]))
                    {
                        var str = ConvertKeyToChar(pressedKeys[index],
                            input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.CapsLock));
                        s = s.Substring(0, cursorPosition) + str + s.Substring(cursorPosition);
                        ++cursorPosition;
                    }
                    else
                    {
                        switch (pressedKeys[index])
                        {
                            case Keys.Delete:
                            case Keys.OemClear:
                            case Keys.Back:
                                if (s.Length > 0 && cursorPosition > 0)
                                {
                                    s = s.Substring(0, cursorPosition - 1) + s.Substring(cursorPosition);
                                    --cursorPosition;
                                }
                                continue;
                            case Keys.Left:
                                --cursorPosition;
                                if (cursorPosition < 0)
                                {
                                    cursorPosition = 0;
                                }
                                continue;
                            case Keys.Right:
                                ++cursorPosition;
                                if (cursorPosition > s.Length)
                                {
                                    cursorPosition = s.Length;
                                }
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }
            return s;
        }

        private static string getFilteredStringInput(string s, KeyboardState input, KeyboardState lastInput)
        {
            foreach (var ch in GuiData.getFilteredKeys())
            {
                s = s.Substring(0, cursorPosition) + ch + s.Substring(cursorPosition);
                ++cursorPosition;
            }
            var pressedKeys = input.GetPressedKeys();
            if (pressedKeys.Length == 1 && lastInput.IsKeyDown(pressedKeys[0]))
            {
                if (pressedKeys[0] == lastHeldKey && IsSpecialKey(pressedKeys[0]))
                {
                    keyRepeatDelay -= GuiData.lastTimeStep;
                    if (keyRepeatDelay <= 0.0)
                    {
                        s = forceHandleKeyPress(s, pressedKeys[0], input, lastInput);
                        keyRepeatDelay = 0.04f;
                    }
                }
                else
                {
                    lastHeldKey = pressedKeys[0];
                    keyRepeatDelay = 0.44f;
                }
            }
            else
            {
                for (var index = 0; index < pressedKeys.Length; ++index)
                {
                    if (!lastInput.IsKeyDown(pressedKeys[index]) && IsSpecialKey(pressedKeys[index]))
                    {
                        switch (pressedKeys[index])
                        {
                            case Keys.Back:
                            case Keys.OemClear:
                                if (s.Length > 0 && cursorPosition > 0)
                                {
                                    s = s.Substring(0, cursorPosition - 1) + s.Substring(cursorPosition);
                                    --cursorPosition;
                                }
                                continue;
                            case Keys.Tab:
                                TabWasPresed = true;
                                continue;
                            case Keys.End:
                                cursorPosition = cursorPosition = s.Length;
                                continue;
                            case Keys.Home:
                                cursorPosition = 0;
                                continue;
                            case Keys.Left:
                                --cursorPosition;
                                if (cursorPosition < 0)
                                {
                                    cursorPosition = 0;
                                }
                                continue;
                            case Keys.Up:
                                UpWasPresed = true;
                                continue;
                            case Keys.Right:
                                ++cursorPosition;
                                if (cursorPosition > s.Length)
                                {
                                    cursorPosition = s.Length;
                                }
                                continue;
                            case Keys.Down:
                                DownWasPresed = true;
                                continue;
                            case Keys.Delete:
                                if (s.Length > 0 && cursorPosition < s.Length)
                                {
                                    s = s.Substring(0, cursorPosition) + s.Substring(cursorPosition + 1);
                                }
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }
            return s;
        }

        private static string forceHandleKeyPress(string s, Keys key, KeyboardState input, KeyboardState lastInput)
        {
            if (!IsSpecialKey(key))
            {
                var str = ConvertKeyToChar(key,
                    input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.CapsLock) || input.IsKeyDown(Keys.RightAlt));
                s = s.Substring(0, cursorPosition) + str + s.Substring(cursorPosition);
                ++cursorPosition;
            }
            else
            {
                switch (key)
                {
                    case Keys.Delete:
                    case Keys.OemClear:
                    case Keys.Back:
                        if (s.Length > 0 && cursorPosition > 0)
                        {
                            s = s.Substring(0, cursorPosition - 1) + s.Substring(cursorPosition);
                            --cursorPosition;
                        }
                        break;
                    case Keys.Tab:
                        TabWasPresed = true;
                        break;
                    case Keys.Left:
                        --cursorPosition;
                        if (cursorPosition < 0)
                        {
                            cursorPosition = 0;
                        }
                        break;
                    case Keys.Up:
                        UpWasPresed = true;
                        break;
                    case Keys.Right:
                        ++cursorPosition;
                        if (cursorPosition > s.Length)
                        {
                            cursorPosition = s.Length;
                        }
                        break;
                    case Keys.Down:
                        DownWasPresed = true;
                        break;
                }
            }
            return s;
        }

        public static bool IsSpecialKey(Keys key)
        {
            var num = (int) key;
            return (num < 65 || num > 90) && (num < 48 || num > 57) &&
                   (key != Keys.Space && key != Keys.OemPeriod && (key != Keys.OemComma && key != Keys.OemTilde)) &&
                   (key != Keys.OemMinus && key != Keys.OemPipe &&
                    (key != Keys.OemOpenBrackets && key != Keys.OemCloseBrackets) &&
                    (key != Keys.OemQuotes && key != Keys.OemQuestion && key != Keys.OemPlus));
        }

        public static string ConvertKeyToChar(Keys key, bool shift)
        {
            switch (key)
            {
                case Keys.Tab:
                    return "\t";
                case Keys.Enter:
                    return "\n";
                case Keys.Space:
                    return " ";
                case Keys.D0:
                    return !shift ? "0" : ")";
                case Keys.D1:
                    return !shift ? "1" : "!";
                case Keys.D2:
                    return !shift ? "2" : "@";
                case Keys.D3:
                    return !shift ? "3" : "#";
                case Keys.D4:
                    return !shift ? "4" : "$";
                case Keys.D5:
                    return !shift ? "5" : "%";
                case Keys.D6:
                    return !shift ? "6" : "^";
                case Keys.D7:
                    return !shift ? "7" : "&";
                case Keys.D8:
                    return !shift ? "8" : "*";
                case Keys.D9:
                    return !shift ? "9" : "(";
                case Keys.A:
                    return !shift ? "a" : "A";
                case Keys.B:
                    return !shift ? "b" : "B";
                case Keys.C:
                    return !shift ? "c" : "C";
                case Keys.D:
                    return !shift ? "d" : "D";
                case Keys.E:
                    return !shift ? "e" : "E";
                case Keys.F:
                    return !shift ? "f" : "F";
                case Keys.G:
                    return !shift ? "g" : "G";
                case Keys.H:
                    return !shift ? "h" : "H";
                case Keys.I:
                    return !shift ? "i" : "I";
                case Keys.J:
                    return !shift ? "j" : "J";
                case Keys.K:
                    return !shift ? "k" : "K";
                case Keys.L:
                    return !shift ? "l" : "L";
                case Keys.M:
                    return !shift ? "m" : "M";
                case Keys.N:
                    return !shift ? "n" : "N";
                case Keys.O:
                    return !shift ? "o" : "O";
                case Keys.P:
                    return !shift ? "p" : "P";
                case Keys.Q:
                    return !shift ? "q" : "Q";
                case Keys.R:
                    return !shift ? "r" : "R";
                case Keys.S:
                    return !shift ? "s" : "S";
                case Keys.T:
                    return !shift ? "t" : "T";
                case Keys.U:
                    return !shift ? "u" : "U";
                case Keys.V:
                    return !shift ? "v" : "V";
                case Keys.W:
                    return !shift ? "w" : "W";
                case Keys.X:
                    return !shift ? "x" : "X";
                case Keys.Y:
                    return !shift ? "y" : "Y";
                case Keys.Z:
                    return !shift ? "z" : "Z";
                case Keys.NumPad0:
                    return "0";
                case Keys.NumPad1:
                    return "1";
                case Keys.NumPad2:
                    return "2";
                case Keys.NumPad3:
                    return "3";
                case Keys.NumPad4:
                    return "4";
                case Keys.NumPad5:
                    return "5";
                case Keys.NumPad6:
                    return "6";
                case Keys.NumPad7:
                    return "7";
                case Keys.NumPad8:
                    return "8";
                case Keys.NumPad9:
                    return "9";
                case Keys.Multiply:
                    return "*";
                case Keys.Add:
                    return "+";
                case Keys.Subtract:
                    return "-";
                case Keys.Decimal:
                    return ".";
                case Keys.Divide:
                    return "/";
                case Keys.OemSemicolon:
                    return !shift ? ";" : ":";
                case Keys.OemPlus:
                    return !shift ? "=" : "+";
                case Keys.OemComma:
                    return !shift ? "," : "<";
                case Keys.OemMinus:
                    return !shift ? "-" : "_";
                case Keys.OemPeriod:
                    return !shift ? "." : ">";
                case Keys.OemQuestion:
                    return !shift ? "/" : "?";
                case Keys.OemTilde:
                    return !shift ? "`" : "~";
                case Keys.OemOpenBrackets:
                    return !shift ? "[" : "{";
                case Keys.OemPipe:
                    return !shift ? "\\" : "|";
                case Keys.OemCloseBrackets:
                    return !shift ? "]" : "}";
                case Keys.OemQuotes:
                    return !shift ? "'" : "\"";
                default:
                    return string.Empty;
            }
        }

        public static void moveCursorToEnd(string targetString)
        {
            cursorPosition = targetString.Length;
        }
    }
}
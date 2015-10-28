// Decompiled with JetBrains decompiler
// Type: Hacknet.UIUtils.SavefileLoginScreen
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Text;
using Hacknet.Gui;
using Hacknet.PlatformAPI.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet.UIUtils
{
    public class SavefileLoginScreen
    {
        private static readonly Color CancelColor = new Color(125, 82, 82);
        private List<string> Answers = new List<string>();
        private bool CanReturnEnter;
        private string currentPrompt = "USERNAME :";
        private readonly List<string> History = new List<string>();
        private bool InPasswordMode;
        private bool IsNewAccountMode = true;
        private bool IsReady;
        public Action<string, string> LoadGameForUserFileAndUsername;
        private bool PreventAdvancing;
        private int promptIndex;
        private readonly List<string> PromptSequence = new List<string>();
        public Action RequestGoBack;
        public Action<string, string> StartNewGameForUsernameAndPass;
        private string terminalString = "";
        private string userPathCache;

        public void WriteToHistory(string message)
        {
            History.Add(message);
        }

        public void ResetForNewAccount()
        {
            promptIndex = 0;
            IsReady = false;
            PromptSequence.Clear();
            PromptSequence.Add("USERNAME :");
            PromptSequence.Add("PASSWORD :");
            PromptSequence.Add("CONFIRM PASS :");
            History.Clear();
            History.Add("-- New Hacknet User Registration --");
            currentPrompt = PromptSequence[promptIndex];
            IsNewAccountMode = true;
            terminalString = "";
            TextBox.cursorPosition = 0;
        }

        public void ResetForLogin()
        {
            promptIndex = 0;
            IsReady = false;
            PromptSequence.Clear();
            PromptSequence.Add("USERNAME :");
            PromptSequence.Add("PASSWORD :");
            History.Clear();
            History.Add("-- Login --");
            currentPrompt = PromptSequence[promptIndex];
            IsNewAccountMode = false;
            terminalString = "";
            TextBox.cursorPosition = 0;
        }

        private void Advance(string answer)
        {
            ++promptIndex;
            Answers.Add(answer);
            if (IsNewAccountMode)
            {
                if (promptIndex == 1 && !SaveFileManager.CanCreateAccountForName(Answers[0]))
                {
                    History.Add(" -- Username already in use. Try Again -- ");
                    promptIndex = 0;
                    Answers.Clear();
                }
                if (promptIndex == 3)
                {
                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        History.Add(" -- Password Cannot be Blank! Try Again -- ");
                        promptIndex = 1;
                        var str = Answers[0];
                        Answers.Clear();
                        Answers.Add(str);
                    }
                    else if (Answers[1] != answer)
                    {
                        History.Add(" -- Password Mismatch! Try Again -- ");
                        promptIndex = 1;
                        var str = Answers[0];
                        Answers.Clear();
                        Answers.Add(str);
                    }
                }
                InPasswordMode = promptIndex == 1 || promptIndex == 2;
            }
            else
                InPasswordMode = promptIndex == 1;
            if (promptIndex >= PromptSequence.Count)
            {
                if (IsNewAccountMode)
                {
                    History.Add(" -- Details Confirmed -- ");
                    currentPrompt = "READY - PRESS ENTER TO CONFIRM";
                    IsReady = true;
                }
                else
                {
                    var filePathForLogin = SaveFileManager.GetFilePathForLogin(Answers[0], Answers[1]);
                    userPathCache = filePathForLogin;
                    if (filePathForLogin == null)
                    {
                        promptIndex = 0;
                        currentPrompt = PromptSequence[promptIndex];
                        History.Add(" -- Invalid Login Details -- ");
                    }
                    else
                    {
                        IsReady = true;
                        currentPrompt = "READY - PRESS ENTER TO CONFIRM";
                    }
                }
            }
            else
                currentPrompt = PromptSequence[promptIndex];
        }

        public void Draw(SpriteBatch sb, Rectangle dest)
        {
            var width = 300;
            var rectangle1 = dest;
            var rectangle2 = new Rectangle(dest.X, dest.Y, dest.Width - width, dest.Height);
            if (!IsNewAccountMode)
                dest = rectangle2;
            var spriteFont = GuiData.smallfont;
            var num1 = (int) (spriteFont.MeasureString(currentPrompt).X + 4.0);
            GuiData.spriteBatch.DrawString(spriteFont, currentPrompt, new Vector2(dest.X, dest.Y + dest.Height - 16),
                Color.White);
            if (!IsReady)
            {
                TextBox.MaskingText = InPasswordMode;
                terminalString = TextBox.doTerminalTextField(16392802, dest.X + num1, dest.Y + dest.Height - 18,
                    dest.Width, 20, 1, terminalString, GuiData.UISmallfont);
            }
            if (!IsNewAccountMode)
            {
                var pos = new Vector2(rectangle1.X + rectangle2.Width, rectangle1.Y);
                if (SaveFileManager.Accounts.Count > 0)
                {
                    TextItem.doFontLabel(pos, "LOCAL ACCOUNTS ::", GuiData.font, Color.Gray, width, 22f);
                    pos.Y += 22f;
                }
                for (var index = 0; index < SaveFileManager.Accounts.Count; ++index)
                {
                    if (Button.doButton(2870300 + index + index*12, (int) pos.X, (int) pos.Y, width, 18,
                        SaveFileManager.Accounts[index].Username, Color.Black))
                    {
                        Answers = new List<string>(new string[2]
                        {
                            SaveFileManager.Accounts[index].Username,
                            SaveFileManager.Accounts[index].Password
                        });
                        promptIndex = 2;
                        TextBox.BoxWasActivated = true;
                        IsReady = true;
                        break;
                    }
                    pos.Y += 22f;
                }
            }
            if (TextBox.BoxWasActivated)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(currentPrompt);
                stringBuilder.Append(" ");
                var str1 = terminalString;
                if (InPasswordMode)
                {
                    var str2 = "";
                    for (var index = 0; index < str1.Length; ++index)
                        str2 += "*";
                    str1 = str2;
                }
                stringBuilder.Append(str1);
                History.Add(stringBuilder.ToString());
                Advance(terminalString);
                terminalString = "";
                TextBox.cursorPosition = 0;
                TextBox.BoxWasActivated = false;
            }
            var y1 = dest.Y + dest.Height + 12;
            GuiData.spriteBatch.Draw(Utils.white, new Rectangle(dest.X, y1, dest.Width/2, 1), Utils.SlightlyDarkGray);
            var y2 = y1 + 8;
            if (IsReady)
            {
                if (!GuiData.getKeyboadState().IsKeyDown(Keys.Enter))
                    CanReturnEnter = true;
                if ((!IsNewAccountMode ||
                     Button.doButton(16392804, dest.X, y2, dest.Width/3, 28, "CONFIRM", Color.White) ||
                     CanReturnEnter && Utils.keyPressed(GuiData.lastInput, Keys.Enter, new PlayerIndex?())) &&
                    !PreventAdvancing)
                {
                    if (IsNewAccountMode)
                    {
                        if (Answers.Count < 3)
                        {
                            ResetForNewAccount();
                        }
                        else
                        {
                            var str1 = Answers[0];
                            var str2 = Answers[1];
                            TextBox.MaskingText = false;
                            if (StartNewGameForUsernameAndPass != null)
                                StartNewGameForUsernameAndPass(str1, str2);
                        }
                    }
                    else
                    {
                        TextBox.MaskingText = false;
                        if (LoadGameForUserFileAndUsername != null)
                            LoadGameForUserFileAndUsername(userPathCache, Answers[0]);
                    }
                    PreventAdvancing = true;
                }
                y2 += 36;
            }
            if (Button.doButton(16392806, dest.X, y2, dest.Width/3, 22, "CANCEL", CancelColor) && RequestGoBack != null)
            {
                TextBox.MaskingText = false;
                RequestGoBack();
            }
            var num2 = GuiData.ActiveFontConfig.tinyFontCharHeight + 8f;
            var position = new Vector2(dest.X, dest.Y + dest.Height - 20 - num2);
            for (var index = History.Count - 1; index >= 0; --index)
            {
                sb.DrawString(GuiData.UISmallfont, History[index], position, Color.White);
                position.Y -= num2;
            }
        }
    }
}
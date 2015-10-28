using System;
using System.Collections.Generic;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    internal class AdvancedTutorial : ExeModule
    {
        private static List<string> commandSequence;
        private static List<string[]> altCommandequence;
        private static List<string> feedbackSequence;
        private readonly SoundEffect advanceFlash;
        private NodeBounceEffect bounceEffect;
        private float flashTimer;

        private readonly string[] hintButtonDelimiter = new string[1]
        {
            "|"
        };

        private string[] hintButtonText;
        private float hintTextFadeTimer;
        private string lastCommand;
        private string[] renderText;
        private readonly SoundEffect startSound;
        private int state;
        private List<Action> stepCompletionSequence;

        public AdvancedTutorial(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            state = 0;
            lastCommand = "";
            ramCost = 500;
            IdentifierName = "Tutorial v16.2";
            flashTimer = 1f;
            startSound = os.content.Load<SoundEffect>("SFX/DoomShock");
            advanceFlash = os.content.Load<SoundEffect>("SFX/BrightFlash");
            commandSequence = null;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            bounceEffect = new NodeBounceEffect
            {
                NodeHitDelay = 0.02f,
                TimeBetweenBounces = 0.2f
            };
            var strArray =
                Utils.readEntireFile("Content/Post/AdvancedTutorialData.txt").Replace("\r\n", "\n").Split(new string[1]
                {
                    "\n\n%&%&%&%\n"
                }, StringSplitOptions.None);
            if (commandSequence == null)
            {
                altCommandequence = new List<string[]>();
                for (var index = 0; index < 15; ++index)
                    altCommandequence.Add(new string[0]);
                commandSequence = new List<string>();
                commandSequence.Add("&#*@(&#@(&#@&)@&#)(@&)@#");
                commandSequence.Add("connect " + os.thisComputer.ip);
                commandSequence.Add("scan");
                commandSequence.Add("dc");
                altCommandequence[3] = new string[1]
                {
                    "disconnect"
                };
                commandSequence.Add("connect");
                commandSequence.Add("probe");
                altCommandequence[5] = new string[1]
                {
                    "nmap"
                };
                commandSequence.Add("porthack");
                commandSequence.Add("scan");
                commandSequence.Add("ls");
                altCommandequence[8] = new string[1]
                {
                    "dir"
                };
                commandSequence.Add("cd bin");
                altCommandequence[9] = new string[3]
                {
                    "cd ../bin",
                    "cd ../../bin",
                    "cd /bin"
                };
                commandSequence.Add("cat config.txt");
                altCommandequence[10] = new string[1]
                {
                    "less config.txt"
                };
                commandSequence.Add("cd ..");
                altCommandequence[11] = new string[2]
                {
                    "cd..",
                    "cd /"
                };
                commandSequence.Add("cd log");
                altCommandequence[12] = new string[3]
                {
                    "cd ../log",
                    "cd ../../log",
                    "cd /log"
                };
                commandSequence.Add("rm *");
                commandSequence.Add("dc");
                altCommandequence[14] = new string[1]
                {
                    "disconnect"
                };
            }
            if (feedbackSequence == null)
            {
                feedbackSequence = new List<string>();
                feedbackSequence.Add(strArray[0]);
                feedbackSequence.Add(strArray[1]);
                feedbackSequence.Add(strArray[2]);
                feedbackSequence.Add(strArray[3]);
                feedbackSequence.Add(strArray[4]);
                feedbackSequence.Add(strArray[5]);
                feedbackSequence.Add(strArray[6]);
                feedbackSequence.Add(strArray[7]);
                feedbackSequence.Add(strArray[8]);
                feedbackSequence.Add(strArray[9]);
                feedbackSequence.Add(strArray[10]);
                feedbackSequence.Add(strArray[11]);
                feedbackSequence.Add(strArray[12]);
                feedbackSequence.Add(strArray[13]);
                feedbackSequence.Add(strArray[14]);
                feedbackSequence.Add(strArray[15]);
            }
            stepCompletionSequence = new List<Action>();
            for (var index = 0; index < feedbackSequence.Count; ++index)
                stepCompletionSequence.Add(null);
            stepCompletionSequence[1] = () =>
            {
                os.netMap.visible = true;
                os.netMap.inputLocked = false;
            };
            stepCompletionSequence[2] = () =>
            {
                os.display.visible = true;
                os.display.inputLocked = false;
            };
            stepCompletionSequence[3] = () => os.netMap.inputLocked = true;
            stepCompletionSequence[4] = () => os.netMap.inputLocked = false;
            stepCompletionSequence[5] = () =>
            {
                os.display.inputLocked = true;
                os.ram.inputLocked = true;
                os.netMap.inputLocked = true;
                os.terminal.visible = true;
                os.terminal.inputLocked = false;
                os.terminal.clearCurrentLine();
            };
            stepCompletionSequence[8] = () =>
            {
                os.display.inputLocked = false;
                os.ram.inputLocked = false;
            };
            state = 0;
            getRenderText();
        }

        public override void Update(float t)
        {
            base.Update(t);
            var lastRunCommand = os.terminal.getLastRunCommand();
            if (lastCommand != lastRunCommand)
            {
                lastCommand = lastRunCommand;
                parseCommand();
            }
            else if (GuiData.getKeyboadState().IsKeyDown(Keys.F8))
                lastCommand = "";
            flashTimer -= t;
            flashTimer = Math.Max(flashTimer, 0.0f);
            bounceEffect.Update(t, nodePos =>
            {
                bounceEffect.NodeHitDelay = Math.Max(0.0f, Utils.randm(0.5f) - 0.2f);
                bounceEffect.TimeBetweenBounces = 0.15f + Utils.randm(0.7f);
            });
            hintTextFadeTimer = Math.Max(0.0f, hintTextFadeTimer - t);
        }

        public void parseCommand()
        {
            try
            {
                if (state < commandSequence.Count)
                {
                    var flag = lastCommand.ToLower().StartsWith(commandSequence[state]);
                    if (!flag)
                    {
                        for (var index = 0; index < altCommandequence[state].Length; ++index)
                            flag |= lastCommand.ToLower().StartsWith(altCommandequence[state][index]);
                    }
                    if (!flag)
                        return;
                    advanceState();
                }
                else
                    lastCommand = null;
            }
            catch (Exception ex)
            {
                lastCommand = "";
            }
        }

        private void advanceState()
        {
            ++state;
            getRenderText();
            if (state > 0)
                printCurrentCommandToTerminal();
            flashTimer = 1f;
            if (stepCompletionSequence[state] != null)
                stepCompletionSequence[state]();
            if (state <= 1)
                return;
            advanceFlash.Play(0.6f, 1f, 1f);
        }

        public void printCurrentCommandToTerminal()
        {
            os.terminal.writeLine("\n--------------------------------------------------");
            os.terminal.writeLine(" ");
            for (var index = 0; index < renderText.Length; ++index)
            {
                if (!renderText[index].Contains("<#") && !renderText[index].Contains("#>"))
                    os.terminal.writeLine(renderText[index]);
            }
            os.terminal.writeLine(" ");
            os.terminal.writeLine("--------------------------------------------------\n");
        }

        public void getRenderText()
        {
            var data = feedbackSequence[state];
            var chArray = new char[1]
            {
                '\n'
            };
            var strArray = data.Split(hintButtonDelimiter, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length > 1)
            {
                hintButtonText = DisplayModule.cleanSplitForWidth(strArray[1], 178).Split(chArray);
                data = strArray[0];
                hintTextFadeTimer = 0.0f;
            }
            else
                hintButtonText = null;
            renderText = Utils.SuperSmartTwimForWidth(data, bounds.Width - 10, GuiData.tinyfont).Split(chArray);
        }

        public override void Killed()
        {
            base.Killed();
            if (!os.multiplayer && os.initShowsTutorial)
            {
                os.currentMission.sendEmail(os);
                os.Flags.AddFlag("TutorialComplete");
                os.initShowsTutorial = false;
            }
            os.mailicon.isEnabled = true;
            os.terminal.visible = true;
            os.terminal.inputLocked = false;
            os.netMap.visible = true;
            os.netMap.inputLocked = false;
            os.ram.visible = true;
            os.ram.inputLocked = false;
            os.display.visible = true;
            os.display.inputLocked = false;
            if (state >= feedbackSequence.Count - 1)
                return;
            AchievementsManager.Unlock("kill_tutorial", false);
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            var rectangle = this.bounds;
            ++rectangle.X;
            ++rectangle.Y;
            rectangle.Width -= 2;
            rectangle.Height -= 2;
            rectangle.Height -= PANEL_HEIGHT;
            rectangle.Y += PANEL_HEIGHT;
            PatternDrawer.draw(rectangle, 1f, os.highlightColor*0.2f, os.highlightColor, spriteBatch);
            rectangle.X += 2;
            rectangle.Y += 2;
            rectangle.Width -= 4;
            rectangle.Height -= 4;
            spriteBatch.Draw(Utils.white, rectangle, os.darkBackgroundColor*0.99f);
            drawTarget("app:");
            var height1 = 260;
            var bounds = new Rectangle(this.bounds.X + 1, this.bounds.Y + this.bounds.Height - height1,
                this.bounds.Width - 2, height1);
            bounceEffect.Draw(spriteBatch, bounds, Utils.AdditivizeColor(os.highlightColor)*0.35f,
                Utils.AddativeWhite*0.5f);
            var height2 = 210;
            var destinationRectangle1 = new Rectangle(bounds.X, bounds.Y + (height1 - height2), bounds.Width, height2);
            spriteBatch.Draw(Utils.gradient, destinationRectangle1, new Rectangle?(), os.darkBackgroundColor, 0.0f,
                Vector2.Zero, SpriteEffects.FlipVertically, 0.7f);
            var destinationRectangle2 = destinationRectangle1;
            destinationRectangle2.Y = bounds.Y;
            destinationRectangle2.Height = height1 - height2;
            spriteBatch.Draw(Utils.white, destinationRectangle2, os.darkBackgroundColor);
            spriteBatch.Draw(Utils.white, rectangle, os.highlightColor*flashTimer);
            spriteBatch.DrawString(GuiData.font, Utils.FlipRandomChars("Tutorial", 0.024),
                new Vector2(this.bounds.X + 5, this.bounds.Y + 22), os.subtleTextColor*0.6f);
            spriteBatch.DrawString(GuiData.font, "Tutorial", new Vector2(this.bounds.X + 5, this.bounds.Y + 22),
                os.subtleTextColor);
            var charHeight = GuiData.ActiveFontConfig.tinyFontCharHeight + 1f;
            var dpos = RenderText(renderText, new Vector2(this.bounds.X + 5, this.bounds.Y + 67), charHeight, 1f);
            dpos.Y += charHeight;
            if (state == 0)
            {
                if (
                    !Button.doButton(2933201, this.bounds.X + 10, (int) (dpos.Y + 20.0), this.bounds.Width - 20, 30,
                        "Continue", new Color?()) &&
                    (os.terminal.visible || !Utils.keyPressed(GuiData.lastInput, Keys.Enter, new PlayerIndex?())))
                    return;
                advanceState();
                startSound.Play(1f, 0.0f, 0.0f);
                startSound.Play(1f, 0.0f, 0.0f);
            }
            else
            {
                if (hintButtonText == null)
                    return;
                if (Button.doButton(2933202, this.bounds.X + 10, (int) (dpos.Y + 6.0), this.bounds.Width - 20, 20,
                    "Hint", new Color?()))
                    hintTextFadeTimer = 9f;
                dpos.Y += 30f;
                dpos.X += 10f;
                var opacityMod = Math.Min(1f, hintTextFadeTimer/3f);
                RenderText(hintButtonText, dpos, charHeight, opacityMod);
            }
        }

        private Vector2 RenderText(string[] stringData, Vector2 dpos, float charHeight, float opacityMod = 1f)
        {
            var flag = false;
            for (var index = 0; index < stringData.Length; ++index)
            {
                var text = stringData[index];
                if (text.Length > 0)
                {
                    if (text.Trim() == "<#")
                        flag = true;
                    else if (text.Trim() == "#>")
                    {
                        flag = false;
                    }
                    else
                    {
                        spriteBatch.DrawString(GuiData.tinyfont, text, dpos,
                            (flag ? os.highlightColor : Color.White)*opacityMod);
                        dpos.Y += charHeight;
                    }
                }
            }
            return dpos;
        }

        private Vector2 RenderTextOld(string[] stringData, Vector2 dpos, float charHeight, float opacityMod = 1f)
        {
            for (var index = 0; index < stringData.Length; ++index)
            {
                var text = stringData[index];
                if (text.Length > 0)
                {
                    var flag = false;
                    if (text[0] == 35)
                    {
                        text = text.Substring(1, text.Length - 2);
                        flag = true;
                    }
                    spriteBatch.DrawString(GuiData.tinyfont, text, dpos,
                        (flag ? os.highlightColor : Color.White)*opacityMod);
                    dpos.Y += charHeight;
                }
            }
            return dpos;
        }
    }
}
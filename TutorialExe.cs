// Decompiled with JetBrains decompiler
// Type: Hacknet.TutorialExe
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class TutorialExe : ExeModule
    {
        public static bool advanced = false;
        private static List<string> commandSequence;
        private static List<string> feedbackSequence;
        private float flashTimer;
        private string lastCommand;
        private string[] renderText;
        private int state;

        public TutorialExe(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            state = 0;
            lastCommand = "";
            ramCost = 500;
            IdentifierName = "Tutorial";
            flashTimer = 1f;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (advanced)
            {
                if (commandSequence == null)
                {
                    commandSequence = new List<string>();
                    commandSequence.Add("connect " + os.thisComputer.ip);
                    commandSequence.Add("scan");
                    commandSequence.Add("connect");
                    commandSequence.Add("probe");
                    commandSequence.Add("exe porthack");
                    commandSequence.Add("scan");
                    commandSequence.Add("ls");
                    commandSequence.Add("cd bin");
                    commandSequence.Add("ls");
                    commandSequence.Add("scp sshcrack.exe");
                    commandSequence.Add("cd ..");
                    commandSequence.Add("cd log");
                    commandSequence.Add("rm *");
                    commandSequence.Add("dc");
                }
                if (feedbackSequence == null)
                {
                    feedbackSequence = new List<string>();
                    feedbackSequence.Add(
                        "#As of right now you are#\n#at risk!#\nLearn as quickly as possible. Connect to a computer by typing \"connect [IP]\" - or by clicking on a node on the network map. Connect to your own server on the map now by clicking the green circle.\n \n \nIf at any point you get stuck, a command list can be found by typing \"help\".");
                    feedbackSequence.Add(
                        "Good work.\nThe first thing to do on any system is scan it for adjacent nodes. This will reveal more computers on your map that you can use. Scan this computer now by typing \n#scan#\n and run it by pressing enter.");
                    feedbackSequence.Add(
                        "It's time for you to connect to an outside computer. Be aware that attempting to compromise the security of another's computer is Illegal under the U.S.C. Act 1030-18. Proceed at your own risk by using the command \n#connect [TARGET IP]#\n or by clicking a blue node on the network map.");
                    feedbackSequence.Add(
                        "A computer's security system and open ports can be analysed using the \n#probe#\n (nmap) command. analyse the computer you are currently connected to.");
                    feedbackSequence.Add(
                        "Here you can see the active ports, active security, and the number of open ports required to sucsesfully crack this machine using PortHack.\nThis Machine has no active security and requires no open ports to crack. If you are prepared to, it is possible to crack this comput er using the program \n#PortHack#\n. To run a program you own on the machine you are connected to, use the command \n#exe [PROGRAM NAME]#\n. A program name will never require \".exe on the end\"");
                    feedbackSequence.Add(
                        "Congratulations. You have taken control of an external system and are now it's administrator. You can do whatever you like with it, however you should start by using \n#scan#\n to locate additional computers");
                    feedbackSequence.Add(
                        "Next, you should investigate the filesystem. Use the \n#ls#\n command to view the files and folders in your current directory.");
                    feedbackSequence.Add(
                        "Navigate to the \n#bin#\n (Binaries) folder to search for useful executables usind the command \n#cd [FOLDER NAME]#\n");
                    feedbackSequence.Add("List the files in this folder using \n#ls#\n");
                    feedbackSequence.Add(
                        "The file \"\n#SSHcrack.exe#\n\" is a program used to open a locked port running SSH on an external server. If you had it on your local computer, you could run it by using \"exe SSHCrack [SSH PORT NUMBER]\" - but you need it first. Download files from an external machine by using \n#scp [FILE NAME]#\n\n NOTE: Downloading a file you do not own is highly illegal and punishable by prison sentence.");
                    feedbackSequence.Add(
                        "That's sure to come in handy.\n Now to clear your tracks before you leave. Move up a folder in the directory using \n#cd ..#\n");
                    feedbackSequence.Add("Move to the log folder.\n#cd [FOLDER NAME]#\n");
                    feedbackSequence.Add(
                        "You can delete a file using the command \n#rm [FILENAME]#\n , however you can delete ALL files in the current directory with the command \n#rm *#\n");
                    feedbackSequence.Add("Excellent work.\n\nTo Disconnect from a computer, use the \n#dc#\n command");
                    feedbackSequence.Add(
                        "Congratulations, You have completed the guided section of this tutorial.\n\nTo finish it, you must locate the Process ID of this tutorial program and kill it. the \n#help#\n command will give you a complete command list at any time. Use \n#exe Tutorial#\n to run this program again.");
                }
            }
            else
            {
                if (commandSequence == null)
                {
                    commandSequence = new List<string>();
                    commandSequence.Add("connect " + os.thisComputer.ip);
                    commandSequence.Add("scan");
                    commandSequence.Add("connect");
                    commandSequence.Add("probe");
                    commandSequence.Add("exe porthack");
                    commandSequence.Add("scan");
                    commandSequence.Add("ls");
                    commandSequence.Add("cd log");
                    commandSequence.Add("rm *");
                    commandSequence.Add("dc");
                }
                if (feedbackSequence == null)
                {
                    feedbackSequence = new List<string>();
                    feedbackSequence.Add(
                        "#As of right now you are#\n#at risk!#\nLearn as quickly as possible. Connect to a computer by typing \"connect [IP]\" - or by clicking on a node on the network map. Connect to your own server on the map now by clicking the green circle.\n \n \nIf at any point you get stuck, a command list can be found by typing \"help\".");
                    feedbackSequence.Add(
                        "Good work.\nThe first thing to do on any system is scan it for adjacent nodes. This will reveal more computers on your map that you can use. Scan this computer now by typing \n#scan#\n and run it by pressing enter.");
                    feedbackSequence.Add(
                        "It's time for you to connect to an outside computer. Be aware that attempting to compromise the security of another's computer is Illegal under the U.S.C. Act 1030-18. Proceed at your own risk by using the command \n#connect [TARGET IP]#\n or by clicking a blue node on the network map.");
                    feedbackSequence.Add(
                        "A computer's security system and open ports can be analysed using the \n#probe#\n command. Analyse the computer you are currently connected to.");
                    feedbackSequence.Add(
                        "Here you can see the active ports, active security, and the number of open ports required to successfully crack this machine using PortHack.\nThis Machine has no active security and requires no open ports to crack. If you are prepared to, it is possible to crack this computer using the program \n#PortHack#\n. To run a program you own on the machine you are connected to, use the command \n#exe \"PROGRAM NAME\"#\n. A program name will never require \".exe on the end\"");
                    feedbackSequence.Add(
                        "Congratulations. You have taken control of an external system and are now it's \nadministrator. You can do whatever you like with it, however you should start by using \n#scan#\n to locate additional computers");
                    feedbackSequence.Add(
                        "Next, you should investigate the filesystem. Use the \n#ls#\n command to view the files and folders in your current directory.\nFeel free to play around and investigate anything you like. This is your node now.");
                    feedbackSequence.Add(
                        "Move to the log folder when you are ready.\n#cd [FOLDER NAME]#\nThe Up and Down arrow keys let you quickly access previous commands, and the tab key will autocomplete file and program names in the terminal.");
                    feedbackSequence.Add(
                        "You can delete a file using the command \n#rm [FILENAME]#\n , however you can delete ALL files in the current directory with the command \n#rm *#\n");
                    feedbackSequence.Add("Excellent work.\n\nTo Disconnect from a computer, use the \n#dc#\n command");
                    feedbackSequence.Add(
                        "Congratulations, You have completed the guided section of this tutorial.\n\nTo finish it, you must locate the Process ID of this tutorial program and kill it. the \n#help#\n command will give you a complete command list at any time. Use \n#exe Tutorial#\n to run this program again.\nOnce you complete this, check your email with the icon in the top right...");
                }
            }
            state = 0;
            getRenderText();
            printCurrentCommandToTerminal();
        }

        public override void Update(float t)
        {
            base.Update(t);
            var lastRunCommand = os.terminal.getLastRunCommand();
            if (!lastCommand.Equals(lastRunCommand))
            {
                lastCommand = lastRunCommand;
                parseCommand();
            }
            flashTimer -= t;
            flashTimer = Math.Max(flashTimer, 0.0f);
        }

        public void parseCommand()
        {
            if (state >= commandSequence.Count || !lastCommand.ToLower().StartsWith(commandSequence[state]))
                return;
            ++state;
            getRenderText();
            printCurrentCommandToTerminal();
            flashTimer = 1f;
        }

        public void printCurrentCommandToTerminal()
        {
            os.terminal.writeLine("--------------------------------------------------");
            os.terminal.writeLine(" ");
            for (var index = 0; index < renderText.Length; ++index)
                os.terminal.writeLine(renderText[index]);
            os.terminal.writeLine(" ");
            os.terminal.writeLine("--------------------------------------------------");
        }

        public void getRenderText()
        {
            var chArray = new char[1]
            {
                '\n'
            };
            renderText = DisplayModule.cleanSplitForWidth(feedbackSequence[state], 200).Split(chArray);
        }

        public override void Killed()
        {
            base.Killed();
            if (os.multiplayer || !os.initShowsTutorial)
                return;
            os.currentMission.sendEmail(os);
            os.initShowsTutorial = false;
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            var rectangle = bounds;
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
            spriteBatch.Draw(Utils.white, rectangle, os.highlightColor*flashTimer);
            drawTarget("");
            spriteBatch.DrawString(GuiData.font, "Tutorial", new Vector2(bounds.X + 3, bounds.Y + 22),
                os.subtleTextColor);
            var num = 12f;
            var position = new Vector2(bounds.X + 5, bounds.Y + 57);
            for (var index = 0; index < renderText.Length; ++index)
            {
                var text = renderText[index];
                if (text.Length > 0)
                {
                    var flag = false;
                    if (text[0] == 35)
                    {
                        text = text.Substring(1, text.Length - 2);
                        flag = true;
                    }
                    spriteBatch.DrawString(GuiData.tinyfont, text, position, flag ? os.highlightColor : Color.White);
                    position.Y += num;
                }
            }
            position.Y += num;
        }
    }
}
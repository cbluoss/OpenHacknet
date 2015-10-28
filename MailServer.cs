using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MailServer : Daemon
    {
        public const int TYPE = 0;
        public const int SENDER = 1;
        public const int SUBJECT = 2;
        public const int BODY = 3;
        public const int ATTACHMENT = 4;
        public const int UNREAD = 0;
        public const int READ = 1;
        public static bool shouldGenerateJunk = true;
        private static readonly string emailSplitDelimiter = "@*&^#%@)_!_)*#^@!&*)(#^&\n";

        private static readonly string[] emailSplitDelims = new string[1]
        {
            emailSplitDelimiter
        };

        private static readonly string[] spaceDelim = new string[1]
        {
            "#%#"
        };

        private static SoundEffect buttonSound;
        private Folder accounts;
        private List<int> accountsPath;
        private bool addingNewReplyString;
        public Texture2D corner;
        private string[] emailData;
        private readonly List<string> emailReplyStrings = new List<string>();
        public Color evenLine;
        private int inboxPage;
        private bool missionIncompleteReply;
        public Color oddLine;
        public Texture2D panel;
        private Rectangle panelRect;
        private readonly List<MailResponder> responders;
        private Folder root;
        private List<int> rootPath;
        private ScrollableSectionedPanel sectionedPanel;
        private FileEntry selectedEmail;
        public Color senderDarkeningColor;
        public Color seperatorLineColor;
        public Action setupComplete;
        private int state;
        public Color textColor;
        private Color themeColor = new Color(125, 5, 6);
        private int totalPagesDetected = -1;
        public Texture2D unopenedIcon;
        private UserDetail user;
        private Folder userFolder;

        public MailServer(Computer c, string name, OS os)
            : base(c, name, os)
        {
            state = 0;
            panel = os.content.Load<Texture2D>("Panel");
            corner = os.content.Load<Texture2D>("Corner");
            unopenedIcon = os.content.Load<Texture2D>("UnopenedMail");
            buttonSound = os.content.Load<SoundEffect>("SFX/Bip");
            panelRect = new Rectangle();
            evenLine = new Color(80, 81, 83);
            oddLine = new Color(58, 58, 58);
            senderDarkeningColor = new Color(0, 0, 0, 100);
            seperatorLineColor = Color.Transparent;
            textColor = Color.White;
            responders = new List<MailResponder>();
        }

        public override void initFiles()
        {
            base.initFiles();
            initFilesystem();
            for (var index = 0; index < 10; ++index)
                comp.users.Add(new UserDetail(UsernameGenerator.getName()));
            for (var index = 0; index < comp.users.Count; ++index)
            {
                var userDetail = comp.users[index];
                if (userDetail.type == 1 || userDetail.type == 0 || userDetail.type == 2)
                {
                    var folder = new Folder(userDetail.name);
                    folder.files.Add(new FileEntry("Username: " + userDetail.name + "\nPassword: " + userDetail.pass,
                        "AccountInfo"));
                    var f = new Folder("inbox");
                    addJunkEmails(f);
                    folder.folders.Add(f);
                    folder.folders.Add(new Folder("sent"));
                    accounts.folders.Add(folder);
                }
            }
            if (setupComplete == null)
                return;
            setupComplete();
        }

        public override void loadInit()
        {
            base.loadInit();
            root = comp.files.root.searchForFolder("mail");
            accounts = root.searchForFolder("accounts");
        }

        public void addJunkEmails(Folder f)
        {
            if (!shouldGenerateJunk)
                return;
            var num = Utils.random.Next(10);
            for (var index = 0; index < num; ++index)
                f.files.Add(new FileEntry(generateEmail("Re: Junk", BoatMail.JunkEmail, "admin@" + comp.name),
                    "Re:_Junk#" + OS.currentElapsedTime));
        }

        public void initFilesystem()
        {
            rootPath = new List<int>();
            accountsPath = new List<int>();
            root = comp.files.root.searchForFolder("mail");
            if (root == null)
            {
                root = new Folder("mail");
                comp.files.root.folders.Add(root);
            }
            rootPath.Add(comp.files.root.folders.IndexOf(root));
            accountsPath.Add(comp.files.root.folders.IndexOf(root));
            accounts = new Folder("accounts");
            accountsPath.Add(0);
            root.folders.Add(accounts);
        }

        public void setThemeColor(Color newThemeColor)
        {
            themeColor = newThemeColor;
        }

        public void addResponder(MailResponder resp)
        {
            responders.Add(resp);
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            state = 0;
            inboxPage = 0;
            totalPagesDetected = -1;
        }

        public void viewInbox(UserDetail newUser)
        {
            userFolder = null;
            state = 3;
            for (var index1 = 0; index1 < comp.users.Count; ++index1)
            {
                if (comp.users[index1].name.Equals(newUser.name))
                {
                    user = comp.users[index1];
                    for (var index2 = 0; index2 < accounts.folders.Count; ++index2)
                    {
                        if (accounts.folders[index2].name.Equals(user.name))
                        {
                            userFolder = accounts.folders[index2];
                            break;
                        }
                    }
                    break;
                }
            }
            comp.currentUser = user;
            var folder = userFolder;
        }

        public void addMail(string mail, string userTo)
        {
            for (var index = 0; index < responders.Count; ++index)
                responders[index].mailReceived(mail, userTo);
            Folder folder = null;
            for (var index = 0; index < accounts.folders.Count; ++index)
            {
                if (accounts.folders[index].name.Equals(userTo))
                {
                    folder = accounts.folders[index].folders[0];
                    break;
                }
            }
            if (folder == null)
                return;
            folder.files.Insert(0, new FileEntry(mail, getSubject(mail)));
        }

        public bool MailWithSubjectExists(string userName, string mailSubject)
        {
            for (var index1 = 0; index1 < accounts.folders.Count; ++index1)
            {
                if (accounts.folders[index1].name.Equals(userName))
                {
                    var folder = accounts.folders[index1];
                    for (var index2 = 0; index2 < folder.files.Count; ++index2)
                    {
                        if (folder.files[index2].data.Split(emailSplitDelims, StringSplitOptions.None)[2].ToLower() ==
                            mailSubject.ToLower())
                            return true;
                    }
                    break;
                }
            }
            return false;
        }

        public override void userAdded(string name, string pass, byte type)
        {
            base.userAdded(name, pass, type);
            if (type != 0 && type != 1 && type != 2)
                return;
            var folder = new Folder(name);
            folder.files.Add(new FileEntry("Username: " + name + "\nPassword: " + pass, "AccountInfo"));
            var f = new Folder("inbox");
            addJunkEmails(f);
            folder.folders.Add(f);
            folder.folders.Add(new Folder("sent"));
            accounts.folders.Add(folder);
        }

        public override string getSaveString()
        {
            return "<MailServer name=\"" + name + "\" color=\"" + Utils.convertColorToParseableString(themeColor) +
                   "\"/>";
        }

        public virtual void drawTopBar(Rectangle bounds, SpriteBatch sb)
        {
            panelRect = new Rectangle(bounds.X, bounds.Y, bounds.Width - corner.Width, panel.Height);
            sb.Draw(panel, panelRect, themeColor);
            sb.Draw(corner, new Vector2(bounds.X + bounds.Width - corner.Width, bounds.Y), themeColor);
        }

        public virtual void drawBackingGradient(Rectangle boundsTo, SpriteBatch sb)
        {
            var destinationRectangle = boundsTo;
            ++destinationRectangle.X;
            destinationRectangle.Width -= 2;
            destinationRectangle.Height -= 2;
            sb.Draw(Utils.gradient, destinationRectangle, Color.Black);
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            var position = new Vector2(bounds.X + 8, bounds.Y + 120);
            drawTopBar(bounds, sb);
            switch (state)
            {
                case 0:
                    sb.DrawString(GuiData.font, name + " Mail Server", position, textColor);
                    position.Y += 80f;
                    position.Y += 35f;
                    if (Button.doButton(800002, (int) position.X, (int) position.Y, 300, 40, "Login", themeColor))
                    {
                        state = 2;
                        os.displayCache = "";
                        os.execute("login");
                        do
                            ; while (os.displayCache.Equals(""));
                        os.display.command = name;
                    }
                    position.Y += 45f;
                    if (!Button.doButton(800003, (int) position.X, (int) position.Y, 300, 30, "Exit", themeColor))
                        break;
                    os.display.command = "connect";
                    break;
                case 1:
                    sb.DrawString(GuiData.font, "Test", position, textColor);
                    position.Y += 80f;
                    if (!Button.doButton(800001, (int) position.X, (int) position.Y, 300, 60, "testing", new Color?()))
                        break;
                    state = 0;
                    break;
                case 2:
                    drawBackingGradient(bounds, sb);
                    sb.DrawString(GuiData.font, "login", position, textColor);
                    position.Y += 80f;
                    doLoginDisplay(bounds, sb);
                    break;
                case 3:
                    doInboxDisplay(bounds, sb);
                    break;
                case 4:
                    doEmailViewerDisplay(bounds, sb);
                    break;
                case 5:
                    doRespondDisplay(bounds, sb);
                    break;
            }
        }

        private int GetRenderTextHeight()
        {
            return (int) (GuiData.ActiveFontConfig.tinyFontCharHeight + 2.0);
        }

        private int DrawMailMessageText(Rectangle textBounds, SpriteBatch sb, string[] text)
        {
            if (sectionedPanel == null || sectionedPanel.PanelHeight != GetRenderTextHeight())
                sectionedPanel = new ScrollableSectionedPanel(GetRenderTextHeight(), sb.GraphicsDevice);
            sectionedPanel.NumberOfPanels = text.Length;
            var itemsDrawn = 0;
            sectionedPanel.Draw((index, dest, spBatch) =>
            {
                spBatch.DrawString(GuiData.tinyfont, text[index], new Vector2(dest.X, dest.Y), Color.White, 0.0f,
                    Vector2.Zero, 1f, SpriteEffects.None, 0.8f);
                ++itemsDrawn;
            }, sb, textBounds);
            if (sectionedPanel.NumberOfPanels*sectionedPanel.PanelHeight < textBounds.Height)
                return sectionedPanel.NumberOfPanels*sectionedPanel.PanelHeight;
            return textBounds.Height;
        }

        public void doEmailViewerDisplay(Rectangle bounds, SpriteBatch sb)
        {
            var vector2 = new Vector2(bounds.X + 2, bounds.Y + 20);
            if (Button.doButton(800007, (int) vector2.X, (int) vector2.Y, bounds.Width - 20 - corner.Width, 30,
                "Return to Inbox", os.darkBackgroundColor))
                state = 3;
            vector2.Y += 35f;
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = bounds.X + 1;
            destinationRectangle.Y = (int) vector2.Y;
            destinationRectangle.Width = bounds.Width - 2;
            destinationRectangle.Height = 38;
            sb.Draw(Utils.white, destinationRectangle, textColor);
            vector2.Y += 3f;
            sb.DrawString(GuiData.UITinyfont, "<" + emailData[1] + ">", vector2, Color.Black);
            vector2.Y += 18f;
            sb.DrawString(GuiData.UITinyfont, "Subject: " + emailData[2], vector2, Color.Black);
            vector2.Y += 25f;
            vector2.X += 20f;
            var num1 = 25;
            var num2 = (emailData.Length - 4 + 1)*num1;
            var textBounds = new Rectangle((int) vector2.X, (int) vector2.Y, bounds.Width - 22,
                (int) (bounds.Height - (vector2.Y - (double) bounds.Y) - num2));
            var str = Utils.SmartTwimForWidth(emailData[3], bounds.Width - 50, GuiData.tinyfont);
            vector2.Y += DrawMailMessageText(textBounds, sb, str.Split(Utils.newlineDelim));
            vector2.Y += num1 - 5;
            var num3 = 0;
            for (var index1 = 4; index1 < emailData.Length; ++index1)
            {
                var strArray = emailData[index1].Split(spaceDelim, StringSplitOptions.RemoveEmptyEntries);
                var flag = true;
                if (strArray.Length >= 1)
                {
                    if (strArray[0].Equals("link"))
                    {
                        var labelSize = TextItem.doMeasuredTinyLabel(vector2,
                            "LINK : " + strArray[1] + "@" + strArray[2], new Color?());
                        var computer = Programs.getComputer(os, strArray[2]);
                        if (!os.netMap.visibleNodes.Contains(os.netMap.nodes.IndexOf(computer)))
                            DrawButtonGlow(vector2, labelSize);
                        if (Button.doButton(800009 + num3, (int) (vector2.X + (double) labelSize.X + 5.0),
                            (int) vector2.Y, 20, 17, "+", new Color?()))
                        {
                            computer.highlightFlashTime = 1f;
                            os.netMap.discoverNode(computer);
                            SFX.addCircle(Programs.getComputer(os, strArray[2]).getScreenSpacePosition(), Color.White,
                                32f);
                            buttonSound.Play();
                        }
                    }
                    else if (strArray[0].Equals("account"))
                    {
                        var labelSize = TextItem.doMeasuredTinyLabel(vector2,
                            "ACCOUNT : " + strArray[1] + " : User=" + strArray[3] + " Pass=" + strArray[4], new Color?());
                        DrawButtonGlow(vector2, labelSize);
                        if (Button.doButton(801009 + num3, (int) (vector2.X + (double) labelSize.X + 5.0),
                            (int) vector2.Y, 20, 17, "+", new Color?()))
                        {
                            var computer = Programs.getComputer(os, strArray[1]);
                            computer.highlightFlashTime = 1f;
                            os.netMap.discoverNode(computer);
                            computer.highlightFlashTime = 1f;
                            SFX.addCircle(computer.getScreenSpacePosition(), Color.White, 32f);
                            for (var index2 = 0; index2 < computer.users.Count; ++index2)
                            {
                                var userDetail = computer.users[index2];
                                if (userDetail.name.Equals(strArray[3]))
                                {
                                    userDetail.known = true;
                                    computer.users[index2] = userDetail;
                                    break;
                                }
                            }
                            buttonSound.Play();
                        }
                    }
                    else if (strArray[0].Equals("note"))
                    {
                        var labelSize = TextItem.doMeasuredTinyLabel(vector2, "NOTE : " + strArray[1], new Color?());
                        if (!NotesExe.NoteExists(strArray[2], os))
                            DrawButtonGlow(vector2, labelSize);
                        if (Button.doButton(800009 + num3, (int) (vector2.X + (double) labelSize.X + 5.0),
                            (int) vector2.Y, 20, 17, "+", new Color?()))
                            NotesExe.AddNoteToOS(strArray[2], os, false);
                    }
                    else
                        flag = false;
                    if (flag)
                    {
                        ++num3;
                        vector2.Y += num1;
                    }
                }
            }
            vector2.Y = bounds.Y + bounds.Height - 35;
            if (!Button.doButton(90200, (int) vector2.X, (int) vector2.Y, 300, 30, "Reply", new Color?()))
                return;
            emailReplyStrings.Clear();
            addingNewReplyString = false;
            missionIncompleteReply = false;
            state = 5;
        }

        private void DrawButtonGlow(Vector2 dpos, Vector2 labelSize)
        {
            var rectangle = new Rectangle((int) (dpos.X + (double) labelSize.X + 5.0), (int) dpos.Y, 20, 17);
            var num1 = Utils.QuadraticOutCurve((float) (1.0 - os.timer%1.0));
            var num2 = 8.5f;
            var destinationRectangle = Utils.InsetRectangle(rectangle, (int) (-1.0*num2*(1.0 - num1)));
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, Utils.AddativeWhite*num1*0.32f);
            GuiData.spriteBatch.Draw(Utils.white, rectangle, Color.Black*0.7f);
        }

        public virtual void doInboxHeader(Rectangle bounds, SpriteBatch sb)
        {
            var vector2_1 = new Vector2(bounds.X + 2, bounds.Y + 20);
            sb.DrawString(GuiData.font, "Logged in as " + user.name, vector2_1, textColor);
            vector2_1.Y += 26f;
            if (Button.doButton(800007, bounds.X + bounds.Width - 85, (int) vector2_1.Y, 80, 30, "Logout", themeColor))
                state = 0;
            if (totalPagesDetected <= 0)
                return;
            var width = 100;
            var height = 20;
            vector2_1.Y = bounds.Y + 91 - (height + 4);
            if (inboxPage < totalPagesDetected &&
                Button.doButton(801008, (int) vector2_1.X, (int) vector2_1.Y, width, height, "<", new Color?()))
                ++inboxPage;
            vector2_1.X += width + 2;
            var vector2_2 = TextItem.doMeasuredFontLabel(vector2_1,
                (inboxPage + 1).ToString() + (object) " / " + (totalPagesDetected + 1), GuiData.tinyfont,
                Utils.AddativeWhite, float.MaxValue, float.MaxValue);
            vector2_1.X += vector2_2.X + 4f;
            if (inboxPage <= 0 ||
                !Button.doButton(801009, (int) vector2_1.X, (int) vector2_1.Y, width, height, ">", new Color?()))
                return;
            --inboxPage;
        }

        public void doInboxDisplay(Rectangle bounds, SpriteBatch sb)
        {
            var height = 24;
            doInboxHeader(bounds, sb);
            var vector2 = new Vector2(bounds.X + 2, bounds.Y + 91);
            var folder = userFolder.folders[0];
            var destinationRectangle1 = new Rectangle(bounds.X + 2, (int) vector2.Y, bounds.Width - 4, height);
            Button.outlineOnly = true;
            var num1 = 0;
            while (destinationRectangle1.Y + destinationRectangle1.Height < bounds.Y + bounds.Height - 2)
            {
                sb.Draw(Utils.white, destinationRectangle1, num1%2 == 0 ? evenLine : oddLine);
                ++num1;
                var num2 = destinationRectangle1.Height;
                destinationRectangle1.Height = 1;
                if (num1 > 1)
                    sb.Draw(Utils.white, destinationRectangle1, seperatorLineColor);
                destinationRectangle1.Height = num2;
                destinationRectangle1.Y += height;
            }
            var destinationRectangle2 = GuiData.tmpRect;
            drawBackingGradient(bounds, sb);
            destinationRectangle2.X = bounds.X + 1;
            destinationRectangle2.Y = (int) vector2.Y - 3;
            destinationRectangle2.Width = bounds.Width - 2;
            destinationRectangle2.Height = 3;
            sb.Draw(Utils.white, destinationRectangle2, themeColor);
            destinationRectangle2.X += destinationRectangle2.Width;
            destinationRectangle2.Width = 3;
            sb.Draw(Utils.white, destinationRectangle2, themeColor);
            destinationRectangle2.X = destinationRectangle1.X;
            destinationRectangle2.Y = (int) vector2.Y;
            destinationRectangle2.Width = 160;
            destinationRectangle2.Height = bounds.Height - (destinationRectangle2.Y - bounds.Y) - 1;
            sb.Draw(Utils.white, destinationRectangle2, senderDarkeningColor);
            destinationRectangle2.X += destinationRectangle2.Width;
            destinationRectangle2.Width = 3;
            sb.Draw(Utils.white, destinationRectangle2, themeColor);
            destinationRectangle2.X = destinationRectangle1.X;
            destinationRectangle2.Y = (int) vector2.Y;
            destinationRectangle2.Width = 160;
            destinationRectangle2.Height = bounds.Height - (destinationRectangle2.Y - bounds.Y) - 1;
            destinationRectangle1.Y = (int) vector2.Y;
            TextItem.DrawShadow = false;
            var val1 = num1;
            var num3 = Math.Max(0, val1*inboxPage - 1);
            totalPagesDetected = (int) (folder.files.Count/(double) val1);
            var num4 = Math.Min(val1, folder.files.Count - num3);
            for (var index = num3; index < num3 + num4; ++index)
            {
                try
                {
                    var strArray = folder.files[index].data.Split(emailSplitDelims, StringSplitOptions.None);
                    var num2 = Convert.ToByte(strArray[0]);
                    var myID = 8100 + index;
                    if (GuiData.hot == myID)
                    {
                        var destinationRectangle3 = new Rectangle(bounds.X + 2, destinationRectangle1.Y + 1,
                            bounds.Width - 4, height - 2);
                        sb.Draw(Utils.white, destinationRectangle3, index%2 == 0 ? Color.White*0.07f : Color.Black*0.2f);
                    }
                    if (Button.doButton(myID, bounds.X + 2, destinationRectangle1.Y + 1, bounds.Width - 4, height - 2,
                        "", Color.Transparent))
                    {
                        state = 4;
                        selectedEmail = folder.files[index];
                        emailData = selectedEmail.data.Split(emailSplitDelims, StringSplitOptions.None);
                        selectedEmail.data = "1" + selectedEmail.data.Substring(1);
                        if (sectionedPanel != null)
                            sectionedPanel.ScrollDown = 0.0f;
                    }
                    if (num2 == 0)
                        sb.Draw(unopenedIcon, vector2 + Vector2.One, themeColor);
                    TextItem.doFontLabel(vector2 + new Vector2(unopenedIcon.Width + 1, 2f), strArray[1],
                        GuiData.tinyfont, textColor, destinationRectangle2.Width - unopenedIcon.Width - 3, height);
                    TextItem.doFontLabel(vector2 + new Vector2(destinationRectangle2.Width + 10, 2f), strArray[2],
                        GuiData.tinyfont, textColor, bounds.Width - destinationRectangle2.Width - 20, height);
                    vector2.Y += height;
                    destinationRectangle1.Y = (int) vector2.Y;
                }
                catch (FormatException ex)
                {
                }
            }
            Button.outlineOnly = false;
        }

        public void doRespondDisplay(Rectangle bounds, SpriteBatch sb)
        {
            var pos = new Vector2(bounds.X + 2, bounds.Y + 20);
            string str = null;
            var width = bounds.Width - 20 - corner.Width;
            if (Button.doButton(800007, (int) pos.X, (int) pos.Y, width, 30, "Return to Inbox", os.darkBackgroundColor))
                state = 3;
            pos.Y += 50f;
            var num1 = 24;
            TextItem.doFontLabel(pos, "Additional Details :", GuiData.smallfont, new Color?(),
                bounds.Width - (float) ((pos.X - (double) bounds.Width)*2.0), float.MaxValue);
            pos.Y += num1;
            for (var index = 0; index < emailReplyStrings.Count; ++index)
            {
                TextItem.doFontLabel(pos + new Vector2(25f, 0.0f), emailReplyStrings[index], GuiData.tinyfont,
                    new Color?(), (float) (bounds.Width - (pos.X - (double) bounds.X)*2.0 - 20.0), float.MaxValue);
                var num2 = Math.Min(GuiData.tinyfont.MeasureString(emailReplyStrings[index]).X,
                    (float) (bounds.Width - (pos.X - (double) bounds.X)*2.0 - 20.0));
                if (Button.doButton(80000 + index*100, (int) (pos.X + (double) num2 + 30.0), (int) pos.Y, 20, 20, "-",
                    new Color?()))
                    emailReplyStrings.RemoveAt(index);
                pos.Y += num1;
            }
            if (addingNewReplyString)
            {
                string data = null;
                var stringCommand = Programs.parseStringFromGetStringCommand(os, out data);
                if (data == null)
                    data = "";
                pos.Y += 5f;
                GuiData.spriteBatch.Draw(Utils.white,
                    new Rectangle(bounds.X + 1, (int) pos.Y, bounds.Width - 2 - bounds.Width/9, 40),
                    os.indentBackgroundColor);
                pos.Y += 10f;
                TextItem.doFontLabel(pos + new Vector2(25f, 0.0f), data, GuiData.tinyfont, new Color?(), float.MaxValue,
                    float.MaxValue);
                var vector2 = GuiData.tinyfont.MeasureString(data);
                vector2.Y = 0.0f;
                if (os.timer%1.0 <= 0.5)
                    GuiData.spriteBatch.Draw(Utils.white,
                        new Rectangle((int) (pos.X + (double) vector2.X + 2.0) + 25, (int) pos.Y, 4, 20), Color.White);
                var num2 = bounds.Width - 1 - bounds.Width/10;
                if (stringCommand ||
                    Button.doButton(8000094, bounds.X + num2 - 4, (int) pos.Y - 10, bounds.Width/9 - 3, 40, "Add",
                        os.highlightColor))
                {
                    if (!stringCommand)
                        os.terminal.executeLine();
                    addingNewReplyString = false;
                    emailReplyStrings.Add(data);
                    str = null;
                }
                else
                    str = data;
            }
            else if (Button.doButton(8000098, (int) (pos.X + 25.0), (int) pos.Y, 20, 20, "+", new Color?()))
            {
                addingNewReplyString = true;
                os.execute("getString Detail");
                os.terminal.executionPreventionIsInteruptable = true;
            }
            pos.Y += 50f;
            if (Button.doButton(800008, (int) pos.X, (int) pos.Y, width, 30, "Send", new Color?()) &&
                os.currentMission != null)
            {
                if (str != null)
                {
                    os.terminal.executeLine();
                    addingNewReplyString = false;
                    if (!string.IsNullOrEmpty(str))
                        emailReplyStrings.Add(str);
                }
                var flag = attemptCompleteMission(os.currentMission);
                if (!flag)
                {
                    for (var index = 0; index < os.branchMissions.Count && !flag; ++index)
                    {
                        flag = attemptCompleteMission(os.branchMissions[index]);
                        if (flag)
                            os.branchMissions.Clear();
                    }
                }
                if (!flag)
                    missionIncompleteReply = true;
            }
            pos.Y += 45f;
            if (OS.DEBUG_COMMANDS && Settings.forceCompleteEnabled &&
                Button.doButton(800009, (int) pos.X, (int) pos.Y, width, 30, "Force Complete", new Color?()))
            {
                if (os.currentMission != null)
                    os.currentMission.finish();
                state = 3;
            }
            pos.Y += 70f;
            if (!missionIncompleteReply)
                return;
            PatternDrawer.draw(new Rectangle(bounds.X + 2, (int) pos.Y, bounds.Width - 4, 128), 1f, os.lockedColor*0.1f,
                os.brightLockedColor, sb, PatternDrawer.errorTile);
            var text = "Mission Incomplete";
            var vector2_1 = GuiData.font.MeasureString(text);
            TextItem.doLabel(new Vector2(bounds.X + bounds.Width/2 - vector2_1.X/2f, pos.Y + 40f), text, new Color?());
        }

        private bool attemptCompleteMission(ActiveMission mission)
        {
            if (!mission.isComplete(emailReplyStrings) ||
                !mission.ShouldIgnoreSenderVerification && !(mission.email.sender == emailData[1]))
                return false;
            mission.finish();
            state = 3;
            return true;
        }

        public void doLoginDisplay(Rectangle bounds, SpriteBatch sb)
        {
            var num1 = bounds.X + 20;
            var num2 = bounds.Y + 100;
            var strArray = os.displayCache.Split(new string[1]
            {
                "#$#$#$$#$&$#$#$#$#"
            }, StringSplitOptions.None);
            var text1 = "";
            var text2 = "";
            var num3 = -1;
            var num4 = 0;
            if (strArray[0].Equals("loginData"))
            {
                text1 = !(strArray[1] != "") ? os.terminal.currentLine : strArray[1];
                if (strArray.Length > 2)
                {
                    num4 = 1;
                    text2 = strArray[2];
                    if (text2.Equals(""))
                    {
                        for (var index = 0; index < os.terminal.currentLine.Length; ++index)
                            text2 += "*";
                    }
                    else
                    {
                        var str = "";
                        for (var index = 0; index < text2.Length; ++index)
                            str += "*";
                        text2 = str;
                    }
                }
                if (strArray.Length > 3)
                {
                    num4 = 2;
                    num3 = Convert.ToInt32(strArray[3]);
                }
            }
            var destinationRectangle = GuiData.tmpRect;
            destinationRectangle.X = bounds.X + 2;
            destinationRectangle.Y = num2;
            destinationRectangle.Height = 200;
            destinationRectangle.Width = bounds.Width - 4;
            sb.Draw(Utils.white, destinationRectangle, num3 == 0 ? os.lockedColor : os.indentBackgroundColor);
            if (num3 != 0 && num3 != -1)
            {
                for (var index1 = 0; index1 < comp.users.Count; ++index1)
                {
                    if (comp.users[index1].name.Equals(text1))
                    {
                        user = comp.users[index1];
                        for (var index2 = 0; index2 < accounts.folders.Count; ++index2)
                        {
                            if (accounts.folders[index2].name.Equals(user.name))
                            {
                                userFolder = accounts.folders[index2];
                                break;
                            }
                        }
                        break;
                    }
                }
                state = 3;
            }
            destinationRectangle.Height = 22;
            var num5 = num2 + 30;
            var vector2 = TextItem.doMeasuredLabel(new Vector2(num1, num5), "Login ", textColor);
            if (num3 == 0)
            {
                var num6 = num1 + (int) vector2.X;
                TextItem.doLabel(new Vector2(num6, num5), "Failed", os.brightLockedColor);
                num1 = num6 - (int) vector2.X;
            }
            var num7 = num5 + 60;
            if (num4 == 0)
            {
                destinationRectangle.Y = num7;
                sb.Draw(Utils.white, destinationRectangle, os.subtleTextColor);
            }
            sb.DrawString(GuiData.smallfont, "username :", new Vector2(num1, num7), textColor);
            var num8 = num1 + 100;
            sb.DrawString(GuiData.smallfont, text1, new Vector2(num8, num7), textColor);
            var num9 = num8 - 100;
            var num10 = num7 + 30;
            if (num4 == 1)
            {
                destinationRectangle.Y = num10;
                sb.Draw(Utils.white, destinationRectangle, os.subtleTextColor);
            }
            sb.DrawString(GuiData.smallfont, "password :", new Vector2(num9, num10), textColor);
            var num11 = num9 + 100;
            sb.DrawString(GuiData.smallfont, text2, new Vector2(num11, num10), textColor);
            var y1 = num10 + 30;
            var x = num11 - 100;
            if (num3 != -1)
            {
                if (Button.doButton(12345, x, y1, 70, 30, "Back", os.indentBackgroundColor))
                    state = 0;
                if (!Button.doButton(123456, x + 75, y1, 70, 30, "Retry", os.indentBackgroundColor))
                    return;
                os.displayCache = "";
                os.execute("login");
                do
                    ; while (os.displayCache.Equals(""));
                os.display.command = name;
            }
            else
            {
                var y2 = y1 + 65;
                for (var index = 0; index < comp.users.Count; ++index)
                {
                    if (comp.users[index].known && validUser(comp.users[index].type))
                    {
                        if (Button.doButton(123457 + index, x, y2, 300, 25,
                            "User: " + comp.users[index].name + " Pass: " + comp.users[index].pass,
                            os.darkBackgroundColor))
                            forceLogin(comp.users[index].name, comp.users[index].pass);
                        y2 += 27;
                    }
                }
            }
        }

        public void forceLogin(string username, string pass)
        {
            var str = os.terminal.prompt;
            os.terminal.currentLine = username;
            os.terminal.executeLine();
            do
                ; while (os.terminal.prompt.Equals(str));
            os.terminal.currentLine = pass;
            os.terminal.executeLine();
        }

        public new static bool validUser(byte type)
        {
            if (!Daemon.validUser(type))
                return type == 2;
            return true;
        }

        public static string generateEmail(string subject, string body, string sender)
        {
            return "0" + emailSplitDelimiter + cleanString(sender) + emailSplitDelimiter + cleanString(subject) +
                   emailSplitDelimiter + minimalCleanString(body);
        }

        public static string generateEmail(string subject, string body, string sender, List<string> attachments)
        {
            var str = generateEmail(subject, body, sender) + emailSplitDelimiter;
            for (var index = 0; index < attachments.Count; ++index)
                str = str + attachments[index] + emailSplitDelimiter;
            return str;
        }

        public static string getSubject(string mail)
        {
            return mail.Split(emailSplitDelims, StringSplitOptions.None)[2].Replace(' ', '_');
        }

        public static string cleanString(string s)
        {
            return s.Replace('\n', '_').Replace('\r', '_').Replace(emailSplitDelimiter, "_");
        }

        public static string minimalCleanString(string s)
        {
            return s.Replace('\t', ' ');
        }

        public struct EMailData
        {
            public string sender;
            public string body;
            public string subject;
            public List<string> attachments;

            public EMailData(string sendr, string bod, string subj, List<string> _attachments)
            {
                sender = sendr;
                body = bod;
                subject = subj;
                attachments = _attachments;
            }
        }
    }
}
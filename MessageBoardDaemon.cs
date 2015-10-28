// Decompiled with JetBrains decompiler
// Type: Hacknet.MessageBoardDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class MessageBoardDaemon : AuthenticatingDaemon
    {
        private const int THREAD_PREVIEW_HEIGHT = 415;
        private static readonly Color UsernameColor = new Color(17, 119, 67);
        private static readonly Color ImplicationColor = new Color(56, 184, 131);
        private static Dictionary<MessageBoardPostImage, Texture2D> Images;
        public string BoardName = "/el/ - Digital Security";

        private readonly string boardsListingString =
            "[a/b/c/d/e/f/g/gif/h/hr/k/m/o/p/r/s/t/u/v/vg/vr/w/wg][i/ic][r9k][s4s][cm/hm/lgbt/y][3/adv/an/asp/cgl/ck/co/diy/fa/fit/gd/hc/int/jp/lit/mlp/mu/n/out/po/pol/sci/soc/sp/tg/toy/trv/tv/vp/wsg/x][rs]";

        private int CurrentThreadHeight = 100;
        private Folder rootFolder;
        private MessageBoardState state = MessageBoardState.Board;
        private Vector2 ThreadScrollPosition = Vector2.Zero;
        private Folder threadsFolder;
        private readonly ScrollableSectionedPanel threadsPanel;
        public List<string> ThreadsToAdd = new List<string>();
        private MessageBoardThread viewingThread;

        public MessageBoardDaemon(Computer c, OS os)
            : base(c, "Message Board", os)
        {
            if (Images == null)
            {
                Images = new Dictionary<MessageBoardPostImage, Texture2D>();
                Images.Add(MessageBoardPostImage.Academic, os.content.Load<Texture2D>("Sprites/Academic_Logo"));
                Images.Add(MessageBoardPostImage.Sun, os.content.Load<Texture2D>("Sprites/Sun"));
                Images.Add(MessageBoardPostImage.Snake, os.content.Load<Texture2D>("Sprites/Snake"));
                Images.Add(MessageBoardPostImage.Circle, os.content.Load<Texture2D>("CircleOutline"));
                Images.Add(MessageBoardPostImage.Duck, os.content.Load<Texture2D>("Sprites/Duck"));
                Images.Add(MessageBoardPostImage.Page, os.content.Load<Texture2D>("Sprites/Page"));
                Images.Add(MessageBoardPostImage.Speech, os.content.Load<Texture2D>("Sprites/SpeechBubble"));
                Images.Add(MessageBoardPostImage.Mod, os.content.Load<Texture2D>("Sprites/Hammer"));
                Images.Add(MessageBoardPostImage.Chip, os.content.Load<Texture2D>("Sprites/Chip"));
            }
            threadsPanel = new ScrollableSectionedPanel(415, GuiData.spriteBatch.GraphicsDevice);
        }

        public override void initFiles()
        {
            base.initFiles();
            rootFolder = new Folder("ImageBoard");
            threadsFolder = new Folder("Threads");
            rootFolder.folders.Add(threadsFolder);
            comp.files.root.folders.Add(rootFolder);
            for (var index = 0; index < ThreadsToAdd.Count; ++index)
                AddThread(ThreadsToAdd[index]);
            ThreadsToAdd.Clear();
        }

        public override void loadInit()
        {
            base.loadInit();
            rootFolder = comp.files.root.searchForFolder("ImageBoard");
            threadsFolder = rootFolder.searchForFolder("Threads");
        }

        public override string getSaveString()
        {
            return "<MessageBoard name=\"" + name + "\"/>";
        }

        public void AddThread(string threadData)
        {
            if (threadsFolder == null)
            {
                ThreadsToAdd.Add(threadData);
            }
            else
            {
                string str;
                do
                {
                    str = Utils.getRandomByte().ToString("000") + Utils.getRandomByte().ToString("000") +
                          Utils.getRandomByte().ToString("000") + ".tm";
                } while (threadsFolder.searchForFile(str) != null);
                threadsFolder.files.Add(new FileEntry(threadData, str));
            }
        }

        public MessageBoardThread ParseThread(string threadData)
        {
            var strArray = threadData.Split(new string[3]
            {
                "------------------------------------------\r\n",
                "------------------------------------------\n",
                "------------------------------------------"
            }, StringSplitOptions.None);
            var str1 = strArray[0].Replace("\n", "").Replace("\r", "");
            var messageBoardThread = new MessageBoardThread
            {
                id = str1,
                posts = new List<MessageBoardPost>()
            };
            for (var index = 1; index < strArray.Length; ++index)
            {
                if (strArray[index].Length > 1)
                {
                    var messageBoardPost = new MessageBoardPost();
                    var str2 = strArray[index];
                    if (strArray[index].StartsWith("#"))
                    {
                        var str3 = strArray[index].Substring(1, strArray[index].IndexOf('\n'));
                        try
                        {
                            var messageBoardPostImage =
                                (MessageBoardPostImage) Enum.Parse(typeof (MessageBoardPostImage), str3);
                            messageBoardPost.img = messageBoardPostImage;
                            str2 = strArray[index].Substring(strArray[index].IndexOf('\n') + 1);
                        }
                        catch (ArgumentException ex)
                        {
                            messageBoardPost.img = MessageBoardPostImage.None;
                        }
                    }
                    else
                        messageBoardPost.img = MessageBoardPostImage.None;
                    messageBoardPost.text = str2;
                    messageBoardThread.posts.Add(messageBoardPost);
                }
            }
            return messageBoardThread;
        }

        public void ViewThread(MessageBoardThread thread, int width, int margin, int ImageSize, int headerOffset)
        {
            CurrentThreadHeight = 20;
            for (var index = 0; index < thread.posts.Count; ++index)
                CurrentThreadHeight += MeasurePost(thread.posts[index], width, margin, ImageSize, headerOffset) +
                                       margin*2;
            state = MessageBoardState.Thread;
            viewingThread = thread;
            ThreadScrollPosition = Vector2.Zero;
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            state = MessageBoardState.Board;
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            var height = 60;
            var dest = new Rectangle(bounds.X, bounds.Y, bounds.Width, height);
            DrawHeader(sb, dest);
            var rectangle = bounds;
            rectangle.Y += height;
            rectangle.Height -= height + 1;
            switch (state)
            {
                case MessageBoardState.Thread:
                    DrawFullThreadView(sb, viewingThread, rectangle);
                    break;
                default:
                    threadsPanel.NumberOfPanels = threadsFolder.files.Count;
                    threadsPanel.Draw(
                        (index, drawArea, sBatch) =>
                            DrawThread(ParseThread(threadsFolder.files[index].data), sBatch, drawArea, true), sb,
                        rectangle);
                    break;
            }
        }

        private void DrawHeader(SpriteBatch sb, Rectangle dest)
        {
            var num1 = 4;
            sb.Draw(Utils.white, new Rectangle(dest.X + num1, dest.Y + 4, dest.Width - num1*2, 1), Color.White*0.5f);
            var position = new Vector2(dest.X + num1, dest.Y + 5);
            var index = 0;
            var num2 = 7;
            while (position.X + (double) (num2*2) < dest.X + dest.Width && index < boardsListingString.Length)
            {
                sb.DrawString(GuiData.detailfont, string.Concat(boardsListingString[index]), position, Color.White*0.6f);
                ++index;
                position.X += num2;
            }
            sb.DrawString(GuiData.detailfont, "]", position, Color.White*0.8f);
            try
            {
                if (BoardName == null)
                    BoardName = name;
                sb.DrawString(GuiData.font, BoardName, new Vector2(dest.X + num1, dest.Y + 19), os.highlightColor);
            }
            catch (Exception ex)
            {
            }
            if (state != MessageBoardState.Board)
            {
                var width = 200;
                if (Button.doButton(1931655802, dest.X + dest.Width - width - 6, dest.Y + dest.Height/2 - 4, width, 24,
                    "Back to Board", os.highlightColor))
                    state = MessageBoardState.Board;
            }
            sb.Draw(Utils.white, new Rectangle(dest.X + num1, dest.Y + dest.Height - 2, dest.Width - num1*2, 1),
                Color.White*0.5f);
        }

        private void DrawFullThreadView(SpriteBatch sb, MessageBoardThread thread, Rectangle dest)
        {
            var rectangle = dest;
            rectangle.Height = CurrentThreadHeight + 50;
            var flag = CurrentThreadHeight > dest.Height;
            if (flag)
            {
                ScrollablePanel.beginPanel(1931655001, rectangle, ThreadScrollPosition);
                rectangle.X = rectangle.Y = 0;
            }
            DrawThread(thread, GuiData.spriteBatch, rectangle, false);
            if (!flag)
                return;
            float maxScroll = Math.Max(dest.Height, CurrentThreadHeight - dest.Height);
            ThreadScrollPosition = ScrollablePanel.endPanel(1931655001, ThreadScrollPosition, dest, maxScroll, false);
        }

        private void DrawThread(MessageBoardThread thread, SpriteBatch sb, Rectangle bounds, bool isPreview = false)
        {
            var num1 = 4;
            var margin = 8;
            var num2 = 450;
            var height = 36;
            var num3 = 20;
            var ImageSize = 80;
            var ThreadFooterSize = 30;
            var font = GuiData.tinyfont;
            var dest = new Rectangle(bounds.X + margin, bounds.Y + margin, num2, height);
            var num4 = bounds.Y;
            var list = thread.posts;
            if (isPreview)
                list = GetLastPostsToFitHeight(thread, 415 - ThreadFooterSize, bounds.Width, margin, ImageSize, num3,
                    ThreadFooterSize, int.MaxValue);
            for (var index = 0; index < list.Count; ++index)
            {
                var messageBoardPost = list[index];
                var width = bounds.Width - 4*margin;
                if (messageBoardPost.img != MessageBoardPostImage.None)
                    width -= ImageSize;
                var text1 = Utils.SmartTwimForWidth(messageBoardPost.text, width, font);
                var vector2 = font.MeasureString(text1);
                vector2.Y *= 1.3f;
                dest.Y = num4;
                var val2 = (int) vector2.X + margin*4;
                if (messageBoardPost.img != MessageBoardPostImage.None)
                    val2 += ImageSize + margin;
                dest.Width = Math.Max(num2, val2);
                dest.Height = (int) vector2.Y + 2*margin;
                dest.Width = Math.Max(dest.Width, ImageSize + 2*margin);
                if (messageBoardPost.img != MessageBoardPostImage.None)
                    dest.Height = Math.Max(dest.Height, ImageSize + 2*margin);
                dest.Height += num3 + margin*2;
                DrawPost(text1, messageBoardPost.img, dest, margin, ImageSize, num3, sb, font);
                num4 += dest.Height + num1;
                if (index == 0 && isPreview)
                {
                    var thread1 = thread;
                    var text2 = "[+] " + (thread.posts.Count - list.Count) + " posts and image replies omitted";
                    var text3 = "Click here to view.";
                    sb.DrawString(GuiData.tinyfont, text2, new Vector2(dest.X, num4), os.lightGray);
                    if (Button.doButton(17839000 + thread.id.GetHashCode(), dest.X + 290, num4 - 1, 160, 17, text3,
                        new Color?()))
                    {
                        Console.WriteLine("clicked " + index);
                        ViewThread(thread1, bounds.Width, margin, ImageSize, num3);
                    }
                    num4 += 16 + num1;
                }
            }
            sb.Draw(Utils.white,
                new Rectangle(bounds.X + margin, bounds.Y + bounds.Height - 6, bounds.Width - margin*2, 1),
                Color.White*0.5f);
        }

        private int MeasurePost(MessageBoardPost post, int width, int margin, int ImageSize, int postHeaderOffset)
        {
            var font = GuiData.tinyfont;
            var num = postHeaderOffset;
            var width1 = width - 4*margin;
            if (post.img != MessageBoardPostImage.None)
                width1 -= ImageSize;
            var text = Utils.SmartTwimForWidth(post.text, width1, font);
            var vector2 = font.MeasureString(text);
            vector2.Y *= 1.3f;
            return Math.Max(num + (int) vector2.Y + 2*margin, ImageSize + 2*margin);
        }

        private void DrawPost(string text, MessageBoardPostImage img, Rectangle dest, int margin, int ImageSize,
            int postheaderOffset, SpriteBatch sb, SpriteFont font)
        {
            sb.Draw(Utils.white, dest, os.highlightColor*0.2f);
            var vector2 = new Vector2(dest.X + margin, dest.Y + margin);
            sb.Draw(Utils.white,
                new Rectangle((int) vector2.X, (int) vector2.Y, postheaderOffset - 5, postheaderOffset - 5),
                os.indentBackgroundColor);
            sb.DrawString(GuiData.smallfont, "Anonymous", vector2 + new Vector2(postheaderOffset, -2f), UsernameColor);
            sb.DrawString(GuiData.detailfont, "01/01/1970(Thu)00:00 UTC+0:0", vector2 + new Vector2(112f, 3f),
                Utils.SlightlyDarkGray);
            vector2.Y += postheaderOffset;
            if (img != MessageBoardPostImage.None && Images.ContainsKey(img))
            {
                var destinationRectangle = new Rectangle(dest.X + margin, dest.Y + margin + postheaderOffset, ImageSize,
                    ImageSize);
                sb.Draw(Images[img], destinationRectangle, Color.White);
                vector2.X += ImageSize + margin + margin;
            }
            var strArray = text.Split(Utils.newlineDelim);
            var num = font.MeasureString(strArray[0]).Y;
            for (var index = 0; index < strArray.Length; ++index)
                sb.DrawString(font, strArray[index], vector2 + new Vector2(0.0f, index*(num + 2f)),
                    strArray[index].StartsWith(">") ? ImplicationColor : Color.White);
        }

        private List<MessageBoardPost> GetLastPostsToFitHeight(MessageBoardThread thread, int height, int width,
            int margin, int ImageSize, int PostHeaderOffset, int ThreadFooterSize, int maxOPSize = 2147483647)
        {
            var list = new List<MessageBoardPost>();
            var num1 = ThreadFooterSize + margin*4;
            var val2 = MeasurePost(thread.posts[0], width, margin, ImageSize, PostHeaderOffset);
            var num2 = Math.Min(maxOPSize, val2);
            list.Add(thread.posts[0]);
            var num3 = num1 + num2;
            for (var index = thread.posts.Count - 1; index >= 0; --index)
            {
                var num4 = MeasurePost(thread.posts[index], width, margin, ImageSize, PostHeaderOffset);
                if (num3 + num4 < height)
                {
                    list.Insert(1, thread.posts[index]);
                    num3 += num4;
                }
                else
                    break;
            }
            return list;
        }

        private enum MessageBoardState
        {
            Thread,
            Board
        }
    }
}
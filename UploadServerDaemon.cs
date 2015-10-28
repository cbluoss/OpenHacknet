using System.Collections.Generic;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class UploadServerDaemon : AuthenticatingDaemon
    {
        private const string DEFAULT_FOLDERNAME = "Drop";
        private const string STORAGE_FOLDER_FOLDERNAME = "Uploads";
        private const string MESSAGE_FILENAME = "Server_Message.txt";

        private const string MESSAGE_FILE_DATA =
            "You are authenticated for open file uploading to this server.\nUse the \"upload [LOCAL_FILENAME]\" command to upload files\n\n\nREMINDER: Files downloaded with [scp] are downloaded to ~/home \n(or ~/bin for executables).\nuploads from there need their path proceeded as such:  home/[FILENAME]\nThe Tab key can be used to autocomplete paths and names of local files to upload.";

        private const int NUMBER_OF_ARROWS = 90;
        private const float MAX_FADE_TIME = 2f;
        private static readonly Vector2 ARROW_VELOCITY = new Vector2(0.0f, 400f);
        private readonly Texture2D arrow;
        private List<float> arrowDepths;
        private List<float> arrowFades;
        private List<Vector2> arrowPositions;
        private readonly Color darkThemeColor;
        public string Foldername;
        private readonly Color lightThemeColor;
        public bool needsAuthentication;
        private Folder root;
        private UploadServerState state;
        private Folder storageFolder;
        private readonly Color themeColor;
        private float uploadDetectedEffectTimer;
        private int uploadFileCountLastFrame;

        public UploadServerDaemon(Computer computer, string serviceName, Color themeColor, OS opSystem,
            string foldername = null, bool needsAuthentication = false)
            : base(computer, serviceName, opSystem)
        {
            arrow = os.content.Load<Texture2D>("Arrow");
            this.themeColor = themeColor;
            lightThemeColor = Color.Lerp(themeColor, Color.White, 0.4f);
            darkThemeColor = Color.Lerp(themeColor, Color.Black, 0.8f);
            Foldername = foldername;
            if (Foldername == null)
                Foldername = "Drop";
            this.needsAuthentication = needsAuthentication;
        }

        public override string getSaveString()
        {
            return "<UploadServerDaemon name=\"" + name + "\" foldername=\"" + Foldername + "\" color=\"" +
                   Utils.convertColorToParseableString(themeColor) + "\" needsAuth=\"" + needsAuthentication + "\" />";
        }

        public override void initFiles()
        {
            base.initFiles();
            root = comp.files.root.searchForFolder(Foldername);
            if (root == null)
            {
                root = new Folder(Foldername);
                comp.files.root.folders.Add(root);
            }
            storageFolder = root.searchForFolder("Uploads");
            if (storageFolder == null)
            {
                storageFolder = new Folder("Uploads");
                root.folders.Add(storageFolder);
            }
            root.files.Add(
                new FileEntry(
                    "You are authenticated for open file uploading to this server.\nUse the \"upload [LOCAL_FILENAME]\" command to upload files\n\n\nREMINDER: Files downloaded with [scp] are downloaded to ~/home \n(or ~/bin for executables).\nuploads from there need their path proceeded as such:  home/[FILENAME]\nThe Tab key can be used to autocomplete paths and names of local files to upload.",
                    "Server_Message.txt"));
            uploadFileCountLastFrame = storageFolder.files.Count;
        }

        public override void loadInit()
        {
            base.loadInit();
            root = comp.files.root.searchForFolder(Foldername);
            storageFolder = root.searchForFolder("Uploads");
        }

        public override void navigatedTo()
        {
            base.navigatedTo();
            if (!needsAuthentication)
                moveToActiveState();
            else
                state = UploadServerState.Menu;
        }

        private void moveToActiveState()
        {
            state = UploadServerState.Active;
            Programs.sudo(os, () =>
            {
                Programs.cd(new string[2]
                {
                    "cd",
                    Foldername + "/Uploads"
                }, os);
                os.display.command = name;
            });
            if (comp.userLoggedIn)
                return;
            comp.userLoggedIn = true;
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);
            drawBackground(bounds, sb);
            var num1 = bounds.X + 10;
            var num2 = bounds.Y + 10;
            TextItem.doFontLabel(new Vector2(num1, num2), name, GuiData.font, new Color?(), bounds.Width - 20,
                bounds.Height);
            var num3 = num2 + ((int) GuiData.font.MeasureString(name).Y + 2);
            var text = "Error: System.IO.FileNotFoundException went unhandled\n" + "File Server_Message.txt not found\n" +
                       "at UploadDaemonCore.RenderModule.HelptextDisplay.cs\nline 107 position 95";
            var fileEntry = root.searchForFile("Server_Message.txt");
            if (fileEntry != null)
                text = fileEntry.data;
            TextItem.doFontLabel(new Vector2(num1, num3), text, GuiData.tinyfont, new Color?(), float.MaxValue,
                float.MaxValue);
        }

        public void drawBackground(Rectangle bounds, SpriteBatch sb)
        {
            bounds = new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
            sb.Draw(Utils.gradient, bounds, themeColor);
            sb.Draw(arrow,
                new Rectangle(bounds.X, bounds.Y + bounds.Height/3, bounds.Width/2,
                    (int) (bounds.Height*0.666660010814667) + 1),
                new Rectangle(arrow.Width/2, 0, arrow.Width/2, arrow.Height), darkThemeColor);
            if (arrowPositions == null)
            {
                arrowPositions = new List<Vector2>();
                arrowFades = new List<float>();
                arrowDepths = new List<float>();
                for (var index = 0; index < 90; ++index)
                {
                    var num = 70f;
                    arrowPositions.Add(
                        new Vector2((float) (-num + Utils.random.NextDouble()*(num + (double) bounds.Width)),
                            bounds.Height/(float) (Utils.random.NextDouble()*2.0)));
                    arrowFades.Add((float) (0.100000001490116 + 0.899999998509884*Utils.random.NextDouble()));
                    arrowDepths.Add((float) (0.200000002980232 + (Utils.random.NextDouble() - 0.2)));
                }
            }
            var num1 = (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
            for (var index1 = 0; index1 < arrowPositions.Count; ++index1)
            {
                arrowPositions[index1] -= num1*ARROW_VELOCITY*arrowDepths[index1];
                List<float> list;
                int index2;
                (list = arrowFades)[index2 = index1] = list[index2] - num1/2f;
                if (arrowFades[index1] < 0.0)
                {
                    var num2 = 70f;
                    var vector2 = new Vector2(
                        (float) (-num2 + Utils.random.NextDouble()*(num2 + (double) bounds.Width)),
                        bounds.Height/(float) (Utils.random.NextDouble()*2.0));
                    arrowPositions[index1] = vector2;
                    arrowFades[index1] = (float) (0.5 + 1.5*Utils.random.NextDouble());
                    arrowDepths[index1] = (float) (0.200000002980232 + Utils.random.NextDouble()*0.4);
                }
                var scale = 0.3f*arrowDepths[index1];
                var pos = new Vector2(bounds.X + arrowPositions[index1].X,
                    bounds.Y + bounds.Height + arrowPositions[index1].Y);
                var rectForSpritePos = Utils.getClipRectForSpritePos(bounds, arrow, pos, scale);
                var vector2_1 = new Vector2(rectForSpritePos.X, rectForSpritePos.Y);
                pos += vector2_1;
                var destinationRectangle = new Rectangle((int) pos.X, (int) pos.Y,
                    (int) (scale*(double) (rectForSpritePos.Width - rectForSpritePos.X)),
                    (int) (scale*(double) (rectForSpritePos.Height - rectForSpritePos.Y)));
                sb.Draw(arrow, destinationRectangle, rectForSpritePos,
                    lightThemeColor*(arrowDepths[index1]*1.5f)*arrowFades[index1], 0.0f, Vector2.Zero,
                    SpriteEffects.None, arrowDepths[index1]);
            }
            drawUploadDetectedEffect(bounds, sb);
        }

        public void drawUploadDetectedEffect(Rectangle bounds, SpriteBatch sb)
        {
            uploadDetectedEffectTimer -= (float) os.lastGameTime.ElapsedGameTime.TotalSeconds;
            if (uploadFileCountLastFrame < storageFolder.files.Count)
            {
                uploadDetectedEffectTimer = 4f;
                uploadFileCountLastFrame = storageFolder.files.Count;
            }
            if (uploadDetectedEffectTimer <= 0.0)
                return;
            var white = Color.White;
            white.A = 0;
            white *= uploadDetectedEffectTimer/4f;
            var flag = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            TextItem.doFontLabel(new Vector2(bounds.X + bounds.Width/3, bounds.Y + bounds.Height - 60),
                "UPLOAD COMPLETE", GuiData.titlefont, white, bounds.Width/3*2, 58f);
            TextItem.DrawShadow = flag;
        }

        private enum UploadServerState
        {
            Menu,
            Login,
            Active
        }
    }
}
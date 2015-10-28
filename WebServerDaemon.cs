// Decompiled with JetBrains decompiler
// Type: Hacknet.WebServerDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.IO;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class WebServerDaemon : Daemon
    {
        public const string ROOT_FOLDERNAME = "web";
        public const string DEFAULT_PAGE_FILE = "index.html";
        public const string TEMP_WEBPAGE_CACHE_FILENAME = "/Content/Web/Cache/HN_OS_WebCache.html";
        private const string DEFAULT_PAGE_DATA_LOCATION = "Content/Web/BaseImageWebPage.html";
        private const string COMPANY_NAME_SENTINAL = "#$#COMPANYNAME#$#";
        private const string COMPANY_NAME_COMPACT_SENTINAL = "#$#LC_COMPANYNAME#$#";
        public const int BASE_BAR_HEIGHT = 16;
        private static string BaseWebpageData;
        private static string BaseComnayPageData;
        public FileEntry lastLoadedFile;
        public Folder root;
        private string saveURL = "";
        private bool shouldShow404;
        private readonly string webPageFileLocation;

        public WebServerDaemon(Computer computer, string serviceName, OS opSystem,
            string pageFileLocation = "Content/Web/BaseImageWebPage.html")
            : base(computer, serviceName, opSystem)
        {
            if (pageFileLocation == null)
                return;
            webPageFileLocation = pageFileLocation;
            if (!(webPageFileLocation != "Content/Web/BaseImageWebPage.html"))
                return;
            BaseWebpageData = null;
        }

        public override void initFiles()
        {
            root = new Folder("web");
            if (BaseWebpageData == null)
                BaseWebpageData = FileSanitiser.purifyStringForDisplay(Utils.readEntireFile(webPageFileLocation));
            var fileEntry = new FileEntry(BaseWebpageData, "index.html");
            root.files.Add(fileEntry);
            lastLoadedFile = fileEntry;
            comp.files.root.folders.Add(root);
        }

        public override void loadInit()
        {
            root = comp.files.root.searchForFolder("web");
        }

        public void generateBaseCorporateSite(string companyName,
            string targetBaseFile = "Content/Web/BaseCorporatePage.html")
        {
            if (BaseComnayPageData == null || targetBaseFile != "Content/Web/BaseCorporatePage.html")
                BaseComnayPageData = FileSanitiser.purifyStringForDisplay(Utils.readEntireFile(targetBaseFile));
            var dataEntry = BaseComnayPageData.Replace("#$#COMPANYNAME#$#", companyName)
                .Replace("#$#LC_COMPANYNAME#$#", companyName.Replace(' ', '_'));
            var fileEntry = root.searchForFile("index.html");
            if (fileEntry == null)
                fileEntry = new FileEntry(dataEntry, "index.html");
            else
                fileEntry.data = dataEntry;
            comp.files.root.searchForFolder("home").files.Add(new FileEntry(fileEntry.data, "index_BACKUP.html"));
        }

        public virtual void LoadWebPage(string url = "index.html")
        {
            saveURL = url;
            var fileEntry = root.searchForFile(url);
            if (fileEntry != null)
            {
                shouldShow404 = false;
                ShowPage(fileEntry.data);
            }
            else
                shouldShow404 = true;
            lastLoadedFile = fileEntry;
        }

        public virtual void ShowPage(string pageData)
        {
            var str = Directory.GetCurrentDirectory() + "/Content/Web/Cache/HN_OS_WebCache.html";
            Utils.writeToFile(pageData, str);
            WebRenderer.navigateTo(str);
        }

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            bounds.Height -= 16;
            ++bounds.X;
            bounds.Width -= 2;
            if (shouldShow404)
            {
                var vector2 = new Vector2(bounds.X + 10, bounds.Y + bounds.Height/4);
                sb.Draw(Utils.white, new Rectangle(bounds.X, (int) vector2.Y, (int) (bounds.Width*0.7), 80),
                    os.highlightColor*0.3f);
                TextItem.doFontLabel(vector2 + new Vector2(0.0f, 10f), "Error 404", GuiData.font, Color.White,
                    float.MaxValue, float.MaxValue);
                TextItem.doFontLabel(vector2 + new Vector2(0.0f, 42f), "Page not found", GuiData.smallfont, Color.White,
                    float.MaxValue, float.MaxValue);
            }
            else
                os.postFXDrawActions += () => WebRenderer.drawTo(bounds, sb);
            var rectangle = bounds;
            rectangle.Y += bounds.Height;
            rectangle.Height = 16;
            var flag = Button.smallButtonDraw;
            Button.smallButtonDraw = true;
            var width = 200;
            if (Button.doButton(83801, rectangle.X + 1, rectangle.Y + 1, width, rectangle.Height - 2,
                "HN: Exit Web View", new Color?()))
                os.display.command = "connect";
            if (os.hasConnectionPermission(false) &&
                Button.doButton(83805, rectangle.X + rectangle.Width - (width + 1), rectangle.Y + 1, width,
                    rectangle.Height - 2, "View Source", new Color?()))
                showSourcePressed();
            Button.smallButtonDraw = flag;
        }

        public virtual void showSourcePressed()
        {
            var str = "";
            var count = Programs.getNavigationPathAtPath("", os, root).Count;
            for (var index = 0; index < count; ++index)
                str += "../";
            os.runCommand("cd " + (str + "web"));
            os.delayer.Post(ActionDelayer.Wait(0.1), () => os.runCommand("cat " + lastLoadedFile.name));
        }

        public override void navigatedTo()
        {
            LoadWebPage("index.html");
        }

        public override string getSaveString()
        {
            return "<WebServer name=\"" + name + "\" url=\"" + saveURL + "\" />";
        }
    }
}
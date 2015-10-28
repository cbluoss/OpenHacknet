// Decompiled with JetBrains decompiler
// Type: Hacknet.OnlineWebServerDaemon
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Net;

namespace Hacknet
{
    internal class OnlineWebServerDaemon : WebServerDaemon
    {
        private static readonly string DEFAULT_PAGE_URL = "http://www.google.com";
        private string lastRequestedURL;
        public string webURL = DEFAULT_PAGE_URL;

        public OnlineWebServerDaemon(Computer computer, string serviceName, OS opSystem)
            : base(computer, serviceName, opSystem, "Content/Web/BaseImageWebPage.html")
        {
        }

        public void setURL(string url)
        {
            webURL = url;
        }

        public override void LoadWebPage(string url = null)
        {
            if (url == null || url == "index.html")
                url = webURL;
            var webClient = new WebClient();
            webClient.DownloadStringAsync(new Uri(url));
            webClient.DownloadStringCompleted += web_DownloadStringCompleted;
            lastRequestedURL = url;
        }

        private void web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
                instantiateWebPage(e.Result);
            else
                instantiateWebPage(FileSanitiser.purifyStringForDisplay(Utils.readEntireFile("Content/Web/404Page.html")));
        }

        private void instantiateWebPage(string body)
        {
            var str1 = FileSanitiser.purifyStringForDisplay(body);
            var str2 = lastRequestedURL;
            var fileEntry = root.searchForFile(str2);
            if (fileEntry == null)
            {
                fileEntry = root.searchForFile("index.html");
                str2 = "index.html";
            }
            if (fileEntry != null)
                fileEntry.data = str1;
            base.LoadWebPage(str2);
        }

        public override string getSaveString()
        {
            return "<OnlineWebServer name=\"" + name + "\" url=\"" + webURL + "\" />";
        }
    }
}
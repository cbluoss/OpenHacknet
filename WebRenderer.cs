using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Hacknet.Effects;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Hacknet
{
    internal class WebRenderer
    {
        private static int width = 500;
        private static int height = 500;
        private static string url = "http://www.google.com";
        private static WebRenderer instance;
        public static Texture2D texture;
        private static Bitmap formsBuffer;
        private static WebBrowser browser;
        private static GraphicsDevice graphics;
        private static bool loadingPage;

        public static void setSize(int frameWidth, int frameHeight)
        {
            width = frameWidth;
            height = frameHeight;
            if (graphics == null)
                return;
            texture = new Texture2D(graphics, width, height, false, SurfaceFormat.Color);
            formsBuffer = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        }

        public static void init(GraphicsDevice gd)
        {
            instance = new WebRenderer();
            graphics = gd;
            loadingPage = true;
        }

        public static void navigateTo(string urlTo)
        {
            loadingPage = true;
            Console.WriteLine("Launching Web Thread");
            url = urlTo;
            var thread = new Thread(instance.GetWebScreenshot);
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static void render()
        {
            browser.DrawToBitmap(formsBuffer, new System.Drawing.Rectangle(0, 0, width, height));
            CopyBitmapToTexture();
        }

        private void GetWebScreenshot()
        {
            browser = new WebBrowser();
            browser.Size = new Size(width, height);
            browser.AllowNavigation = false;
            browser.WebBrowserShortcutsEnabled = false;
            browser.ScrollBarsEnabled = false;
            browser.ScriptErrorsSuppressed = true;
            browser.WebBrowserShortcutsEnabled = false;
            browser.DocumentCompleted += web_DocumentCompleted;
            browser.Url = new Uri(url);
            Application.Run();
        }

        private void web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                loadingPage = false;
                browser.DrawToBitmap(formsBuffer, new System.Drawing.Rectangle(0, 0, width, height));
                CopyBitmapToTexture();
            }
            catch (InvalidOperationException ex)
            {
            }
            Application.ExitThread();
        }

        private static void CopyBitmapToTexture()
        {
            var numArray = new byte[4*width*height];
            var bitmapdata = formsBuffer.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bitmapdata.Scan0, numArray, 0, numArray.Length);
            formsBuffer.UnlockBits(bitmapdata);
            var index = 0;
            while (index < 4*texture.Width*texture.Height)
            {
                var num = numArray[index];
                numArray[index] = numArray[index + 2];
                numArray[index + 2] = num;
                numArray[index + 3] = byte.MaxValue;
                index += 4;
            }
            texture.SetData(numArray);
        }

        public static void drawTo(Rectangle bounds, SpriteBatch sb)
        {
            if (!loadingPage)
                sb.Draw(texture, bounds, Color.White);
            else
                WebpageLoadingEffect.DrawLoadingEffect(bounds, sb, OS.currentInstance, true);
        }

        public static string getURL()
        {
            return url;
        }
    }
}
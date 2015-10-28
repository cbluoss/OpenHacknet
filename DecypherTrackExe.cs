using System;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class DecypherTrackExe : ExeModule
    {
        private const float LOADING_TIME = 3.5f;
        private const float COMPLETE_TIME = 10f;
        private const float ERROR_TIME = 6f;
        private static readonly Color LoadingBarColorRed = new Color(196, 29, 60, 80);
        private static readonly Color LoadingBarColorBlue = new Color(29, 113, 196, 80);
        private string destFilename;
        private Folder destFolder;
        private string displayHeader = "Unknown";
        private string displayIP = "Unknown";
        private string errorMessage = "Unknown Error";
        private float percentComplete;
        private DecHeadStatus status = DecHeadStatus.Loading;
        private Computer targetComputer;
        private FileEntry targetFile;
        private string targetFilename;
        private float timeOnThisPhase;

        public DecypherTrackExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            IdentifierName = "DEC File Tracer";
            ramCost = 240;
            if (p.Length < 2)
            {
                status = DecHeadStatus.Error;
                errorMessage = "No File Provided";
            }
            else
                InitializeFiles(p[1]);
        }

        private void InitializeFiles(string filename)
        {
            targetComputer = os.thisComputer;
            if (os.connectedComp != null)
                targetComputer = os.connectedComp;
            destFolder = Programs.getCurrentFolder(os);
            targetFilename = filename;
            destFilename = filename.Replace(".dec", "[NUMBER][EXT]");
        }

        public override void Update(float t)
        {
            base.Update(t);
            timeOnThisPhase += t;
            float num;
            switch (status)
            {
                case DecHeadStatus.Error:
                    num = 6f;
                    if (timeOnThisPhase >= 6.0)
                    {
                        isExiting = true;
                    }
                    break;
                case DecHeadStatus.Complete:
                    num = 10f;
                    if (timeOnThisPhase >= 10.0)
                    {
                        isExiting = true;
                    }
                    break;
                default:
                    num = 3.5f;
                    if (timeOnThisPhase >= 3.5)
                    {
                        if (CompleteLoading())
                        {
                            GetHeaders();
                            status = DecHeadStatus.Complete;
                        }
                        timeOnThisPhase = 0.0f;
                    }
                    break;
            }
            percentComplete = timeOnThisPhase/num;
        }

        private bool CompleteLoading()
        {
            try
            {
                targetFile = destFolder.searchForFile(targetFilename);
                if (targetFile == null)
                {
                    status = DecHeadStatus.Error;
                    errorMessage = "File not found";
                    return false;
                }
                if (FileEncrypter.FileIsEncrypted(targetFile.data, "") != 0)
                    return true;
                status = DecHeadStatus.Error;
                errorMessage = "File is not\nDEC encrypted";
                return false;
            }
            catch (Exception ex)
            {
                status = DecHeadStatus.Error;
                errorMessage = "Fatal error in loading";
                return false;
            }
        }

        private void GetHeaders()
        {
            var strArray = FileEncrypter.DecryptHeaders(targetFile.data, "");
            displayHeader = strArray[0];
            displayIP = strArray[1];
            os.write(" \n \n---------------Header Analysis complete---------------\n \nDEC Encrypted File " +
                     targetFilename + " headers:");
            os.write("Encryption Header    : \"" + strArray[0] + "\"");
            os.write("Encryption Source IP: \"" + strArray[1] + "\"");
            os.write(" \n---------------------------------------------------------\n ");
        }

        private Rectangle DrawLoadingMessage(string message, float startPoint, Rectangle dest, bool showLoading = true,
            bool highlight = false)
        {
            var num1 = 0.18f;
            var num2 = (percentComplete - startPoint)/num1;
            if (percentComplete > (double) startPoint)
            {
                dest.Y += 22;
                spriteBatch.Draw(Utils.white, dest, Color.Black);
                var point = percentComplete <= startPoint + (double) num1 ? num2 : 1f;
                dest.Width = (int) (dest.Width*(double) Utils.QuadraticOutCurve(Utils.QuadraticOutCurve(point)));
                spriteBatch.Draw(Utils.white, dest,
                    status == DecHeadStatus.Complete ? LoadingBarColorBlue : LoadingBarColorRed);
                spriteBatch.DrawString(GuiData.tinyfont, message, new Vector2(bounds.X + 6, dest.Y + 2),
                    highlight ? Color.Black : Color.White);
                if (showLoading)
                {
                    if (percentComplete > startPoint + (double) num1)
                        spriteBatch.DrawString(GuiData.tinyfont, "COMPLETE", new Vector2(bounds.X + 172, dest.Y + 2),
                            Color.Black);
                    else
                        spriteBatch.DrawString(GuiData.tinyfont, (num2*100f).ToString("00") + "%",
                            new Vector2(bounds.X + 195, dest.Y + 2), Color.White);
                }
            }
            return dest;
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            var dest = bounds;
            ++dest.X;
            ++dest.Y;
            dest.Width -= 2;
            dest.Height -= 2;
            status.ToString();
            switch (status)
            {
                case DecHeadStatus.Error:
                    PatternDrawer.draw(dest, 1.2f, Color.Transparent, os.lockedColor, spriteBatch,
                        PatternDrawer.binaryTile);
                    var destinationRectangle = new Rectangle(bounds.X + 1, bounds.Y + 20, 200, 50);
                    if (bounds.Height > 120)
                    {
                        spriteBatch.Draw(Utils.white, destinationRectangle, os.lockedColor);
                        spriteBatch.DrawString(GuiData.font, "ERROR", new Vector2(bounds.X + 6, bounds.Y + 24),
                            Color.White);
                    }
                    destinationRectangle.Y += 50;
                    destinationRectangle.Height = 80;
                    destinationRectangle.Width = 250;
                    if (bounds.Height <= 160)
                        break;
                    spriteBatch.Draw(Utils.white, destinationRectangle, Color.Black);
                    spriteBatch.DrawString(GuiData.smallfont, errorMessage, new Vector2(bounds.X + 6, bounds.Y + 74),
                        Color.White);
                    break;
                case DecHeadStatus.Loading:
                    PatternDrawer.draw(dest, 1.2f, Color.Transparent, os.lockedColor, spriteBatch,
                        PatternDrawer.binaryTile);
                    var rectangle1 = new Rectangle(bounds.X + 1, bounds.Y + 20, 200, 50);
                    spriteBatch.Draw(Utils.white, rectangle1, Color.Black);
                    spriteBatch.DrawString(GuiData.font, "Loading...", new Vector2(bounds.X + 6, bounds.Y + 24),
                        Color.White);
                    rectangle1.Height = 20;
                    rectangle1.Width = 240;
                    rectangle1.Y += 28;
                    rectangle1 = DrawLoadingMessage("Reading File...", 0.0f, rectangle1, true, false);
                    rectangle1 = DrawLoadingMessage("Parsing Headers...", 0.18f, rectangle1, true, false);
                    rectangle1 = DrawLoadingMessage("Verifying Content...", 0.36f, rectangle1, true, false);
                    rectangle1 = DrawLoadingMessage("Checking DEC...", 0.54f, rectangle1, true, false);
                    rectangle1 = DrawLoadingMessage("Reading Codes...", 0.72f, rectangle1, true, false);
                    break;
                case DecHeadStatus.Complete:
                    PatternDrawer.draw(dest, 1.2f, Color.Transparent, os.highlightColor*0.5f, spriteBatch,
                        PatternDrawer.binaryTile);
                    if (bounds.Height <= 70)
                        break;
                    var rectangle2 = new Rectangle(bounds.X + 1, bounds.Y + 20, 200, 85);
                    spriteBatch.Draw(Utils.white, rectangle2, Color.Black);
                    spriteBatch.DrawString(GuiData.font, "Operation\nComplete", new Vector2(bounds.X + 6, bounds.Y + 24),
                        Color.White);
                    rectangle2.Height = 20;
                    rectangle2.Width = 240;
                    rectangle2.Y += 63;
                    if (rectangle2.Y + rectangle2.Height < bounds.Y + bounds.Height - 10)
                        rectangle2 = DrawLoadingMessage("Headers:", 0.0f, rectangle2, false, false);
                    if (rectangle2.Y + rectangle2.Height < bounds.Y + bounds.Height - 10)
                        rectangle2 = DrawLoadingMessage("\"" + displayHeader + "\"", 0.1f, rectangle2, false, true);
                    if (rectangle2.Y + rectangle2.Height < bounds.Y + bounds.Height - 10)
                        rectangle2 = DrawLoadingMessage("Source IP:", 0.2f, rectangle2, false, false);
                    if (rectangle2.Y + rectangle2.Height >= bounds.Y + bounds.Height - 10)
                        break;
                    rectangle2 = DrawLoadingMessage("\"" + displayIP + "\"", 0.3f, rectangle2, false, true);
                    break;
            }
        }

        private enum DecHeadStatus
        {
            Error,
            Loading,
            Complete
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class DecypherExe : ExeModule
    {
        private const float LOADING_TIME = 3.5f;
        private const float WORKING_TIME = 10f;
        private const float COMPLETE_TIME = 3f;
        private const float ERROR_TIME = 6f;
        private readonly List<int> columnsActive = new List<int>();
        private int columnsDrawn = 10;
        private string destFilename;
        private Folder destFolder;
        private string displayHeader = "Unknown";
        private string displayIP = "Unknown";
        private string errorMessage = "Unknown Error";
        private float lastLockedPercentage;
        private int lcgSeed = 1;
        private readonly string password = "";
        private float percentComplete;
        private readonly List<int> rowsActive = new List<int>();
        private int rowsDrawn = 10;
        private DecypherStatus status = DecypherStatus.Loading;
        private Computer targetComputer;
        private FileEntry targetFile;
        private string targetFilename;
        private float timeOnThisPhase;
        private string writtenFilename = "Unknown";

        public DecypherExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            IdentifierName = "Decypher Module";
            ramCost = 370;
            if (p.Length < 2)
            {
                status = DecypherStatus.Error;
                errorMessage = "No File Provided";
            }
            else
            {
                InitializeFiles(p[1]);
                if (p.Length <= 2)
                    return;
                password = p[2];
            }
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
                case DecypherStatus.Error:
                    num = 6f;
                    if (timeOnThisPhase >= 6.0)
                    {
                        isExiting = true;
                    }
                    break;
                case DecypherStatus.Working:
                    num = 10f;
                    if (timeOnThisPhase >= 10.0)
                    {
                        try
                        {
                            CompleteWorking();
                        }
                        catch (Exception ex)
                        {
                            status = DecypherStatus.Error;
                            timeOnThisPhase = 0.0f;
                            errorMessage = "Fatal error in decryption\nfile may be corrupt";
                        }
                        status = DecypherStatus.Complete;
                        timeOnThisPhase = 0.0f;
                        break;
                    }
                    if (percentComplete%0.100000001490116 < 0.00999999977648258)
                    {
                        lastLockedPercentage = percentComplete;
                        lcgSeed = Utils.random.Next();
                        rowsActive.Clear();
                        columnsActive.Clear();
                        for (var index = 0; index < columnsDrawn; ++index)
                        {
                            if (Utils.random.NextDouble() < 0.2)
                                rowsActive.Add(index);
                        }
                        for (var index = 0; index < rowsDrawn; ++index)
                        {
                            if (Utils.random.NextDouble() < 0.2)
                                columnsActive.Add(index);
                        }
                    }
                    break;
                case DecypherStatus.Complete:
                    num = 3f;
                    if (timeOnThisPhase >= 3.0)
                    {
                        isExiting = true;
                    }
                    break;
                default:
                    num = 3.5f;
                    if (timeOnThisPhase >= 3.5)
                    {
                        if (CompleteLoading())
                            status = DecypherStatus.Working;
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
                    status = DecypherStatus.Error;
                    errorMessage = "File not found";
                    return false;
                }
                switch (FileEncrypter.FileIsEncrypted(targetFile.data, password))
                {
                    case 0:
                        status = DecypherStatus.Error;
                        errorMessage = "File is not\nDEC encrypted";
                        return false;
                    case 2:
                        status = DecypherStatus.Error;
                        errorMessage = !(password == "")
                            ? "Provided Password\nIs Incorrect"
                            : "This File Requires\n a Password ";
                        return false;
                    default:
                        return true;
                }
            }
            catch (Exception ex)
            {
                status = DecypherStatus.Error;
                errorMessage = "Fatal error in loading";
                return false;
            }
        }

        private void CompleteWorking()
        {
            var strArray = FileEncrypter.DecryptString(targetFile.data, password);
            if (!destFilename.Contains("[NUMBER]"))
                destFilename += "[NUMBER]";
            var str1 = destFilename.Replace("[NUMBER]", "");
            var str2 = strArray[3] == null ? str1.Replace("[EXT]", ".txt") : str1.Replace("[EXT]", strArray[3]);
            if (destFolder.containsFile(str2))
            {
                var num = 1;
                do
                {
                    str2 = destFilename.Replace("[NUMBER]", "(" + num + ")");
                    ++num;
                } while (destFolder.containsFile(str2));
            }
            var fileEntry = new FileEntry(strArray[2], str2);
            writtenFilename = str2;
            destFolder.files.Add(fileEntry);
            os.write("Decryption complete - file " + targetFilename + " decrypted to target file " + targetFilename);
            os.write("Encryption Header    : \"" + strArray[0] + "\"");
            os.write("Encryption Source IP: \"" + strArray[1] + "\"");
            displayHeader = strArray[0];
            displayIP = strArray[1];
        }

        private Rectangle DrawLoadingMessage(string message, float startPoint, Rectangle dest, bool showLoading = true)
        {
            var num1 = 0.18f;
            var num2 = (percentComplete - startPoint)/num1;
            if (percentComplete > (double) startPoint)
            {
                dest.Y += 22;
                spriteBatch.Draw(Utils.white, dest, Color.Black);
                spriteBatch.DrawString(GuiData.tinyfont, message, new Vector2(bounds.X + 6, dest.Y + 2), Color.White);
                if (showLoading)
                {
                    if (percentComplete > startPoint + (double) num1)
                        spriteBatch.DrawString(GuiData.tinyfont, "COMPLETE", new Vector2(bounds.X + 172, dest.Y + 2),
                            Color.DarkGreen);
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
            status.ToString();
            switch (status)
            {
                case DecypherStatus.Error:
                    PatternDrawer.draw(bounds, 1.2f, Color.Transparent, os.lockedColor, spriteBatch,
                        PatternDrawer.errorTile);
                    var destinationRectangle1 = new Rectangle(bounds.X + 1, bounds.Y + 20, 200, 50);
                    spriteBatch.Draw(Utils.white, destinationRectangle1, os.lockedColor);
                    spriteBatch.DrawString(GuiData.font, "ERROR", new Vector2(bounds.X + 6, bounds.Y + 24), Color.White);
                    destinationRectangle1.Y += 50;
                    destinationRectangle1.Height = 80;
                    destinationRectangle1.Width = 250;
                    spriteBatch.Draw(Utils.white, destinationRectangle1, Color.Black);
                    spriteBatch.DrawString(GuiData.smallfont, errorMessage, new Vector2(bounds.X + 6, bounds.Y + 74),
                        Color.White);
                    break;
                case DecypherStatus.Loading:
                    PatternDrawer.draw(bounds, 1.2f, Color.Transparent, os.highlightColor*0.2f, spriteBatch,
                        PatternDrawer.thinStripe);
                    var rectangle1 = new Rectangle(bounds.X + 1, bounds.Y + 20, 200, 50);
                    spriteBatch.Draw(Utils.white, rectangle1, Color.Black);
                    spriteBatch.DrawString(GuiData.font, "Loading...", new Vector2(bounds.X + 6, bounds.Y + 24),
                        Color.White);
                    rectangle1.Height = 20;
                    rectangle1.Width = 240;
                    rectangle1.Y += 28;
                    DrawLoadingMessage("Reading Codes...", 0.72f,
                        DrawLoadingMessage("Checking DEC...", 0.54f,
                            DrawLoadingMessage("Verifying Content...", 0.36f,
                                DrawLoadingMessage("Parsing Headers...", 0.18f,
                                    DrawLoadingMessage("Reading File...", 0.0f, rectangle1, true), true), true), true),
                        true);
                    break;
                case DecypherStatus.Working:
                    var destinationRectangle2 = new Rectangle(bounds.X + 1, bounds.Y + 1, 200, 40);
                    spriteBatch.Draw(Utils.white, destinationRectangle2, Color.Black);
                    spriteBatch.DrawString(GuiData.font, "Decode:: " + (percentComplete*100f).ToString("00.0") + " %",
                        new Vector2(bounds.X + 2, bounds.Y + 2), Color.White);
                    var rectangle2 = new Rectangle(bounds.X, bounds.Y + 50, bounds.Width, bounds.Height - 50);
                    var num1 = rectangle2.X;
                    var num2 = 12;
                    var num3 = rectangle2.Y;
                    var num4 = 13;
                    var num5 = 0;
                    var num6 = 0;
                    var num7 = 0;
                    Utils.LCG.reSeed(lcgSeed);
                    while (num1 + num2 < rectangle2.X + rectangle2.Width)
                    {
                        var num8 = rectangle2.Y;
                        num7 = 0;
                        while (num8 + num4 < rectangle2.Y + rectangle2.Height)
                        {
                            var flag = rowsActive.Contains(num6) || columnsActive.Contains(num7);
                            var ch =
                                targetFile.data[
                                    (int)
                                        (((flag ? percentComplete : lastLockedPercentage)%10.0*targetFile.data.Length*
                                          3.0 + num5*222)%targetFile.data.Length)];
                            spriteBatch.DrawString(GuiData.tinyfont, string.Concat(ch), new Vector2(num1, num8),
                                Color.Lerp(Color.White, os.highlightColor,
                                    flag ? Utils.randm(1f) : Utils.LCG.NextFloat()));
                            num8 += num4;
                            ++num5;
                            ++num7;
                        }
                        num1 += num2 + 2;
                        ++num6;
                    }
                    columnsDrawn = num6;
                    rowsDrawn = num7;
                    break;
                case DecypherStatus.Complete:
                    PatternDrawer.draw(bounds, 1.2f, Color.Transparent, os.highlightColor*0.2f, spriteBatch,
                        PatternDrawer.thinStripe);
                    if (bounds.Height <= 60)
                        break;
                    var rectangle3 = new Rectangle(bounds.X + 1, bounds.Y + 20, 200, 85);
                    spriteBatch.Draw(Utils.white, rectangle3, Color.Black);
                    spriteBatch.DrawString(GuiData.font, "Operation\nComplete", new Vector2(bounds.X + 6, bounds.Y + 24),
                        Color.White);
                    rectangle3.Height = 20;
                    rectangle3.Width = 240;
                    rectangle3.Y += 63;
                    if (rectangle3.Y + rectangle3.Height < bounds.Y + bounds.Height)
                        rectangle3 = DrawLoadingMessage("Headers:", 0.0f, rectangle3, false);
                    if (rectangle3.Y + rectangle3.Height < bounds.Y + bounds.Height)
                        rectangle3 = DrawLoadingMessage("\"" + displayHeader + "\"", 0.1f, rectangle3, false);
                    if (rectangle3.Y + rectangle3.Height < bounds.Y + bounds.Height)
                        rectangle3 = DrawLoadingMessage("Source IP:", 0.3f, rectangle3, false);
                    if (rectangle3.Y + rectangle3.Height < bounds.Y + bounds.Height)
                        rectangle3 = DrawLoadingMessage("\"" + displayIP + "\"", 0.4f, rectangle3, false);
                    if (rectangle3.Y + rectangle3.Height < bounds.Y + bounds.Height)
                        rectangle3 = DrawLoadingMessage("Output File:", 0.6f, rectangle3, false);
                    if (rectangle3.Y + rectangle3.Height < bounds.Y + bounds.Height)
                        rectangle3 = DrawLoadingMessage("\"" + writtenFilename + "\"", 0.7f, rectangle3, false);
                    if (rectangle3.Y + rectangle3.Height < bounds.Y + bounds.Height)
                        rectangle3 = DrawLoadingMessage("Operation Complete", 0.9f, rectangle3, false);
                    if (rectangle3.Y + rectangle3.Height >= bounds.Y + bounds.Height)
                        break;
                    rectangle3 = DrawLoadingMessage("Shutting Down", 0.95f, rectangle3, false);
                    break;
            }
        }

        private enum DecypherStatus
        {
            Error,
            Loading,
            Working,
            Complete
        }
    }
}
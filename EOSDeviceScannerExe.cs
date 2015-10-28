using System;
using System.Collections.Generic;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class EOSDeviceScannerExe : ExeModule
    {
        private const float TOTAL_TIME = 8f;
        private const float timeBetweenBounces = 0.07f;
        private int devicesFound;
        private readonly string errorMessage;
        private bool IsComplete;
        private bool isError;
        private readonly List<Vector2> locations = new List<Vector2>();
        private readonly List<string> ResultBodies = new List<string>();
        private readonly List<string> ResultTitles = new List<string>();
        private readonly Computer targetComp;
        private float timer;
        private float timeToNextBounce;

        public EOSDeviceScannerExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            name = "eOS_DeviceScanner";
            ramCost = 300;
            IdentifierName = "eOS Device Scanner";
            targetComp = os.connectedComp;
            if (targetComp == null)
                targetComp = os.thisComputer;
            locations.Add(new Vector2(Utils.rand(), Utils.rand()));
            locations.Add(new Vector2(Utils.rand(), Utils.rand()));
            if (!(targetComp.adminIP != os.thisComputer.ip))
                return;
            isError = true;
            errorMessage = "ADMIN ACCESS\nREQUIRED FOR SCAN";
            IsComplete = true;
            for (var index = 0; index < 30; ++index)
                locations.Add(new Vector2(Utils.rand(), Utils.rand()));
        }

        public override void Update(float t)
        {
            base.Update(t);
            timer += t;
            if (timer > 8.0 && !IsComplete)
                Completed();
            if (timer >= 8.0 || errorMessage != null)
                return;
            timeToNextBounce -= t;
            if (timeToNextBounce > 0.0)
                return;
            locations.Add(new Vector2(Utils.rand(), Utils.rand()));
            timeToNextBounce = 0.07f;
        }

        public override void Completed()
        {
            base.Completed();
            IsComplete = true;
            if (targetComp.attatchedDeviceIDs != null)
            {
                var strArray = targetComp.attatchedDeviceIDs.Split(Utils.commaDelim,
                    StringSplitOptions.RemoveEmptyEntries);
                var num = 0.0f;
                for (var index = 0; index < strArray.Length; ++index)
                {
                    var device = Programs.getComputer(os, strArray[index]);
                    if (device != null)
                    {
                        Action action = () =>
                        {
                            os.netMap.discoverNode(device);
                            var loc = os.netMap.GetNodeDrawPos(device.location) +
                                      new Vector2(os.netMap.bounds.X, os.netMap.bounds.Y) +
                                      new Vector2(NetworkMap.NODE_SIZE/2);
                            SFX.addCircle(loc, os.highlightColor, 120f);
                            os.delayer.Post(ActionDelayer.Wait(0.2), () => SFX.addCircle(loc, os.highlightColor, 80f));
                            os.delayer.Post(ActionDelayer.Wait(0.4), () => SFX.addCircle(loc, os.highlightColor, 65f));
                            os.write("eOS Device \"" + device.name + "\" opened for connection at " + device.ip);
                            ResultTitles.Add(device.name);
                            ResultBodies.Add(device.ip + " " + device.location + "\n" + Guid.NewGuid());
                        };
                        os.delayer.Post(ActionDelayer.Wait(num), action);
                        ++num;
                        ++devicesFound;
                    }
                }
            }
            if (devicesFound != 0)
                return;
            isError = true;
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            var flag1 = TextItem.DrawShadow;
            TextItem.DrawShadow = false;
            var vector2_1 = locations[0];
            var vector2_2 = new Vector2(bounds.X + 2f, bounds.Y + 26f);
            var vector2_3 = new Vector2(bounds.Width - 4f, bounds.Height - 30f);
            if (vector2_3.X > 0.0 && vector2_3.Y > 0.0)
            {
                for (var index = 1; index < locations.Count; ++index)
                {
                    var vector2_4 = locations[index];
                    if (index == locations.Count - 1)
                        vector2_4 = Vector2.Lerp(vector2_1, vector2_4,
                            (float) (1.0 - timeToNextBounce/0.0700000002980232));
                    Utils.drawLine(spriteBatch, vector2_2 + vector2_1*vector2_3, vector2_2 + vector2_4*vector2_3,
                        Vector2.Zero,
                        (isError ? Utils.AddativeRed : Utils.AddativeWhite)*0.5f*(index/(float) locations.Count), 0.4f);
                    vector2_1 = locations[index];
                }
                for (var index = 1; index < locations.Count; ++index)
                    spriteBatch.Draw(Utils.white, locations[index]*vector2_3 + vector2_2, Utils.AddativeWhite);
            }
            if (IsComplete)
            {
                var flag2 = errorMessage != null;
                var destinationRectangle1 = new Rectangle(bounds.X + 1, bounds.Y + 26, bounds.Width - 2,
                    bounds.Height - 28);
                spriteBatch.Draw(Utils.white, destinationRectangle1, Color.Black*0.7f);
                var rectangle = destinationRectangle1;
                rectangle.Height = Math.Min(35, bounds.Height/5);
                var str = flag2 ? "ERROR" : "SCAN COMPLETE";
                TextItem.doFontLabel(new Vector2(rectangle.X, rectangle.Y), str, GuiData.titlefont, os.highlightColor,
                    rectangle.Width, rectangle.Height);
                TextItem.doFontLabel(new Vector2(rectangle.X, rectangle.Y), Utils.FlipRandomChars(str, 0.1),
                    GuiData.titlefont, Utils.AddativeWhite*(0.1f*Utils.rand()), rectangle.Width, rectangle.Height);
                var destinationRectangle2 = new Rectangle(rectangle.X, rectangle.Y + rectangle.Height, rectangle.Width,
                    1);
                spriteBatch.Draw(Utils.white, destinationRectangle2, Utils.AddativeWhite*0.5f);
                if (!isExiting)
                {
                    var text = "DEVICES FOUND : " + devicesFound;
                    if (flag2)
                        text = errorMessage;
                    var pos = new Vector2(rectangle.X + 2, rectangle.Y + rectangle.Height + 2);
                    TextItem.doFontLabel(pos, text, GuiData.titlefont,
                        (devicesFound > 0 ? Utils.AddativeWhite : Color.Red)*0.8f, bounds.Width - 10,
                        flag2 ? bounds.Height*0.8f : 23f);
                    pos.Y += 25f;
                    for (var index = 0;
                        index < ResultTitles.Count && pos.Y - (double) bounds.Y + 60.0 <= bounds.Height;
                        ++index)
                    {
                        TextItem.doFontLabel(pos, Utils.FlipRandomChars(ResultTitles[index], 0.01), GuiData.font,
                            Color.Lerp(os.highlightColor, Utils.AddativeWhite,
                                (float) (0.200000002980232 + 0.100000001490116*Utils.rand())), bounds.Width - 10, 24f);
                        pos.Y += 22f;
                        TextItem.doFontLabel(pos, ResultBodies[index], GuiData.detailfont, Utils.AddativeWhite*0.85f,
                            bounds.Width - 10, 30f);
                        pos.Y += 30f;
                        pos.Y += 4f;
                    }
                }
                if (!isExiting &&
                    Button.doButton(646464029 + PID, bounds.X + 2, bounds.Y + bounds.Height - 2 - 20, bounds.Width - 50,
                        20, "Exit", os.lockedColor))
                    isExiting = true;
            }
            else
            {
                var height = Math.Min(38, bounds.Height/3);
                var rectangle = new Rectangle(bounds.X + 1, bounds.Y + bounds.Height/2 - height/2, bounds.Width - 2,
                    height);
                spriteBatch.Draw(Utils.white, rectangle, Color.Black*0.7f);
                TextItem.doFontLabelToSize(rectangle, Utils.FlipRandomChars("SCANNING", 0.009), GuiData.titlefont,
                    IsComplete ? os.highlightColor : Utils.AddativeWhite*0.8f);
                TextItem.doFontLabelToSize(rectangle, Utils.FlipRandomChars("SCANNING", 0.15), GuiData.titlefont,
                    IsComplete ? os.highlightColor : Utils.AddativeWhite*(0.18f*Utils.rand()));
            }
            TextItem.DrawShadow = flag1;
        }
    }
}
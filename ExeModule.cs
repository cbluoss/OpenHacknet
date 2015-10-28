using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class ExeModule : Module
    {
        public static float FADEOUT_RATE = 0.5f;
        public static float MOVE_UP_RATE = 350f;
        public static int DEFAULT_RAM_COST = 246;
        private int baseRamCost;
        public float fade = 1f;
        public string IdentifierName = "UNKNOWN";
        public bool isExiting;
        public float moveUpBy;
        public bool needsProxyAccess;
        public bool needsRemoval;
        public int PID;
        public int ramCost = DEFAULT_RAM_COST;
        public string targetIP = "";

        public ExeModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            targetIP = operatingSystem.connectedComp == null
                ? operatingSystem.thisComputer.ip
                : operatingSystem.connectedComp.ip;
            bool flag;
            byte randomByte;
            do
            {
                flag = false;
                randomByte = Utils.getRandomByte();
                for (var index = 0; index < os.exes.Count; ++index)
                {
                    if (os.exes[index].PID == randomByte)
                    {
                        flag = true;
                        break;
                    }
                }
            } while (flag);
            os.currentPID = randomByte;
            PID = randomByte;
            bounds = location;
        }

        public override void LoadContent()
        {
            bounds.Height = ramCost;
        }

        public override void Update(float t)
        {
            bounds.Height = ramCost;
            if (isExiting)
            {
                if (fade >= 1.0)
                    baseRamCost = ramCost;
                ramCost = (int) (baseRamCost*(double) fade);
                bounds.Height = ramCost;
                fade -= t*FADEOUT_RATE;
                if (fade <= 0.0)
                    needsRemoval = true;
            }
            if (moveUpBy < 0.0)
                return;
            var num = (int) (MOVE_UP_RATE*(double) t);
            bounds.Y -= num;
            moveUpBy -= num;
            bounds.Y = Math.Max(bounds.Y, os.ram.bounds.Y + RamModule.contentStartOffset);
        }

        public override void Draw(float t)
        {
        }

        public virtual void Completed()
        {
        }

        public virtual void Killed()
        {
        }

        public virtual void drawOutline()
        {
            var destinationRectangle = bounds;
            RenderedRectangle.doRectangleOutline(destinationRectangle.X, destinationRectangle.Y,
                destinationRectangle.Width, destinationRectangle.Height, 1, os.moduleColorSolid);
            ++destinationRectangle.X;
            ++destinationRectangle.Y;
            destinationRectangle.Width -= 2;
            destinationRectangle.Height -= 2;
            spriteBatch.Draw(Utils.white, destinationRectangle, os.moduleColorBacking*fade);
        }

        public virtual void drawTarget(string typeName = "app:")
        {
            if (bounds.Height <= 14)
                return;
            var text = "IP: " + targetIP;
            var destinationRectangle = bounds;
            destinationRectangle.Height = Math.Min(bounds.Height, 14);
            ++destinationRectangle.X;
            destinationRectangle.Width -= 2;
            spriteBatch.Draw(Utils.white, destinationRectangle, os.exeModuleTopBar);
            RenderedRectangle.doRectangleOutline(destinationRectangle.X, destinationRectangle.Y,
                destinationRectangle.Width, destinationRectangle.Height, 1, os.topBarColor);
            if (destinationRectangle.Height < 14)
                return;
            var vector2 = GuiData.detailfont.MeasureString(text);
            spriteBatch.DrawString(GuiData.detailfont, text, new Vector2(bounds.X + bounds.Width - vector2.X, bounds.Y),
                os.exeModuleTitleText);
            spriteBatch.DrawString(GuiData.detailfont, typeName + IdentifierName, new Vector2(bounds.X + 2, bounds.Y),
                os.exeModuleTitleText);
        }

        public Rectangle GetContentAreaDest()
        {
            return new Rectangle(bounds.X + 1, bounds.Y + PANEL_HEIGHT, bounds.Width - 2,
                bounds.Height - PANEL_HEIGHT - 1);
        }
    }
}
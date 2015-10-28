// Decompiled with JetBrains decompiler
// Type: Hacknet.MedicalPortExe
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class MedicalPortExe : ExeModule
    {
        private const float RUNTIME = 22f;
        private const float COMPLETE_TIME = 2f;
        private bool[] CompletedIndexes;
        private readonly Color DarkBaseColor = new Color(5, 0, 36);
        private readonly Color DarkFinColor = new Color(179, 25, 94);
        private Color[] displayData;
        private float elapsedTime;
        private readonly Color LightBaseColor = new Color(39, 32, 83);
        private readonly Color LightFinColor = new Color(225, 14, 79);
        private float sucsessTimer;

        public MedicalPortExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            name = "KBT_PortTest";
            ramCost = 400;
            IdentifierName = "KBT Port Tester";
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (sucsessTimer <= 0.0)
            {
                elapsedTime += t;
                if (elapsedTime >= 22.0)
                    Complete();
            }
            else
            {
                sucsessTimer -= t;
                if (sucsessTimer <= 0.0)
                    isExiting = true;
            }
            if (displayData == null)
                InitializeDisplay();
            UpdateDisplay();
        }

        private void InitializeDisplay()
        {
            displayData = new Color[bounds.Height - 4 - PANEL_HEIGHT];
            CompletedIndexes = new bool[displayData.Length];
            for (var index = 0; index < displayData.Length; ++index)
                CompletedIndexes[index] = false;
        }

        private void UpdateDisplay()
        {
            if (elapsedTime%(double) (22f/displayData.Length) < 0.0199999995529652)
            {
                var num = 0;
                int index;
                do
                {
                    index = Utils.random.Next(displayData.Length);
                    ++num;
                } while (CompletedIndexes[index] && num < bounds.Height*2);
                CompletedIndexes[index] = true;
            }
            var index1 = Utils.random.Next(displayData.Length);
            if (CompletedIndexes[index1])
                displayData[index1] = Color.Lerp(displayData[index1], LightBaseColor, Utils.rand(0.5f));
            for (var index2 = 0; index2 < displayData.Length; ++index2)
                displayData[index2] = !CompletedIndexes[index2]
                    ? Color.Lerp(displayData[index2], Color.Lerp(DarkBaseColor, LightBaseColor, Utils.rand(1f)), 0.05f)
                    : Color.Lerp(displayData[index2], Color.Lerp(DarkFinColor, LightFinColor, Utils.rand(1f)), 0.05f);
        }

        private void Complete()
        {
            var computer = Programs.getComputer(os, targetIP);
            if (computer != null)
                computer.openPort(104, os.thisComputer.ip);
            sucsessTimer = 2f;
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            if (displayData == null)
                return;
            var destinationRectangle = bounds;
            ++destinationRectangle.X;
            destinationRectangle.Width -= 2;
            destinationRectangle.Y += 2 + PANEL_HEIGHT;
            destinationRectangle.Height = 1;
            var num1 = destinationRectangle.Width;
            for (var index = 0; index < displayData.Length; ++index)
            {
                var range = (float) (0.850000023841858*(1.0 - elapsedTime/22.0));
                var num2 = 0.95f - range;
                destinationRectangle.Width = !CompletedIndexes[index]
                    ? num1
                    : (int) (num1*(Utils.rand(range) + (double) num2));
                spriteBatch.Draw(Utils.white, destinationRectangle, displayData[index]);
                ++destinationRectangle.Y;
                if (destinationRectangle.Y > bounds.Y + bounds.Height - 2)
                    break;
            }
        }
    }
}
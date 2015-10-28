// Decompiled with JetBrains decompiler
// Type: Hacknet.ForkBombExe
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class ForkBombExe : ExeModule
    {
        public static float RAM_CHANGE_PS = 150f;
        public static string binary = "";
        public int binaryScroll;
        public int charsWide;
        public bool frameSwitch;
        public string runnerIP = "";
        private readonly int targetRamUse = 999999999;

        public ForkBombExe(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
            ramCost = 10;
            runnerIP = "UNKNOWN";
            IdentifierName = "ForkBomb";
        }

        public ForkBombExe(Rectangle location, OS operatingSystem, string ipFrom)
            : base(location, operatingSystem)
        {
            ramCost = 10;
            runnerIP = ipFrom;
            IdentifierName = "ForkBomb";
        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (binary.Equals(""))
                binary = Computer.generateBinaryString(5064);
            charsWide = (int) (bounds.Width/7.69999980926514 + 0.5);
        }

        public override void Update(float t)
        {
            base.Update(t);
            if (frameSwitch)
            {
                binaryScroll = binaryScroll + 1;
                if (binaryScroll >= binary.Length - (charsWide + 1))
                    binaryScroll = 0;
            }
            frameSwitch = !frameSwitch;
            if (targetRamUse == ramCost)
                return;
            var num = (int) (t*(double) RAM_CHANGE_PS);
            if (os.ramAvaliable < num)
            {
                Completed();
            }
            else
            {
                ramCost += num;
                if (ramCost <= targetRamUse)
                    return;
                ramCost = targetRamUse;
            }
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            var num = 8f;
            var startIndex = binaryScroll;
            if (startIndex >= binary.Length - (charsWide + 1))
                startIndex = 0;
            var position = new Vector2(bounds.X, bounds.Y);
            while (position.Y < bounds.Y + bounds.Height - (double) num)
            {
                spriteBatch.DrawString(GuiData.detailfont, binary.Substring(startIndex, charsWide), position,
                    Color.White);
                startIndex += charsWide;
                if (startIndex >= binary.Length - (charsWide + 1))
                    startIndex = 0;
                position.Y += num;
            }
        }

        public override void Completed()
        {
            base.Completed();
            os.thisComputer.crash(runnerIP);
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.CoreModule
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class CoreModule : Module
    {
        private static Texture2D LockSprite;
        private bool guiInputLockStatus;
        public bool inputLocked;

        public CoreModule(Rectangle location, OS operatingSystem)
            : base(location, operatingSystem)
        {
        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (LockSprite != null)
                return;
            LockSprite = os.content.Load<Texture2D>("Lock");
        }

        public override void PreDrawStep()
        {
            base.PreDrawStep();
            if (!inputLocked)
                return;
            guiInputLockStatus = GuiData.blockingInput;
            GuiData.blockingInput = true;
        }

        public override void PostDrawStep()
        {
            base.PostDrawStep();
            if (!inputLocked)
                return;
            GuiData.blockingInput = false;
            GuiData.blockingInput = guiInputLockStatus;
            var destinationRectangle = bounds;
            if (!destinationRectangle.Contains(GuiData.getMousePoint()))
                return;
            GuiData.spriteBatch.Draw(Utils.white, destinationRectangle, Color.Gray*0.5f);
            var position = new Vector2(destinationRectangle.X + destinationRectangle.Width/2 - LockSprite.Width/2,
                destinationRectangle.Y + destinationRectangle.Height/2 - LockSprite.Height/2);
            GuiData.spriteBatch.Draw(LockSprite, position, Color.White);
        }
    }
}
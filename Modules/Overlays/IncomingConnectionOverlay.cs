using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Modules.Overlays
{
    public class IncomingConnectionOverlay
    {
        private const float DURATION = 6f;
        private static readonly Color DrawColor = new Color(290, 0, 0, 0);
        private readonly Texture2D CautionSign;
        private readonly Texture2D CautionSignBG;
        public bool IsActive;
        private readonly SoundEffect sound1;
        private readonly SoundEffect sound2;
        private float timeElapsed;

        public IncomingConnectionOverlay(object OSobj)
        {
            CautionSign = ((OS) OSobj).content.Load<Texture2D>("Sprites/Icons/CautionIcon");
            CautionSignBG = ((OS) OSobj).content.Load<Texture2D>("Sprites/Icons/CautionIconBG");
            sound1 = ((OS) OSobj).content.Load<SoundEffect>("SFX/DoomShock");
            sound2 = ((OS) OSobj).content.Load<SoundEffect>("SFX/BrightFlash");
        }

        public void Activate()
        {
            IsActive = true;
            timeElapsed = 0.0f;
            sound1.Play();
            sound2.Play();
        }

        public void Update(float dt)
        {
            timeElapsed += dt;
            if (timeElapsed <= 6.0)
                return;
            IsActive = false;
        }

        public void Draw(Rectangle dest, SpriteBatch sb)
        {
            if (!IsActive)
                return;
            var num1 = timeElapsed;
            if (timeElapsed > 5.5)
                num1 -= 5.5f;
            if (num1 <= 0.5 && num1%0.100000001490116 < 0.0500000007450581)
                return;
            var height1 = 120;
            var num2 = timeElapsed/6.0;
            var num3 = 1f;
            var num4 = 0.2f;
            if (timeElapsed < (double) num4)
            {
                num3 = timeElapsed/num4;
                height1 = (int) (height1*(timeElapsed/(double) num4));
            }
            else if (timeElapsed > 6.0 - num4)
            {
                var num5 = (float) (1.0 - (timeElapsed - (6.0 - num4)));
                num3 = num5;
                height1 = (int) (height1*(double) num5);
            }
            var destinationRectangle = new Rectangle(dest.X, dest.Y + dest.Height/2 - height1/2, dest.Width, height1);
            sb.Draw(Utils.white, destinationRectangle, Color.Black*0.9f*num3);
            var text1 = "INCOMING CONNECTION";
            var text2 = "External unsyndicated UDP traffic on port 22\nLogging all activity to ~/log";
            var num6 = dest.Width/3;
            var height2 = (int) (24.0*num3);
            var dest1 = new Rectangle(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width,
                height2);
            PatternDrawer.draw(dest1, 1f, Color.Transparent, DrawColor, sb, PatternDrawer.warningStripe);
            dest1.Y += height1 - height2;
            PatternDrawer.draw(dest1, 1f, Color.Transparent, DrawColor, sb, PatternDrawer.warningStripe);
            var width1 = 700;
            var rectangle1 = new Rectangle(destinationRectangle.X + destinationRectangle.Width/2 - width1/2,
                destinationRectangle.Y, width1, destinationRectangle.Height);
            var width2 = (int) (CautionSign.Width/(double) CautionSign.Height*height1);
            var rectangle2 = new Rectangle(rectangle1.X, rectangle1.Y, width2, height1);
            rectangle2 = Utils.InsetRectangle(rectangle2, -30);
            sb.Draw(CautionSignBG, rectangle2, Color.Black*num3);
            var num7 = 4;
            rectangle2 = new Rectangle(rectangle2.X + num7, rectangle2.Y + num7, rectangle2.Width - num7*2,
                rectangle2.Height - num7*2);
            sb.Draw(CautionSign, rectangle2,
                Color.Lerp(Color.Red, DrawColor, (float) (0.949999988079071 + 0.0500000007450581*Utils.rand())));
            var dest2 = new Rectangle(rectangle1.X + rectangle2.Width + 2*num7 - 18, rectangle1.Y + 4,
                rectangle1.Width - (rectangle2.Width + num7*2) + 20, (int) (rectangle1.Height*0.8));
            TextItem.doFontLabelToSize(dest2, text1, GuiData.titlefont, DrawColor);
            dest2.Y += dest2.Height - 27;
            dest2.Height = (int) (rectangle1.Height*0.2);
            TextItem.doFontLabelToSize(dest2, text2, GuiData.detailfont, DrawColor*num3);
        }
    }
}
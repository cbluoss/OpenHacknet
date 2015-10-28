using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class BoatMail : MailServer
    {
        public static string JunkEmail =
            "HOr$e Exp@nding R0Lexxxx corp\n\nHello Mister Mott,\nI am but a humble nigerian prince who is crippled by the instability in my country, and require a transfer of 5000 united states dollar in order to rid my country of the scourge of the musclebeasts which roam our plains and ravage our villages\nPlease send these funds to real_nigerian_prince_the_third@boatmail.com\nYours in jegus,\nNigerian Prince";

        public Texture2D logo;

        public BoatMail(Computer c, string name, OS os)
            : base(c, name, os)
        {
            oddLine = Color.White;
            evenLine = Color.White;
            setThemeColor(new Color(155, 155, 230));
            seperatorLineColor = new Color(22, 22, 22, 150);
            panel = TextureBank.load("BoatmailHeader", os.content);
            logo = TextureBank.load("BoatmailLogo", os.content);
            textColor = Color.Black;
        }

        public override void drawBackingGradient(Rectangle boundsTo, SpriteBatch sb)
        {
        }

        public override void doInboxHeader(Rectangle bounds, SpriteBatch sb)
        {
        }

        public override void drawTopBar(Rectangle bounds, SpriteBatch sb)
        {
            os.postFXDrawActions += () =>
            {
                var destinationRectangle = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                sb.Draw(Utils.white, destinationRectangle, Color.White);
                destinationRectangle.Height = panel.Height;
                sb.Draw(panel, destinationRectangle, Color.White);
                destinationRectangle.Width = destinationRectangle.Height = 36;
                destinationRectangle.X += 30;
                destinationRectangle.Y += 10;
                sb.Draw(logo, destinationRectangle, Color.White);
            };
        }
    }
}
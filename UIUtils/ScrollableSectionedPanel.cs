using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.UIUtils
{
    public class ScrollableSectionedPanel
    {
        private RenderTarget2D AboveFragment;
        private RenderTarget2D BelowFragment;
        private readonly SpriteBatch fragmentBatch;
        private readonly GraphicsDevice graphics;
        public bool HasScrollBar = true;
        public int NumberOfPanels;
        public int PanelHeight = 100;
        public float ScrollDown;

        public ScrollableSectionedPanel(int panelHeight, GraphicsDevice graphics)
        {
            PanelHeight = panelHeight;
            fragmentBatch = new SpriteBatch(graphics);
            this.graphics = graphics;
        }

        private void UpdateInput(Rectangle dest)
        {
            ScrollDown += GuiData.getMouseWheelScroll()*20f;
            ScrollDown = Math.Max(Math.Min(ScrollDown, GetMaxScroll(dest)), 0.0f);
        }

        private float GetMaxScroll(Rectangle dest)
        {
            return NumberOfPanels*PanelHeight - (float) dest.Height;
        }

        public void Draw(Action<int, Rectangle, SpriteBatch> DrawSection, SpriteBatch sb, Rectangle destination)
        {
            UpdateInput(destination);
            SetRenderTargetsToFrame(destination);
            var flag = destination.Contains(GuiData.getMousePoint());
            var vector2_1 = GuiData.scrollOffset;
            var vector2_2 = new Vector2(destination.X, destination.Y - ScrollDown%PanelHeight);
            if (flag)
                GuiData.scrollOffset = vector2_2;
            var spriteBatch = GuiData.spriteBatch;
            var index1 = 0;
            var num1 = ScrollDown;
            while (num1 >= (double) PanelHeight)
            {
                num1 -= PanelHeight;
                ++index1;
            }
            var num2 = 0;
            if (ScrollDown%(double) PanelHeight != 0.0)
            {
                GuiData.spriteBatch = fragmentBatch;
                RenderToTarget(DrawSection, AboveFragment, index1, new Rectangle(0, 0, destination.Width, PanelHeight));
                var height = PanelHeight - (int) (ScrollDown%(double) PanelHeight);
                var destinationRectangle = new Rectangle(destination.X, destination.Y, destination.Width, height);
                var rectangle = new Rectangle(0, PanelHeight - destinationRectangle.Height, destination.Width, height);
                sb.Draw(AboveFragment, destinationRectangle, rectangle, Color.White);
                num2 += height;
                ++index1;
            }
            GuiData.spriteBatch = spriteBatch;
            if (flag)
                GuiData.scrollOffset = vector2_1;
            var index2 = index1;
            var rectangle1 = new Rectangle(destination.X, destination.Y + num2, destination.Width, PanelHeight);
            while (num2 + PanelHeight < destination.Height && index2 < NumberOfPanels)
            {
                DrawSection(index2, rectangle1, sb);
                ++index2;
                num2 += PanelHeight;
                rectangle1.Y = destination.Y + num2;
            }
            vector2_2 = new Vector2(destination.X, rectangle1.Y);
            if (flag)
                GuiData.scrollOffset = vector2_2;
            if (index2 < NumberOfPanels && destination.Height - num2 > 0)
            {
                GuiData.spriteBatch = fragmentBatch;
                RenderToTarget(DrawSection, BelowFragment, index2, new Rectangle(0, 0, destination.Width, PanelHeight));
                var height = destination.Height - num2;
                var destinationRectangle = new Rectangle(destination.X, rectangle1.Y, destination.Width, height);
                var rectangle2 = new Rectangle(0, 0, destination.Width, height);
                sb.Draw(BelowFragment, destinationRectangle, rectangle2, Color.White);
                var num3 = num2 + height;
                var num4 = index1 + 1;
            }
            GuiData.spriteBatch = spriteBatch;
            if (flag)
                GuiData.scrollOffset = vector2_1;
            if (!HasScrollBar)
                return;
            var width = 7;
            DrawScrollBar(
                new Rectangle(destination.X + destination.Width - width - 2, destination.Y, width,
                    destination.Height - 1), width);
        }

        private void DrawScrollBar(Rectangle dest, int width)
        {
            ScrollDown = ScrollBar.doVerticalScrollBar(184602004, dest.X, dest.Y, dest.Width, dest.Height,
                NumberOfPanels*PanelHeight, ScrollDown);
        }

        private void RenderToTarget(Action<int, Rectangle, SpriteBatch> DrawSection, RenderTarget2D target, int index,
            Rectangle destination)
        {
            var renderTarget = (RenderTarget2D) graphics.GetRenderTargets()[0].RenderTarget;
            graphics.SetRenderTarget(target);
            graphics.Clear(Color.Transparent);
            fragmentBatch.Begin();
            DrawSection(index, destination, fragmentBatch);
            fragmentBatch.End();
            graphics.SetRenderTarget(renderTarget);
        }

        private void SetRenderTargetsToFrame(Rectangle frame)
        {
            if (AboveFragment != null && AboveFragment.Width == frame.Width)
                return;
            AboveFragment = new RenderTarget2D(graphics, frame.Width, PanelHeight);
            BelowFragment = new RenderTarget2D(graphics, frame.Width, PanelHeight);
        }
    }
}
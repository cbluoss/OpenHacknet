// Decompiled with JetBrains decompiler
// Type: Hacknet.Gui.ScrollablePanel
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Gui
{
    public static class ScrollablePanel
    {
        private static Color scrollBarColor = new Color(120, 120, 120, 80);
        private static Stack<RenderTarget2D> targets;
        private static Stack<SpriteBatch> batches;
        private static List<RenderTarget2D> targetPool;
        private static List<SpriteBatch> batchPool;
        private static Stack<Vector2> offsetStack;

        public static void beginPanel(int id, Rectangle drawbounds, Vector2 scroll)
        {
            if (targets == null)
            {
                targets = new Stack<RenderTarget2D>();
                batches = new Stack<SpriteBatch>();
                targets.Push((RenderTarget2D) GuiData.spriteBatch.GraphicsDevice.GetRenderTargets()[0].RenderTarget);
                targetPool = new List<RenderTarget2D>();
                targetPool.Add(new RenderTarget2D(GuiData.spriteBatch.GraphicsDevice, drawbounds.Width,
                    drawbounds.Height));
                batchPool = new List<SpriteBatch>();
                batchPool.Add(new SpriteBatch(GuiData.spriteBatch.GraphicsDevice));
                batches.Push(GuiData.spriteBatch);
                offsetStack = new Stack<Vector2>();
            }
            if (batchPool.Count <= 0)
                batchPool.Add(new SpriteBatch(GuiData.spriteBatch.GraphicsDevice));
            var spriteBatch = batchPool[batchPool.Count - 1];
            batchPool.RemoveAt(batchPool.Count - 1);
            batches.Push(spriteBatch);
            var flag = false;
            for (var index = targetPool.Count - 1; index >= 0; --index)
            {
                var renderTarget2D = targetPool[index];
                if (renderTarget2D.Width == drawbounds.Width && renderTarget2D.Height == drawbounds.Height)
                {
                    targets.Push(renderTarget2D);
                    targetPool.RemoveAt(index);
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                var renderTarget2D = new RenderTarget2D(GuiData.spriteBatch.GraphicsDevice, drawbounds.Width,
                    drawbounds.Height);
                targets.Push(renderTarget2D);
                Console.WriteLine("Creating RenderTarget");
            }
            offsetStack.Push(GuiData.scrollOffset);
            GuiData.scrollOffset = new Vector2(drawbounds.X - scroll.X, drawbounds.Y - scroll.Y);
            GuiData.spriteBatch.GraphicsDevice.SetRenderTarget(targets.Peek());
            GuiData.spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            batches.Peek().Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            GuiData.spriteBatch = batches.Peek();
        }

        public static Vector2 endPanel(int id, Vector2 scroll, Rectangle bounds, float maxScroll,
            bool onlyScrollWithMouseOver = false)
        {
            batches.Peek().End();
            var renderTarget2D = targets.Pop();
            GuiData.spriteBatch.GraphicsDevice.SetRenderTarget(targets.Peek());
            targetPool.Add(renderTarget2D);
            batchPool.Add(batches.Pop());
            GuiData.spriteBatch = batches.Peek();
            GuiData.scrollOffset = offsetStack.Pop();
            var rectangle1 = GuiData.tmpRect;
            rectangle1.X = (int) scroll.X;
            rectangle1.Y = (int) scroll.Y;
            rectangle1.Width = bounds.Width;
            rectangle1.Height = bounds.Height;
            try
            {
                GuiData.spriteBatch.Draw(renderTarget2D, bounds, rectangle1, Color.White);
            }
            catch (InvalidOperationException ex)
            {
                return scroll;
            }
            if (!onlyScrollWithMouseOver || bounds.Contains(GuiData.getMousePoint()))
                scroll.Y += GuiData.getMouseWheelScroll()*20f;
            scroll.Y = Math.Max(Math.Min(scroll.Y, maxScroll), 0.0f);
            var rectangle2 = GuiData.tmpRect;
            var num1 = 5f;
            var num2 = bounds.Height/maxScroll*bounds.Height;
            var num3 = bounds.Height - 4f;
            var num4 = scroll.Y/maxScroll*(bounds.Height - num2);
            rectangle2.Y = (int) (num4 - num2/2.0 + num2/2.0 + bounds.Y);
            rectangle2.X = (int) (bounds.X + bounds.Width - 1.5*num1 - 2.0);
            rectangle2.Height = (int) num2;
            rectangle2.Width = (int) num1;
            scroll.Y = ScrollBar.doVerticalScrollBar(id, rectangle2.X, bounds.Y, rectangle2.Width, bounds.Height,
                renderTarget2D.Height, scroll.Y);
            scroll.Y = Math.Max(Math.Min(scroll.Y, maxScroll), 0.0f);
            return scroll;
        }

        public static void ClearCache()
        {
            targets = null;
        }
    }
}
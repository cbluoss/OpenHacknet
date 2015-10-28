// Decompiled with JetBrains decompiler
// Type: Hacknet.UIUtils.AnimatedSpriteExporter
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.UIUtils
{
    public static class AnimatedSpriteExporter
    {
        public static void ExportAnimation(string folderPath, string nameStarter, int width, int height,
            float framesPerSecond, float totalTime, GraphicsDevice gd, Action<float> update,
            Action<SpriteBatch, Rectangle> draw, int antialiasingMultiplier = 1)
        {
            var width1 = width*antialiasingMultiplier;
            var height1 = height*antialiasingMultiplier;
            var renderTarget = new RenderTarget2D(gd, width1, height1, false, SurfaceFormat.Rgba64, DepthFormat.Depth16,
                8, RenderTargetUsage.PlatformContents);
            var spriteBatch1 = new SpriteBatch(gd);
            gd.PresentationParameters.MultiSampleCount = 8;
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            gd.SetRenderTarget(renderTarget);
            var num1 = 1f/framesPerSecond;
            var num2 = 0.0f;
            var num3 = 0;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var spriteBatch2 = GuiData.spriteBatch;
            GuiData.spriteBatch = spriteBatch1;
            var rectangle = new Rectangle(0, 0, width1, height1);
            while (num2 < (double) totalTime)
            {
                gd.Clear(Color.Transparent);
                spriteBatch1.Begin();
                draw(spriteBatch1, rectangle);
                spriteBatch1.End();
                update(num1);
                gd.SetRenderTarget(null);
                var str = string.Concat(nameStarter, "_", num3, ".png");
                using (var fileStream = File.Create(folderPath + "/" + str))
                    renderTarget.SaveAsPng(fileStream, width, height);
                gd.SetRenderTarget(renderTarget);
                ++num3;
                num2 += num1;
            }
            GuiData.spriteBatch = spriteBatch2;
            gd.SetRenderTarget(currentRenderTarget);
        }
    }
}
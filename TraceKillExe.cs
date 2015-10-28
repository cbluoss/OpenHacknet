// Decompiled with JetBrains decompiler
// Type: Hacknet.TraceKillExe
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.Effects;
using Hacknet.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    internal class TraceKillExe : ExeModule
    {
        private const float TIME_BETWEEN_FOCUS_POINTS = 4f;
        private const float FOCUS_POINT_TRANSITION_TIME = 1.2f;
        private const float MAX_FOCUS_POINT_IDLE_TIME = 0.1f;
        private static readonly Color BoatBarColor = new Color(0, 0, 0, 200);
        private BarcodeEffect BotBarcode;
        private readonly Texture2D circle;
        private Vector2 EffectFocus = new Vector2(0.5f);
        private readonly SpriteBatch effectSB;
        private RenderTarget2D effectTarget;
        private float focusPointIdleTime;
        private Vector2 focusPointLocation = Vector2.Zero;
        private float focusPointTransitionTime;
        private bool hasDoneBurstForThisFocusPoint;
        private readonly List<PointImpactEffect> ImpactEffects = new List<PointImpactEffect>();
        private bool isOnFocusPoint;
        private float timeOnFocusPoint;
        private float timer;
        private float timeTillNextFocusPoint = 1f;
        private float traceActivityTimer;
        private readonly SoundEffect traceKillSound;

        public TraceKillExe(Rectangle location, OS operatingSystem, string[] p)
            : base(location, operatingSystem)
        {
            name = "TraceKill";
            ramCost = 600;
            IdentifierName = "TraceKill";
            targetIP = os.thisComputer.ip;
            effectSB = new SpriteBatch(GuiData.spriteBatch.GraphicsDevice);
            circle = os.content.Load<Texture2D>("Circle");
            traceKillSound = os.content.Load<SoundEffect>("SFX/TraceKill");
        }

        public override void Update(float t)
        {
            base.Update(t);
            UpdateEffect(t);
            if (!isExiting)
            {
                if (os.traceTracker.timeSinceFreezeRequest > 0.25)
                    traceKillSound.Play();
                os.traceTracker.timeSinceFreezeRequest = 0.0f;
            }
            if (BotBarcode != null)
                BotBarcode.Update(t*(os.traceTracker.active ? 3f : 2f));
            if (os.traceTracker.active)
                traceActivityTimer += t;
            else
                traceActivityTimer = 0.0f;
        }

        private void UpdateEffect(float t)
        {
            var num = 1f;
            if (os.traceTracker.active)
                num = 1.6f;
            timer += t*num;
            var vector2 = new Vector2((float) Math.Sin(timer*2.0), (float) Math.Sin(timer))*0.4f + new Vector2(0.5f);
            if (isOnFocusPoint && focusPointTransitionTime >= 1.20000004768372)
            {
                EffectFocus = focusPointLocation;
                timeOnFocusPoint += t;
                if (!hasDoneBurstForThisFocusPoint)
                {
                    ImpactEffects.Add(new PointImpactEffect(focusPointLocation, os));
                    hasDoneBurstForThisFocusPoint = true;
                }
                if (timeOnFocusPoint >= (double) focusPointIdleTime)
                {
                    isOnFocusPoint = false;
                    focusPointTransitionTime = 1.2f;
                    timeTillNextFocusPoint = Utils.randm(4f);
                }
            }
            else if (isOnFocusPoint && focusPointTransitionTime < 1.20000004768372 ||
                     !isOnFocusPoint && focusPointTransitionTime > 0.0)
            {
                focusPointTransitionTime += t*(isOnFocusPoint ? 1f : -1f);
                var point = focusPointTransitionTime/1.2f;
                EffectFocus = Vector2.Lerp(vector2, focusPointLocation, Utils.QuadraticOutCurve(point));
            }
            else
            {
                EffectFocus = vector2;
                timeTillNextFocusPoint -= t;
                if (timeTillNextFocusPoint <= 0.0)
                {
                    isOnFocusPoint = true;
                    focusPointTransitionTime = 0.0f;
                    timeOnFocusPoint = 0.0f;
                    hasDoneBurstForThisFocusPoint = false;
                    focusPointIdleTime = Utils.randm(0.1f);
                    focusPointLocation = new Vector2(Utils.randm(1f), Utils.randm(1f));
                }
            }
            for (var index = 0; index < ImpactEffects.Count; ++index)
            {
                var pointImpactEffect = ImpactEffects[index];
                pointImpactEffect.timeEnabled += t;
                if (pointImpactEffect.timeEnabled > 3.0)
                {
                    ImpactEffects.RemoveAt(index);
                    --index;
                }
                else
                    ImpactEffects[index] = pointImpactEffect;
            }
        }

        public override void Draw(float t)
        {
            base.Draw(t);
            drawOutline();
            drawTarget("app:");
            DrawEffect(t);
            if (bounds.Height > 100)
            {
                var dest = new Rectangle(bounds.X + 1, bounds.Y + 60, (int) (bounds.Width*0.7), 40);
                TextItem.doRightAlignedBackingLabelFill(dest,
                    os.traceTracker.active ? "SUPPRESSION ACTIVE" : "        SCANNING...", GuiData.titlefont,
                    BoatBarColor,
                    os.traceTracker.active
                        ? Color.Lerp(Color.Red, os.brightLockedColor, Utils.rand(1f))
                        : Color.White*0.8f);
                if (!os.traceTracker.active)
                {
                    dest.Y += dest.Height + 2;
                    dest.Height = 16;
                    TextItem.doRightAlignedBackingLabel(dest, "TraceKill v0.8011", GuiData.detailfont, BoatBarColor,
                        Color.DarkGray);
                }
            }
            if (bounds.Height <= 40)
                return;
            if (BotBarcode == null)
                BotBarcode = new BarcodeEffect(bounds.Width - 2, true, false);
            var maxHeight = 12;
            var height = 22;
            BotBarcode.Draw(bounds.X + 1, bounds.Y + bounds.Height - (maxHeight + height), bounds.Width - 2, maxHeight,
                spriteBatch, BoatBarColor);
            var destinationRectangle = new Rectangle(bounds.X + 1, bounds.Y + bounds.Height - height - 1,
                bounds.Width - 2, height);
            spriteBatch.Draw(Utils.white, destinationRectangle, BoatBarColor);
            var width = 130;
            if (Button.doButton(34004301, destinationRectangle.X + destinationRectangle.Width - width - 2,
                destinationRectangle.Y + 2, width, destinationRectangle.Height - 4, "EXIT", os.brightLockedColor))
                isExiting = true;
            var dest1 = new Rectangle(destinationRectangle.X + 2, destinationRectangle.Y + 5,
                destinationRectangle.Width - width - 4, destinationRectangle.Height/2);
            TextItem.doRightAlignedBackingLabel(dest1, traceActivityTimer.ToString("0.000000"), GuiData.detailfont,
                Color.Transparent, Color.White*0.8f);
            dest1.Y += dest1.Height - 2;
            TextItem.doRightAlignedBackingLabel(dest1, os.connectedComp != null ? os.connectedComp.ip : "UNKNOWN",
                GuiData.detailfont, Color.Transparent, Color.White*0.8f);
        }

        private void DrawEffect(float t)
        {
            var num = 16;
            var destinationRectangle = new Rectangle(bounds.X + 1, bounds.Y + num, bounds.Width - 2,
                bounds.Height - (num + 2));
            if (effectTarget == null || effectTarget.Width != destinationRectangle.Width ||
                effectTarget.Height < destinationRectangle.Height)
            {
                if (effectTarget != null)
                    effectTarget.Dispose();
                effectTarget = new RenderTarget2D(spriteBatch.GraphicsDevice, destinationRectangle.Width,
                    destinationRectangle.Height, false, SurfaceFormat.Rgba64, DepthFormat.None, 4,
                    RenderTargetUsage.PlatformContents);
            }
            var currentRenderTarget = Utils.GetCurrentRenderTarget();
            spriteBatch.GraphicsDevice.SetRenderTarget(effectTarget);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            effectSB.Begin();
            var dest = destinationRectangle;
            dest.X = 0;
            dest.Y = 0;
            DrawEffectFill(effectSB, dest);
            DrawImpactEffects(effectSB, dest);
            effectSB.End();
            spriteBatch.GraphicsDevice.SetRenderTarget(currentRenderTarget);
            var rectangle = destinationRectangle;
            rectangle.X = rectangle.Y = 0;
            spriteBatch.Draw(effectTarget, destinationRectangle, rectangle, Color.White);
        }

        private void DrawImpactEffects(SpriteBatch sb, Rectangle dest)
        {
            var color = os.traceTracker.active ? Color.Red : Utils.AddativeWhite;
            for (var index = 0; index < ImpactEffects.Count; ++index)
            {
                var pointImpactEffect = ImpactEffects[index];
                var vector2 = new Vector2(dest.X + pointImpactEffect.location.X*dest.Width,
                    dest.Y + pointImpactEffect.location.Y*dest.Height);
                if (pointImpactEffect.timeEnabled <= 1.0)
                {
                    var num = Utils.QuadraticOutCurve(Utils.QuadraticOutCurve(pointImpactEffect.timeEnabled/1f));
                    pointImpactEffect.cne.color = color*num;
                    pointImpactEffect.cne.ScaleFactor = num;
                    sb.Draw(circle, vector2, new Rectangle?(), color*(1f - num), 0.0f,
                        new Vector2(circle.Width/2, circle.Height/2), (float) (num/(double) circle.Width*30.0),
                        SpriteEffects.None, 0.7f);
                }
                else
                {
                    var num = Utils.QuadraticOutCurve((float) ((pointImpactEffect.timeEnabled - 1.0)/2.0));
                    pointImpactEffect.cne.color = color*(1f - num);
                    pointImpactEffect.cne.ScaleFactor = 1f;
                }
                pointImpactEffect.cne.draw(sb, vector2);
            }
        }

        private void DrawEffectFill(SpriteBatch sb, Rectangle dest)
        {
            sb.Draw(Utils.white, dest, os.indentBackgroundColor*0.8f);
            var num1 = 10f;
            var num2 = 2.5f;
            var lineThickness = 2f;
            var num3 = 0.08f;
            var baseColor = os.traceTracker.active ? os.lockedColor : Utils.VeryDarkGray*0.4f;
            var color = os.traceTracker.active ? os.brightLockedColor : Color.Gray;
            var targetPos = EffectFocus*new Vector2(dest.Width, dest.Height);
            var list = new List<Action>();
            var num4 = 0;
            var vector2 = new Vector2(dest.X - num1/2f, dest.Y - num1);
            while (vector2.Y + (double) lineThickness < dest.Y + dest.Height + (double) num1)
            {
                vector2.X = dest.X - num1/2f;
                while (vector2.X + (double) lineThickness < dest.X + dest.Width + (double) num1)
                {
                    var pos = vector2;
                    var amount = 1f - Utils.QuadraticOutCurve(Vector2.Distance(pos, targetPos)/dest.Height);
                    var highlightColor = Color.Lerp(baseColor, color, amount);
                    var length = Math.Min(num1*1.1f, Vector2.Distance(vector2, targetPos));
                    DrawTracerLine(sb, pos, lineThickness, targetPos, length, baseColor, highlightColor);
                    list.Add(() => DrawTracerLineShadow(sb, pos, lineThickness, targetPos, length, Color.Black*0.6f));
                    vector2.X += num1 + lineThickness/2f;
                }
                ++num4;
                num1 += num2;
                lineThickness += num3;
                vector2.Y += num1;
                if (num1 <= 0.0)
                    break;
            }
            for (var index = 0; index < list.Count; ++index)
                list[index]();
        }

        private void DrawTracerLine(SpriteBatch sb, Vector2 pos, float thickness, Vector2 target, float length,
            Color baseColor, Color highlightColor)
        {
            var rotation = (float) Math.Atan2(target.Y - (double) pos.Y, target.X - (double) pos.X);
            var destinationRectangle = new Rectangle((int) pos.X, (int) pos.Y, (int) length, (int) thickness);
            sb.Draw(Utils.white, destinationRectangle, new Rectangle?(), baseColor, rotation, Vector2.Zero,
                SpriteEffects.None, 0.7f);
            sb.Draw(Utils.gradientLeftRight, destinationRectangle, new Rectangle?(), highlightColor, rotation,
                Vector2.Zero, SpriteEffects.None, 0.7f);
        }

        private void DrawTracerLineShadow(SpriteBatch sb, Vector2 pos, float thickness, Vector2 target, float length,
            Color color)
        {
            var rotation = (float) Math.Atan2(target.Y - (double) pos.Y, target.X - (double) pos.X);
            sb.Draw(Utils.gradient, pos, new Rectangle?(), color, rotation, Vector2.Zero,
                new Vector2(length, (int) thickness), SpriteEffects.None, 0.6f);
            var rectangle = new Rectangle((int) pos.X, (int) pos.Y, (int) length, (int) thickness);
        }

        private struct PointImpactEffect
        {
            public const float TransInTime = 1f;
            public const float TransOutTime = 2f;
            public readonly ConnectedNodeEffect cne;
            public float timeEnabled;
            public Vector2 location;

            public PointImpactEffect(Vector2 location, OS os)
            {
                cne = new ConnectedNodeEffect(os, true);
                timeEnabled = 0.0f;
                this.location = location;
            }
        }
    }
}
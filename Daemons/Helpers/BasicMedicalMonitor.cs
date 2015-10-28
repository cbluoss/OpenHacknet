// Decompiled with JetBrains decompiler
// Type: Hacknet.Daemons.Helpers.BasicMedicalMonitor
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Daemons.Helpers
{
    public class BasicMedicalMonitor : IMedicalMonitor
    {
        private readonly CLinkBuffer<MonitorRecordKeypoint> data = new CLinkBuffer<MonitorRecordKeypoint>(1024);
        private readonly Func<float, float, List<MonitorRecordKeypoint>> HeartBeatAction;
        private readonly Func<float, float, List<MonitorRecordKeypoint>> UpdateAction;

        public BasicMedicalMonitor(Func<float, float, List<MonitorRecordKeypoint>> updateAction,
            Func<float, float, List<MonitorRecordKeypoint>> heartbeatAction)
        {
            UpdateAction = updateAction;
            HeartBeatAction = heartbeatAction;
        }

        public void Update(float dt)
        {
            var list = UpdateAction(data.Get(0).value, dt);
            for (var index = 0; index < list.Count; ++index)
                data.Add(list[index]);
        }

        public void HeartBeat(float beatTime)
        {
            var list = HeartBeatAction(data.Get(0).value, beatTime);
            for (var index = 0; index < list.Count; ++index)
                data.Add(list[index]);
        }

        public void Draw(Rectangle bounds, SpriteBatch sb, Color c, float timeRollback)
        {
            var offset = 0;
            var num1 = 4f;
            var num2 = 100f;
            var num3 = bounds.Height/3f;
            var x = bounds.Width - num1;
            var vector2_1 = new Vector2(x, 0.0f);
            var flag = false;
            var vector2_2 = new Vector2(bounds.X, bounds.Y + bounds.Height/2f);
            if (timeRollback > 0.0)
            {
                var num4 = 0.0f;
                while (num4 < (double) timeRollback)
                {
                    var monitorRecordKeypoint1 = data.Get(offset);
                    if (monitorRecordKeypoint1.timeOffset != 0.0)
                    {
                        if (num4 + (double) monitorRecordKeypoint1.timeOffset >= timeRollback)
                        {
                            var monitorRecordKeypoint2 = data.Get(offset - 1);
                            var num5 = (timeRollback - num4)/monitorRecordKeypoint1.timeOffset;
                            vector2_1 = Vector2.Lerp(new Vector2(x, monitorRecordKeypoint2.value*num3),
                                new Vector2(x, monitorRecordKeypoint1.value*num3), 1f - num5);
                            flag = true;
                            x -= (float) (monitorRecordKeypoint1.timeOffset*(double) num2*(1.0 - num5));
                            --offset;
                            break;
                        }
                        --offset;
                        num4 += monitorRecordKeypoint1.timeOffset;
                    }
                    else
                        break;
                }
            }
            while (x >= (double) num1)
            {
                var monitorRecordKeypoint = data.Get(offset);
                if (monitorRecordKeypoint.timeOffset == 0.0)
                    break;
                var vector2_3 = new Vector2(x, monitorRecordKeypoint.value*num3);
                if (flag)
                    Utils.drawLine(sb, vector2_2 + vector2_1, vector2_2 + vector2_3, Vector2.Zero, c, 0.56f);
                x -= monitorRecordKeypoint.timeOffset*num2;
                vector2_1 = vector2_3;
                --offset;
                flag = true;
            }
        }

        public float GetCurrentValue(float timeRollback)
        {
            var offset = 0;
            var vector2 = new Vector2(0.0f, 0.0f);
            var num1 = 0.0f;
            while (num1 < (double) timeRollback)
            {
                var monitorRecordKeypoint1 = data.Get(offset);
                if (monitorRecordKeypoint1.timeOffset != 0.0)
                {
                    if (num1 + (double) monitorRecordKeypoint1.timeOffset >= timeRollback)
                    {
                        var monitorRecordKeypoint2 = data.Get(offset - 1);
                        var num2 = (timeRollback - num1)/monitorRecordKeypoint1.timeOffset;
                        vector2 = Vector2.Lerp(new Vector2(0.0f, monitorRecordKeypoint2.value*1f),
                            new Vector2(0.0f, monitorRecordKeypoint1.value*1f), 1f - num2);
                        var num3 = offset - 1;
                        break;
                    }
                    --offset;
                    num1 += monitorRecordKeypoint1.timeOffset;
                }
                else
                    break;
            }
            return vector2.Y;
        }

        public struct MonitorRecordKeypoint
        {
            public float timeOffset;
            public float value;
        }
    }
}
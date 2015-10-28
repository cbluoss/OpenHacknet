// Decompiled with JetBrains decompiler
// Type: Hacknet.Effects.AudioVisualizer
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Hacknet.Effects
{
    public class AudioVisualizer
    {
        private List<ReadOnlyCollection<float>> samplesHistory;
        private readonly VisualizationData visData = new VisualizationData();

        public void Draw(Rectangle bounds, SpriteBatch sb)
        {
            if (samplesHistory == null)
            {
                samplesHistory = new List<ReadOnlyCollection<float>>();
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
                samplesHistory.Add(new ReadOnlyCollection<float>(new float[0]));
            }
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.IsVisualizationEnabled = true;
                MediaPlayer.GetVisualizationData(visData);
            }
            if (visData == null)
                return;
            var num1 = bounds.Height/(float) visData.Frequencies.Count;
            var vector2 = new Vector2(bounds.X, bounds.Y);
            for (var index1 = 0; index1 < visData.Frequencies.Count; ++index1)
            {
                var index2 = index1;
                sb.Draw(Utils.white,
                    new Rectangle((int) vector2.X, (int) vector2.Y,
                        (int) (bounds.Width*(double) visData.Frequencies[index2]), Math.Max(1, (int) num1)),
                    OS.currentInstance.highlightColor*(float) (0.200000002980232 + visData.Frequencies[index2]/2.0)*0.2f);
                vector2.Y += num1;
            }
            samplesHistory.Add(new ReadOnlyCollection<float>(visData.Samples.ToArray()));
            samplesHistory.RemoveAt(0);
            for (var index1 = 0; index1 < samplesHistory.Count; ++index1)
            {
                var num2 = index1/(float) (samplesHistory.Count - 1);
                vector2.Y = bounds.Y;
                for (var index2 = 0; index2 < samplesHistory[index1].Count; ++index2)
                {
                    var color = index1 >= samplesHistory.Count - 1
                        ? Utils.AddativeWhite*0.7f
                        : OS.currentInstance.highlightColor;
                    sb.Draw(Utils.white,
                        new Vector2(
                            bounds.X + (float) (samplesHistory[index1][index2]*(bounds.Width/4.0) + bounds.Width*0.75) +
                            index1*2 - samplesHistory.Count, vector2.Y), new Rectangle?(),
                        color*0.6f*(0.01f + num2)*0.4f);
                    vector2.Y += num1;
                }
            }
        }
    }
}
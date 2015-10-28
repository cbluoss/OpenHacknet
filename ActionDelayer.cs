using System;
using System.Collections.Generic;

namespace Hacknet
{
    internal class ActionDelayer
    {
        public delegate bool Condition(ActionDelayer messagePump);

        private readonly List<Pair> nextPairs = new List<Pair>();
        private readonly List<Pair> pairs = new List<Pair>();

        public DateTime Time { get; private set; }

        public void Pump()
        {
            Time = DateTime.Now;
            pairs.AddRange(nextPairs);
            nextPairs.Clear();
            for (var index = 0; index < pairs.Count; ++index)
            {
                var pair = pairs[index];
                if (pair.Condition(this))
                {
                    pair.Action();
                    pairs.RemoveAt(index--);
                }
            }
        }

        public void RunAllDelayedActions()
        {
            pairs.AddRange(nextPairs);
            nextPairs.Clear();
            Time = DateTime.MaxValue;
            for (var index = 0; index < pairs.Count; ++index)
            {
                var pair = pairs[index];
                if (pair.Condition(this))
                {
                    pair.Action();
                    pairs.RemoveAt(index--);
                }
            }
        }

        public void Post(Condition condition, Action action)
        {
            nextPairs.Add(new Pair
            {
                Condition = condition,
                Action = action
            });
        }

        public void PostAnimation(IEnumerator<Condition> animation)
        {
            Action tick = null;
            tick = () =>
            {
                if (!animation.MoveNext())
                    return;
                Post(animation.Current, tick);
            };
            tick();
        }

        public static Condition WaitUntil(DateTime time)
        {
            return x => x.Time >= time;
        }

        public static Condition Wait(double time)
        {
            return WaitUntil(DateTime.Now + TimeSpan.FromSeconds(time));
        }

        public static Condition NextTick()
        {
            return x => true;
        }

        public static Condition FileDeleted(Folder f, string filename)
        {
            return x => !f.containsFile(filename);
        }

        private struct Pair
        {
            public Condition Condition;
            public Action Action;
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.InputMap
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet
{
    public struct InputMap
    {
        private static readonly InputMap e = new InputMap();
        public InputStates now;
        public InputStates last;

        public static InputMap Empty
        {
            get
            {
                var inputMap = e;
                return e;
            }
        }

        public InputMap(InputStates last, InputStates now)
        {
            this.last = last;
            this.now = now;
        }

        public static bool operator ==(InputMap self, InputMap other)
        {
            return self.now == other.now && self.last == other.last;
        }

        public static bool operator !=(InputMap self, InputMap other)
        {
            return !(self.now == other.now) || !(self.last == other.last);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
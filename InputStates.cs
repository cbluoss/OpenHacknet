// Decompiled with JetBrains decompiler
// Type: Hacknet.InputStates
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet
{
    public struct InputStates
    {
        public float movement;
        public bool jumping;
        public bool useItem;
        public float wmovement;
        public bool wjumping;
        public bool wuseItem;

        public static bool operator ==(InputStates self, InputStates other)
        {
            return self.movement == (double) other.movement && self.jumping == other.jumping &&
                   (self.useItem == other.useItem && self.wmovement == (double) other.wmovement) &&
                   (self.wjumping == other.wjumping && self.wuseItem == other.wuseItem);
        }

        public static bool operator !=(InputStates self, InputStates other)
        {
            return self.movement != (double) other.movement || self.jumping != other.jumping ||
                   (self.useItem != other.useItem || self.wmovement != (double) other.wmovement) ||
                   (self.wjumping != other.wjumping || self.wuseItem != other.wuseItem);
        }

        public override bool Equals(object obj)
        {
            return obj is InputStates && (InputStates) obj == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
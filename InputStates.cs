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
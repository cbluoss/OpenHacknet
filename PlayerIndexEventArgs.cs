using System;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    internal class PlayerIndexEventArgs : EventArgs
    {
        public PlayerIndexEventArgs(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public PlayerIndex PlayerIndex { get; }
    }
}
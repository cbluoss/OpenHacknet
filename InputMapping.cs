// Decompiled with JetBrains decompiler
// Type: Hacknet.InputMapping
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    public class InputMapping
    {
        private static InputStates ret;
        private static InputMap map;
        private static InputStates lastCalculatedState;

        public static InputStates getStatesFromKeys(KeyboardState keys, GamePadState pad, GamePadThumbSticks sticks)
        {
            var inputStates = ret;
            ret.movement = keys.IsKeyDown(Keys.Right) || pad.DPad.Right == ButtonState.Pressed
                ? 1f
                : (keys.IsKeyDown(Keys.Left) || pad.DPad.Left == ButtonState.Pressed
                    ? -1f
                    : sticks.Left.X == 0.0 ? 0.0f : sticks.Left.X);
            var buttons1 = pad.Buttons;
            ret.jumping = buttons1.A == ButtonState.Pressed || keys.IsKeyDown(Keys.Up);
            var buttons2 = pad.Buttons;
            ret.useItem = buttons2.B == ButtonState.Pressed || keys.IsKeyDown(Keys.RightShift);
            ret.wmovement = keys.IsKeyDown(Keys.D) || pad.DPad.Right == ButtonState.Pressed
                ? 1f
                : (keys.IsKeyDown(Keys.A) || pad.DPad.Left == ButtonState.Pressed
                    ? -1f
                    : sticks.Left.X == 0.0 ? 0.0f : sticks.Left.X);
            ret.wjumping = pad.Buttons.A == ButtonState.Pressed || keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.Space) ||
                           pad.Triggers.Left > 0.0;
            var buttons3 = pad.Buttons;
            ret.wuseItem = buttons3.B == ButtonState.Pressed || keys.IsKeyDown(Keys.LeftShift);
            lastCalculatedState = ret;
            return ret;
        }

        public static InputMap getMapFromKeys(KeyboardState keys, GamePadState pad)
        {
            map.last = lastCalculatedState;
            map.now = getStatesFromKeys(keys, pad, pad.ThumbSticks);
            return map;
        }
    }
}
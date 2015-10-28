// Decompiled with JetBrains decompiler
// Type: Hacknet.InputState
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    public class InputState
    {
        public const int MaxInputs = 4;
        public static InputState Empty = new InputState();
        public readonly bool[] GamePadWasConnected;
        public GamePadState[] CurrentGamePadStates;
        public KeyboardState[] CurrentKeyboardStates;
        public GamePadState[] LastGamePadStates;
        public KeyboardState[] LastKeyboardStates;

        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[4];
            CurrentGamePadStates = new GamePadState[4];
            LastKeyboardStates = new KeyboardState[4];
            LastGamePadStates = new GamePadState[4];
            GamePadWasConnected = new bool[4];
        }

        public void Update()
        {
            for (var index = 0; index < 4; ++index)
            {
                LastKeyboardStates[index] = CurrentKeyboardStates[index];
                LastGamePadStates[index] = CurrentGamePadStates[index];
                CurrentKeyboardStates[index] = Keyboard.GetState((PlayerIndex) index);
                CurrentGamePadStates[index] = GamePad.GetState((PlayerIndex) index);
                if (CurrentGamePadStates[index].IsConnected)
                    GamePadWasConnected[index] = true;
            }
        }

        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                playerIndex = controllingPlayer.Value;
                var index = (int) playerIndex;
                if (CurrentKeyboardStates[index].IsKeyDown(key))
                    return LastKeyboardStates[index].IsKeyUp(key);
                return false;
            }
            if (!IsNewKeyPress(key, PlayerIndex.One, out playerIndex) &&
                !IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) &&
                !IsNewKeyPress(key, PlayerIndex.Three, out playerIndex))
                return IsNewKeyPress(key, PlayerIndex.Four, out playerIndex);
            return true;
        }

        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                playerIndex = controllingPlayer.Value;
                var index = (int) playerIndex;
                if (CurrentGamePadStates[index].IsButtonDown(button))
                    return LastGamePadStates[index].IsButtonUp(button);
                return false;
            }
            if (!IsNewButtonPress(button, PlayerIndex.One, out playerIndex) &&
                !IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) &&
                !IsNewButtonPress(button, PlayerIndex.Three, out playerIndex))
                return IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
            return true;
        }

        public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (!IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) &&
                !IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) &&
                !IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex))
                return IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
            return true;
        }

        public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (!IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) &&
                !IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex))
                return IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
            return true;
        }

        public bool IsMenuUp(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            if (!IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) &&
                !IsNewKeyPress(Keys.W, controllingPlayer, out playerIndex) &&
                !IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex))
                return IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
            return true;
        }

        public bool IsMenuDown(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            if (!IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) &&
                !IsNewKeyPress(Keys.D, controllingPlayer, out playerIndex) &&
                !IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex))
                return IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
            return true;
        }

        public bool IsPauseGame(PlayerIndex? controllingPlayer)
        {
            PlayerIndex playerIndex;
            if (!IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) &&
                !IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex))
                return IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
            return true;
        }
    }
}
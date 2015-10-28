using System;
using Microsoft.Xna.Framework;

namespace Hacknet
{
    public abstract class GameScreen
    {
        private bool otherScreenHasFocus;

        public bool IsPopup { get; protected set; }

        public TimeSpan TransitionOnTime { get; protected set; } = TimeSpan.Zero;

        public TimeSpan TransitionOffTime { get; protected set; } = TimeSpan.Zero;

        public float TransitionPosition { get; protected set; } = 1f;

        public byte TransitionAlpha
        {
            get { return (byte) (byte.MaxValue - TransitionPosition*(double) byte.MaxValue); }
        }

        public ScreenState ScreenState { get; protected set; }

        public bool IsExiting { get; protected internal set; }

        public bool IsActive
        {
            get
            {
                if (otherScreenHasFocus)
                    return false;
                if (ScreenState != ScreenState.TransitionOn)
                    return ScreenState == ScreenState.Active;
                return true;
            }
        }

        public ScreenManager ScreenManager { get; internal set; }

        public PlayerIndex? ControllingPlayer { get; internal set; }

        public virtual void LoadContent()
        {
        }

        public virtual void UnloadContent()
        {
        }

        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            this.otherScreenHasFocus = otherScreenHasFocus;
            if (IsExiting)
            {
                ScreenState = ScreenState.TransitionOff;
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                    return;
                ScreenManager.RemoveScreen(this);
            }
            else if (coveredByOtherScreen)
            {
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                    ScreenState = ScreenState.TransitionOff;
                else
                    ScreenState = ScreenState.Hidden;
            }
            else if (UpdateTransition(gameTime, TransitionOnTime, -1))
                ScreenState = ScreenState.TransitionOn;
            else
                ScreenState = ScreenState.Active;
        }

        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            TransitionPosition += (!(time == TimeSpan.Zero)
                ? (float) (gameTime.ElapsedGameTime.TotalMilliseconds/time.TotalMilliseconds)
                : 1f)*direction;
            if ((direction >= 0 || TransitionPosition > 0.0) && (direction <= 0 || TransitionPosition < 1.0))
                return true;
            TransitionPosition = MathHelper.Clamp(TransitionPosition, 0.0f, 1f);
            return false;
        }

        public virtual void HandleInput(InputState input)
        {
        }

        public virtual void Draw(GameTime gameTime)
        {
        }

        public void ExitScreen()
        {
            if (TransitionOffTime == TimeSpan.Zero)
                ScreenManager.RemoveScreen(this);
            else
                IsExiting = true;
        }

        public virtual void inputMethodChanged(bool usingGamePad)
        {
        }
    }
}
// Decompiled with JetBrains decompiler
// Type: Hacknet.GuiData
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System.Collections.Generic;
using System.Text;
using Hacknet.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacknet
{
    public static class GuiData
    {
        public static Vector2 temp;
        private static Point tmpPoint;
        public static Color tmpColor = new Color();
        public static Rectangle tmpRect = new Rectangle();
        public static Color Default_Selected_Color = new Color(0, 166, 235);
        public static Color Default_Unselected_Color = new Color(byte.MaxValue, 128, 0);
        public static Color Default_Backing_Color = new Color(30, 30, 50, 100);
        public static Color Default_Light_Backing_Color = new Color(80, 80, 100, byte.MaxValue);
        public static Color Default_Lit_Backing_Color = new Color(byte.MaxValue, 199, 41, 100);
        public static Color Default_Dark_Neutral_Color = new Color(10, 10, 15, 200);
        public static Color Default_Dark_Background_Color = new Color(40, 40, 45, 180);
        public static Color Default_Trans_Grey = new Color(30, 30, 30, 100);
        public static Color Default_Trans_Grey_Bright = new Color(60, 60, 60, 100);
        public static Color Default_Trans_Grey_Dark = new Color(20, 20, 20, 200);
        public static Color Default_Trans_Grey_Strong = new Color(80, 80, 80, 100);
        public static Color Default_Trans_Grey_Solid = new Color(100, 100, 100, byte.MaxValue);
        public static int lastMouseWheelPos = -1;
        private static int lastMouseScroll;
        public static int hot = -1;
        public static int active = -1;
        public static int enganged = -1;
        public static bool blockingInput;
        public static bool blockingTextInput;
        public static bool willBlockTextInput;
        private static string keysPressedThisFrame = "";
        public static Vector2 scrollOffset = Vector2.Zero;
        public static float lastTimeStep = 0.016f;
        private static bool initialized;
        public static List<FontCongifOption> FontConfigs = new List<FontCongifOption>();
        public static FontCongifOption ActiveFontConfig;
        public static MouseState lastMouse;
        public static MouseState mouse;
        public static InputState lastInput;
        public static SpriteBatch spriteBatch;
        public static SpriteFont font;
        public static SpriteFont titlefont;
        public static SpriteFont smallfont;
        public static SpriteFont tinyfont;
        public static SpriteFont UITinyfont;
        public static SpriteFont UISmallfont;
        public static SpriteFont detailfont;
        private static TextInputHook TextInputHook;

        public static void InitFontOptions(ContentManager content)
        {
            var str = ActiveFontConfig.name;
            FontConfigs.Add(new FontCongifOption
            {
                name = "default",
                smallFont = content.Load<SpriteFont>("Font12"),
                tinyFont = content.Load<SpriteFont>("Font10"),
                bigFont = content.Load<SpriteFont>("Font23"),
                tinyFontCharHeight = 10f
            });
            if (string.IsNullOrEmpty(str))
                ActiveFontConfig = FontConfigs[0];
            FontConfigs.Add(new FontCongifOption
            {
                name = "medium",
                smallFont = content.Load<SpriteFont>("Font14"),
                tinyFont = content.Load<SpriteFont>("Font12"),
                bigFont = content.Load<SpriteFont>("Font23"),
                tinyFontCharHeight = 14f
            });
            FontConfigs.Add(new FontCongifOption
            {
                name = "large",
                smallFont = content.Load<SpriteFont>("Font16"),
                tinyFont = content.Load<SpriteFont>("Font14"),
                bigFont = content.Load<SpriteFont>("Font23"),
                tinyFontCharHeight = 16f
            });
            var flag = false;
            for (var index = 0; index < FontConfigs.Count; ++index)
            {
                if (FontConfigs[index].name == str)
                {
                    ActivateFontConfig(FontConfigs[index]);
                    flag = true;
                    break;
                }
            }
            if (flag)
                return;
            ActivateFontConfig(FontConfigs[0]);
        }

        public static void ActivateFontConfig(string configName)
        {
            for (var index = 0; index < FontConfigs.Count; ++index)
            {
                if (FontConfigs[index].name == configName)
                {
                    ActivateFontConfig(FontConfigs[index]);
                    break;
                }
            }
        }

        public static void ActivateFontConfig(FontCongifOption config)
        {
            smallfont = config.smallFont;
            tinyfont = config.tinyFont;
            font = config.bigFont;
            ActiveFontConfig = config;
        }

        public static void init(GameWindow window)
        {
            if (initialized)
                return;
            TextInputHook = new TextInputHook(window.Handle);
            initialized = true;
        }

        public static void doInput()
        {
            lastMouse = mouse;
            mouse = Mouse.GetState();
            if (lastMouseWheelPos == -1)
                lastMouseWheelPos = mouse.ScrollWheelValue;
            lastMouseScroll = lastMouseWheelPos - mouse.ScrollWheelValue;
            lastMouseWheelPos = mouse.ScrollWheelValue;
            blockingInput = false;
            blockingTextInput = willBlockTextInput;
            willBlockTextInput = false;
        }

        public static void doInput(InputState input)
        {
            doInput();
            lastInput = input;
        }

        public static void setTimeStep(float t)
        {
            lastTimeStep = t;
        }

        public static KeyboardState getKeyboadState()
        {
            return lastInput.CurrentKeyboardStates[0];
        }

        public static KeyboardState getLastKeyboadState()
        {
            return lastInput.LastKeyboardStates[0];
        }

        public static Vector2 getMousePos()
        {
            temp.X = mouse.X - scrollOffset.X;
            temp.Y = mouse.Y - scrollOffset.Y;
            return temp;
        }

        public static Point getMousePoint()
        {
            tmpPoint.X = mouse.X - (int) scrollOffset.X;
            tmpPoint.Y = mouse.Y - (int) scrollOffset.Y;
            return tmpPoint;
        }

        public static float getMouseWheelScroll()
        {
            return lastMouseScroll/120;
        }

        public static bool isMouseLeftDown()
        {
            return mouse.LeftButton == ButtonState.Pressed;
        }

        public static bool mouseLeftUp()
        {
            return lastMouse.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
        }

        public static bool mouseWasPressed()
        {
            return lastMouse.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed;
        }

        public static void startDraw()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        }

        public static void endDraw()
        {
            spriteBatch.End();
        }

        public static char[] getFilteredKeys()
        {
            var buffer = TextInputHook.Buffer;
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < buffer.Length; ++index)
            {
                if (buffer[index] >= 32 && buffer[index] <= 126)
                    stringBuilder.Append(buffer[index]);
            }
            TextInputHook.clearBuffer();
            var str = stringBuilder.ToString();
            var chArray = new char[str.Length];
            for (var index = 0; index < str.Length; ++index)
                chArray[index] = str[index];
            return chArray;
        }

        public struct FontCongifOption
        {
            public SpriteFont smallFont;
            public SpriteFont tinyFont;
            public SpriteFont bigFont;
            public string name;
            public float tinyFontCharHeight;
        }
    }
}
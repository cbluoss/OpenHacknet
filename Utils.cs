// Decompiled with JetBrains decompiler
// Type: Hacknet.Utils
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Hacknet.Effects;
using Hacknet.Misc;
using Hacknet.UIUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Hacknet
{
    internal static class Utils
    {
        public static float PARRALAX_MULTIPLIER = 1f;
        public static float MIN_DIFF_FOR_PARRALAX = 0.1f;
        public static Random random = new Random();
        public static byte[] byteBuffer = new byte[1];
        public static readonly string LevelStateFilename = "LevelState.lst";
        public static Color VeryDarkGray = new Color(22, 22, 22);
        public static Color SlightlyDarkGray = new Color(100, 100, 100);
        public static Color AddativeWhite = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
        public static Color AddativeRed = new Color(byte.MaxValue, 15, 15, 0);
        private static HSLColor hslColor = new HSLColor(1f, 1f, 1f);

        public static char[] newlineDelim = new char[1]
        {
            '\n'
        };

        public static string[] robustNewlineDelim = new string[2]
        {
            "\r\n",
            "\n"
        };

        public static char[] spaceDelim = new char[1]
        {
            ' '
        };

        public static string[] commaDelim = new string[3]
        {
            " ,",
            ", ",
            ","
        };

        public static string[] directorySplitterDelim = new string[2]
        {
            "/",
            "\\"
        };

        public static LCG LCG = new LCG(true);
        private static CollidableRectangle cacheRect;
        public static Texture2D white;
        public static Texture2D gradient;
        public static Texture2D gradientLeftRight;
        public static AudioEmitter emitter;
        public static Vector3 vec3;
        public static StorageDevice device;
        public static Color col;

        public static CollidableRectangle sect(CollidableRectangle first, CollidableRectangle second)
        {
            var checkable1 = first.checkable;
            var checkable2 = second.checkable;
            if (!checkable1.Intersects(checkable2))
                return CollidableRectangle.Zero;
            var rectangle = Rectangle.Intersect(checkable1, checkable2);
            return new CollidableRectangle
            {
                Width = rectangle.Width,
                Height = rectangle.Height,
                pos = new Vector2(rectangle.Location.X, rectangle.Location.Y)
            };
        }

        public static CollidableRectangle sect(Rectangle first, Rectangle second)
        {
            if (!first.Intersects(second))
                return CollidableRectangle.Zero;
            var rectangle = Rectangle.Intersect(first, second);
            return new CollidableRectangle
            {
                Width = rectangle.Width,
                Height = rectangle.Height,
                pos = new Vector2(rectangle.Location.X, rectangle.Location.Y)
            };
        }

        public static bool compareRects(CollidableRectangle first, CollidableRectangle second)
        {
            return first.pos.Equals(second.pos) && first.Width == (double) second.Width &&
                   first.Height == (double) second.Height;
        }

        public static Vector2 getParallax(Vector2 objectPosition, Vector2 CameraPosition, float objectDepth,
            float focusDepth)
        {
            if (objectDepth >= (double) focusDepth)
            {
                var num = objectDepth - (double) focusDepth > 0.100000001490116
                    ? (objectDepth - focusDepth)*PARRALAX_MULTIPLIER
                    : 0.0f*PARRALAX_MULTIPLIER;
                return new Vector2((CameraPosition.X - objectPosition.X)*num, 0.0f);
            }
            var num1 = objectDepth - (double) focusDepth < -0.0500000007450581
                ? (objectDepth - focusDepth)*PARRALAX_MULTIPLIER
                : 0.0f;
            return new Vector2(
                (float) ((CameraPosition.X - (double) objectPosition.X)*(num1 == 0.0 ? 0.0 : num1 - 1.0)), 0.0f);
        }

        public static void drawLine(SpriteBatch spriteBatch, Vector2 vector1, Vector2 vector2, Vector2 OffsetPosition,
            Color Colour, float Depth)
        {
            var texture = white;
            var x = Vector2.Distance(vector1, vector2);
            var rotation = (float) Math.Atan2(vector2.Y - (double) vector1.Y, vector2.X - (double) vector1.X);
            spriteBatch.Draw(texture, OffsetPosition + vector1, new Rectangle?(), Colour, rotation, Vector2.Zero,
                new Vector2(x, 1f), SpriteEffects.None, Depth);
        }

        public static bool keyPressed(InputState input, Keys key, PlayerIndex? player)
        {
            if (GuiData.blockingTextInput)
                return false;
            var index = 0;
            if (player.HasValue)
                index = (int) player.Value;
            var keyboardState1 = input.CurrentKeyboardStates[index];
            var keyboardState2 = input.LastKeyboardStates[index];
            return keyboardState1.IsKeyDown(key) && keyboardState2.IsKeyUp(key);
        }

        public static bool buttonPressed(InputState input, Buttons button, PlayerIndex? player)
        {
            var gamePadState1 = input.CurrentGamePadStates[(int) player.Value];
            var gamePadState2 = input.LastGamePadStates[(int) player.Value];
            return gamePadState1.IsButtonDown(button) && gamePadState2.IsButtonUp(button);
        }

        public static bool arraysAreTheSame(Keys[] a, Keys[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (var index = 0; index < a.Length; ++index)
            {
                if (a[index].CompareTo(b[index]) == 0)
                    return false;
            }
            return true;
        }

        public static bool arraysAreTheSame(Buttons[] a, Buttons[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (var index = 0; index < a.Length; ++index)
            {
                if (a[index].CompareTo(b[index]) == 0)
                    return false;
            }
            return true;
        }

        public static float rand(float range)
        {
            return (float) (random.NextDouble()*range - random.NextDouble()*range);
        }

        public static float randm(float range)
        {
            return (float) random.NextDouble()*range;
        }

        public static float rand()
        {
            return (float) random.NextDouble();
        }

        public static byte getRandomByte()
        {
            random.NextBytes(byteBuffer);
            return byteBuffer[0];
        }

        public static AudioEmitter emitterAtPosition(float x, float y)
        {
            if (emitter == null)
                emitter = new AudioEmitter();
            vec3.X = x;
            vec3.Y = y;
            vec3.Z = 0.0f;
            emitter.Position = vec3;
            return emitter;
        }

        public static AudioEmitter emitterAtPosition(float x, float y, float z)
        {
            if (emitter == null)
                emitter = new AudioEmitter();
            vec3.X = x;
            vec3.Y = y;
            vec3.Z = z;
            emitter.Position = vec3;
            return emitter;
        }

        public static Texture2D White(ContentManager content)
        {
            if (white == null || white.IsDisposed)
                white = TextureBank.load("Other\\white", content);
            return white;
        }

        public static Color makeColor(byte r, byte g, byte b, byte a)
        {
            col.R = r;
            col.G = g;
            col.B = b;
            col.A = a;
            return col;
        }

        public static Color makeColorAddative(Color c)
        {
            col.R = c.R;
            col.G = c.G;
            col.B = c.B;
            col.A = 0;
            return col;
        }

        public static bool rectEquals(CollidableRectangle rec1, CollidableRectangle rec2)
        {
            return Math.Abs(rec1.pos.X - rec2.pos.X) < 1.0/1000.0 && Math.Abs(rec1.pos.X - rec2.pos.X) < 1.0/1000.0 &&
                   (Math.Abs(rec1.Width - rec2.Width) < 1.0/1000.0 && Math.Abs(rec1.Height - rec2.Height) < 1.0/1000.0);
        }

        public static bool flipCoin()
        {
            return random.NextDouble() > 0.5;
        }

        public static byte randomCompType()
        {
            return flipCoin() ? (byte) 1 : (byte) 2;
        }

        public static void writeToFile(string data, string filename)
        {
            using (var streamWriter = new StreamWriter(filename))
            {
                streamWriter.Write(data);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        public static void SafeWriteToFile(string data, string filename)
        {
            var str = filename + ".tmp";
            if (!Directory.Exists(str))
                Directory.CreateDirectory(Path.GetDirectoryName(str));
            using (var streamWriter = new StreamWriter(str, false))
            {
                streamWriter.Write(data);
                streamWriter.Flush();
                streamWriter.Close();
            }
            if (File.Exists(filename))
                File.Delete(filename);
            File.Move(str, filename);
        }

        public static void SafeWriteToFile(byte[] data, string filename)
        {
            var str = filename + ".tmp";
            if (!Directory.Exists(str))
                Directory.CreateDirectory(Path.GetDirectoryName(str));
            using (var streamWriter = new StreamWriter(str, false))
            {
                streamWriter.Write(data);
                streamWriter.Flush();
                streamWriter.Close();
            }
            File.Delete(filename);
            File.Move(str, filename);
        }

        public static void appendToFile(string data, string filename)
        {
            var streamWriter = new StreamWriter(filename, true);
            streamWriter.Write(data);
            streamWriter.Close();
        }

        public static string readEntireFile(string filename)
        {
            var streamReader = new StreamReader(TitleContainer.OpenStream(filename));
            var str = streamReader.ReadToEnd();
            streamReader.Close();
            return str;
        }

        public static char getRandomLetter()
        {
            return Convert.ToChar(Convert.ToInt32(Math.Floor(26.0*random.NextDouble() + 65.0)));
        }

        public static char getRandomChar()
        {
            if (random.NextDouble() > 0.7)
                return string.Concat(Math.Min((int) Math.Floor((double) random.Next(0, 10)), 9))[0];
            return getRandomLetter();
        }

        public static char getRandomNumberChar()
        {
            return string.Concat(Math.Min((int) Math.Floor((double) random.Next(0, 10)), 9))[0];
        }

        public static Color convertStringToColor(string input)
        {
            var color = Color.White;
            var separator = new char[3]
            {
                ',',
                ' ',
                '/'
            };
            var strArray = input.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length < 3)
                return color;
            var num1 = byte.MaxValue;
            var num2 = byte.MaxValue;
            var num3 = byte.MaxValue;
            for (var index = 0; index < 3; ++index)
            {
                try
                {
                    var num4 = Convert.ToByte(strArray[index]);
                    switch (index)
                    {
                        case 0:
                            num1 = num4;
                            continue;
                        case 1:
                            num2 = num4;
                            continue;
                        case 2:
                            num3 = num4;
                            continue;
                        default:
                            continue;
                    }
                }
                catch (FormatException ex)
                {
                }
                catch (OverflowException ex)
                {
                }
            }
            color = new Color(num1, num2, num3);
            return color;
        }

        public static string convertColorToParseableString(Color c)
        {
            return c.R + "," + c.G + "," + c.B;
        }

        public static Rectangle getClipRectForSpritePos(Rectangle bounds, Texture2D tex, Vector2 pos, float scale)
        {
            var num1 = (int) (tex.Width*(double) scale);
            var num2 = (int) (tex.Height*(double) scale);
            int y;
            var x = y = 0;
            var width = tex.Width;
            var height = tex.Height;
            if (pos.X < (double) bounds.X)
                x += (int) (bounds.X - (double) pos.X);
            if (pos.Y < (double) bounds.Y)
                y += (int) (bounds.Y - (double) pos.Y);
            if (pos.X + (double) num1 > bounds.X + bounds.Width)
                width -= (int) ((pos.X + (double) num1 - (bounds.X + bounds.Width))*(1.0/scale));
            if (pos.Y + (double) num2 > bounds.Y + bounds.Height)
                height -= (int) ((pos.Y + (double) num2 - (bounds.Y + bounds.Height))*(1.0/scale));
            if (x > tex.Width)
            {
                x = tex.Width;
                width = 0;
            }
            if (y > tex.Height)
            {
                y = tex.Height;
                height = 0;
            }
            return new Rectangle(x, y, width, height);
        }

        public static string SmartTwimForWidth(string data, int width, SpriteFont font)
        {
            var strArray = data.Split(new string[2]
            {
                "\r\n",
                "\n"
            }, StringSplitOptions.None);
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                var text = strArray[index1];
                if (font.MeasureString(text).X > (double) width)
                {
                    var str1 = "";
                    for (var index2 = 0; index2 < text.Length; ++index2)
                    {
                        var flag = false;
                        switch (text[index2])
                        {
                            case '\t':
                            case ' ':
                                str1 += text[index2].ToString();
                                break;
                            default:
                                flag = true;
                                break;
                        }
                        if (flag)
                            break;
                    }
                    var list = new List<string>(text.Substring(str1.Length).Split(new string[1]
                    {
                        " "
                    }, StringSplitOptions.None));
                    var str2 = "";
                    var str3 = "";
                    while (list.Count > 0)
                    {
                        if (font.MeasureString(str3 + " " + list[0]).X > (double) width)
                        {
                            str2 = str2 + str1 + str3.Trim() + "\r\n";
                            str3 = "";
                        }
                        str3 = str3 + list[0] + " ";
                        list.RemoveAt(0);
                    }
                    strArray[index1] = str2 + str1 + str3.Trim() + "\r\n";
                }
            }
            var str = "";
            for (var index = 0; index < strArray.Length; ++index)
                str = str + strArray[index] + "\r\n";
            return str.Trim();
        }

        public static string SuperSmartTwimForWidth(string data, int width, SpriteFont font)
        {
            var strArray = data.Split(new string[2]
            {
                "\r\n",
                "\n"
            }, StringSplitOptions.None);
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                var text = strArray[index1];
                if (font.MeasureString(text).X > (double) width)
                {
                    var str1 = "";
                    for (var index2 = 0; index2 < text.Length; ++index2)
                    {
                        var flag = false;
                        switch (text[index2])
                        {
                            case '\t':
                            case ' ':
                                str1 += text[index2].ToString();
                                break;
                            default:
                                flag = true;
                                break;
                        }
                        if (flag)
                            break;
                    }
                    var list = new List<string>(text.Substring(str1.Length).Split(new string[1]
                    {
                        " "
                    }, StringSplitOptions.None));
                    var str2 = "";
                    var stringBuilder = new StringBuilder();
                    while (list.Count > 0)
                    {
                        if (font.MeasureString(stringBuilder + " " + list[0]).X > (double) width)
                        {
                            str2 = str2 + str1 + stringBuilder.ToString().Trim() + "\r\n";
                            stringBuilder.Clear();
                        }
                        var length1 = 1;
                        if (font.MeasureString(list[0]).X > (double) width)
                        {
                            var num1 = 0;
                            var num2 = 1;
                            if (font.MeasureString(list[0].Substring(0, length1)).X >= (double) width &&
                                font.MeasureString(list[0].Substring(0, length1 - 1)).X < (double) width)
                                num2 = length1;
                            int length2;
                            for (length2 = num2;
                                length2 < list[0].Length &&
                                font.MeasureString(list[0].Substring(0, length2)).X < (double) width;
                                ++length2)
                            {
                                var num3 = 40;
                                if (length2 + num3 < list[0].Length &&
                                    font.MeasureString(list[0].Substring(0, length2 + num3)).X < (double) width)
                                {
                                    length2 += num3;
                                    num1 += num3;
                                }
                            }
                            var num4 = length2 - 1;
                            if (num4 == 0)
                            {
                                stringBuilder.Append(list[0]);
                                stringBuilder.Append(" ");
                                list.RemoveAt(0);
                            }
                            else
                            {
                                var str3 = list[0];
                                stringBuilder.Append(list[0].Substring(0, num4));
                                stringBuilder.Append(" ");
                                list[0] = str3.Substring(num4);
                            }
                        }
                        else
                        {
                            stringBuilder.Append(list[0]);
                            stringBuilder.Append(" ");
                            list.RemoveAt(0);
                        }
                    }
                    strArray[index1] = str2 + str1 + stringBuilder.ToString().Trim() + "\r\n";
                }
            }
            var str = "";
            for (var index = 0; index < strArray.Length; ++index)
                str = str + strArray[index] + "\r\n";
            return str.Trim();
        }

        public static float QuadraticOutCurve(float point)
        {
            return (float) ((-100000000.0*point*(point - 2.0) - 1.0)/100000000.0);
        }

        public static float QuadraticInCurve(float point)
        {
            return 1E+08f*point*point*point*point;
        }

        public static RenderTarget2D GetCurrentRenderTarget()
        {
            var renderTargets = GuiData.spriteBatch.GraphicsDevice.GetRenderTargets();
            if (renderTargets.Length == 0)
                return null;
            return (RenderTarget2D) renderTargets[0].RenderTarget;
        }

        public static Rectangle InsetRectangle(Rectangle rect, int inset)
        {
            return new Rectangle(rect.X + inset, rect.Y + inset, rect.Width - inset*2, rect.Height - inset*2);
        }

        public static Vector2 GetNearestPointOnCircle(Vector2 point, Vector2 CircleCentre, float circleRadius)
        {
            var num1 = point.X - CircleCentre.X;
            var num2 = point.Y - CircleCentre.Y;
            var num3 = (float) Math.Sqrt(num1*(double) num1 + num2*(double) num2);
            return new Vector2(CircleCentre.X + num1/num3*circleRadius, CircleCentre.Y + num2/num3*circleRadius);
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < (double) min)
                val = min;
            if (val > (double) max)
                val = max;
            return val;
        }

        public static Vector2 Clamp(Vector2 val, float min, float max)
        {
            return new Vector2(Clamp(val.X, min, max), Clamp(val.Y, min, max));
        }

        public static string RandomFromArray(string[] array)
        {
            return array[random.Next(array.Length)];
        }

        public static string GetNonRepeatingFilename(string filename, string extension, Folder f)
        {
            var str = filename;
            var num = 0;
            bool flag;
            do
            {
                flag = true;
                for (var index = 0; index < f.files.Count; ++index)
                {
                    if (f.files[index].name == str + extension)
                    {
                        ++num;
                        str = string.Concat(filename, "(", num, ")");
                        flag = false;
                        break;
                    }
                }
            } while (!flag);
            return str + extension;
        }

        public static string FlipRandomChars(string original, double chancePerChar)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < original.Length; ++index)
            {
                if (random.NextDouble() < chancePerChar)
                    stringBuilder.Append(getRandomChar());
                else
                    stringBuilder.Append(original[index]);
            }
            return stringBuilder.ToString();
        }

        public static Vector3 ColorToVec3(Color c)
        {
            var num = c.A/(float) byte.MaxValue;
            return new Vector3(c.R/(float) byte.MaxValue*num, c.G/(float) byte.MaxValue*num,
                c.B/(float) byte.MaxValue*num);
        }

        public static Color AdditivizeColor(Color c)
        {
            col.R = c.R;
            col.G = c.G;
            col.B = c.B;
            col.A = 0;
            return col;
        }

        public static Vector2 RotatePoint(Vector2 point, float angle)
        {
            return PolarToCartesian(angle + GetPolarAngle(point), point.Length());
        }

        public static bool DebugGoFast()
        {
            if (Settings.debugCommandsEnabled)
                return GuiData.getKeyboadState().IsKeyDown(Keys.LeftAlt);
            return false;
        }

        public static void SendRealWorldEmail(string subject, string to, string body)
        {
            var from = new MailAddress("fractalalligatordev@gmail.com");
            var to1 = new MailAddress(to);
            using (var message = new MailMessage(from, to1)
            {
                Subject = subject,
                Body = body
            })
                new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(from.Address, "rgaekwivookrsfqg")
                }.Send(message);
        }

        public static string GenerateReportFromException(Exception ex)
        {
            var data1 = ex.GetType() + "\r\n\r\n" + ex.Message + "\r\n\r\nSource : " + ex.Source + "\r\n\r\n" +
                        ex.StackTrace + ex + "\r\n\r\n";
            if (ex.InnerException != null)
                data1 = data1 + "Inner : ---------------\r\n\r\n" +
                        GenerateReportFromException(ex.InnerException)
                            .Replace("\t", "\0")
                            .Replace("\r\n", "\r\n\t")
                            .Replace("\0", "\t") + "\r\n\r\n";
            var data2 = FileSanitiser.purifyStringForDisplay(data1);
            try
            {
                data2 = SuperSmartTwimForWidth(data2, 800, GuiData.smallfont);
            }
            catch (Exception ex1)
            {
            }
            return data2;
        }

        public static bool FloatEquals(float a, float b)
        {
            return Math.Abs(a - b) < 9.99999974737875E-05;
        }

        public static Vector2 PolarToCartesian(float angle, float magnitude)
        {
            return new Vector2(magnitude*(float) Math.Cos(angle), magnitude*(float) Math.Sin(angle));
        }

        public static float GetPolarAngle(Vector2 point)
        {
            return (float) Math.Atan2(point.Y, point.X);
        }

        public static Vector3 NormalizeRotationVector(Vector3 rot)
        {
            return new Vector3(rot.X%6.283185f, rot.Y%6.283185f, rot.Z%6.283185f);
        }

        public static void FillEverywhereExcept(Rectangle bounds, Rectangle fullscreen, SpriteBatch sb, Color col)
        {
            var destinationRectangle1 = new Rectangle(fullscreen.X, fullscreen.Y, bounds.X - fullscreen.X,
                fullscreen.Height);
            var destinationRectangle2 = new Rectangle(bounds.X, fullscreen.Y, bounds.Width, bounds.Y - fullscreen.Y);
            var destinationRectangle3 = new Rectangle(bounds.X, bounds.Y + bounds.Height, bounds.Width,
                fullscreen.Height - (bounds.Y + bounds.Height));
            var destinationRectangle4 = new Rectangle(bounds.X + bounds.Width, fullscreen.Y,
                fullscreen.Width - (bounds.X + bounds.Width), fullscreen.Height);
            sb.Draw(white, destinationRectangle1, col);
            sb.Draw(white, destinationRectangle4, col);
            sb.Draw(white, destinationRectangle2, col);
            sb.Draw(white, destinationRectangle3, col);
        }

        public static bool CheckStringIsRenderable(string input)
        {
            var str =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890,./!@#$%^&*()<>\\\":;{}_-+= \r\n?[]`~'|";
            for (var index = 0; index < input.Length; ++index)
            {
                if (!str.Contains(input[index]))
                {
                    Console.WriteLine("\r\n------------------\r\nInvalid Char : {" + input[index] +
                                      "}\r\n----------------------\r\n");
                    return false;
                }
            }
            return true;
        }

        public static void AppendToErrorFile(string text)
        {
            var path = "RuntimeErrors.txt";
            if (!File.Exists(path))
                File.WriteAllText(path, "Hacknet v" + MainMenu.OSVersion + " Runtime ErrorLog\r\n\r\n");
            using (var streamWriter = File.AppendText(path))
                streamWriter.WriteLine(text);
        }

        public static string[] SplitToTokens(string input)
        {
            return Regex.Matches(input, "[\\\"].+?[\\\"]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList().ToArray();
        }

        public static string[] SplitToTokens(string[] input)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < input.Length; ++index)
            {
                stringBuilder.Append(input[index]);
                stringBuilder.Append(" ");
            }
            return SplitToTokens(stringBuilder.ToString());
        }

        public static Color ColorFromHexString(string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            var num = uint.Parse(hexString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var white = Color.White;
            if (hexString.Length == 8)
            {
                white.A = (byte) (num >> 24);
                white.R = (byte) (num >> 16);
                white.G = (byte) (num >> 8);
                white.B = (byte) num;
            }
            else
            {
                if (hexString.Length != 6)
                    throw new InvalidOperationException("Invald hex representation of an ARGB or RGB color value.");
                white.R = (byte) (num >> 16);
                white.G = (byte) (num >> 8);
                white.B = (byte) num;
            }
            return white;
        }

        public static string ReadEntireContentsOfStream(Stream input)
        {
            var str = new StreamReader(input).ReadToEnd();
            input.Flush();
            input.Close();
            input.Dispose();
            return str;
        }

        public static void SendErrorEmail(Exception ex, string postfix = "", string extraData = "")
        {
            var body =
                string.Concat(
                    GenerateReportFromException(ex) + (object) "\r\n White:" + white + (object) "\r\n WhiteDisposed:" +
                    white.IsDisposed + (object) "\r\n SmallFont:" + GuiData.smallfont + (object) "\r\n TinyFont:" +
                    GuiData.tinyfont + "\r\n LineEffectTarget:" + FlickeringTextEffect.GetReportString() +
                    "\r\n PostProcessort stuff:" + PostProcessor.GetStatusReportString(), "\r\nRESOLUTION:\r\n ",
                    Game1.getSingleton().GraphicsDevice.PresentationParameters.BackBufferWidth, "x") +
                Game1.getSingleton().GraphicsDevice.PresentationParameters.BackBufferHeight + "\r\nFullscreen: " +
                (Game1.getSingleton().graphics.IsFullScreen ? "true" : "false") + "\r\n Adapter: " +
                Game1.getSingleton().GraphicsDevice.Adapter.Description + "\r\n Device Name: " +
                Game1.getSingleton().GraphicsDevice.Adapter.DeviceName + (object) "\r\n Status: " +
                Game1.getSingleton().GraphicsDevice.GraphicsDeviceStatus + "\r\n Extra:\r\n" + extraData + "\r\n";
            SendRealWorldEmail(
                "Hackent " + postfix + MainMenu.OSVersion + " Crash " + DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToShortTimeString(), "hacknetbugs+Hacknet@gmail.com", body);
        }

        public static void SendThreadedErrorReport(Exception ex, string postfix = "", string extraData = "")
        {
            new Thread(() => SendErrorEmail(ex, postfix, extraData))
            {
                IsBackground = true,
                Name = (postfix + "_Errorthread")
            }.Start();
        }

        public static Color GetComplimentaryColor(Color c)
        {
            hslColor = HSLColor.FromRGB(c);
            hslColor.Luminosity = Math.Max(0.4f, hslColor.Luminosity);
            hslColor.Saturation = Math.Max(0.4f, hslColor.Saturation);
            hslColor.Saturation = Math.Min(0.55f, hslColor.Saturation);
            hslColor.Hue -= 3.141593f;
            if (hslColor.Hue < 0.0)
                hslColor.Hue += 6.283185f;
            return hslColor.ToRGB();
        }

        public static void MeasureTimedSpeechSection()
        {
            var strArray = readEntireFile("Content/Post/BitSpeech.txt").Split(newlineDelim);
            var num1 = 0.0f;
            for (var index1 = 0; index1 < strArray.Length; ++index1)
            {
                var str = strArray[index1];
                var num2 = 0.0f;
                for (var index2 = 0; index2 < str.Length; ++index2)
                {
                    if (str[index2] == 35)
                        ++num2;
                    else if (str[index2] == 37)
                        num2 += 0.5f;
                    else
                        num2 += 0.05f;
                }
                Console.WriteLine("LINE " + index1 + ": " + num1 + "  --  " + num2 + "   : " + strArray[index1]);
                num1 += num2;
            }
        }

        public static Color HSL2RGB(double h, double sl, double l)
        {
            var num1 = l;
            var num2 = l;
            var num3 = l;
            var num4 = l <= 0.5 ? l*(1.0 + sl) : l + sl - l*sl;
            if (num4 > 0.0)
            {
                var num5 = l + l - num4;
                var num6 = (num4 - num5)/num4;
                h *= 6.0;
                var num7 = (int) h;
                var num8 = h - num7;
                var num9 = num4*num6*num8;
                var num10 = num5 + num9;
                var num11 = num4 - num9;
                switch (num7)
                {
                    case 0:
                        num1 = num4;
                        num2 = num10;
                        num3 = num5;
                        break;
                    case 1:
                        num1 = num11;
                        num2 = num4;
                        num3 = num5;
                        break;
                    case 2:
                        num1 = num5;
                        num2 = num4;
                        num3 = num10;
                        break;
                    case 3:
                        num1 = num5;
                        num2 = num11;
                        num3 = num4;
                        break;
                    case 4:
                        num1 = num10;
                        num2 = num5;
                        num3 = num4;
                        break;
                    case 5:
                        num1 = num4;
                        num2 = num5;
                        num3 = num11;
                        break;
                }
            }
            return new Color
            {
                R = Convert.ToByte(num1*byte.MaxValue),
                G = Convert.ToByte(num2*byte.MaxValue),
                B = Convert.ToByte(num3*byte.MaxValue),
                A = byte.MaxValue
            };
        }

        public static void RGB2HSL(Color rgb, out double h, out double s, out double l)
        {
            var val1 = rgb.R/(double) byte.MaxValue;
            var val2_1 = rgb.G/(double) byte.MaxValue;
            var val2_2 = rgb.B/(double) byte.MaxValue;
            h = 0.0;
            s = 0.0;
            l = 0.0;
            var num1 = Math.Max(Math.Max(val1, val2_1), val2_2);
            var num2 = Math.Min(Math.Min(val1, val2_1), val2_2);
            l = (num2 + num1)/2.0;
            if (l <= 0.0)
                return;
            var num3 = num1 - num2;
            s = num3;
            if (s <= 0.0)
                return;
            s /= l <= 0.5 ? num1 + num2 : 2.0 - num1 - num2;
            var num4 = (num1 - val1)/num3;
            var num5 = (num1 - val2_1)/num3;
            var num6 = (num1 - val2_2)/num3;
            h = val1 != num1
                ? (val2_1 != num1
                    ? (val1 == num2 ? 3.0 + num5 : 5.0 - num4)
                    : (val2_2 == num2 ? 1.0 + num4 : 3.0 - num6))
                : (val2_1 == num2 ? 5.0 + num6 : 1.0 - num5);
            h /= 6.0;
        }
    }
}
using System;
using System.Text;

namespace Hacknet
{
    public static class FileEncrypter
    {
        private static readonly string[] HeaderSplitDelimiters = new string[1]
        {
            "::"
        };

        public static string EncryptString(string data, string header, string ipLink, string pass = "",
            string fileExtension = null)
        {
            var passCodeFromString1 = GetPassCodeFromString(pass);
            var passCodeFromString2 = GetPassCodeFromString("");
            var stringBuilder = new StringBuilder();
            var str = "#DEC_ENC::" + Encrypt(header, passCodeFromString2) + "::" + Encrypt(ipLink, passCodeFromString2) +
                      "::" + Encrypt("ENCODED", passCodeFromString1);
            if (fileExtension != null)
                str = str + "::" + Encrypt(fileExtension, passCodeFromString2);
            stringBuilder.Append(str);
            stringBuilder.Append("\r\n");
            stringBuilder.Append(Encrypt(data, passCodeFromString1));
            return stringBuilder.ToString();
        }

        private static ushort GetPassCodeFromString(string code)
        {
            return (ushort) code.GetHashCode();
        }

        private static string Encrypt(string data, ushort passcode)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < data.Length; ++index)
            {
                var num = data[index]*1822 + short.MaxValue + passcode;
                stringBuilder.Append(num + " ");
            }
            return stringBuilder.ToString().Trim();
        }

        private static string Decrypt(string data, ushort passcode)
        {
            var stringBuilder = new StringBuilder();
            foreach (var str in data.Split(Utils.spaceDelim, StringSplitOptions.RemoveEmptyEntries))
            {
                var num = (Convert.ToInt32(str) - short.MaxValue - passcode)/1822;
                stringBuilder.Append((char) num);
            }
            return stringBuilder.ToString().Trim();
        }

        public static string[] DecryptString(string data, string pass = "")
        {
            var strArray1 = new string[6];
            var passCodeFromString1 = GetPassCodeFromString(pass);
            var passCodeFromString2 = GetPassCodeFromString("");
            var strArray2 = data.Split(Utils.robustNewlineDelim, StringSplitOptions.RemoveEmptyEntries);
            if (strArray2.Length < 2)
                throw new FormatException("Tried to decrypt an invalid valid DEC ENC file");
            var strArray3 = strArray2[0].Split(HeaderSplitDelimiters, StringSplitOptions.None);
            if (strArray3.Length < 4)
                throw new FormatException("Tried to decrypt an invalid valid DEC ENC file");
            var str1 = Decrypt(strArray3[1], passCodeFromString2);
            var str2 = Decrypt(strArray3[2], passCodeFromString2);
            var str3 = Decrypt(strArray3[3], passCodeFromString1);
            string str4 = null;
            if (strArray3.Length > 4)
                str4 = Decrypt(strArray3[4], passCodeFromString2);
            string str5 = null;
            var str6 = "1";
            if (str3 == "ENCODED")
                str5 = Decrypt(strArray2[1], passCodeFromString1);
            else
                str6 = "0";
            strArray1[0] = str1;
            strArray1[1] = str2;
            strArray1[2] = str5;
            strArray1[3] = str4;
            strArray1[4] = str6;
            strArray1[5] = str3;
            return strArray1;
        }

        public static string[] DecryptHeaders(string data, string pass = "")
        {
            var strArray1 = new string[3];
            var passCodeFromString = GetPassCodeFromString("");
            var strArray2 = data.Split(Utils.robustNewlineDelim, StringSplitOptions.RemoveEmptyEntries);
            if (strArray2.Length < 2)
                throw new FormatException("Tried to decrypt an invalid valid DEC ENC file");
            var strArray3 = strArray2[0].Split(HeaderSplitDelimiters, StringSplitOptions.None);
            if (strArray3.Length < 4)
                throw new FormatException("Tried to decrypt an invalid valid DEC ENC file");
            var str1 = Decrypt(strArray3[1], passCodeFromString);
            var str2 = Decrypt(strArray3[2], passCodeFromString);
            string str3 = null;
            if (strArray3.Length > 4)
                str3 = Decrypt(strArray3[4], passCodeFromString);
            strArray1[0] = str1;
            strArray1[1] = str2;
            strArray1[2] = str3;
            return strArray1;
        }

        public static int FileIsEncrypted(string data, string pass = "")
        {
            if (data.StartsWith("#DEC_ENC::"))
            {
                var strArray = DecryptString(data, pass);
                if (strArray[5] != "ENCODED")
                {
                    if (strArray[4] == "0")
                        return 2;
                }
                else
                {
                    if (strArray[4] == "0")
                        return 2;
                    if (strArray[4] == "1")
                        return 1;
                }
            }
            return 0;
        }

        public static string MakeReplacementsForDisplay(string input)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < input.Length; ++index)
            {
                if (GuiData.tinyfont.Characters.Contains(input[index]))
                    stringBuilder.Append(input[index]);
                else
                    stringBuilder.Append("_");
            }
            return stringBuilder.ToString();
        }
    }
}
using System.IO;

namespace Hacknet.UIUtils
{
    internal class AudioUtils
    {
        public static void openWav(string filename, out double[] left, out double[] right)
        {
            var numArray = File.ReadAllBytes(filename);
            int num1 = numArray[22];
            int index1;
            int index2;
            int num2;
            for (index1 = 12;
                numArray[index1] != 100 || numArray[index1 + 1] != 97 ||
                (numArray[index1 + 2] != 116 || numArray[index1 + 3] != 97);
                index1 = index2 + (4 + num2))
            {
                index2 = index1 + 4;
                num2 = numArray[index2] + numArray[index2 + 1]*256 + numArray[index2 + 2]*65536 +
                       numArray[index2 + 3]*16777216;
            }
            var index3 = index1 + 8;
            var length = (numArray.Length - index3)/2;
            if (num1 == 2)
                length /= 2;
            left = new double[length];
            right = num1 != 2 ? null : new double[length];
            var index4 = 0;
            while (index3 < numArray.Length)
            {
                left[index4] = bytesToDouble(numArray[index3], numArray[index3 + 1]);
                index3 += 2;
                if (num1 == 2)
                {
                    right[index4] = bytesToDouble(numArray[index3], numArray[index3 + 1]);
                    index3 += 2;
                }
                ++index4;
            }
        }

        private static double bytesToDouble(byte firstByte, byte secondByte)
        {
            return (short) (secondByte << 8 | firstByte)/32768.0;
        }
    }
}
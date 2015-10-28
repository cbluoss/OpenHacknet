using System.Collections.Generic;
using System.Text;

namespace Hacknet
{
    public static class VehicleInfo
    {
        public static List<VehicleType> vehicleTypes;

        public static void init()
        {
            var strArray1 = Utils.readEntireFile("Content/files/VehicleTypes.txt").Split(Utils.newlineDelim);
            var chArray = new char[1]
            {
                '#'
            };
            vehicleTypes = new List<VehicleType>(strArray1.Length);
            for (var index = 0; index < strArray1.Length; ++index)
            {
                var strArray2 = strArray1[index].Split(chArray);
                vehicleTypes.Add(new VehicleType(strArray2[0], strArray2[1]));
            }
        }

        public static VehicleRegistration getRandomRegistration()
        {
            var index1 = Utils.random.Next(vehicleTypes.Count);
            var vehicleType = vehicleTypes[index1];
            var plate = (Utils.getRandomLetter() + Utils.getRandomLetter() + Utils.getRandomLetter()).ToString() +
                        "-" + Utils.getRandomLetter() + Utils.getRandomLetter() + Utils.getRandomLetter();
            var stringBuilder = new StringBuilder();
            var num1 = 12;
            var num2 = 4;
            for (var index2 = 0; index2 < num1; ++index2)
            {
                if (index2%num2 == 0 && index2 > 0)
                    stringBuilder.Append('-');
                else
                    stringBuilder.Append(Utils.getRandomChar());
            }
            var regNumber = stringBuilder.ToString();
            return new VehicleRegistration(vehicleType, plate, regNumber);
        }
    }
}
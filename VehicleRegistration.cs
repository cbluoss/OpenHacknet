// Decompiled with JetBrains decompiler
// Type: Hacknet.VehicleRegistration
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet
{
    public struct VehicleRegistration
    {
        public VehicleType vehicle;
        public string licencePlate;
        public string licenceNumber;

        public VehicleRegistration(VehicleType vehicleType, string plate, string regNumber)
        {
            vehicle = vehicleType;
            licencePlate = plate;
            licenceNumber = regNumber;
        }

        public new string ToString()
        {
            return vehicle.maker + " " + vehicle.model + " | Plate: " + licencePlate + " Licence: " + licenceNumber;
        }
    }
}
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
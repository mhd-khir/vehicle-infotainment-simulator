
namespace InfotainmentSystem.Charging
{
    public class ChargingStationData
    {
        public string Name { get;}
        public float Distance { get; }
        public float? MaximumPowerKw { get; }

        /// <summary>
        /// Creates a new instance of ChargingStationData with the specified name, distance and maximum power.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="distance"></param>
        /// <param name="maximumPowerKw"></param>
        public ChargingStationData(string name, float distance, float? maximumPowerKw)
        {
            Name = name;
            Distance = distance;
            MaximumPowerKw = maximumPowerKw;
        }
    }
}

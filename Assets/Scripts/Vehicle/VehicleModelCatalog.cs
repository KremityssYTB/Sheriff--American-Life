using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    [Serializable]
    public sealed class VehicleModelEntry
    {
        public string manufacturer;
        public string model;
        public int year;
        public TrafficVehicleStyle style;
        [Range(0.1f, 10f)] public float trafficWeight = 1f;
        [Range(0.5f, 2f)] public float speedMultiplier = 1f;
        [Tooltip("Assign an original or properly licensed visual prefab. Leave empty to use the temporary procedural silhouette.")]
        public GameObject visualPrefab;

        public string DisplayName { get { return year + " " + manufacturer + " " + model; } }
    }

    [CreateAssetMenu(fileName = "VehicleModelCatalog", menuName = "Sheriff American Life/Vehicle Model Catalog")]
    public sealed class VehicleModelCatalog : ScriptableObject
    {
        public VehicleModelEntry[] vehicles = new VehicleModelEntry[0];

        public static VehicleModelEntry SelectRandom(VehicleModelCatalog catalog)
        {
            VehicleModelEntry[] source = catalog != null && catalog.vehicles != null && catalog.vehicles.Length > 0
                ? catalog.vehicles : BuiltInLineup;
            float total = 0f;
            for (int i = 0; i < source.Length; i++)
                if (source[i] != null) total += Mathf.Max(0.01f, source[i].trafficWeight);
            float roll = UnityEngine.Random.value * total;
            for (int i = 0; i < source.Length; i++)
            {
                VehicleModelEntry entry = source[i];
                if (entry == null) continue;
                roll -= Mathf.Max(0.01f, entry.trafficWeight);
                if (roll <= 0f) return entry;
            }
            return source[source.Length - 1];
        }

        // Representative historical lineup. Expand the asset catalog with additional
        // generations, trims and regional models; this is not an exhaustive vehicle database.
        private static readonly VehicleModelEntry[] BuiltInLineup =
        {
            Entry("Ford","Model T",1927,TrafficVehicleStyle.Sedan),
            Entry("Chevrolet","Bel Air",1957,TrafficVehicleStyle.Sedan),
            Entry("Ford","Thunderbird",1965,TrafficVehicleStyle.Muscle),
            Entry("Volkswagen","Beetle",1967,TrafficVehicleStyle.Sedan),
            Entry("Chevrolet","Camaro",1969,TrafficVehicleStyle.Muscle),
            Entry("Dodge","Charger",1969,TrafficVehicleStyle.Muscle),
            Entry("Ford","Mustang",1969,TrafficVehicleStyle.Muscle),
            Entry("Toyota","Corolla",1974,TrafficVehicleStyle.Sedan),
            Entry("Datsun","240Z",1973,TrafficVehicleStyle.Exotic),
            Entry("Chevrolet","C10",1972,TrafficVehicleStyle.Pickup),
            Entry("Jeep","CJ-7",1982,TrafficVehicleStyle.Suv),
            Entry("Toyota","Supra",1994,TrafficVehicleStyle.Muscle),
            Entry("Nissan","Skyline GT-R",1999,TrafficVehicleStyle.Muscle),
            Entry("Honda","Civic",1998,TrafficVehicleStyle.Sedan),
            Entry("Ford","Crown Victoria",2008,TrafficVehicleStyle.Sedan),
            Entry("Chevrolet","Tahoe",2008,TrafficVehicleStyle.Suv),
            Entry("Dodge","Challenger",2010,TrafficVehicleStyle.Muscle),
            Entry("Toyota","Camry",2012,TrafficVehicleStyle.Sedan),
            Entry("Ford","F-150",2015,TrafficVehicleStyle.Pickup),
            Entry("Dodge","Charger",2018,TrafficVehicleStyle.Muscle),
            Entry("Toyota","Tacoma",2019,TrafficVehicleStyle.Pickup),
            Entry("Toyota","RAV4",2020,TrafficVehicleStyle.Suv),
            Entry("Ford","Explorer",2021,TrafficVehicleStyle.Suv),
            Entry("Chevrolet","Corvette C8",2021,TrafficVehicleStyle.Exotic),
            Entry("Lamborghini","Huracan",2021,TrafficVehicleStyle.Exotic),
            Entry("Lamborghini","Urus",2022,TrafficVehicleStyle.Suv),
            Entry("Dodge","Durango",2023,TrafficVehicleStyle.Suv),
            Entry("Ford","Mustang",2024,TrafficVehicleStyle.Muscle),
            Entry("Toyota","Prius",2024,TrafficVehicleStyle.Sedan),
            Entry("Honda","Accord",2024,TrafficVehicleStyle.Sedan),
            Entry("Tesla","Model 3",2024,TrafficVehicleStyle.Sedan),
            Entry("Lamborghini","Revuelto",2024,TrafficVehicleStyle.Exotic),
            Entry("Ford","F-150 Lightning",2024,TrafficVehicleStyle.Pickup),
            Entry("Chevrolet","Silverado",2025,TrafficVehicleStyle.Pickup),
            Entry("Toyota","Camry",2025,TrafficVehicleStyle.Sedan),
            Entry("Dodge","Charger Daytona",2025,TrafficVehicleStyle.Muscle),
            Entry("Lamborghini","Temerario",2025,TrafficVehicleStyle.Exotic)
        };

        private static VehicleModelEntry Entry(string make, string model, int year, TrafficVehicleStyle style)
        {
            return new VehicleModelEntry {
                manufacturer = make, model = model, year = year, style = style,
                trafficWeight = style == TrafficVehicleStyle.Exotic ? 0.25f : 1f,
                speedMultiplier = style == TrafficVehicleStyle.Pickup || style == TrafficVehicleStyle.Suv ? 0.92f : 1f
            };
        }
    }
}

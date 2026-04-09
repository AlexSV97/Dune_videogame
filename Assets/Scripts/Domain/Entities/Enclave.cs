using System;
using System.Collections.Generic;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Domain.Entities
{
    [Serializable]
    public class Enclave
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public EnclaveType Type { get; set; }
        public HabitatType Habitat { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double InfluenceRadius { get; set; }
        public double DefenseBonus { get; set; }
        public double ProductionBonus { get; set; }
        public List<string> OwnedFacilities { get; set; } = new();
        public List<string> GarrisonedCreatures { get; set; } = new();
        public Dictionary<ResourceType, double> BaseProduction { get; set; } = new();
        public Dictionary<ResourceType, double> MonthlyUpkeep { get; set; } = new();
        public EnclaveStatus Status { get; set; } = EnclaveStatus.Active;
        public bool IsPlayerControlled { get; set; }
        public double Loyalty { get; set; } = 100;
        public double MaxLoyalty { get; set; } = 100;

        public Enclave() { }

        public Enclave(string id, string name, EnclaveType type, HabitatType habitat, double x, double y, double z)
        {
            Id = id;
            Name = name;
            Type = type;
            Habitat = habitat;
            PositionX = x;
            PositionY = y;
            PositionZ = z;
            SetBaseStats();
        }

        private void SetBaseStats()
        {
            (InfluenceRadius, DefenseBonus, ProductionBonus) = Type switch
            {
                EnclaveType.Sietch => (500, 0.3, 0.2),
                EnclaveType.ImperialOutpost => (300, 0.5, 0.1),
                EnclaveType.HarkonnenRefinery => (400, 0.2, 0.4),
                EnclaveType.SmugglerCamp => (200, 0.1, 0.3),
                EnclaveType.GreatHouseEstate => (600, 0.4, 0.25),
                _ => (300, 0.2, 0.2)
            };

            BaseProduction = Type switch
            {
                EnclaveType.Sietch => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Water, 50 },
                    { ResourceType.Knowledge, 10 }
                },
                EnclaveType.ImperialOutpost => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Credits, 100 }
                },
                EnclaveType.HarkonnenRefinery => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Spice, 200 },
                    { ResourceType.Credits, 150 }
                },
                EnclaveType.SmugglerCamp => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Credits, 75 },
                    { ResourceType.Spice, 50 }
                },
                EnclaveType.GreatHouseEstate => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Credits, 200 },
                    { ResourceType.Population, 20 }
                },
                _ => new Dictionary<ResourceType, double>()
            };

            MonthlyUpkeep = Type switch
            {
                EnclaveType.Sietch => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Water, 20 }
                },
                EnclaveType.HarkonnenRefinery => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Water, 50 },
                    { ResourceType.Credits, 30 }
                },
                _ => new Dictionary<ResourceType, double>()
            };
        }

        public double CalculateDefensePower()
        {
            double baseDefense = 100;
            foreach (var creatureId in GarrisonedCreatures)
            {
                baseDefense += 50;
            }
            return baseDefense * (1 + DefenseBonus);
        }

        public void ModifyLoyalty(double change)
        {
            Loyalty = Math.Clamp(Loyalty + change, 0, MaxLoyalty);
            if (Loyalty <= 0)
            {
                Status = EnclaveStatus.Rebellion;
            }
        }
    }

    public enum EnclaveStatus
    {
        Active,
        UnderSiege,
        Destroyed,
        Rebellion,
        Conquered
    }
}

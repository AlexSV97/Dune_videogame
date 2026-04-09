using System;
using System.Collections.Generic;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Domain.Entities
{
    [Serializable]
    public class Facility
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FacilityType Type { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public string ParentEnclaveId { get; set; }
        public double Efficiency { get; set; } = 1.0;
        public double MaxEfficiency { get; set; } = 2.0;
        public double Health { get; set; }
        public double MaxHealth { get; set; }
        public double ConstructionCost { get; set; }
        public double MonthlyUpkeep { get; set; }
        public Dictionary<ResourceType, double> ProductionPerMonth { get; set; } = new();
        public Dictionary<ResourceType, double> ConsumptionPerMonth { get; set; } = new();
        public bool IsOperational { get; set; } = true;
        public int Level { get; set; } = 1;
        public int MaxLevel { get; set; } = 5;
        public List<string> RequiredResearch { get; set; } = new();
        public double UpgradeCostMultiplier { get; set; } = 1.5;

        public Facility() { }

        public Facility(string id, string name, FacilityType type)
        {
            Id = id;
            Name = name;
            Type = type;
            MaxHealth = GetBaseMaxHealth(type);
            Health = MaxHealth;
            SetProductionProfile();
        }

        private void SetProductionProfile()
        {
            (ConstructionCost, MonthlyUpkeep, MaxLevel) = Type switch
            {
                FacilityType.SpiceRefinery => (5000, 100, 5),
                FacilityType.WindTrap => (1500, 20, 3),
                FacilityType.WaterExtractor => (3000, 50, 4),
                FacilityType.DefenseGrid => (4000, 80, 3),
                FacilityType.TrainingCamp => (2500, 60, 4),
                FacilityType.ResearchLab => (6000, 120, 5),
                FacilityType.TradePost => (2000, 30, 3),
                FacilityType.WormSanctuary => (10000, 200, 3),
                _ => (1000, 25, 3)
            };

            ProductionPerMonth = Type switch
            {
                FacilityType.SpiceRefinery => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Spice, 100 }
                },
                FacilityType.WindTrap => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Water, 30 }
                },
                FacilityType.WaterExtractor => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Water, 80 }
                },
                FacilityType.TradePost => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Credits, 150 }
                },
                FacilityType.ResearchLab => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Knowledge, 20 }
                },
                _ => new Dictionary<ResourceType, double>()
            };

            ConsumptionPerMonth = Type switch
            {
                FacilityType.SpiceRefinery => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Water, 40 }
                },
                FacilityType.WaterExtractor => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Credits, 50 }
                },
                FacilityType.TrainingCamp => new Dictionary<ResourceType, double>
                {
                    { ResourceType.Water, 30 },
                    { ResourceType.Credits, 25 }
                },
                _ => new Dictionary<ResourceType, double>()
            };
        }

        private static double GetBaseMaxHealth(FacilityType type) => type switch
        {
            FacilityType.SpiceRefinery => 500,
            FacilityType.WindTrap => 200,
            FacilityType.WaterExtractor => 300,
            FacilityType.DefenseGrid => 400,
            FacilityType.TrainingCamp => 250,
            FacilityType.ResearchLab => 350,
            FacilityType.TradePost => 150,
            FacilityType.WormSanctuary => 600,
            _ => 200
        };

        public Dictionary<ResourceType, double> CalculateNetProduction()
        {
            var netProduction = new Dictionary<ResourceType, double>();
            
            if (!IsOperational) return netProduction;

            foreach (var production in ProductionPerMonth)
            {
                double amount = production.Value * Efficiency * (1 + (Level - 1) * 0.25);
                netProduction[production.Key] = amount;
            }

            foreach (var consumption in ConsumptionPerMonth)
            {
                if (netProduction.ContainsKey(consumption.Key))
                    netProduction[consumption.Key] -= consumption.Value * Efficiency;
                else
                    netProduction[consumption.Key] = -consumption.Value * Efficiency;
            }

            return netProduction;
        }

        public double GetUpgradeCost()
        {
            if (Level >= MaxLevel) return 0;
            return ConstructionCost * UpgradeCostMultiplier * Level;
        }

        public bool CanUpgrade()
        {
            return Level < MaxLevel && IsOperational;
        }

        public void Upgrade()
        {
            if (CanUpgrade())
            {
                Level++;
                MaxHealth += 50;
                Health = MaxHealth;
                Efficiency = Math.Min(MaxEfficiency, Efficiency + 0.1);
            }
        }

        public void TakeDamage(double damage)
        {
            Health = Math.Max(0, Health - damage);
            if (Health <= 0)
            {
                IsOperational = false;
            }
            else if (Health < MaxHealth * 0.25)
            {
                Efficiency = Math.Max(0.5, Efficiency - 0.2);
            }
        }

        public void Repair()
        {
            Health = MaxHealth;
            Efficiency = 1.0;
            IsOperational = true;
        }
    }
}

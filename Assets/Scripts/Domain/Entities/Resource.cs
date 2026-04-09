using System;
using System.Collections.Generic;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Domain.Entities
{
    [Serializable]
    public class Resource
    {
        public ResourceType Type { get; set; }
        public double Amount { get; set; }
        public double MaxCapacity { get; set; }
        public double ProductionRate { get; set; }
        public double ConsumptionRate { get; set; }

        public Resource() { }

        public Resource(ResourceType type, double amount, double maxCapacity = 10000)
        {
            Type = type;
            Amount = amount;
            MaxCapacity = maxCapacity;
            ProductionRate = 0;
            ConsumptionRate = 0;
        }

        public double NetChange => ProductionRate - ConsumptionRate;

        public void AddAmount(double amount)
        {
            Amount = Math.Min(Amount + amount, MaxCapacity);
        }

        public void Consume(double amount)
        {
            Amount = Math.Max(0, Amount - amount);
        }
    }

    [Serializable]
    public class Inventory
    {
        public Dictionary<ResourceType, Resource> Resources { get; set; } = new();
        public List<string> OwnedFacilities { get; set; } = new();
        public List<string> OwnedCreatures { get; set; } = new();
        public List<string> ResearchUnlocked { get; set; } = new();

        public Inventory()
        {
            InitializeResources();
        }

        private void InitializeResources()
        {
            Resources[ResourceType.Spice] = new Resource(ResourceType.Spice, 1000, 50000);
            Resources[ResourceType.Water] = new Resource(ResourceType.Water, 500, 25000);
            Resources[ResourceType.Credits] = new Resource(ResourceType.Credits, 5000, 100000);
            Resources[ResourceType.Knowledge] = new Resource(ResourceType.Knowledge, 0, 1000);
            Resources[ResourceType.Population] = new Resource(ResourceType.Population, 100, 5000);
        }

        public bool TryGetResource(ResourceType type, out Resource resource)
        {
            return Resources.TryGetValue(type, out resource);
        }

        public bool HasResource(ResourceType type, double requiredAmount)
        {
            return Resources.TryGetValue(type, out var resource) && resource.Amount >= requiredAmount;
        }

        public void SpendResource(ResourceType type, double amount)
        {
            if (Resources.TryGetValue(type, out var resource))
            {
                resource.Consume(amount);
            }
        }

        public void AddResource(ResourceType type, double amount)
        {
            if (Resources.TryGetValue(type, out var resource))
            {
                resource.AddAmount(amount);
            }
        }
    }
}

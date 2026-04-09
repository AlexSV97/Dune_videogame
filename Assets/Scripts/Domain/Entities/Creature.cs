using System;
using System.Collections.Generic;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Domain.Entities
{
    [Serializable]
    public class Creature
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CreatureType Type { get; set; }
        public DietType Diet { get; set; }
        public double Health { get; set; }
        public double MaxHealth { get; set; }
        public double AttackPower { get; set; }
        public double DefensePower { get; set; }
        public double Speed { get; set; }
        public double MaintenanceCost { get; set; }
        public HabitatType PreferredHabitat { get; set; }
        public bool IsTamed { get; set; }
        public int TamingProgress { get; set; }
        public Dictionary<ResourceType, double> ResourceProduction { get; set; } = new();

        public Creature() { }

        public Creature(string id, string name, CreatureType type)
        {
            Id = id;
            Name = name;
            Type = type;
            MaxHealth = GetBaseMaxHealth(type);
            Health = MaxHealth;
            AttackPower = GetBaseAttack(type);
            DefensePower = GetBaseDefense(type);
            Speed = GetBaseSpeed(type);
            MaintenanceCost = GetMaintenanceCost(type);
            Diet = GetDiet(type);
            IsTamed = false;
            TamingProgress = 0;
        }

        private static double GetBaseMaxHealth(CreatureType type) => type switch
        {
            CreatureType.Sandworm => 5000,
            CreatureType.Sandtrout => 500,
            CreatureType.FremenRider => 150,
            CreatureType.Sardaukar => 180,
            CreatureType.Ornithopter => 100,
            CreatureType.Dewback => 120,
            _ => 100
        };

        private static double GetBaseAttack(CreatureType type) => type switch
        {
            CreatureType.Sandworm => 500,
            CreatureType.Sandtrout => 50,
            CreatureType.FremenRider => 40,
            CreatureType.Sardaukar => 50,
            CreatureType.Ornithopter => 30,
            CreatureType.Dewback => 25,
            _ => 20
        };

        private static double GetBaseDefense(CreatureType type) => type switch
        {
            CreatureType.Sandworm => 400,
            CreatureType.Sandtrout => 100,
            CreatureType.FremenRider => 20,
            CreatureType.Sardaukar => 45,
            CreatureType.Ornithopter => 15,
            CreatureType.Dewback => 30,
            _ => 15
        };

        private static double GetBaseSpeed(CreatureType type) => type switch
        {
            CreatureType.Sandworm => 2,
            CreatureType.Sandtrout => 0.5f,
            CreatureType.FremenRider => 8,
            CreatureType.Sardaukar => 6,
            CreatureType.Ornithopter => 15,
            CreatureType.Dewback => 5,
            _ => 5
        };

        private static double GetMaintenanceCost(CreatureType type) => type switch
        {
            CreatureType.Sandworm => 100,
            CreatureType.Sandtrout => 20,
            CreatureType.FremenRider => 30,
            CreatureType.Sardaukar => 50,
            CreatureType.Ornithopter => 40,
            CreatureType.Dewback => 15,
            _ => 10
        };

        private static DietType GetDiet(CreatureType type) => type switch
        {
            CreatureType.Sandworm => DietType.Spices feeder (Sandworm),
            CreatureType.Sandtrout => DietType.Spices feeder (Sandworm),
            CreatureType.FremenRider => DietType.Omnivore,
            CreatureType.Sardaukar => DietType.Carnivore,
            CreatureType.Ornithopter => DietType.Herbivore,
            CreatureType.Dewback => DietType.Herbivore,
            _ => DietType.Omnivore
        };

        public void TakeDamage(double damage)
        {
            double actualDamage = Math.Max(1, damage - DefensePower * 0.2);
            Health = Math.Max(0, Health - actualDamage);
        }

        public bool IsAlive => Health > 0;

        public void Heal(double amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
        }
    }
}

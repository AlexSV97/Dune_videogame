using NUnit.Framework;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Tests
{
    [TestFixture]
    public class DomainEntityTests
    {
        [Test]
        public void Resource_AddAmount_RespectsMaxCapacity()
        {
            var resource = new Resource(ResourceType.Spice, 100, 200);
            
            resource.AddAmount(150);
            
            Assert.AreEqual(200, resource.Amount);
        }

        [Test]
        public void Resource_Consume_DoesNotGoBelowZero()
        {
            var resource = new Resource(ResourceType.Water, 50, 100);
            
            resource.Consume(100);
            
            Assert.AreEqual(0, resource.Amount);
        }

        [Test]
        public void Resource_NetChange_CalculatesCorrectly()
        {
            var resource = new Resource(ResourceType.Credits, 1000, 10000);
            resource.ProductionRate = 100;
            resource.ConsumptionRate = 30;
            
            Assert.AreEqual(70, resource.NetChange);
        }

        [Test]
        public void Creature_Initialization_SetsCorrectDefaults()
        {
            var creature = new Creature("c1", "Test Worm", CreatureType.Sandworm);
            
            Assert.AreEqual(CreatureType.Sandworm, creature.Type);
            Assert.AreEqual("Test Worm", creature.Name);
            Assert.IsFalse(creature.IsTamed);
            Assert.AreEqual(0, creature.TamingProgress);
            Assert.IsTrue(creature.IsAlive);
        }

        [Test]
        public void Creature_TakeDamage_RespectsDefense()
        {
            var creature = new Creature("c1", "Sardaukar", CreatureType.Sardaukar);
            double initialHealth = creature.Health;
            double damage = 50;
            
            creature.TakeDamage(damage);
            
            double actualDamage = initialHealth - creature.Health;
            Assert.Less(actualDamage, damage);
            Assert.Greater(actualDamage, 0);
        }

        [Test]
        public void Creature_Heal_DoesNotExceedMaxHealth()
        {
            var creature = new Creature("c1", "Dewback", CreatureType.Dewback);
            creature.Health = 50;
            
            creature.Heal(100);
            
            Assert.AreEqual(creature.MaxHealth, creature.Health);
        }

        [Test]
        public void Creature_IsAlive_ReturnsFalseWhenDead()
        {
            var creature = new Creature("c1", "Ornithopter", CreatureType.Ornithopter);
            creature.TakeDamage(creature.MaxHealth + 1);
            
            Assert.IsFalse(creature.IsAlive);
        }

        [Test]
        public void Enclave_Initialization_SetsCorrectStats()
        {
            var enclave = new Enclave("e1", "Tabr", EnclaveType.Sietch, HabitatType.DesertShallow, 0, 0, 0);
            
            Assert.AreEqual(EnclaveType.Sietch, enclave.Type);
            Assert.AreEqual(100, enclave.MaxLoyalty);
            Assert.AreEqual(EnclaveStatus.Active, enclave.Status);
        }

        [Test]
        public void Enclave_ModifyLoyalty_RespectsBounds()
        {
            var enclave = new Enclave("e1", "Test", EnclaveType.Sietch, HabitatType.DesertShallow, 0, 0, 0);
            
            enclave.ModifyLoyalty(200);
            Assert.AreEqual(enclave.MaxLoyalty, enclave.Loyalty);
            
            enclave.ModifyLoyalty(-200);
            Assert.AreEqual(0, enclave.Loyalty);
        }

        [Test]
        public void Enclave_CalculateDefensePower_IncludesGarrison()
        {
            var enclave = new Enclave("e1", "Test", EnclaveType.ImperialOutpost, HabitatType.Settlement, 0, 0, 0);
            enclave.GarrisonedCreatures.Add("creature1");
            enclave.GarrisonedCreatures.Add("creature2");
            
            double defense = enclave.CalculateDefensePower();
            
            Assert.Greater(defense, 100);
        }

        [Test]
        public void Facility_Initialization_SetsCorrectDefaults()
        {
            var facility = new Facility("f1", "Test Refinery", FacilityType.SpiceRefinery);
            
            Assert.AreEqual(FacilityType.SpiceRefinery, facility.Type);
            Assert.IsTrue(facility.IsOperational);
            Assert.AreEqual(1, facility.Level);
        }

        [Test]
        public void Facility_CalculateNetProduction_IncludesEfficiency()
        {
            var facility = new Facility("f1", "Test", FacilityType.SpiceRefinery);
            facility.Efficiency = 0.5;
            
            var netProduction = facility.CalculateNetProduction();
            
            Assert.True(netProduction.ContainsKey(ResourceType.Spice));
            Assert.Less(netProduction[ResourceType.Spice], facility.ProductionPerMonth[ResourceType.Spice]);
        }

        [Test]
        public void Facility_Upgrade_IncreasesLevelAndStats()
        {
            var facility = new Facility("f1", "Test", FacilityType.SpiceRefinery);
            int initialLevel = facility.Level;
            double initialMaxHealth = facility.MaxHealth;
            
            facility.Upgrade();
            
            Assert.Greater(facility.Level, initialLevel);
            Assert.Greater(facility.MaxHealth, initialMaxHealth);
        }

        [Test]
        public void Facility_CannotUpgradeBeyondMaxLevel()
        {
            var facility = new Facility("f1", "Test", FacilityType.WindTrap);
            
            for (int i = 0; i < 10; i++)
            {
                facility.Upgrade();
            }
            
            Assert.LessOrEqual(facility.Level, facility.MaxLevel);
        }

        [Test]
        public void Facility_TakeDamage_Below25Percent_LowersEfficiency()
        {
            var facility = new Facility("f1", "Test", FacilityType.SpiceRefinery);
            facility.Efficiency = 1.0;
            facility.Health = facility.MaxHealth * 0.2;
            
            facility.TakeDamage(1);
            
            Assert.Less(facility.Efficiency, 1.0);
        }

        [Test]
        public void GameState_Initialization_CreatesDefaultState()
        {
            var gameState = new GameState("Player1", DifficultyLevel.Veteran);
            
            Assert.AreEqual("Player1", gameState.PlayerName);
            Assert.AreEqual(DifficultyLevel.Veteran, gameState.Difficulty);
            Assert.AreEqual(1, gameState.CurrentMonth);
            Assert.AreEqual(10256, gameState.CurrentYear);
            Assert.IsNotNull(gameState.Inventory);
        }

        [Test]
        public void GameState_ApplyDifficultySettings_ModifiesGlobals()
        {
            var beginner = new GameState("Test", DifficultyLevel.Beginner);
            var messiah = new GameState("Test", DifficultyLevel.Messiah);
            
            double beginnerBonus = beginner.GlobalModifiers["ProductionBonus"];
            double messiahBonus = messiah.GlobalModifiers["ProductionBonus"];
            
            Assert.Greater(beginnerBonus, messiahBonus);
        }

        [Test]
        public void GameState_AddFacility_AssignsCorrectId()
        {
            var gameState = new GameState("Test", DifficultyLevel.Standard);
            var enclave = gameState.Enclaves[0];
            var facility = new Facility("f1", "Test", FacilityType.WindTrap);
            
            gameState.AddFacility(facility, enclave.Id);
            
            Assert.True(facility.Id.StartsWith("WindTrap_"));
            Assert.Contains(facility.Id, enclave.OwnedFacilities);
        }

        [Test]
        public void GameState_AdvanceMonth_HandlesYearTransition()
        {
            var gameState = new GameState("Test", DifficultyLevel.Standard);
            gameState.CurrentMonth = 12;
            
            gameState.AdvanceMonth();
            
            Assert.AreEqual(1, gameState.CurrentMonth);
            Assert.AreEqual(10257, gameState.CurrentYear);
        }

        [Test]
        public void GameState_Clone_CreatesIndependentCopy()
        {
            var original = new GameState("Test", DifficultyLevel.Standard);
            original.Inventory.AddResource(ResourceType.Spice, 500);
            
            var clone = original.Clone();
            clone.Inventory.SpendResource(ResourceType.Spice, 100);
            
            Assert.AreNotEqual(
                original.Inventory.Resources[ResourceType.Spice].Amount,
                clone.Inventory.Resources[ResourceType.Spice].Amount
            );
        }

        [Test]
        public void SimulationEvent_GenerateRandomEvent_CreatesValidEvent()
        {
            var evt = SimulationEvent.GenerateRandomEvent(5);
            
            Assert.IsNotNull(evt);
            Assert.AreEqual(5, evt.MonthTriggered);
            Assert.IsFalse(evt.IsResolved);
            Assert.Greater(evt.ResourceImpact.Count, 0);
        }

        [Test]
        public void EventChoice_CanAfford_ReturnsTrueWhenSufficientResources()
        {
            var inventory = new Inventory();
            var choice = new EventChoice("Test", 0, new System.Collections.Generic.Dictionary<ResourceType, double>
            {
                { ResourceType.Credits, 100 }
            });
            
            Assert.IsTrue(choice.CanAfford(inventory));
        }

        [Test]
        public void EventChoice_CanAfford_ReturnsFalseWhenInsufficientResources()
        {
            var inventory = new Inventory();
            inventory.SpendResource(ResourceType.Credits, 5000);
            
            var choice = new EventChoice("Test", 0, new System.Collections.Generic.Dictionary<ResourceType, double>
            {
                { ResourceType.Credits, 1000 }
            });
            
            Assert.IsFalse(choice.CanAfford(inventory));
        }

        [Test]
        public void Inventory_HasResource_ReturnsCorrectResult()
        {
            var inventory = new Inventory();
            
            Assert.IsTrue(inventory.HasResource(ResourceType.Spice, 500));
            Assert.IsFalse(inventory.HasResource(ResourceType.Spice, 2000));
        }

        [Test]
        public void Inventory_SpendResource_DoesNotGoNegative()
        {
            var inventory = new Inventory();
            
            inventory.SpendResource(ResourceType.Water, 1000);
            
            Assert.GreaterOrEqual(inventory.Resources[ResourceType.Water].Amount, 0);
        }
    }
}

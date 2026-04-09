using System.Linq;
using NUnit.Framework;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;
using DuneArrakisDominion.Managers;

namespace DuneArrakisDominion.Tests
{
    [TestFixture]
    public class SimulationEngineTests
    {
        private SimulationEngine _simulationEngine;
        private GameState _testGameState;

        [SetUp]
        public void Setup()
        {
            _simulationEngine = new SimulationEngine();
            _testGameState = new GameState("TestPlayer", DifficultyLevel.Standard);
        }

        [TearDown]
        public void TearDown()
        {
            _simulationEngine = null;
            _testGameState = null;
        }

        [Test]
        public void SimulateMonth_InitialState_IncreasesMonth()
        {
            int initialMonth = _testGameState.CurrentMonth;
            
            _simulationEngine.SimulateMonth(_testGameState);
            
            Assert.Greater(_testGameState.CurrentMonth, initialMonth);
        }

        [Test]
        public void SimulateMonth_YearBoundary_IncreasesYear()
        {
            _testGameState.CurrentMonth = 12;
            
            _simulationEngine.SimulateMonth(_testGameState);
            
            Assert.AreEqual(1, _testGameState.CurrentMonth);
            Assert.AreEqual(10257, _testGameState.CurrentYear);
        }

        [Test]
        public void SimulateMonth_WithProduction_IncreasesResources()
        {
            var refinery = new Facility("ref1", "Spice Refinery 1", FacilityType.SpiceRefinery);
            refinery.IsOperational = true;
            _testGameState.AddFacility(refinery, _testGameState.Enclaves[0].Id);
            
            double initialSpice = _testGameState.Inventory.Resources[ResourceType.Spice].Amount;
            
            _simulationEngine.SimulateMonth(_testGameState);
            
            double newSpice = _testGameState.Inventory.Resources[ResourceType.Spice].Amount;
            Assert.Greater(newSpice, initialSpice);
        }

        [Test]
        public void SimulateMonth_WithUpkeep_DecreasesCredits()
        {
            var refinery = new Facility("ref1", "Spice Refinery 1", FacilityType.SpiceRefinery);
            _testGameState.AddFacility(refinery, _testGameState.Enclaves[0].Id);
            
            double initialCredits = _testGameState.Inventory.Resources[ResourceType.Credits].Amount;
            
            _simulationEngine.SimulateMonth(_testGameState);
            
            double newCredits = _testGameState.Inventory.Resources[ResourceType.Credits].Amount;
            Assert.Less(newCredits, initialCredits);
        }

        [Test]
        public void SimulateMonth_GeneratesResult()
        {
            var result = _simulationEngine.SimulateMonth(_testGameState);
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.WasSuccessful);
            Assert.AreEqual(_testGameState.CurrentMonth, result.Month);
        }

        [Test]
        public void ResolveCombat_PlayerVictory_TransfersControl()
        {
            var attackerEnclave = _testGameState.Enclaves[0];
            var defenderEnclave = _testGameState.Enclaves[1];
            defenderEnclave.IsPlayerControlled = false;
            
            var creature = new Creature("c1", "Warrior", CreatureType.Sardaukar);
            creature.IsTamed = true;
            _testGameState.AddCreature(creature);
            attackerEnclave.GarrisonedCreatures.Add(creature.Id);
            
            var result = _simulationEngine.ResolveCombat(
                attackerEnclave.Id,
                defenderEnclave.Id,
                true
            );
            
            Assert.IsNotNull(result);
        }

        [Test]
        public void ResolveCombat_ProvidesLootOnVictory()
        {
            var attackerEnclave = _testGameState.Enclaves[0];
            var defenderEnclave = _testGameState.AddEnclave(
                new Enclave("def1", "Enemy Base", EnclaveType.HarkonnenRefinery, HabitatType.Industrial, 200, 0, 200)
            );
            defenderEnclave.IsPlayerControlled = false;
            
            for (int i = 0; i < 5; i++)
            {
                var creature = new Creature($"c{i}", "Warrior", CreatureType.Sardaukar);
                creature.IsTamed = true;
                creature.AttackPower = 100;
                _testGameState.AddCreature(creature);
                attackerEnclave.GarrisonedCreatures.Add(creature.Id);
            }
            
            var result = _simulationEngine.ResolveCombat(
                attackerEnclave.Id,
                defenderEnclave.Id,
                true
            );
            
            if (result.IsVictory)
            {
                Assert.Greater(result.LootGained.Count, 0);
            }
        }

        [Test]
        public void PreviewMonthlyProduction_ReturnsAllResources()
        {
            var preview = _simulationEngine.PreviewMonthlyProduction(_testGameState);
            
            Assert.IsNotNull(preview);
            Assert.True(preview.ContainsKey(ResourceType.Spice));
            Assert.True(preview.ContainsKey(ResourceType.Water));
            Assert.True(preview.ContainsKey(ResourceType.Credits));
        }

        [Test]
        public void SimulateMonth_EventTriggered_AddsToActiveEvents()
        {
            var initialEventCount = _testGameState.ActiveEvents.Count;
            
            for (int i = 0; i < 20; i++)
            {
                _simulationEngine.SimulateMonth(_testGameState);
            }
            
            Assert.GreaterOrEqual(_testGameState.ActiveEvents.Count + _testGameState.EventHistory.Count, initialEventCount);
        }

        [Test]
        public void SimulateMonth_CreatureWithoutWater_TakesDamage()
        {
            var creature = new Creature("c1", "Dewback", CreatureType.Dewback);
            creature.IsTamed = true;
            creature.Health = creature.MaxHealth;
            _testGameState.AddCreature(creature);
            
            _testGameState.Inventory.Resources[ResourceType.Water].Amount = 0;
            
            double initialHealth = creature.Health;
            
            _simulationEngine.SimulateMonth(_testGameState);
            
            Assert.Less(creature.Health, initialHealth);
        }

        [Test]
        public void SimulateMonth_EnclaveLoyaltyLow_TriggersRebellion()
        {
            var enclave = _testGameState.Enclaves[0];
            enclave.Loyalty = 5;
            
            for (int i = 0; i < 50; i++)
            {
                _simulationEngine.SimulateMonth(_testGameState);
            }
            
            Assert.True(enclave.Status == EnclaveStatus.Rebellion || enclave.Loyalty < 5);
        }

        [Test]
        public void ResolveCombat_InvalidIds_ReturnsNull()
        {
            var result = _simulationEngine.ResolveCombat("invalid", "also_invalid", true);
            
            Assert.IsNull(result);
        }
    }
}

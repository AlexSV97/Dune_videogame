using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;
using DuneArrakisDominion.Services;
using DuneArrakisDominion.Managers;

namespace DuneArrakisDominion.Tests
{
    [TestFixture]
    public class CalculationServiceTests
    {
        private CalculationService _calculationService;
        private GameState _testGameState;

        [SetUp]
        public void Setup()
        {
            _calculationService = new CalculationService();
            _testGameState = new GameState("TestPlayer", DifficultyLevel.Standard);
        }

        [TearDown]
        public void TearDown()
        {
            _calculationService = null;
            _testGameState = null;
        }

        [Test]
        public void CalculateSpiceProduction_WithNoFacilities_ReturnsZero()
        {
            double production = _calculationService.CalculateSpiceProduction(_testGameState);
            Assert.AreEqual(0, production);
        }

        [Test]
        public void CalculateSpiceProduction_WithSpiceRefinery_ReturnsCorrectAmount()
        {
            var refinery = new Facility("ref1", "Spice Refinery 1", FacilityType.SpiceRefinery);
            refinery.IsOperational = true;
            _testGameState.AddFacility(refinery, _testGameState.Enclaves[0].Id);

            double production = _calculationService.CalculateSpiceProduction(_testGameState);
            
            Assert.Greater(production, 0);
        }

        [Test]
        public void CalculateWaterProduction_WithWindTrap_ReturnsCorrectAmount()
        {
            var windTrap = new Facility("wt1", "Wind Trap 1", FacilityType.WindTrap);
            windTrap.IsOperational = true;
            _testGameState.AddFacility(windTrap, _testGameState.Enclaves[0].Id);

            double production = _calculationService.CalculateWaterProduction(_testGameState);
            
            Assert.Greater(production, 0);
        }

        [Test]
        public void CalculateMonthlyUpkeep_WithNoFacilities_ReturnsZero()
        {
            double upkeep = _calculationService.CalculateMonthlyUpkeep(_testGameState);
            Assert.AreEqual(0, upkeep);
        }

        [Test]
        public void CalculateMonthlyUpkeep_WithFacility_ReturnsCorrectAmount()
        {
            var refinery = new Facility("ref1", "Spice Refinery 1", FacilityType.SpiceRefinery);
            _testGameState.AddFacility(refinery, _testGameState.Enclaves[0].Id);

            double upkeep = _calculationService.CalculateMonthlyUpkeep(_testGameState);
            
            Assert.Greater(upkeep, 0);
            Assert.AreEqual(refinery.MonthlyUpkeep, upkeep, 0.01);
        }

        [Test]
        public void CalculateCombatResult_AttackerDoubleDefender_ReturnsHighVictoryChance()
        {
            double result = _calculationService.CalculateCombatResult(200, 100);
            Assert.Greater(result, 0.9);
        }

        [Test]
        public void CalculateCombatResult_EqualPower_ReturnsMediumVictoryChance()
        {
            double result = _calculationService.CalculateCombatResult(100, 100);
            Assert.Greater(result, 0.3);
            Assert.Less(result, 0.7);
        }

        [Test]
        public void CalculateCombatResult_DefenderStronger_ReturnsLowVictoryChance()
        {
            double result = _calculationService.CalculateCombatResult(50, 100);
            Assert.Less(result, 0.2);
        }

        [Test]
        public void CalculateCombatResult_ZeroDefender_ReturnsFullVictory()
        {
            double result = _calculationService.CalculateCombatResult(100, 0);
            Assert.AreEqual(1.0, result);
        }

        [Test]
        public void CalculateNetResourceChange_SpiceProduction_ReturnsPositive()
        {
            var refinery = new Facility("ref1", "Spice Refinery 1", FacilityType.SpiceRefinery);
            refinery.IsOperational = true;
            _testGameState.AddFacility(refinery, _testGameState.Enclaves[0].Id);

            double netChange = _calculationService.CalculateNetResourceChange(_testGameState, ResourceType.Spice);
            
            Assert.Greater(netChange, 0);
        }

        [Test]
        public void CalculateNetResourceChange_WithConsumption_ReturnsNegative()
        {
            var refinery = new Facility("ref1", "Spice Refinery 1", FacilityType.SpiceRefinery);
            refinery.IsOperational = true;
            refinery.ConsumptionPerMonth[ResourceType.Water] = 100;
            _testGameState.AddFacility(refinery, _testGameState.Enclaves[0].Id);

            double netChange = _calculationService.CalculateNetResourceChange(_testGameState, ResourceType.Water);
            
            Assert.Less(netChange, 0);
        }

        [Test]
        public void CalculateCreatureTamingDifficulty_Sandworm_ReturnsHighest()
        {
            double sandwormDifficulty = _calculationService.CalculateCreatureTamingDifficulty(CreatureType.Sandworm, 0);
            double dewbackDifficulty = _calculationService.CalculateCreatureTamingDifficulty(CreatureType.Dewback, 0);
            
            Assert.Greater(sandwormDifficulty, dewbackDifficulty);
        }

        [Test]
        public void CalculateCreatureTamingDifficulty_ProgressionIncreasesDifficulty()
        {
            double baseDifficulty = _calculationService.CalculateCreatureTamingDifficulty(CreatureType.FremenRider, 0);
            double advancedDifficulty = _calculationService.CalculateCreatureTamingDifficulty(CreatureType.FremenRider, 50);
            
            Assert.Greater(advancedDifficulty, baseDifficulty);
        }

        [Test]
        public void AdvanceTaming_SufficientResources_TamesCreature()
        {
            var creature = new Creature("c1", "Test Rider", CreatureType.FremenRider);
            creature.IsTamed = false;
            
            var (success, _) = _calculationService.AdvanceTaming(creature, 100, 100);
            
            Assert.IsTrue(success);
            Assert.IsTrue(creature.IsTamed);
        }

        [Test]
        public void AdvanceTaming_InsufficientResources_DoesNotTame()
        {
            var creature = new Creature("c1", "Test Rider", CreatureType.FremenRider);
            creature.IsTamed = false;
            
            var (success, progress) = _calculationService.AdvanceTaming(creature, 1, 1);
            
            Assert.IsFalse(success);
            Assert.IsFalse(creature.IsTamed);
            Assert.Greater(progress, 0);
        }

        [Test]
        public void CalculateTradeValue_SpiceAtHarkonnenRefinery_ReturnsIncreased()
        {
            double baseValue = _calculationService.CalculateTradeValue(ResourceType.Spice, EnclaveType.HarkonnenRefinery);
            double marketValue = _calculationService.CalculateTradeValue(ResourceType.Spice, EnclaveType.SmugglerCamp);
            
            Assert.Greater(baseValue, marketValue);
        }

        [Test]
        public void CalculateTradeValue_WaterAtSietch_ReturnsIncreased()
        {
            double sietchValue = _calculationService.CalculateTradeValue(ResourceType.Water, EnclaveType.Sietch);
            double outpostValue = _calculationService.CalculateTradeValue(ResourceType.Water, EnclaveType.ImperialOutpost);
            
            Assert.Greater(sietchValue, outpostValue);
        }

        [Test]
        public void CanAffordConstruction_EnoughCredits_ReturnsTrue()
        {
            _testGameState.Inventory.AddResource(ResourceType.Credits, 10000);
            
            bool canAfford = _calculationService.CanAffordConstruction(FacilityType.WindTrap, _testGameState);
            
            Assert.IsTrue(canAfford);
        }

        [Test]
        public void CanAffordConstruction_InsufficientCredits_ReturnsFalse()
        {
            _testGameState.Inventory.SpendResource(ResourceType.Credits, 5000);
            
            bool canAfford = _calculationService.CanAffordConstruction(FacilityType.SpiceRefinery, _testGameState);
            
            Assert.IsFalse(canAfford);
        }
    }
}

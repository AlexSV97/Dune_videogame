using System;
using System.Collections.Generic;
using System.Linq;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;
using DuneArrakisDominion.Services;

namespace DuneArrakisDominion.Managers
{
    public class SimulationEngine
    {
        private readonly CalculationService _calculationService;
        private readonly Random _random = new();

        public event Action<SimulationResult> OnMonthSimulated;
        public event Action<CombatResult> OnCombatResolved;
        public event Action<SimulationEvent> OnEventTriggered;

        public SimulationEngine()
        {
            _calculationService = new CalculationService();
        }

        public SimulationEngine(CalculationService calculationService)
        {
            _calculationService = calculationService;
        }

        public SimulationResult SimulateMonth(GameState state)
        {
            var result = new SimulationResult
            {
                Month = state.CurrentMonth,
                Year = state.CurrentYear
            };

            state.CurrentPhase = MonthPhase.Production;
            ProcessProduction(state, result);
            
            state.CurrentPhase = MonthPhase.Event;
            ProcessEvents(state, result);

            state.CurrentPhase = MonthPhase.Resolution;
            ProcessUpkeep(state, result);
            ProcessCreatureMaintenance(state, result);
            ProcessEnclaveEvents(state);

            state.CurrentPhase = MonthPhase.Planning;
            state.AdvanceMonth();

            OnMonthSimulated?.Invoke(result);
            return result;
        }

        private void ProcessProduction(GameState state, SimulationResult result)
        {
            var globalBonus = state.GlobalModifiers.GetValueOrDefault("ProductionBonus", 1.0);

            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                double netChange = _calculationService.CalculateNetResourceChange(state, resourceType);
                
                if (state.Inventory.TryGetResource(resourceType, out var resource))
                {
                    double previousAmount = resource.Amount;
                    resource.AddAmount(netChange);
                    
                    result.ResourceChanges[resourceType] = new ResourceChange
                    {
                        PreviousAmount = previousAmount,
                        Change = netChange,
                        NewAmount = resource.Amount
                    };
                }
            }

            int spiceProduced = (int)result.ResourceChanges.GetValueOrDefault(ResourceType.Spice)?.Change ?? 0;
            state.TotalSpiceCollected += Math.Max(0, spiceProduced);
        }

        private void ProcessEvents(GameState state, SimulationResult result)
        {
            double eventChance = state.GlobalModifiers.GetValueOrDefault("EventFrequency", 1.0) * 0.15;
            
            foreach (var existingEvent in state.ActiveEvents.ToList())
            {
                if (existingEvent.RequiresPlayerDecision && !existingEvent.IsResolved)
                {
                    continue;
                }
                
                existingEvent.ApplyEvent(state.Inventory);
                result.AppliedEvents.Add(existingEvent);
                state.EventHistory.Add(existingEvent);
                state.ActiveEvents.Remove(existingEvent);
            }

            if (_random.NextDouble() < eventChance)
            {
                var newEvent = SimulationEvent.GenerateRandomEvent(state.CurrentMonth);
                state.ActiveEvents.Add(newEvent);
                result.NewEvents.Add(newEvent);
                OnEventTriggered?.Invoke(newEvent);
            }
        }

        private void ProcessUpkeep(GameState state, SimulationResult result)
        {
            double totalUpkeep = 0;

            foreach (var facility in state.Facilities.Where(f => f.IsOperational))
            {
                totalUpkeep += facility.MonthlyUpkeep;
                
                foreach (var consumption in facility.ConsumptionPerMonth)
                {
                    if (state.Inventory.TryGetResource(consumption.Key, out var resource))
                    {
                        double consumptionAmount = consumption.Value * facility.Efficiency;
                        resource.Consume(consumptionAmount);
                    }
                }
            }

            foreach (var enclave in state.Enclaves.Where(e => e.IsPlayerControlled))
            {
                foreach (var upkeep in enclave.MonthlyUpkeep)
                {
                    if (state.Inventory.TryGetResource(upkeep.Key, out var resource))
                    {
                        resource.Consume(upkeep.Value);
                    }
                }
            }

            if (state.Inventory.TryGetResource(ResourceType.Credits, out var credits))
            {
                credits.Consume(totalUpkeep);
                result.TotalUpkeep = totalUpkeep;
            }
        }

        private void ProcessCreatureMaintenance(GameState state, SimulationResult result)
        {
            foreach (var creature in state.Creatures.Where(c => c.IsTamed && c.IsAlive))
            {
                double waterCost = creature.Type == CreatureType.Sandworm ? 20 : 
                                   creature.Type == CreatureType.Sandtrout ? 5 : 2;

                if (state.Inventory.TryGetResource(ResourceType.Water, out var water))
                {
                    if (water.Amount < waterCost)
                    {
                        creature.Health -= waterCost * 0.1;
                        
                        if (creature.Health <= 0)
                        {
                            creature.IsAlive = false;
                            result.DeadCreatures.Add(creature.Id);
                        }
                    }
                    else
                    {
                        water.Consume(waterCost);
                    }
                }
            }
        }

        private void ProcessEnclaveEvents(GameState state)
        {
            foreach (var enclave in state.Enclaves)
            {
                if (enclave.Status == EnclaveStatus.Destroyed)
                    continue;

                if (enclave.Status == EnclaveStatus.Rebellion)
                {
                    if (enclave.IsPlayerControlled)
                    {
                        enclave.IsPlayerControlled = false;
                        enclave.Status = EnclaveStatus.Conquered;
                    }
                }

                if (!enclave.IsPlayerControlled && enclave.Status == EnclaveStatus.Active)
                {
                    double rebellionChance = 0.02 * (1 - enclave.Loyalty / enclave.MaxLoyalty);
                    
                    if (_random.NextDouble() < rebellionChance)
                    {
                        enclave.ModifyLoyalty(-20);
                    }
                }
            }
        }

        public CombatResult ResolveCombat(
            GameState state,
            string attackerId,
            string defenderId,
            bool isPlayerAttacking)
        {
            var attacker = state.FindEntityById<Enclave>(attackerId) ?? 
                          (object)state.FindEntityById<Creature>(attackerId) as Enclave;
            var defender = state.FindEntityById<Enclave>(defenderId) ?? 
                          (object)state.FindEntityById<Creature>(defenderId) as Enclave;

            double attackerPower, defenderPower;

            if (attacker is Enclave attackerEnclave)
            {
                var (attack, _) = _calculationService.CalculateMilitaryPower(state, attackerEnclave.Id);
                attackerPower = attack;
            }
            else if (attacker is Creature attackerCreature)
            {
                attackerPower = attackerCreature.AttackPower;
            }
            else
            {
                attackerPower = 0;
            }

            if (defender is Enclave defenderEnclave)
            {
                var (_, defense) = _calculationService.CalculateMilitaryPower(state, defenderEnclave.Id);
                defenderPower = defense;
            }
            else if (defender is Creature defenderCreature)
            {
                defenderPower = defenderCreature.DefensePower;
            }
            else
            {
                defenderPower = 0;
            }

            double attackerStrength = state.GlobalModifiers.GetValueOrDefault("EnemyStrength", 1.0);
            
            if (!isPlayerAttacking)
            {
                attackerPower *= attackerStrength;
            }

            double victoryChance = _calculationService.CalculateCombatResult(attackerPower, defenderPower);
            
            var combatResult = new CombatResult
            {
                AttackerId = attackerId,
                DefenderId = defenderId,
                AttackerPower = attackerPower,
                DefenderPower = defenderPower,
                VictoryChance = victoryChance,
                IsVictory = _random.NextDouble() < victoryChance,
                IsPlayerAttacking = isPlayerAttacking
            };

            if (combatResult.IsVictory)
            {
                ProcessVictory(state, combatResult);
            }
            else
            {
                ProcessDefeat(state, combatResult);
            }

            OnCombatResolved?.Invoke(combatResult);
            return combatResult;
        }

        private void ProcessVictory(GameState state, CombatResult result)
        {
            var defender = state.FindEntityById<Enclave>(result.DefenderId) ?? 
                          state.FindEntityById<Creature>(result.DefenderId);

            if (defender is Enclave enclave)
            {
                if (result.IsPlayerAttacking)
                {
                    enclave.IsPlayerControlled = true;
                    enclave.Status = EnclaveStatus.Active;
                    enclave.ModifyLoyalty(30);
                    state.EnclavesConquered++;
                }
                
                double damage = enclave.MaxHealth * 0.3;
                foreach (var facility in state.Facilities.Where(f => f.ParentEnclaveId == enclave.Id))
                {
                    facility.TakeDamage(damage);
                }
            }
            else if (defender is Creature creature)
            {
                creature.TakeDamage(creature.MaxHealth * 0.5);
                
                if (!creature.IsAlive)
                {
                    state.Creatures.Remove(creature);
                    if (result.IsPlayerAttacking)
                    {
                        state.EnemiesDefeated++;
                    }
                }
            }

            int lootCredits = (int)(result.DefenderPower * 0.1);
            state.Inventory.AddResource(ResourceType.Credits, lootCredits);
            result.LootGained[ResourceType.Credits] = lootCredits;
        }

        private void ProcessDefeat(GameState state, CombatResult result)
        {
            var attacker = state.FindEntityById<Enclave>(result.AttackerId) ?? 
                          state.FindEntityById<Creature>(result.AttackerId);

            if (attacker is Enclave enclave)
            {
                double damage = enclave.MaxHealth * 0.2;
                foreach (var facility in state.Facilities.Where(f => f.ParentEnclaveId == enclave.Id))
                {
                    facility.TakeDamage(damage);
                }
            }
            else if (attacker is Creature creature)
            {
                creature.TakeDamage(creature.MaxHealth * 0.3);
            }

            int repairCost = (int)(result.AttackerPower * 0.05);
            state.Inventory.SpendResource(ResourceType.Credits, repairCost);
            result.RepairCost = repairCost;
        }

        public Dictionary<ResourceType, double> PreviewMonthlyProduction(GameState state)
        {
            var preview = new Dictionary<ResourceType, double>();

            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                preview[resourceType] = _calculationService.CalculateNetResourceChange(state, resourceType);
            }

            return preview;
        }
    }

    public class SimulationResult
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public Dictionary<ResourceType, ResourceChange> ResourceChanges { get; set; } = new();
        public List<SimulationEvent> NewEvents { get; set; } = new();
        public List<SimulationEvent> AppliedEvents { get; set; } = new();
        public List<string> DeadCreatures { get; set; } = new();
        public double TotalUpkeep { get; set; }
        public bool WasSuccessful { get; set; } = true;
        public string ErrorMessage { get; set; }
    }

    public class ResourceChange
    {
        public double PreviousAmount { get; set; }
        public double Change { get; set; }
        public double NewAmount { get; set; }

        public bool IsPositive => Change > 0;
        public bool IsNegative => Change < 0;
    }

    public class CombatResult
    {
        public string AttackerId { get; set; }
        public string DefenderId { get; set; }
        public double AttackerPower { get; set; }
        public double DefenderPower { get; set; }
        public double VictoryChance { get; set; }
        public bool IsVictory { get; set; }
        public bool IsPlayerAttacking { get; set; }
        public Dictionary<ResourceType, double> LootGained { get; set; } = new();
        public double RepairCost { get; set; }
    }
}

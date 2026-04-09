using System;
using System.Collections.Generic;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Services
{
    public class CalculationService
    {
        public double CalculateSpiceProduction(GameState state)
        {
            double baseProduction = 0;
            double globalBonus = state.GlobalModifiers.GetValueOrDefault("ProductionBonus", 1.0);

            foreach (var facility in state.Facilities)
            {
                if (facility.Type == FacilityType.SpiceRefinery && facility.IsOperational)
                {
                    baseProduction += facility.ProductionPerMonth.GetValueOrDefault(ResourceType.Spice, 0);
                }
            }

            foreach (var enclave in state.Enclaves)
            {
                if (enclave.IsPlayerControlled)
                {
                    baseProduction += enclave.BaseProduction.GetValueOrDefault(ResourceType.Spice, 0);
                }
            }

            return baseProduction * globalBonus;
        }

        public double CalculateWaterProduction(GameState state)
        {
            double baseProduction = 0;

            foreach (var facility in state.Facilities)
            {
                if (facility.IsOperational)
                {
                    if (facility.ProductionPerMonth.TryGetValue(ResourceType.Water, out double waterProd))
                    {
                        baseProduction += waterProd * facility.Efficiency * (1 + (facility.Level - 1) * 0.25);
                    }
                }
            }

            foreach (var enclave in state.Enclaves)
            {
                if (enclave.IsPlayerControlled && enclave.BaseProduction.TryGetValue(ResourceType.Water, out double waterBase))
                {
                    baseProduction += waterBase * (1 + enclave.ProductionBonus);
                }
            }

            return baseProduction;
        }

        public double CalculateMonthlyUpkeep(GameState state)
        {
            double upkeep = 0;

            foreach (var facility in state.Facilities)
            {
                if (facility.IsOperational)
                {
                    upkeep += facility.MonthlyUpkeep;
                }
            }

            foreach (var creature in state.Creatures)
            {
                if (creature.IsAlive && creature.IsTamed)
                {
                    upkeep += creature.MaintenanceCost;
                }
            }

            foreach (var enclave in state.Enclaves)
            {
                if (enclave.IsPlayerControlled)
                {
                    foreach (var cost in enclave.MonthlyUpkeep)
                    {
                        upkeep += cost.Value;
                    }
                }
            }

            return upkeep;
        }

        public double CalculateNetResourceChange(GameState state, ResourceType resourceType)
        {
            double production = 0;
            double consumption = 0;

            foreach (var facility in state.Facilities)
            {
                if (!facility.IsOperational) continue;

                var netProduction = facility.CalculateNetProduction();
                
                if (netProduction.TryGetValue(resourceType, out double net))
                {
                    if (net >= 0)
                        production += net;
                    else
                        consumption += Math.Abs(net);
                }
            }

            foreach (var enclave in state.Enclaves)
            {
                if (enclave.IsPlayerControlled)
                {
                    if (enclave.BaseProduction.TryGetValue(resourceType, out double baseProd))
                    {
                        production += baseProd * (1 + enclave.ProductionBonus);
                    }
                }
            }

            if (state.Inventory.TryGetResource(resourceType, out var resource))
            {
                consumption += resource.ConsumptionRate;
            }

            return production - consumption;
        }

        public (double attackPower, double defensePower) CalculateMilitaryPower(GameState state, string enclaveId = null)
        {
            double attackPower = 0;
            double defensePower = 0;

            foreach (var creature in state.Creatures)
            {
                if (!creature.IsAlive || !creature.IsTamed) continue;
                
                if (!string.IsNullOrEmpty(enclaveId))
                {
                    var parentEnclave = state.Enclaves.Find(e => e.GarrisonedCreatures.Contains(creature.Id));
                    if (parentEnclave?.Id != enclaveId) continue;
                }

                attackPower += creature.AttackPower;
                defensePower += creature.DefensePower;
            }

            foreach (var facility in state.Facilities)
            {
                if (!string.IsNullOrEmpty(enclaveId) && facility.ParentEnclaveId != enclaveId) continue;
                
                if (facility.Type == FacilityType.DefenseGrid && facility.IsOperational)
                {
                    defensePower += 200 * facility.Efficiency * facility.Level;
                }
            }

            if (!string.IsNullOrEmpty(enclaveId))
            {
                var enclave = state.Enclaves.Find(e => e.Id == enclaveId);
                if (enclave != null)
                {
                    defensePower *= (1 + enclave.DefenseBonus);
                }
            }

            return (attackPower, defensePower);
        }

        public double CalculateCombatResult(double attackerPower, double defenderPower)
        {
            if (defenderPower == 0) return 1.0;

            double ratio = attackerPower / defenderPower;
            
            if (ratio >= 2.0)
            {
                return 0.9 + (ratio - 2.0) * 0.05;
            }
            else if (ratio >= 1.5)
            {
                return 0.7 + (ratio - 1.5) * 0.4;
            }
            else if (ratio >= 1.0)
            {
                return 0.4 + (ratio - 1.0) * 0.6;
            }
            else if (ratio >= 0.5)
            {
                return 0.1 + (ratio - 0.5) * 0.6;
            }
            else
            {
                return Math.Max(0, ratio * 0.2);
            }
        }

        public Dictionary<ResourceType, double> CalculateEventImpact(SimulationEvent evt, GameState state)
        {
            var impact = new Dictionary<ResourceType, double>(evt.ResourceImpact);
            double globalModifier = state.GlobalModifiers.GetValueOrDefault("EventFrequency", 1.0);
            double severity = evt.Severity;

            foreach (var key in impact.Keys)
            {
                impact[key] *= severity * globalModifier;
            }

            return impact;
        }

        public double CalculateCreatureTamingDifficulty(CreatureType type, int currentProgress)
        {
            double baseDifficulty = type switch
            {
                CreatureType.Sandworm => 1000,
                CreatureType.Sandtrout => 200,
                CreatureType.FremenRider => 100,
                CreatureType.Sardaukar => 150,
                CreatureType.Ornithopter => 80,
                CreatureType.Dewback => 50,
                _ => 100
            };

            double progressModifier = 1.0 + (currentProgress / 100.0);
            return baseDifficulty * progressModifier;
        }

        public (bool success, double remainingProgress) AdvanceTaming(
            Creature creature, 
            double waterSpent, 
            double spiceOffered)
        {
            double progressGain = (waterSpent / 10) + (spiceOffered / 5);
            
            if (creature.Type == CreatureType.Sandworm)
            {
                progressGain *= 0.5;
            }
            else if (creature.Type == CreatureType.Dewback)
            {
                progressGain *= 1.5;
            }

            creature.TamingProgress += (int)progressGain;

            if (creature.TamingProgress >= 100)
            {
                creature.IsTamed = true;
                creature.TamingProgress = 100;
                return (true, 0);
            }

            return (false, creature.TamingProgress);
        }

        public double CalculateTradeValue(ResourceType resource, EnclaveType enclaveType)
        {
            double baseValue = resource switch
            {
                ResourceType.Spice => 10,
                ResourceType.Water => 5,
                ResourceType.Credits => 1,
                ResourceType.Knowledge => 50,
                ResourceType.Population => 20,
                _ => 1
            };

            double modifier = enclaveType switch
            {
                EnclaveType.HarkonnenRefinery => resource == ResourceType.Spice ? 1.5 : 1.0,
                EnclaveType.SmugglerCamp => resource == ResourceType.Spice ? 0.8 : 1.2,
                EnclaveType.Sietch => resource == ResourceType.Water ? 1.3 : 1.0,
                EnclaveType.ImperialOutpost => resource == ResourceType.Credits ? 1.2 : 0.9,
                _ => 1.0
            };

            return baseValue * modifier;
        }

        public bool CanAffordConstruction(FacilityType type, GameState state)
        {
            double cost = type switch
            {
                FacilityType.SpiceRefinery => 5000,
                FacilityType.WindTrap => 1500,
                FacilityType.WaterExtractor => 3000,
                FacilityType.DefenseGrid => 4000,
                FacilityType.TrainingCamp => 2500,
                FacilityType.ResearchLab => 6000,
                FacilityType.TradePost => 2000,
                FacilityType.WormSanctuary => 10000,
                _ => 1000
            };

            return state.Inventory.HasResource(ResourceType.Credits, cost);
        }

        public double CalculateGlobalEfficiency(GameState state)
        {
            if (state.Facilities.Count == 0) return 1.0;

            double totalEfficiency = 0;
            int operationalCount = 0;

            foreach (var facility in state.Facilities)
            {
                if (facility.IsOperational)
                {
                    totalEfficiency += facility.Efficiency;
                    operationalCount++;
                }
            }

            return operationalCount > 0 ? totalEfficiency / operationalCount : 1.0;
        }
    }
}

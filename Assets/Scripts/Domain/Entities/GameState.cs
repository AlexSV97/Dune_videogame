using System;
using System.Collections.Generic;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Domain.Entities
{
    [Serializable]
    public class GameState
    {
        public string SaveId { get; set; }
        public DateTime LastSaved { get; set; }
        public string PlayerName { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public MonthPhase CurrentPhase { get; set; }
        
        public Inventory Inventory { get; set; }
        public List<Creature> Creatures { get; set; } = new();
        public List<Enclave> Enclaves { get; set; } = new();
        public List<Facility> Facilities { get; set; } = new();
        public List<SimulationEvent> ActiveEvents { get; set; } = new();
        public List<SimulationEvent> EventHistory { get; set; } = new();
        
        public Dictionary<string, double> GlobalModifiers { get; set; } = new();
        
        public int TotalSpiceCollected { get; set; }
        public int TotalWaterConsumed { get; set; }
        public int EnemiesDefeated { get; set; }
        public int EnclavesConquered { get; set; }

        public GameState()
        {
            Initialize();
        }

        public GameState(string playerName, DifficultyLevel difficulty)
        {
            PlayerName = playerName;
            Difficulty = difficulty;
            Initialize();
        }

        private void Initialize()
        {
            SaveId = Guid.NewGuid().ToString();
            LastSaved = DateTime.Now;
            CurrentMonth = 1;
            CurrentYear = 10256;
            CurrentPhase = MonthPhase.Planning;
            Inventory = new Inventory();
            
            ApplyDifficultySettings();
            InitializeStartingEnclaves();
        }

        private void ApplyDifficultySettings()
        {
            switch (Difficulty)
            {
                case DifficultyLevel.Beginner:
                    GlobalModifiers["ProductionBonus"] = 1.2;
                    GlobalModifiers["EnemyStrength"] = 0.7;
                    GlobalModifiers["EventFrequency"] = 0.5;
                    break;
                case DifficultyLevel.Standard:
                    GlobalModifiers["ProductionBonus"] = 1.0;
                    GlobalModifiers["EnemyStrength"] = 1.0;
                    GlobalModifiers["EventFrequency"] = 1.0;
                    break;
                case DifficultyLevel.Veteran:
                    GlobalModifiers["ProductionBonus"] = 0.8;
                    GlobalModifiers["EnemyStrength"] = 1.3;
                    GlobalModifiers["EventFrequency"] = 1.2;
                    break;
                case DifficultyLevel.Messiah:
                    GlobalModifiers["ProductionBonus"] = 0.6;
                    GlobalModifiers["EnemyStrength"] = 1.5;
                    GlobalModifiers["EventFrequency"] = 1.5;
                    break;
            }
        }

        private void InitializeStartingEnclaves()
        {
            Enclaves.Add(new Enclave(
                "sietch_tabr",
                "Sietch Tabr",
                EnclaveType.Sietch,
                HabitatType.DesertShallow,
                0, 0, 0
            ));

            Enclaves.Add(new Enclave(
                "sietch_arrakis",
                "Sietch Arrakis",
                EnclaveType.Sietch,
                HabitatType.DesertDeep,
                100, 0, 50
            ));

            Enclaves[0].IsPlayerControlled = true;
        }

        public void AdvanceMonth()
        {
            CurrentMonth++;
            if (CurrentMonth > 12)
            {
                CurrentMonth = 1;
                CurrentYear++;
            }
            CurrentPhase = MonthPhase.Planning;
            
            foreach (var evt in ActiveEvents)
            {
                evt.MonthTriggered = CurrentMonth;
            }
        }

        public Creature AddCreature(Creature creature)
        {
            creature.Id = $"{creature.Type}_{Creatures.Count + 1}_{Guid.NewGuid().ToString()[..8]}";
            Creatures.Add(creature);
            return creature;
        }

        public Enclave AddEnclave(Enclave enclave)
        {
            enclave.Id = $"{enclave.Type}_{Enclaves.Count + 1}_{Guid.NewGuid().ToString()[..8]}";
            Enclaves.Add(enclave);
            return enclave;
        }

        public Facility AddFacility(Facility facility, string enclaveId)
        {
            facility.Id = $"{facility.Type}_{Facilities.Count + 1}_{Guid.NewGuid().ToString()[..8]}";
            facility.ParentEnclaveId = enclaveId;
            Facilities.Add(facility);
            
            var enclave = Enclaves.Find(e => e.Id == enclaveId);
            enclave?.OwnedFacilities.Add(facility.Id);
            
            return facility;
        }

        public T FindEntityById<T>(string id) where T : class
        {
            if (typeof(T) == typeof(Creature))
                return Creatures.Find(c => c.Id == id) as T;
            if (typeof(T) == typeof(Enclave))
                return Enclaves.Find(e => e.Id == id) as T;
            if (typeof(T) == typeof(Facility))
                return Facilities.Find(f => f.Id == id) as T;
            return null;
        }

        public GameState Clone()
        {
            return new GameState
            {
                SaveId = this.SaveId,
                LastSaved = this.LastSaved,
                PlayerName = this.PlayerName,
                Difficulty = this.Difficulty,
                CurrentMonth = this.CurrentMonth,
                CurrentYear = this.CurrentYear,
                CurrentPhase = this.CurrentPhase,
                Inventory = this.Inventory,
                Creatures = new List<Creature>(this.Creatures),
                Enclaves = new List<Enclave>(this.Enclaves),
                Facilities = new List<Facility>(this.Facilities),
                ActiveEvents = new List<SimulationEvent>(this.ActiveEvents),
                EventHistory = new List<SimulationEvent>(this.EventHistory),
                GlobalModifiers = new Dictionary<string, double>(this.GlobalModifiers),
                TotalSpiceCollected = this.TotalSpiceCollected,
                TotalWaterConsumed = this.TotalWaterConsumed,
                EnemiesDefeated = this.EnemiesDefeated,
                EnclavesConquered = this.EnclavesConquered
            };
        }
    }
}

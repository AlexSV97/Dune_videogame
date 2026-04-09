using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;
using DuneArrakisDominion.Services;

namespace DuneArrakisDominion.Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    
                    if (_instance == null)
                    {
                        var go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Services")]
        [SerializeField] private PersistenceService _persistenceService;
        [SerializeField] private CrewAiClient _crewAiClient;
        private SimulationEngine _simulationEngine;
        private CalculationService _calculationService;

        [Header("Game State")]
        [SerializeField] private GameState _currentGameState;
        
        public GameState CurrentGameState => _currentGameState;
        public bool IsGameLoaded => _currentGameState != null;
        
        public event Action<GameState> OnGameStateChanged;
        public event Action<SimulationResult> OnMonthSimulated;
        public event Action<CombatResult> OnCombatResolved;
        public event Action<SimulationEvent> OnEventTriggered;
        public event Action<AIRecommendation> OnAIRecommendationReceived;
        public event Action OnGameStarted;
        public event Action OnGameSaved;
        public event Action OnGameLoaded;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            _persistenceService ??= new PersistenceService();
            _simulationEngine = new SimulationEngine();
            _calculationService = new CalculationService();
            
            _simulationEngine.OnMonthSimulated += HandleMonthSimulated;
            _simulationEngine.OnCombatResolved += HandleCombatResolved;
            _simulationEngine.OnEventTriggered += HandleEventTriggered;

            if (_crewAiClient != null)
            {
                _crewAiClient.OnRecommendationReceived += HandleAIRecommendation;
            }
        }

        public void StartNewGame(string playerName, DifficultyLevel difficulty)
        {
            _currentGameState = new GameState(playerName, difficulty);
            OnGameStateChanged?.Invoke(_currentGameState);
            OnGameStarted?.Invoke();
        }

        public async Task StartNewGameAsync(string playerName, DifficultyLevel difficulty)
        {
            await Task.Run(() => StartNewGame(playerName, difficulty));
        }

        public async Task<bool> LoadGameAsync(string saveId)
        {
            var loadedState = await _persistenceService.LoadGameAsync(saveId);
            
            if (loadedState != null)
            {
                _currentGameState = loadedState;
                OnGameStateChanged?.Invoke(_currentGameState);
                OnGameLoaded?.Invoke();
                return true;
            }
            
            return false;
        }

        public bool LoadGameSync(string saveId)
        {
            var loadedState = _persistenceService.LoadGameSync(saveId);
            
            if (loadedState != null)
            {
                _currentGameState = loadedState;
                OnGameStateChanged?.Invoke(_currentGameState);
                OnGameLoaded?.Invoke();
                return true;
            }
            
            return false;
        }

        public async Task<bool> SaveGameAsync()
        {
            if (_currentGameState == null) return false;
            
            bool success = await _persistenceService.SaveGameAsync(_currentGameState);
            
            if (success)
            {
                OnGameSaved?.Invoke();
            }
            
            return success;
        }

        public bool SaveGameSync()
        {
            if (_currentGameState == null) return false;
            
            bool success = _persistenceService.SaveGameSync(_currentGameState);
            
            if (success)
            {
                OnGameSaved?.Invoke();
            }
            
            return success;
        }

        public SimulationResult SimulateMonth()
        {
            if (_currentGameState == null)
            {
                return new SimulationResult
                {
                    WasSuccessful = false,
                    ErrorMessage = "No game loaded"
                };
            }

            var result = _simulationEngine.SimulateMonth(_currentGameState);
            OnGameStateChanged?.Invoke(_currentGameState);
            
            return result;
        }

        public CombatResult ResolveCombat(string attackerId, string defenderId, bool isPlayerAttacking)
        {
            if (_currentGameState == null) return null;
            
            var result = _simulationEngine.ResolveCombat(
                _currentGameState,
                attackerId,
                defenderId,
                isPlayerAttacking
            );
            
            OnGameStateChanged?.Invoke(_currentGameState);
            return result;
        }

        public void AddFacility(FacilityType type, string enclaveId)
        {
            if (_currentGameState == null) return;

            if (!_calculationService.CanAffordConstruction(type, _currentGameState))
            {
                Debug.LogWarning("Cannot afford facility construction");
                return;
            }

            var facility = new Facility(
                Guid.NewGuid().ToString(),
                type.ToString(),
                type
            );

            _currentGameState.AddFacility(facility, enclaveId);
            OnGameStateChanged?.Invoke(_currentGameState);
        }

        public void UpgradeFacility(string facilityId)
        {
            if (_currentGameState == null) return;

            var facility = _currentGameState.FindEntityById<Facility>(facilityId);
            
            if (facility == null || !facility.CanUpgrade()) return;

            double upgradeCost = facility.GetUpgradeCost();
            
            if (!_currentGameState.Inventory.HasResource(ResourceType.Credits, upgradeCost))
            {
                Debug.LogWarning("Cannot afford facility upgrade");
                return;
            }

            _currentGameState.Inventory.SpendResource(ResourceType.Credits, upgradeCost);
            facility.Upgrade();
            
            OnGameStateChanged?.Invoke(_currentGameState);
        }

        public Creature AddCreature(CreatureType type)
        {
            if (_currentGameState == null) return null;

            var creature = new Creature(
                Guid.NewGuid().ToString(),
                type.ToString(),
                type
            );

            return _currentGameState.AddCreature(creature);
        }

        public void TameCreature(string creatureId, double waterSpent, double spiceOffered)
        {
            if (_currentGameState == null) return;

            var creature = _currentGameState.FindEntityById<Creature>(creatureId);
            
            if (creature == null || creature.IsTamed) return;

            _currentGameState.Inventory.SpendResource(ResourceType.Water, waterSpent);
            _currentGameState.Inventory.SpendResource(ResourceType.Spice, spiceOffered);

            _simulationEngine.AdvanceTaming(creature, waterSpent, spiceOffered);
            
            OnGameStateChanged?.Invoke(_currentGameState);
        }

        public async Task<AIRecommendation> RequestAIRecommendation()
        {
            if (_currentGameState == null || _crewAiClient == null) return null;

            return await _crewAiClient.RequestStrategicAdvice(_currentGameState);
        }

        public async Task<List<AIRecommendation>> RequestFullAIAnalysis()
        {
            if (_currentGameState == null || _crewAiClient == null) return null;

            return await _crewAiClient.RequestFullAnalysis(_currentGameState);
        }

        public void ResolveEventChoice(string eventId, int choiceIndex)
        {
            if (_currentGameState == null) return;

            var evt = _currentGameState.ActiveEvents.Find(e => e.Id == eventId);
            
            if (evt == null || choiceIndex >= evt.Choices.Count) return;

            var choice = evt.Choices[choiceIndex];
            
            if (!choice.CanAfford(_currentGameState.Inventory))
            {
                Debug.LogWarning("Cannot afford this choice");
                return;
            }

            choice.Apply(_currentGameState.Inventory);
            
            foreach (var enclave in _currentGameState.Enclaves)
            {
                enclave.ModifyLoyalty(choice.LoyaltyChange);
            }

            evt.IsResolved = true;
            _currentGameState.EventHistory.Add(evt);
            _currentGameState.ActiveEvents.Remove(evt);
            
            OnGameStateChanged?.Invoke(_currentGameState);
        }

        public Dictionary<ResourceType, double> PreviewProduction()
        {
            if (_currentGameState == null) return null;
            return _simulationEngine.PreviewMonthlyProduction(_currentGameState);
        }

        public List<SaveMetadata> GetSaveList()
        {
            return _persistenceService.GetAllSavesMetadataSync();
        }

        private void HandleMonthSimulated(SimulationResult result)
        {
            OnMonthSimulated?.Invoke(result);
        }

        private void HandleCombatResolved(CombatResult result)
        {
            OnCombatResolved?.Invoke(result);
        }

        private void HandleEventTriggered(SimulationEvent evt)
        {
            OnEventTriggered?.Invoke(evt);
        }

        private void HandleAIRecommendation(AIRecommendation recommendation)
        {
            OnAIRecommendationReceived?.Invoke(recommendation);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}

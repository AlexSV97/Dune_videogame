using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DuneArrakisDominion.Services
{
    public class CrewAiClient : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] private string baseUrl = "http://localhost:5000";
        [SerializeField] private float requestTimeout = 30f;
        
        private PersistenceService _persistenceService;

        public event Action<AIRecommendation> OnRecommendationReceived;
        public event Action<string> OnRequestFailed;

        private void Awake()
        {
            _persistenceService = new PersistenceService();
        }

        public async Task<AIRecommendation> RequestStrategicAdvice(GameState gameState)
        {
            string jsonPayload = CreatePayload(gameState);
            
            try
            {
                string response = await SendPostRequestAsync("/api/analyze", jsonPayload);
                return ParseRecommendation(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"CrewAI request failed: {ex.Message}");
                OnRequestFailed?.Invoke(ex.Message);
                return CreateFallbackRecommendation();
            }
        }

        public async Task<AIRecommendation> RequestFinancialAdvice(GameState gameState)
        {
            string jsonPayload = CreatePayload(gameState);
            
            try
            {
                string response = await SendPostRequestAsync("/api/mentat/financial", jsonPayload);
                return ParseRecommendation(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Mentat Financial request failed: {ex.Message}");
                OnRequestFailed?.Invoke(ex.Message);
                return CreateFallbackRecommendation();
            }
        }

        public async Task<AIRecommendation> RequestBeastMasterAdvice(GameState gameState)
        {
            string jsonPayload = CreatePayload(gameState);
            
            try
            {
                string response = await SendPostRequestAsync("/api/beastmaster/advice", jsonPayload);
                return ParseRecommendation(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"BeastMaster request failed: {ex.Message}");
                OnRequestFailed?.Invoke(ex.Message);
                return CreateFallbackRecommendation();
            }
        }

        public async Task<List<AIRecommendation>> RequestFullAnalysis(GameState gameState)
        {
            var tasks = new List<Task<AIRecommendation>>
            {
                RequestFinancialAdvice(gameState),
                RequestBeastMasterAdvice(gameState)
            };

            var results = await Task.WhenAll(tasks);
            
            OnRecommendationReceived?.Invoke(results[0]);
            if (results[1] != null)
            {
                OnRecommendationReceived?.Invoke(results[1]);
            }
            
            return new List<AIRecommendation>(results);
        }

        private string CreatePayload(GameState gameState)
        {
            var payload = new Dictionary<string, object>
            {
                ["playerName"] = gameState.PlayerName,
                ["difficulty"] = gameState.Difficulty.ToString(),
                ["currentMonth"] = gameState.CurrentMonth,
                ["currentYear"] = gameState.CurrentYear,
                ["resources"] = new Dictionary<string, double>
                {
                    ["spice"] = gameState.Inventory.TryGetResource(Domain.Enums.ResourceType.Spice, out var spice) ? spice.Amount : 0,
                    ["water"] = gameState.Inventory.TryGetResource(Domain.Enums.ResourceType.Water, out var water) ? water.Amount : 0,
                    ["credits"] = gameState.Inventory.TryGetResource(Domain.Enums.ResourceType.Credits, out var credits) ? credits.Amount : 0,
                    ["knowledge"] = gameState.Inventory.TryGetResource(Domain.Enums.ResourceType.Knowledge, out var knowledge) ? knowledge.Amount : 0,
                    ["population"] = gameState.Inventory.TryGetResource(Domain.Enums.ResourceType.Population, out var pop) ? pop.Amount : 0
                },
                ["facilities"] = new List<object>(),
                ["enclaves"] = gameState.Enclaves.Count,
                ["activeCreatures"] = gameState.Creatures.Count,
                ["activeEvents"] = gameState.ActiveEvents.Count
            };

            foreach (var facility in gameState.Facilities)
            {
                ((List<object>)payload["facilities"]).Add(new Dictionary<string, object>
                {
                    ["type"] = facility.Type.ToString(),
                    ["level"] = facility.Level,
                    ["operational"] = facility.IsOperational
                });
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(payload, options);
        }

        private async Task<string> SendPostRequestAsync(string endpoint, string jsonPayload)
        {
            using var request = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)requestTimeout;

            var operation = request.SendWebRequest();
            
            float elapsed = 0;
            while (!operation.isDone && elapsed < requestTimeout)
            {
                await Task.Delay(100);
                elapsed += 0.1f;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"Request failed: {request.error}");
            }

            return request.downloadHandler.text;
        }

        private AIRecommendation ParseRecommendation(string jsonResponse)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                return JsonSerializer.Deserialize<AIRecommendation>(jsonResponse, options);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse recommendation: {ex.Message}");
                return CreateFallbackRecommendation();
            }
        }

        private AIRecommendation CreateFallbackRecommendation()
        {
            return new AIRecommendation
            {
                AgentName = "Fallback Advisor",
                Priority = RecommendationPriority.Medium,
                ActionType = "conserve_resources",
                Title = "Conervar Recursos",
                Description = "Recomendación por defecto: enfocarse en la producción de especia.",
                Reasoning = "Sin datos de la IA externa, se recomienda priorizar la extracción de especia.",
                Confidence = 0.3f
            };
        }

        public void SetBaseUrl(string url)
        {
            baseUrl = url;
        }
    }

    [Serializable]
    public class AIRecommendation
    {
        public string AgentName { get; set; }
        public RecommendationPriority Priority { get; set; }
        public string ActionType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reasoning { get; set; }
        public float Confidence { get; set; }
        public Dictionary<string, double> ResourceImpact { get; set; } = new();
        public List<string> RecommendedCreatures { get; set; } = new();
        public List<string> RecommendedFacilities { get; set; } = new();
    }

    public enum RecommendationPriority
    {
        Critical,
        High,
        Medium,
        Low
    }

    public static class RecommendationExtensions
    {
        public static bool IsCritical(this AIRecommendation rec) => 
            rec.Priority == RecommendationPriority.Critical;
        
        public static bool IsHighPriority(this AIRecommendation rec) => 
            rec.Priority == RecommendationPriority.High || rec.Priority == RecommendationPriority.Critical;
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using DuneArrakisDominion.Domain.Entities;

namespace DuneArrakisDominion.Services
{
    public class PersistenceService
    {
        private static readonly string SaveDirectory = Path.Combine(
            UnityEngine.Application.persistentDataPath,
            "Saves"
        );

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public PersistenceService()
        {
            EnsureSaveDirectoryExists();
        }

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        public async Task<bool> SaveGameAsync(GameState gameState)
        {
            try
            {
                string filePath = GetSaveFilePath(gameState.SaveId);
                gameState.LastSaved = DateTime.Now;
                
                string json = JsonSerializer.Serialize(gameState, JsonOptions);
                await File.WriteAllTextAsync(filePath, json);
                
                UnityEngine.Debug.Log($"Game saved successfully: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to save game: {ex.Message}");
                return false;
            }
        }

        public bool SaveGameSync(GameState gameState)
        {
            try
            {
                string filePath = GetSaveFilePath(gameState.SaveId);
                gameState.LastSaved = DateTime.Now;
                
                string json = JsonSerializer.Serialize(gameState, JsonOptions);
                File.WriteAllText(filePath, json);
                
                UnityEngine.Debug.Log($"Game saved successfully: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to save game: {ex.Message}");
                return false;
            }
        }

        public async Task<GameState> LoadGameAsync(string saveId)
        {
            try
            {
                string filePath = GetSaveFilePath(saveId);
                
                if (!File.Exists(filePath))
                {
                    UnityEngine.Debug.LogWarning($"Save file not found: {filePath}");
                    return null;
                }
                
                string json = await File.ReadAllTextAsync(filePath);
                GameState gameState = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
                
                UnityEngine.Debug.Log($"Game loaded successfully: {filePath}");
                return gameState;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to load game: {ex.Message}");
                return null;
            }
        }

        public GameState LoadGameSync(string saveId)
        {
            try
            {
                string filePath = GetSaveFilePath(saveId);
                
                if (!File.Exists(filePath))
                {
                    UnityEngine.Debug.LogWarning($"Save file not found: {filePath}");
                    return null;
                }
                
                string json = File.ReadAllText(filePath);
                GameState gameState = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
                
                UnityEngine.Debug.Log($"Game loaded successfully: {filePath}");
                return gameState;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to load game: {ex.Message}");
                return null;
            }
        }

        public async Task<GameState> LoadLatestSaveAsync()
        {
            var saves = await GetAllSavesMetadataAsync();
            
            if (saves.Count == 0)
            {
                UnityEngine.Debug.Log("No save files found.");
                return null;
            }

            saves.Sort((a, b) => b.LastSaved.CompareTo(a.LastSaved));
            return await LoadGameAsync(saves[0].SaveId);
        }

        public async Task<GameState> LoadLatestSaveSync()
        {
            var saves = GetAllSavesMetadataSync();
            
            if (saves.Count == 0)
            {
                UnityEngine.Debug.Log("No save files found.");
                return null;
            }

            saves.Sort((a, b) => b.LastSaved.CompareTo(a.LastSaved));
            return LoadGameSync(saves[0].SaveId);
        }

        public async Task<string> ExportGameStateJsonAsync(GameState gameState)
        {
            string exportPath = Path.Combine(SaveDirectory, "game_state.json");
            string json = JsonSerializer.Serialize(gameState, JsonOptions);
            await File.WriteAllTextAsync(exportPath, json);
            return exportPath;
        }

        public string ExportGameStateJsonSync(GameState gameState)
        {
            string exportPath = Path.Combine(SaveDirectory, "game_state.json");
            string json = JsonSerializer.Serialize(gameState, JsonOptions);
            File.WriteAllText(exportPath, json);
            return exportPath;
        }

        public async Task<GameState> ImportGameStateJsonAsync(string jsonPath)
        {
            try
            {
                string json = await File.ReadAllTextAsync(jsonPath);
                return JsonSerializer.Deserialize<GameState>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to import game state: {ex.Message}");
                return null;
            }
        }

        public GameState ImportGameStateJsonSync(string jsonPath)
        {
            try
            {
                string json = File.ReadAllText(jsonPath);
                return JsonSerializer.Deserialize<GameState>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to import game state: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteSaveAsync(string saveId)
        {
            try
            {
                string filePath = GetSaveFilePath(saveId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    UnityEngine.Debug.Log($"Save deleted: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to delete save: {ex.Message}");
                return false;
            }
        }

        public async Task<List<SaveMetadata>> GetAllSavesMetadataAsync()
        {
            var saves = new List<SaveMetadata>();
            
            try
            {
                if (!Directory.Exists(SaveDirectory))
                    return saves;

                foreach (string file in Directory.GetFiles(SaveDirectory, "*.json"))
                {
                    try
                    {
                        string json = await File.ReadAllTextAsync(file);
                        var gameState = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
                        
                        if (gameState != null)
                        {
                            saves.Add(new SaveMetadata
                            {
                                SaveId = gameState.SaveId,
                                PlayerName = gameState.PlayerName,
                                Difficulty = gameState.Difficulty,
                                CurrentMonth = gameState.CurrentMonth,
                                CurrentYear = gameState.CurrentYear,
                                LastSaved = gameState.LastSaved
                            });
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to get saves metadata: {ex.Message}");
            }
            
            return saves;
        }

        public List<SaveMetadata> GetAllSavesMetadataSync()
        {
            var saves = new List<SaveMetadata>();
            
            try
            {
                if (!Directory.Exists(SaveDirectory))
                    return saves;

                foreach (string file in Directory.GetFiles(SaveDirectory, "*.json"))
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var gameState = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
                        
                        if (gameState != null)
                        {
                            saves.Add(new SaveMetadata
                            {
                                SaveId = gameState.SaveId,
                                PlayerName = gameState.PlayerName,
                                Difficulty = gameState.Difficulty,
                                CurrentMonth = gameState.CurrentMonth,
                                CurrentYear = gameState.CurrentYear,
                                LastSaved = gameState.LastSaved
                            });
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to get saves metadata: {ex.Message}");
            }
            
            return saves;
        }

        private string GetSaveFilePath(string saveId)
        {
            return Path.Combine(SaveDirectory, $"{saveId}.json");
        }

        public bool SaveExists(string saveId)
        {
            return File.Exists(GetSaveFilePath(saveId));
        }
    }

    public class SaveMetadata
    {
        public string SaveId { get; set; }
        public string PlayerName { get; set; }
        public Domain.Enums.DifficultyLevel Difficulty { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public DateTime LastSaved { get; set; }

        public string DisplayName => $"{PlayerName} - Month {CurrentMonth}, Year {CurrentYear}";
        public string DisplayDate => LastSaved.ToString("yyyy-MM-dd HH:mm");
    }
}

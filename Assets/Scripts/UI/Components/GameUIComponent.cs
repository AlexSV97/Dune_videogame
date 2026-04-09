using System;
using System.Collections.Generic;
using UnityEngine;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;
using DuneArrakisDominion.Managers;

namespace DuneArrakisDominion.UI
{
    public abstract class GameUIComponent : MonoBehaviour
    {
        protected GameManager GameManager => GameManager.Instance;

        protected virtual void OnEnable()
        {
            if (GameManager != null)
            {
                GameManager.OnGameStateChanged += OnGameStateChanged;
                GameManager.OnGameStarted += OnGameStarted;
                GameManager.OnGameLoaded += OnGameLoaded;
            }
        }

        protected virtual void OnDisable()
        {
            if (GameManager != null)
            {
                GameManager.OnGameStateChanged -= OnGameStateChanged;
                GameManager.OnGameStarted -= OnGameStarted;
                GameManager.OnGameLoaded -= OnGameLoaded;
            }
        }

        protected virtual void Start()
        {
            if (GameManager != null && GameManager.IsGameLoaded)
            {
                RefreshUI();
            }
        }

        protected virtual void OnGameStateChanged(GameState state)
        {
            RefreshUI();
        }

        protected virtual void OnGameStarted()
        {
            RefreshUI();
        }

        protected virtual void OnGameLoaded()
        {
            RefreshUI();
        }

        protected abstract void RefreshUI();

        protected void SafeUpdateText(UnityEngine.UI.Text textComponent, string value)
        {
            if (textComponent != null)
            {
                textComponent.text = value;
            }
        }

        protected void SafeUpdateText(TMPro.TextMeshProUGUI textComponent, string value)
        {
            if (textComponent != null)
            {
                textComponent.text = value;
            }
        }

        protected void SafeUpdateImage(UnityEngine.UI.Image imageComponent, Sprite sprite)
        {
            if (imageComponent != null && sprite != null)
            {
                imageComponent.sprite = sprite;
            }
        }

        protected void SafeSetActive(GameObject gameObject, bool active)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(active);
            }
        }
    }

    public class UIEvents
    {
        private static UIEvents _instance;
        public static UIEvents Instance => _instance ??= new UIEvents();

        public event Action<ResourceType, double, double> OnResourceChanged;
        public event Action<SimulationEvent> OnEventOccurred;
        public event Action<AIRecommendation> OnRecommendationReceived;
        public event Action<CombatResult> OnCombatOccurred;
        public event Action<string> OnNotificationRequested;
        public event Action<MonthPhase> OnPhaseChanged;

        public void EmitResourceChanged(ResourceType type, double oldAmount, double newAmount)
        {
            OnResourceChanged?.Invoke(type, oldAmount, newAmount);
        }

        public void EmitEventOccurred(SimulationEvent evt)
        {
            OnEventOccurred?.Invoke(evt);
        }

        public void EmitRecommendationReceived(AIRecommendation rec)
        {
            OnRecommendationReceived?.Invoke(rec);
        }

        public void EmitCombatOccurred(CombatResult result)
        {
            OnCombatOccurred?.Invoke(result);
        }

        public void EmitNotification(string message)
        {
            OnNotificationRequested?.Invoke(message);
        }

        public void EmitPhaseChanged(MonthPhase phase)
        {
            OnPhaseChanged?.Invoke(phase);
        }
    }

    public class NotificationData
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public float Duration { get; set; } = 3f;
        public Sprite Icon { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Combat,
        Resource
    }
}

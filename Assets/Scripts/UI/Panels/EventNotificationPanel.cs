using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Managers;

namespace DuneArrakisDominion.UI.Panels
{
    public class EventNotificationPanel : GameUIComponent
    {
        [Header("Panel References")]
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private TextMeshProUGUI eventSeverityText;
        [SerializeField] private Image eventIcon;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button aiAdviceButton;

        [Header("Queue Display")]
        [SerializeField] private Transform eventQueueContainer;
        [SerializeField] private GameObject eventQueueItemPrefab;
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve animationCurve;

        private Queue<SimulationEvent> _eventQueue = new();
        private SimulationEvent _currentEvent;
        private List<GameObject> _activeChoiceButtons = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (GameManager != null)
            {
                GameManager.OnEventTriggered += HandleEventTriggered;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseCurrentEvent);
            }

            if (aiAdviceButton != null)
            {
                aiAdviceButton.onClick.AddListener(RequestAIAdvice);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (GameManager != null)
            {
                GameManager.OnEventTriggered -= HandleEventTriggered;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseCurrentEvent);
            }

            if (aiAdviceButton != null)
            {
                aiAdviceButton.onClick.RemoveListener(RequestAIAdvice);
            }

            ClearChoiceButtons();
        }

        private void HandleEventTriggered(SimulationEvent evt)
        {
            if (evt.RequiresPlayerDecision)
            {
                _eventQueue.Enqueue(evt);
                UpdateEventQueueDisplay();
                
                if (_currentEvent == null)
                {
                    ShowNextEvent();
                }
            }
            else
            {
                ShowQuickNotification(evt);
            }
        }

        protected override void RefreshUI()
        {
            if (GameManager.CurrentGameState == null) return;
            
            foreach (var evt in GameManager.CurrentGameState.ActiveEvents)
            {
                if (evt.RequiresPlayerDecision && evt != _currentEvent)
                {
                    if (!_eventQueue.Contains(evt))
                    {
                        _eventQueue.Enqueue(evt);
                    }
                }
            }
            
            UpdateEventQueueDisplay();
        }

        private void ShowNextEvent()
        {
            if (_eventQueue.Count == 0)
            {
                eventPanel?.SetActive(false);
                return;
            }

            _currentEvent = _eventQueue.Dequeue();
            UpdateEventQueueDisplay();
            DisplayEvent(_currentEvent);
        }

        private void DisplayEvent(SimulationEvent evt)
        {
            if (eventPanel != null)
            {
                eventPanel.SetActive(true);
                AnimatePanelIn();
            }

            SafeUpdateText(eventTitleText, evt.Name);
            SafeUpdateText(eventDescriptionText, evt.Description);
            SafeUpdateText(eventSeverityText, $"Severidad: {evt.Severity:P0}");
            
            UpdateEventIcon(evt.Type);
            CreateChoiceButtons(evt);
        }

        private void UpdateEventIcon(Domain.Enums.EventType type)
        {
            if (eventIcon == null) return;

            Color iconColor = type switch
            {
                Domain.Enums.EventType.Sandstorm => new Color(0.6f, 0.4f, 0.2f),
                Domain.Enums.EventType.SpiceBlow => new Color(1f, 0.6f, 0f),
                Domain.Enums.EventType.PoliticalUprising => Color.yellow,
                Domain.Enums.EventType.ImperialInspection => Color.magenta,
                Domain.Enums.EventType.TradeOpportunity => Color.green,
                Domain.Enums.EventType.Attack => Color.red,
                Domain.Enums.EventType.Plague => new Color(0.5f, 0.3f, 0.5f),
                Domain.Enums.EventType.Discovery => Color.cyan,
                _ => Color.white
            };

            eventIcon.color = iconColor;
        }

        private void CreateChoiceButtons(SimulationEvent evt)
        {
            ClearChoiceButtons();

            if (evt.Choices.Count == 0)
            {
                CreateDefaultContinueButton();
                return;
            }

            foreach (var choice in evt.Choices)
            {
                CreateChoiceButton(choice);
            }
        }

        private void CreateChoiceButton(EventChoice choice)
        {
            if (choiceButtonPrefab == null || choicesContainer == null) return;

            var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
            var button = buttonObj.GetComponent<Button>();
            var choiceText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (choiceText != null)
            {
                string costText = "";
                foreach (var cost in choice.ResourceCosts)
                {
                    costText += $"\n-{cost.Value:F0} {cost.Key}";
                }
                choiceText.text = $"{choice.ChoiceName}{costText}";
            }

            if (button != null)
            {
                bool canAfford = choice.CanAfford(GameManager.CurrentGameState.Inventory);
                button.interactable = canAfford;
                
                var colors = button.colors;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                button.colors = colors;

                int choiceIndex = _currentEvent.Choices.IndexOf(choice);
                button.onClick.AddListener(() => SelectChoice(choiceIndex));
            }

            _activeChoiceButtons.Add(buttonObj);
        }

        private void CreateDefaultContinueButton()
        {
            if (choiceButtonPrefab == null || choicesContainer == null) return;

            var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
            var button = buttonObj.GetComponent<Button>();
            var choiceText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (choiceText != null)
            {
                choiceText.text = "Continuar";
            }

            if (button != null)
            {
                button.onClick.AddListener(CloseCurrentEvent);
            }

            _activeChoiceButtons.Add(buttonObj);
        }

        private void ClearChoiceButtons()
        {
            foreach (var button in _activeChoiceButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            _activeChoiceButtons.Clear();
        }

        private void SelectChoice(int choiceIndex)
        {
            if (_currentEvent == null) return;

            GameManager.ResolveEventChoice(_currentEvent.Id, choiceIndex);
            CloseCurrentEvent();
        }

        private void CloseCurrentEvent()
        {
            if (_currentEvent != null && !_currentEvent.IsResolved)
            {
                _currentEvent.IsResolved = true;
                _currentEvent.ApplyEvent(GameManager.CurrentGameState.Inventory);
                GameManager.CurrentGameState.EventHistory.Add(_currentEvent);
            }

            AnimatePanelOut(() =>
            {
                _currentEvent = null;
                ShowNextEvent();
            });
        }

        private void ShowQuickNotification(SimulationEvent evt)
        {
            UIEvents.Instance.EmitNotification($"{evt.Name}: {evt.Description}");
            UIEvents.Instance.EmitEventOccurred(evt);
        }

        private void RequestAIAdvice()
        {
            if (_currentEvent == null) return;

            _ = GameManager.RequestAIRecommendation();
            UIEvents.Instance.EmitNotification("Consultando a los Mentats...");
        }

        private void UpdateEventQueueDisplay()
        {
            if (eventQueueContainer == null) return;

            foreach (Transform child in eventQueueContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var evt in _eventQueue)
            {
                var queueItem = Instantiate(eventQueueItemPrefab, eventQueueContainer);
                var text = queueItem.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = evt.Name;
                }
            }
        }

        private void AnimatePanelIn()
        {
            if (eventPanel == null || animationCurve == null) return;

            var rect = eventPanel.GetComponent<RectTransform>();
            Vector2 startPos = rect.anchoredPosition + Vector2.down * 100;
            Vector2 endPos = rect.anchoredPosition;

            StartCoroutine(AnimateRect(rect, startPos, endPos, animationDuration));
        }

        private void AnimatePanelOut(Action onComplete)
        {
            if (eventPanel == null || animationCurve == null)
            {
                onComplete?.Invoke();
                return;
            }

            var rect = eventPanel.GetComponent<RectTransform>();
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = rect.anchoredPosition + Vector2.down * 100;

            StartCoroutine(AnimateRect(rect, startPos, endPos, animationDuration, onComplete));
        }

        private System.Collections.IEnumerator AnimateRect(
            RectTransform rect, 
            Vector2 start, 
            Vector2 end, 
            float duration,
            Action onComplete = null)
        {
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            rect.anchoredPosition = end;
            onComplete?.Invoke();
        }
    }
}

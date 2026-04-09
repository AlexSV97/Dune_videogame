using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Managers;

namespace DuneArrakisDominion.UI.Panels
{
    public class GameHudPanel : GameUIComponent
    {
        [Header("Date Display")]
        [SerializeField] private TextMeshProUGUI monthText;
        [SerializeField] private TextMeshProUGUI yearText;
        [SerializeField] private TextMeshProUGUI phaseText;

        [Header("Action Buttons")]
        [SerializeField] private Button nextMonthButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button aiAdviceButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button facilitiesButton;
        [SerializeField] private Button creaturesButton;

        [Header("Info Panels")]
        [SerializeField] private ResourceManagerUI resourceManager;
        [SerializeField] private EventNotificationPanel eventPanel;
        [SerializeField] private GameObject enclaveInfoPanel;
        [SerializeField] private GameObject productionPreviewPanel;

        [Header("Sub Panels")]
        [SerializeField] private GameObject facilitiesPanel;
        [SerializeField] private GameObject creaturesPanel;
        [SerializeField] private GameObject saveLoadPanel;
        [SerializeField] private GameObject menuPanel;

        private List<Button> _navigationButtons;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (GameManager != null)
            {
                GameManager.OnMonthSimulated += OnMonthSimulated;
                GameManager.OnCombatResolved += OnCombatResolved;
                GameManager.OnAIRecommendationReceived += OnAIRecommendationReceived;
            }

            SetupButtons();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (GameManager != null)
            {
                GameManager.OnMonthSimulated -= OnMonthSimulated;
                GameManager.OnCombatResolved -= OnCombatResolved;
                GameManager.OnAIRecommendationReceived -= OnAIRecommendationReceived;
            }

            RemoveButtonListeners();
        }

        private void SetupButtons()
        {
            _navigationButtons = new List<Button>
            {
                nextMonthButton, saveButton, loadButton, aiAdviceButton,
                menuButton, facilitiesButton, creaturesButton
            };

            nextMonthButton?.onClick.AddListener(OnNextMonthClicked);
            saveButton?.onClick.AddListener(OnSaveClicked);
            loadButton?.onClick.AddListener(OnLoadClicked);
            aiAdviceButton?.onClick.AddListener(OnAIAdviceClicked);
            menuButton?.onClick.AddListener(OnMenuClicked);
            facilitiesButton?.onClick.AddListener(OnFacilitiesClicked);
            creaturesButton?.onClick.AddListener(OnCreaturesClicked);
        }

        private void RemoveButtonListeners()
        {
            nextMonthButton?.onClick.RemoveListener(OnNextMonthClicked);
            saveButton?.onClick.RemoveListener(OnSaveClicked);
            loadButton?.onClick.RemoveListener(OnLoadClicked);
            aiAdviceButton?.onClick.RemoveListener(OnAIAdviceClicked);
            menuButton?.onClick.RemoveListener(OnMenuClicked);
            facilitiesButton?.onClick.RemoveListener(OnFacilitiesClicked);
            creaturesButton?.onClick.RemoveListener(OnCreaturesClicked);
        }

        protected override void RefreshUI()
        {
            if (GameManager.CurrentGameState == null) return;

            var state = GameManager.CurrentGameState;

            UpdateDateDisplay(state.CurrentMonth, state.CurrentYear, state.CurrentPhase);
            UpdateButtonStates();
        }

        private void UpdateDateDisplay(int month, int year, Domain.Enums.MonthPhase phase)
        {
            string[] monthNames = 
            {
                "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
            };

            SafeUpdateText(monthText, monthNames[month - 1]);
            SafeUpdateText(yearText, $"Año {year}");
            SafeUpdateText(phaseText, GetPhaseName(phase));
        }

        private string GetPhaseName(Domain.Enums.MonthPhase phase) => phase switch
        {
            Domain.Enums.MonthPhase.Planning => "Planificación",
            Domain.Enums.MonthPhase.Production => "Producción",
            Domain.Enums.MonthPhase.Resolution => "Resolución",
            Domain.Enums.MonthPhase.Event => "Evento",
            _ => "Desconocido"
        };

        private void UpdateButtonStates()
        {
            if (nextMonthButton != null)
            {
                bool canAdvance = GameManager.CurrentGameState?.CurrentPhase == Domain.Enums.MonthPhase.Planning;
                nextMonthButton.interactable = canAdvance;
            }
        }

        private void OnNextMonthClicked()
        {
            if (GameManager.CurrentGameState?.CurrentPhase != Domain.Enums.MonthPhase.Planning)
            {
                UIEvents.Instance.EmitNotification("Debes completar la fase de planificación primero.");
                return;
            }

            var result = GameManager.SimulateMonth();
            
            if (!result.WasSuccessful)
            {
                UIEvents.Instance.EmitNotification($"Error: {result.ErrorMessage}");
            }
        }

        private void OnSaveClicked()
        {
            CloseAllSubPanels();
            TogglePanel(saveLoadPanel);
            UIEvents.Instance.EmitNotification("Panel de Guardado/Carga abierto.");
        }

        private void OnLoadClicked()
        {
            OnSaveClicked();
        }

        private async void OnAIAdviceClicked()
        {
            aiAdviceButton.interactable = false;
            
            UIEvents.Instance.EmitNotification("Consultando a los Mentats y Maestros de Bestias...");
            
            var recommendations = await GameManager.RequestFullAIAnalysis();
            
            if (recommendations != null && recommendations.Count > 0)
            {
                foreach (var rec in recommendations)
                {
                    UIEvents.Instance.EmitRecommendationReceived(rec);
                }
            }
            else
            {
                UIEvents.Instance.EmitNotification("Los Mentats no pudieron procesar tu solicitud.");
            }
            
            aiAdviceButton.interactable = true;
        }

        private void OnMenuClicked()
        {
            CloseAllSubPanels();
            TogglePanel(menuPanel);
        }

        private void OnFacilitiesClicked()
        {
            CloseAllSubPanels();
            TogglePanel(facilitiesPanel);
        }

        private void OnCreaturesClicked()
        {
            CloseAllSubPanels();
            TogglePanel(creaturesPanel);
        }

        private void TogglePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }

        private void CloseAllSubPanels()
        {
            if (facilitiesPanel != null) facilitiesPanel.SetActive(false);
            if (creaturesPanel != null) creaturesPanel.SetActive(false);
            if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
            if (menuPanel != null) menuPanel.SetActive(false);
        }

        private void OnMonthSimulated(SimulationResult result)
        {
            string notification = $"Mes {result.Month} completado. ";
            
            if (result.NewEvents.Count > 0)
            {
                notification += $"{result.NewEvents.Count} nuevo(s) evento(s).";
            }
            
            UIEvents.Instance.EmitNotification(notification);
        }

        private void OnCombatResolved(CombatResult result)
        {
            string outcome = result.IsVictory ? "Victoria" : "Derrota";
            UIEvents.Instance.EmitNotification($"Combate: {outcome}!");
            UIEvents.Instance.EmitCombatOccurred(result);
        }

        private void OnAIRecommendationReceived(AIRecommendation recommendation)
        {
            UIEvents.Instance.EmitRecommendationReceived(recommendation);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuneArrakisDominion.Domain.Entities;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.UI.Components
{
    public class ResourceManagerUI : GameUIComponent
    {
        [Header("Resource Display")]
        [SerializeField] private GameObject resourceItemPrefab;
        [SerializeField] private Transform resourceContainer;
        
        [Header("Resource Values")]
        [SerializeField] private TextMeshProUGUI spiceText;
        [SerializeField] private TextMeshProUGUI waterText;
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI knowledgeText;
        [SerializeField] private TextMeshProUGUI populationText;
        
        [Header("Production Preview")]
        [SerializeField] private TextMeshProUGUI spiceProductionText;
        [SerializeField] private TextMeshProUGUI waterProductionText;
        [SerializeField] private TextMeshProUGUI creditsProductionText;
        
        [Header("Visual Indicators")]
        [SerializeField] private Image spiceIcon;
        [SerializeField] private Image waterIcon;
        [SerializeField] private Image creditsIcon;
        
        [Header("Colors")]
        [SerializeField] private Color positiveChangeColor = Color.green;
        [SerializeField] private Color negativeChangeColor = Color.red;
        [SerializeField] private Color neutralColor = Color.white;

        private Dictionary<ResourceType, ResourceDisplayData> _resourceDisplays = new();
        private Dictionary<ResourceType, double> _previousValues = new();

        protected override void Start()
        {
            base.Start();
            InitializeResourceDisplays();
        }

        private void InitializeResourceDisplays()
        {
            _resourceDisplays[ResourceType.Spice] = new ResourceDisplayData
            {
                Text = spiceText,
                ProductionText = spiceProductionText,
                Icon = spiceIcon
            };
            
            _resourceDisplays[ResourceType.Water] = new ResourceDisplayData
            {
                Text = waterText,
                ProductionText = waterProductionText,
                Icon = waterIcon
            };
            
            _resourceDisplays[ResourceType.Credits] = new ResourceDisplayData
            {
                Text = creditsText,
                ProductionText = creditsProductionText,
                Icon = creditsIcon
            };
            
            _resourceDisplays[ResourceType.Knowledge] = new ResourceDisplayData
            {
                Text = knowledgeText
            };
            
            _resourceDisplays[ResourceType.Population] = new ResourceDisplayData
            {
                Text = populationText
            };
        }

        protected override void RefreshUI()
        {
            if (GameManager.CurrentGameState == null) return;

            var state = GameManager.CurrentGameState;
            var preview = GameManager.PreviewProduction();

            foreach (var kvp in _resourceDisplays)
            {
                if (state.Inventory.TryGetResource(kvp.Key, out var resource))
                {
                    UpdateResourceDisplay(kvp.Key, resource.Amount, preview.GetValueOrDefault(kvp.Key, 0));
                    _previousValues[kvp.Key] = resource.Amount;
                }
            }
        }

        private void UpdateResourceDisplay(ResourceType type, double amount, double production)
        {
            if (!_resourceDisplays.TryGetValue(type, out var displayData)) return;
            
            string formattedAmount = FormatResourceAmount(amount);
            SafeUpdateText(displayData.Text, formattedAmount);

            if (displayData.ProductionText != null && production != 0)
            {
                string sign = production > 0 ? "+" : "";
                string colorHex = production > 0 ? "#4CAF50" : "#F44336";
                ColorUtility.TryParseHtmlString(colorHex, out var color);
                
                displayData.ProductionText.text = $"<color=#{colorHex}>{sign}{production:F0}/mes</color>";
            }

            if (displayData.Icon != null)
            {
                AnimateResourceIcon(displayData.Icon, production);
            }
        }

        private string FormatResourceAmount(double amount)
        {
            if (amount >= 1000000)
                return $"{amount / 1000000:F1}M";
            if (amount >= 1000)
                return $"{amount / 1000:F1}K";
            return $"{amount:F0}";
        }

        private void AnimateResourceIcon(Image icon, double production)
        {
            if (production > 0)
            {
                icon.color = positiveChangeColor;
            }
            else if (production < 0)
            {
                icon.color = negativeChangeColor;
            }
            else
            {
                icon.color = neutralColor;
            }
        }

        private void OnDestroy()
        {
            _resourceDisplays.Clear();
            _previousValues.Clear();
        }

        private class ResourceDisplayData
        {
            public TextMeshProUGUI Text { get; set; }
            public TextMeshProUGUI ProductionText { get; set; }
            public Image Icon { get; set; }
        }
    }

    public class ResourceItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI productionText;
        [SerializeField] private Slider capacitySlider;

        public void Initialize(ResourceType type, double amount, double maxCapacity, double production)
        {
            nameText.text = type.ToString();
            amountText.text = $"{amount:F0} / {maxCapacity:F0}";
            
            if (capacitySlider != null)
            {
                capacitySlider.maxValue = maxCapacity;
                capacitySlider.value = amount;
            }

            if (productionText != null)
            {
                productionText.text = production >= 0 ? $"+{production:F0}" : $"{production:F0}";
                productionText.color = production >= 0 ? Color.green : Color.red;
            }
        }

        public void UpdateAmount(double amount, double maxCapacity, double production)
        {
            amountText.text = $"{amount:F0} / {maxCapacity:F0}";
            
            if (capacitySlider != null)
            {
                capacitySlider.value = amount;
            }

            if (productionText != null)
            {
                productionText.text = production >= 0 ? $"+{production:F0}" : $"{production:F0}";
                productionText.color = production >= 0 ? Color.green : Color.red;
            }
        }
    }
}

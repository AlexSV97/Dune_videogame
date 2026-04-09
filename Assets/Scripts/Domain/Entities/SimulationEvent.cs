using System;
using System.Collections.Generic;
using DuneArrakisDominion.Domain.Enums;

namespace DuneArrakisDominion.Domain.Entities
{
    [Serializable]
    public class SimulationEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EventType Type { get; set; }
        public int MonthTriggered { get; set; }
        public double Severity { get; set; }
        public bool IsResolved { get; set; }
        public Dictionary<ResourceType, double> ResourceImpact { get; set; } = new();
        public List<string> AffectedEnclaveIds { get; set; } = new();
        public List<string> AffectedCreatureIds { get; set; } = new();
        public string RecommendedAction { get; set; }
        public bool RequiresPlayerDecision { get; set; }
        public List<EventChoice> Choices { get; set; } = new();

        public SimulationEvent() { }

        public static SimulationEvent GenerateRandomEvent(int currentMonth)
        {
            var random = new Random();
            var eventType = (EventType)random.Next(Enum.GetValues(typeof(EventType)).Length);
            var severity = random.NextDouble() * 0.5 + 0.5;

            var evt = new SimulationEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = eventType,
                MonthTriggered = currentMonth,
                Severity = severity,
                IsResolved = false,
                RequiresPlayerDecision = random.NextDouble() > 0.7
            };

            SetEventDetails(evt, eventType);
            return evt;
        }

        private static void SetEventDetails(SimulationEvent evt, EventType type)
        {
            switch (type)
            {
                case EventType.Sandstorm:
                    evt.Name = "Tormenta de Arena";
                    evt.Description = "Una poderosa tormenta de arena azota la región.";
                    evt.ResourceImpact[ResourceType.Water] = -50 * evt.Severity;
                    evt.RecommendedAction = "Refugiar unidades y activar defensas de tormenta.";
                    break;

                case EventType.SpiceBlow:
                    evt.Name = "Explosión de Especia";
                    evt.Description = "Un géisers de especia ha surgido en el desierto.";
                    evt.ResourceImpact[ResourceType.Spice] = 200 * evt.Severity;
                    evt.RecommendedAction = "Desplegar inmediatamente equipos de recolección.";
                    break;

                case EventType.PoliticalUprising:
                    evt.Name = "Levantamiento Político";
                    evt.Description = "Los Fremen protestan por las políticas imperiales.";
                    evt.ResourceImpact[ResourceType.Population] = -20 * evt.Severity;
                    evt.RecommendedAction = "Negociar o reforzar presencia militar.";
                    evt.Choices.Add(new EventChoice("Negociar", 50, new Dictionary<ResourceType, double> { { ResourceType.Credits, -100 } }));
                    evt.Choices.Add(new EventChoice("Represión", -20, new Dictionary<ResourceType, double>()));
                    break;

                case EventType.ImperialInspection:
                    evt.Name = "Inspección Imperial";
                    evt.Description = "Mensajeros del Emperor llegan para inspeccionar.";
                    evt.ResourceImpact[ResourceType.Credits] = -150 * evt.Severity;
                    evt.RecommendedAction = "Preparar tributo y apariencia de lealtad.";
                    break;

                case EventType.TradeOpportunity:
                    evt.Name = "Oportunidad Comercial";
                    evt.Description = "Comerciantes Guild ofrecen tratos beneficiosos.";
                    evt.ResourceImpact[ResourceType.Credits] = 100 * evt.Severity;
                    evt.RecommendedAction = "Aceptar el acuerdo comercial.";
                    break;

                case EventType.Attack:
                    evt.Name = "Ataque Inesperado";
                    evt.Description = "Fuerzas hostiles han sido detectadas.";
                    evt.ResourceImpact[ResourceType.Population] = -30 * evt.Severity;
                    evt.RecommendedAction = "Movilizar defensas y contraatacar.";
                    break;

                case EventType.Plague:
                    evt.Name = "Plaga del Desierto";
                    evt.Description = "Una enfermedad spread among settlers.";
                    evt.ResourceImpact[ResourceType.Population] = -25 * evt.Severity;
                    evt.ResourceImpact[ResourceType.Water] = -30 * evt.Severity;
                    evt.RecommendedAction = "Cuarentena y distribución de agua limpia.";
                    break;

                case EventType.Discovery:
                    evt.Name = "Descubrimiento Arqueoológico";
                    evt.Description = "Ruinas antiguas han sido encontradas.";
                    evt.ResourceImpact[ResourceType.Knowledge] = 50 * evt.Severity;
                    evt.RecommendedAction = "Investigar los hallazgos.";
                    break;
            }
        }

        public void ApplyEvent(Inventory inventory)
        {
            foreach (var impact in ResourceImpact)
            {
                if (impact.Value > 0)
                {
                    inventory.AddResource(impact.Key, impact.Value);
                }
                else
                {
                    inventory.SpendResource(impact.Key, Math.Abs(impact.Value));
                }
            }
            IsResolved = true;
        }
    }

    [Serializable]
    public class EventChoice
    {
        public string ChoiceName { get; set; }
        public double LoyaltyChange { get; set; }
        public Dictionary<ResourceType, double> ResourceCosts { get; set; }
        public Dictionary<ResourceType, double> ResourceRewards { get; set; }
        public string OutcomeDescription { get; set; }

        public EventChoice(string name, double loyaltyChange, Dictionary<ResourceType, double> costs)
        {
            ChoiceName = name;
            LoyaltyChange = loyaltyChange;
            ResourceCosts = costs;
            ResourceRewards = new Dictionary<ResourceType, double>();
        }

        public bool CanAfford(Inventory inventory)
        {
            foreach (var cost in ResourceCosts)
            {
                if (!inventory.HasResource(cost.Key, Math.Abs(cost.Value)))
                    return false;
            }
            return true;
        }

        public void Apply(Inventory inventory)
        {
            foreach (var cost in ResourceCosts)
            {
                inventory.SpendResource(cost.Key, Math.Abs(cost.Value));
            }
            foreach (var reward in ResourceRewards)
            {
                inventory.AddResource(reward.Key, reward.Value);
            }
        }
    }
}

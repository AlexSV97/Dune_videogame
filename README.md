# Dune-Arrakis-Dominion

Videojuego de estrategia y simulación basado en el universo de Dune, desarrollado en Unity 3D con C#.

## Arquitectura del Proyecto

```
Assets/
├── Scripts/
│   ├── Domain/           # Entidades puras de C# (sin MonoBehaviour)
│   │   ├── Entities/     # GameState, Creature, Enclave, Facility, etc.
│   │   └── Enums/        # CreatureType, FacilityType, ResourceType, etc.
│   ├── Managers/         # Controladores de juego (MonoBehaviour)
│   │   ├── GameManager.cs       # Singleton principal
│   │   └── SimulationEngine.cs  # Cerebro matemático
│   ├── Services/         # Lógica de negocio
│   │   ├── PersistenceService.cs   # Guardado/Carga
│   │   ├── CalculationService.cs    # Cálculos matemáticos
│   │   └── CrewAiClient.cs         # Cliente HTTP para IA
│   ├── UI/
│   │   ├── Components/   # Componentes reutilizables
│   │   └── Panels/       # Paneles de la interfaz
│   └── Visuals/
│       └── Camera/       # Control de cámara
├── Prefabs/              # Prefabs de objetos
├── Scenes/              # Escenas de Unity
└── Tests/               # Pruebas unitarias

automatization/
├── agents/              # Agentes CrewAI
│   └── crewai_monthly_decision.py
└── requirements.txt     # Dependencias Python
```

## Características Principales

- **Sistema de Simulación**: Motor matemático que calcula producción, combates y eventos
- **Sistema de Persistencia**: Guardado/carga en JSON local
- **Integración IA**: Agentes CrewAI para recomendaciones estratégicas
- **Arquitectura Limpia**: Separación entre dominio, servicios y presentación
- **Patrón Observer**: UI reactiva a cambios de estado

## Requisitos

- Unity 2021.3 LTS o superior
- TextMeshPro
- .NET 4.x o .NET Standard 2.0

## Guía de Configuración Detallada

Consulta **[UNITY_SETUP_GUIDE.md](./UNITY_SETUP_GUIDE.md)** para instrucciones paso a paso de:
- Cómo crear el proyecto en Unity Hub
- Configurar Assembly Definitions
- Crear la escena y el HUD
- Ejecutar tests
- Configurar la API Python
- Hacer build

## Instalación Rápida

1. **Seguir la guía completa** → `UNITY_SETUP_GUIDE.md`
2. **Importar TMP Essential Resources** (Window → TextMeshPro → Import)
3. **Ejecutar tests** (Window → General → Test Runner → Run All)

## Configuración de la API Python

```bash
cd automatization
pip install -r requirements.txt
python agents/crewai_monthly_decision.py
```

La API estará disponible en `http://localhost:5000`

## Uso

### Iniciar Nuevo Juego
```csharp
GameManager.Instance.StartNewGame("Paul Atreides", DifficultyLevel.Standard);
```

### Simular Mes
```csharp
var result = GameManager.Instance.SimulateMonth();
```

### Solicitar Recomendaciones IA
```csharp
var recommendations = await GameManager.Instance.RequestFullAIAnalysis();
```

## Testing

Los tests están en `Assets/Tests/EditMode/` y cubren:

- **CalculationServiceTests**: Validación de cálculos matemáticos
- **SimulationEngineTests**: Lógica de simulación
- **DomainEntityTests**: Comportamiento de entidades

## Licencia

MIT License

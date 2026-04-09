# Dune-Arrakis-Dominion - Configuración en Unity

## 1. Crear el Proyecto en Unity

1. **Abrir Unity Hub** → **New Project**
2. Seleccionar **3D (Built-in Render Pipeline)** o **3D (URP)**
3. Nombre del proyecto: `DuneArrakisDominion`
4. Location: Elegir ubicación
5. Click **Create Project**

## 2. Configurar el Assembly Definition

Para mejor organización y compilación, crear Assembly Definition Files:

### `Assets/Assembly-CSharp.asmdef`
```json
{
    "name": "DuneArrakisDominion",
    "rootNamespace": "DuneArrakisDominion",
    "references": [
        "Unity.TextMeshPro"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### `Assets/Tests/Assembly-CSharp-Tests.asmdef`
```json
{
    "name": "DuneArrakisDominion.Tests",
    "rootNamespace": "DuneArrakisDominion.Tests",
    "references": [
        "DuneArrakisDominion"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

## 3. Instalar Paquetes Necesarios

### Desde Window → Package Manager:

1. **TextMeshPro** (si no está instalado)
   - Window → TextMeshPro → Import TMP Essential Resources

2. **Unity Test Framework**
   - Window → Package Manager → Unity Registry → Test Framework → Install

3. **JSON Serialization** (ya incluido en Unity)

## 4. Estructura de Escenas

### Escena Principal: `MainScene.unity`

#### Configuración del Canvas:
1. **Create → UI → Canvas**
2. Canvas Scaler:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
   - Match: `0.5`

#### Configuración del HUD:
1. **Canvas**
   - **ResourceHudPanel** (padre)
     - ResourceItem prefab × 5 (Spice, Water, Credits, Knowledge, Population)
     - Producción mensual preview
   - **GameHudPanel** (padre)
     - **TopBar**: Month/Year, Phase indicator
     - **ActionButtons**: NextMonth, Save, Load, AIAdvice, Menu
     - **BottomBar**: Facilities, Creatures, Events shortcuts
   - **EventNotificationPanel** (overlay)
   - **NotificationToast** (para alertas rápidas)

#### Configuración del Mapa 3D:
1. **Create → 3D Object → Plane** (Terreno base)
2. **Main Camera** con `CameraController.cs`
3. Luz direccional para simular sol de Arrakis
4. **EnclaveMarkers** como prefabs 3D o Sprites

## 5. Configuración de Prefabs

### EnclaveMarker Prefab
```
EnclaveMarker (Prefab)
├── Model (3D mesh o Sprite)
├── SelectionIndicator (Circle/Highlight)
├── InfoPopup (Canvas - opcional)
├── EnclaveVisuals.cs (componente)
└── BoxCollider (para raycasts)
```

### FacilityMarker Prefab
```
FacilityMarker (Prefab)
├── Model (3D mesh)
├── StatusIndicator (veredio/rojo)
├── FacilityVisuals.cs
└── SphereCollider
```

## 6. GameManager Setup

1. **Create Empty** → Nombre: `GameManager`
2. Añadir componentes:
   - `GameManager.cs`
   - `CrewAiClient.cs`
3. Configurar en Inspector:
   - Base URL: `http://localhost:5000`
   - Request Timeout: `30`

## 7. Cámaras

### MainCamera
1. Position: `(0, 50, -50)`
2. Rotation: `(55, 0, 0)`
3. Añadir `CameraController.cs`

## 8. Iluminación

### Directional Light (Sun)
- Rotation: `(50, -30, 0)`
- Intensity: `1.2`
- Color: `#FFE4B5` (arena cálida)

### Ambient Light
- Color: `#FFD699`
- Intensity: `0.4`

## 9. Materiales del Desierto

Crear materiales para el terreno de Arrakis:
- **SandMaterial**: Color arena, Low Specular
- **RockMaterial**: Color roca, High Specular

## 10. Configuración de Build

### Player Settings (Edit → Project Settings → Player)
- Company Name: `YourStudio`
- Product Name: `Dune-Arrakis-Dominion`
- Default Icon: Logo de Dune

### Quality Settings
- Medium o Low para mejor rendimiento
- Shadow Quality: Disable o Low

## 11. Ejecutar Tests

1. **Window → General → Test Runner**
2. Click **Run All** en EditMode tests
3. Verificar que todos los tests pasen

## 12. Configuración del API Python

### Instalación de Dependencias:

```bash
cd automatization
pip install -r requirements.txt
```

### Ejecutar el Servidor:

```bash
python agents/crewai_monthly_decision.py
```

El servidor estará disponible en `http://localhost:5000`

### Endpoints Disponibles:
- `POST /api/analyze` - Análisis completo
- `POST /api/mentat/financial` - Consejos financieros
- `POST /api/beastmaster/advice` - Consejos sobre criaturas
- `GET /api/health` - Estado del servicio

## 13. Workflow de Desarrollo

### Flujo de Trabajo:
1. **Desarrollar en Unity** (scripts C#)
2. **Ejecutar tests** antes de commit
3. **Testear con API Python** para integración
4. **Build & Test** en plataforma objetivo

### Comandos Útiles:
```bash
# En Unity (desde terminal)
unity -projectPath . -buildTarget Windows -executeMethod BuildScript.Build

# En Python
python agents/crewai_monthly_decision.py
```

## 14. Configuración de Versionado

.gitignore recomendado:
```
# Unity
Library/
Temp/
Obj/
Build/
Logs/
UserSettings/

# Python
__pycache__/
*.pyc
.venv/
*.egg-info/

# IDE
.vscode/
.idea/

# OS
.DS_Store
Thumbs.db

# Saves (opcional - guardar solo en persistencia)
# Saves/
```

## 15. Próximos Pasos

1. [ ] Implementar modelos 3D de enclaves
2. [ ] Crear animaciones de eventos
3. [ ] Implementar sistema de audio
4. [ ] Añadir más tipos de instalaciones
5. [ ] Implementar IA enemiga
6. [ ] Sistema de guardado en la nube
7. [ ] Multijugador básico

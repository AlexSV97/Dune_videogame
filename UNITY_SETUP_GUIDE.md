# Guía Completa: Configurar Dune-Arrakis-Dominion en Unity Hub

## Tabla de Contenidos
1. [Requisitos Previos](#requisitos-previos)
2. [Paso 1: Crear Proyecto en Unity Hub](#paso-1-crear-proyecto-en-unity-hub)
3. [Paso 2: Copiar Estructura de Archivos](#paso-2-copiar-estructura-de-archivos)
4. [Paso 3: Configurar Assembly Definitions](#paso-3-configurar-assembly-definitions)
5. [Paso 4: Instalar Paquetes](#paso-4-instalar-paquetes)
6. [Paso 5: Crear la Escena Principal](#paso-5-crear-la-escena-principal)
7. [Paso 6: Configurar GameManager](#paso-6-configurar-gamemanager)
8. [Paso 7: Crear UI del HUD](#paso-7-crear-ui-del-hud)
9. [Paso 8: Configurar Cámaras y Luces](#paso-8-configurar-cámaras-y-luces)
10. [Paso 9: Ejecutar Tests](#paso-9-ejecutar-tests)
11. [Paso 10: Configurar Python API](#paso-10-configurar-python-api)
12. [Paso 11: Build y Prueba](#paso-11-build-y-prueba)

---

## Requisitos Previos

| Software | Versión Mínima | Descarga |
|----------|----------------|----------|
| Unity Hub | 3.0+ | [unity.com/download](https://unity.com/download) |
| Unity Editor | 2021.3 LTS o 2022.3 LTS | Se instala desde Unity Hub |
| Visual Studio | 2019/2022 | [visualstudio.com](https://visualstudio.com) |
| Python | 3.9+ | [python.org](https://python.org) |
| Git | Cualquier versión | [git-scm.com](https://git-scm.com) |

---

## Paso 1: Crear Proyecto en Unity Hub

### 1.1 Abrir Unity Hub
```
1. Ejecutar Unity Hub
2. Ir a la pestaña "Projects"
```

### 1.2 Crear Nuevo Proyecto
```
1. Click en "New Project"
2. Seleccionar "3D (Built-in Render Pipeline)" o "3D (URP)"
3. Configurar:
   - Project Name: DuneArrakisDominion
   - Location: C:\Users\PC\OneDrive\Desktop\Dune_videogame\Unity (crear esta carpeta primero)
4. Click "Create Project"
```

### 1.3 Esperar a que cargue
```
- Unity abrirá el proyecto
- Esperar a que el progreso llegue al 100%
- La barra de progreso está en la esquina inferior derecha
```

---

## Paso 2: Copiar Estructura de Archivos

### 2.1 Verificar Estructura Actual
```
En Unity, ir a Project Window (por defecto abajo a la izquierda)
Deberías ver:
- Assets/
- Packages/
- ProjectSettings/
```

### 2.2 Crear Carpetas desde Unity
```
1. En Project Window, click derecho en Assets
2. Create > Folder
3. Nombrar las carpetas exactamente como se indica:

Assets/
├── Scripts/
│   ├── Domain/
│   │   ├── Entities/
│   │   └── Enums/
│   ├── Managers/
│   ├── Services/
│   ├── UI/
│   │   ├── Components/
│   │   └── Panels/
│   └── Visuals/
│       └── Camera/
├── Prefabs/
├── Scenes/
├── Resources/
└── Tests/
    └── EditMode/
```

### 2.3 Copiar Scripts desde tu Carpeta
```
Desde el explorador de archivos:
1. Copiar todos los archivos .cs de la carpeta Scripts
2. Pegarlos en las carpetas correspondientes dentro de Unity

También puedes arrastrar archivos desde el explorador de Windows
directamente a las carpetas en Unity.
```

### 2.4 Refrescar Proyecto
```
1. En Unity: Window > General > Project
2. Click derecho en Assets > Refresh
O usar Ctrl+R para refrescar
```

---

## Paso 3: Configurar Assembly Definitions

### 3.1 Crear Assembly Principal
```
1. Click derecho en Assets
2. Create > Assembly Definition
3. Nombrar: "DuneArrakisDominion"
4. Double-click para abrir y configurar:
```

**DuneArrakisDominion.asmdef:**
```json
{
    "name": "DuneArrakisDominion",
    "rootNamespace": "DuneArrakisDominion",
    "references": [],
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

### 3.2 Crear Assembly de Tests
```
1. Click derecho en Assets/Tests
2. Create > Assembly Definition
3. Nombrar: "DuneArrakisDominion.Tests"
4. Configurar:
```

**DuneArrakisDominion.Tests.asmdef:**
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

---

## Paso 4: Instalar Paquetes

### 4.1 TextMeshPro
```
1. Window > TextMeshPro > Import TMP Essential Resources
2. En la ventana que aparece, click "Import"
3. Esperar a que termine la importación
```

### 4.2 Unity Test Framework
```
1. Window > Package Manager
2. En el dropdown superior, seleccionar "Unity Registry"
3. Buscar "Test Framework"
4. Click en el paquete > Install
5. Esperar a que instale
```

### 4.3 JSON Package (ya incluido)
```
El paquete JSON ya viene incluido en Unity por defecto.
No necesitas instalar nada adicional.
```

---

## Paso 5: Crear la Escena Principal

### 5.1 Crear Escena
```
1. File > New Scene (o Ctrl+N)
2. Seleccionar "Basic (Built-in)" o "Empty"
3. File > Save Scene (Ctrl+Shift+S)
4. Guardar como "MainScene" en Assets/Scenes/
```

### 5.2 Configurar Terreno
```
1. GameObject > 3D Object > Plane
2. Renombrar a "DesertFloor"
3. Configurar Transform:
   - Position: (0, 0, 0)
   - Scale: (500, 1, 500)
4. Crear material de arena:
   a. Right-click en Assets > Create > Material
   b. Nombrar "SandMaterial"
   c. En Inspector:
      - Albedo: Color arena (#C2B280)
      - Smoothness: 0.1
   d. Arrastrar el material al Plane
```

### 5.3 Crear Terreno Decorativo
```
1. GameObject > 3D Object > Terrain
2. Configurar tamaño: (500, 50, 500)
3. Añadir colinas simples con la herramienta de Terrain
```

---

## Paso 6: Configurar GameManager

### 6.1 Crear GameManager
```
1. GameObject > Create Empty
2. Renombrar a "GameManager"
3. Posición: (0, 0, 0)
```

### 6.2 Añadir Componentes
```
En el Inspector del GameManager:
1. Add Component > Buscar "GameManager" > Click
2. Add Component > Buscar "CrewAiClient" > Click
3. Configurar CrewAiClient:
   - Base URL: http://localhost:5000
   - Request Timeout: 30
```

### 6.3 Persistir entre Escenas
```
El GameManager ya tiene DontDestroyOnLoad en el script.
Asegúrate de que el script esté correcto:
- El GameManager se marcará con [DisallowMultipleComponent]
```

---

## Paso 7: Crear UI del HUD

### 7.1 Crear Canvas
```
1. GameObject > UI > Canvas
2. Renombrar a "HUDCanvas"
3. En Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match: 0.5
4. Screen Match Mode: Expand
```

### 7.2 Crear Panel Superior (Recursos)
```
1. Right-click en HUDCanvas > UI > Panel
2. Renombrar a "ResourcePanel"
3. Configurar RectTransform:
   - Anchor: Top Stretch
   - Pivot: (0.5, 1)
   - Top: 0
   - Left: 0
   - Right: 0
   - Height: 80
4. Color: Semi-transparente negro (Alpha: 150)
```

### 7.3 Crear Elementos de Recursos
```
Para cada recurso (Spice, Water, Credits, Knowledge, Population):

1. Right-click en ResourcePanel > UI > Panel
2. Nombrar según recurso (ej: "SpiceDisplay")
3. Añadir como hijos:
   - Image (ícono del recurso)
   - TextMeshPro (nombre y valor)
   - TextMeshPro (producción mensual)

Repetir para cada recurso.
```

### 7.4 Crear Panel de Botones
```
1. Right-click en HUDCanvas > UI > Panel
2. Renombrar a "ActionPanel"
3. Posición: Bottom Center
4. Configurar altura: 60
5. Añadir botones hijos:
   - "NextMonthButton" - "Siguiente Mes"
   - "SaveButton" - "Guardar"
   - "LoadButton" - "Cargar"
   - "AIAdviceButton" - "Consultar IA"
```

### 7.5 Crear Panel de Fecha
```
1. Right-click en HUDCanvas > UI > Panel
2. Renombrar a "DatePanel"
3. Posición: Top Left
4. Offset X: 20, Y: -20
5. Añadir TextMeshPro con formato:
   "Mes: [MES] - Año: [AÑO]"
   "Fase: [FASE]"
```

### 7.6 Asignar Scripts a la UI
```
1. Crear GameObject vacío "HUDManager"
2. Añadir componente "GameHudPanel"
3. Arrastrar los elementos de UI a los campos del Inspector:
   - Resource Display refs
   - Action Buttons refs
   - Date Display refs
```

---

## Paso 8: Configurar Cámaras y Luces

### 8.1 Configurar Cámara Principal
```
1. Seleccionar Main Camera
2. Configurar Transform:
   - Position: (0, 50, -50)
   - Rotation: (55, 0, 0)
3. Añadir componente "CameraController"
4. Configurar límites en Inspector:
   - Min Height: 10
   - Max Height: 100
   - Pan Speed: 30
   - Rotation Speed: 50
```

### 8.2 Configurar Luz Direccional
```
1. Seleccionar Directional Light (o crear nuevo)
2. Transform:
   - Rotation: (50, -30, 0)
3. Light Component:
   - Intensity: 1.2
   - Color: #FFE4B5
```

### 8.3 Luz Ambiental
```
1. Window > Rendering > Lighting
2. En Environment:
   - Ambient Color: #FFD699
   - Ambient Intensity: 0.4
```

### 8.4 Skybox (Opcional)
```
1. Window > Rendering > Lighting
2. Environment tab
3. Skybox Material: Asignar uno de los presets de Unity
   o crear uno estilo atardecer de Dune
```

---

## Paso 9: Ejecutar Tests

### 9.1 Abrir Test Runner
```
1. Window > General > Test Runner
2. Click en "PlayMode" o "EditMode" tab
```

### 9.2 Ejecutar EditMode Tests
```
1. Click en "Run All" en EditMode tab
2. Esperar resultados
3. Verificar que todos los tests pasen (verde)
```

### 9.3 Si Hay Errores de Compilación
```
1. Revisar Console (Window > General > Console)
2. Errores comunes:
   - Namespace incorrecto → Verificar "DuneArrakisDominion" en todos los archivos
   - Missing references → Recompilar (Assets > Scripts > Compile)
   - Missing assemblies → Verificar .asmdef files
```

### 9.4 Forzar Recompilación
```
1. Assets > Scripts > Compile
O
1. Click derecho en Assembly Definition
2. Reimport
```

---

## Paso 10: Configurar Python API

### 10.1 Instalar Python (si no está)
```
1. Descargar Python 3.9+ de python.org
2. Durante instalación, MARCAR "Add Python to PATH"
3. Verificar: Abrir terminal y escribir
   python --version
```

### 10.2 Crear Entorno Virtual
```
Abrir Terminal en la carpeta automatization:

cd C:\Users\PC\OneDrive\Desktop\Dune_videogame\automatization

python -m venv venv

Activar entorno:
- Windows: .\venv\Scripts\activate
```

### 10.3 Instalar Dependencias
```
pip install -r requirements.txt

Paquetes que se instalarán:
- crewai
- crewai-tools
- fastapi
- uvicorn
- pydantic
- httpx
```

### 10.4 Ejecutar la API
```
python agents/crewai_monthly_decision.py

Deberías ver:
INFO:     Uvicorn running on http://0.0.0.0:5000
```

### 10.5 Probar la API
```
Abrir navegador e ir a:
http://localhost:5000

Deberías ver el JSON de estado de la API.
```

---

## Paso 11: Build y Prueba

### 11.1 Configurar Build Settings
```
1. File > Build Settings
2. Platform: Windows, Mac, Linux (según tu SO)
3. Click "Switch Platform"
4. Añadir Scene:
   - Drag MainScene desde Project a Scenes In Build
```

### 11.2 Configurar Player Settings
```
1. Edit > Project Settings > Player
2. Other Settings:
   - Company Name: TuNombre
   - Product Name: Dune-Arrakis-Dominion
   - Version: 0.1.0
   - Default Icon: (opcional) tu logo
```

### 11.3 Hacer Build
```
1. File > Build Settings
2. Click "Build"
3. Elegir ubicación de salida
4. Esperar compilación
5. Ejecutar el .exe generado
```

### 11.4 Probar en Unity (Play Mode)
```
1. Click en botón Play (▶) en Unity
2. La escena debería cargar
3. En Console deberías ver "Game loaded successfully"
4. Click en "Next Month" para probar simulación
```

---

## Solución de Problemas Comunes

### Error: "The type or namespace name 'DuneArrakisDominion' could not be found"
```
Solución:
1. Verificar que los namespaces coincidan con las carpetas
2. Assets > Scripts > Compile
3. Si persiste: Restart Unity
```

### Error: "Assembly has reference to itself"
```
Solución:
1. Abrir DuneArrakisDominion.asmdef
2. Remover cualquier referencia en "references"
3. Save y recompilar
```

### Error: "JsonSerializer not found"
```
Solución:
System.Text.Json está incluido en .NET Standard 2.1
Verificar que el Assembly Definition no tenga:
"noEngineReferences": true

Cambiar a false si es necesario.
```

### Error: "UnityWebRequest requires internet access"
```
Solución:
- La API local (localhost) funciona sin internet
- Si probando con servidor real, verificar firewall
```

### Tests no aparecen en Test Runner
```
Solución:
1. Ensurer que el Assembly Definition de Tests tenga:
   "includePlatforms": ["Editor"]
2. Reiniciar Unity
3. Window > General > Test Runner > Restart
```

---

## Checklist Final

```
□ Unity Hub abierto
□ Proyecto creado con nombre correcto
□ Todos los scripts copiados
□ Assembly Definitions creados
□ TextMeshPro importado
□ Test Framework instalado
□ Escena principal guardada
□ GameManager configurado
□ HUD creado con Canvas
□ Cámaras y luces configuradas
□ Tests pasando
□ Python API funcionando
□ Build completado exitosamente
```

---

## Próximos Pasos Después de Setup

1. **Crear Prefabs de Enclaves** - Modelos 3D para Sietch, Refinerías, etc.
2. **Implementar Sistema de Audio** - Música y efectos de sonido del desierto
3. **Añadir Animaciones** - Transiciones de UI y eventos
4. **Crear Más Escenas** - Menú principal, pantalla de carga
5. **Implementar IA Enemiga** - NPC que toman decisiones
6. **Sistema de Guardado en la Nube** - Cloud saves

---

## Recursos Adicionales

- Documentación Unity: [docs.unity3d.com](https://docs.unity3d.com)
- TextMeshPro: [docs.unity3d.com/Packages/com.unity.textmeshpro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0)
- CrewAI Docs: [docs.crewai.com](https://docs.crewai.com)
- FastAPI: [fastapi.tiangolo.com](https://fastapi.tiangolo.com)

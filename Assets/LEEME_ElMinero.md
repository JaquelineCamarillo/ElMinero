# Manual del Proyecto — "El Minero"

**Materia:** Creación de Videojuegos — UTNG (Universidad Tecnológica del Norte de Guanajuato)  
**Grupo:** GIDS6102-E  
**Desarrollador:** Camarillo Olaez  
**Motor:** Unity 6 (`6000.6.3f1`) — Plantilla Universal 2D (URP)  
**Plataformas objetivo:** Windows (Escritorio), WebGL (Navegadores web) y Android (APK)

---

## 1. Cómo abrir y construir el proyecto

El proyecto está diseñado para construirse con un solo clic de forma procedural y determinista a través de scripts de Editor:

1. Abre el proyecto en **Unity 6** seleccionando la carpeta raíz `C:\Users\juana\ElMinero`.
2. En la barra de menús superior de Unity, dirígete al menú:
   ```
   Herramientas > El Minero > ★ Construir TODO
   ```
3. Espera unos segundos mientras la barra de progreso ejecuta automáticamente:
   - **1. Carpetas:** Estructura completa y organizada en `Assets/`.
   - **2. Tags y Layers:** Registra `Player`, `Hazard`, `Collectible`, `Checkpoint`, `Exit`, layers `Suelo`, `Jugador`, `Peligro`, `Coleccionable`, sorting layers y matriz de colisión 2D.
   - **3. Sprites y Tilesets:** Configuración de todos los PNG a `PPU = 16`, `Point Filter`, sin compresión, y genera los tiles de `MinaRayas`, `ElNopal` y `LaValenciana`.
   - **4. Animaciones:** Generación de clips `.anim` y controladores `.controller` (Minero, Antorcha, Veta, Gema, Roca, PlataformaQuebradiza, InsigniaX2).
   - **5. Materiales y Configs:** `SinFriccion.physicsMaterial2D` y los ScriptableObjects `LevelConfig` de cada nivel.
   - **6. Prefabs:** Prefabs modulares con todas sus referencias conectadas en `Assets/Prefabs/`.
   - **7. Escenas:** Construcción de las 6 escenas completas (`MenuPrincipal`, `Nivel1_MinaRayas`, `Nivel2_ElNopal`, `Nivel3_LaValenciana`, `Victoria`, `Derrota`).
   - **8. Build Settings:** Ordenamiento de escenas del índice 0 al 5 y configuración de Player Settings.
4. Para probar el juego completo:
   - Abre la escena `Assets/Scenes/MenuPrincipal.unity`.
   - Haz clic en el botón **Play**.
   - Ingresa tu nombre (o deja "Refugio"), haz clic en **¡Iniciar Escape!** y juega los tres niveles hasta la victoria.

---

## 2. Lista de Scripts del Proyecto

### Jugador (`Assets/Scripts/Jugador/`)
- **`PlayerController.cs`**: Controla el movimiento horizontal a 5 u/s, salto calibrado a 2.5 u ($v = \sqrt{2gh}$), caída rápida, corte de salto, coyote time y jump buffer.
- **`PlayerHealth.cs`**: Gestiona las 3 vidas del minero, daño, invulnerabilidad con parpadeo visual (1.5 s) y eventos `OnLifeLost` y `OnDeath`.
- **`PlayerInputHandler.cs`**: Unifica la entrada de teclado (A/D, Espacio, Esc) con los controles táctiles de pantalla en móviles.
- **`PlayerAnimator.cs`**: Vincula los estados del jugador con el `Animator` (Speed, IsGrounded, VelocityY, Hurt, Dead, Win).
- **`NameLabel.cs`**: Muestra el nombre del jugador en texto flotante sobre el personaje sin invertirse al cambiar de dirección.

### Managers (`Assets/Scripts/Managers/`)
- **`GameManager.cs`**: Singleton persistente (`DontDestroyOnLoad`) que almacena el nombre del minero, puntaje acumulado, récord histórico y flujo global entre escenas.
- **`LevelManager.cs`**: Administrador de nivel; coordina tiempo, coleccionables, reaparición en checkpoints y reinicio integral con `IResettable`.
- **`LevelTimer.cs`**: Cuenta regresiva del derrumbe; alerta visual en rojo durante los últimos 10 segundos y derrota al llegar a cero.
- **`CollectibleManager.cs`**: Rastrea vetas y gemas recolectadas, y valida el bono de plata completa.
- **`SceneLoader.cs`**: Singleton que ejecuta transiciones suaves con fundido a negro (fade in / fade out) entre escenas.
- **`AudioManager.cs`**: Singleton seguro (`null-safe`) para reproducción de música con aceleración de tempo/pitch al final del tiempo y pool de efectos de sonido (SFX).

### Niveles y Entorno (`Assets/Scripts/Niveles/` y `Assets/Scripts/Entorno/`)
- **`LevelConfig.cs`**: ScriptableObject que define los parámetros de cada nivel (tiempo límite, vetas, gemas, rocas, color de fondo).
- **`Checkpoint.cs`**: Punto de guardado que al tocarse activa su luz mística azul y actualiza la posición de reaparición.
- **`LevelExit.cs`**: Trigger de salida que otorga el bono si se recolectó toda la plata y avanza al siguiente nivel.
- **`CaveInZone.cs`**: Trigger oculto que al cruzarse derriba un bloque de techo a espaldas del jugador cortando su retirada.
- **`BloqueDerrumbe.cs`**: Bloque físico pesado que cae durante el derrumbe sorpresa.
- **`MineTremor.cs`**: Genera temblores periódicos de cueva cada 20–25 s usando impulsos de Cinemachine.
- **`ParallaxBackground.cs`**: Desplaza las 3 capas de fondo a diferentes velocidades relativas a la cámara con repetición infinita.
- **`FlickeringLight2D.cs`**: Simula el parpadeo natural de antorchas y faroles usando ruido Perlin sobre luces 2D de URP.

### Plataformas (`Assets/Scripts/Plataformas/`)
- **`MovingPlatform.cs`**: Plataforma kinemática que se traslada suavemente entre dos puntos A y B con pausas en los extremos.
- **`BreakablePlatform.cs`**: Plataforma quebradiza que vibra al pisarse, colapsa a los 2 segundos y se restaura al reaparecer.

### Obstáculos y Coleccionables (`Assets/Scripts/Obstaculos/` y `Assets/Scripts/Coleccionables/`)
- **`FallingRock.cs`**: Roca dinámica giratoria que cae del techo y se destruye al impactar el suelo.
- **`RockSpawner.cs`**: Generador de rocas que proyecta una advertencia de sombra/polvo 0.6 s antes del impacto.
- **`DeathPit.cs`**: Zona letal en fosos o precipicios que resta una vida y regresa al jugador al checkpoint.
- **`Collectible.cs`**: Clase base para elementos recolectables con detección de triggers.
- **`SilverVein.cs`**: Veta de plata (+10 puntos).
- **`AmethystGem.cs`**: Gema de amatista (+25 puntos).
- **`StreakSystem.cs`**: Sistema de racha que otorga multiplicador x2 durante 10 s al juntar 3 minerales seguidos sin daño.

### Cámara y Utilidades (`Assets/Scripts/Camara/` y `Assets/Scripts/Utilidades/`)
- **`CameraFollowSetup.cs`**: Configuración y respaldo de seguimiento 2D garantizando la distancia $Z = -10$ y límites del nivel.
- **`CameraShake.cs`**: Sacudida de cámara local para eventos de impacto y derrumbe.
- **`Constantes.cs`**: Centralización de nombres de escenas, tags, layers, sorting layers y claves de PlayerPrefs.
- **`IResettable.cs`**: Interfaz implementada por todos los elementos que deben reiniciarse tras perder vidas.

### Interfaz de Usuario (`Assets/Scripts/UI/`)
- **`HUDController.cs`**: Visualiza vidas (3 corazones), reloj digital, contador de plata, puntaje e insignia x2 animada.
- **`MainMenuController.cs`**: Controla el Menú Principal, ingreso de nombre, modal de instrucciones y botón de salida.
- **`PauseMenu.cs`**: Menú de pausa accesible con Escape o botón en pantalla (`Time.timeScale = 0`).
- **`VictoryScreen.cs`**: Pantalla final de victoria con estadísticas personalizadas y opción de jugar de nuevo.
- **`DefeatScreen.cs`**: Pantalla de derrota con causa explicativa y botón de reintento.
- **`TouchControls.cs`**: Botones táctiles virtuales de gran tamaño para izquierda, derecha y salto en dispositivos móviles.

### Editor (`Assets/Scripts/Editor/`)
- **`ElMineroBuilder.cs`**: Menú maestro de construcción integral y opciones individuales.
- **`SpriteImportSetup.cs`**: Importador automatizado de sprites (PPU=16, Point filter, slicing y tiles).
- **`AnimationGenerator.cs`**: Generador procedural de clips `.anim` y controladores `.controller`.
- **`PrefabGenerator.cs`**: Constructor automatizado de todos los prefabs del proyecto.
- **`LevelBuilder.cs`**: Diseñador procedural de las 6 escenas completas del videojuego.

---

## 3. Cómo cambiar parámetros de un nivel

Todos los parámetros clave de los niveles están desacoplados en assets **ScriptableObject** tipo `LevelConfig` ubicados en:
```
Assets/Settings/Niveles/
├── LevelConfig_Nivel1.asset
├── LevelConfig_Nivel2.asset
└── LevelConfig_Nivel3.asset
```

Para modificarlos:
1. Selecciona cualquiera de los archivos `.asset` en el Project Explorer.
2. En el panel **Inspector** puedes alterar en tiempo real:
   - **Time Limit:** Tiempo de escape en segundos (ej. 90, 75, 70).
   - **Silver Vein Count / Amethyst Gem Count:** Cantidad requerida de coleccionables.
   - **Points:** Puntuación de vetas (10), gemas (25) y bono de plata completa (50).
   - **Rock Spawn Interval:** Frecuencia con la que caen las rocas (4.0s, 3.0s, 2.2s).
   - **Camera Background Color:** Color de fondo característico de la mina.

---

## 4. Cómo reemplazar sprites y agregar audios

### Reemplazo de Sprites
1. Todos los sprites originales se encuentran en `Assets/Sprites/` y `Assets/Tiles/`.
2. Para sustituir un sprite por una versión mejorada:
   - Guarda tu nueva imagen PNG con el **mismo nombre de archivo** en la misma carpeta.
   - Mantén la misma resolución o proporción de celdas (ej. Minero en celdas de 32×32, Tiles en 16×16).
   - Ve a `Herramientas > El Minero > 3. Configurar importación de sprites y crear Tile Palettes`.
   - Unity re-importará y rebanará automáticamente las hojas con PPU = 16 y filtro Point sin romper ninguna animación o escena.

### Incorporación de Audios
El sistema de sonido está preparado en `Assets/Audios/LEEME_Audios.txt`. Para añadir música y efectos:
1. Coloca tus archivos de música (`MusicaMenu`, `MusicaNivel1`, `MusicaNivel2`, `MusicaNivel3`, `MusicaVictoria`, `MusicaDerrota`) en `Assets/Audios/Musica/`.
2. Coloca tus efectos (`Pasos`, `Salto`, `CaidaRoca`, `ImpactoRoca`, `RecolectarPlata`, `RecolectarGema`, `ColapsoPlataforma`, `Temblor`, `DerrumbeSorpresa`, `PerderVida`, `Checkpoint`, `Click`) en `Assets/Audios/SFX/`.
3. Selecciona el prefab `[AudioManager]` en `Assets/Prefabs/Sistemas/` y arrastra los clips correspondientes a los campos del Inspector.

---

## 5. Cómo generar cada Build

El proyecto ya tiene configurados los Player Settings y la lista de escenas ordenada:
- `Scenes/MenuPrincipal.unity` (0)
- `Scenes/Nivel1_MinaRayas.unity` (1)
- `Scenes/Nivel2_ElNopal.unity` (2)
- `Scenes/Nivel3_LaValenciana.unity` (3)
- `Scenes/Victoria.unity` (4)
- `Scenes/Derrota.unity` (5)

Para compilar:

### A) Build para Windows (Escritorio)
1. En Unity, abre `File > Build Profiles` (o `Build Settings`).
2. Selecciona la plataforma **Windows, Mac, Linux** y como Target Platform **Windows (x86_64)**.
3. Haz clic en **Build** y selecciona una carpeta de salida (ej. `Builds/Windows/`).

### B) Build para WebGL (Navegadores)
1. Abre `File > Build Profiles`.
2. Selecciona **WebGL** y presiona **Switch Platform** si aún no está seleccionada.
3. El proyecto ya tiene configurado el `Decompression Fallback: true` para funcionar en cualquier servidor web sin requerir compresión gzip en el servidor.
4. Haz clic en **Build** y selecciona la carpeta (ej. `Builds/WebGL/`).

### C) Build para Android (APK)
1. Abre `File > Build Profiles`.
2. Selecciona **Android** y haz clic en **Switch Platform**.
3. El proyecto tiene preconfigurado:
   - Orientación forzada en **Landscape**.
   - API Mínima: **Android 7.0 'Nougat' (API Level 24)**.
4. Conecta tu dispositivo Android con depuración USB o selecciona **Build** para generar el archivo `ElMinero.apk`.

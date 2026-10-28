# Bitácora de Desarrollo — "El Minero"

**Proyecto:** El Minero (Plataformas 2D en Unity 6.6 / URP 2D)  
**Autora:** Juana Jaqueline Camarillo Olaez (UTNG — GIDS6102-E)  
**Materia / Docente:** Creación de Videojuegos / Gabriel Barrón Rodríguez  
**Fecha de inicio:** 28 de octubre de 2026  

---

## 1. Inventario Inicial del Proyecto (Paso 1)

### 1.1 Lo que existe y funciona
- **Estructura base del proyecto Unity 6 (6000.6.3f1) URP 2D**:
  - Configuración básica de proyecto con Universal Render Pipeline 17.6 y New Input System 1.20.
  - Física 2D configurada con `SinFriccion.physicsMaterial2D`.
  - Paquetes esenciales: Cinemachine 3.1.2, Input System 1.20, URP 17.6.
- **Scripts del Jugador (`Assets/Scripts/Jugador/`)**:
  - `PlayerController.cs`: Movimiento horizontal a 5 u/s, salto calibrado a 2.5 u ($v = \sqrt{2gh}$), gravedad multiplicada en caída (×1.5), corte de salto, coyote time (0.1 s), jump buffer (0.1 s).
  - `PlayerHealth.cs`: 3 vidas, sistema de daño e invulnerabilidad temporal (1.5 s con parpadeo visual), reaparición en checkpoints.
  - `PlayerInputHandler.cs`: Entrada para teclado y botones virtuales táctiles.
  - `PlayerAnimator.cs`: Conexión de estados del jugador con Animator.
  - `NameLabel.cs`: Etiqueta de texto flotante sobre el minero con el nombre del jugador.
- **Managers (`Assets/Scripts/Managers/`)**:
  - `GameManager.cs`: Singleton persistente con datos de sesión (nombre, puntaje).
  - `LevelManager.cs`: Coordinación de nivel, checkpoints, tiempo y reinicio.
  - `LevelTimer.cs`: Temporizador descendente con alerta en los últimos 10 s.
  - `CollectibleManager.cs`: Conteo de vetas de plata y gemas de amatista, verificación del bono.
  - `SceneLoader.cs`: Transición con fundido a negro entre escenas.
  - `AudioManager.cs`: Singleton básico de reproducción de audio.
- **Obstáculos, Coleccionables y Plataformas existentes**:
  - `SilverVein.cs`, `AmethystGem.cs`, `StreakSystem.cs` (racha x2 tras 3 coleccionables sin daño).
  - `RockSpawner.cs`, `FallingRock.cs` (rocas con sombra de aviso previa 0.6 s).
  - `DeathPit.cs` (zonas letales).
  - `MovingPlatform.cs` (movimiento horizontal entre puntos con pausas).
  - `BreakablePlatform.cs` (colapso a los 2 s, restauración a los 4 s).
  - `CaveInZone.cs`, `BloqueDerrumbe.cs` (derrumbe sorpresa que bloquea el camino de regreso).
  - `MineTremor.cs` (sacudidas con Cinemachine Impulse).
  - `Checkpoint.cs`, `LevelExit.cs`.
  - `ParallaxBackground.cs`, `FlickeringLight2D.cs`.
- **Arte y Audio base**:
  - Sprites base del paquete 1 (minero, tilesets de las 3 minas, vetas, amatistas, antorchas).
  - Recursos v2 recién integrados en `Assets/`: nuevos sprites de enemigos (murciélagos, araña, alacrán, vagoneta), minerales distintivos (aguilarita, pepita de oro, pirargirita), elementos de entorno (elevador, malacate, cable, riel, fuego, capilla, templo San Cayetano), interfaz renovada (estrellas, botones con estados normal/resaltado/presionado, sliders 9-slice, toggles, miniaturas, iconos, logo autora), fuentes TTF (PressStart2P y PixelifySans) y paquete completo de efectos WAV y pistas OGG.

### 1.2 Lo que existe incompleto o requiere refactorización
- **Control de entrada / Gamepad**:
  - `PlayerInputHandler.cs` no integra adecuadamente el esquema de Gamepad de Input System (stick, cruceta, botón Sur para saltar, botón Start para pausar).
- **Gestión de Audio**:
  - No existe el archivo `MezcladorPrincipal.mixer` con los grupos `Musica` (parámetro `VolMusica`) y `Efectos` (parámetro `VolEfectos`).
  - Falta la aceleración de pitch (1.08) y volumen (+2 dB) en el último 30% del tiempo.
  - No están configurados los AudioSources 3D para bucles ambientales (fuego, vagoneta, malacate).
- **Flujo de Escenas y Estados por Nivel**:
  - Las escenas `Victoria.unity` y `Derrota.unity` aún existen y deben ser sustituidas por paneles integrados dentro de cada nivel (`ResultsPanel` y `GameOverPanel`), más la nueva escena `Final.unity`.
  - Las escenas de prueba `SampleScene` y `EscenaPrueba_Fase1` deben ser depuradas/movidas.
  - No existen las escenas `Splash.unity`, `Intro.unity` y `Final.unity`.
- **Interfaz de Usuario (UI)**:
  - El Menú Principal carece de: selector de niveles con candados y miniaturas, panel de opciones completas con sliders/toggle/calidad/controles/borrado, y panel de créditos con scroll y botón Volver en la esquina superior izquierda (`Ignore Layout`).
  - En cada nivel faltan: tarjeta informativa inicial con dato histórico y botón "¡A escapar!", cartel emergente de 3 s al recoger el mineral distintivo, panel de resultados con conteo animado, evaluación estricta de 1 a 3 estrellas con rebote de escala y sfx progresivos, y panel de Game Over con causa explicativa.
- **Configuración e Importación de Sprites (v2)**:
  - Los sprites nuevos del paquete v2 no han sido rebanados con sus celdas exactas ni configurados con sus bordes 9-slice en `SpriteImportSetup.cs`.
- **Enemigos y Obstáculos nuevos**:
  - No existen scripts de IA para enemigos: falta `EnemyBase.cs`, `BatPatrol.cs`, `BatDiver.cs`, `SpiderDrop.cs`, `ScorpionAI.cs`, `RunawayCart.cs`, `FireHazard.cs`, `MineElevator.cs`.
- **Diseño de Niveles 1, 2 y 3**:
  - Las 3 minas no reflejan la totalidad de especificaciones históricas y mecánicas de la Tabla 5 (Mina de Rayas con elevador y tutorial; El Nopal con capilla, rieles y vagonetas; La Valenciana con maquinaria quemada, alacranes, murciélagos rojos, picadas y salida al Templo de San Cayetano).

### 1.3 Lo que falta por completo
- Clases de IA de enemigos y obstáculos programados (`EnemyBase`, `BatPatrol`, `BatDiver`, `SpiderDrop`, `ScorpionAI`, `RunawayCart`, `FireHazard`, `MineElevator`).
- Paneles UI modulares: `OptionsPanel`, `CreditsPanel`, `LevelSelectPanel`, `ResultsPanel`, `GameOverPanel`, `LevelStartCard`, `MineralPopup`.
- Gestor de guardado (`SaveManager.cs`) y gestor de configuración (`SettingsManager.cs`) con PlayerPrefs (`NivelDesbloqueado`, `Estrellas_N1..N3`, `MejorPuntaje_N1..N3`, `MejorTiempo_N1..N3`, `MineralEncontrado_N1..N3`, `NombreJugador`, volúmenes, calidad).
- Escenas `Splash`, `Intro` y `Final`.
- Métodos automatizados de build: `ElMineroBuilder.CompilarBuilds` para Windows (.exe), WebGL y Android (.apk).
- Documentación completa requerida: `CAMBIOS_GDD.md`, `MANUAL_USUARIO.md`, `CREDITOS.md`, `README.md`.

---

## 2. Decisiones Técnicas y de Diseño
- **Limpieza del repositorio**: Se retiraron `.vscode/`, `*.slnx`, `Assets/TextMesh Pro/Examples & Extras/`, `Assets/Welcome/` y `SampleScene.unity`. La escena de prueba `EscenaPrueba_Fase1.unity` se trasladó a `Assets/Scenes/Pruebas/` fuera de Build Settings.
- **Flujo de ramas Git**: Se trabajará bajo una metodología de ramas por funcionalidad (`feature/...`) con commits atómicos en español, fusionando localmente a `main` y subiendo ambas a GitHub al no contar con la herramienta CLI `gh` en el entorno.
- **Construcción idempotente**: Todas las escenas, prefabs y animaciones se seguirán generando desde código C# de Editor (`ElMineroBuilder.ConstruirTodoElProyecto`) sin manipular YAML manualmente.

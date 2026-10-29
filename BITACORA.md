# Bitácora de Desarrollo — "El Minero"

**Proyecto:** El Minero (Plataformas 2D en Unity 6.6 / URP 2D)  
**Autora:** Juana Jaqueline Camarillo Olaez (UTNG — GIDS6102-E)  
**Materia / Docente:** Creación de Videojuegos / Gabriel Barrón Rodríguez  
**Fecha de inicio:** 28 de octubre de 2026  
**Versión:** 1.0.0  

---

## 1. Inventario Inicial del Proyecto (Paso 1)

### 1.1 Lo que existía y funcionaba
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

### 1.2 Lo que existía incompleto o requería refactorización
- **Control de entrada / Gamepad**:
  - `PlayerInputHandler.cs` no integraba adecuadamente el esquema de Gamepad de Input System (stick, cruceta, botón Sur para saltar, botón Start para pausar).
- **Gestión de Audio**:
  - No existía el archivo `MezcladorPrincipal.mixer` con los grupos `Musica` (parámetro `VolMusica`) y `Efectos` (parámetro `VolEfectos`).
  - Faltaba la aceleración de pitch (1.08) y volumen (+2 dB) en el último 30% del tiempo.
  - No estaban configurados los AudioSources 3D para bucles ambientales (fuego, vagoneta, malacate).
- **Flujo de Escenas y Estados por Nivel**:
  - Las escenas `Victoria.unity` y `Derrota.unity` aún existían y debían ser sustituidas por paneles integrados dentro de cada nivel (`ResultsPanel` y `GameOverPanel`), más la nueva escena `Final.unity`.
  - Las escenas de prueba `SampleScene` y `EscenaPrueba_Fase1` debían ser depuradas/movidas.
  - No existían las escenas `Splash.unity`, `Intro.unity` y `Final.unity`.
- **Interfaz de Usuario (UI)**:
  - El Menú Principal carecía de: selector de niveles con candados y miniaturas, panel de opciones completas con sliders/toggle/calidad/controles/borrado, y panel de créditos con scroll y botón Volver en la esquina superior izquierda (`Ignore Layout`).
  - En cada nivel faltaban: tarjeta informativa inicial con dato histórico y botón "¡A escapar!", cartel emergente de 3 s al recoger el mineral distintivo, panel de resultados con conteo animado, evaluación estricta de 1 a 3 estrellas con rebote de escala y sfx progresivos, y panel de Game Over con causa explicativa.
- **Configuración e Importación de Sprites (v2)**:
  - Los sprites nuevos del paquete v2 no estaban rebanados con sus celdas exactas ni configurados con sus bordes 9-slice en `SpriteImportSetup.cs`.
- **Enemigos y Obstáculos nuevos**:
  - No existían scripts de IA para enemigos: faltaba `EnemyBase.cs`, `BatPatrol.cs`, `BatDiver.cs`, `SpiderDrop.cs`, `ScorpionAI.cs`, `RunawayCart.cs`, `FireHazard.cs`, `MineElevator.cs`.
- **Diseño de Niveles 1, 2 y 3**:
  - Las 3 minas no reflejaban la totalidad de especificaciones históricas y mecánicas de la Tabla 5 (Mina de Rayas con elevador y tutorial; El Nopal con capilla, rieles y vagonetas; La Valenciana con maquinaria quemada, alacranes, murciélagos rojos, picadas y salida al Templo de San Cayetano).

### 1.3 Lo que faltaba por completo
- Clases de IA de enemigos y obstáculos programados (`EnemyBase`, `BatPatrol`, `BatDiver`, `SpiderDrop`, `ScorpionAI`, `RunawayCart`, `FireHazard`, `MineElevator`).
- Paneles UI modulares: `OptionsPanel`, `CreditsPanel`, `LevelSelectPanel`, `ResultsPanel`, `GameOverPanel`, `LevelStartCard`, `MineralPopup`.
- Gestor de guardado y configuración (`SettingsManager.cs`) con PlayerPrefs (`NivelDesbloqueado`, `Estrellas_N1..N3`, `MejorPuntaje_N1..N3`, `MejorTiempo_N1..N3`, `MineralEncontrado_N1..N3`, `NombreJugador`, volúmenes, calidad).
- Escenas `Splash`, `Intro` y `Final`.
- Métodos automatizados de build: `BuildSettingsSetup` para Windows (.exe), WebGL y Android (.apk).
- Documentación completa requerida: `CAMBIOS_GDD.md`, `MANUAL_USUARIO.md`, `CREDITOS.md`, `README.md`.

---

## 2. Registro Cronológico de Fases y Ramas Git

### Rama 1: `feature/limpieza-y-sprites`
- **Objetivo:** Adecuar repositorio, limpiar archivos redundantes, ajustar `.gitignore` y configurar el importador automatizado de sprites.
- **Acciones:**
  - Actualización de `.gitignore` con exclusiones de `*.slnx`, `.vscode/`, `Builds/`, temporales y logs.
  - Eliminación de `Assets/TextMesh Pro/Examples & Extras/`, `Assets/Welcome/` y `Assets/Scenes/SampleScene.unity`.
  - Reubicación de `EscenaPrueba_Fase1.unity` a `Assets/Scenes/Pruebas/`.
  - Sustitución de `Salida_Superficie.png` por el arte renovado del Templo de San Cayetano y eliminación de duplicados con sufijo " 1".
  - Creación de `ElMineroSpritePostprocessor.cs` y refactorización de `SpriteImportSetup.cs`: PPU=16, Point Filter, sin compresión, corte en celdas de hojas de sprite y bordes 9-slice para paneles, botones, sliders y toggles.
  - Ejecución en batchmode verificada con 0 errores.
- **Merge a `main`:** Commit `6a08466` fusionado a `main` y sincronizado en GitHub.

### Rama 2: `feature/audio`
- **Objetivo:** Implementar la arquitectura de audio con AudioMixer, crossfade dual, curva de urgencia y atenuación de pausa.
- **Acciones:**
  - Creación procedural de `Assets/Audios/MezcladorPrincipal.mixer` mediante `AudioMixerBuilder.cs`, exponiendo `VolMusica` y `VolEfectos`.
  - Refactorización de `AudioManager.cs` con dos fuentes dedicadas a música para transición con crossfade suave de 1.0 s.
  - Implementación de aceleración de pitch (1.08) y volumen (+2 dB) durante el último 30% del temporizador en niveles.
  - Aplicación de filtro pasa-bajos / atenuación en pausa (`PausarTodoElAudio(true)`).
  - Implementación de métodos para todos los efectos de sonido (rocas, derrumbe, daño, muerte, gema, mineral, racha, estrellas, clic, error, etc.) y bucles 3D para vagoneta, fuego y malacate.
- **Merge a `main`:** Commit `1766a2a` fusionado a `main` y sincronizado en GitHub.

### Rama 3: `feature/enemigos`
- **Objetivo:** Desarrollar e instanciar todos los enemigos, obstáculos especiales y minerales distintivos.
- **Acciones:**
  - Creación de jerarquía de scripts: `EnemyBase.cs`, `BatPatrol.cs` (patrulla sinusoidal y chillido a < 6u), `BatDiver.cs` (detección en cono y picada a 7 u/s), `SpiderDrop.cs` (descenso por hilo al pasar por debajo), `ScorpionAI.cs` (patrulla terrestre y persecución a 3 u/s), `RunawayCart.cs` (recorrido a 9 u/s con aviso previo 1s y audio 3D), `RunawayCartTrigger.cs`, `FireHazard.cs` (fuego con ciclo 3s encendido/2s apagado) y `MineElevator.cs` (malacate funcional).
  - Implementación de `DistinctiveMineral.cs` para Aguilarita (N1), Pepita de Oro (N2) y Pirargirita (N3), otorgando 50 pts y persistencia en PlayerPrefs.
  - Actualización de `CollectibleManager.cs`, `LevelManager.cs` y `HUDController.cs` con notificación emergente de mineral.
  - Generación procedural de clips de animación y Animator Controllers con `AnimationGenerator.cs`.
  - Generación de todos los prefabs en `Assets/Prefabs/` mediante `PrefabGenerator.cs`.
- **Merge a `main`:** Commit `0c6bb46` fusionado a `main` y sincronizado en GitHub.

### Rama 4: `feature/opciones`
- **Objetivo:** Crear el panel de opciones modal según la Sección 3.4 y la persistencia de configuraciones.
- **Acciones:**
  - Creación de `SettingsManager.cs`: persistencia de volumen de música, efectos, silencio, calidad visual y función `BorrarProgreso()` que preserva la configuración de audio/gráficos.
  - Creación de `OptionsPanel.cs` con layout estricto: botón Volver en esquina superior izquierda con `ignoreLayout = true`, sliders de volumen vinculados a decibelios del AudioMixer, toggle de silencio, selector de calidad gráfica, resumen de controles y botón de confirmación de borrado de progreso.
  - Integración en `PauseMenu.cs` para permitir acceso al panel de opciones en partida.
- **Merge a `main`:** Commit `a0be4f3` fusionado a `main` y sincronizado en GitHub.

### Rama 5: `feature/narrativa`
- **Objetivo:** Flujo narrativo completo (Splash, Intro, Final, UI de niveles y Menú Principal).
- **Acciones:**
  - Creación de `FontAssetBuilder.cs` para instanciar TMP Font Assets reales (`PressStart2P SDF` y `PixelifySans SDF`) con texturas de atlas y materiales enlazados.
  - Creación de controladores: `SplashController.cs` (fade 2s autora + fade 2s logo juego), `IntroController.cs` (texto mecanografiado y avance interactivo), `FinalController.cs` (resumen de logros, 3 minerales históricos, créditos en scroll), `LevelStartCard.cs` (tarjeta de nivel con dato histórico), `LevelSelectPanel.cs` (3 tarjetas con miniaturas, estrellas y candados), `CreditsPanel.cs` (créditos completos con enlace a GitHub), `ResultsPanel.cs` (conteo dinámico de tiempo/puntos, evaluación de 1-3 estrellas con rebote y audio progresivo) y `GameOverPanel.cs` (motivo personalizado de derrota).
  - Creación de `UIPrefabBuilder.cs` y compilación procedural de los 6 prefabs UI en `Assets/Prefabs/UI/`.
- **Merge a `main`:** Commit `d146e3a` fusionado a `main` y sincronizado en GitHub.

### Rama 6: `feature/niveles-2-3`
- **Objetivo:** Construcción de las 7 escenas del juego con terreno, iluminación URP 2D, enemigos, obstáculos, cámaras Cinemachine y HUD.
- **Acciones:**
  - Actualización de `LevelConfig.cs` con campos para mineral distintivo y parámetros históricos.
  - Configuración de los 3 ScriptableObjects en `Assets/Settings/Niveles/` (Nivel 1: 100s, 6 vetas, 2 gemas, Aguilarita; Nivel 2: 120s, 8 vetas, 3 gemas, Pepita de Oro; Nivel 3: 140s, 10 vetas, 4 gemas, Pirargirita).
  - Refactorización integral de `LevelBuilder.cs`:
    - `ConstruirSplash`: Grupos de autora y juego con CanvasGroup y desvanecimiento.
    - `ConstruirMenuPrincipal`: Logo, minero en Idle, subtítulo, versión v1.0.0, InputField de nombre (por defecto "Refugio"), 6 botones madera y los 3 paneles modales instanciados.
    - `ConstruirIntro`: Panel translúcido con texto de contextualización del siglo XIX en Guanajuato.
    - `ConstruirNivel1_MinaRayas`: 130 u, 100 s, Aguilarita, 2 murciélagos, elevador malacate, Capilla, 7 spawners de roca y 2 derrumbes.
    - `ConstruirNivel2_ElNopal`: 170 u, 120 s, Pepita de Oro, 2 vagonetas desbocadas con aviso, 3 arañas, 2 murciélagos, plataformas móviles, 2 Capillas, 10 spawners y 3 derrumbes.
    - `ConstruirNivel3_LaValenciana`: 210 u, 140 s, Pirargirita, 3 alacranes con patrulla/persecución, 3 murciélagos rojos en picada, 2 arañas, 2 vagonetas, 2 fuegos intermitentes, elevador malacate, 2 Capillas, 14 spawners, 4 derrumbes y salida al Templo de San Cayetano con luz cálida.
    - `ConstruirFinal`: Escena de celebración con minero frente al Templo, logros acumulados, 3 minerales recolectados y créditos.
    - Eliminación de escenas obsoletas (`Victoria.unity`, `Derrota.unity`, `SampleScene.unity`).
    - Configuración automática de las 7 escenas en `EditorBuildSettings`.
- **Merge a `main`:** Commit `f732970` fusionado a `main` y sincronizado en GitHub.

### Rama 7: `feature/builds`
- **Objetivo:** Automatización de compilación de ejecutables multiplataforma y verificación de builds.
- **Acciones:**
  - Creación de `BuildSettingsSetup.cs` con métodos:
    - `ConfigurarBuildSettings()`: Registra las 7 escenas y aplica metadatos de autor, versión 1.0.0 e icono `Minero_Retrato.png`.
    - `CompilarWindows()`: Compila Windows Standalone 64-bit en `Builds/Windows/ElMinero.exe`.
    - `CompilarWebGL()`: Configura compresión Gzip y descompresión alternativa en `Builds/WebGL/`.
    - `CompilarAndroid()`: Configura identificador `com.utng.elminero`, SDK mínimo API 26 y compila APK en `Builds/Android/ElMinero.apk`.
    - `CompilarTodasLasPlataformas()`: Ejecuta las compilaciones con manejo de excepciones tolerante a módulos opcionales.
  - Integración en `ElMineroBuilder.ConstruirTodoElProyecto()`.
  - Compilación exitosa de `Builds/Windows/ElMinero.exe` (ejecutable verificado junto con `UnityPlayer.dll` y `ElMinero_Data`).
- **Merge a `main`:** Commit `e71e96b` fusionado a `main` y sincronizado en GitHub.

---

## 3. Estado Final del Proyecto

- **Compilación:** 0 errores de compilación CS en Unity 6.6 (6000.6.3f1).
- **Flujo de Escenas en Build Settings:**
  1. `Assets/Scenes/Splash.unity` (Índice 0)
  2. `Assets/Scenes/MenuPrincipal.unity` (Índice 1)
  3. `Assets/Scenes/Intro.unity` (Índice 2)
  4. `Assets/Scenes/Nivel1_MinaRayas.unity` (Índice 3)
  5. `Assets/Scenes/Nivel2_ElNopal.unity` (Índice 4)
  6. `Assets/Scenes/Nivel3_LaValenciana.unity` (Índice 5)
  7. `Assets/Scenes/Final.unity` (Índice 6)
- **Ejecutable compilado:** `Builds/Windows/ElMinero.exe`.
- **Repositorio:** 100% sincronizado en GitHub (rama `main` con historial completo de feature branches).

---

## 4. ResoluciÃ³n Integral de Incidencias y Paquete de Recursos v3 (Ramas ix/*)

Para responder a la retroalimentaciÃ³n tÃ©cnica y optimizar el rendimiento y la experiencia de usuario, se crearon e integraron 8 ramas de correcciÃ³n bajo control de versiones estricto:

### 4.1 Rama ix/audio
- **Causa raÃ­z:** En Unity 6, la exposiciÃ³n dinÃ¡mica de parÃ¡metros del AudioMixer (VolMusica / VolEfectos) fallaba arrojando errores en consola (Exposed name does not exist). AdemÃ¡s, los clips de audio quedaban nulos ({fileID: 0}) al compilar builds externas al editor. La mÃºsica del nivel 1 era idÃ©ntica a la del menÃº y el nivel 2 quedaba en silencio porque AudioManager.PlayMusicNivel no se invocaba.
- **SoluciÃ³n implementada:**
  - CreaciÃ³n de AudioLibrary.cs (ScriptableObject) en Assets/Resources/AudioLibrary.asset cargado en runtime mediante Resources.Load<AudioLibrary>("AudioLibrary"), asegurando que todos los clips estÃ©n presentes en .exe, WebGL y APK.
  - AudioManager.cs aplica el volumen directamente sobre las fuentes de mÃºsica, el pool de SFX y las fuentes 3D registradas dinÃ¡micamente (RegistrarFuenteEfectos).
  - Enrutamiento musical explÃ­cito en LevelManager.Start(), IntroController, SplashController y FinalController (MenÃº = vals guanajuatense, N1 = misteriosa, N2 = rÃ­tmica, N3 = urgente, Final = victoria).
- **Ramas y Merge:** Rama ix/audio fusionada a main y sincronizada en remoto.

### 4.2 Rama ix/ui-tamanos
- **Causa raÃ­z:** Varios paneles de UI no mantenÃ­an proporciones canÃ³nicas (desbordes de pantalla), tamaÃ±os de fuente inferiores a 26 pt y el selector de calidad grÃ¡fica con TMP_Dropdown arrojaba error de plantilla faltante.
- **SoluciÃ³n implementada:**
  - SpriteImportSetup.cs importÃ³ y rebanÃ³ los nuevos assets v3 (Castillete 48Ã—80 Bottom Center, Bola de Fuego 16Ã—16, Humo 16Ã—16, Llamas de Incendio 16Ã—32 Bottom Center, TelaraÃ±a 16Ã—16 Top Center, Fondos de MenÃº y Logo Grande).
  - En OptionsPanel.cs se sustituyÃ³ el dropdown por un selector cÃ­clico con flechas â—€  Alta  â–¶ sobre los niveles de QualitySettings.names.
  - En UIPrefabBuilder.cs se calibraron las dimensiones canÃ³nicas modales (Opciones 1200Ã—860, CrÃ©ditos 1200Ã—860, Selector de Niveles 1500Ã—900, Resultados 1200Ã—900, Game Over 1200Ã—760, LevelStartCard 1100Ã—650, Controles 1100Ã—700) forzando tamaÃ±o tipogrÃ¡fico mÃ­nimo de 26 pt y multiplicador 9-slice pixelsPerUnitMultiplier = 0.25f.
- **Ramas y Merge:** Rama ix/ui-tamanos fusionada a main y sincronizada en remoto.

### 4.3 Rama ix/menu
- **Causa raÃ­z:** MenÃº principal plano y estÃ¡tico sin profundidad visual, con tÃ­tulos reducidos y alineaciones desproporcionadas.
- **SoluciÃ³n implementada:**
  - CreaciÃ³n de MenuParallax.cs con oscilaciÃ³n sinusoidal suave en reposo y respuesta fÃ­sica al puntero/mouse en 3 capas de fondo (Capa0_Cielo factor 0.2, Capa1_Ciudad factor 0.5, Capa2_Frente factor 1.0).
  - IntegraciÃ³n del nuevo tÃ­tulo Logo_ElMinero_Grande.png (1160Ã—256), sprite de Refugio en reposo (~200 px) sobre los rieles, panel derecho estilizado (620Ã—740) con botones de 540 px e input de nombre, y texto de versiÃ³n a 28 pt.
- **Ramas y Merge:** Rama ix/menu fusionada a main y sincronizada en remoto.

### 4.4 Rama ix/paneles
- **Causa raÃ­z:** El panel de pausa no ocultaba sus botones al abrir opciones provocando solapamiento; las tarjetas de inicio de nivel mostraban un marco en blanco si faltaba miniatura; caracteres especiales rotos en narrativas.
- **SoluciÃ³n implementada:**
  - PauseMenu.cs oculta la botonera de pausa al abrir opciones, ubica el panel de opciones como Ãºltimo hijo visual (SetAsLastSibling) y se suscribe al evento AlCerrar para restaurar la pausa sin conflicto.
  - IntroController.cs codificado en UTF-8 limpio con panel de 1500Ã—520 y texto a 46 pt.
  - LevelStartCard.cs oculta el componente de miniatura limpiamente (color.a = 0) si no existe imagen asignada, y permite confirmaciÃ³n/cierre inmediato con mando y teclado.
- **Ramas y Merge:** Rama ix/paneles fusionada a main y sincronizada en remoto.

### 4.5 Rama ix/elevador
- **Causa raÃ­z:** El elevador modificaba la jerarquÃ­a del jugador con 	ransform.SetParent, generando desajustes de escala y desconexiÃ³n visual con el cable del malacate.
- **SoluciÃ³n implementada:**
  - MÃ¡quina de estados completa en MineElevator.cs: EsperandoParaSubir, Subiendo, EsperandoParaBajar, Bajando.
  - Retraso de 0.5 s para arrancar al subir y 3.0 s de espera antes de descender.
  - Arrastre fÃ­sico mediante desplazamiento delta en Rigidbody2D sin emparentar objetos.
  - Cable continuo renderizado con LineRenderer anclado a la rueda superior del malacate.
  - Castillete_Malacate posicionado firmemente sobre la repisa superior del tiro en el Nivel 3 (sin flotar).
- **Ramas y Merge:** Rama ix/elevador fusionada a main y sincronizada en remoto.

### 4.6 Rama ix/enemigos
- **Causa raÃ­z:** Conflictos de colisiones en enemigos con objetos coleccionables y comportamientos anÃ³malos de detecciÃ³n de suelo.
- **SoluciÃ³n implementada:**
  - ConfiguraciÃ³n formal de la Capa 12 (Enemigo) en Constantes.cs y matriz de colisiones de ElMineroBuilder: los enemigos colisionan con Suelo y Jugador, e ignoran coleccionables.
  - SpiderDrop.cs lanza raycast hacia arriba para anclar Telarana_Techo.png al techo de la mina y dibuja su hilo elÃ¡stico con LineRenderer.
  - ScorpionAI.cs detecta suelo exclusivamente en la capa Suelo desde la base de sus patas ignorando su propio collider.
- **Ramas y Merge:** Rama ix/enemigos fusionada a main y sincronizada en remoto.

### 4.7 Rama ix/objetos-flotantes
- **Causa raÃ­z:** Bloques de derrumbe y checkpoints desfasados del piso del tilemap en ciertas secciones.
- **SoluciÃ³n implementada:**
  - BloqueDerrumbe.cs calcula con raycast descendente el punto exacto del suelo para sellar el tÃºnel de piso a techo de manera hermÃ©tica.
  - Checkpoint.cs ancla su pivote Bottom Center con raycast al suelo, asigna sprites rebanados y luz cÃ¡lida Light2D (intensidad 1.2, radio 4.0).
  - RunawayCart.cs inicia oculta, emite seÃ±al "Â¡!" de 1 s, rueda a 9 u/s y se reinicia mediante IResettable.
  - CreaciÃ³n de LevelValidator.cs (Herramientas > El Minero > Validar niveles) para auditar y plantar automÃ¡ticamente cualquier objeto proceduralmente.
- **Ramas y Merge:** Rama ix/objetos-flotantes fusionada a main y sincronizada en remoto.

### 4.8 Rama ix/fuego
- **Causa raÃ­z:** Carencia de los obstÃ¡culos dinÃ¡micos de fuego del paquete v3 en el clÃ­max del Nivel 3.
- **SoluciÃ³n implementada:**
  - Fireball.cs: Proyectil dinÃ¡mico con gravedad 2.5 que inflige 1 corazÃ³n de daÃ±o y se destruye al impactar suelo o al jugador.
  - FireballSpawner.cs: Generador en el techo que emite advertencia de humo (Humo.png) durante 0.8 s antes de soltar la bola de fuego, periÃ³dico y con soporte para IResettable.
  - FireWallChase.cs y FireChaseTrigger.cs: Muro de fuego perseguidor en los Ãºltimos 35 tiles de La Valenciana; al activarse muestra "Â¡LA MINA SE INCENDIA!" (56 pt, 2 s), avanza a 2.4 u/s con sonido de fuego rugiente en bucle, castiga con 1 punto de vida regresando al checkpoint si alcanza a Refugio, y se detiene automÃ¡ticamente en la salida a la superficie.
- **Ramas y Merge:** Rama ix/fuego fusionada a main y sincronizada en remoto.

---

## 5. Resumen de Calidad y ValidaciÃ³n Final

- **CompilaciÃ³n en Unity 6.6:** 0 errores de compilaciÃ³n CS en modo batch (atch.log).
- **ValidaciÃ³n de Niveles:** LevelValidator ejecutado en modo batch con salida limpia (cÃ³digo de salida 0).
- **Ejecutable Windows:** Builds/Windows/ElMinero.exe compilado exitosamente (109 MB, 7 escenas empaquetadas).
- **Control de Versiones:** 100% de ramas integradas y subidas al repositorio pÃºblico de GitHub sin recurrir a force push.
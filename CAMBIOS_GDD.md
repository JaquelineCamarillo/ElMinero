# Registro de Cambios respecto al GDD Inicial — "El Minero"

**Proyecto:** El Minero  
**Autora:** Juana Jaqueline Camarillo Olaez (UTNG — GIDS6102-E)  
**Materia / Docente:** Creación de Videojuegos / Gabriel Barrón Rodríguez  
**Fecha de Entrega:** 27 de noviembre de 2026  
**Versión:** 1.0.0  

---

Este documento detalla todas las adiciones, ajustes y precisiones técnicas implementadas durante el desarrollo de "El Minero" respecto a la versión inicial del Documento de Diseño de Juego (GDD).

---

## 1. Flujo de Escenas y Estructura Narrativa

| Aspecto | GDD Inicial | Versión Final Implementada | Justificación / Beneficio |
| :--- | :--- | :--- | :--- |
| **Escenas en Build Settings** | 6 escenas (`MenuPrincipal`, `Nivel1_MinaRayas`, `Nivel2_ElNopal`, `Nivel3_LaValenciana`, `Victoria`, `Derrota`). | 7 escenas ordenadas:<br>1. `Splash`<br>2. `MenuPrincipal`<br>3. `Intro`<br>4. `Nivel1_MinaRayas`<br>5. `Nivel2_ElNopal`<br>6. `Nivel3_LaValenciana`<br>7. `Final` | Se eliminaron las escenas aisladas de Victoria y Derrota, integrando paneles modales interactivos (`ResultsPanel` y `GameOverPanel`) dentro de cada nivel. Se crearon `Splash` (identidad de UTNG y autora), `Intro` (contexto histórico) y `Final` (superficie frente al Templo de San Cayetano). |
| **Transiciones** | Carga directa de escena. | Fundido a negro (2.0 s de fade in/out) controlado por `SceneLoader.cs` singleton persistente. | Evita parpadeos y proporciona una experiencia cinematográfica fluida. |

---

## 2. Ajustes en Duración de Niveles y Sistema de Estrellas

Para garantizar una curva de dificultad justa y alcanzable que premie la exploración:

| Nivel | Tiempo Inicial GDD | Tiempo Ajustado | Criterio de 1 Estrella (★) | Criterio de 2 Estrellas (★★) | Criterio de 3 Estrellas (★★★) |
| :--- | :---: | :---: | :--- | :--- | :--- |
| **1 · Mina de Rayas** | 90 s | **100 s** | Llegar a la salida vivo | Completar con ≥ 120 pts | Completar con ≥ 150 pts y más de 30 s restantes |
| **2 · El Nopal** | 75 s | **120 s** | Llegar a la salida vivo | Completar con ≥ 180 pts | Completar con ≥ 220 pts y más de 35 s restantes |
| **3 · La Valenciana** | 70 s | **140 s** | Salir a la superficie | Completar con ≥ 240 pts | Completar con ≥ 300 pts y más de 40 s restantes |

> **Nota:** La animación de estrellas en el panel de resultados muestra un efecto elástico (punch scale) y reproduce una nota musical ascendente por cada estrella conseguida.

---

## 3. Minerales Distintivos con Contexto Histórico

Se añadió un coleccionable especial único por nivel (50 puntos c/u) que refleja el patrimonio minero de Guanajuato:

1. **Nivel 1 (Mina de Rayas): Aguilarita (`Ag₄SeS`)**
   - *Historia:* Descubierta en 1891 por el ingeniero mexicano Ponciano Aguilar en las proximidades de la Mina de Rayas.
   - *Mecánica:* Al recolectarse, despliega una tarjeta flotante informativa durante 3 segundos.
2. **Nivel 2 (El Nopal): Pepita de Oro**
   - *Historia:* Representa las vetas auríferas encontradas en los tiros profundos de El Nopal.
3. **Nivel 3 (La Valenciana): Pirargirita (`Ag₃SbS₃`, Plata Roja)**
   - *Historia:* Mineral de plata oscura y sulfuro de antimonio que convirtió a La Valenciana en una de las minas más ricas del planeta.

---

## 4. Arquitectura de Audio y Mezcla Dinámica

1. **Mezclador Principal (`MezcladorPrincipal.mixer`)**:
   - Grupos enrutados: `Master`, `Musica` y `Efectos`.
   - Parámetros expuestos: `VolMusica` y `VolEfectos`, mapeados logarítmicamente de 0.0001 a 1.0 ($-80\text{ dB}$ a $0\text{ dB}$).
2. **Curva de Urgencia**:
   - Cuando el reloj desciende por debajo del 30% del tiempo total del nivel, la música aumenta progresivamente su volumen (+2 dB) y su tono (pitch 1.08).
   - En los últimos 10 segundos, suena el segundero de alarma (`Tic_Reloj.wav`) y el reloj parpadea en rojo.
3. **Audio Espacial 3D**:
   - Vagonetas desbocadas, fuego de gas y el elevador tipo malacate emiten sonido tridimensional dependiente de la posición de la cámara y del jugador.
4. **Filtro de Pausa**:
   - Al pausar el juego se atenúa suavemente la música para dar protagonismo a la interfaz.

---

## 5. Nuevos Enemigos e Inteligencia Artificial 2D

Se diseñó e implementó un sistema jerárquico de enemigos heredados de `EnemyBase.cs`:

- **Murciélago Estándar (`BatPatrol.cs`)**: Patrulla horizontal entre puntos fijos con ondulación senoidal ($A = 0.5\text{ u}$) a $2\text{ u/s}$. Emite chillido cuando el jugador se acerca a menos de $6\text{ u}$.
- **Murciélago Rojo en Picada (`BatDiver.cs`)**: Permanece colgado del techo esperando. Al detectar al jugador en un cono inferior a menos de $5\text{ u}$, se lanza en picada a $7\text{ u/s}$.
- **Araña de Techo (`SpiderDrop.cs`)**: Cuelga de un hilo elástico invisible. Al pasar el jugador debajo, cae rápidamente y regresa lentamente al techo.
- **Alacrán Terrestre (`ScorpionAI.cs`)**: Patrulla sobre plataformas a $1.5\text{ u/s}$. Si el jugador se aproxima a su rango visual, acelera a $3\text{ u/s}$ en persecución continua.
- **Vagoneta Desbocada (`RunawayCart.cs`)**: Se activa mediante triggers de paso. Despliega un aviso flotante "¡!" 1 segundo antes de avanzar a toda velocidad ($9\text{ u/s}$) por los rieles.
- **Llamas de Gas (`FireHazard.cs`)**: Obstáculo de suelo que alterna entre 3 segundos activo (inflige daño) y 2 segundos inactivo (seguro).
- **Elevador Tipo Malacate (`MineElevator.cs`)**: Plataforma vertical automática que sube y baja conectada mediante un cable visual a la rueda del malacate superior.
- **Capilla Minera (`Checkpoint_Capilla`)**: Checkpoints temáticos que recrean los altares coloniales subterráneos venerados por los mineros, activando una luz cálida al ser alcanzados.

---

## 6. Panel de Opciones y Persistencia Robusta

- **Diseño visual:** Imagen `Panel_Madera` sliced centrada en pantalla.
- **Botón Volver:** Esquina superior izquierda con icono redondo y componente `LayoutElement.ignoreLayout = true` para no desconfigurar el flujo vertical.
- **Controles interactivos:** Sliders con sprite swap en manija y fondo, toggle de silencio ("Mute"), dropdown de calidad gráfica (Baja, Media, Alta) y panel de referencia de teclas.
- **Borrado seguro de progreso:** Modal de confirmación que elimina récords de niveles y estrellas, preservando intactas las preferencias de sonido y gráficos del usuario.

---

## 7. Soporte Multiplataforma y Automatización

- Se creó `BuildSettingsSetup.cs` para compilar de manera desatendida y repetible:
  - **Windows:** Ejecutable de 64 bits autocontenido (`Builds/Windows/ElMinero.exe`).
  - **WebGL:** Configuración optimizada con compresión Gzip y descompresión de respaldo (`Builds/WebGL/`).
  - **Android:** Paquete APK firmado (`Builds/Android/ElMinero.apk`) con identificador `com.utng.elminero` y nivel mínimo de API 26.
- Todo el proyecto puede regenerarse de forma limpia e idempotente con el método `ElMineroBuilder.ConstruirTodoElProyecto()`.

---

## 8. IntegraciÃ³n del Paquete de Recursos v3 y Perfeccionamiento TÃ©cnico

Para cumplir con las especificaciones finales del juego y resolver fallos de ejecuciÃ³n detectados en el motor, se aÃ±adieron los siguientes cambios estructurales:

1. **CatÃ¡logo de Recursos v3 (SpriteImportSetup.cs)**:
   - Castillete_Malacate.png: Celda 48Ã—80, 4 cuadros de animaciÃ³n de rueda giratoria, pivote Bottom Center.
   - Bola_Fuego.png: Celda 16Ã—16, 4 cuadros de animaciÃ³n, pivote Center.
   - Humo.png: Celda 16Ã—16, 4 cuadros de advertencia en techo previa a la caÃ­da de fuego.
   - Llamas_Incendio.png: Celda 16Ã—32, 4 cuadros de fuego vertical, pivote Bottom Center.
   - Telarana_Techo.png: Celda 16Ã—16, pivote Top Center para anclaje al techo.
   - Logo_ElMinero_Grande.png: TÃ­tulo estilizado en alta resoluciÃ³n (~291Ã—64 original / 1160Ã—256 en lienzo UI).
   - Fondos del MenÃº en 3 capas independientes (FondoMenu_Capa0_Cielo, FondoMenu_Capa1_Ciudad, FondoMenu_Capa2_Frente).

2. **Arquitectura de Audio Directo y AudioLibrary**:
   - En lugar de depender de parÃ¡metros expuestos de AudioMixer (susceptibles a fallas de reflexiÃ³n en Unity 6), AudioManager.cs controla directamente AudioSource.volume para mÃºsica, SFX y bucles 3D.
   - CentralizaciÃ³n de todos los clips de sonido en un ScriptableObject (AudioLibrary.asset) alojado en Assets/Resources/, garantizando disponibilidad absoluta en ejecutables autÃ³nomos.

3. **FÃ­sica del Malacate y Elevador (MineElevator.cs)**:
   - EliminaciÃ³n de la jerarquÃ­a forzada (	ransform.SetParent); ahora el jugador es transportado calculando el desplazamiento $\Delta y$ aplicado directamente al Rigidbody2D del minero.
   - ConexiÃ³n continua mediante LineRenderer entre la rueda del castillete y la base del elevador.
   - Estados de espera controlados: 0.5 s para iniciar ascenso y 3.0 s para descenso.

4. **ClÃ­max del Nivel 3: Bolas de Fuego y Muro de Incendio**:
   - FireballSpawner.cs proyecta humo en el techo durante 0.8 s antes de dejar caer una bola de fuego con fÃ­sica gravitatoria activa.
   - FireWallChase.cs activa una persecuciÃ³n cinematogrÃ¡fica en los Ãºltimos 35 tiles de La Valenciana a .4\text{ u/s}$, con alerta "Â¡LA MINA SE INCENDIA!" (56 pt) y sonido envolvente de llamas.

5. **Parallax Interactivo del MenÃº Principal (MenuParallax.cs)**:
   - Movimiento diferencial en 3 planos segÃºn la posiciÃ³n del cursor con oscilaciÃ³n armÃ³nica en reposo.

6. **Selector de Calidad GrÃ¡fica Sin Dropdown**:
   - SustituciÃ³n de TMP_Dropdown por un componente selector cÃ­clico â—€  Alta  â–¶ que opera sobre QualitySettings.names.
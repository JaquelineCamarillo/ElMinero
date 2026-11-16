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

## 8. Integración del Paquete de Recursos v3 y Perfeccionamiento Técnico

Para cumplir con las especificaciones finales del juego y resolver fallos de ejecución detectados en el motor, se añadieron los siguientes cambios estructurales:

1. **Catálogo de Recursos v3 (SpriteImportSetup.cs)**:
   - Castillete_Malacate.png: Celda 48×80, 4 cuadros de animación de rueda giratoria, pivote Bottom Center.
   - Bola_Fuego.png: Celda 16×16, 4 cuadros de animación, pivote Center.
   - Humo.png: Celda 16×16, 4 cuadros de advertencia en techo previa a la caída de fuego.
   - Llamas_Incendio.png: Celda 16×32, 4 cuadros de fuego vertical, pivote Bottom Center.
   - Telarana_Techo.png: Celda 16×16, pivote Top Center para anclaje al techo.
   - Logo_ElMinero_Grande.png: Título estilizado en alta resolución (~291×64 original / 1160×256 en lienzo UI).
   - Fondos del Menú en 3 capas independientes (FondoMenu_Capa0_Cielo, FondoMenu_Capa1_Ciudad, FondoMenu_Capa2_Frente).

2. **Arquitectura de Audio Directo y AudioLibrary**:
   - En lugar de depender de parámetros expuestos de AudioMixer (susceptibles a fallas de reflexión en Unity 6), AudioManager.cs controla directamente AudioSource.volume para música, SFX y bucles 3D.
   - Centralización de todos los clips de sonido en un ScriptableObject (AudioLibrary.asset) alojado en Assets/Resources/, garantizando disponibilidad absoluta en ejecutables autónomos.

3. **Física del Malacate y Elevador (MineElevator.cs)**:
   - Eliminación de la jerarquía forzada (transform.SetParent); ahora el jugador es transportado calculando el desplazamiento Δy aplicado directamente al Rigidbody2D del minero.
   - Conexión continua mediante LineRenderer entre la rueda del castillete y la base del elevador.
   - Estados de espera controlados: 0.5 s para iniciar ascenso y 3.0 s para descenso.

4. **Clímax del Nivel 3: Bolas de Fuego y Muro de Incendio**:
   - FireballSpawner.cs proyecta humo en el techo durante 0.8 s antes de dejar caer una bola de fuego con física gravitatoria activa.
   - FireWallChase.cs activa una persecución cinematográfica en los últimos 35 tiles de La Valenciana a 2.4 u/s, con alerta "¡LA MINA SE INCENDIA!" (56 pt) y sonido envolvente de llamas.

5. **Parallax Interactivo del Menú Principal (MenuParallax.cs)**:
   - Movimiento diferencial en 3 planos según la posición del cursor con oscilación armónica en reposo.

6. **Opción de calidad gráfica retirada**:
   - Por decisión de diseño se quitó la fila "Calidad" del panel de Opciones (selector, código en OptionsPanel y SettingsManager, y la clave de PlayerPrefs). El juego usa un solo nivel de calidad por defecto.

---

## Cambios de la versión 1.1 (noviembre 2026): interfaz, nivel 3, jefe final y escena del Templo

### Interfaz
- Textos en UTF-8 sin símbolos raros; ningún texto del juego usa ▶ ◀ ♥ ✓ ★ (las fuentes no los tienen).
- Menú principal con un solo panel de madera (600 × 820) bajo el título, botones Pixelify Sans 36 y el minero parado sobre los rieles.
- Panel "Controles del juego" de 1300 × 860 con todo dentro del marco; tarjeta de inicio de nivel de 1400 × 880 con miniatura de la mina y dato histórico.
- Validador de interfaz (Herramientas > El Minero > Validar interfaz) y capturas automáticas en `Docs/Capturas/`.

### Nivel 3 — La Valenciana (rediseño completo)
- El nivel se construye **desde el mapa** `Assets/Settings/Niveles/Mapa_Nivel3.txt` (216 × 50 tiles) y ahora **sube**: galería inicial (y = 4), zona de maquinaria quemada de 1817, galería de rieles con vagoneta (y = 8), **tiro con elevador** hasta el nivel alto (y = 30), arena del jefe (y = 34) y huida del incendio hasta la salida (y = 38).
- Tiempo límite: **220 s**. 16 vetas, 4 gemas y la pirargirita (en un nicho lateral del tiro, al que se salta desde la jaula en movimiento).
- **Elevador del tiro** funcional: se activa al pararse en la jaula, sube a 3 u/s de y = 8 a y = 30, llega al ras del piso, baja vacío a los 3 s y vuelve abajo si el jugador reaparece. Castillete apoyado en los dos bordes del tiro, rueda animada y cable que se alarga; caen rocas por el tiro mientras sube.
- **Incendio final** corregido: el muro de fuego no existe hasta cruzar el trigger X (después del jefe), aparece 9 u detrás del jugador, avanza a 2.6 u/s siguiendo su altura, y si lo alcanza el jugador reaparece en el checkpoint C4 y el muro se reinicia. Las bolas de fuego explotan en el piso y vuelven a un pool.

### Jefe final: "El Coloso de Plata Roja"
> **Leyenda ficticia creada para el juego**: "Cuentan los mineros de La Valenciana que, en lo más hondo de la Veta Madre, duerme un coloso de roca y plata roja. El derrumbe lo ha despertado… y bloquea la última salida."

- Arena cerrada por compuertas de bloques; intro con cartel "EL COLOSO DE PLATA ROJA — Guardián de la Veta Madre" y barra de vida de 3 segmentos.
- 3 fases: embestida (9/11/12 u/s), pisotón con ondas de choque que se saltan, lanzamiento de rocas en arco y rocas que caen del techo.
- Solo se le hace daño **cayéndole encima cuando está aturdido** (después de chocar con la pared).
- Al vencerlo: jingle, +300 puntos, +1 vida y se abre la salida. Si el jugador pierde una vida durante la pelea, reaparece antes de la arena y el jefe se reinicia.

### Escena final jugable: `Final_Templo`
- Reemplaza a la escena `Final`. El minero sale por el **Tiro de San Cayetano** y camina por el atrio hasta el **Templo de San Cayetano** mientras amanece: repican las campanas, el pueblo celebra, las palomas vuelan y los faroles se apagan.
- En la puerta del templo aparece el panel final con el nombre del jugador, puntaje total, estrellas / 9, minerales distintivos / 3 y si venció al Coloso; botones Créditos (con desplazamiento sobre el amanecer) y Menú.
- Créditos: "Jefe final y escena del Templo de San Cayetano: arte y audio generados con asistencia de IA (Claude, Anthropic)".


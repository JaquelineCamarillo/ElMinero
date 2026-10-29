# El Minero — Videojuego de Plataformas 2D

![Unity 6](https://img.shields.io/badge/Unity-6000.6.3f1%20(6.6)-blue.svg)
![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP%202D-purple.svg)
![Input System](https://img.shields.io/badge/Input%20System-1.20%20(New%20Only)-orange.svg)
![Plataformas](https://img.shields.io/badge/Plataformas-Windows%20%7C%20WebGL%20%7C%20Android-green.svg)
![Licencia](https://img.shields.io/badge/Licencia-Educativa%20UTNG-lightgrey.svg)

> **"El Minero"** es un videojuego de plataformas en dos dimensiones ambientado en las legendarias minas subterráneas de Guanajuato, México, a finales del siglo XIX. Desarrollado con **Unity 6.6**, **Universal Render Pipeline (URP 2D)** y lenguaje **C#**.

---

## 📖 Sinopsis

Tras un catastrófico derrumbe subterráneo en la **Mina de Rayas**, el minero barretero (**Refugio**) queda incomunicado a cientos de metros de profundidad. Con su pico, su casco iluminado y su valor, deberá sortear fosos letales, rocas desprendidas, vagonetas desbocadas, alacranes y murciélagos, subiendo nivel a nivel por **El Nopal** hasta salir victorioso a la superficie en **La Valenciana**, junto al histórico **Templo de San Cayetano**.

---

## 🎮 Características Principales

- **Física de Plataformas Pulida:**
  - Velocidad horizontal de $5\text{ u/s}$.
  - Salto de altura variable hasta $2.5\text{ u}$ ($v = \sqrt{2gh}$), caída acelerada ($\times 1.5$) y corte de salto.
  - Asistencias de control: *Coyote Time* ($0.1\text{ s}$) y *Jump Buffering* ($0.1\text{ s}$).
  - Material físico sin fricción que evita adherencias no deseadas a muros.
- **Sistema de Salud y Checkpoints:**
  - 3 Corazones de vida con parpadeo de invulnerabilidad temporal ($1.5\text{ s}$).
  - Reaparición inmediata en el último Checkpoint o Capilla Minera alcanzado.
- **Temporizador de Urgencia:**
  - Reloj de cuenta regresiva por nivel.
  - Curva de audio de urgencia: en el último 30% del tiempo la música sube $+2\text{ dB}$ y acelera su tono (pitch 1.08); en los últimos 10 s el reloj parpadea en rojo con alarma sonora.
- **Puntaje, Racha x2 y Evaluación de Estrellas:**
  - Vetas de plata ($10\text{ pts}$), Gemas de amatista ($25\text{ pts}$) y Minerales Históricos ($50\text{ pts}$).
  - Bono de $+50\text{ pts}$ por limpiar todas las vetas de un nivel.
  - **Racha x2:** 3 coleccionables consecutivos sin recibir daño activan el doble puntaje durante $10\text{ s}$.
  - Evaluación de **1 a 3 estrellas** con animación de rebote y sonidos ascendentes.
- **Enemigos y Obstáculos Históricos:**
  - Murciélagos estándar (vuelo sinusoidal y chillido), murciélagos rojos en picada, arañas colgantes, alacranes terrestres de persecución agresiva, vagonetas desbocadas con aviso sonoro y visual, llamas de gas intermitentes y elevadores tipo malacate.
- **Minerales Distintivos con Contexto Cultural:**
  - **Aguilarita** (Mina de Rayas, descubierta en 1891 por Ponciano Aguilar).
  - **Pepita de Oro** (El Nopal).
  - **Pirargirita / Plata Roja** (La Valenciana).
- **Audio Pro:**
  - Mezclador `MezcladorPrincipal.mixer` con buses de `Música` y `Efectos`.
  - Transición con crossfade dual entre pistas (1.0 s) y atenuación en menú de pausa.
- **Totalmente Automatizado e Idempotente:**
  - Generación de escenas, animaciones, prefabs y Build Settings 100% mediante scripts de Editor (`ElMineroBuilder.ConstruirTodoElProyecto`).

---

## 🕹️ Controles

| Acción | Teclado | Mando (Gamepad) | Táctil (Móvil) |
| :--- | :--- | :--- | :--- |
| **Moverse** | `A` / `D` o `Flechas` | Stick Izquierdo / Cruceta | Botones `◄` / `►` |
| **Saltar** | `Espacio` o `W` | Botón Sur (`A` / `Cruz`) | Botón `▲` |
| **Pausar** | `Escape` / `P` | Botón Start / Menú | Icono Pausa |

---

## 📂 Flujo de Escenas en Build Settings

El proyecto cuenta con 7 escenas ordenadas según las especificaciones del GDD:
1. `Assets/Scenes/Splash.unity`: Presentación de la UTNG y logo de la autora.
2. `Assets/Scenes/MenuPrincipal.unity`: Menú principal con entrada de nombre, selector de niveles, opciones y créditos.
3. `Assets/Scenes/Intro.unity`: Contextualización narrativa histórica estilo máquina de escribir.
4. `Assets/Scenes/Nivel1_MinaRayas.unity`: Nivel 1 (100 s, Aguilarita, elevador y murciélagos).
5. `Assets/Scenes/Nivel2_ElNopal.unity`: Nivel 2 (120 s, Pepita de Oro, vagonetas desbocadas y arañas).
6. `Assets/Scenes/Nivel3_LaValenciana.unity`: Nivel 3 (140 s, Pirargirita, alacranes, fuego y salida al Templo).
7. `Assets/Scenes/Final.unity`: Celebración frente al Templo de San Cayetano, resumen de logros y créditos.

---

## 🛠️ Cómo Compilar y Ejecutar

### 1. Ejecución Directa en Unity Editor
1. Abre el proyecto en **Unity 6.6 (6000.6.3f1)**.
2. En el menú superior de Unity, selecciona:
   `Herramientas > El Minero > Construir todo el proyecto`.
3. Abre `Assets/Scenes/Splash.unity` o `Assets/Scenes/MenuPrincipal.unity` y pulsa **Play**.

### 2. Compilación en Modo Batch (Sin Editor Abierto)
Para compilar y construir todo el proyecto de manera desatendida:
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\juana\ElMinero" -executeMethod ElMinero.Editor.ElMineroBuilder.ConstruirTodoElProyecto -logFile "C:\Users\juana\ElMinero\Logs\batch.log"
```

### 3. Compilación de Ejecutables (Builds)
Para generar el ejecutable de Windows Standalone (.exe):
```powershell
"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\juana\ElMinero" -executeMethod ElMinero.Editor.BuildSettingsSetup.CompilarWindows -logFile "C:\Users\juana\ElMinero\Logs\batch_build_windows.log"
```
El ejecutable compilado se guardará en:  
`Builds/Windows/ElMinero.exe`

---

## 👥 Datos Académicos y Créditos

- **Autora:** Juana Jaqueline Camarillo Olaez
- **Carrera:** Ingeniería en Desarrollo y Gestión de Software, grupo GIDS6102-E
- **Institución:** Universidad Tecnológica del Norte de Guanajuato (**UTNG**)
- **Materia:** Creación de Videojuegos
- **Docente:** Gabriel Barrón Rodríguez
- **Repositorio Oficial:** [GitHub — ElMinero](https://github.com/JaquelineCamarillo/ElMinero)

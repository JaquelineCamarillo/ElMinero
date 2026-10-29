# Manual de Usuario — "El Minero"

**Videojuego de Plataformas 2D en Unity 6 (URP 2D)**  
**Autora:** Juana Jaqueline Camarillo Olaez  
**Institución:** Universidad Tecnológica del Norte de Guanajuato (UTNG)  
**Materia:** Creación de Videojuegos · Docente: Gabriel Barrón Rodríguez  
**Versión:** 1.0.0  

---

## 1. Sinopsis e Historia

Guanajuato, finales del siglo XIX. La minería subterránea vive su época más intensa y peligrosa.
El jugador encarna a un valiente barretero (nombre por defecto **"Refugio"**, personalizable desde el menú principal) que queda atrapado en lo más profundo de la **Mina de Rayas** tras un repentino derrumbe geológico.

Tu misión es ascender a contrarreloj a través de tres niveles históricos:
1. **Mina de Rayas:** Los tiros profundos y túneles oscuros del primer colapso.
2. **Mina El Nopal:** Rieles empinados, vagonetas desbocadas y capillas subterráneas donde los mineros oraban antes de bajar.
3. **Mina La Valenciana:** Las chimeneas más calientes y peligrosas, defendidas por alacranes y murciélagos rojos, hasta alcanzar la superficie y la luz del sol junto al majestuoso **Templo de San Cayetano**.

---

## 2. Requisitos del Sistema

### PC (Windows)
- **Sistema Operativo:** Windows 10 o Windows 11 (64 bits).
- **Procesador:** Intel Core i3 / AMD Ryzen 3 o superior.
- **Memoria RAM:** 4 GB mínimo.
- **Gráficos:** Tarjeta compatible con DirectX 11 / 12 o Vulkan.
- **Almacenamiento:** 300 MB de espacio disponible.

### WebGL (Navegador)
- Navegador moderno con soporte para WebGL 2.0 y WebAssembly (Google Chrome, Mozilla Firefox, Microsoft Edge u Opera).

### Android (Dispositivos Móviles)
- **Sistema Operativo:** Android 8.0 (Oreo / API 26) o superior.
- **Pantalla táctil** capacitiva.

---

## 3. Instalación y Ejecución

1. **En Windows:**
   - Descarga o descomprime la carpeta `Builds/Windows/`.
   - Haz doble clic sobre `ElMinero.exe`.
2. **En Unity Editor (Modo Desarrollo):**
   - Abre el proyecto en Unity 6.6 (`6000.6.3f1`).
   - Carga la escena `Assets/Scenes/Splash.unity` o `Assets/Scenes/MenuPrincipal.unity`.
   - Presiona el botón **Play**.

---

## 4. Controles del Juego

El juego cuenta con soporte para Teclado, Mando / Gamepad y Pantalla Táctil.

### 4.1 Teclado y Ratón
| Acción | Teclas Principales | Teclas Alternativas |
| :--- | :--- | :--- |
| **Mover a la Izquierda** | `A` | `Flecha Izquierda` |
| **Mover a la Derecha** | `D` | `Flecha Derecha` |
| **Saltar** | `Espacio` | `W` o `Flecha Arriba` |
| **Pausa / Opciones** | `Escape` | `P` |
| **Avanzar diálogos / Intro** | `Espacio` / `Enter` | Clic izquierdo |

### 4.2 Gamepad (Mando Xbox / PlayStation / Genérico)
| Acción | Botón |
| :--- | :--- |
| **Movimiento** | Stick Analógico Izquierdo / Cruceta (D-Pad) |
| **Saltar** | Botón Sur (`A` en Xbox / `Cruz` en PlayStation) |
| **Pausa** | Botón Menú / Start |

### 4.3 Pantalla Táctil (Móviles / Android)
- **Botón `◄` (Izquierda):** Desplaza al minero a la izquierda.
- **Botón `►` (Derecha):** Desplaza al minero a la derecha.
- **Botón `▲` (Salto):** Salto de altura variable (mantener presionado para mayor impulso).
- **Icono de Pausa (Esquina superior derecha):** Abre el menú de pausa.

---

## 5. Reglas del Juego y Mecánicas

### 5.1 Físicas y Movimiento
- **Velocidad de avance:** 5 unidades/segundo.
- **Salto variable:** Salta hasta 2.5 unidades de altura manteniendo pulsado el botón. Soltar antes interrumpe el salto para maniobras precisas.
- **Coyote Time (0.1 s):** Te permite saltar fracciones de segundo después de haber dejado el borde de una plataforma.
- **Jump Buffer (0.1 s):** Registra tu pulsación de salto instantes antes de tocar suelo para que saltes de inmediato al aterrizar.

### 5.2 Vidas e Invulnerabilidad
- Cuentas con **3 Corazones**.
- Al recibir daño de un enemigo, roca o trampa, pierdes 1 corazón y reapareces en el último **Checkpoint** o **Capilla** visitado.
- Cuentas con **1.5 segundos de invulnerabilidad** tras reaparecer (el minero parpadeará visualmente).
- Con **0 corazones**, ocurre el **Game Over**.

### 5.3 Tiempo y Cuenta Regresiva
- Cada nivel tiene un reloj descendente:
  - Mina de Rayas: 100 segundos.
  - El Nopal: 120 segundos.
  - La Valenciana: 140 segundos.
- Si el reloj llega a **00:00**, se produce el colapso de la mina (Game Over por tiempo).
- En el último 30% del tiempo la música acelera su ritmo y tono (+2 dB, pitch 1.08), y en los últimos 10 segundos el reloj parpadea en rojo mientras suena un aviso sonoro.

---

## 6. Puntuación, Racha x2 y Estrellas

### 6.1 Coleccionables y Puntos
- **Veta de Plata:** 10 puntos.
- **Gema de Amatista:** 25 puntos.
- **Mineral Distintivo Histórico:** 50 puntos + tarjeta informativa de 3 segundos.
- **Bono por recolectar todas las vetas:** +50 puntos adicionales al completar el nivel.

### 6.2 Sistema de Racha x2
- Si recoges **3 coleccionables consecutivos sin recibir daño**, se activará el **Multiplicador Racha x2**.
- Todos los puntos obtenidos durante los siguientes **10 segundos** se duplicarán.
- En la interfaz aparecerá una insignia dorada parpadeante con el temporizador de la racha.
- Recibir daño cancela la racha de inmediato.

### 6.3 Evaluación de Estrellas por Nivel
Al llegar a la salida de cada nivel, se evalúa tu desempeño:
- **★ (1 Estrella):** Completar el nivel con vida.
- **★★ (2 Estrellas):**
  - Nivel 1: Obtener al menos 120 puntos.
  - Nivel 2: Obtener al menos 180 puntos.
  - Nivel 3: Obtener al menos 240 puntos.
- **★★★ (3 Estrellas):**
  - Nivel 1: Obtener al menos 150 puntos y terminar con más de 30 segundos restantes.
  - Nivel 2: Obtener al menos 220 puntos y terminar con más de 35 segundos restantes.
  - Nivel 3: Obtener al menos 300 puntos y terminar con más de 40 segundos restantes.

---

## 7. Guía de Enemigos y Obstáculos

| Elemento | Comportamiento | Cómo Evitarlo |
| :--- | :--- | :--- |
| **Rocas que caen** | Aparecen proyectadas desde el techo. Muestran una sombra de advertencia 0.6 s antes del impacto. | No te detengas sobre las sombras de aviso en el suelo. |
| **Derrumbe Sorpresa** | Al cruzar ciertas zonas, el techo colapsa a tus espaldas bloqueando el retroceso. | Avanza siempre hacia adelante con paso firme. |
| **Murciélago Estándar** | Vuela en trayectoria oscilatoria horizontal emitiendo chillidos al acercarte. | Salta por encima o pasa por debajo cuando su vuelo esté en el punto más alto. |
| **Murciélago Rojo** | Descansa en el techo y se lanza en picada letal al detectar tu presencia abajo. | Camina con cautela y acelera cuando comience su descenso para esquivarlo. |
| **Araña de Techo** | Cuelga de un hilo y cae repentinamente al pasar debajo. | Provoca su bajada acercándote un instante y retrocediendo, o corre rápido por debajo. |
| **Alacrán Terrestre** | Camina por las plataformas; si te ve, acelerará en persecución agresiva. | Salta sobre él o utiliza plataformas elevadas para alejarte de su rango. |
| **Vagoneta Desbocada** | Suena en la distancia y muestra un aviso "¡!" 1 s antes de cruzar los rieles a toda velocidad. | Brinca rápidamente sobre plataformas elevadas antes de que pase. |
| **Llamas de Gas** | Fuego emanado del subsuelo que quema durante 3 s y se apaga durante 2 s. | Cronometra el salto justo cuando la llama comience a extinguirse. |
| **Plataformas Quebradizas** | Se rompen 2 segundos después de pisarlas y reaparecen 4 segundos después. | Salta rápidamente sin titubear sobre ellas. |
| **Elevador Malacate** | Jaula de madera impulsada por un malacate que asciende y desciende verticalmente. | Súbete a la plataforma para superar acantilados insalvables a pie. |

---

## 8. Menú de Opciones y Gestión de Partida

Desde el menú principal o durante la pausa puedes acceder a **Opciones**:
- **Volumen de Música:** Control continuo de 0 a 100%.
- **Volumen de Efectos (SFX):** Control independiente de efectos y ambiente.
- **Silenciar (Mute):** Apagado general de audio.
- **Calidad Gráfica:** Selección entre Baja, Media y Alta.
- **Borrar Progreso:** Te permite reiniciar las estrellas y niveles desbloqueados si deseas empezar de cero, confirmando en un diálogo de seguridad. Tu configuración de audio y vídeo se mantendrá guardada.

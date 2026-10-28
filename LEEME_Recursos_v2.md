# El Minero: Recursos v2 (enemigos, minerales, interfaz, música y sonidos)

Este paquete **se suma** al primero (`LEEME_Sprites.md`).
Copia la carpeta `Assets` de este ZIP dentro de `C:\Users\juana\ElMinero\` y acepta combinar carpetas.
El único archivo que se **reemplaza** es `Sprites/Entorno/Salida_Superficie.png`; la versión nueva muestra el Templo de San Cayetano al salir de La Valenciana.

La importación es la misma que en el paquete 1: **PPU 16, Filter Point, Compression None**. Las hojas de animación van en **Sprite Mode Multiple** y se rebanan con **Grid By Cell Size**.

---

## 1. Enemigos (`Sprites/Enemigos/`)

| Archivo | Celda | Cuadros | Nivel | Notas |
|---|---|---|---|---|
| Murcielago.png | 16×16 | 4 (alas arriba, medio, abajo, medio) | 1 y 2 | FPS 10. Mira al frente. |
| Murcielago_Rojo.png | 16×16 | 4 | 3 | Variante agresiva que hace picada. |
| Arana.png | 16×16 | 3 | 2 y 3 | FPS 6. El hilo sale por arriba. |
| Hilo_Arana.png | 2×16 | 1 | 2 y 3 | Draw Mode **Tiled** (Mesh Type Full Rect), estirar en Y. |
| Alacran_Caminar.png | 32×16 | 4 | 3 | FPS 8. Mira a la **izquierda** (flipX para la derecha). |
| Alacran_Ataque.png | 32×16 | 2 (preparar, golpe) | 3 | FPS 6, sin loop. |
| Vagoneta_Desbocada.png | 32×24 | 2 | 2 y 3 | FPS 12. Ruedas girando y chispas. |

## 2. Minerales distintivos de cada mina (`Sprites/Coleccionables/`)

| Archivo | Celda | Cuadros | Nivel | Puntos sugeridos |
|---|---|---|---|---|
| Aguilarita.png | 16×16 | 3 | 1: Mina de Rayas | 50 |
| Pepita_Oro.png | 16×16 | 3 | 2: El Nopal | 50 |
| Pirargirita.png | 16×16 | 3 | 3: La Valenciana | 50 |
| Checkpoint_Capilla.png | 24×32 | 2 (velas apagadas, encendidas) | 2: El Nopal | Checkpoint con forma de nicho de la capilla minera |

## 3. Entorno especial

| Archivo | Celda | Cuadros | Uso |
|---|---|---|---|
| Entorno/Malacate.png | 32×32 | 4 (rueda girando) | Va arriba del tiro, en los niveles 1 y 3. FPS 8. |
| Plataformas/Elevador_Jaula.png | 48×24 | 1 | Elevador vertical del tiro. El collider va en el piso (abajo, alto ≈ 0.4 u). |
| Entorno/Cable.png | 2×16 | 1 | Cable del elevador. Draw Mode **Tiled**. |
| Entorno/Riel.png | 16×8 | 1 | Rieles de la vagoneta. Repetir en fila. |
| Obstaculos/Fuego.png | 16×16 | 4 | Brasas de la maquinaria quemada, nivel 3. FPS 10. |
| Entorno/Letrero.png | 32×24 | 1 | Letreros del tutorial; el texto va con TextMeshPro encima. |
| Entorno/Deco_VetaCobre.png | 16×16 | 1 | Decoración de pared, nivel 2. |
| Entorno/Deco_VetaOro.png | 16×16 | 1 | Decoración de pared, nivel 2. |
| Entorno/Deco_VetaPlataRoja.png | 16×16 | 1 | Decoración de pared, nivel 3. |
| Entorno/Salida_Superficie.png | 32×48 | 1 | **Reemplaza la anterior.** Bocamina con el templo al amanecer. |

## 4. Interfaz (`Sprites/UI/`)

Todos los botones tienen 3 estados para usar **Transition = Sprite Swap**: normal, `_Resaltado` y `_Presionado`.

| Archivo(s) | Tamaño | Uso / 9-slice |
|---|---|---|
| Estrella_Llena, Estrella_Vacia | 32×32 | Panel de resultados y selección de niveles. |
| Boton_Nivel (+ _Resaltado, _Presionado) | 48×48 | Botón de nivel desbloqueado. Border 6. |
| Boton_Nivel_Bloqueado | 48×48 | Nivel bloqueado, con candado. |
| Icono_Candado | 16×16 | Candado suelto. |
| Miniatura_MinaRayas / _ElNopal / _LaValenciana | 162×92 | Vista previa de cada mina en la selección de niveles. |
| Boton_Redondo (+ estados) | 32×32 | Botón redondo base (ponle un icono encima). |
| Boton_Volver (+ estados) | 32×32 | Botón redondo **con la flecha ya incluida**. |
| Boton_Madera_Resaltado | 48×16 | Estado resaltado del Boton_Madera del paquete 1. Border 4. |
| Slider_Fondo | 64×12 | Background del Slider. **Border 4**, Image Type Sliced. |
| Slider_Relleno | 64×12 | Fill del Slider. **Border 4**, Sliced. |
| Slider_Manija | 16×16 | Handle del Slider. |
| Toggle_Fondo | 24×24 | Background del Toggle. |
| Toggle_Check | 24×24 | Checkmark del Toggle. |
| Iconos/Icono_*.png | 16×16 | Volver, Musica, Efectos, Opciones, Jugar, Salir, Creditos, Niveles, Reiniciar, Siguiente, Menu, Pausa, FlechaAbajo, Calidad, Controles |
| Logo_Autora | 128×64 | Logo para la pantalla de inicio (splash). |

## 5. Fuentes (`Fuentes/`)

- **PressStart2P-Regular.ttf**: títulos y botones. Tiene acentos, ñ, ¡ y ¿.
- **PixelifySans.ttf**: textos largos (historia, créditos, datos históricos). Es más legible.
- Las dos tienen licencia **SIL Open Font License**; los `OFL_*.txt` deben quedarse junto a las fuentes.
- Crea los Font Assets en `Window > TextMeshPro > Font Asset Creator`. Usa Atlas Render Mode **RASTER** (o SDF con tamaño 16 o múltiplos) y agrega los caracteres `áéíóúÁÉÍÓÚñÑ¡¿ü`.

## 6. Audio (`Audios/`)

### Música (`Audios/Musica/`, OGG, todas hacen loop sin corte salvo los jingles)

| Archivo | Dónde | Carácter |
|---|---|---|
| Musica_Menu.ogg | Splash, menú, selección de niveles, créditos | Vals en 3/4, aire de estudiantina guanajuatense |
| Musica_Nivel1_MinaRayas.ogg | Nivel 1 | Misteriosa y tranquila, con golpes de pico |
| Musica_Nivel2_ElNopal.ogg | Nivel 2 | Rítmica, con aire español |
| Musica_Nivel3_LaValenciana.ogg | Nivel 3 | Urgente y rápida |
| Musica_Victoria.ogg | Panel de resultados y final | Jingle de unos 3 s, **sin loop** |
| Musica_GameOver.ogg | Game Over | Jingle de unos 4 s, **sin loop** |

### Efectos (`Audios/SFX/`, WAV)

| Archivo | Evento |
|---|---|
| Salto | El jugador salta |
| Aterrizar | Toca el suelo tras una caída |
| Paso_1, Paso_2 | Pasos al caminar, alternados cada ~0.3 s |
| Recolectar_Plata | Veta de plata |
| Recolectar_Gema | Gema de amatista |
| Recolectar_Mineral | Mineral distintivo (aguilarita, oro, pirargirita) |
| Danio | El jugador recibe daño |
| Perder_Vida | Pierde una vida y reaparece |
| Roca_Cae / Roca_Impacto | Roca cayendo / roca que choca con el suelo |
| Plataforma_Cruje / Plataforma_Colapso | Plataforma quebradiza al pisarla / al caer |
| Temblor | Temblor de mina (1.5 s) |
| Derrumbe | Derrumbe sorpresa (2.5 s) |
| Checkpoint | Se activa un checkpoint |
| Click_UI / Hover_UI | Clic / pasar el mouse sobre un botón |
| Pausa | Abrir el menú de pausa |
| Estrella_1, Estrella_2, Estrella_3 | Cada estrella que aparece en el panel de resultados (tono ascendente) |
| Desbloqueo_Nivel | Se desbloquea el siguiente nivel |
| Racha_x2 | Se activa el multiplicador x2 |
| Tic_Reloj | Cada segundo en los últimos 10 s |
| Murcielago_Chillido | Un murciélago cerca o en picada |
| Arana_Baja | Una araña baja por su hilo |
| Alacran_Ataque | Ataque del alacrán |
| Vagoneta_Rodando_Loop | Vagoneta desbocada (loop) |
| Fuego_Loop | Zona de fuego (loop, AudioSource 3D cercano) |
| Malacate_Loop | Elevador del malacate en movimiento (loop) |
| Nivel_Completado | Tocar la salida del nivel |

## 7. Créditos de estos recursos

- **Arte pixel art, música y efectos:** generados por código con asistencia de IA (Claude, de Anthropic) para el proyecto de Juana Jaqueline Camarillo Olaez, siguiendo la dirección de arte del GDD.
- **Fuentes:** Press Start 2P (© 2012 The Press Start 2P Project Authors) y Pixelify Sans (© 2021 The Pixelify Sans Project Authors), bajo SIL Open Font License 1.1, vía Google Fonts.

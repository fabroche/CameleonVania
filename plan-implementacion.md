# Plan de Implementación - Game Jam Educativa 🎓

> **Filosofía**: Aprender implementando. Terminar el juego es la meta, pero entender cada línea de código es la prioridad.

---

## 🎯 Tu Objetivo

**Primario**: Aprender y entender cada sistema que implementes
**Secundario**: Terminar un juego funcional

**Metodología**:
- YO te guío con conceptos, arquitectura y dirección
- TÚ implementas, investigas y resuelves
- Pides ayuda cuando te atores, pero intentas primero
- Cada feature tiene un "¿Por qué?" explicado

---

## 📚 Principios de Aprendizaje

### Regla 80/20
- 80% del tiempo: Tú implementas por tu cuenta
- 20% del tiempo: Pides guía cuando te atoras

### Ciclo de Aprendizaje por Feature
```
1. ENTENDER: ¿Qué hace esta feature?
2. DISEÑAR: ¿Cómo la implementarías tú?
3. INVESTIGAR: Buscar en Unity docs, tutorials
4. IMPLEMENTAR: Escribir código
5. DEBUGGEAR: Resolver errores (aprendes más aquí)
6. REFACTORIZAR: Mejorar el código
7. VALIDAR: ¿Funciona como esperabas?
```

### Cuando Pedir Ayuda
```
✅ PIDE AYUDA si:
- Llevas >30 min atascado en el mismo error
- No entiendes un concepto fundamental
- Necesitas validar tu approach antes de implementar
- Quieres code review

❌ NO PIDAS AYUDA si:
- Aún no has leído la documentación de Unity
- No has intentado debuggear con Debug.Log
- Es tu primer intento de resolver el problema
```

---

## 📅 PLAN DE 6 DÍAS

---

## DÍA 1 (26 ENE - HOY) - FUNDAMENTOS 2D

### **Objetivo del Día**: Entender Unity 2D, física 2D, input system

---

### 🔧 FEATURE 1: Setup del Proyecto (2-3h)

#### **Conceptos a Aprender:**
- Diferencia entre Unity 2D y 3D template
- Sistema de layers y collision matrix
- Project structure best practices
- Git workflow para Unity

#### **Tu Tarea:**
```
1. Crear proyecto Unity 6 con 2D template
   - ¿Por qué 2D template? → Investiga qué trae configurado por default

2. Configurar Git
   - Busca: "Unity .gitignore best practices"
   - Entiende QUÉ carpetas no deben subirse a Git y POR QUÉ

3. Crear estructura de carpetas
   - Diseña TU propia estructura primero
   - Luego compárala con el estándar de la industria

4. Configurar Layers
   - Investiga: ¿Qué es un Layer en Unity?
   - Investiga: ¿Qué es un LayerMask?
   - Crea layers: Player, Enemy, Ground, Water, Collectible, SmallGap

5. Configurar Collision Matrix
   - Investiga: Physics 2D Collision Matrix
   - Decide: ¿Qué debe colisionar con qué?
   - Configura la matriz según tu decisión
```

#### **Recursos de Aprendizaje:**
```
Unity Docs:
- Layers and Collision Matrix: docs.unity3d.com/Manual/Layers.html
- 2D Physics: docs.unity3d.com/Manual/Physics2DReference.html

YouTube:
- "Unity 2D Setup Tutorial" - Brackeys
- "Unity Layers Explained" - cualquier tutorial
```

#### **Checkpoint de Validación:**
```
Antes de continuar, debes poder responder:
✓ ¿Por qué el player NO debe colisionar con collectibles?
✓ ¿Cuál es la diferencia entre un Collider y un Trigger?
✓ ¿Por qué Library/ y Temp/ no van en Git?
```

#### **🆘 Pide Ayuda Si:**
- No entiendes la diferencia entre Layer y Tag
- La Collision Matrix te confunde
- Git no funciona correctamente

---

### 🎮 FEATURE 2: Player Controller 2D (3-4h)

#### **Conceptos a Aprender:**
- Rigidbody2D vs Transform.Translate
- FixedUpdate vs Update para física
- Ground detection con Raycast/OverlapCircle
- Input.GetAxisRaw vs Input.GetAxis

#### **Ejercicio Previo (15 min):**
```
Antes de escribir código, responde en papel:

1. ¿Por qué usar Rigidbody2D en lugar de mover el Transform directamente?
2. ¿Qué pasa si pones física en Update() en vez de FixedUpdate()?
3. ¿Cómo detectarías si el player está en el suelo?
   - Dibuja un diagrama

Busca las respuestas en Unity docs si no las sabes.
```

#### **Tu Tarea - Iteración 1 (Movimiento Básico):**
```
OBJETIVO: Player se mueve left/right

1. Crear GameObject "Player"
   - Agregar Rigidbody2D
   - Investiga cada propiedad: Gravity Scale, Linear Drag, etc.
   - ¿Qué hace "Freeze Rotation Z"? Prueba con y sin él

2. Agregar CapsuleCollider2D
   - ¿Por qué Capsule en vez de Box? Investiga

3. Crear script PlayerController2D.cs

4. Implementar movimiento horizontal:
   - Lee Unity docs: Input.GetAxisRaw("Horizontal")
   - Investiga: ¿Update() o FixedUpdate()? ¿Por qué?
   - Implementa usando Rigidbody2D.velocity

5. Testing:
   - Crea plataformas (GameObject con BoxCollider2D)
   - Asegura que player no atraviesa el suelo
```

#### **Desafío de Implementación:**
```
NO mires código de referencia todavía.

Intenta implementar:
1. Variable [SerializeField] private float moveSpeed
2. En Update(): Leer input horizontal
3. En FixedUpdate(): Aplicar velocidad al Rigidbody2D

Prueba, debuggea, ajusta.
```

#### **Tu Tarea - Iteración 2 (Jump):**
```
OBJETIVO: Player puede saltar

ANTES de implementar, investiga:
- ¿Cómo detectar si está en el suelo?
- ¿Qué es un Ground Check?
- ¿Raycast o OverlapCircle? Ventajas de cada uno

DECISIÓN: Elige TÚ qué método usar y por qué.

Implementa:
1. Ground detection
2. Jump cuando presionas Space (Input.GetButtonDown("Jump"))
3. Solo permite jump si isGrounded == true

Problemas comunes que enfrentarás:
- Jump infinito en el aire → Aprenderás sobre ground check
- Player se pega al techo → Aprenderás sobre Rigidbody constraints
- Jump no se siente bien → Aprenderás sobre jump force tuning
```

#### **Debugging Challenge:**
```
Si tu jump no funciona, ANTES de pedir ayuda:

1. Agrega Debug.Log() para verificar:
   - ¿Se está leyendo el input?
   - ¿isGrounded es true/false en el momento correcto?
   - ¿Se está aplicando la fuerza al Rigidbody?

2. Usa Gizmos para visualizar el ground check en Scene view
   - Investiga: OnDrawGizmosSelected()

3. Revisa la consola de Unity: ¿Hay errores?
```

#### **Recursos de Aprendizaje:**
```
Unity Docs:
- Rigidbody2D: docs.unity3d.com/Manual/class-Rigidbody2D.html
- Physics2D.OverlapCircle: docs.unity3d.com/ScriptReference/Physics2D.OverlapCircle.html
- Input System: docs.unity3d.com/ScriptReference/Input.html

YouTube:
- "2D Character Controller" - Brackeys
- "Ground Check in Unity 2D"
```

#### **Checkpoint de Validación:**
```
Antes de continuar:
✓ ¿Por qué la física va en FixedUpdate()?
✓ ¿Qué diferencia hay entre velocity y AddForce?
✓ ¿Por qué necesitas un LayerMask en el ground check?
✓ Explica tu implementación de ground detection

Si no puedes responder, investiga más antes de continuar.
```

#### **🆘 Pide Ayuda Si:**
- Tu player atraviesa el suelo (después de revisar layers)
- Ground check no funciona (después de Debug.Log)
- No entiendes la diferencia entre Update/FixedUpdate

---

### 📷 FEATURE 3: Cámara 2D (1-2h)

#### **Conceptos a Aprender:**
- Orthographic vs Perspective camera
- Camera follow patterns
- Cinemachine básico

#### **Ejercicio de Diseño:**
```
Antes de implementar:

1. Dibuja en papel 3 opciones de cámara:
   - Opción A: Cámara pegada al player (sin smoothing)
   - Opción B: Cámara con lag (smoothing)
   - Opción C: Cámara con "dead zone"

2. ¿Cuál se siente mejor para un Metroidvania? ¿Por qué?

3. Investiga qué es Cinemachine y si lo necesitas
```

#### **Tu Tarea - Opción 1: Manual (Simple):**
```
OBJETIVO: Entender cómo funciona una camera follow

1. Crear script CameraFollow2D.cs
2. Variables:
   - Transform target (el player)
   - Vector3 offset
   - float smoothSpeed

3. En LateUpdate():
   - Calcular posición deseada (target.position + offset)
   - Lerp entre posición actual y deseada
   - Aplicar a transform.position

4. Investiga:
   - ¿Por qué LateUpdate() y no Update()?
   - ¿Qué hace Vector3.Lerp?
   - Prueba diferentes valores de smoothSpeed

DESAFÍO: Implementa esto SIN ver código de referencia.
```

#### **Tu Tarea - Opción 2: Cinemachine (Profesional):**
```
OBJETIVO: Aprender a usar herramientas de la industria

1. Instalar Cinemachine:
   - Window → Package Manager → Cinemachine

2. Investigar:
   - Lee la documentación oficial de Cinemachine
   - ¿Qué es una Virtual Camera?
   - ¿Qué hace "Framing Transposer"?

3. Setup:
   - GameObject → Cinemachine → 2D Camera
   - Configura Follow: Player
   - Experimenta con los valores de Dead Zone
   - Investiga cada parámetro: ¿qué hace?

4. Compara: ¿Qué ventajas tiene Cinemachine vs tu script manual?
```

#### **Checkpoint:**
```
✓ ¿Cuándo usar LateUpdate() para cámaras?
✓ ¿Qué es el "camera lag" y por qué existe?
✓ Si usaste Cinemachine: Explica qué hace cada componente
```

---

### 📝 ENTREGABLE DÍA 1

Al final del día, debes tener:
```
✅ Proyecto configurado con Git
✅ Player que se mueve left/right
✅ Player que puede saltar
✅ Ground check funcional
✅ Cámara siguiendo al player

PERO MÁS IMPORTANTE:
✅ Entiendes POR QUÉ cada parte funciona como funciona
✅ Puedes explicar tu código a otra persona
✅ Has debuggeado al menos 3 problemas tú solo
```

#### **Autoevaluación:**
```
Antes de dormir, responde:
1. ¿Qué fue lo más difícil hoy?
2. ¿Qué concepto entiendes mejor ahora?
3. ¿Qué necesitas reforzar mañana?
4. ¿Cuántas veces recurriste a Debug.Log? (más = mejor)
```

---

## DÍA 2 (27 ENE) - COMBATE Y HEALTH SYSTEM

### **Objetivo del Día**: Entender sistemas de daño, State Machines, eventos

---

### 💊 FEATURE 4: Health System (2-3h)

#### **Conceptos a Aprender:**
- Component-based architecture
- Events y delegates en C#
- Reutilización de código

#### **Ejercicio de Diseño (30 min):**
```
Antes de escribir código:

1. En papel, diseña un Health System:
   - ¿Qué propiedades necesita? (health actual, max health, etc.)
   - ¿Qué métodos necesita? (TakeDamage, Heal, etc.)
   - ¿Cómo notificará a otros sistemas cuando cambia la salud?

2. Preguntas críticas:
   - ¿Puede el health ser negativo? ¿Cómo lo evitas?
   - ¿Qué pasa cuando health llega a 0?
   - ¿Cómo sabrá la UI que la salud cambió?

3. Investiga:
   - ¿Qué es un Event en C#?
   - ¿Qué es un delegate?
   - ¿Qué es el patrón Observer?
```

#### **Tu Tarea - Iteración 1 (Health Básico):**
```
OBJETIVO: Un componente reutilizable de salud

1. Crear Health.cs

2. Implementa (SIN copiar código):
   - Variables: maxHealth, currentHealth
   - Método: TakeDamage(float damage)
   - Método: Heal(float amount)
   - Lógica: No permitir health < 0 o > maxHealth

3. Testing:
   - Crea botones de prueba que llamen TakeDamage()
   - Usa Debug.Log para ver el health cambiar

4. Problema a resolver:
   - ¿Cómo Mathf.Clamp puede ayudarte?
```

#### **Tu Tarea - Iteración 2 (Events):**
```
OBJETIVO: Que otros scripts sepan cuando cambia el health

INVESTIGA PRIMERO:
- C# Events tutorial
- Unity Event system
- System.Action vs UnityEvent

IMPLEMENTA:
1. Agregar eventos:
   - public event System.Action OnDeath;
   - public event System.Action<float> OnHealthChanged;

2. Disparar eventos en los momentos correctos:
   - OnHealthChanged cuando TakeDamage() o Heal()
   - OnDeath cuando currentHealth <= 0

3. Crear un script HealthTester.cs:
   - Suscribirse a los eventos
   - Debug.Log cuando se disparan
   - Esto te enseña cómo funcionan los eventos

DESAFÍO: Implementa esto investigando la sintaxis de eventos en C#
```

#### **Debugging Challenge:**
```
Errores comunes que encontrarás:
1. NullReferenceException en eventos
   - Investiga el operador "?." (null-conditional)
   - OnDeath?.Invoke() vs OnDeath.Invoke()

2. Eventos no se disparan
   - ¿Te suscribiste correctamente?
   - Debug.Log DENTRO del método para verificar que se llama

3. Health no actualiza en Inspector
   - Investiga [SerializeField]
```

#### **Checkpoint:**
```
✓ ¿Qué es un event y por qué usarlo?
✓ ¿Cuál es la diferencia entre public variable y public event?
✓ ¿Por qué usar OnHealthChanged?.Invoke() con "?"?
✓ Explica cómo otro script se suscribe a tus eventos
```

---

### ⚔️ FEATURE 5: Combat System - Player Attack (3-4h)

#### **Conceptos a Aprender:**
- Collision detection en 2D
- Physics2D.OverlapCircle
- LayerMask filtering
- Gizmos para debugging visual

#### **Ejercicio de Diseño:**
```
Diseña en papel:

1. ¿Cómo detectarías enemigos cerca del player?
   - Opción A: OnTriggerEnter2D (trigger collider)
   - Opción B: Physics2D.OverlapCircle (cada vez que atacas)
   - Opción C: Raycast en dirección del player

   ¿Cuál eliges y por qué?

2. ¿Cómo sabe el player hacia dónde atacar?
   - ¿Hacia donde está mirando?
   - ¿Un punto específico en el espacio?

3. Dibuja el "attack range" en tu diseño
```

#### **Tu Tarea - Iteración 1 (Attack Detection):**
```
OBJETIVO: Detectar enemigos en rango de ataque

1. Crear PlayerAttack.cs

2. Variables a definir:
   - float attackRange
   - float attackDamage
   - LayerMask enemyLayer
   - Transform attackPoint (posición desde donde atacar)

3. Investigar:
   - ¿Qué hace Physics2D.OverlapCircle?
   - ¿Cómo se usa un LayerMask?
   - ¿Qué retorna OverlapCircle? (array de colliders)

4. Implementar método Attack():
   - Input: Tecla X (Input.GetKeyDown(KeyCode.X))
   - Detectar enemigos con OverlapCircle
   - Por ahora: Debug.Log cuántos enemigos detectó

5. Setup en Unity:
   - Crear GameObject hijo "AttackPoint" (posición frente al player)
   - Configurar enemyLayer en Inspector
```

#### **Tu Tarea - Iteración 2 (Aplicar Daño):**
```
OBJETIVO: Que el ataque haga daño

1. Implementar:
   - Por cada enemigo detectado en OverlapCircle
   - Obtener su componente Health (GetComponent<Health>())
   - Llamar health.TakeDamage(attackDamage)

2. Testing:
   - Crear un enemy dummy (GameObject con Health)
   - Atacarlo y ver en consola que recibe daño

3. Problema a resolver:
   - ¿Qué pasa si el enemigo no tiene Health component?
   - Investiga el operador "?." (null-conditional)
```

#### **Tu Tarea - Iteración 3 (Visual Debugging):**
```
OBJETIVO: Ver el attack range en Scene view

Investiga y aprende:
- OnDrawGizmosSelected()
- Gizmos.DrawWireSphere()

Implementa:
1. Visualizar el attackRange en Scene view
2. Color rojo cuando dibujes
3. Debe aparecer solo cuando seleccionas el player

Esto te ayudará MUCHO a debuggear.
```

#### **Checkpoint:**
```
✓ ¿Por qué usar LayerMask en lugar de detectar todos los colliders?
✓ ¿Cuál es la diferencia entre OverlapCircle y CircleCast?
✓ ¿Por qué GetComponent puede retornar null?
✓ ¿Cómo Gizmos te ayudan a debuggear?
```

---

### 🤖 FEATURE 6: Enemy AI 2D - State Machine (4-6h)

#### **Conceptos a Aprender:**
- Finite State Machines (FSM)
- Enum para estados
- Switch statements
- AI básica (patrol, chase, attack)

#### **Estudio Previo (30 min):**
```
INVESTIGACIÓN OBLIGATORIA antes de codear:

1. ¿Qué es una State Machine?
   - Busca: "Finite State Machine tutorial"
   - Dibuja un diagrama de estados para un enemigo:
     [Idle] → [Patrol] → [Chase] → [Attack]

2. ¿Qué son las transiciones?
   - ¿Cuándo un enemigo pasa de Patrol a Chase?
   - ¿Cuándo pasa de Chase a Attack?

3. ¿Cómo detectar al player?
   - ¿Raycast?
   - ¿Distance check?
   - ¿Trigger collider?

Diseña TU solución en papel antes de codear.
```

#### **Tu Tarea - Iteración 1 (Setup y Estados):**
```
OBJETIVO: Estructura básica de State Machine

1. Crear EnemyAI2D.cs

2. Definir enum State:
   public enum State { Idle, Patrol, Chase, Attack }

3. Variables:
   - State currentState
   - float detectionRange
   - float attackRange
   - LayerMask playerLayer
   - Transform player (cache reference en Start)

4. En Update():
   - Switch statement para currentState
   - Por ahora cada case solo Debug.Log el estado

Implementa esto. Es la base de todo.
```

#### **Tu Tarea - Iteración 2 (Patrol State):**
```
OBJETIVO: Enemigo patrulla left/right

DISEÑO:
- Enemy se mueve en un rango desde su posición inicial
- Al llegar al límite, da vuelta
- Velocidad constante

IMPLEMENTA:
1. Variables:
   - float patrolSpeed
   - float patrolDistance
   - Vector2 startPosition
   - bool movingRight

2. Método Patrol():
   - Mover enemy con Rigidbody2D.velocity
   - Detectar cuando está muy lejos de startPosition
   - Dar vuelta (flip)

3. Método Flip():
   - Cambiar localScale.x (multiplica por -1)

DESAFÍO: No copies código. Piensa cómo lo harías.
```

#### **Tu Tarea - Iteración 3 (Chase State):**
```
OBJETIVO: Enemy persigue al player

IMPLEMENTA:
1. Detectar cuando player está en rango:
   - float distanceToPlayer = Vector2.Distance(...)
   - Si distanceToPlayer < detectionRange → cambiar a Chase

2. Método Chase():
   - Calcular dirección hacia player
   - Mover enemy hacia player
   - Flip según dirección

3. Transición:
   - Si muy lejos → volver a Patrol
   - Si muy cerca → cambiar a Attack
```

#### **Tu Tarea - Iteración 4 (Attack State):**
```
OBJETIVO: Enemy ataca cuando está cerca

IMPLEMENTA:
1. Variables:
   - float attackDamage
   - float attackCooldown
   - float lastAttackTime

2. Método Attack():
   - Detenerse (velocity = 0)
   - Si cooldown pasó:
     - Buscar PlayerHealth component
     - Aplicar daño
     - Actualizar lastAttackTime

3. Cooldown:
   - Investiga: Time.time
   - ¿Cómo verificas que pasaron X segundos?
```

#### **Debugging Challenge:**
```
Problemas que enfrentarás:

1. Enemy no detecta player:
   - Debug.Log la distancia cada frame
   - Verifica el LayerMask del player
   - Dibuja Gizmos del detectionRange

2. Enemy se queda en un estado:
   - Debug.Log cada cambio de estado
   - Verifica las condiciones de transición

3. Patrol no funciona:
   - Debug.DrawLine para visualizar movimiento
   - Verifica que Rigidbody2D no está en Kinematic
```

#### **Checkpoint:**
```
✓ Dibuja el state diagram de tu enemigo
✓ Explica cuándo ocurre cada transición
✓ ¿Por qué es importante el attackCooldown?
✓ ¿Qué ventajas tiene usar State Machine vs if/else?
```

---

### 📝 ENTREGABLE DÍA 2

```
✅ Health System funcional con eventos
✅ Player puede atacar y hacer daño
✅ Enemy con AI básica (patrol, chase, attack)
✅ Enemy puede dañar al player
✅ Combat loop completo funciona

APRENDIZAJE:
✅ Entiendes qué es una State Machine
✅ Sabes usar eventos en C#
✅ Puedes debuggear con Gizmos y Debug.Log
✅ Entiendes LayerMask y por qué importa
```

---

## DÍA 3 (28 ENE) - SISTEMA DE TRANSFORMACIÓN (CORE)

### **Objetivo del Día**: Implementar la mecánica única del juego

---

### 🦎 FEATURE 7: Transformation System (6-8h)

**⚠️ ESTE ES EL SISTEMA MÁS COMPLEJO. Tómate tu tiempo para entenderlo.**

#### **Conceptos a Aprender:**
- ScriptableObjects
- Modular design
- Stats modifiers
- Spawning objects on death

#### **Estudio Previo OBLIGATORIO (1h):**
```
ANTES de escribir UNA SOLA línea:

1. Investiga ScriptableObjects:
   - ¿Qué son?
   - ¿Por qué usarlos en vez de MonoBehaviour?
   - Mira tutorial: "ScriptableObjects Explained" - Unity

2. Diseña el sistema en papel:
   - Dibuja el flujo completo:
     [Player mata Enemy]
     → [Enemy dropea Máscara]
     → [Player recoge Máscara]
     → [Player se transforma]
     → [Stats cambian]
     → [Puede usar habilidad única]
     → [Puede cancelar transformación]

3. Lista de componentes necesarios:
   - ¿Qué scripts necesitas?
   - ¿Cómo se comunican entre sí?

NO SIGAS HASTA QUE HAYAS HECHO ESTO.
```

#### **Tu Tarea - Fase 1: ScriptableObject (1-2h):**
```
OBJETIVO: Data container para transformaciones

1. Investiga cómo crear ScriptableObjects

2. Crear TransformationType.cs:
   [CreateAssetMenu(...)]

3. Propiedades a definir:
   - string transformName (ej: "Pez")
   - GameObject modelPrefab (modelo 3D)
   - float speedMultiplier
   - float jumpMultiplier
   - bool canSwim
   - bool canWallClimb
   - bool canFitSmallGaps

4. Crear 3 ScriptableObjects en Unity:
   - Transformation_Fish
   - Transformation_Spider
   - Transformation_Ladybug

5. Llenar valores en Inspector

INVESTIGA: ¿Por qué usar ScriptableObject en vez de una clase normal?
```

#### **Tu Tarea - Fase 2: PlayerTransform Component (2-3h):**
```
OBJETIVO: Sistema que gestiona las transformaciones

1. Crear PlayerTransform.cs

2. Variables:
   - TransformationType currentTransform
   - float baseSpeed, baseJump, baseAttack
   - GameObject currentModel
   - Referencias a PlayerController, PlayerAttack

3. Método TransformInto(TransformationType newType):
   Pseudocódigo:
   - Destruir modelo actual (si existe)
   - Instanciar nuevo modelo (newType.modelPrefab)
   - Aplicar stat modifiers a PlayerController
   - Guardar referencia a currentTransform

4. Método RevertToBase():
   - Destruir modelo transformado
   - Restaurar stats originales
   - currentTransform = null

5. Métodos de utilidad:
   - bool CanSwim() → return currentTransform?.canSwim ?? false
   - bool CanWallClimb() → ...
   - bool CanFitSmallGaps() → ...

IMPLEMENTA tú mismo. Si te atoras, pide ayuda ESPECÍFICA.
```

#### **Tu Tarea - Fase 3: TransformMask Collectible (1h):**
```
OBJETIVO: Item que transforma al player

1. Crear TransformMask.cs

2. Variables:
   - TransformationType transformationType

3. OnTriggerEnter2D:
   - Detectar si el collider es Player
   - Obtener PlayerTransform component
   - Llamar TransformInto(transformationType)
   - Destroy(gameObject) // Máscara desaparece

4. Setup en Unity:
   - Crear prefab TransformMask
   - Agregar SpriteRenderer (o modelo 3D)
   - Agregar collider con "Is Trigger" activado
   - Configurar Layer: Collectible
```

#### **Tu Tarea - Fase 4: MaskDrop on Enemy Death (1h):**
```
OBJETIVO: Enemy dropea máscara al morir

1. Crear MaskDrop.cs (component del enemy)

2. Variables:
   - GameObject maskPrefab
   - TransformationType transformType

3. En Start():
   - Obtener Health component del enemy
   - Suscribirse al evento OnDeath

4. Método DropMask():
   - Instantiate maskPrefab en posición del enemy
   - Configurar el TransformationType de la máscara

5. Importante:
   - OnDestroy() → Desuscribirse del evento
   - Investiga por qué es importante cleanup de eventos
```

#### **Testing Completo (1h):**
```
Escenario de Testing:

1. Setup:
   - Player en escena
   - Enemy con Health + MaskDrop
   - TransformMask prefab configurado

2. Test Flow:
   - Ataca enemy hasta matarlo
   - Enemy debe dropear máscara
   - Recoge máscara
   - Player debe transformarse
   - Stats deben cambiar
   - Debug.Log cada paso

3. Debug:
   - Si algo falla, usa Debug.Log en CADA paso
   - Verifica que eventos se disparan
   - Verifica que referencias no son null
```

#### **Problemas Comunes:**
```
1. NullReferenceException al transformar:
   - Verifica que PlayerController está cacheado en Start
   - Usa ?. operator: playerController?.SetSpeed(...)

2. Máscara no aparece:
   - Debug.Log en OnDeath event
   - Verifica que maskPrefab está asignado
   - Verifica posición de spawn

3. Stats no cambian:
   - Debug.Log antes y después de cambiar
   - Verifica los multipliers en ScriptableObject
```

#### **Checkpoint CRÍTICO:**
```
Antes de continuar al Día 4:

✓ ¿Qué es un ScriptableObject y por qué lo usaste?
✓ ¿Cómo funciona el operador "?." y por qué es importante?
✓ ¿Por qué es importante desuscribirse de eventos en OnDestroy?
✓ Dibuja el flujo completo del sistema de transformación
✓ Explica qué hace cada componente

Si no puedes responder, revisa y entiende antes de seguir.
```

---

### 📝 ENTREGABLE DÍA 3

```
✅ Sistema de transformación 100% funcional
✅ Puedes matar enemy, recoger máscara, transformarte
✅ Stats cambian al transformar
✅ Puedes revertir transformación

APRENDIZAJE:
✅ Entiendes ScriptableObjects
✅ Sabes implementar modular systems
✅ Entiendes el flujo de eventos complejo
✅ Puedes debuggear sistemas multi-componente
```

---

## DÍA 4 (29 ENE) - HABILIDADES ESPECIALES

### **Objetivo del Día**: Implementar las 3 habilidades únicas

---

### 🐟 FEATURE 8: Nadar (Water Zones) (2-3h)

#### **Conceptos a Aprender:**
- Trigger zones
- Physics en agua (sin gravedad)
- State temporal del player

#### **Diseño Previo:**
```
En papel:
1. ¿Cómo funcionaría el agua?
   - Player entra → gravedad off
   - Player sale → gravedad on
   - Si NO es Pez → no puede entrar (o muere)

2. ¿Cómo detectar entrada/salida?
   - OnTriggerEnter2D / OnTriggerExit2D

3. Dibuja el área de agua en tu nivel
```

#### **Tu Tarea:**
```
1. Crear WaterZone.cs

2. OnTriggerEnter2D:
   - Verificar si es Player
   - Obtener PlayerTransform
   - Si CanSwim():
     - Desactivar gravedad (rb.gravityScale = 0)
     - Mensaje: "Entraste al agua"
   - Si NO CanSwim():
     - Pushback (AddForce hacia atrás)
     - O aplicar daño

3. OnTriggerExit2D:
   - Restaurar gravedad (rb.gravityScale = valor original)

4. PlayerController modificación:
   - Agregar bool isInWater
   - Movimiento en agua: libre en X e Y
   - Investiga: Free movement en agua

IMPLEMENTA. Es más simple de lo que parece.
```

---

### 🕷️ FEATURE 9: Wall Climb (2-3h)

#### **Conceptos a Aprender:**
- Raycasting para detectar paredes
- Cambiar movimiento según contexto

#### **Diseño:**
```
1. ¿Cómo detectar pared?
   - Raycast horizontal
   - ¿Qué LayerMask?

2. ¿Cómo subir?
   - Input Vertical
   - rb.velocity.y = climbSpeed

3. ¿Cuándo dejar de escalar?
   - No hay pared
   - Player salta
   - Transformación cancela
```

#### **Tu Tarea:**
```
1. En PlayerController2D:

2. Método CheckWall():
   - Raycast derecha/izquierda según facing direction
   - LayerMask: Ground layer (walls)
   - Return true si toca pared

3. En Update():
   - if (canWallClimb && CheckWall() && Input.GetAxis("Vertical"))
     - rb.gravityScale = 0
     - rb.velocity.y = Input.GetAxis("Vertical") * climbSpeed
   - else:
     - Restaurar gravedad

DESAFÍO: Implementa sin ver código de referencia.
```

---

### 🐞 FEATURE 10: Small Gaps (1-2h)

#### **Concepto:**
- Collider más pequeño o gate específico

#### **Diseño Simple:**
```
Opción A: Cambiar tamaño del collider
- Al transformar en Mariquita:
  - collider.size *= 0.5f

Opción B: Gate con trigger
- SmallGapTrigger.cs
- Solo deja pasar si es Mariquita
```

#### **Tu Tarea:**
```
Elige la opción que prefieras e implementa.

Testing:
- Crear túnel estrecho
- Solo Mariquita puede pasar
- Otras formas son bloqueadas
```

---

### 📝 ENTREGABLE DÍA 4

```
✅ Las 3 habilidades funcionan
✅ Cada transformación desbloquea su área
✅ Física se comporta correctamente (agua, climb)

APRENDIZAJE:
✅ Raycasting avanzado
✅ Modificación de física en runtime
✅ Trigger zones
```

---

## DÍA 5 (30 ENE) - UI, AUDIO, POLISH

### **Objetivo**: Hacer el juego jugable y pulido

#### **Features del Día:**
- UI (Health bar, transform indicator)
- GameManager (restart, game over)
- Audio (SFX + music)
- Visual polish (particles, camera shake)

**Estas features son más directas. Implementa usando patterns aprendidos.**

---

## DÍA 6 (31 ENE) - TESTING + BUILD

### **Objetivo**: Build funcional en itch.io

---

## 📚 RECURSOS DE APRENDIZAJE GENERAL

### **Unity Docs (Tu Mejor Amigo):**
```
docs.unity3d.com

Busca SIEMPRE en docs antes de preguntar:
- Rigidbody2D
- Physics2D
- Input
- ScriptableObject
- Events
```

### **YouTube Channels:**
```
- Brackeys (basics)
- Code Monkey (intermediate)
- Tarodev (advanced)
- Sebastian Lague (concepts)
```

### **C# Fundamentals:**
```
- microsoft.com/learn/csharp
- Events and Delegates tutorial
- LINQ basics
```

---

## 🆘 CÓMO PEDIR AYUDA EFECTIVAMENTE

### **❌ MAL:**
```
"No funciona, ayuda"
"Tengo un error"
"No entiendo nada"
```

### **✅ BIEN:**
```
"Mi player atraviesa el suelo. Configuré:
- Rigidbody2D: Dynamic, Gravity 3
- Collider2D en player y ground
- Layers configurados
- Collision Matrix activada

He probado:
- Aumentar mass del Rigidbody
- Cambiar collision detection a Continuous

Error en consola: [copiar error]
Screenshot: [adjuntar]

¿Qué estoy pasando por alto?"
```

### **Información a Incluir Siempre:**
```
1. ¿Qué intentas lograr?
2. ¿Qué pasa actualmente?
3. ¿Qué has intentado?
4. Código relevante
5. Errores de consola
6. Screenshots si aplica
```

---

## 🎓 FILOSOFÍA DE APRENDIZAJE

### **El Error es tu Maestro:**
```
Cada error que resuelves TÚ SOLO = aprendizaje permanente
Cada error que te resuelvo yo = aprendizaje temporal

Meta: Que puedas implementar un juego completo TÚ SOLO después de esta jam.
```

### **No Copies y Pegues:**
```
Si ves código de referencia:
1. Léelo línea por línea
2. Entiende QUÉ hace cada línea
3. Cierra el código
4. Implementa de memoria
5. Compara tu versión con el original

Copiar/pegar = 0 aprendizaje
```

### **Debug.Log es tu Superpoder:**
```
Usa Debug.Log en TODOS LADOS:
- Antes y después de cambiar valores
- En cada rama de if/else
- Al entrar/salir de métodos
- Para verificar que eventos se disparan

Programadores pro usan Debug.Log TODO el tiempo.
```

---

## 📊 TRACKING DE PROGRESO

### **Daily Log (Llenar cada noche):**
```
DÍA: [fecha]

Implementé:
- [ ] Feature X
- [ ] Feature Y

Aprendí:
- Concepto X: [breve explicación]
- Concepto Y: [breve explicación]

Problemas resueltos:
1. [Problema] → [Solución encontrada]
2. [Problema] → [Solución encontrada]

Dudas para mañana:
- [ ] Duda 1
- [ ] Duda 2

Autoevaluación:
- Nivel de comprensión (1-10): __
- Confianza en implementar solo (1-10): __
```

---

## 🎯 META FINAL

Al terminar la jam, debes poder:

```
✅ Crear un player controller 2D desde cero
✅ Implementar State Machines
✅ Usar eventos para comunicación entre sistemas
✅ Crear ScriptableObjects y entender cuándo usarlos
✅ Debuggear problemas complejos
✅ Entender física 2D
✅ Implementar AI básica
✅ Construir sistemas modulares

Y lo más importante:
✅ Saber DÓNDE buscar cuando no sabes algo
✅ Tener confianza para implementar por tu cuenta
```

---

## 💪 MENSAJE FINAL

> **"La frustración es parte del aprendizaje. Cada error es una oportunidad para entender mejor. No te rindas cuando te atores, pide ayuda específica y sigue adelante."**

> **"Al final de esta jam, no solo tendrás un juego, tendrás conocimiento que nadie te puede quitar."**

**¡Vamos a hacer un gran juego Y aprender muchísimo en el proceso!**

---

## 📞 ESTOY AQUÍ PARA TI

Recuerda:
- Intenta primero (30 min mínimo)
- Pide ayuda específica cuando te atores
- No tengas miedo de preguntar "por qué"
- Celebra cada pequeño logro

**¡Éxito en tu aprendizaje! 🚀**

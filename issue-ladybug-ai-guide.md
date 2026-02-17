# LadyBug AI - Enemigo Evasivo - Implementation Guide

**Feature: Enemy AI Variant - LadyBug (Cobarde/Huye del Player)**

---

## 📚 PARTE 1: TEORÍA (30 min)

### ¿Qué vamos a implementar?

Un **enemigo evasivo** que huye del player cuando lo detecta. La **LadyBug** es un enemigo cobarde que:

- 🏃 **Huye** cuando detecta al player (no ataca)
- 🧱 **Valida suelo** adelante para no caer
- 🔄 **Cambia dirección** si encuentra un borde
- 🦘 **Salta sobre el player** si lo tiene muy cerca

Este patrón es común en juegos como:
- **Zelda**: Cuccos huyen del jugador
- **Hollow Knight**: Crawlids escapan al detectarte
- **Celeste**: Algunos enemigos evitan confrontación

---

### 🎯 Conceptos Clave

#### 1. **Finite State Machine (FSM) - Modificada**

**Base AI (EnemyAI2D):**
```
Idle → Patrol → Chase → Attack
           ↓              ↑
        Stunned ←---------┘
```

**LadyBug AI (Nuevo):**
```
Idle → Patrol → Flee (huir) → Stunned
           ↓         ↑
           └---------┘
```

**Cambios:**
- ❌ **Removido:** Chase (perseguir), Attack (atacar)
- ✅ **Agregado:** Flee (huir)
- ✅ **Modificado:** Patrol (incluye ground check)

---

#### 2. **Flee Behavior (Huir)**

**Concepto:** Moverse en dirección **opuesta** al player.

```
Player        LadyBug
  🧍 ----------> 🐞
     Detected!


  🧍 <---------- 🐞
     Fleeing!
```

**Cálculo de dirección de huida:**

```csharp
// Dirección HACIA el player
Vector2 toPlayer = (_player.position - transform.position).normalized;

// Dirección de HUIDA (opuesta)
Vector2 fleeDirection = -toPlayer;

// Aplicar movimiento
_rb.linearVelocity = new Vector2(fleeDirection.x * fleeSpeed, _rb.linearVelocity.y);
```

---

#### 3. **Edge Detection (Pared Invisible - Solución Simple)**

**Problema:** Si huyes sin mirar, caes por bordes.

```
❌ SIN protección:
    Suelo           Suelo
═══════════         ═══════
           ↓ 🐞 Cae!
           ↓
           ↓
```

```
✅ CON pared invisible:
    Suelo
═══════════         ═══════
         ║ ← Pared invisible (LadyBugOnly layer)
         🐞 (choca y voltea)
```

**Método:** **Pared invisible con colisión exclusiva**.

**Setup:**
1. Crear Layer **"LadyBugOnly"**
2. Configurar Collision Matrix (solo colisiona LadyBugOnly ↔ LadyBugOnly)
3. Crear prefab de pared invisible (BoxCollider2D en layer LadyBugOnly)
4. Level designer coloca paredes en bordes peligrosos

**Ventajas:**
- ✅ **Sin código extra** (física normal de Unity)
- ✅ **100% confiable** (no hay bugs de raycast)
- ✅ **Fácil para level designer** (arrastrar prefab)
- ✅ **Visual en editor** (opcional sprite rojo semi-transparente)

**Detección de colisión:**
```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    // Si choca con pared invisible, voltear
    if (collision.gameObject.layer == LayerMask.NameToLayer("LadyBugOnly"))
    {
        Flip();
    }
}
```

---

#### 4. **Jump Over Player (Salto Evasivo)**

**Concepto:** Si el player está MUY cerca mientras huyes, saltar para pasarlo por encima.

```
Situación: Player bloqueando camino

ANTES:                  DESPUÉS:
🧍 → 🐞                    🧍   🐞
                              ↗️ Salto!
                           🧍
```

**Condiciones para saltar:**
1. Está en estado **Flee**
2. Distancia al player < `jumpOverDistance` (ej: 1.5f)
3. Está en el **suelo** (`isGrounded = true`)
4. Player está **adelante** (misma dirección de huida)

**Implementación:**
```csharp
// Verificar si player está adelante
Vector2 toPlayer = _player.position - transform.position;
bool playerAhead = (toPlayer.x > 0 && _movingRight) || (toPlayer.x < 0 && !_movingRight);

// Si está cerca y adelante, saltar
if (playerAhead && distanceToPlayer < jumpOverDistance && _isGrounded)
{
    Jump();
}
```

---

### 📊 Comparación de Comportamientos

| Aspecto | EnemyAI2D (Base) | LadyBugAI |
|---------|------------------|-----------|
| **Detección de player** | Chase → Attack | Flee (huir) |
| **Movimiento hacia player** | ✅ Sí (Chase) | ❌ No (opuesto) |
| **Ataque** | ✅ Sí | ❌ No (solo huye) |
| **Edge protection** | ❌ No | ✅ Sí (pared invisible) |
| **Salto** | ❌ No | ✅ Sí (sobre player) |
| **Cambio de dirección** | Solo en patrulla | Patrulla + borde detectado |
| **Estados** | 5 (Idle, Patrol, Chase, Attack, Stunned) | 4 (Idle, Patrol, Flee, Stunned) |

---

### 🎮 Diagrama de Estados Completo

```
┌─────────────────────────────────────────────────────────┐
│                    LADYBUG AI FSM                       │
└─────────────────────────────────────────────────────────┘

    START
      ↓
   [Idle] ────────────────────┐
      ↓                        │
   [Patrol]                    │
      │                        │
      │ Player detectado       │
      │ (distancia < range)    │
      ↓                        │
   [Flee] ←──────────────┐    │
      │                   │    │
      │ No hay suelo      │    │
      │ adelante?         │    │
      ├─→ Flip() ─────────┘    │
      │                        │
      │ Player muy cerca?      │
      ├─→ Jump()               │
      │                        │
      │ Player lejos?          │
      │ (distancia > range)    │
      └─→ Return to Patrol ────┘

   [Stunned] ← (Cuando recibe daño)
      │
      │ (stunDuration termina)
      ↓
   Return to previous state
```

---

### ❌ Errores Comunes vs ✅ Soluciones

| ❌ Problema | ✅ Solución |
|------------|-----------|
| LadyBug cae por bordes mientras huye | Colocar paredes invisibles (EdgeWall) en todos los bordes |
| Se queda atascada contra pared | Flip() automático al colisionar con EdgeWall (OnCollisionEnter2D) |
| No salta sobre player | Verificar `isGrounded` antes de saltar |
| Salta constantemente | Cooldown de salto + distancia mínima |
| Dirección de huida incorrecta | `fleeDirection = -toPlayer` (negativo) |
| Flip mientras está en el aire | Opcional: Verificar `_isGrounded` en OnCollisionEnter2D |

---

### 🧩 Arquitectura del Código

**Estructura del script:**

```csharp
public class LadyBugAI : MonoBehaviour
{
    // ESTADOS
    private enum State { Idle, Patrol, Flee, Stunned }

    // MOVIMIENTO
    private float patrolSpeed = 2f;
    private float fleeSpeed = 4f;      // ← MÁS RÁPIDO que patrulla

    // DETECCIÓN
    private float detectionRange = 5f;
    private float fleeRangeHysteresis = 1.5f;

    // SALTO (NUEVO)
    private float jumpForce = 8f;
    private float jumpOverDistance = 1.5f;
    private float jumpCooldown = 1f;

    // GROUND CHECK (SELF - para salto)
    private Transform groundCheckPoint;
    private float groundCheckRadiusSelf = 0.2f;
    private LayerMask groundLayer;

    // MÉTODOS NUEVOS
    private void FleeBehavior();              // Huir del player
    private void Jump();                       // Saltar sobre player
    private void OnCollisionEnter2D(Collision2D collision); // Detectar pared invisible y voltear
}
```

---

### 🎓 Preguntas de Comprensión

<details>
<summary>❓ ¿Por qué LadyBug necesita paredes invisibles pero el AI base no?</summary>

**Respuesta:**

El **AI base (EnemyAI2D)** solo se mueve durante **Patrol** en un área limitada (`patrolDistance`) alrededor de su posición inicial. Nunca se aleja mucho, así que el diseñador de niveles puede asegurar que no haya bordes en esa zona.

**LadyBug**, en cambio, **huye sin límite de distancia** cuando detecta al player. Puede correr indefinidamente en una dirección, lo que aumenta el riesgo de caer por un borde si no está protegida.

**Solución:** Usar paredes invisibles (EdgeWall) en bordes peligrosos. Son más simples y confiables que código de ground check.

**Comparación:**
- AI Base: "Patrulla 3m a izquierda, 3m a derecha" (zona segura predefinida)
- LadyBug: "Huye hasta que player esté lejos" (puede llegar a cualquier borde) → Necesita EdgeWalls
</details>

<details>
<summary>❓ ¿Por qué `fleeSpeed` debe ser mayor que `patrolSpeed`?</summary>

**Respuesta:**

Para crear **tensión en el gameplay**. Si la LadyBug huye **más lento** que el player se mueve, el jugador siempre la alcanzaría, haciendo que el comportamiento de huida sea inútil.

**Valores recomendados:**
- `patrolSpeed = 2f` (lento, relajado)
- `fleeSpeed = 4f` (rápido, pánico)
- `playerMoveSpeed = 5f` (referencia)

Así, el player PUEDE alcanzarla, pero requiere esfuerzo. Si la LadyBug tiene ventaja inicial, puede escapar.

**Game feel:**
- Flee demasiado lento → Demasiado fácil de atrapar (frustrante para LadyBug)
- Flee demasiado rápido → Imposible de atrapar (frustrante para player)
- **Balance:** Flee = 80% de player speed aproximadamente
</details>

<details>
<summary>❓ ¿Cómo funciona el salto "sobre el player"? ¿No simplemente salta?</summary>

**Respuesta:**

Es un salto **contextual**, no aleatorio. Solo salta cuando:

1. **Player está adelante** (en la dirección de huida)
2. **Muy cerca** (< `jumpOverDistance`)
3. **En el suelo** (puede saltar)

**Escenario:**
```
Situación A: Player detrás (NO saltar)
🐞 → → →    🧍
(Huyendo)

Situación B: Player adelante bloqueando (SÍ saltar)
   🐞
    ↗️ Salto!
🧍
```

El salto le permite **evitar quedar atrapada** contra el player. Sin esto, quedaría "empujando" contra el player sin poder moverse.

**Implementación técnica:**
- Verificar que `toPlayer.x` tiene el mismo signo que la dirección de movimiento
- Solo saltar si distancia < 1.5f (rango de "pánico")
- Cooldown para evitar saltos continuos
</details>

---

## 🛠️ PARTE 2: IMPLEMENTACIÓN (2-3h)

### Setup Previo en Unity

Antes de empezar con el código, prepara el escenario de prueba.

#### Paso 0A: Configurar Layer "LadyBugOnly"

**En Unity:**

1. **Crear Layer:**
   - Edit → Project Settings → Tags and Layers
   - En "Layers", encontrar primer slot vacío
   - Agregar: "LadyBugOnly"

2. **Configurar Collision Matrix:**
   - Edit → Project Settings → Physics 2D
   - Scroll hasta "Layer Collision Matrix"
   - Encontrar "LadyBugOnly" (vertical) y "Enemy" (horizontal)
   - **Marcar** la intersección (activar colisión)
   - **Desmarcar** todas las demás intersecciones de LadyBugOnly (solo colisiona con Enemy layer)

3. **Crear Prefab EdgeWall:**
   - Hierarchy → Create Empty GameObject → "EdgeWall"
   - Add Component → Box Collider 2D
   - Box Collider 2D settings:
     - Size: (0.2, 2) // Delgado y alto
     - Is Trigger: ❌ NO (debe ser collider sólido)
   - GameObject settings:
     - Layer: "LadyBugOnly"
   - (Opcional) Add SpriteRenderer con sprite rojo semi-transparente para visualizar en editor
   - Drag to Prefabs folder: Assets/Prefabs/Level/EdgeWall.prefab
   - **Uso:** Level designer coloca este prefab en bordes de plataformas para prevenir caídas de LadyBug

---

#### Paso 0B: Crear Prefab de LadyBug Enemy

**En Unity:**

1. **Duplicar enemigo base:**
   - Hierarchy → Enemy (base) → Duplicate
   - Renombrar a "LadyBug_Enemy"

2. **Configurar componentes:**
   - Remove script `EnemyAI2D` (lo reemplazaremos)
   - Mantener: Rigidbody2D, Collider2D, Health

3. **Ajustar física (Rigidbody2D):**
   - Gravity Scale: `3` (igual que player)
   - Linear Drag: `1` (para frenado suave)
   - Freeze Rotation Z: `✅ activado`

4. **Crear Prefab:**
   - Assets → Prefabs → Enemies → "LadyBug_Enemy.prefab"

---

### Paso 1: Crear Script Base

**TU TURNO:** Crea el archivo `LadyBugAI.cs` con la estructura básica.

**Ubicación:** `Assets/Scripts/Enemies/LadyBugAI.cs`

**Requisitos:**
1. Enum de estados (Idle, Patrol, Flee, Stunned)
2. Variables de movimiento (patrol, flee speeds)
3. Variables de detección (range, hysteresis)
4. Referencias (Rigidbody2D, Transform player, etc.)

<details>
<summary>💡 Pista 1: Estructura del enum y variables básicas</summary>

```csharp
using UnityEngine;

public class LadyBugAI : MonoBehaviour
{
    [SerializeField]
    private enum State
    {
        Idle,
        Patrol,
        Flee,    // ← NUEVO (reemplaza Chase/Attack)
        Stunned
    }

    private State _currentState;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float fleeSpeed = 4f; // ← NUEVO (más rápido)
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool _movingRight = true;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    private float _fleeRangeHysteresis = 1.5f; // ← Hysteresis para Flee

    // TODO: Agregar variables de salto y ground check self
}
```
</details>

<details>
<summary>💡 Pista 2: Variables de salto y ground check self</summary>

```csharp
[Header("Jump")]
[SerializeField] private float jumpForce = 8f;
[SerializeField] private float jumpOverDistance = 1.5f;
[SerializeField] private float jumpCooldown = 1f;
private float _lastJumpTime = -999f;

[Header("Knockback/Stun")]
[SerializeField] private float stunDuration = 0.5f;
private float _stunEndTime = 0f;
private State _previousState = State.Patrol;

[Header("References")]
private Rigidbody2D _rb;
private Transform _player;
private Vector2 _startPosition;
private bool _isGrounded;
```
</details>

<details>
<summary>✅ Solución Completa - Script Base</summary>

```csharp
using UnityEngine;

public class LadyBugAI : MonoBehaviour
{
    [SerializeField]
    private enum State
    {
        Idle,
        Patrol,
        Flee,    // Huir del player
        Stunned
    }

    private State _currentState;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool _movingRight = true;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    private float _fleeRangeHysteresis = 1.5f; // Player debe alejarse más para dejar de huir

    [Header("Jump Over Player")]
    [Tooltip("Fuerza del salto evasivo")]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("Distancia mínima al player para intentar saltar sobre él")]
    [SerializeField] private float jumpOverDistance = 1.5f;

    [Tooltip("Cooldown entre saltos")]
    [SerializeField] private float jumpCooldown = 1f;

    private float _lastJumpTime = -999f;

    [Header("Knockback/Stun")]
    [SerializeField] private float stunDuration = 0.5f;
    private float _stunEndTime = 0f;
    private State _previousState = State.Patrol;

    [Header("Ground Check (Self)")]
    [Tooltip("Transform para verificar si LadyBug está en el suelo")]
    [SerializeField] private Transform groundCheckPoint;

    [Tooltip("Radio del ground check de LadyBug")]
    [SerializeField] private float groundCheckRadiusSelf = 0.2f;

    [Tooltip("Layer del suelo (Ground)")]
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    private Rigidbody2D _rb;
    private Transform _player;
    private Vector2 _startPosition;
    private bool _isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[LadyBugAI] Player not found! Make sure Player has 'Player' tag.");
        }

        _startPosition = transform.position;

        // Subscribe to knockback event
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnTakeDamageWithKnockback += HandleKnockback;
        }

        ChangeState(State.Patrol);
    }

    void OnDestroy()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnTakeDamageWithKnockback -= HandleKnockback;
        }
    }

    void Update()
    {
        // Verificar si está en el suelo
        _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadiusSelf, groundLayer);

        switch (_currentState)
        {
            case State.Idle:
                // Idle behavior (si lo necesitas)
                break;

            case State.Patrol:
                PatrolBehavior();
                break;

            case State.Flee:
                FleeBehavior();
                break;

            case State.Stunned:
                StunnedBehavior();
                break;
        }
    }

    private void ChangeState(State newState)
    {
        Debug.Log($"[LadyBugAI] State: {_currentState} → {newState}");
        _currentState = newState;
    }

    // TODO: Implementar métodos de comportamiento
    private void PatrolBehavior() { }
    private void FleeBehavior() { }
    private void StunnedBehavior() { }
    private void HandleKnockback(Vector2 direction, float damage) { }
    private void Jump() { }
    private void Flip() { }
    private void OnCollisionEnter2D(Collision2D collision) { } // Detectar pared invisible
}
```

**Valores recomendados (Inspector):**
- `patrolSpeed = 2f`
- `fleeSpeed = 4f`
- `detectionRange = 5f`
- `jumpForce = 8f`
- `jumpOverDistance = 1.5f`
- `jumpCooldown = 1f`
- `stunDuration = 0.5f`
- `groundCheckRadiusSelf = 0.2f`
- `groundLayer = Ground` (Layer 8)
</details>

---

### Paso 2: Implementar PatrolBehavior

**TU TURNO:** Implementa el comportamiento de patrulla (igual que el base AI).

**Requisitos:**
1. Moverse left/right dentro de `patrolDistance`
2. Flip() al llegar a los límites
3. Detectar al player y cambiar a Flee

<details>
<summary>💡 Pista 1: Movimiento básico de patrulla</summary>

```csharp
private void PatrolBehavior()
{
    // Dirección de movimiento
    float direction = _movingRight ? 1 : -1;
    _rb.linearVelocity = new Vector2(direction * patrolSpeed, _rb.linearVelocity.y);

    // Verificar límites de patrulla
    float distanceFromStart = transform.position.x - _startPosition.x;

    if (_movingRight && distanceFromStart > patrolDistance)
    {
        Flip();
    }
    else if (!_movingRight && distanceFromStart < -patrolDistance)
    {
        Flip();
    }

    // TODO: Detectar player
}
```
</details>

<details>
<summary>💡 Pista 2: Detección de player</summary>

```csharp
private void PatrolBehavior()
{
    float direction = _movingRight ? 1 : -1;
    _rb.linearVelocity = new Vector2(direction * patrolSpeed, _rb.linearVelocity.y);

    float distanceFromStart = transform.position.x - _startPosition.x;

    // Límites de patrulla
    if (_movingRight && distanceFromStart > patrolDistance)
    {
        Flip();
    }
    else if (!_movingRight && distanceFromStart < -patrolDistance)
    {
        Flip();
    }

    // Detectar player
    if (_player != null)
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer < detectionRange)
        {
            ChangeState(State.Flee); // Huir en lugar de Chase
        }
    }
}
```
</details>

<details>
<summary>✅ Solución Completa - PatrolBehavior</summary>

```csharp
/// <summary>
/// Patrulla dentro de un área definida y detecta al player.
/// Si detecta player, cambia a estado Flee.
/// Los bordes están protegidos por paredes invisibles (LadyBugOnly layer).
/// </summary>
private void PatrolBehavior()
{
    // Movimiento horizontal
    float direction = _movingRight ? 1 : -1;
    _rb.linearVelocity = new Vector2(direction * patrolSpeed, _rb.linearVelocity.y);

    // Verificar límites de patrulla (distancia desde posición inicial)
    float distanceFromStart = transform.position.x - _startPosition.x;

    if (_movingRight && distanceFromStart > patrolDistance)
    {
        Flip();
    }
    else if (!_movingRight && distanceFromStart < -patrolDistance)
    {
        Flip();
    }

    // Detectar si el player está cerca
    if (_player != null)
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer < detectionRange)
        {
            Debug.Log($"[LadyBugAI] Player detected at distance {distanceToPlayer}, starting to flee!");
            ChangeState(State.Flee);
        }
    }
}
```
</details>

---

### Paso 3: Implementar FleeBehavior

**TU TURNO:** Implementa el comportamiento de huida del player.

**Requisitos:**
1. Calcular dirección **opuesta** al player
2. Moverse en esa dirección a `fleeSpeed`
3. Flip según dirección de huida
4. Intentar saltar sobre player si está muy cerca
5. Volver a Patrol si player se aleja

**Nota:** Los bordes están manejados automáticamente por paredes invisibles (OnCollisionEnter2D).

<details>
<summary>💡 Pista 1: Calcular dirección de huida</summary>

```csharp
private void FleeBehavior()
{
    if (_player == null) return;

    // Dirección HACIA el player
    Vector2 toPlayer = (_player.position - transform.position).normalized;

    // Dirección de HUIDA (opuesta)
    Vector2 fleeDirection = -toPlayer;

    // Aplicar velocidad de huida
    _rb.linearVelocity = new Vector2(fleeDirection.x * fleeSpeed, _rb.linearVelocity.y);

    // TODO: Flip según dirección
    // TODO: Salto sobre player
    // TODO: Verificar si volver a Patrol
}
```
</details>

<details>
<summary>💡 Pista 2: Flip y salto</summary>

```csharp
private void FleeBehavior()
{
    if (_player == null) return;

    Vector2 toPlayer = (_player.position - transform.position).normalized;
    Vector2 fleeDirection = -toPlayer;

    _rb.linearVelocity = new Vector2(fleeDirection.x * fleeSpeed, _rb.linearVelocity.y);

    // Flip según dirección de huida
    if (fleeDirection.x > 0 && !_movingRight)
    {
        Flip();
    }
    else if (fleeDirection.x < 0 && _movingRight)
    {
        Flip();
    }

    // Intentar saltar sobre player si está muy cerca
    float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

    if (distanceToPlayer < jumpOverDistance && _isGrounded)
    {
        // Verificar si player está adelante (en dirección de huida)
        bool playerAhead = (toPlayer.x > 0 && _movingRight) || (toPlayer.x < 0 && !_movingRight);

        if (playerAhead && Time.time >= _lastJumpTime + jumpCooldown)
        {
            Jump();
        }
    }

    // TODO: Verificar si volver a Patrol
}
```
</details>

<details>
<summary>✅ Solución Completa - FleeBehavior</summary>

```csharp
/// <summary>
/// Comportamiento de huida: moverse en dirección opuesta al player.
/// Los bordes están protegidos por paredes invisibles (OnCollisionEnter2D).
/// Incluye salto sobre player si está muy cerca.
/// </summary>
private void FleeBehavior()
{
    if (_player == null) return;

    // Calcular dirección HACIA el player
    Vector2 toPlayer = (_player.position - transform.position).normalized;

    // Dirección de HUIDA (opuesta al player)
    Vector2 fleeDirection = -toPlayer;

    // Aplicar velocidad de huida
    _rb.linearVelocity = new Vector2(fleeDirection.x * fleeSpeed, _rb.linearVelocity.y);

    // Flip según dirección de huida
    if (fleeDirection.x > 0 && !_movingRight)
    {
        Flip();
    }
    else if (fleeDirection.x < 0 && _movingRight)
    {
        Flip();
    }

    // Calcular distancia al player
    float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

    // Intentar saltar sobre player si está muy cerca
    if (distanceToPlayer < jumpOverDistance && _isGrounded)
    {
        // Verificar si player está ADELANTE (en dirección de movimiento)
        // Si player está detrás, no saltar
        bool playerAhead = (toPlayer.x > 0 && _movingRight) || (toPlayer.x < 0 && !_movingRight);

        if (playerAhead && Time.time >= _lastJumpTime + jumpCooldown)
        {
            Debug.Log("[LadyBugAI] Player blocking path, jumping over!");
            Jump();
        }
    }

    // Volver a Patrol si player se aleja lo suficiente (hysteresis)
    if (distanceToPlayer > detectionRange * _fleeRangeHysteresis)
    {
        Debug.Log($"[LadyBugAI] Player far enough ({distanceToPlayer}), returning to patrol");
        ChangeState(State.Patrol);
    }
}
```

**Explicación:**
- `fleeDirection = -toPlayer` → Invertir dirección (huir)
- `fleeSpeed > patrolSpeed` → Huida más rápida que patrulla
- `!HasGroundAhead()` → Cambiar dirección si hay borde
- `playerAhead` → Solo saltar si player bloquea el camino
- `_fleeRangeHysteresis` → Evitar flickering entre Flee/Patrol (player debe alejarse más)
</details>

---

### Paso 4: Implementar OnCollisionEnter2D (Detectar Pared Invisible)

**TU TURNO:** Implementa la detección de colisión con paredes invisibles para voltear automáticamente.

**Requisitos:**
1. Detectar colisión con layer "LadyBugOnly"
2. Voltear (Flip) automáticamente

<details>
<summary>✅ Solución Completa - OnCollisionEnter2D</summary>

```csharp
/// <summary>
/// Detecta colisión con paredes invisibles (LadyBugOnly layer) y voltea automáticamente.
/// Esto evita que LadyBug caiga por bordes durante patrulla o huida.
/// </summary>
private void OnCollisionEnter2D(Collision2D collision)
{
    // Verificar si es una pared invisible (LadyBugOnly layer)
    if (collision.gameObject.layer == LayerMask.NameToLayer("LadyBugOnly"))
    {
        Debug.Log("[LadyBugAI] Hit edge wall, flipping direction");
        Flip();
    }
}
```

**Explicación:**
- `LayerMask.NameToLayer("LadyBugOnly")` → Obtener índice del layer por nombre
- Flip automático cuando choca con pared invisible
- Simple y confiable (física de Unity maneja todo)
</details>

---

### Paso 5: Implementar Jump()

**TU TURNO:** Implementa el salto evasivo sobre el player.

**Requisitos:**
1. Aplicar fuerza vertical al Rigidbody2D
2. Marcar timestamp del último salto
3. Log de debug

<details>
<summary>💡 Pista</summary>

```csharp
private void Jump()
{
    if (_rb == null) return;

    // Aplicar fuerza vertical
    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);

    // Marcar timestamp
    _lastJumpTime = Time.time;

    Debug.Log("[LadyBugAI] Jumped!");
}
```
</details>

<details>
<summary>✅ Solución Completa - Jump</summary>

```csharp
/// <summary>
/// Salta verticalmente (para pasar sobre el player cuando bloquea el camino).
/// Mantiene velocidad horizontal para continuar huyendo.
/// </summary>
private void Jump()
{
    if (_rb == null || !_isGrounded) return;

    // Aplicar fuerza vertical (mantener velocidad horizontal actual)
    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);

    // Marcar timestamp del último salto (para cooldown)
    _lastJumpTime = Time.time;

    Debug.Log("[LadyBugAI] Jumping over player!");
}
```

**Nota:** Mantener `linearVelocity.x` para que continúe huyendo horizontalmente mientras salta.
</details>

---

### Paso 6: Implementar StunnedBehavior y HandleKnockback

**TU TURNO:** Copia la lógica de stun del AI base (es idéntica).

<details>
<summary>✅ Solución Completa - Stun System</summary>

```csharp
/// <summary>
/// Comportamiento de stun: esperar hasta que termine la duración.
/// </summary>
private void StunnedBehavior()
{
    // No sobrescribir velocidad - dejar que knockback funcione naturalmente

    if (Time.time >= _stunEndTime)
    {
        Debug.Log($"[LadyBugAI] Stun ended, returning to {_previousState}");
        ChangeState(_previousState);
    }
}

/// <summary>
/// Maneja el knockback cuando recibe daño.
/// </summary>
private void HandleKnockback(Vector2 direction, float damage)
{
    if (_rb == null) return;

    // Guardar estado anterior
    if (_currentState != State.Stunned)
    {
        _previousState = _currentState;
    }

    // Aplicar knockback
    float knockbackForce = damage * 0.6f;
    _rb.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);

    // Cambiar a Stunned
    _stunEndTime = Time.time + stunDuration;
    ChangeState(State.Stunned);

    Debug.Log($"[LadyBugAI] Knocked back! Stunned for {stunDuration}s");
}
```
</details>

---

### Paso 7: Implementar Flip()

**TU TURNO:** Implementa la rotación del sprite.

<details>
<summary>✅ Solución Completa - Flip</summary>

```csharp
/// <summary>
/// Voltea el sprite (escala X negativa).
/// </summary>
private void Flip()
{
    _movingRight = !_movingRight;

    Vector3 scale = transform.localScale;
    scale.x *= -1;
    transform.localScale = scale;
}
```
</details>

---

### Paso 8: Gizmos de Debug

**OPCIONAL:** Agrega visualización en Scene view.

<details>
<summary>✅ Solución - OnDrawGizmosSelected</summary>

```csharp
#if UNITY_EDITOR
private void OnDrawGizmosSelected()
{
    // Área de patrulla
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(_startPosition, patrolDistance);

    // Rango de detección (color según estado)
    Gizmos.color = _currentState == State.Flee ? Color.yellow : Color.green;
    Gizmos.DrawWireSphere(transform.position, detectionRange);

    // Ground check self (para salto)
    if (groundCheckPoint != null)
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadiusSelf);
    }

    // Jump over range
    Gizmos.color = Color.magenta;
    Gizmos.DrawWireSphere(transform.position, jumpOverDistance);
}
#endif
```
</details>

---

## 🧪 PARTE 3: TESTING (30-45 min)

### Setup en Unity

**Antes de testear:**

1. **Crear GroundCheck Point:**
   - En LadyBug_Enemy, crear Empty Child: "GroundCheckPoint"
   - Posicionar en los pies del enemigo (Y = -0.5 aprox)
   - Asignar a `groundCheckPoint` en Inspector

2. **Asignar Layer Mask:**
   - `groundLayer` → Seleccionar "Ground"

3. **Asignar Layer del LadyBug:**
   - LadyBug_Enemy GameObject → Layer → "Enemy"

4. **Ajustar valores iniciales:**
   - `patrolSpeed = 2f`
   - `fleeSpeed = 4f`
   - `detectionRange = 5f`
   - `jumpForce = 8f`
   - `jumpOverDistance = 1.5f`
   - `jumpCooldown = 1f`
   - `groundCheckRadiusSelf = 0.2f`

---

### Tests Incrementales

#### ✅ Test 1: Patrulla Básica

**Objetivo:** Verificar que la patrulla funciona igual que el AI base.

**Pasos:**
1. Play en Unity
2. LadyBug debe patrullar left/right dentro de `patrolDistance`
3. Debe voltear (Flip) al llegar a los límites

**Resultado esperado:**
```
[LadyBugAI] State: Idle → Patrol
(Patrulla 3m izq, 3m der)
```

**Debugging:**
- Si no se mueve: Verificar Rigidbody2D, `patrolSpeed > 0`
- Si no voltea: Verificar `patrolDistance` en Inspector

---

#### ✅ Test 2: Pared Invisible - Edge Detection (Patrol)

**Objetivo:** Verificar que las paredes invisibles evitan caídas durante patrulla.

**Setup:**
1. Crear Layer "LadyBugOnly" en Unity (si no existe)
2. Configurar Collision Matrix: LadyBugOnly ↔ LadyBug (activado)
3. Crear GameObject "EdgeWall":
   - Add BoxCollider2D (ajustar tamaño: 0.2 ancho x 2 altura)
   - Layer: LadyBugOnly
   - Position: En el borde de la plataforma (verticalmente)
4. Colocar LadyBug cerca del borde

**Pasos:**
1. Play
2. LadyBug patrulla hacia el borde
3. Debe chocar con pared invisible y voltear automáticamente

**Resultado esperado:**
```
[LadyBugAI] Hit edge wall, flipping direction
```

**Debugging:**
- Si cae: Verificar que EdgeWall tiene layer "LadyBugOnly"
- Si no voltea: Verificar Collision Matrix (LadyBugOnly ↔ LadyBug)
- Si no detecta: Verificar que LadyBug tiene el layer correcto configurado

---

#### ✅ Test 3: Detección de Player y Flee

**Objetivo:** Verificar que huye cuando detecta al player.

**Pasos:**
1. Acercarte al LadyBug (< 5f)
2. LadyBug debe cambiar a estado Flee
3. Debe huir en dirección **opuesta** a ti

**Resultado esperado:**
```
[LadyBugAI] Player detected at distance 4.5, starting to flee!
[LadyBugAI] State: Patrol → Flee
```

**Debugging:**
- Si no detecta: Verificar `detectionRange` y Player tag
- Si huye hacia ti: Verificar `fleeDirection = -toPlayer`
- Si no huye: Verificar `fleeSpeed > 0`

---

#### ✅ Test 4: Pared Invisible - Edge Detection Mientras Huye

**Objetivo:** Verificar que las paredes invisibles evitan caídas durante huida.

**Setup:**
1. Asegurar que EdgeWalls están colocadas en todos los bordes peligrosos
2. Perseguir a LadyBug hacia el borde

**Pasos:**
1. LadyBug huye hacia el borde
2. Debe chocar con pared invisible
3. Debe voltear y huir en dirección opuesta (alejándose del player)

**Resultado esperado:**
```
[LadyBugAI] Hit edge wall, flipping direction
```

**Debugging:**
- Si cae: Verificar que hay EdgeWall en ese borde
- Si no voltea: Verificar OnCollisionEnter2D está implementado
- Si atraviesa pared: Verificar Collision Matrix configurado correctamente

---

#### ✅ Test 5: Salto Sobre Player

**Objetivo:** Verificar que salta cuando player bloquea el camino.

**Setup:**
1. Crear pasillo estrecho
2. Bloquear el camino de huida de LadyBug

**Pasos:**
1. Perseguir a LadyBug
2. Cuando esté muy cerca (< 1.5f) y bloqueada
3. Debe saltar sobre ti

**Resultado esperado:**
```
[LadyBugAI] Player blocking path, jumping over!
[LadyBugAI] Jumped!
```

**Debugging:**
- Si no salta: Verificar `_isGrounded = true`, `jumpOverDistance`, `playerAhead`
- Si salta constantemente: Verificar `jumpCooldown` funciona
- Si salta muy bajo: Aumentar `jumpForce` (ej: 10f)

---

#### ✅ Test 6: Volver a Patrol

**Objetivo:** Verificar que vuelve a patrullar cuando player se aleja.

**Pasos:**
1. Hacer que LadyBug huya
2. Alejarte del LadyBug (> 7.5f con hysteresis 1.5)
3. Debe volver a Patrol

**Resultado esperado:**
```
[LadyBugAI] Player far enough (8.2), returning to patrol
[LadyBugAI] State: Flee → Patrol
```

**Debugging:**
- Si vuelve muy pronto: Aumentar `_fleeRangeHysteresis` (ej: 2.0f)
- Si nunca vuelve: Verificar cálculo de distancia

---

#### ✅ Test 7: Knockback y Stun

**Objetivo:** Verificar que el sistema de stun funciona.

**Pasos:**
1. Atacar a LadyBug (con player attack)
2. Debe recibir knockback
3. Debe entrar en estado Stunned
4. Después de 0.5s, volver al estado anterior

**Resultado esperado:**
```
[LadyBugAI] Knocked back! Stunned for 0.5s
[LadyBugAI] State: Flee → Stunned
[LadyBugAI] Stun ended, returning to Flee
```

---

## 🐛 DEBUGGING

### Problema 1: "LadyBug cae por bordes"

**Síntomas:**
- Pared invisible no funciona
- Cae mientras patrulla o huye

**Causas:**

❌ **Pared invisible mal configurada**
```
// EdgeWall no tiene layer "LadyBugOnly"
// O Collision Matrix no está configurado
```

✅ **Solución:**
```
1. Unity → Inspector → EdgeWall GameObject
2. Layer → "LadyBugOnly"
3. Edit → Project Settings → Physics 2D
4. Collision Matrix: Marcar intersección LadyBugOnly ↔ LadyBug
```

❌ **OnCollisionEnter2D no implementado**
```csharp
// Falta el método en LadyBugAI.cs
```

✅ **Solución:**
```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.layer == LayerMask.NameToLayer("LadyBugOnly"))
    {
        Flip();
    }
}
```

❌ **EdgeWall no colocada en todos los bordes**
```
// Level designer olvidó poner pared en un borde
```

✅ **Solución:**
```
// Colocar EdgeWall prefab en TODOS los bordes peligrosos
// Usar Scene view para verificar visualmente
```

---

### Problema 2: "No huye del player"

**Síntomas:**
- Detecta al player pero no cambia a Flee
- Se queda en Patrol

**Causas:**

❌ **Player sin tag "Player"**
```csharp
// _player es null
```

✅ **Solución:**
```csharp
// En Unity: Seleccionar Player GameObject → Inspector → Tag → "Player"
```

❌ **`detectionRange` muy bajo**
```csharp
detectionRange = 1f; // ← Muy cerca
```

✅ **Solución:**
```csharp
detectionRange = 5f; // Rango razonable
```

---

### Problema 3: "Huye hacia el player en lugar de alejarse"

**Síntomas:**
- LadyBug corre HACIA el player
- Dirección de huida incorrecta

**Causa:**

❌ **Falta negativo en fleeDirection**
```csharp
Vector2 fleeDirection = toPlayer; // ← ERROR: va HACIA player
```

✅ **Solución:**
```csharp
Vector2 fleeDirection = -toPlayer; // ← CORRECTO: opuesto
```

---

### Problema 4: "No salta sobre el player"

**Síntomas:**
- Llega cerca del player pero no salta
- Se queda empujando contra el player

**Causas:**

❌ **`_isGrounded = false`**
```csharp
// No puede saltar si no está en el suelo
```

✅ **Solución:**
```csharp
// Verificar que groundCheckPoint está bien posicionado
// Verificar que groundLayer incluye Ground
```

❌ **`playerAhead` es false**
```csharp
// Player está detrás, no adelante
```

✅ **Verificar:**
```csharp
// Debug.Log para ver si playerAhead es true
Debug.Log($"playerAhead: {playerAhead}, distance: {distanceToPlayer}");
```

---

### Problema 5: "Salta continuamente"

**Síntomas:**
- Salta en cada frame
- Parece que vuela

**Causa:**

❌ **Cooldown no funciona**
```csharp
// Falta verificar Time.time >= _lastJumpTime + jumpCooldown
```

✅ **Solución:**
```csharp
if (playerAhead && Time.time >= _lastJumpTime + jumpCooldown)
{
    Jump();
}
```

---

### Problema 6: "Flip mientras está en el aire"

**Síntomas:**
- Rota mientras salta
- Sprite se voltea en el aire (antinatural)

**Causa:**

❌ **OnCollisionEnter2D ejecutándose durante salto**
```csharp
// La pared invisible choca mientras LadyBug está saltando
// y causa Flip en el aire
```

✅ **Solución (Opcional):**
```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    // Solo voltear si está en el suelo
    if (collision.gameObject.layer == LayerMask.NameToLayer("LadyBugOnly") && _isGrounded)
    {
        Flip();
    }
}
```

**Nota:** En la práctica, este problema es raro porque las paredes invisibles están en los bordes, no en el aire. Si ocurre, agregar verificación `_isGrounded`.

---

## ✅ CHECKPOINT

### Preguntas de Validación

<details>
<summary>❓ ¿Qué diferencia hay entre `detectionRange` y `_fleeRangeHysteresis`?</summary>

**Respuesta:**

**`detectionRange`** es la distancia a la que LadyBug EMPIEZA a huir.

**`_fleeRangeHysteresis`** es un multiplicador que define a qué distancia DEJA de huir.

**Sin hysteresis (flickering):**
```
Player a 5.1f → Flee OFF → Patrol
Player a 4.9f → Flee ON → Flee
Player a 5.1f → Flee OFF → Patrol (flicker constante)
```

**Con hysteresis (estable):**
```
Player a 4.9f → Flee ON → Flee (empieza a huir)
Player a 5.1f → Sigue en Flee (hysteresis)
Player a 7.5f → Flee OFF → Patrol (debe alejarse más)
```

**Valores:**
- `detectionRange = 5f` (empieza a huir)
- `_fleeRangeHysteresis = 1.5f` (debe alejarse 1.5x = 7.5f para dejar de huir)

Esto evita cambios constantes de estado (flickering).
</details>

<details>
<summary>❓ ¿Por qué necesitamos verificar `playerAhead` antes de saltar?</summary>

**Respuesta:**

Para evitar saltos innecesarios cuando el player está **detrás**.

**Sin verificación:**
```
Player detrás:
🐞 → → →    🧍
         (Saltando sin razón)
    ↗️
```

**Con verificación:**
```
Player adelante (bloqueando):
   🐞
    ↗️ Salto!
🧍

Player detrás (no saltar):
🐞 → → →    🧍
(Sigue huyendo normalmente)
```

**Implementación:**
```csharp
bool playerAhead = (toPlayer.x > 0 && _movingRight) || (toPlayer.x < 0 && !_movingRight);
```

Verifica que el vector `toPlayer` apunta en la misma dirección que el movimiento.
</details>

<details>
<summary>❓ ¿Qué pasa si `fleeSpeed` es menor que `patrolSpeed`?</summary>

**Respuesta:**

Crearía un comportamiento **antinatural** e **ineficaz**.

**Problema de game feel:**
- Patrulla: 2f (relajado, tranquilo)
- Flee: 1.5f (más lento) ← **Contradicción**

Da la impresión de que LadyBug huye "sin ganas" o "cansado", lo cual no tiene sentido para un enemigo asustado.

**Problema de gameplay:**
Si `fleeSpeed < playerMoveSpeed`, el player SIEMPRE alcanzará a LadyBug, sin importar cuánto huya. Hace que el comportamiento de huida sea inútil.

**Valores recomendados:**
- `patrolSpeed = 2f` (lento)
- `fleeSpeed = 4f` (pánico, el doble)
- `playerMoveSpeed = 5f` (referencia)

Así, LadyBug huye rápido pero aún es alcanzable con esfuerzo.
</details>

---

## 💡 MEJORAS OPCIONALES

Si terminas rápido y quieres pulir el sistema:

### Gameplay

- [ ] **Stamina System**: LadyBug se cansa de huir, debe descansar
- [ ] **Wall Detection**: Huir en dirección perpendicular si choca con pared
- [ ] **Group Behavior**: Múltiples LadyBugs huyen juntas
- [ ] **Sound Effects**: Sonido de pánico al detectar player

### Visual/Audio

- [ ] **Flee Animation**: Animación de correr asustado
- [ ] **Panic Indicator**: Icono de exclamación al detectar player
- [ ] **Dust Particles**: Partículas al huir rápido
- [ ] **Squash & Stretch**: Deformación del sprite al saltar

### Advanced

- [ ] **Predictive Flee**: Huir en dirección del movimiento del player (anticipar)
- [ ] **Hide Behavior**: Buscar obstáculos para esconderse
- [ ] **Call for Help**: Alertar a otros enemigos cercanos

---

## 🎉 COMPLETANDO LADYBUG AI

Con esta feature implementada:

```
✅ Estado Flee (huir del player)
✅ Ground check adelante (evita caer)
✅ Flip automático si encuentra borde
✅ Salto sobre player (evasión)
✅ Hysteresis para estabilidad
✅ Sistema de knockback/stun heredado
✅ Gizmos de debug
```

**Siguiente:** Spider AI (en otra rama)

---

**¡Éxito con la implementación! Recuerda: 80/20 - Intenta primero, pide ayuda si te atascas >30 min.** 🐞

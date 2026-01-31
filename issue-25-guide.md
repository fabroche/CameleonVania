# Issue #25: Wall Climb - Spider Ability - Implementation Guide

**Feature 9 - Day 4: Special Abilities**

---

## 📚 PARTE 1: TEORÍA (30-40 min)

### ¿Qué vamos a implementar?

Un **sistema de escalado** que permite a la transformación **Spider** escalar paredes y techos. El player puede:
- 🕷️ Pegarse a paredes verticales
- 🙃 Escalar en techos (boca abajo)
- 🔄 Transicionar entre pared → techo sin soltarse
- 🦘 Saltar de la pared (wall jump)
- 🔁 Rotar el modelo según la superficie

---

### 🎯 Conceptos Clave

#### 1. **Raycasting**

**¿Qué es?**
Un **rayo invisible** que detecta colisiones en una dirección.

```
Player                    Wall
  🕷️ ──────────────→    ║║║║
     Raycast Right       ║║║║
```

**Información que obtenemos:**
- ¿Golpeó algo? (`RaycastHit2D.collider != null`)
- ¿A qué distancia? (`RaycastHit2D.distance`)
- **¿Cuál es el normal de la superficie?** (`RaycastHit2D.normal`) ← CRÍTICO

---

#### 2. **Surface Normal (Vector Normal)**

**¿Qué es?**
Un **vector perpendicular** a la superficie, que apunta "hacia afuera".

**Visualización:**

```
        TECHO
    ═══════════════
         ↓ Normal (0, -1)


    ║              ║
    ║ Normal       ║ Normal
←───║ (-1, 0)      ║───→ (1, 0)
    ║              ║
 PARED          PARED
  DER.           IZQ.


         ↑ Normal (0, 1)
    ───────────────
        SUELO
```

**Tabla de Normales:**

| Superficie | Normal Vector | Descripción |
|------------|---------------|-------------|
| Pared Derecha | `(-1, 0)` | Apunta hacia la izquierda ← |
| Pared Izquierda | `(1, 0)` | Apunta hacia la derecha → |
| Techo | `(0, -1)` | Apunta hacia abajo ↓ |
| Suelo | `(0, 1)` | Apunta hacia arriba ↑ |

**¿Por qué es importante?**
El normal nos dice **qué tipo de superficie** es, sin necesidad de comparar posiciones.

---

#### 3. **Conversión Normal → Rotación**

**Problema:** Queremos que Spider se vea "pegado" a la superficie.

**Solución:** Rotar el modelo visual según el normal.

**Método: Atan2**

```csharp
// Calcular ángulo del normal
float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;

// Ajustar para orientación del sprite
float rotation = angle + 90f;
```

**¿Por qué +90°?**
Porque `Atan2` devuelve el ángulo del vector, pero el sprite por defecto mira hacia **arriba** (0° = up). Sumamos 90° para compensar.

**Tabla de Conversión:**

| Normal | Atan2 Result | +90° Ajuste | Rotación Final |
|--------|--------------|-------------|----------------|
| `(-1, 0)` Pared Der. | -180° | -180° + 90° | **-90°** |
| `(1, 0)` Pared Izq. | 0° | 0° + 90° | **90°** |
| `(0, -1)` Techo | -90° | -90° + 90° | **0°** → ajustar a **180°** |
| `(0, 1)` Suelo | 90° | 90° + 90° | **180°** → ajustar a **0°** |

**Nota:** Para techo/suelo necesitamos lógica adicional para manejar 180°/0°.

---

#### 4. **Movimiento Relativo a Superficie**

**Problema:** En el techo, `Input.GetAxis("Horizontal")` debe mover left/right, no afectar la gravedad.

**Solución:** Calcular direcciones **relativas a la superficie**.

```csharp
// Dirección "derecha" en la superficie (perpendicular al normal)
Vector2 surfaceRight = new Vector2(surfaceNormal.y, -surfaceNormal.x);

// Dirección "hacia arriba" en la superficie (opuesto al normal)
Vector2 surfaceUp = -surfaceNormal;
```

**Ejemplo:**

```
TECHO: normal = (0, -1)
- surfaceRight = (-1, 0) → Horizontal mueve left/right ✅
- surfaceUp = (0, 1) → Vertical "cae" del techo

PARED DERECHA: normal = (-1, 0)
- surfaceRight = (0, 1) → Horizontal mueve up/down (rotado 90°)
- surfaceUp = (1, 0) → Vertical mueve hacia la pared
```

**Problema:** Esto hace que los controles sean confusos (horizontal mueve vertical en paredes).

**Solución práctica:** Usar lógica específica por tipo de superficie:
- **Paredes:** Vertical = arriba/abajo, Horizontal = NO usado (o cambiar facing)
- **Techo:** Horizontal = izq/der, Vertical = despegarse

---

#### 5. **Wall Jump**

**Concepto:** Al presionar Jump mientras escalas, te **desprendes** de la pared con un impulso.

**Direcciones del impulso:**

```
PARED DERECHA:              PARED IZQUIERDA:
    ║                              ║
    ║ 🕷️ (escalando)              🕷️ ║
    ║                              ║
    ║   Jump!                  Jump! ║
    ║     ↗️ impulso         impulso ↖️  ║
    ║                              ║
```

**Vector de impulso:**
```csharp
// Dirección horizontal: opuesta a la pared
float jumpDirX = isFacingRight ? -1f : 1f;

// Vector final
Vector2 wallJumpVelocity = new Vector2(jumpDirX * wallJumpForceX, wallJumpForceY);
```

---

### 🧩 Arquitectura del Sistema

**Estructura de GameObjects:**

```
Player (GameObject)
├── Rigidbody2D (NO rotar)
├── CapsuleCollider2D (NO rotar)
├── PlayerController.cs
├── PlayerTransform.cs
└── model3DParent (Transform) ← ROTAR ESTE (ya existe)
    └── currentModel (instanciado dinámicamente)
```

**¿Por qué separar model3DParent?**
- ✅ Rotar modelo NO afecta física
- ✅ Raycasts siguen apuntando correctamente
- ✅ Collider no se deforma
- ✅ Ya está configurado en tu proyecto por PlayerTransform

---

### 📊 Comparación de Métodos de Detección

| Método | Pros | Cons | Uso |
|--------|------|------|-----|
| **Múltiples Raycasts** | ✅ Detecta todas las direcciones<br>✅ Permite transiciones fluidas<br>✅ Predecible | ❌ 4 raycasts/frame | ✅ Recomendado |
| **Raycast Direccional** | ✅ Eficiente (1 raycast)<br>✅ Menos código | ❌ No detecta superficies pasivas<br>❌ Dificulta transiciones | Para sistemas simples |
| **ContactFilter2D** | ✅ Detecta automáticamente contactos | ❌ Complejo<br>❌ Requiere ya estar tocando | Sistemas avanzados |

---

### 🎮 Estados del Sistema

```
Estado: NORMAL                Estado: CLIMBING
- Gravedad normal             - Gravedad = 0
- Movimiento horizontal       - Movimiento vertical (paredes)
- Jump normal                 - Movimiento horizontal (techo)
                              - Wall jump

Transición NORMAL → CLIMBING:
- Spider transformation ✅
- Superficie detectada ✅
- Input de movimiento hacia superficie ✅

Transición CLIMBING → NORMAL:
- Presiona Jump (wall jump)
- No hay superficie
- Cambia transformación
```

---

### ❌ Errores Comunes vs ✅ Soluciones

| ❌ Problema | ✅ Solución |
|------------|-----------|
| Rotar el Player completo → Física rota | Rotar solo model3DParent (child) |
| Raycasts apuntan en dirección incorrecta después de rotar | Usar transform.position como origin, direcciones en world space |
| Controles invertidos en techo | Lógica específica por superficie |
| Player se "pega" al suelo al caminar | Solo climbing si NO está en suelo (isGrounded = false) |
| Transiciones bruscas entre superficies | Lerp de rotación con velocidad |

---

### 🎓 Preguntas de Comprensión

<details>
<summary>❓ ¿Por qué necesitamos 4 raycasts en lugar de 1?</summary>

**Respuesta:**
Para detectar superficies en **todas las direcciones** (derecha, izquierda, arriba, abajo). Esto permite:
- Transiciones automáticas (pared → techo)
- Detectar la superficie más cercana
- Saber qué tipo de superficie es (por el normal)

Con 1 solo raycast, solo detectarías la dirección que estás mirando.
</details>

<details>
<summary>❓ ¿Qué pasa si el sprite por defecto mira hacia la derecha en lugar de arriba?</summary>

**Respuesta:**
Debes ajustar el offset en la fórmula:
- Si mira arriba (0° = up): `rotation = angle + 90°`
- Si mira derecha (0° = right): `rotation = angle + 0°` (sin ajuste)
- Si mira abajo (0° = down): `rotation = angle - 90°`

Esto depende de cómo esté orientado tu modelo 3D/sprite en el editor.
</details>

<details>
<summary>❓ ¿Por qué surfaceRight = (normal.y, -normal.x)?</summary>

**Respuesta:**
Es una **rotación de 90° del normal**. Matemáticamente:
- Rotar 90° en sentido horario: `(x, y) → (y, -x)`
- Ejemplo: normal `(0, -1)` → surfaceRight `(-1, 0)` (apunta izquierda en techo)

Esto nos da la dirección perpendicular al normal, que es "derecha" en la superficie.
</details>

---

## 🛠️ PARTE 2: IMPLEMENTACIÓN (2-3h)

### Setup Previo en Unity

#### Paso 0: Verificar Jerarquía Visual (Ya Configurada)

**BUENAS NOTICIAS:** Tu proyecto ya tiene la estructura correcta configurada por `PlayerTransform.cs`.

**Estructura actual:**

```
Player (GameObject root)
├── Rigidbody2D (física aquí)
├── CapsuleCollider2D
├── PlayerController
├── PlayerTransform
├── Health
└── model3DParent (Transform) ← ESTE es tu "Visual"
    └── currentModel (instanciado dinámicamente)
```

**¿Por qué funciona?**
- ✅ `Player` root = Física (Rigidbody2D, Collider) NO se rota
- ✅ `model3DParent` = Visual que SÍ se rotará
- ✅ Modelo instanciado hereda rotación del parent

---

**TU ÚNICA TAREA:** Asignar referencia en PlayerController.

<details>
<summary>💡 Pista: ¿Qué necesito hacer?</summary>

En `PlayerController.cs`, agregar una variable que apunte a `model3DParent`:

```csharp
[Header("Visual Feedback")]
[SerializeField] private Transform visualTransform;
```

Luego en Unity Inspector:
- Seleccionar Player
- Arrastrar `model3DParent` al campo `visualTransform`
</details>

<details>
<summary>✅ Solución: Asignación automática</summary>

**Opción A: Asignar manualmente en Inspector (Recomendado)**

1. **Agregar variable en PlayerController.cs:**
```csharp
[Header("Visual Feedback")]
[Tooltip("Transform del model3DParent para rotar el modelo")]
[SerializeField] private Transform visualTransform;
```

2. **En Unity Inspector:**
   - Selecciona **Player** en Hierarchy
   - En componente **PlayerController**
   - Arrastra **model3DParent** (child del Player) al campo **visualTransform**

---

**Opción B: Asignación automática en código**

```csharp
void Start()
{
    _rb = GetComponent<Rigidbody2D>();
    originalGravity = _rb.gravityScale;

    // Obtener referencia automática desde PlayerTransform
    PlayerTransform pt = GetComponent<PlayerTransform>();
    if (pt != null && visualTransform == null)
    {
        visualTransform = pt.model3DParent;
        Debug.Log("[PlayerController] visualTransform auto-assigned to model3DParent");
    }

    // Verificar que se asignó
    if (visualTransform == null)
    {
        Debug.LogWarning("[PlayerController] visualTransform no asignado! Rotación visual no funcionará.");
    }
}
```

**Estructura final verificada:**
```
Player (NO rota - física intacta)
└── model3DParent (SÍ rota - solo visual)
    └── modelo instanciado (hereda rotación)
```

✅ **No necesitas crear nuevos GameObjects**, tu estructura actual es perfecta.
</details>

---

### Paso 1: Variables de Climbing

**TU TURNO:** Agrega las variables necesarias en `PlayerController.cs`.

**Requisitos:**
- Variables de configuración (SerializeField)
- Variables de estado (private)
- Referencia al transform visual

<details>
<summary>💡 Pista 1: Variables de configuración</summary>

```csharp
[Header("Wall Climbing")]
[Tooltip("Velocidad de escalado en paredes")]
[SerializeField] private float climbSpeed = 3f;

[Tooltip("Distancia de raycast para detectar paredes")]
[SerializeField] private float wallCheckDistance = 0.6f;

[Tooltip("Fuerza de wall jump (x: horizontal, y: vertical)")]
[SerializeField] private Vector2 wallJumpForce = new Vector2(6f, 10f);
```
</details>

<details>
<summary>💡 Pista 2: Variables de estado</summary>

```csharp
// Estado de climbing
private bool isClimbing = false;
private Vector2 currentSurfaceNormal = Vector2.zero;
private float originalGravity;

// Referencia al transform visual (model3DParent de PlayerTransform)
[Header("Visual Feedback")]
[Tooltip("Transform del model3DParent - asignar desde Inspector o auto-assign en Start")]
[SerializeField] private Transform visualTransform;
[SerializeField] private float rotationSpeed = 10f;
```
</details>

<details>
<summary>✅ Solución Completa - Variables</summary>

```csharp
// Agregar al PlayerController.cs después de las variables de Jump:

[Header("Wall Climbing - Spider")]
[Tooltip("Velocidad de escalado en paredes/techo")]
[SerializeField] private float climbSpeed = 3f;

[Tooltip("Distancia de raycast para detectar superficies escalables")]
[SerializeField] private float wallCheckDistance = 0.6f;

[Tooltip("Fuerza de wall jump (x: alejarse de pared, y: altura)")]
[SerializeField] private Vector2 wallJumpForce = new Vector2(6f, 10f);

[Header("Visual Feedback")]
[Tooltip("Transform del model3DParent para rotar el modelo")]
[SerializeField] private Transform visualTransform;

[Tooltip("Velocidad de rotación del modelo (suavidad)")]
[SerializeField] private float rotationSpeed = 10f;

// Estado de climbing
private bool isClimbing = false;
private Vector2 currentSurfaceNormal = Vector2.zero;
private float originalGravity;
private float targetRotation = 0f; // Rotación objetivo del modelo
```

**Valores recomendados:**
- `climbSpeed = 3f` (más lento que caminar 5f)
- `wallCheckDistance = 0.6f` (un poco más que el radio del collider)
- `wallJumpForce = (6f, 10f)` (horizontal moderado, vertical alto)
- `rotationSpeed = 10f` (transición suave, no instantánea)
</details>

---

### Paso 2: Guardar Gravedad Original

**TU TURNO:** En `Start()`, guarda la gravedad original del Rigidbody2D.

**¿Por qué?**
Cuando escalas, ponemos gravedad = 0. Al salir, necesitamos restaurarla.

<details>
<summary>💡 Pista</summary>

```csharp
void Start()
{
    // ... código existente ...

    // Guardar gravedad original
    originalGravity = _rb.gravityScale;
}
```
</details>

<details>
<summary>✅ Solución</summary>

```csharp
void Start()
{
    _rb = GetComponent<Rigidbody2D>();

    // Guardar gravedad original para restaurar después de climbing
    originalGravity = _rb.gravityScale;

    // Auto-asignar visualTransform desde PlayerTransform si no está asignado
    if (visualTransform == null)
    {
        PlayerTransform pt = GetComponent<PlayerTransform>();
        if (pt != null && pt.model3DParent != null)
        {
            visualTransform = pt.model3DParent;
            Debug.Log("[PlayerController] visualTransform auto-assigned to model3DParent");
        }
    }

    // Verificar que visualTransform está asignado
    if (visualTransform == null)
    {
        Debug.LogWarning("[PlayerController] visualTransform no asignado! Rotación visual no funcionará.");
    }
}
```
</details>

---

### Paso 3: Detectar Superficies Escalables (4 Raycasts)

**TU TURNO:** Implementa el método que detecta paredes, techo, etc.

**Requisitos:**
1. Lanzar 4 raycasts: Right, Left, Up, Down
2. Retornar `true` si detecta superficie escalable
3. Guardar el `normal` de la superficie en un `out` parameter

**Pseudocódigo:**
```
CheckClimbableSurface(out normal):
    1. Raycast Right → si golpea, normal = hit.normal, return true
    2. Raycast Left → si golpea, normal = hit.normal, return true
    3. Raycast Up → si golpea, normal = hit.normal, return true
    4. (Opcional) Raycast Down → para detectar suelo como superficie
    5. Si ninguno golpea, return false
```

<details>
<summary>💡 Pista 1: Estructura básica</summary>

```csharp
private bool CheckClimbableSurface(out Vector2 surfaceNormal)
{
    surfaceNormal = Vector2.zero;

    Vector2 origin = transform.position;

    // 1. Raycast Right
    RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, groundLayer);
    if (hitRight.collider != null)
    {
        surfaceNormal = hitRight.normal;
        return true;
    }

    // TODO: Raycast Left, Up, Down

    return false;
}
```
</details>

<details>
<summary>💡 Pista 2: Raycasts completos</summary>

```csharp
private bool CheckClimbableSurface(out Vector2 surfaceNormal)
{
    surfaceNormal = Vector2.zero;
    Vector2 origin = transform.position;

    // Raycast en 4 direcciones
    RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, groundLayer);
    RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, groundLayer);
    RaycastHit2D hitUp = Physics2D.Raycast(origin, Vector2.up, wallCheckDistance, groundLayer);
    // Down opcional (para detectar si está en suelo)

    // Verificar en orden de prioridad
    if (hitRight.collider != null)
    {
        surfaceNormal = hitRight.normal;
        return true;
    }
    // ... repetir para otros

    return false;
}
```
</details>

<details>
<summary>✅ Solución Completa - CheckClimbableSurface</summary>

```csharp
/// <summary>
/// Detecta si hay una superficie escalable cerca (pared, techo).
/// Usa 4 raycasts para detectar en todas las direcciones.
/// </summary>
/// <param name="surfaceNormal">Normal de la superficie detectada</param>
/// <returns>True si hay superficie escalable</returns>
private bool CheckClimbableSurface(out Vector2 surfaceNormal)
{
    surfaceNormal = Vector2.zero;

    // Origin del raycast (centro del player)
    Vector2 origin = transform.position;

    // Raycast en 4 direcciones
    RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, groundLayer);
    RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, groundLayer);
    RaycastHit2D hitUp = Physics2D.Raycast(origin, Vector2.up, wallCheckDistance, groundLayer);
    // Raycast Down es opcional (para detectar si está en suelo y NO permitir climb)

    // Verificar cuál detectó algo (prioridad: paredes primero, luego techo)
    if (hitRight.collider != null)
    {
        surfaceNormal = hitRight.normal;

        if (debugLogs)
        {
            Debug.Log($"[PlayerController] Surface detected RIGHT. Normal: {surfaceNormal}");
        }

        return true;
    }
    else if (hitLeft.collider != null)
    {
        surfaceNormal = hitLeft.normal;

        if (debugLogs)
        {
            Debug.Log($"[PlayerController] Surface detected LEFT. Normal: {surfaceNormal}");
        }

        return true;
    }
    else if (hitUp.collider != null)
    {
        surfaceNormal = hitUp.normal;

        if (debugLogs)
        {
            Debug.Log($"[PlayerController] Surface detected UP (ceiling). Normal: {surfaceNormal}");
        }

        return true;
    }

    // No hay superficie escalable cerca
    return false;
}
```

**Explicación:**
- `Vector2 origin = transform.position` → Centro del player (donde se originan los raycasts)
- `Physics2D.Raycast(origin, direction, distance, layerMask)` → Lanza rayo
- `hitRight.normal` → Vector perpendicular a la superficie
- `groundLayer` → Solo detecta objetos en capa "Ground" (paredes, techo, suelo)

**Nota:** Orden de verificación importa. Si estás en una esquina, detectará primero la pared derecha.
</details>

---

### Paso 4: Convertir Normal a Rotación (Atan2)

**TU TURNO:** Implementa el método que convierte el normal de la superficie a un ángulo de rotación.

**Requisitos:**
1. Usar `Mathf.Atan2(normal.y, normal.x)` para calcular ángulo
2. Convertir radianes → grados (`* Mathf.Rad2Deg`)
3. Ajustar para orientación del sprite (+ offset)
4. Manejar casos especiales (techo 180°)

<details>
<summary>💡 Pista 1: Cálculo básico</summary>

```csharp
private float GetRotationFromNormal(Vector2 normal)
{
    // Calcular ángulo del normal
    float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;

    // Ajustar para que el sprite apunte correctamente
    float rotation = angle + 90f; // Ajuste depende de orientación del sprite

    return rotation;
}
```
</details>

<details>
<summary>💡 Pista 2: Manejar techo (caso especial)</summary>

```csharp
private float GetRotationFromNormal(Vector2 normal)
{
    float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
    float rotation = angle + 90f;

    // Caso especial: Techo (normal apunta hacia abajo)
    if (Mathf.Abs(normal.y + 1f) < 0.1f) // normal.y ≈ -1
    {
        rotation = 180f; // Boca abajo
    }

    return rotation;
}
```
</details>

<details>
<summary>✅ Solución Completa - GetRotationFromNormal</summary>

```csharp
/// <summary>
/// Convierte el normal de una superficie a un ángulo de rotación.
/// Usa Atan2 para calcular el ángulo del vector normal.
/// </summary>
/// <param name="normal">Vector normal de la superficie</param>
/// <returns>Ángulo de rotación en grados (Euler Z)</returns>
private float GetRotationFromNormal(Vector2 normal)
{
    // Mathf.Atan2 devuelve el ángulo del vector en radianes
    // Convertimos a grados con Mathf.Rad2Deg
    float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;

    // Ajustar para la orientación del sprite
    // Esto depende de cómo esté orientado tu modelo por defecto
    // Si el sprite mira hacia arriba (0° = up), sumamos 90°
    float rotation = angle + 90f;

    // Casos especiales para superficies ortogonales
    float tolerance = 0.1f;

    // PARED DERECHA: normal ≈ (-1, 0)
    if (Mathf.Abs(normal.x + 1f) < tolerance && Mathf.Abs(normal.y) < tolerance)
    {
        return -90f; // Rotar 90° a la derecha
    }

    // PARED IZQUIERDA: normal ≈ (1, 0)
    else if (Mathf.Abs(normal.x - 1f) < tolerance && Mathf.Abs(normal.y) < tolerance)
    {
        return 90f; // Rotar 90° a la izquierda
    }

    // TECHO: normal ≈ (0, -1)
    else if (Mathf.Abs(normal.y + 1f) < tolerance && Mathf.Abs(normal.x) < tolerance)
    {
        return 180f; // Boca abajo
    }

    // SUELO: normal ≈ (0, 1)
    else if (Mathf.Abs(normal.y - 1f) < tolerance && Mathf.Abs(normal.x) < tolerance)
    {
        return 0f; // Posición normal
    }

    // DEFAULT: Superficie diagonal (rampa, etc.)
    return rotation;
}
```

**Explicación:**
- `Atan2(y, x)` → Ángulo del vector (0° = derecha, 90° = arriba)
- `+ 90f` → Ajuste porque sprite mira arriba por defecto
- Casos especiales aseguran rotaciones exactas (90°, -90°, 180°, 0°)
- `tolerance = 0.1f` → Margen de error para comparaciones float
</details>

---

### Paso 5: Calcular Dirección de Climbing (Flag Pattern)

**IMPORTANTE:** Siguiendo el patrón de tu proyecto (Movement en Update, física en FixedUpdate), vamos a separar:
- **Update()**: Calcular dirección de movimiento
- **FixedUpdate()**: Aplicar física al Rigidbody2D

**TU TURNO:** Implementa el método que CALCULA (no aplica) la dirección de movimiento.

**Requisitos:**
1. En **paredes**: Vertical = arriba/abajo, Horizontal = NO usado
2. En **techo**: Horizontal = izq/der, Vertical = despegarse (o ignorar)
3. **NO aplicar velocidad** (solo calcular dirección)
4. Retornar Vector2 con la dirección calculada

**Pseudocódigo:**
```
CalculateClimbingDirection(surfaceNormal, verticalInput, horizontalInput):
    1. Determinar tipo de superficie (pared vs techo)
    2. Si es pared → usar Input Vertical (Vector2.up/down * speed)
    3. Si es techo → usar Input Horizontal (Vector2.left/right * speed)
    4. RETORNAR dirección (NO aplicar todavía)
```

<details>
<summary>💡 Pista 1: Detectar tipo de superficie</summary>

```csharp
private Vector2 CalculateClimbingDirection(Vector2 surfaceNormal, float verticalInput, float horizontalInput)
{
    // Determinar tipo de superficie
    bool isWall = Mathf.Abs(surfaceNormal.x) > 0.5f; // Normal horizontal → pared
    bool isCeiling = surfaceNormal.y < -0.5f; // Normal apunta abajo → techo

    Vector2 climbDirection = Vector2.zero;

    // TODO: Calcular dirección según tipo

    return climbDirection;
}
```
</details>

<details>
<summary>💡 Pista 2: Movimiento por tipo</summary>

```csharp
private Vector2 CalculateClimbingDirection(Vector2 surfaceNormal, float verticalInput, float horizontalInput)
{
    bool isWall = Mathf.Abs(surfaceNormal.x) > 0.5f;
    bool isCeiling = surfaceNormal.y < -0.5f;

    Vector2 climbDirection = Vector2.zero;

    if (isCeiling)
    {
        // TECHO: Horizontal mueve left/right
        climbDirection = new Vector2(horizontalInput * climbSpeed, 0f);
    }
    else if (isWall)
    {
        // PARED: Vertical mueve up/down
        // Mantener velocidad horizontal actual
        climbDirection = new Vector2(_rb.linearVelocity.x, verticalInput * climbSpeed);
    }

    return climbDirection; // Retornar, NO aplicar
}
```
</details>

<details>
<summary>✅ Solución Completa - CalculateClimbingDirection + ApplyClimbingPhysics</summary>

**Paso 5A: Calcular Dirección (Update)**

```csharp
/// <summary>
/// Calcula la dirección de movimiento en climbing (NO aplica física).
/// Llamado desde Update() para calcular, física se aplica en FixedUpdate().
/// </summary>
/// <param name="surfaceNormal">Normal de la superficie actual</param>
/// <param name="verticalInput">Input vertical del player</param>
/// <param name="horizontalInput">Input horizontal del player</param>
/// <returns>Vector2 con la dirección de movimiento</returns>
private Vector2 CalculateClimbingDirection(Vector2 surfaceNormal, float verticalInput, float horizontalInput)
{
    // Determinar tipo de superficie basado en el normal
    // PARED: normal.x significativo (apunta horizontal)
    bool isWall = Mathf.Abs(surfaceNormal.x) > 0.5f;

    // TECHO: normal.y apunta hacia abajo (< -0.5)
    bool isCeiling = surfaceNormal.y < -0.5f;

    Vector2 climbDirection = Vector2.zero;

    if (isCeiling)
    {
        // TECHO: Movimiento horizontal (left/right)
        climbDirection = new Vector2(horizontalInput * climbSpeed, 0f);

        if (debugLogs && verticalInput != 0f)
        {
            Debug.Log("[PlayerController] En techo - Input Vertical ignorado");
        }
    }
    else if (isWall)
    {
        // PARED: Movimiento vertical (up/down)
        // Mantener velocidad horizontal actual (no interferir con movimiento lateral)
        climbDirection = new Vector2(_rb.linearVelocity.x, verticalInput * climbSpeed);

        // Opcional: Cambiar facing direction con horizontal
        if (horizontalInput > 0.1f)
        {
            _isFacingRight = true;
        }
        else if (horizontalInput < -0.1f)
        {
            _isFacingRight = false;
        }
    }
    else
    {
        // SUELO u otra superficie: No debería estar climbing
        Debug.LogWarning("[PlayerController] Climbing en superficie no reconocida");
        climbDirection = Vector2.zero;
    }

    return climbDirection; // RETORNAR, NO aplicar
}
```

---

**Paso 5B: Aplicar Física (FixedUpdate)**

```csharp
/// <summary>
/// Aplica física de climbing (llamado desde FixedUpdate).
/// </summary>
private void ApplyClimbingPhysics()
{
    if (_shouldApplyClimbingPhysics)
    {
        // APLICAR velocidad calculada en Update()
        _rb.linearVelocity = _climbingDirection;

        if (debugLogs)
        {
            Debug.Log($"[FixedUpdate] Applying climbing velocity: {_climbingDirection}");
        }
    }
}
```

---

**Paso 5C: Modificar FixedUpdate()**

```csharp
void FixedUpdate()
{
    // Don't apply movement if stunned
    if (isStunned)
    {
        if (Time.time >= stunEndTime)
        {
            isStunned = false;
        }
        return;
    }

    // Aplicar física de climbing o movimiento normal
    if (_shouldApplyClimbingPhysics)
    {
        ApplyClimbingPhysics(); // Física de climbing
    }
    else
    {
        // Movimiento horizontal normal (solo si NO está climbing)
        _rb.linearVelocity = new Vector2(_moveInput * moveSpeed, _rb.linearVelocity.y);
    }
}
```

---

**Explicación del Flag Pattern:**

```
Update() [Frame rate variable]
  ↓
HandleClimbing()
  ↓
ActivateClimbing()
  ↓
_climbingDirection = CalculateClimbingDirection()  ← Calcular
_shouldApplyClimbingPhysics = true                 ← Flag ON

⏱️ [Tiempo fijo 50fps]

FixedUpdate() [Frame rate fijo]
  ↓
if (_shouldApplyClimbingPhysics)
  ↓
ApplyClimbingPhysics()
  ↓
_rb.linearVelocity = _climbingDirection            ← Aplicar
```

**Ventajas:**
- ✅ Input responsivo (Update)
- ✅ Física estable (FixedUpdate)
- ✅ Consistente con tu código existente
- ✅ Separación de responsabilidades
</details>

---

### Paso 6: Wall Jump

**TU TURNO:** Implementa la lógica de saltar desde la pared.

**Requisitos:**
1. Detectar Input.GetButtonDown("Jump") mientras isClimbing
2. Salir del estado climbing (isClimbing = false, restaurar gravedad)
3. Aplicar impulso horizontal (alejarse de la pared) + vertical

<details>
<summary>💡 Pista 1: Detectar wall jump</summary>

```csharp
private void WallJump()
{
    // Verificar que está climbing y presiona Jump
    if (Input.GetButtonDown("Jump") && isClimbing)
    {
        // Salir de climbing
        isClimbing = false;
        _rb.gravityScale = originalGravity;

        // TODO: Aplicar impulso
    }
}
```
</details>

<details>
<summary>💡 Pinta 2: Calcular dirección de impulso</summary>

```csharp
private void WallJump()
{
    if (Input.GetButtonDown("Jump") && isClimbing)
    {
        isClimbing = false;
        _rb.gravityScale = originalGravity;

        // Dirección horizontal: opuesta a la pared
        // Si miras derecha (pared a la derecha), saltar izquierda
        float jumpDirX = isFacingRight ? -1f : 1f;

        // Aplicar impulso
        Vector2 wallJumpVelocity = new Vector2(
            jumpDirX * wallJumpForce.x,
            wallJumpForce.y
        );

        _rb.linearVelocity = wallJumpVelocity;
    }
}
```
</details>

<details>
<summary>✅ Solución Completa - WallJump</summary>

```csharp
/// <summary>
/// Maneja el wall jump (saltar desde la pared).
/// Player se despega de la pared con impulso horizontal + vertical.
/// </summary>
private void WallJump()
{
    // Verificar que está climbing y presiona Jump
    if (Input.GetButtonDown("Jump") && isClimbing)
    {
        // Salir del estado climbing
        isClimbing = false;

        // Restaurar gravedad normal
        _rb.gravityScale = originalGravity;

        // Calcular dirección horizontal del salto
        // Si está mirando derecha (pared a la derecha), saltar hacia la izquierda
        // Si está mirando izquierda (pared a la izquierda), saltar hacia la derecha
        float jumpDirX = isFacingRight ? -1f : 1f;

        // Aplicar velocidad de wall jump
        Vector2 wallJumpVelocity = new Vector2(
            jumpDirX * wallJumpForce.x,  // Horizontal: alejarse de pared
            wallJumpForce.y               // Vertical: altura del salto
        );

        _rb.linearVelocity = wallJumpVelocity;

        if (debugLogs)
        {
            Debug.Log($"[PlayerController] Wall Jump! Direction: {jumpDirX}, Velocity: {wallJumpVelocity}");
        }
    }
}
```

**Explicación:**
- `isFacingRight ? -1f : 1f` → Si miras derecha, saltar izquierda (opuesto)
- `wallJumpForce.x` → Fuerza horizontal (alejarse de pared)
- `wallJumpForce.y` → Fuerza vertical (altura del salto)
- Restaurar gravedad antes de aplicar velocidad (para que caiga normalmente)
</details>

---

### Paso 7: Integrar Climbing en Update() (Clean Code)

**IMPORTANTE:** Para mantener el código limpio y mantenible, vamos a encapsular toda la lógica de climbing en un método separado.

**TU TURNO:** Modifica el método `Update()` e implementa `HandleClimbing()`.

**Requisitos:**
1. Verificar si tiene transformación Spider (PlayerTransform.CanWallClimb())
2. Detectar superficie escalable
3. Activar/desactivar estado climbing
4. Llamar a métodos correspondientes
5. NO permitir climbing si está en el suelo (isGrounded = true)

**Pseudocódigo:**
```
Update():
    1. Verificar PlayerTransform.CanWallClimb()
    2. Verificar CheckClimbableSurface(out normal)
    3. Verificar Input de movimiento (Vertical != 0)
    4. Verificar NO está en suelo (isGrounded = false)

    5. Si todas las condiciones → Activar climbing
       - isClimbing = true
       - gravityScale = 0
       - Llamar ClimbingMovement()

    6. Si NO → Desactivar climbing
       - isClimbing = false
       - gravityScale = original

    7. Siempre verificar WallJump()
```

<details>
<summary>💡 Pista 1: Verificar condiciones</summary>

```csharp
void Update()
{
    // ... código existente (Movement, Jump, etc.) ...

    // Wall Climbing
    PlayerTransform pt = GetComponent<PlayerTransform>();

    if (pt != null && pt.CanWallClimb())
    {
        // Verificar si hay superficie escalable
        bool hasSurface = CheckClimbableSurface(out Vector2 surfaceNormal);

        // Input de movimiento (quiere escalar)
        float verticalInput = Input.GetAxis("Vertical");
        bool wantsToClimb = Mathf.Abs(verticalInput) > 0.1f || isClimbing;

        // NO permitir climbing si está en suelo
        bool canStartClimbing = !_isGrounded;

        // TODO: Activar/desactivar climbing
    }
}
```
</details>

<details>
<summary>💡 Pista 2: Activar/Desactivar climbing</summary>

```csharp
void Update()
{
    // ... código existente ...

    PlayerTransform pt = GetComponent<PlayerTransform>();

    if (pt != null && pt.CanWallClimb())
    {
        bool hasSurface = CheckClimbableSurface(out Vector2 surfaceNormal);
        float verticalInput = Input.GetAxis("Vertical");
        bool wantsToClimb = Mathf.Abs(verticalInput) > 0.1f || isClimbing;
        bool canStartClimbing = !_isGrounded;

        if (hasSurface && wantsToClimb && canStartClimbing)
        {
            // ACTIVAR climbing
            if (!isClimbing)
            {
                isClimbing = true;
                _rb.gravityScale = 0f;
            }

            currentSurfaceNormal = surfaceNormal;
            ClimbingMovement(surfaceNormal);
        }
        else
        {
            // DESACTIVAR climbing
            if (isClimbing)
            {
                isClimbing = false;
                _rb.gravityScale = originalGravity;
            }
        }

        // Wall Jump (siempre verificar)
        WallJump();
    }
    else
    {
        // No tiene transformación Spider → No puede climbing
        if (isClimbing)
        {
            isClimbing = false;
            _rb.gravityScale = originalGravity;
        }
    }
}
```
</details>

<details>
<summary>✅ Solución Completa - Update() Limpio + HandleClimbing()</summary>

**Paso 7A: Update() simplificado**

```csharp
void Update()
{
    // Código existente (Movement, Jump, etc.)
    Movement();
    Jump();

    // === WALL CLIMBING SYSTEM ===
    HandleClimbing(); // ← Todo encapsulado aquí

    // Actualizar rotación visual (siempre, climbing o no)
    UpdateVisualRotation();
}
```

---

**Paso 7B: Método HandleClimbing() (AGREGAR)**

```csharp
/// <summary>
/// Maneja todo el sistema de wall climbing.
/// Verifica habilidad, detecta superficies, activa/desactiva climbing y wall jump.
/// </summary>
private void HandleClimbing()
{
    // Obtener componente PlayerTransform
    PlayerTransform pt = GetComponent<PlayerTransform>();

    // Verificar si tiene habilidad de wall climbing (Spider transformation)
    if (pt != null && pt.CanWallClimb())
    {
        // Detectar superficie escalable cerca
        bool hasSurface = CheckClimbableSurface(out Vector2 surfaceNormal);

        // Input de movimiento (¿quiere escalar?)
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // Si ya está climbing, continuar aunque no haya input
        // Si no está climbing, solo activar si hay input
        bool wantsToClimb = _isClimbing || Mathf.Abs(verticalInput) > 0.1f || Mathf.Abs(horizontalInput) > 0.1f;

        // NO permitir climbing si está en el suelo (evitar "pegarse" al caminar)
        bool canStartClimbing = !_isGrounded;

        // Decidir si activar climbing
        if (hasSurface && wantsToClimb && canStartClimbing)
        {
            ActivateClimbing(surfaceNormal);
        }
        else
        {
            DeactivateClimbing();
        }

        // Wall Jump (siempre verificar, incluso si ya no está climbing)
        WallJump();
    }
    else
    {
        // No tiene transformación Spider → Forzar salir de climbing
        ForceExitClimbing();
    }
}
```

---

**Paso 7C: Métodos auxiliares (AGREGAR)**

```csharp
/// <summary>
/// Activa el estado de climbing y calcula dirección de movimiento.
/// La física se aplica en FixedUpdate().
/// </summary>
private void ActivateClimbing(Vector2 surfaceNormal)
{
    // ACTIVAR CLIMBING (si no estaba ya activo)
    if (!_isClimbing)
    {
        _isClimbing = true;
        _rb.gravityScale = 0f; // Anular gravedad

        if (debugLogs)
        {
            Debug.Log("[PlayerController] Started climbing");
        }
    }

    // Guardar normal actual
    _currentSurfaceNormal = surfaceNormal;

    // Leer input
    float verticalInput = Input.GetAxis("Vertical");
    float horizontalInput = Input.GetAxis("Horizontal");

    // CALCULAR dirección (no aplicar todavía)
    _climbingDirection = CalculateClimbingDirection(surfaceNormal, verticalInput, horizontalInput);

    // Activar flag para aplicar en FixedUpdate
    _shouldApplyClimbingPhysics = true;
}

/// <summary>
/// Desactiva el estado de climbing y restaura gravedad.
/// </summary>
private void DeactivateClimbing()
{
    if (_isClimbing)
    {
        _isClimbing = false;
        _rb.gravityScale = _originalGravity; // Restaurar gravedad
        _shouldApplyClimbingPhysics = false; // Desactivar flag

        if (debugLogs)
        {
            Debug.Log("[PlayerController] Stopped climbing");
        }
    }
}

/// <summary>
/// Fuerza salida de climbing cuando se pierde la transformación Spider.
/// </summary>
private void ForceExitClimbing()
{
    if (_isClimbing)
    {
        _isClimbing = false;
        _rb.gravityScale = _originalGravity;
        _shouldApplyClimbingPhysics = false; // Desactivar flag

        if (debugLogs)
        {
            Debug.Log("[PlayerController] Lost Spider transformation - climbing disabled");
        }
    }
}
```

---

**Beneficios de esta estructura:**

✅ **Update() limpio y legible** (5 líneas de lógica)
✅ **Single Responsibility Principle** (cada método hace UNA cosa)
✅ **Fácil de debuggear** (puedes aislar problemas)
✅ **Fácil de testear** (métodos individuales)
✅ **Fácil de mantener** (cambios localizados)
✅ **Profesional y escalable**

**Explicación:**
- `HandleClimbing()` → Orquesta toda la lógica de climbing
- `ActivateClimbing()` → Encapsula activación de estado
- `DeactivateClimbing()` → Encapsula desactivación de estado
- `ForceExitClimbing()` → Maneja pérdida de transformación
- `pt.CanWallClimb()` → Verifica transformación Spider
- `!_isGrounded` → Evita "pegarse" al suelo
- `wantsToClimb` → Mantiene climbing en transiciones
</details>

---

### Paso 8: Rotación Visual del Modelo

**TU TURNO:** Implementa el método que rota el `model3DParent` según la superficie.

**Requisitos:**
1. Calcular rotación objetivo según estado (climbing o normal)
2. Aplicar rotación suavemente (Lerp)
3. Solo rotar `model3DParent`, NO el Player root

<details>
<summary>💡 Pista 1: Calcular rotación objetivo</summary>

```csharp
private void UpdateVisualRotation()
{
    if (visualTransform == null) return;

    // Determinar rotación objetivo
    if (isClimbing)
    {
        // Calcular rotación según superficie
        targetRotation = GetRotationFromNormal(currentSurfaceNormal);
    }
    else
    {
        // Normal: sin rotación
        targetRotation = 0f;
    }

    // TODO: Aplicar rotación con Lerp
}
```
</details>

<details>
<summary>💡 Pista 2: Aplicar rotación suave</summary>

```csharp
private void UpdateVisualRotation()
{
    if (visualTransform == null) return;

    if (isClimbing)
    {
        targetRotation = GetRotationFromNormal(currentSurfaceNormal);
    }
    else
    {
        targetRotation = 0f;
    }

    // Obtener rotación actual (solo eje Z)
    float currentZ = visualTransform.eulerAngles.z;

    // Normalizar ángulo (-180 a 180)
    if (currentZ > 180f) currentZ -= 360f;

    // Interpolar suavemente (Lerp)
    float newZ = Mathf.LerpAngle(currentZ, targetRotation, rotationSpeed * Time.deltaTime);

    // Aplicar rotación
    visualTransform.rotation = Quaternion.Euler(0f, 0f, newZ);
}
```
</details>

<details>
<summary>✅ Solución Completa - UpdateVisualRotation</summary>

```csharp
/// <summary>
/// Actualiza la rotación del modelo visual según el estado.
/// - Climbing: Rota según superficie (pared/techo)
/// - Normal: Rotación 0° (de pie)
/// Usa Lerp para transición suave.
/// </summary>
private void UpdateVisualRotation()
{
    // Verificar que visualTransform está asignado
    if (visualTransform == null)
    {
        return;
    }

    // Determinar rotación objetivo según estado
    if (isClimbing)
    {
        // Calcular rotación basada en el normal de la superficie
        targetRotation = GetRotationFromNormal(currentSurfaceNormal);
    }
    else
    {
        // Estado normal: sin rotación (de pie)
        targetRotation = 0f;
    }

    // Obtener rotación actual (solo eje Z, 2D)
    float currentZ = visualTransform.eulerAngles.z;

    // Normalizar ángulo a rango -180 a 180
    // (Unity devuelve 0-360, pero para Lerp es mejor -180 a 180)
    if (currentZ > 180f)
    {
        currentZ -= 360f;
    }

    // Interpolar suavemente entre rotación actual y objetivo
    // Mathf.LerpAngle maneja correctamente el wrap-around (359° → 0°)
    float newZ = Mathf.LerpAngle(currentZ, targetRotation, rotationSpeed * Time.deltaTime);

    // Aplicar rotación solo en eje Z (2D)
    visualTransform.rotation = Quaternion.Euler(0f, 0f, newZ);
}
```

**Explicación:**
- `visualTransform.eulerAngles.z` → Obtener solo rotación Z (2D)
- `if (currentZ > 180f) currentZ -= 360f` → Normalizar (Unity usa 0-360, necesitamos -180 a 180)
- `Mathf.LerpAngle()` → Interpola correctamente ángulos (maneja 359° → 0°)
- `rotationSpeed * Time.deltaTime` → Velocidad de rotación framerate-independent
- Solo rotamos en Z (2D), X e Y quedan en 0
</details>

---

### Paso 9: Debugging Visual (Gizmos)

**OPCIONAL:** Agrega visualización de raycasts en Scene view.

<details>
<summary>✅ OnDrawGizmos - Visualizar Raycasts</summary>

```csharp
#if UNITY_EDITOR
void OnDrawGizmos()
{
    // Solo dibujar si tiene transformación Spider
    PlayerTransform pt = GetComponent<PlayerTransform>();
    if (pt == null || !pt.CanWallClimb()) return;

    Vector2 origin = transform.position;

    // Color de los raycasts
    Gizmos.color = Color.cyan;

    // Raycast Right
    Gizmos.DrawLine(origin, origin + Vector2.right * wallCheckDistance);

    // Raycast Left
    Gizmos.DrawLine(origin, origin + Vector2.left * wallCheckDistance);

    // Raycast Up
    Gizmos.DrawLine(origin, origin + Vector2.up * wallCheckDistance);

    // Si está climbing, dibujar normal de la superficie
    if (isClimbing)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + currentSurfaceNormal * 2f);
    }
}
#endif
```

**Uso:** En Scene view verás líneas cyan = raycasts, línea amarilla = normal de superficie.
</details>

---

## 🧪 PARTE 3: TESTING (30-45 min)

### Setup de Test en Unity

Antes de testear, necesitas:

**1. Verificar visualTransform:**
- Selecciona Player en Hierarchy
- En Inspector, PlayerController
- Verifica que `visualTransform` apunta a `model3DParent`
- Si no está asignado, arrástralo manualmente desde Hierarchy
- O confía en la auto-asignación en Start()

**2. Crear escenario de test:**
- Paredes verticales a ambos lados
- Plataforma con techo
- Enemigo para dropear máscara Spider

**3. Valores recomendados (Inspector):**
- `climbSpeed = 3f`
- `wallCheckDistance = 0.6f`
- `wallJumpForce = (6, 10)`
- `rotationSpeed = 10f`

---

### Tests Incrementales

#### ✅ Test 1: Detectar Pared

**Objetivo:** Verificar que los raycasts detectan paredes.

**Pasos:**
1. Transformarte en Spider (recoger máscara)
2. Acercarte a una pared
3. Observar en Console: logs de "Surface detected"

**Resultado esperado:**
```
[PlayerController] Surface detected RIGHT. Normal: (-1, 0)
```

**Debugging:**
- Si no detecta: Verificar que pared tiene Layer "Ground"
- Si detecta desde muy lejos: Reducir `wallCheckDistance`

---

#### ✅ Test 2: Activar Climbing

**Objetivo:** Verificar que entras en modo climbing.

**Pasos:**
1. Estar transformado en Spider
2. Acercarte a pared (NO estar en suelo)
3. Presionar W (Vertical Up)

**Resultado esperado:**
```
[PlayerController] Started climbing
```
- Player se "pega" a la pared
- Gravedad = 0 (no cae)
- Modelo rota 90° hacia la pared

**Debugging:**
- Si no activa: Verificar `isGrounded = false` (saltar antes de pared)
- Si se cae: Verificar `gravityScale = 0f` se aplica

---

#### ✅ Test 3: Escalar Pared (Up/Down)

**Objetivo:** Verificar movimiento vertical en pared.

**Pasos:**
1. Activar climbing en pared
2. Presionar W (subir)
3. Presionar S (bajar)

**Resultado esperado:**
- W → Player sube a velocidad `climbSpeed`
- S → Player baja a velocidad `climbSpeed`
- Sin input → Player se queda quieto (no cae)

**Debugging:**
- Si no se mueve: Verificar que `ClimbingMovement()` se llama cada frame
- Si se mueve demasiado rápido/lento: Ajustar `climbSpeed`

---

#### ✅ Test 4: Escalar Techo (Left/Right)

**Objetivo:** Verificar movimiento horizontal en techo.

**Pasos:**
1. Escalar pared hasta llegar al techo
2. Continuar presionando W (transición pared → techo)
3. En techo, presionar A/D

**Resultado esperado:**
- Transición automática pared → techo (sin soltarse)
- Modelo rota a 180° (boca abajo)
- A → Player se mueve izquierda
- D → Player se mueve derecha

**Debugging:**
- Si se cae en transición: Verificar que `CheckClimbableSurface()` detecta techo (raycast UP)
- Si no se mueve horizontal: Verificar condición `isCeiling` en `ClimbingMovement()`

---

#### ✅ Test 5: Wall Jump

**Objetivo:** Verificar salto desde pared.

**Pasos:**
1. Estar escalando una pared
2. Presionar Space (Jump)

**Resultado esperado:**
```
[PlayerController] Wall Jump! Direction: -1, Velocity: (-6, 10)
```
- Player se despega de la pared
- Impulso horizontal (alejarse) + vertical (altura)
- Gravedad restaurada (cae normalmente)
- Rotación vuelve a 0° (de pie)

**Debugging:**
- Si no salta: Verificar que `WallJump()` se llama en Update
- Si salta en dirección incorrecta: Verificar cálculo `jumpDirX`
- Si no cae después: Verificar `gravityScale = originalGravity`

---

#### ✅ Test 6: Rotación del Modelo

**Objetivo:** Verificar que el modelo rota correctamente.

**Pasos:**
1. Escalar pared derecha → Verificar rotación -90°
2. Saltar a pared izquierda → Verificar rotación 90°
3. Subir a techo → Verificar rotación 180°
4. Caer al suelo → Verificar rotación 0°

**Resultado esperado:**
- Rotaciones suaves (Lerp), no instantáneas
- Modelo siempre "pegado" a la superficie
- Física NO afectada por rotación (solo visual)

**Debugging:**
- Si no rota: Verificar que `visualTransform` apunta a `model3DParent`
- Si rota el Player completo: Verificar que estás rotando `visualTransform`, NO `transform`
- Si rotación es brusca: Aumentar `rotationSpeed`
- Para verificar: `Debug.Log($"Rotating: {visualTransform.name}");` debe mostrar "model3DParent"

---

#### ✅ Test 7: Perder Transformación

**Objetivo:** Verificar que al cambiar forma, se desactiva climbing.

**Pasos:**
1. Estar escalando como Spider
2. Recoger otra máscara (Frog o Ladybug)

**Resultado esperado:**
```
[PlayerController] Lost Spider transformation - climbing disabled
```
- Climbing desactivado inmediatamente
- Gravedad restaurada
- Player cae normalmente

**Debugging:**
- Si sigue escalando: Verificar condición `pt.CanWallClimb()` en Update
- Si se queda flotando: Verificar restauración de gravedad

---

### Tests de Edge Cases

#### ⚠️ Test 8: Esquinas (Pared → Techo)

**Setup:** Esquina interior (L invertida)

```
═══════ TECHO
       ║
       ║ PARED
```

**Testear:**
1. Escalar pared hacia esquina
2. Continuar presionando W al llegar a esquina

**Comportamiento esperado:**
- Transición suave pared → techo
- Sin soltarse
- Rotación cambia de -90° a 180°

---

#### ⚠️ Test 9: Esquinas Exteriores

**Setup:** Esquina exterior

```
PARED ║
      ║
      ═══════ PLATAFORMA
```

**Testear:**
1. Escalar pared hacia arriba
2. Llegar al borde superior (sin techo)

**Comportamiento esperado:**
- Climbing se desactiva (no hay superficie UP)
- Player salta/cae normalmente
- Puede subir a la plataforma

---

#### ⚠️ Test 10: Suelo + Pared (No pegarse al caminar)

**Setup:** Caminar cerca de una pared

**Testear:**
1. Caminar hacia una pared (sin saltar)
2. Estar en suelo (isGrounded = true)

**Comportamiento esperado:**
- NO activar climbing (condición `!isGrounded`)
- Player camina normalmente, choca con pared
- No se "pega" accidentalmente

---

## 🐛 DEBUGGING

### Problema 1: "No detecta paredes"

**Síntomas:**
- Raycasts no retornan colisiones
- Nunca entra en modo climbing

**Causas posibles:**

❌ **Pared no tiene Layer "Ground"**
```csharp
// Solución: Seleccionar pared en Hierarchy
// → Inspector → Layer → Ground
```

❌ **wallCheckDistance muy corto**
```csharp
// En Inspector, aumentar wallCheckDistance
wallCheckDistance = 0.8f; // Probar con 0.8 en lugar de 0.6
```

❌ **groundLayer no incluye capa Ground**
```csharp
// En PlayerController Inspector
// → Ground Layer → Marcar "Ground"
```

**Debug:**
```csharp
// En CheckClimbableSurface(), agregar:
Debug.DrawRay(origin, Vector2.right * wallCheckDistance, Color.red, 0.1f);

// Ver raycasts en Scene view (líneas rojas)
```

---

### Problema 2: "Se activa climbing en el suelo"

**Síntomas:**
- Player se "pega" a paredes al caminar
- Rotación extraña al caminar cerca de paredes

**Causa:**
```csharp
❌ Falta condición !isGrounded
```

**Solución:**
```csharp
✅ En Update(), verificar:
bool canStartClimbing = !_isGrounded;

if (hasSurface && wantsToClimb && canStartClimbing) {
    // ...
}
```

---

### Problema 3: "Rotación rota todo el Player"

**Síntomas:**
- Raycasts dejan de funcionar
- Collider se deforma
- Física extraña

**Causa:**
```csharp
❌ Estás rotando transform en lugar de visualTransform
❌ visualTransform apunta al Player root en lugar de model3DParent
```

**Solución:**
```csharp
✅ En UpdateVisualRotation():
visualTransform.rotation = Quaternion.Euler(0f, 0f, newZ);
// NO: transform.rotation

// Verificar en Inspector:
// visualTransform debe apuntar a "model3DParent" (child)

// O verificar en código:
Debug.Log($"Visual transform: {visualTransform.name}"); // Debe ser "model3DParent"
Debug.Log($"Is child: {visualTransform.parent == transform}"); // Debe ser true
```

---

### Problema 4: "Wall Jump salta hacia la pared"

**Síntomas:**
- Salta pero no se aleja de la pared
- O salta en dirección opuesta a la esperada

**Causa:**
```csharp
❌ Dirección de jumpDirX incorrecta
```

**Solución:**
```csharp
✅ Verificar lógica:
float jumpDirX = isFacingRight ? -1f : 1f;
// Si miras derecha (pared a la derecha), saltar izquierda (-1)
// Si miras izquierda (pared a la izquierda), saltar derecha (1)

// Debug:
Debug.Log($"Facing right: {isFacingRight}, JumpDir: {jumpDirX}");
```

---

### Problema 5: "No se mueve en techo"

**Síntomas:**
- Entra en modo climbing en techo
- Modelo rota a 180° (correcto)
- Pero no se mueve con A/D

**Causa:**
```csharp
❌ Condición isCeiling incorrecta
```

**Solución:**
```csharp
✅ En ClimbingMovement(), verificar:
bool isCeiling = surfaceNormal.y < -0.5f;
// Normal del techo apunta hacia abajo: (0, -1)

// Debug:
Debug.Log($"Surface normal: {surfaceNormal}, IsCeiling: {isCeiling}");
```

---

### Problema 6: "No transiciona de pared a techo"

**Síntomas:**
- Se cae al llegar a la esquina pared-techo
- No detecta el techo

**Causa:**
```csharp
❌ Raycast UP no detecta techo (muy corto o bloqueado)
```

**Solución:**
```csharp
✅ Aumentar wallCheckDistance:
wallCheckDistance = 0.8f;

✅ Verificar que techo tiene collider y Layer "Ground"

// Debug con Gizmos:
void OnDrawGizmos() {
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(transform.position, transform.position + Vector3.up * wallCheckDistance);
}
```

---

### Problema 7: "Gravedad no se restaura"

**Síntomas:**
- Al salir de climbing, sigue flotando
- No cae normalmente

**Causa:**
```csharp
❌ No restaura gravityScale al desactivar climbing
```

**Solución:**
```csharp
✅ En todas las salidas de climbing:

if (isClimbing) {
    isClimbing = false;
    _rb.gravityScale = originalGravity; // ← CRÍTICO
}

// Verificar que originalGravity se guardó en Start()
```

---

### Herramientas de Debug

**1. Console Logs:**
```csharp
if (debugLogs) {
    Debug.Log($"Climbing: {isClimbing}, Surface: {currentSurfaceNormal}, Rotation: {targetRotation}");
}
```

**2. Gizmos en Scene View:**
```csharp
void OnDrawGizmos() {
    // Raycasts
    // Normal de superficie
    // Dirección de wall jump
}
```

**3. Inspector Watch:**
- PlayerController → isClimbing (ver en runtime)
- Rigidbody2D → Gravity Scale (debe ser 0 cuando climbing)
- Visual Transform → Rotation Z (ver rotación en tiempo real)

---

## ✅ CHECKPOINT

### Preguntas de Validación

<details>
<summary>❓ ¿Por qué usamos 4 raycasts en lugar de solo detectar la pared mirando?</summary>

**Respuesta:**
Para permitir **transiciones fluidas** entre superficies (pared → techo) sin soltarse. Si solo detectáramos la dirección que miramos, al llegar a una esquina, perderíamos contacto con la superficie. Los 4 raycasts detectan:
- Right/Left: Paredes a los lados
- Up: Techo arriba (para transiciones)
- Down (opcional): Evitar activar en suelo

Esto permite que al escalar una pared y llegar al techo, el raycast UP detecte el techo antes de perder la pared, manteniendo el estado climbing.
</details>

<details>
<summary>❓ ¿Qué hace exactamente `Mathf.Atan2(normal.y, normal.x)`?</summary>

**Respuesta:**
Calcula el **ángulo del vector normal** en radianes, respecto al eje X.

Matemáticamente:
- `Atan2(y, x)` → ángulo en radianes del vector (x, y)
- Rango: -π a π (-180° a 180°)

Ejemplos:
- `Atan2(0, 1)` = 0° (vector apunta derecha →)
- `Atan2(1, 0)` = 90° (vector apunta arriba ↑)
- `Atan2(0, -1)` = 180° (vector apunta izquierda ←)
- `Atan2(-1, 0)` = -90° (vector apunta abajo ↓)

Luego sumamos 90° para ajustar la orientación del sprite (depende de cómo esté orientado por defecto).
</details>

<details>
<summary>❓ ¿Por qué separamos model3DParent en lugar de rotar el Player completo?</summary>

**Respuesta:**
Porque rotar el GameObject **Player** rotaría también:
- ❌ **Rigidbody2D** → Física se comporta raro (gravedad rota)
- ❌ **Collider** → Se deforma/rota, colisiones incorrectas
- ❌ **Raycasts** → Direcciones `Vector2.right`, `Vector2.up` rotarían, detección falla

Al rotar solo **model3DParent**:
- ✅ Física sin afectar (Rigidbody2D en parent)
- ✅ Collider sin deformar
- ✅ Raycasts apuntan correctamente (world space)
- ✅ Solo el feedback visual cambia
- ✅ Ya está configurado en tu proyecto por PlayerTransform

Arquitectura actual:
```
Player (0° siempre) ← Física aquí
└── model3DParent (rota) ← Solo visual (ya existe)
    └── currentModel (hereda rotación)
```
</details>

<details>
<summary>❓ ¿Cómo funciona la condición `!isGrounded` para evitar climbing en el suelo?</summary>

**Respuesta:**
Evita que el player se "pegue" accidentalmente a paredes al **caminar** cerca de ellas.

**Escenario sin `!isGrounded`:**
```
Player camina → Toca pared → Raycast detecta pared → Climbing activado ❌
```

**Con `!isGrounded`:**
```
Player camina → Toca pared → isGrounded = true → NO permite climbing ✅
Player salta → Toca pared → isGrounded = false → Permite climbing ✅
```

Esto hace que solo puedas escalar si estás en el aire (saltaste o caíste), no si estás caminando en el suelo.
</details>

<details>
<summary>❓ ¿Por qué el wall jump usa `jumpDirX = isFacingRight ? -1f : 1f`?</summary>

**Respuesta:**
Para saltar **en dirección opuesta** a la pared (alejarse).

Lógica:
- Si `isFacingRight = true` → Player mira derecha → Pared está a la derecha → Saltar **izquierda** (-1)
- Si `isFacingRight = false` → Player mira izquierda → Pared está a la izquierda → Saltar **derecha** (1)

Visualización:
```
PARED DERECHA:           PARED IZQUIERDA:
    ║                          ║
    ║ 🕷️→ (facing right)     ←🕷️ ║ (facing left)
    ║                          ║
    ║  ← saltar (-1)      saltar → ║ (1)
```

El operador ternario invierte la dirección para alejarse de la pared.
</details>

---

## 💡 MEJORAS OPCIONALES (Polish)

Si terminas rápido y quieres mejorar el sistema:

### Gameplay Enhancements

- [ ] **Wall Slide (deslizarse en pared)**
  - Si NO presionas input, deslizarse lentamente hacia abajo
  - Velocidad reducida: `rb.linearVelocity.y = -wallSlideSpeed`

- [ ] **Corner Detection**
  - Detectar esquinas y permitir "asomarse"
  - Raycast desde offset para detectar espacio

- [ ] **Climb Stamina**
  - Sistema de stamina para climbing (tiempo limitado)
  - Regenera en el suelo

### Visual/Audio Feedback

- [ ] **Climbing Particles**
  - Pequeñas partículas al escalar (polvo, chispas)

- [ ] **Climbing Sound**
  - Sonido de garras/patas escalando

- [ ] **Wall Jump Trail**
  - Trail effect al hacer wall jump

### Advanced Features

- [ ] **Diagonal Surfaces (Ramps)**
  - Gracias a Atan2, ya funciona con rampas
  - Testear en superficies 45°

- [ ] **Multi-Player Support**
  - Verificación `if (player != gameObject)` en eventos

- [ ] **Ceiling Drop (teclear abajo en techo)**
  - En techo, presionar S para soltarse voluntariamente

**Nota:** Agregar al `polish-backlog.md` después de completar la feature.

---

## 🎉 COMPLETANDO FEATURE 9

Con esta feature implementada:

```
✅ Spider puede escalar paredes
✅ Spider puede escalar techo (boca abajo)
✅ Transiciones fluidas pared ↔ techo
✅ Wall jump funcional
✅ Rotación visual correcta
✅ Física condicional (gravedad on/off)
✅ Gating mechanic: áreas solo accesibles con Spider
```

**Siguiente:** Issue #26 - Small Gaps (Ladybug)

---

**¡Éxito con la implementación! Recuerda: 80/20 - Intenta primero, pide ayuda si te atascas >30 min.** 🕷️

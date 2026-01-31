# Issue #24: Water Zones & Swimming System - Implementation Guide

> **Objetivo:** Implementar zonas de agua mortales para player base, pero navegables para transformación Frog.

**Tiempo estimado:** 2-3 horas
**Dificultad:** ⭐⭐⭐⭐ (Alta - Física condicional y gating)

---

## 📚 PARTE 1: TEORÍA (30 min)

### ¿Qué es el Water Zone System?

**Water Zones** son áreas de trigger que modifican la física del jugador según su transformación actual. Es un **gating mechanic** - zonas que solo pueden explorarse con la transformación correcta.

### Mecánica Core

```
┌─────────────────────────────────────────────────────┐
│              WATER ZONE BEHAVIOR                    │
└─────────────────────────────────────────────────────┘

Player SIN Frog toca agua:
    ↓
Muerte instantánea (999 damage/sec)
    ↓
Respawn (si existe sistema)

Player CON Frog toca agua:
    ↓
Gravedad reducida (0.5x)
    ↓
Puede nadar libremente
    ↓
Sale del agua → Gravedad normal
```

### Diseño de Gating Natural

```
Level Design:
===========

[Inicio] → Platform 1 → [AGUA] → Platform 2 → [Objetivo]
             ↓            ↑
         Sin Frog      Con Frog
         = Bloqueado   = Puede pasar

Esto fuerza al player a:
1. Encontrar y matar Frog enemy
2. Recoger máscara Frog
3. Volver al agua
4. Cruzar a nueva área
```

---

### Conceptos Clave

#### **1. OnTriggerStay2D vs OnTriggerEnter2D**

```csharp
// OnTriggerEnter2D - Se llama UNA VEZ al entrar
void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log("Entró al trigger");
    // Útil para: Pickups, eventos únicos
}

// OnTriggerStay2D - Se llama CADA FRAME mientras está dentro
void OnTriggerStay2D(Collider2D other)
{
    Debug.Log("Está dentro del trigger");
    // Útil para: Damage zones, áreas con efecto continuo
}

// OnTriggerExit2D - Se llama UNA VEZ al salir
void OnTriggerExit2D(Collider2D other)
{
    Debug.Log("Salió del trigger");
    // Útil para: Cleanup, restaurar estado
}
```

**Para Water Zones:** Necesitamos los 3.

```
OnTriggerEnter2D → Log inicial, setup
OnTriggerStay2D  → Aplicar daño continuo O modificar gravedad
OnTriggerExit2D  → Restaurar gravedad normal
```

---

#### **2. Modificación de Gravedad en Runtime**

```csharp
// ❌ MAL - No guardar gravedad original
void OnTriggerEnter2D(Collider2D other)
{
    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    rb.gravityScale = 0.5f; // Reducir gravedad
    // ⚠️ Problema: ¿Cuál era la gravedad original?
}

// ✅ BIEN - Cachear gravedad original
private float originalGravity;

void OnTriggerEnter2D(Collider2D other)
{
    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    originalGravity = rb.gravityScale; // Guardar original
    rb.gravityScale = 0.5f;
}

void OnTriggerExit2D(Collider2D other)
{
    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    rb.gravityScale = originalGravity; // Restaurar
}
```

**¿Por qué cachear?**
- Diferentes transformaciones pueden tener gravedad diferente
- No asumimos que siempre es 3f
- Más flexible y robusto

---

#### **3. Damage Over Time (DOT)**

```csharp
// Damage continuo en OnTriggerStay2D
void OnTriggerStay2D(Collider2D other)
{
    Health health = other.GetComponent<Health>();
    if (health != null)
    {
        // damagePerSecond = 999f (muerte casi instantánea)
        // Time.deltaTime = tiempo desde último frame (~0.016s)
        float damageThisFrame = damagePerSecond * Time.deltaTime;

        health.TakeDamage(damageThisFrame, Vector2.zero);
    }
}
```

**Cálculo:**
```
damagePerSecond = 999f
Time.deltaTime ≈ 0.016s (60 FPS)
damageThisFrame = 999 * 0.016 ≈ 16 damage por frame

Con Health = 100:
100 / 16 ≈ 6 frames para morir
6 frames / 60 FPS ≈ 0.1 segundos = muerte casi instantánea ✅
```

---

#### **4. Conditional Gameplay - PlayerTransform.CanSwim()**

```csharp
// Flujo de decisión
void OnTriggerStay2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return; // No es el player

    if (pt.CanSwim()) // Transformación Frog
    {
        // RUTA A: Puede nadar
        ApplySwimPhysics(other);
    }
    else
    {
        // RUTA B: Muere
        ApplyWaterDamage(other);
    }
}
```

**Tabla de decisión:**

| Transformación | CanSwim() | Resultado en Agua |
|----------------|-----------|-------------------|
| **Base Form** | false | ❌ Muerte instantánea |
| **Frog** | true | ✅ Puede nadar |
| **Spider** | false | ❌ Muerte instantánea |
| **Ladybug** | false | ❌ Muerte instantánea |

---

### Arquitectura de WaterZone

```
WaterZone.cs
├── [Variables]
│   ├── float damagePerSecond = 999f       ← Muerte instantánea
│   ├── float swimGravityScale = 0.5f      ← Gravedad en agua
│   └── Dictionary<int, float> originalGravities ← Cache por player
│
├── [Lifecycle]
│   ├── OnTriggerEnter2D()
│   │   └─ Log, guardar gravedad original
│   │
│   ├── OnTriggerStay2D()
│   │   ├─ Verificar si es Player
│   │   ├─ CanSwim()?
│   │   │   ├─ YES → Aplicar física de natación
│   │   │   └─ NO  → Aplicar daño mortal
│   │   └─ Ejecuta cada frame
│   │
│   └── OnTriggerExit2D()
│       └─ Restaurar gravedad original
│
└── [Helpers]
    ├── ApplySwimPhysics(Collider2D)
    └── ApplyWaterDamage(Collider2D)
```

---

### 🎓 Recursos de Aprendizaje

**OBLIGATORIO - Lee ANTES de implementar:**

1. **Unity Docs:**
   - [OnTriggerStay2D](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnTriggerStay2D.html)
   - [Rigidbody2D.gravityScale](https://docs.unity3d.com/ScriptReference/Rigidbody2D-gravityScale.html)
   - [Time.deltaTime](https://docs.unity3d.com/ScriptReference/Time-deltaTime.html)

2. **Conceptos:**
   - Damage over time (DOT)
   - Gating mechanics en Metroidvanias
   - State modification en runtime

---

## 🛠️ PARTE 2: IMPLEMENTACIÓN (90-120 min)

### Paso 1: Crear el Script Base

**TU TURNO:** Crea el archivo `WaterZone.cs`.

**Ubicación:** `Assets/Scripts/Environment/WaterZone.cs`

<details>
<summary>💡 Pista 1: Estructura básica</summary>

```csharp
using UnityEngine;
using System.Collections.Generic;

public class WaterZone : MonoBehaviour
{
    // Variables aquí

    void OnTriggerEnter2D(Collider2D other)
    {
        // Setup inicial
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Lógica principal (daño o natación)
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Cleanup
    }
}
```
</details>

---

### Paso 2: Definir Variables

**TU TURNO:** Piensa qué variables necesitas.

**Requisitos:**
1. Damage por segundo (mortal)
2. Gravedad mientras nada
3. Cache de gravedad original por jugador

<details>
<summary>💡 Pista 1: Variables de configuración</summary>

```csharp
[Header("Water Settings")]
[Tooltip("Daño por segundo para player sin transformación Frog (999 = muerte instantánea)")]
[SerializeField] private float damagePerSecond = 999f;

[Tooltip("Gravedad mientras nada (0.5 = mitad de gravedad normal)")]
[SerializeField] private float swimGravityScale = 0.5f;
```
</details>

<details>
<summary>💡 Pista 2: Cache de gravedad</summary>

```csharp
[Header("Runtime Data")]
// Dictionary para guardar gravedad original de cada objeto
// Key: instanceID del GameObject, Value: gravityScale original
private Dictionary<int, float> originalGravities = new Dictionary<int, float>();
```

**¿Por qué Dictionary?**
- Puede haber múltiples players (multijugador futuro)
- Cada objeto puede tener gravedad diferente
- Evitamos conflictos
</details>

<details>
<summary>✅ Solución Completa - Variables</summary>

```csharp
[Header("Water Settings")]
[Tooltip("Daño por segundo para player sin transformación Frog (999 = muerte instantánea)")]
[SerializeField] private float damagePerSecond = 999f;

[Tooltip("Gravedad mientras nada (0.5 = mitad de gravedad normal)")]
[SerializeField] private float swimGravityScale = 0.5f;

[Header("Debug")]
[Tooltip("Mostrar logs de entrada/salida")]
[SerializeField] private bool debugLogs = false;

[Header("Runtime Data")]
// Dictionary para guardar gravedad original de cada objeto que entra
private Dictionary<int, float> originalGravities = new Dictionary<int, float>();
```

**Explicación:**
- `damagePerSecond = 999f`: Muerte casi instantánea
- `swimGravityScale = 0.5f`: Gravedad reducida (flotar)
- `originalGravities`: Cache para restaurar después
- `debugLogs`: Para debugging durante desarrollo
</details>

---

### Paso 3: Implementar OnTriggerEnter2D - Setup

**TU TURNO:** Implementa la lógica inicial cuando el player entra.

**Requisitos:**
1. Verificar que es el player
2. Guardar gravedad original en Dictionary

<details>
<summary>💡 Pista 1: Detectar player</summary>

```csharp
void OnTriggerEnter2D(Collider2D other)
{
    // Verificar que tiene PlayerTransform (es el player)
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return; // No es el player

    // Lógica de setup...
}
```
</details>

<details>
<summary>💡 Pista 2: Guardar gravedad original</summary>

```csharp
// Obtener Rigidbody2D
Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
if (rb == null) return;

// Guardar gravedad original en Dictionary
int instanceID = other.gameObject.GetInstanceID();
if (!originalGravities.ContainsKey(instanceID))
{
    originalGravities[instanceID] = rb.gravityScale;
}
```

**GetInstanceID():** ID único del GameObject, perfecto para key del Dictionary.
</details>

<details>
<summary>✅ Solución Completa - OnTriggerEnter2D</summary>

```csharp
void OnTriggerEnter2D(Collider2D other)
{
    // Verificar que es el player
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    // Obtener Rigidbody2D
    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    if (rb == null)
    {
        Debug.LogWarning("[WaterZone] Player doesn't have Rigidbody2D!");
        return;
    }

    // Guardar gravedad original
    int instanceID = other.gameObject.GetInstanceID();
    if (!originalGravities.ContainsKey(instanceID))
    {
        originalGravities[instanceID] = rb.gravityScale;

        if (debugLogs)
        {
            Debug.Log($"[WaterZone] {other.name} entered water. Original gravity: {rb.gravityScale}");
        }
    }
}
```

**Explicación:**
- Verificamos que es player (tiene PlayerTransform)
- Cacheamos gravedad original en Dictionary
- Solo guardamos una vez por objeto (ContainsKey check)
</details>

---

### Paso 4: Implementar OnTriggerStay2D - Lógica Principal

**TU TURNO:** Implementa la lógica que se ejecuta cada frame.

**Requisitos:**
1. Verificar si puede nadar (CanSwim)
2. SI puede → Aplicar física de natación
3. NO puede → Aplicar daño mortal

**Pseudocódigo:**
```
OnTriggerStay2D(other):
    1. Verificar que es Player
    2. Obtener PlayerTransform
    3. if PlayerTransform.CanSwim():
         → Aplicar swimGravityScale
       else:
         → Aplicar damage
```

<details>
<summary>💡 Pista 1: Estructura de decisión</summary>

```csharp
void OnTriggerStay2D(Collider2D other)
{
    // Verificar que es player
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    // Decisión: ¿Puede nadar?
    if (pt.CanSwim())
    {
        // RUTA A: Aplicar física de natación
        ApplySwimPhysics(other);
    }
    else
    {
        // RUTA B: Aplicar daño
        ApplyWaterDamage(other);
    }
}
```
</details>

<details>
<summary>💡 Pista 2: Método ApplySwimPhysics</summary>

```csharp
private void ApplySwimPhysics(Collider2D playerCollider)
{
    Rigidbody2D rb = playerCollider.GetComponent<Rigidbody2D>();
    if (rb == null) return;

    // Reducir gravedad para simular flotación
    rb.gravityScale = swimGravityScale;
}
```
</details>

<details>
<summary>💡 Pista 3: Método ApplyWaterDamage</summary>

```csharp
private void ApplyWaterDamage(Collider2D playerCollider)
{
    Health health = playerCollider.GetComponent<Health>();
    if (health == null) return;

    // Calcular daño de este frame
    float damageThisFrame = damagePerSecond * Time.deltaTime;

    // Aplicar daño (sin knockback en agua)
    health.TakeDamage(damageThisFrame, Vector2.zero);
}
```

**Time.deltaTime:** Convierte "por segundo" a "por frame".
</details>

<details>
<summary>✅ Solución Completa - OnTriggerStay2D</summary>

```csharp
void OnTriggerStay2D(Collider2D other)
{
    // Verificar que es el player
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    // Decisión basada en transformación
    if (pt.CanSwim())
    {
        // Player tiene transformación Frog - puede nadar
        ApplySwimPhysics(other);
    }
    else
    {
        // Player sin Frog - daño mortal
        ApplyWaterDamage(other);
    }
}

private void ApplySwimPhysics(Collider2D playerCollider)
{
    Rigidbody2D rb = playerCollider.GetComponent<Rigidbody2D>();
    if (rb == null) return;

    // Reducir gravedad para simular flotación
    rb.gravityScale = swimGravityScale;

    if (debugLogs)
    {
        Debug.Log($"[WaterZone] {playerCollider.name} swimming (gravity: {swimGravityScale})");
    }
}

private void ApplyWaterDamage(Collider2D playerCollider)
{
    Health health = playerCollider.GetComponent<Health>();
    if (health == null) return;

    // Calcular daño de este frame (damage per second * deltaTime)
    float damageThisFrame = damagePerSecond * Time.deltaTime;

    // Aplicar daño sin knockback (agua no empuja)
    health.TakeDamage(damageThisFrame, Vector2.zero, 0f);

    if (debugLogs)
    {
        Debug.Log($"[WaterZone] {playerCollider.name} taking water damage: {damageThisFrame:F2}");
    }
}
```

**Explicación:**
- `OnTriggerStay2D` se ejecuta cada frame mientras player está en agua
- Decisión dinámica basada en `CanSwim()`
- Métodos separados para swim/damage = código más limpio
</details>

---

### Paso 5: Implementar OnTriggerExit2D - Restaurar

**TU TURNO:** Implementa el cleanup cuando el player sale del agua.

**Requisitos:**
1. Verificar que es el player
2. Restaurar gravedad original desde Dictionary
3. Limpiar entrada del Dictionary

<details>
<summary>💡 Pista 1: Obtener gravedad del Dictionary</summary>

```csharp
void OnTriggerExit2D(Collider2D other)
{
    // Verificar que es player
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    // Obtener instanceID
    int instanceID = other.gameObject.GetInstanceID();

    // Verificar si tenemos gravedad guardada
    if (originalGravities.ContainsKey(instanceID))
    {
        // Restaurar gravedad...
    }
}
```
</details>

<details>
<summary>💡 Pista 2: Restaurar y limpiar</summary>

```csharp
if (originalGravities.ContainsKey(instanceID))
{
    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        // Restaurar gravedad original
        rb.gravityScale = originalGravities[instanceID];
    }

    // Limpiar del Dictionary
    originalGravities.Remove(instanceID);
}
```
</details>

<details>
<summary>✅ Solución Completa - OnTriggerExit2D</summary>

```csharp
void OnTriggerExit2D(Collider2D other)
{
    // Verificar que es el player
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    // Obtener instanceID
    int instanceID = other.gameObject.GetInstanceID();

    // Restaurar gravedad original si existe
    if (originalGravities.ContainsKey(instanceID))
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float restoredGravity = originalGravities[instanceID];
            rb.gravityScale = restoredGravity;

            if (debugLogs)
            {
                Debug.Log($"[WaterZone] {other.name} exited water. Restored gravity: {restoredGravity}");
            }
        }

        // Limpiar del Dictionary
        originalGravities.Remove(instanceID);
    }
}
```

**Explicación:**
- Restauramos gravedad exacta que tenía antes de entrar
- Limpiamos Dictionary para evitar memory leaks
- Log de debugging opcional
</details>

---

### Paso 6: Gizmos para Visualización

**TU TURNO:** Agrega visualización del área de agua en Scene view.

<details>
<summary>💡 Pista: OnDrawGizmos</summary>

```csharp
void OnDrawGizmos()
{
    // Obtener BoxCollider2D
    BoxCollider2D col = GetComponent<BoxCollider2D>();
    if (col == null) return;

    // Dibujar área de agua
    Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f); // Azul semi-transparente
    Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);
}
```
</details>

<details>
<summary>✅ Solución Completa - Gizmos</summary>

```csharp
void OnDrawGizmos()
{
    // Visualizar zona de agua en Scene view
    BoxCollider2D col = GetComponent<BoxCollider2D>();
    if (col != null)
    {
        // Color azul semi-transparente
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.offset, col.size);

        // Borde azul más oscuro
        Gizmos.color = new Color(0f, 0.3f, 0.8f, 0.8f);
        Gizmos.DrawWireCube(col.offset, col.size);
    }
}
```

**Uso:** En Scene view verás rectángulo azul = zona de agua.
</details>

---

### Paso 7: Integración con PlayerController - Swimming Mechanics

**IMPORTANTE:** Para que nadar funcione correctamente, necesitamos modificar `PlayerController.cs`.

#### **Problema a Resolver:**

```
❌ Problema 1: Player se sale del agua con un salto
   - jumpForce (10f) + gravedad baja (0.5f) = Sale volando

❌ Problema 2: No puede saltar en agua
   - Jump requiere isGrounded = true
   - En agua está flotando (isGrounded = false)
```

#### **Solución: Swimming State**

**TU TURNO:** Modifica PlayerController para soportar natación.

<details>
<summary>💡 Pista 1: Agregar variables de natación</summary>

```csharp
// En PlayerController.cs - Después de las variables existentes

[Header("Swimming")]
[Tooltip("Si está actualmente en agua")]
private bool isInWater = false;

[Tooltip("Fuerza de salto/natación en agua (más débil que salto normal)")]
[SerializeField] private float swimJumpForce = 4f;

[Tooltip("Cooldown entre saltos en agua")]
[SerializeField] private float swimJumpCooldown = 0.3f;
private float lastSwimJumpTime = -999f;
```
</details>

<details>
<summary>💡 Pista 2: Método público para WaterZone</summary>

```csharp
/// <summary>
/// Llamado por WaterZone para notificar estado de agua
/// </summary>
public void SetInWater(bool inWater)
{
    isInWater = inWater;
    Debug.Log($"[PlayerController] In water: {inWater}");
}
```
</details>

<details>
<summary>💡 Pista 3: Modificar Jump() con lógica dual</summary>

```csharp
private void Jump()
{
    // CASO 1: Salto normal en tierra
    if (Input.GetButtonDown("Jump") && _isGrounded && !isInWater)
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
    }

    // CASO 2: Natación en agua (sin ground check)
    else if (Input.GetButtonDown("Jump") && isInWater)
    {
        if (Time.time >= lastSwimJumpTime + swimJumpCooldown)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, swimJumpForce);
            lastSwimJumpTime = Time.time;
        }
    }
}
```
</details>

<details>
<summary>✅ Solución Completa - PlayerController Swimming</summary>

**Modificaciones a PlayerController.cs:**

```csharp
// AGREGAR después de las variables de Jump:

[Header("Swimming")]
[Tooltip("Si está actualmente en agua")]
private bool isInWater = false;

[Tooltip("Fuerza de salto/natación en agua (más débil que salto normal)")]
[SerializeField] private float swimJumpForce = 4f; // 40% del jumpForce

[Tooltip("Cooldown entre saltos en agua (para evitar spam)")]
[SerializeField] private float swimJumpCooldown = 0.3f;
private float lastSwimJumpTime = -999f;

// AGREGAR método público:

/// <summary>
/// Llamado por WaterZone para notificar si está en agua.
/// Permite nadar (saltar sin ground check) con física reducida.
/// </summary>
public void SetInWater(bool inWater)
{
    isInWater = inWater;

    if (debugLogs) // Si tienes debug logs
    {
        Debug.Log($"[PlayerController] In water: {inWater}");
    }
}

// REEMPLAZAR método Jump() existente:

private void Jump()
{
    // CASO 1: Salto normal en tierra
    if (Input.GetButtonDown("Jump") && _isGrounded && !isInWater)
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        Debug.Log("[PlayerController] Jump!");
    }

    // CASO 2: Natación en agua (sin ground check, fuerza reducida)
    else if (Input.GetButtonDown("Jump") && isInWater)
    {
        // Cooldown para evitar spam infinito
        if (Time.time >= lastSwimJumpTime + swimJumpCooldown)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, swimJumpForce);
            lastSwimJumpTime = Time.time;
            Debug.Log("[PlayerController] Swim!");
        }
    }
}
```

**Valores recomendados:**
- `jumpForce = 10f` (sin cambios)
- `swimJumpForce = 4f` (40% del normal)
- `swimJumpCooldown = 0.3f` (3 saltos/segundo máx)
</details>

---

### Paso 8: Modificar WaterZone - Notificar Swimming State

**TU TURNO:** Actualiza WaterZone para notificar a PlayerController.

<details>
<summary>💡 Pista: Llamar SetInWater en triggers</summary>

```csharp
// En OnTriggerEnter2D - Después de guardar gravedad:
PlayerController pc = other.GetComponent<PlayerController>();
if (pc != null && pt.CanSwim())
{
    pc.SetInWater(true);
}

// En OnTriggerStay2D - Dentro de ApplySwimPhysics:
PlayerController pc = playerCollider.GetComponent<PlayerController>();
if (pc != null)
{
    pc.SetInWater(true);
}

// En OnTriggerExit2D - Después de restaurar gravedad:
PlayerController pc = other.GetComponent<PlayerController>();
if (pc != null)
{
    pc.SetInWater(false);
}
```
</details>

<details>
<summary>✅ Solución Completa - WaterZone con Swimming</summary>

**Modificar los métodos de WaterZone.cs:**

```csharp
void OnTriggerEnter2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    if (rb == null)
    {
        Debug.LogWarning("[WaterZone] Player doesn't have Rigidbody2D!");
        return;
    }

    // Guardar gravedad original
    int instanceID = other.gameObject.GetInstanceID();
    if (!originalGravities.ContainsKey(instanceID))
    {
        originalGravities[instanceID] = rb.gravityScale;
    }

    // NUEVO: Notificar estado de natación
    PlayerController pc = other.GetComponent<PlayerController>();
    if (pc != null && pt.CanSwim())
    {
        pc.SetInWater(true);

        if (debugLogs)
        {
            Debug.Log($"[WaterZone] {other.name} can swim - enabled swimming mode");
        }
    }
}

void OnTriggerStay2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    if (pt.CanSwim())
    {
        ApplySwimPhysics(other);

        // NUEVO: Asegurar estado de natación (en caso de cambio de forma)
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetInWater(true);
        }
    }
    else
    {
        ApplyWaterDamage(other);

        // NUEVO: NO está nadando (está muriendo)
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetInWater(false);
        }
    }
}

void OnTriggerExit2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    int instanceID = other.gameObject.GetInstanceID();

    if (originalGravities.ContainsKey(instanceID))
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float restoredGravity = originalGravities[instanceID];
            rb.gravityScale = restoredGravity;
        }

        originalGravities.Remove(instanceID);
    }

    // NUEVO: Desactivar modo natación
    PlayerController pc = other.GetComponent<PlayerController>();
    if (pc != null)
    {
        pc.SetInWater(false);

        if (debugLogs)
        {
            Debug.Log($"[WaterZone] {other.name} exited water - disabled swimming mode");
        }
    }
}
```
</details>

---

## 🎯 ALTERNATIVA: Implementación con Eventos (Event-Driven Architecture)

### ¿Por qué usar Eventos?

La solución anterior (Pasos 7-8) funciona perfectamente, pero **acopla** WaterZone con PlayerController:

```csharp
❌ Acoplamiento directo:
WaterZone → llama a → PlayerController.SetInWater()
```

**Con eventos**, desacoplamos los sistemas:

```csharp
✅ Desacoplamiento con eventos:
WaterZone → dispara evento → [cualquiera puede escuchar]
PlayerController → se suscribe → reacciona al evento
```

### Comparación de Enfoques

| Aspecto | Enfoque Directo (Pasos 7-8) | Enfoque con Eventos |
|---------|----------------------------|---------------------|
| **Simplicidad** | ✅ Más simple, menos código | ❌ Requiere sistema de eventos |
| **Acoplamiento** | ❌ WaterZone conoce PlayerController | ✅ Sistemas completamente independientes |
| **Escalabilidad** | ❌ Difícil agregar más listeners | ✅ Cualquier sistema puede suscribirse |
| **Debugging** | ✅ Fácil de seguir el flujo | ❌ Más difícil (eventos ocultos) |
| **Arquitectura** | ❌ Menos profesional | ✅ Patrón profesional (Observer Pattern) |

### ¿Cuándo usar cada uno?

- **Enfoque Directo**: Proyectos pequeños, prototipos rápidos, game jams
- **Enfoque con Eventos**: Proyectos grandes, sistemas reutilizables, arquitectura escalable

---

### Paso 8-ALT: Crear Sistema de Eventos - WaterEvents.cs

**TU TURNO:** Crea un sistema de eventos estáticos para agua.

**Ubicación:** `Assets/Scripts/Environment/WaterEvents.cs`

<details>
<summary>💡 Pista 1: Estructura básica</summary>

```csharp
using UnityEngine;
using System;

/// <summary>
/// Sistema de eventos estáticos para WaterZone.
/// Permite desacoplar WaterZone de PlayerController.
/// </summary>
public static class WaterEvents
{
    // Eventos: delegates que sistemas pueden suscribirse
    public static event Action<GameObject, bool> OnWaterStateChanged;

    // Métodos para disparar eventos
    public static void TriggerWaterStateChanged(GameObject player, bool canSwim)
    {
        OnWaterStateChanged?.Invoke(player, canSwim);
    }
}
```
</details>

<details>
<summary>💡 Pista 2: Eventos Enter/Exit separados</summary>

```csharp
using UnityEngine;
using System;

public static class WaterEvents
{
    // Evento cuando player entra al agua
    public static event Action<GameObject, bool> OnPlayerEnterWater;

    // Evento cuando player sale del agua
    public static event Action<GameObject> OnPlayerExitWater;

    // Métodos para disparar eventos
    public static void PlayerEnterWater(GameObject player, bool canSwim)
    {
        OnPlayerEnterWater?.Invoke(player, canSwim);
    }

    public static void PlayerExitWater(GameObject player)
    {
        OnPlayerExitWater?.Invoke(player);
    }
}
```
</details>

<details>
<summary>✅ Solución Completa - WaterEvents.cs</summary>

```csharp
using UnityEngine;
using System;

/// <summary>
/// Sistema de eventos estáticos para zonas de agua.
/// Implementa el patrón Observer para desacoplar WaterZone de PlayerController.
///
/// Ventajas:
/// - WaterZone NO necesita conocer PlayerController
/// - Cualquier sistema puede suscribirse a eventos de agua
/// - Fácil agregar nuevos listeners (UI, audio, partículas, etc.)
/// </summary>
public static class WaterEvents
{
    // Evento: Player entra al agua
    // Parámetros: (GameObject player, bool canSwim)
    public static event Action<GameObject, bool> OnPlayerEnterWater;

    // Evento: Player sale del agua
    // Parámetros: (GameObject player)
    public static event Action<GameObject> OnPlayerExitWater;

    // Evento: Estado de natación cambia (por cambio de transformación)
    // Parámetros: (GameObject player, bool canSwim)
    public static event Action<GameObject, bool> OnSwimStateChanged;

    /// <summary>
    /// Disparar evento de entrada al agua.
    /// Llamado por WaterZone en OnTriggerEnter2D.
    /// </summary>
    public static void PlayerEnterWater(GameObject player, bool canSwim)
    {
        OnPlayerEnterWater?.Invoke(player, canSwim);
    }

    /// <summary>
    /// Disparar evento de salida del agua.
    /// Llamado por WaterZone en OnTriggerExit2D.
    /// </summary>
    public static void PlayerExitWater(GameObject player)
    {
        OnPlayerExitWater?.Invoke(player);
    }

    /// <summary>
    /// Disparar evento de cambio de estado de natación.
    /// Llamado por WaterZone en OnTriggerStay2D cuando detecta cambio de forma.
    /// </summary>
    public static void SwimStateChanged(GameObject player, bool canSwim)
    {
        OnSwimStateChanged?.Invoke(player, canSwim);
    }
}
```

**Explicación:**
- `Action<GameObject, bool>` = delegate con 2 parámetros (player, canSwim)
- `?.Invoke()` = Null-conditional operator (solo llama si hay suscriptores)
- `static` = No requiere instancia, acceso global
</details>

---

### Paso 9-ALT: Modificar WaterZone - Disparar Eventos

**TU TURNO:** Modifica WaterZone para disparar eventos en lugar de llamar directamente a PlayerController.

<details>
<summary>💡 Pista: Reemplazar SetInWater() con eventos</summary>

```csharp
// EN OnTriggerEnter2D - REEMPLAZAR:
// PlayerController pc = other.GetComponent<PlayerController>();
// if (pc != null && pt.CanSwim())
// {
//     pc.SetInWater(true);
// }

// POR:
WaterEvents.PlayerEnterWater(other.gameObject, pt.CanSwim());

// EN OnTriggerExit2D - REEMPLAZAR:
// PlayerController pc = other.GetComponent<PlayerController>();
// if (pc != null)
// {
//     pc.SetInWater(false);
// }

// POR:
WaterEvents.PlayerExitWater(other.gameObject);
```
</details>

<details>
<summary>✅ Solución Completa - WaterZone con Eventos</summary>

```csharp
void OnTriggerEnter2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    if (rb == null)
    {
        Debug.LogWarning("[WaterZone] Player doesn't have Rigidbody2D!");
        return;
    }

    // Guardar gravedad original
    int instanceID = other.gameObject.GetInstanceID();
    if (!originalGravities.ContainsKey(instanceID))
    {
        originalGravities[instanceID] = rb.gravityScale;
    }

    // NUEVO: Disparar evento (en lugar de llamar SetInWater directamente)
    WaterEvents.PlayerEnterWater(other.gameObject, pt.CanSwim());

    if (debugLogs)
    {
        Debug.Log($"[WaterZone] {other.name} entered water. CanSwim: {pt.CanSwim()}");
    }
}

void OnTriggerStay2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    if (pt.CanSwim())
    {
        ApplySwimPhysics(other);

        // NUEVO: Disparar evento de estado (por si cambia transformación en runtime)
        WaterEvents.SwimStateChanged(other.gameObject, true);
    }
    else
    {
        ApplyWaterDamage(other);

        // NUEVO: Disparar evento de estado (no puede nadar = está muriendo)
        WaterEvents.SwimStateChanged(other.gameObject, false);
    }
}

void OnTriggerExit2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return;

    int instanceID = other.gameObject.GetInstanceID();

    if (originalGravities.ContainsKey(instanceID))
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float restoredGravity = originalGravities[instanceID];
            rb.gravityScale = restoredGravity;
        }

        originalGravities.Remove(instanceID);
    }

    // NUEVO: Disparar evento de salida
    WaterEvents.PlayerExitWater(other.gameObject);

    if (debugLogs)
    {
        Debug.Log($"[WaterZone] {other.name} exited water");
    }
}
```

**Nota:** WaterZone ya NO conoce ni llama a PlayerController. Solo dispara eventos.
</details>

---

### Paso 10-ALT: Modificar PlayerController - Suscribirse a Eventos

**TU TURNO:** Modifica PlayerController para escuchar eventos de WaterEvents.

<details>
<summary>💡 Pista 1: Suscribirse en OnEnable/OnDisable</summary>

```csharp
void OnEnable()
{
    // Suscribirse a eventos
    WaterEvents.OnPlayerEnterWater += HandleEnterWater;
    WaterEvents.OnPlayerExitWater += HandleExitWater;
    WaterEvents.OnSwimStateChanged += HandleSwimStateChange;
}

void OnDisable()
{
    // CRÍTICO: Desuscribirse para evitar memory leaks
    WaterEvents.OnPlayerEnterWater -= HandleEnterWater;
    WaterEvents.OnPlayerExitWater -= HandleExitWater;
    WaterEvents.OnSwimStateChanged -= HandleSwimStateChange;
}
```
</details>

<details>
<summary>💡 Pista 2: Implementar handlers</summary>

```csharp
private void HandleEnterWater(GameObject player, bool canSwim)
{
    // Verificar que el evento es para ESTE player
    if (player != gameObject) return;

    isInWater = canSwim; // Solo activar natación si puede nadar
}

private void HandleExitWater(GameObject player)
{
    if (player != gameObject) return;

    isInWater = false;
}

private void HandleSwimStateChange(GameObject player, bool canSwim)
{
    if (player != gameObject) return;

    isInWater = canSwim;
}
```
</details>

<details>
<summary>✅ Solución Completa - PlayerController con Eventos</summary>

**Agregar a PlayerController.cs:**

```csharp
// MANTENER las variables de swimming (igual que Paso 7):

[Header("Swimming")]
private bool isInWater = false;
[SerializeField] private float swimJumpForce = 4f;
[SerializeField] private float swimJumpCooldown = 0.3f;
private float lastSwimJumpTime = -999f;

// AGREGAR suscripción a eventos:

void OnEnable()
{
    // Suscribirse a eventos de agua
    WaterEvents.OnPlayerEnterWater += HandleEnterWater;
    WaterEvents.OnPlayerExitWater += HandleExitWater;
    WaterEvents.OnSwimStateChanged += HandleSwimStateChange;
}

void OnDisable()
{
    // CRÍTICO: Desuscribirse para evitar memory leaks y errores
    WaterEvents.OnPlayerEnterWater -= HandleEnterWater;
    WaterEvents.OnPlayerExitWater -= HandleExitWater;
    WaterEvents.OnSwimStateChanged -= HandleSwimStateChange;
}

// AGREGAR handlers de eventos:

/// <summary>
/// Handler: Player entra al agua.
/// Activar natación solo si tiene transformación correcta.
/// </summary>
private void HandleEnterWater(GameObject player, bool canSwim)
{
    // Verificar que el evento es para ESTE player
    // (importante si hay múltiples players en escena)
    if (player != gameObject) return;

    isInWater = canSwim;

    if (debugLogs)
    {
        Debug.Log($"[PlayerController] Entered water. CanSwim: {canSwim}");
    }
}

/// <summary>
/// Handler: Player sale del agua.
/// Desactivar modo natación.
/// </summary>
private void HandleExitWater(GameObject player)
{
    if (player != gameObject) return;

    isInWater = false;

    if (debugLogs)
    {
        Debug.Log($"[PlayerController] Exited water");
    }
}

/// <summary>
/// Handler: Estado de natación cambia (por cambio de transformación en runtime).
/// </summary>
private void HandleSwimStateChange(GameObject player, bool canSwim)
{
    if (player != gameObject) return;

    isInWater = canSwim;

    if (debugLogs)
    {
        Debug.Log($"[PlayerController] Swim state changed. CanSwim: {canSwim}");
    }
}

// MANTENER el método Jump() modificado (igual que Paso 7):

private void Jump()
{
    // CASO 1: Salto normal en tierra
    if (Input.GetButtonDown("Jump") && _isGrounded && !isInWater)
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        Debug.Log("[PlayerController] Jump!");
    }

    // CASO 2: Natación en agua
    else if (Input.GetButtonDown("Jump") && isInWater)
    {
        if (Time.time >= lastSwimJumpTime + swimJumpCooldown)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, swimJumpForce);
            lastSwimJumpTime = Time.time;
            Debug.Log("[PlayerController] Swim!");
        }
    }
}
```

**IMPORTANTE:**
- `OnEnable/OnDisable` en lugar de `Awake/OnDestroy` (mejor para pooling)
- Siempre verificar `if (player != gameObject)` en handlers
- Siempre desuscribirse en `OnDisable` (evitar memory leaks)
</details>

---

### Comparación Final: ¿Cuál implementar?

#### **Enfoque Directo (Pasos 7-8)** ✅ Recomendado para este proyecto

```csharp
// WaterZone conoce PlayerController
PlayerController pc = other.GetComponent<PlayerController>();
pc.SetInWater(true);
```

**Pros:**
- ✅ Más simple (menos archivos)
- ✅ Más fácil de debuggear
- ✅ Perfecto para game jam (rápido)

**Cons:**
- ❌ Acoplamiento directo
- ❌ Menos escalable

---

#### **Enfoque con Eventos (Pasos 8-10 ALT)** 🎓 Aprendizaje avanzado

```csharp
// WaterZone NO conoce PlayerController
WaterEvents.PlayerEnterWater(other.gameObject, canSwim);
```

**Pros:**
- ✅ Desacoplamiento total
- ✅ Fácil agregar más listeners (UI, audio, VFX)
- ✅ Arquitectura profesional (Observer Pattern)

**Cons:**
- ❌ Más complejo (requiere WaterEvents.cs)
- ❌ Más difícil de debuggear (eventos ocultos)

---

### Recomendación Final

Para **este proyecto (Game Jam)**:
- **Implementa el enfoque directo (Pasos 7-8)**
- Es más simple y suficiente para el alcance del proyecto

Para **proyectos grandes/profesionales**:
- **Usa el enfoque con eventos (Pasos ALT)**
- Arquitectura más limpia y escalable

---

### Paso 9: Script Completo

<details>
<summary>📄 Código Completo - WaterZone.cs (Con Swimming)</summary>

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Zona de agua que mata al player sin transformación Frog,
/// pero permite nadar si tiene la transformación correcta.
/// Requiere: BoxCollider2D con isTrigger = true
/// </summary>
public class WaterZone : MonoBehaviour
{
    [Header("Water Settings")]
    [Tooltip("Daño por segundo para player sin transformación Frog (999 = muerte instantánea)")]
    [SerializeField] private float damagePerSecond = 999f;

    [Tooltip("Gravedad mientras nada (0.5 = mitad de gravedad normal)")]
    [SerializeField] private float swimGravityScale = 0.5f;

    [Header("Debug")]
    [Tooltip("Mostrar logs de entrada/salida")]
    [SerializeField] private bool debugLogs = false;

    [Header("Runtime Data")]
    // Dictionary para guardar gravedad original de cada objeto que entra
    private Dictionary<int, float> originalGravities = new Dictionary<int, float>();

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar que es el player
        PlayerTransform pt = other.GetComponent<PlayerTransform>();
        if (pt == null) return;

        // Obtener Rigidbody2D
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("[WaterZone] Player doesn't have Rigidbody2D!");
            return;
        }

        // Guardar gravedad original
        int instanceID = other.gameObject.GetInstanceID();
        if (!originalGravities.ContainsKey(instanceID))
        {
            originalGravities[instanceID] = rb.gravityScale;

            if (debugLogs)
            {
                Debug.Log($"[WaterZone] {other.name} entered water. Original gravity: {rb.gravityScale}");
            }
        }

        // Notificar estado de natación a PlayerController
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && pt.CanSwim())
        {
            pc.SetInWater(true);

            if (debugLogs)
            {
                Debug.Log($"[WaterZone] {other.name} can swim - enabled swimming mode");
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Verificar que es el player
        PlayerTransform pt = other.GetComponent<PlayerTransform>();
        if (pt == null) return;

        // Decisión basada en transformación
        if (pt.CanSwim())
        {
            // Player tiene transformación Frog - puede nadar
            ApplySwimPhysics(other);

            // Asegurar estado de natación (en caso de cambio de forma en agua)
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetInWater(true);
            }
        }
        else
        {
            // Player sin Frog - daño mortal
            ApplyWaterDamage(other);

            // NO está nadando (está muriendo)
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetInWater(false);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Verificar que es el player
        PlayerTransform pt = other.GetComponent<PlayerTransform>();
        if (pt == null) return;

        // Obtener instanceID
        int instanceID = other.gameObject.GetInstanceID();

        // Restaurar gravedad original si existe
        if (originalGravities.ContainsKey(instanceID))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float restoredGravity = originalGravities[instanceID];
                rb.gravityScale = restoredGravity;

                if (debugLogs)
                {
                    Debug.Log($"[WaterZone] {other.name} exited water. Restored gravity: {restoredGravity}");
                }
            }

            // Limpiar del Dictionary
            originalGravities.Remove(instanceID);
        }

        // Desactivar modo natación
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetInWater(false);

            if (debugLogs)
            {
                Debug.Log($"[WaterZone] {other.name} exited water - disabled swimming mode");
            }
        }
    }

    private void ApplySwimPhysics(Collider2D playerCollider)
    {
        Rigidbody2D rb = playerCollider.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Reducir gravedad para simular flotación
        rb.gravityScale = swimGravityScale;
    }

    private void ApplyWaterDamage(Collider2D playerCollider)
    {
        Health health = playerCollider.GetComponent<Health>();
        if (health == null) return;

        // Calcular daño de este frame (damage per second * deltaTime)
        float damageThisFrame = damagePerSecond * Time.deltaTime;

        // Aplicar daño sin knockback (agua no empuja)
        health.TakeDamage(damageThisFrame, Vector2.zero, 0f);
    }

    void OnDrawGizmos()
    {
        // Visualizar zona de agua en Scene view
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            // Color azul semi-transparente
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);

            // Borde azul más oscuro
            Gizmos.color = new Color(0f, 0.3f, 0.8f, 0.8f);
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
}
```
</details>

---

## 🧪 PARTE 3: SETUP EN UNITY Y TESTING (45 min)

### Test 1: Crear WaterZone en Escena

**Pasos en Unity:**

1. **Crear GameObject:**
   - Hierarchy → Create Empty → "WaterZone"

2. **Agregar Componentes:**
   - Add Component → **BoxCollider2D**
     - ✅ Activar "Is Trigger"
     - Size: (10, 3) (ejemplo)

   - Add Component → **WaterZone** (tu script)
     - Damage Per Second: 999
     - Swim Gravity Scale: 0.5
     - Debug Logs: ✅ (para testing)

3. **Configurar Layer:**
   - Layer: Water (Layer 9)

4. **Posicionar:**
   - Colocar en área del nivel donde quieres agua
   - Ajustar tamaño según necesites

5. **Visual (Opcional):**
   - Add Component → SpriteRenderer
   - Color: Azul semi-transparente
   - Sorting Layer: Background

---

### Test 2: Verificar Collision Matrix

**Importante:** Player debe poder detectar triggers en Water layer.

1. Edit → Project Settings → Physics 2D
2. Verificar matriz:
   - **Player (Layer 6) ↔ Water (Layer 9): ✅ Activado**

---

### Test 3: Prueba SIN Transformación Frog

**Setup:**
1. Player en forma base (sin transformaciones)
2. WaterZone en escena
3. Play mode

**Acción:**
- Caminar hacia el agua

**Resultado esperado:**
- Console: "entered water. Original gravity: 3"
- Health baja rápidamente
- Player muere en ~0.1 segundos
- Console: "taking water damage"

---

### Test 4: Prueba CON Transformación Frog

**Setup:**
1. Transformar player a Frog (tecla 1 si usas TestTransformations)
2. Caminar hacia el agua

**Resultado esperado:**
- Console: "entered water"
- Console: "swimming (gravity: 0.5)"
- Player flota en agua
- NO recibe daño
- Puede moverse libremente

---

### Test 5: Salir del Agua

**Con Frog en agua:**
- Caminar fuera del agua

**Resultado esperado:**
- Console: "exited water. Restored gravity: 3"
- Gravedad vuelve a normal
- Player cae normalmente

---

### Test 6: Cambiar Transformación en Agua

**Escenario complejo:**

1. Entrar al agua como Frog (puede nadar)
2. Recoger máscara de Spider (cambia transformación)
3. ¿Qué pasa?

**Resultado esperado:**
- Al cambiar a Spider (no puede nadar)
- Empezará a recibir daño inmediatamente
- OnTriggerStay detecta que CanSwim() ahora es false

---

## 🐛 DEBUGGING

### Error 1: "Player atraviesa agua sin detectar"

**Checklist:**
```
[ ] WaterZone tiene BoxCollider2D con isTrigger = true
[ ] Player tiene Rigidbody2D
[ ] Player tiene Collider2D
[ ] Collision Matrix: Player ↔ Water activado
[ ] WaterZone está en Layer "Water"
[ ] Player está en Layer "Player"
```

**Test de diagnóstico:**
```csharp
void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"[WaterZone] Trigger detected: {other.name}");
    // Si no ves NADA → problema de configuración
}
```

---

### Error 2: "Player no muere en agua (sin Frog)"

**Causas posibles:**
1. damagePerSecond muy bajo
2. Health.TakeDamage no funciona
3. PlayerTransform.CanSwim() retorna true incorrectamente

**Debugging:**
```csharp
void ApplyWaterDamage(Collider2D playerCollider)
{
    Debug.Log($"Damage this frame: {damagePerSecond * Time.deltaTime}");

    PlayerTransform pt = playerCollider.GetComponent<PlayerTransform>();
    Debug.Log($"CanSwim: {pt.CanSwim()}");

    Health health = playerCollider.GetComponent<Health>();
    Debug.Log($"Current health: {health.GetCurrentHealth()}");
}
```

---

### Error 3: "Gravedad no se restaura al salir"

**Causa:** No se está guardando en Dictionary o no se limpia.

**Verificar:**
```csharp
void OnTriggerExit2D(Collider2D other)
{
    int instanceID = other.gameObject.GetInstanceID();
    Debug.Log($"InstanceID: {instanceID}");
    Debug.Log($"Exists in Dictionary: {originalGravities.ContainsKey(instanceID)}");

    if (originalGravities.ContainsKey(instanceID))
    {
        Debug.Log($"Original gravity was: {originalGravities[instanceID]}");
    }
}
```

---

### Error 4: "Player puede nadar sin Frog"

**Causa:** CanSwim() retorna true incorrectamente.

**Verificar en PlayerTransform.cs:**
```csharp
public bool CanSwim()
{
    Debug.Log($"Current transformation: {_currentTransformation?.transformName ?? "null"}");
    Debug.Log($"CanSwim value: {_currentTransformation?.canSwim ?? false}");
    return _currentTransformation?.canSwim ?? false;
}
```

**Verificar en Transformation_Frog asset:**
- canSwim debe estar ✅ activado

---

## ✅ CHECKPOINT FINAL

Antes de marcar como completado:

### Funcionalidad
- [ ] WaterZone.cs compila sin errores
- [ ] Player sin Frog muere en agua
- [ ] Player con Frog puede nadar
- [ ] Gravedad se reduce en agua (Frog)
- [ ] Gravedad se restaura al salir
- [ ] Cambiar transformación en agua funciona

### Setup Unity
- [ ] WaterZone GameObject creado
- [ ] BoxCollider2D con isTrigger = true
- [ ] Script WaterZone configurado
- [ ] Layer = Water (Layer 9)
- [ ] Collision Matrix configurada
- [ ] Gizmos muestran área azul en Scene view

### Testing
- [ ] Probado muerte sin Frog
- [ ] Probado natación con Frog
- [ ] Probado salir del agua (gravedad restaura)
- [ ] Probado cambiar forma en agua
- [ ] Console logs muestran eventos correctos

### Code Quality
- [ ] Dictionary limpia correctamente
- [ ] No hay memory leaks
- [ ] Gravedad se restaura siempre
- [ ] Null checks en todos lados

---

## 🎓 PREGUNTAS DE APRENDIZAJE

<details>
<summary>❓ ¿Por qué usar OnTriggerStay2D en vez de OnTriggerEnter2D para el daño?</summary>

**Respuesta:**

```csharp
// ❌ Con OnTriggerEnter2D - Daño una sola vez
void OnTriggerEnter2D(Collider2D other)
{
    health.TakeDamage(999f); // Se ejecuta UNA VEZ al entrar
}
// Problema: Si player sobrevive (health > 999), no recibe más daño

// ✅ Con OnTriggerStay2D - Daño continuo
void OnTriggerStay2D(Collider2D other)
{
    health.TakeDamage(999f * Time.deltaTime); // Cada frame
}
// Correcto: Damage over time hasta morir o salir
```

**OnTriggerStay2D:** Perfecto para áreas que causan daño continuo.
</details>

<details>
<summary>❓ ¿Por qué usar Dictionary en vez de una variable simple para gravedad?</summary>

**Respuesta:**

**Problema con variable simple:**
```csharp
private float originalGravity; // ❌ Solo un valor

void OnTriggerEnter2D(Collider2D other)
{
    originalGravity = rb.gravityScale;
}
// ⚠️ Si entran 2 objetos, se sobrescribe
```

**Solución con Dictionary:**
```csharp
private Dictionary<int, float> originalGravities; // ✅ Múltiples valores

void OnTriggerEnter2D(Collider2D other)
{
    int id = other.gameObject.GetInstanceID();
    originalGravities[id] = rb.gravityScale;
}
// ✅ Cada objeto tiene su propia entrada
```

**Beneficios:**
- Soporta múltiples objetos simultáneamente
- Más robusto y escalable
- Previene bugs sutiles
</details>

<details>
<summary>❓ ¿Qué es Time.deltaTime y por qué multiplicar el daño por él?</summary>

**Respuesta:**

**Time.deltaTime:** Tiempo en segundos desde el último frame (~0.016s a 60 FPS).

**Sin Time.deltaTime:**
```csharp
health.TakeDamage(999f); // Cada frame
// 60 FPS = 999 damage * 60 = 59,940 damage/segundo ❌
```

**Con Time.deltaTime:**
```csharp
health.TakeDamage(999f * Time.deltaTime); // Por segundo
// 60 FPS = 999 * 0.016 * 60 = 999 damage/segundo ✅
```

**Uso:** Convierte valores "por segundo" a "por frame" = frame-rate independent.
</details>

---

## 🚀 PRÓXIMOS PASOS

Una vez completado Issue #24:

### 1. Testing Completo del Gating

**Escenario de exploración:**
```
1. Player llega a zona con agua
2. NO tiene Frog → No puede cruzar (muerte)
3. Explorar para encontrar Frog enemy
4. Matar enemy → Obtener máscara
5. Regresar al agua → Ahora puede cruzar
6. Acceder a nueva área
```

### 2. Level Design

- Crear áreas bloqueadas por agua
- Diseñar rutas alternativas
- Colocar Frog enemies estratégicamente

### 3. Visual Polish (Opcional)

- Sprite de agua animado
- Particle effects (burbujas, salpicaduras)
- Sonidos de agua

### 4. Commit y Push

```bash
git add Assets/Scripts/Environment/WaterZone.cs
git add Assets/Scenes/  # Cambios de escena
git commit -m "feat: Add WaterZone system with swim mechanics

- Player dies instantly in water without Frog transformation
- Frog transformation enables swimming (reduced gravity)
- Gravity restored on exit
- Dictionary-based gravity caching prevents conflicts
- Gating mechanic for level progression

Closes #24

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
git push origin feature/water-zone-swimming
```

### 5. Crear Pull Request

```bash
gh pr create --title "feat: WaterZone & Swimming System" --body "Closes #24"
```

---

## 💡 MEJORAS OPCIONALES (Polish)

Si terminas rápido:

### Visual Feedback
- [ ] Water shader/sprite animado
- [ ] Splash particles al entrar/salir
- [ ] Bubble particles mientras nada
- [ ] Distortion effect bajo el agua

### Audio Feedback
- [ ] Sonido de chapuzón al entrar
- [ ] Sonido ambiente bajo el agua
- [ ] Burbujas mientras nada

### Gameplay Feel
- [ ] Swim speed boost (moverte más rápido en agua)
- [ ] Jump desde agua (saltar al salir)
- [ ] Water current (corriente que empuja)

**Nota:** Agregar al polish-backlog.md.

---

## 🎉 COMPLETANDO FEATURE 8

Con esta feature implementada:

```
✅ Gating mechanic funcional
✅ Transformación Frog tiene utilidad
✅ Exploración incentivada
✅ Física condicional implementada
```

**Siguiente:** Issue #25 - Wall Climb (Spider)

---

**¡Éxito con la implementación! Recuerda: 80/20 - Intenta primero, pide ayuda si te atascas >30 min.** 🏊‍♂️

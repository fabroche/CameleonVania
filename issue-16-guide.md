# Issue #16: TransformMask Collectible - Implementation Guide

> **Objetivo:** Crear el item coleccionable (máscara) que transforma al jugador cuando lo recoge.

**Tiempo estimado:** 1-2 horas
**Dificultad:** ⭐⭐⭐ (Media - Triggers y comunicación)

---

## 📚 PARTE 1: TEORÍA (30 min)

### ¿Qué es TransformMask?

**TransformMask** es el **item coleccionable** que permite al player transformarse. Es el puente entre derrotar un enemigo y obtener una transformación.

### Flujo Completo del Sistema

```
┌─────────────────────────────────────────────────────────┐
│              TRANSFORMATION PICKUP FLOW                  │
└─────────────────────────────────────────────────────────┘

1. ENEMY DEATH (Issue #17 - Próximo)
   Enemy.OnDeath event → MaskDrop spawns TransformMask
        ↓
2. MASK FALLS TO GROUND
   TransformMask GameObject con trigger collider
        ↓
3. PLAYER TOUCHES MASK (Issue #16 - HOY)
   OnTriggerEnter2D detecta player
        ↓
4. VERIFY TRANSFORMATION
   ┌─────────────────────────────────────────┐
   │ ¿Player ya tiene esta transformación?  │
   └─────────────────────────────────────────┘
        │
        ├─ YES → Ignorar pickup, máscara permanece
        │
        └─ NO  → Aplicar transformación
                 PlayerTransform.TransformInto(transformationType)
                      ↓
                 5. CLEANUP
                    Destroy mask GameObject
```

### Lógica de Pickup

| Transformación Actual | Máscara Recogida | Resultado |
|-----------------------|------------------|-----------|
| **Base Form** | Frog | ✅ Transforma a Frog, destruye máscara |
| **Base Form** | Spider | ✅ Transforma a Spider, destruye máscara |
| **Frog** | Frog | ❌ Ignora, máscara permanece |
| **Frog** | Spider | ✅ Cambia a Spider, destruye máscara |
| **Spider** | Frog | ✅ Cambia a Frog, destruye máscara |
| **Spider** | Spider | ❌ Ignora, máscara permanece |

**Nota:** La comparación `_currentTransformation == transformationType` compara **referencias de ScriptableObject**, no strings. Esto significa que si tienes dos assets separados con el mismo nombre, se tratarán como transformaciones diferentes.

---

### Conceptos Clave

#### **1. Collider vs Trigger**

| Aspecto | Collider | Trigger |
|---------|----------|---------|
| **Física** | Bloquea objetos | Atravesable |
| **Uso** | Paredes, suelo | Pickups, zonas |
| **Eventos** | OnCollisionEnter2D | OnTriggerEnter2D |
| **Rigidbody** | Necesita uno en algún objeto | Necesita uno en algún objeto |

**Para coleccionables:** SIEMPRE usar **Trigger** (no queremos que bloquee al player)

```
┌─────────────────────────────────────────────────────┐
│         COLLIDER (isTrigger = false)                │
│                                                     │
│   Player ───X    Item                              │
│            ┌─┴──┐                                   │
│            │WALL│  ← Bloquea movimiento            │
│            └────┘                                   │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│         TRIGGER (isTrigger = true)                  │
│                                                     │
│   Player ────→  [Item]                             │
│            ╱      ╲                                 │
│           ╱        ╲  ← Atraviesa, dispara evento  │
│          ╱          ╲                               │
│         OnTriggerEnter2D()                          │
└─────────────────────────────────────────────────────┘
```

---

#### **2. Detectar al Player: Tag vs Layer vs GetComponent**

```csharp
// ❌ OPCIÓN 1: CompareTag (string) - Puede tener typos
void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))  // ¿"Player" o "player"? Fácil error
    {
        // ...
    }
}

// ✅ OPCIÓN 2: Layer (int) - Más robusto
[SerializeField] private LayerMask playerLayer;

void OnTriggerEnter2D(Collider2D other)
{
    if (((1 << other.gameObject.layer) & playerLayer) != 0)
    {
        // ...
    }
}

// ✅ OPCIÓN 3: GetComponent (más seguro) - Preferido
void OnTriggerEnter2D(Collider2D other)
{
    PlayerTransform playerTransform = other.GetComponent<PlayerTransform>();
    if (playerTransform != null)
    {
        // Definitivamente es el player
    }
}
```

**Recomendación:** Para este proyecto, usa **GetComponent** (Opción 3)

**¿Por qué?**
- ✅ No depende de strings (typos)
- ✅ No depende de layers (configuración)
- ✅ Obtienes directamente el componente que necesitas

---

#### **3. OnTriggerEnter2D - Requisitos**

Para que `OnTriggerEnter2D` funcione, necesitas:

```
✅ AL MENOS UNO de los objetos debe tener Rigidbody2D
✅ AL MENOS UNO debe tener collider con isTrigger = true
✅ Ambos deben tener colliders (trigger o normal)
✅ Ambos deben estar en layers que pueden colisionar
```

**Ejemplo válido:**
```
Player:
  - Rigidbody2D (Dynamic)
  - CapsuleCollider2D (isTrigger = false)

TransformMask:
  - NO Rigidbody2D
  - CircleCollider2D (isTrigger = TRUE)
```

---

#### **4. Comunicación entre GameObjects**

```csharp
// ❌ MAL - Asumir que el componente existe
void OnTriggerEnter2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    pt.TransformInto(transformationType); // CRASH si pt es null
}

// ✅ BIEN - Null check
void OnTriggerEnter2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt != null)
    {
        pt.TransformInto(transformationType);
    }
}

// ✅ MÁS LIMPIO - Null-conditional + early return
void OnTriggerEnter2D(Collider2D other)
{
    PlayerTransform pt = other.GetComponent<PlayerTransform>();
    if (pt == null) return; // Salir si no es el player

    pt.TransformInto(transformationType);
    Destroy(gameObject); // Destruir máscara
}
```

---

### Arquitectura de TransformMask

```
TransformMask.cs
├── [Variables]
│   ├── TransformationData transformationType  ← Qué transformación otorga
│   └── bool hasBeenCollected                  ← Evitar doble recogida
│
├── [Setup en Unity]
│   ├── CircleCollider2D (isTrigger = true)
│   ├── SpriteRenderer (visual)
│   └── Layer: Collectible
│
└── [Métodos]
    ├── OnTriggerEnter2D()
    │   ├── Detectar si es Player
    │   ├── Aplicar transformación
    │   └── Destruir máscara
    │
    └── OnDrawGizmosSelected()
        └── Visualizar trigger range
```

---

### 🎓 Recursos de Aprendizaje

**OBLIGATORIO - Lee ANTES de implementar:**

1. **Unity Docs:**
   - [OnTriggerEnter2D](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnTriggerEnter2D.html)
   - [Collider2D.isTrigger](https://docs.unity3d.com/ScriptReference/Collider2D-isTrigger.html)
   - [Physics2D Collision Matrix](https://docs.unity3d.com/Manual/class-Physics2DManager.html)

2. **Conceptos C#:**
   - GetComponent<T>()
   - Early return pattern
   - GameObject.Destroy()

---

## 🛠️ PARTE 2: IMPLEMENTACIÓN (60-90 min)

### Paso 1: Crear el Script Base

**TU TURNO:** Crea el archivo `TransformMask.cs`.

**Ubicación:** `Assets/Scripts/Collectibles/TransformMask.cs`

<details>
<summary>💡 Pista 1: Estructura básica</summary>

```csharp
using UnityEngine;

public class TransformMask : MonoBehaviour
{
    // Variables aquí

    void OnTriggerEnter2D(Collider2D other)
    {
        // Lógica de pickup
    }

    void OnDrawGizmosSelected()
    {
        // Visualización de trigger
    }
}
```
</details>

---

### Paso 2: Definir Variables

**TU TURNO:** Piensa qué variables necesitas.

**Requisitos:**
1. Referencia al TransformationData que otorga
2. Flag para evitar doble recogida
3. Opcional: Efectos visuales/audio

<details>
<summary>💡 Pista 1: Variable principal</summary>

```csharp
[Header("Transformation Settings")]
[SerializeField] private TransformationData transformationType;
```

**¿Por qué SerializeField?** Para asignarlo en el Inspector de Unity.
</details>

<details>
<summary>💡 Pista 2: Flag de seguridad</summary>

```csharp
[Header("Collection State")]
private bool hasBeenCollected = false;
```

**¿Por qué?** Evitar que se recoja dos veces si hay múltiples triggers.
</details>

<details>
<summary>💡 Pista 3: Opcionales (polish)</summary>

```csharp
[Header("VFX/SFX - Optional")]
[SerializeField] private ParticleSystem collectEffect;
[SerializeField] private AudioClip collectSound;
```

**Nota:** Estos son opcionales para polish posterior.
</details>

<details>
<summary>✅ Solución Completa - Variables</summary>

```csharp
[Header("Transformation Settings")]
[Tooltip("La transformación que otorga esta máscara al jugador")]
[SerializeField] private TransformationData transformationType;

[Header("Collection State")]
private bool hasBeenCollected = false;

[Header("VFX/SFX - Optional")]
[SerializeField] private ParticleSystem collectEffect;
[SerializeField] private AudioClip collectSound;
```

**Explicación:**
- `transformationType`: Asset de ScriptableObject (Transformation_Frog, etc.)
- `hasBeenCollected`: Previene bugs si OnTriggerEnter se llama múltiples veces
- `collectEffect/collectSound`: Para feedback visual/audio (opcional)
</details>

---

### Paso 3: Implementar OnTriggerEnter2D

**TU TURNO:** Implementa la lógica de pickup.

**Requisitos:**
1. Detectar si el objeto que entra es el Player
2. Verificar que no se ha recogido antes
3. Aplicar transformación al player
4. Marcar como recogido
5. Destruir la máscara

**Pseudocódigo:**
```
OnTriggerEnter2D(Collider2D other):
    1. Si ya fue recogido → return
    2. Intentar obtener PlayerTransform del other
    3. Si NO es player (null) → return
    4. Validar que transformationType no sea null
    5. NUEVO: Verificar si player YA tiene esta transformación
       - Si currentTransform == transformationType → return (no hacer nada)
    6. Llamar playerTransform.TransformInto(transformationType)
    7. Marcar hasBeenCollected = true
    8. Opcional: Efectos visuales/audio
    9. Destroy(gameObject)
```

<details>
<summary>💡 Pista 1: Early returns</summary>

```csharp
void OnTriggerEnter2D(Collider2D other)
{
    // Guard clause 1: Ya fue recogido
    if (hasBeenCollected) return;

    // Guard clause 2: No es el player
    PlayerTransform playerTransform = other.GetComponent<PlayerTransform>();
    if (playerTransform == null) return;

    // Guard clause 3: No hay transformation data
    if (transformationType == null)
    {
        Debug.LogError("[TransformMask] No transformation type assigned!");
        return;
    }

    // Aquí va la lógica de pickup...
}
```

**Patrón:** Guard clauses al inicio = código más limpio.
</details>

<details>
<summary>💡 Pista 2: Verificar transformación actual</summary>

```csharp
// Verificar si player ya tiene esta transformación
if (playerTransform._currentTransformation == transformationType)
{
    Debug.Log($"[TransformMask] Player already has {transformationType.transformName} form. Ignoring.");
    return; // No hacer nada, ya tiene esta forma
}
```

**¿Por qué?** Evitar redundancia: si ya eres Frog, recoger otra máscara Frog no hace nada.
</details>

<details>
<summary>💡 Pista 3: Aplicar transformación</summary>

```csharp
// Marcar como recogido ANTES de destruir
hasBeenCollected = true;

// Aplicar transformación
playerTransform.TransformInto(transformationType);

// Log para debugging
Debug.Log($"[TransformMask] Player transformed into {transformationType.transformName}");
```
</details>

<details>
<summary>💡 Pista 3: Destruir máscara</summary>

```csharp
// Opción 1: Destrucción inmediata
Destroy(gameObject);

// Opción 2: Con delay (para efectos)
Destroy(gameObject, 0.5f); // Espera 0.5s antes de destruir
```

**¿Cuál elegir?** Inmediata por ahora, con delay después si agregas efectos.
</details>

<details>
<summary>✅ Solución Completa - OnTriggerEnter2D</summary>

```csharp
void OnTriggerEnter2D(Collider2D other)
{
    // Guard: Ya fue recogido (evitar doble pickup)
    if (hasBeenCollected) return;

    // Guard: Detectar si es el player
    PlayerTransform playerTransform = other.GetComponent<PlayerTransform>();
    if (playerTransform == null) return; // No es el player, ignorar

    // Guard: Validar que hay transformation data
    if (transformationType == null)
    {
        Debug.LogError("[TransformMask] No transformation type assigned! Check Inspector.");
        return;
    }

    // Guard: Verificar si player ya tiene esta transformación
    // IMPORTANTE: _currentTransformation es público en PlayerTransform
    // Compara la referencia del ScriptableObject, no el nombre
    if (playerTransform._currentTransformation == transformationType)
    {
        Debug.Log($"[TransformMask] Player already has {transformationType.transformName} form. Ignoring pickup.");
        return; // No hacer nada, ya tiene esta forma
    }

    // Marcar como recogido ANTES de destruir (importante)
    hasBeenCollected = true;

    // Aplicar transformación (solo si es diferente)
    playerTransform.TransformInto(transformationType);

    Debug.Log($"[TransformMask] Player collected mask: {transformationType.transformName}");

    // Opcional: Reproducir efectos
    if (collectEffect != null)
    {
        collectEffect.Play();
    }

    // Destruir máscara
    Destroy(gameObject, 0.1f); // Pequeño delay para efectos
}
```

**Explicación:**
- **Guard clauses:** Salidas tempranas = código más legible
- **hasBeenCollected primero:** Evita bug si Destroy tarda
- **Null checks:** Robusto contra errores de configuración
- **Debug.Log:** Para testing y debugging
</details>

---

### Paso 4: Visualización con Gizmos

**TU TURNO:** Implementa visualización del trigger en Scene view.

**Objetivo:** Ver el área de pickup en el editor.

<details>
<summary>💡 Pista 1: OnDrawGizmosSelected</summary>

```csharp
void OnDrawGizmosSelected()
{
    // Obtener el collider
    CircleCollider2D col = GetComponent<CircleCollider2D>();
    if (col == null) return;

    // Dibujar el trigger range
    Gizmos.color = Color.yellow; // Color distintivo
    Gizmos.DrawWireSphere(transform.position, col.radius);
}
```
</details>

<details>
<summary>✅ Solución Completa - Gizmos</summary>

```csharp
void OnDrawGizmosSelected()
{
    // Visualizar trigger range
    CircleCollider2D col = GetComponent<CircleCollider2D>();
    if (col != null)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, col.radius);
    }
}
```

**Uso:** Selecciona la máscara en Scene view → verás el círculo amarillo.
</details>

---

### Paso 5: Script Completo

<details>
<summary>📄 Código Completo - TransformMask.cs</summary>

```csharp
using UnityEngine;

/// <summary>
/// Coleccionable que transforma al jugador al recogerlo.
/// Requiere: CircleCollider2D con isTrigger = true
/// </summary>
public class TransformMask : MonoBehaviour
{
    [Header("Transformation Settings")]
    [Tooltip("La transformación que otorga esta máscara al jugador")]
    [SerializeField] private TransformationData transformationType;

    [Header("Collection State")]
    private bool hasBeenCollected = false;

    [Header("VFX/SFX - Optional")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Guard: Ya fue recogido (evitar doble pickup)
        if (hasBeenCollected) return;

        // Guard: Detectar si es el player
        PlayerTransform playerTransform = other.GetComponent<PlayerTransform>();
        if (playerTransform == null) return; // No es el player, ignorar

        // Guard: Validar que hay transformation data
        if (transformationType == null)
        {
            Debug.LogError("[TransformMask] No transformation type assigned! Check Inspector.");
            return;
        }

        // Guard: Verificar si player ya tiene esta transformación
        if (playerTransform._currentTransformation == transformationType)
        {
            Debug.Log($"[TransformMask] Player already has {transformationType.transformName} form. Ignoring pickup.");
            return; // No hacer nada, ya tiene esta forma
        }

        // Marcar como recogido ANTES de destruir (importante)
        hasBeenCollected = true;

        // Aplicar transformación (solo si es diferente)
        playerTransform.TransformInto(transformationType);

        Debug.Log($"[TransformMask] Player transformed into {transformationType.transformName}");

        // Opcional: Reproducir efectos
        if (collectEffect != null)
        {
            collectEffect.Play();
        }

        // TODO: Reproducir sonido (requiere AudioSource)

        // Destruir máscara
        Destroy(gameObject, 0.1f); // Pequeño delay para efectos
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar trigger range en Scene view
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
    }
}
```
</details>

---

## 🧪 PARTE 3: SETUP EN UNITY (30 min)

### Test 1: Crear el Prefab Base

**Pasos en Unity:**

1. **Crear GameObject:**
   - Hierarchy → Create Empty → "TransformMask_Frog"

2. **Agregar Componentes:**
   - Add Component → **CircleCollider2D**
     - ✅ Activar "Is Trigger"
     - Radius: 0.5

   - Add Component → **SpriteRenderer**
     - Asignar un sprite temporal (cualquier sprite de prueba)
     - Color: Amarillo (para distinguir)

   - Add Component → **TransformMask** (tu script)
     - Arrastrar `Transformation_Frog` a "Transformation Type"

3. **Configurar Layer:**
   - Layer: Collectible (Layer 10)

4. **Crear Prefab:**
   - Arrastrar "TransformMask_Frog" a `Assets/Prefabs/Collectibles/`
   - Eliminar de Hierarchy

---

### Test 2: Prueba Manual en Escena

**Setup:**
1. Abrir tu escena de testing (SampleScene)
2. Arrastrar prefab `TransformMask_Frog` a la escena
3. Posicionarlo cerca del player
4. Play

**Resultado esperado:**
- Player toca la máscara
- Console log: "Player collected mask: Frog"
- Player se transforma (stats cambian, modelo 3D aparece)
- Máscara desaparece

**¿Funciona?** ✅ Continúa. ❌ Ve a Debugging.

---

### Test 3: Verificar Collision Matrix

**Problema posible:** Player no detecta la máscara.

**Solución:**
1. Edit → Project Settings → Physics 2D
2. Verificar matriz:
   - **Player (Layer 6) ↔ Collectible (Layer 10): ✅ Activado**

Si está desactivado, actívalo y prueba de nuevo.

---

### Test 4: Múltiples Máscaras

**Crear variantes:**
1. Duplicar `TransformMask_Frog` → `TransformMask_Spider`
2. En Inspector, cambiar:
   - Transformation Type: `Transformation_Spider`
   - Sprite: Cambiar color a azul
3. Repetir para Ladybug (verde)

**Testing:**
- Colocar 3 máscaras en la escena
- Recogerlas en orden: Frog → Spider → Ladybug
- Verificar que la transformación cambia cada vez

**Resultado esperado:**
- Cada máscara transforma correctamente
- Stats cambian según transformación
- Console logs muestran nombres correctos

---

### Test 5: Edge Cases

**Test 5.1: Recoger misma transformación que ya tienes**
- Transformar player en Frog (tecla 1 con TestTransformations)
- Colocar máscara TransformMask_Frog
- Intentar recogerla
- **Resultado esperado:**
  - Console log: "Player already has Frog form. Ignoring pickup."
  - Máscara NO se destruye ✅
  - Player sigue en forma Frog

**Test 5.2: Cambiar de transformación**
- Transformar player en Frog
- Colocar máscara TransformMask_Spider
- Recogerla
- **Resultado esperado:**
  - Console log: "Player transformed into Spider"
  - Máscara se destruye ✅
  - Player cambia a forma Spider

**Test 5.3: Enemy toca máscara**
- Empujar enemy hacia máscara
- ¿La recoge? ❌ (Debe ignorar, solo player)

**Test 5.4: TransformationType null**
- Crear máscara SIN asignar TransformationData
- Tocarla con player
- Console debe mostrar: "No transformation type assigned!"
- Máscara NO se destruye ✅

---

## 🐛 DEBUGGING

### Error 1: "OnTriggerEnter2D no se llama"

**Checklist:**
```
[ ] Máscara tiene CircleCollider2D con isTrigger = true
[ ] Player tiene Rigidbody2D
[ ] Player tiene Collider2D (cualquier tipo)
[ ] Layers pueden colisionar (Physics 2D Matrix)
[ ] Máscara está en Layer "Collectible"
[ ] Player está en Layer "Player"
```

**Test de Diagnóstico:**
```csharp
void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"[TransformMask] Trigger detected: {other.gameObject.name}");
    // Si no ves NADA → problema de configuración
    // Si ves el log pero no funciona → problema de lógica
}
```

---

### Error 2: "NullReferenceException al transformar"

**Causa:** PlayerTransform no está en el Player GameObject.

**Solución:**
```csharp
PlayerTransform playerTransform = other.GetComponent<PlayerTransform>();
if (playerTransform == null)
{
    Debug.LogWarning($"[TransformMask] {other.name} doesn't have PlayerTransform!");
    return;
}
```

---

### Error 3: "Máscara se recoge dos veces"

**Causa:** OnTriggerEnter se llama múltiples veces antes de destruir.

**Solución:** Ya implementado con `hasBeenCollected` flag.

**Verificar:**
```csharp
Debug.Log($"hasBeenCollected BEFORE: {hasBeenCollected}");
hasBeenCollected = true;
Debug.Log($"hasBeenCollected AFTER: {hasBeenCollected}");
```

---

### Error 4: "Máscara cae infinito / atraviesa suelo"

**Causa:** Máscara necesita Rigidbody2D para física.

**Solución (Opción 1 - Estática):**
- NO agregar Rigidbody2D
- Posicionar manualmente en el suelo

**Solución (Opción 2 - Física):**
- Add Component → Rigidbody2D
- Gravity Scale: 1
- Freeze Rotation Z: ✅

**Nota:** Para Issue #17 (drop desde enemy), necesitarás Opción 2.

---

## ✅ CHECKPOINT FINAL

Antes de marcar como completado:

### Funcionalidad
- [ ] TransformMask.cs compila sin errores
- [ ] OnTriggerEnter2D detecta al player
- [ ] Transformación se aplica correctamente
- [ ] Máscara se destruye después de recogerla
- [ ] Gizmos muestran trigger range en Scene view

### Setup Unity
- [ ] Prefabs creados para cada transformación
  - [ ] TransformMask_Frog
  - [ ] TransformMask_Spider
  - [ ] TransformMask_Ladybug
- [ ] Todos tienen CircleCollider2D (isTrigger = true)
- [ ] Transformation Type asignado correctamente
- [ ] Layer = Collectible (10)

### Testing
- [ ] Probado recoger cada máscara individualmente
- [ ] Probado recoger múltiples en secuencia
- [ ] Verificado que enemy NO recoge máscaras
- [ ] Verificado error handling (null transformation)

### Debugging
- [ ] Console logs muestran información útil
- [ ] No hay NullReferenceExceptions
- [ ] Collision Matrix configurada correctamente

---

## 🎓 PREGUNTAS DE APRENDIZAJE

<details>
<summary>❓ ¿Cuál es la diferencia entre OnTriggerEnter2D y OnCollisionEnter2D?</summary>

**Respuesta:**

| OnTriggerEnter2D | OnCollisionEnter2D |
|------------------|-------------------|
| Collider con `isTrigger = true` | Collider con `isTrigger = false` |
| NO hay física (atravesable) | SÍ hay física (bloquea) |
| Usado para: pickups, zonas | Usado para: paredes, suelo |
| Parámetro: `Collider2D` | Parámetro: `Collision2D` (más info) |

**Para coleccionables:** Siempre `OnTriggerEnter2D`
</details>

<details>
<summary>❓ ¿Por qué usar hasBeenCollected en lugar de solo Destroy?</summary>

**Respuesta:**

`Destroy(gameObject)` NO es instantáneo:
- Se ejecuta al final del frame
- OnTriggerEnter puede llamarse múltiples veces en un frame
- Sin flag → transformación se aplica múltiples veces → bugs

**Solución:** `hasBeenCollected` previene re-ejecución.
</details>

<details>
<summary>❓ ¿Por qué GetComponent<PlayerTransform>() en vez de CompareTag("Player")?</summary>

**Respuesta:**

**Ventajas de GetComponent:**
- ✅ No depende de strings (evita typos)
- ✅ Si es null, definitivamente NO es el player
- ✅ Ya tienes la referencia que necesitas usar después
- ✅ Type-safe (verificado en compilación)

**CompareTag:**
- ❌ Depende de configuración manual de tags
- ❌ Puede tener typos ("Player" vs "player")
- ❌ Aún necesitas GetComponent después

**Conclusión:** GetComponent es más robusto.
</details>

<details>
<summary>❓ ¿Por qué verificar si player ya tiene la transformación antes de aplicarla?</summary>

**Respuesta:**

**Razones de diseño:**
- ✅ **UX:** Evita redundancia (ya eres Frog, no necesitas transformarte en Frog otra vez)
- ✅ **Economía:** Máscaras no se desperdician (quedan en el suelo para usarlas después)
- ✅ **Gameplay:** Puedes cambiar entre formas estratégicamente
- ✅ **Feedback:** Log claro al usuario de por qué no pasó nada

**Implementación:**
```csharp
if (playerTransform._currentTransformation == transformationType)
{
    return; // Ya tienes esta forma, ignorar
}
```

**Comportamiento esperado:**
- Frog + máscara Frog → No pasa nada (máscara permanece)
- Frog + máscara Spider → Cambia a Spider (máscara se destruye)
- Base + cualquier máscara → Transforma (máscara se destruye)
</details>

---

## 🚀 PRÓXIMOS PASOS

Una vez completado Issue #16:

### 1. Testing Final

Asegúrate de probar:
- Recoger máscara en aire (si player salta)
- Recoger máscara en suelo
- Cambiar transformación varias veces seguidas

### 2. Commit y Push

```bash
git add Assets/Scripts/Collectibles/TransformMask.cs
git add Assets/Prefabs/Collectibles/
git commit -m "feat: Add TransformMask collectible system

- Implemented TransformMask.cs with trigger detection
- Created prefabs for all transformations
- Added Gizmo visualization for pickup range
- Tested: pickup, transformation, cleanup

Closes #16"
git push origin feature/transform-mask-collectible
```

### 3. Crear Pull Request

```bash
gh pr create --title "Feature: TransformMask Collectible" --body "Closes #16"
```

### 4. Continuar con Issue #17

**MaskDrop on Enemy Death** - Completar el ciclo completo del sistema.

---

## 💡 MEJORAS OPCIONALES (Polish)

Si terminas rápido y quieres agregar polish:

### Visual Feedback
- [ ] Particle effect al recoger
- [ ] Float animation (máscara sube/baja)
- [ ] Glow/pulse effect

### Audio Feedback
- [ ] Sonido de pickup
- [ ] Sonido único por transformación

### Gameplay Feel
- [ ] Magnetismo (player atrae máscaras cercanas)
- [ ] Bounce effect al aparecer (Issue #17)

**Nota:** Estos van al polish-backlog.md para Día 5.

---

**¡Éxito con la implementación! Recuerda: 80/20 - Intenta primero, pide ayuda si te atascas >30 min.** 🎮

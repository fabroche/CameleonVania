# Issue #17: MaskDrop on Enemy Death - Implementation Guide

> **Objetivo:** Completar el ciclo del sistema de transformación - enemigos dropean máscaras cuando mueren.

**Tiempo estimado:** 1-2 horas
**Dificultad:** ⭐⭐⭐⭐ (Alta - Eventos y cleanup)

---

## 📚 PARTE 1: TEORÍA (30 min)

### ¿Qué es MaskDrop?

**MaskDrop** es el componente que se agrega a los enemigos para que **dropeen máscaras cuando mueren**. Completa el ciclo:

```
CICLO COMPLETO DE TRANSFORMACIÓN
================================

1. Player ataca Enemy
        ↓
2. Enemy Health llega a 0
        ↓
3. Health.OnDeath event se dispara
        ↓
4. MaskDrop escucha evento → Spawns TransformMask
        ↓
5. Máscara cae al suelo (física)
        ↓
6. Player recoge máscara (Issue #16)
        ↓
7. PlayerTransform.TransformInto() (Issue #15)
```

---

### Conceptos Clave

#### **1. Events en C# - Suscripción y Desuscripción**

```csharp
// ❌ MAL - Suscribirse sin desuscribirse causa MEMORY LEAKS
void Start()
{
    Health enemyHealth = GetComponent<Health>();
    enemyHealth.OnDeath += DropMask; // Suscribirse

    // ⚠️ Si este GameObject se destruye pero el evento no se limpia,
    // la referencia persiste = MEMORY LEAK
}

// ✅ BIEN - Siempre limpiar en OnDestroy
void Start()
{
    health = GetComponent<Health>();
    health.OnDeath += DropMask;
}

void OnDestroy()
{
    if (health != null)
    {
        health.OnDeath -= DropMask; // CRÍTICO: Desuscribirse
    }
}
```

**¿Por qué es crítico?**

```
┌─────────────────────────────────────────────┐
│          SIN CLEANUP (Memory Leak)          │
└─────────────────────────────────────────────┘

Enemy muere → Destroy(gameObject)
    ↓
MaskDrop destruido PERO OnDeath aún tiene referencia
    ↓
Health intenta llamar método en objeto destruido
    ↓
NullReferenceException O peor: objeto zombie en memoria

┌─────────────────────────────────────────────┐
│          CON CLEANUP (Correcto)             │
└─────────────────────────────────────────────┘

OnDestroy() ejecuta ANTES de destruir
    ↓
health.OnDeath -= DropMask (limpia suscripción)
    ↓
Enemy destruido sin referencias pendientes
    ↓
Garbage Collector puede liberar memoria ✅
```

---

#### **2. Orden de Ejecución: Start vs OnDestroy vs Death**

```csharp
// Escenario: Enemy recibe daño fatal

1. Health.TakeDamage(damage) ejecuta
        ↓
2. _currentHealth <= 0 detectado
        ↓
3. Health.Die() ejecuta
        ↓
4. OnDeath?.Invoke() dispara evento
        ↓
5. MaskDrop.DropMask() ejecuta (suscriptor)
        ↓
6. Destroy(gameObject, 0.5f) en Health.Die()
        ↓
7. 0.5 segundos después...
        ↓
8. MaskDrop.OnDestroy() ejecuta (cleanup)
        ↓
9. GameObject destruido
```

**Pregunta:** ¿Qué pasa si no limpiamos en OnDestroy?

<details>
<summary>💡 Respuesta</summary>

El evento `OnDeath` de `Health` seguirá teniendo una referencia al método `DropMask` de un objeto que ya no existe. Si en el futuro otro objeto intenta suscribirse al mismo evento, puede causar errores sutiles o memory leaks.

**Buena práctica:** SIEMPRE limpiar eventos en `OnDestroy`.
</details>

---

#### **3. Instantiate - Spawning GameObjects**

```csharp
// Opción 1: Spawn en posición exacta del enemy
GameObject mask = Instantiate(
    maskPrefab,              // Qué spawner
    transform.position,       // Dónde
    Quaternion.identity      // Rotación (identity = sin rotación)
);

// Opción 2: Spawn con offset (encima del enemy)
Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
GameObject mask = Instantiate(maskPrefab, spawnPosition, Quaternion.identity);

// Opción 3: Spawn como hijo (no recomendado para drops)
GameObject mask = Instantiate(maskPrefab, transform.position, Quaternion.identity, transform);
// ⚠️ Problema: Si el enemy se destruye, el hijo también
```

**Para MaskDrop:** Usa Opción 1 o 2 (sin parent).

---

#### **4. Configurar TransformationData en el Prefab**

```
┌────────────────────────────────────────────┐
│          ¿Cómo sabe qué máscara dropear?   │
└────────────────────────────────────────────┘

Enemy Prefab:
  ├─ Health.cs
  ├─ EnemyAI.cs
  └─ MaskDrop.cs
      └─ [SerializeField] TransformationData transformToDrop
          └─ Asignar en Inspector: Transformation_Frog (por ejemplo)

Cuando muere:
  → Instantiate TransformMask_Frog
  → Configurar transformationType = transformToDrop
```

---

### Arquitectura de MaskDrop

```
MaskDrop.cs
├── [Variables]
│   ├── GameObject maskPrefab             ← Prefab de TransformMask
│   ├── TransformationData transformToDrop ← Qué transformación otorga
│   └── Health health (cached)            ← Referencia al componente Health
│
├── [Lifecycle]
│   ├── Start()
│   │   ├─ Cachear Health component
│   │   └─ Suscribirse a OnDeath
│   │
│   ├── OnDestroy()
│   │   └─ Desuscribirse de OnDeath (CRÍTICO)
│   │
│   └── DropMask() (callback del evento)
│       ├─ Validar que maskPrefab existe
│       ├─ Instantiate máscara
│       ├─ Configurar transformationType
│       └─ Log para debugging
│
└── [Validaciones]
    └─ Null checks para prefab y transformData
```

---

### 🎓 Recursos de Aprendizaje

**OBLIGATORIO - Lee ANTES de implementar:**

1. **Unity Docs:**
   - [Object.Instantiate](https://docs.unity3d.com/ScriptReference/Object.Instantiate.html)
   - [MonoBehaviour.OnDestroy](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDestroy.html)
   - [Events (C#)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)

2. **Conceptos C#:**
   - Event subscription (`+=`)
   - Event unsubscription (`-=`)
   - Null-conditional operator (`?.`)
   - Memory leaks y garbage collection

---

## 🛠️ PARTE 2: IMPLEMENTACIÓN (60-90 min)

### Paso 1: Crear el Script Base

**TU TURNO:** Crea el archivo `MaskDrop.cs`.

**Ubicación:** `Assets/Scripts/Enemies/MaskDrop.cs`

<details>
<summary>💡 Pista 1: Estructura básica</summary>

```csharp
using UnityEngine;

public class MaskDrop : MonoBehaviour
{
    // Variables aquí

    void Start()
    {
        // Suscribirse a evento
    }

    void OnDestroy()
    {
        // CRÍTICO: Desuscribirse
    }

    void DropMask()
    {
        // Lógica de spawn
    }
}
```
</details>

---

### Paso 2: Definir Variables

**TU TURNO:** Piensa qué variables necesitas.

**Requisitos:**
1. Prefab de la máscara a spawner
2. TransformationData que otorgará la máscara
3. Referencia cacheada a Health component

<details>
<summary>💡 Pista 1: Variables principales</summary>

```csharp
[Header("Mask Drop Settings")]
[Tooltip("Prefab de la máscara a instanciar (TransformMask_Frog, etc.)")]
[SerializeField] private GameObject maskPrefab;

[Tooltip("Transformación que otorgará la máscara")]
[SerializeField] private TransformationData transformToDrop;
```
</details>

<details>
<summary>💡 Pista 2: Cache de componente</summary>

```csharp
[Header("Component References")]
private Health health;
```

**¿Por qué cachear?** Para poder desuscribirnos en OnDestroy.
</details>

<details>
<summary>✅ Solución Completa - Variables</summary>

```csharp
[Header("Mask Drop Settings")]
[Tooltip("Prefab de la máscara a instanciar (TransformMask_Frog, etc.)")]
[SerializeField] private GameObject maskPrefab;

[Tooltip("Transformación que otorgará la máscara")]
[SerializeField] private TransformationData transformToDrop;

[Header("Spawn Settings - Optional")]
[Tooltip("Offset vertical del spawn (para que aparezca encima del enemy)")]
[SerializeField] private float spawnHeightOffset = 0.5f;

[Header("Component References")]
private Health health;
```

**Explicación:**
- `maskPrefab`: Prefab con TransformMask.cs (ej: TransformMask_Frog)
- `transformToDrop`: ScriptableObject (ej: Transformation_Frog)
- `spawnHeightOffset`: Para que no aparezca dentro del suelo
- `health`: Necesario para cleanup en OnDestroy
</details>

---

### Paso 3: Implementar Start() - Suscripción

**TU TURNO:** Implementa la suscripción al evento OnDeath.

**Requisitos:**
1. Obtener componente Health
2. Validar que existe
3. Suscribirse al evento OnDeath

**Pseudocódigo:**
```
Start():
    1. health = GetComponent<Health>()
    2. Si health es null → Error y return
    3. health.OnDeath += DropMask
```

<details>
<summary>💡 Pista 1: GetComponent y validación</summary>

```csharp
void Start()
{
    health = GetComponent<Health>();

    if (health == null)
    {
        Debug.LogError("[MaskDrop] Health component not found!");
        return;
    }
}
```
</details>

<details>
<summary>💡 Pista 2: Suscripción al evento</summary>

```csharp
void Start()
{
    health = GetComponent<Health>();

    if (health == null)
    {
        Debug.LogError("[MaskDrop] Health component not found!");
        return;
    }

    // Suscribirse al evento OnDeath
    health.OnDeath += DropMask;

    Debug.Log($"[MaskDrop] Subscribed to {gameObject.name} OnDeath event");
}
```

**Sintaxis:** `evento += método`
</details>

<details>
<summary>✅ Solución Completa - Start()</summary>

```csharp
void Start()
{
    // Cachear componente Health
    health = GetComponent<Health>();

    // Validar que existe
    if (health == null)
    {
        Debug.LogError($"[MaskDrop] Health component not found on {gameObject.name}!");
        enabled = false; // Deshabilitar script si falta componente
        return;
    }

    // Suscribirse al evento OnDeath
    health.OnDeath += DropMask;

    Debug.Log($"[MaskDrop] {gameObject.name} ready to drop {transformToDrop?.transformName ?? "Unknown"} mask");
}
```

**Explicación:**
- Cacheamos `health` para usarlo en OnDestroy
- `enabled = false` deshabilita el script si falta Health
- Log confirma que la suscripción funcionó
</details>

---

### Paso 4: Implementar OnDestroy() - CRÍTICO

**TU TURNO:** Implementa la desuscripción del evento.

**Requisitos:**
1. Verificar que health no es null
2. Desuscribirse de OnDeath

<details>
<summary>💡 Pista 1: Pattern básico</summary>

```csharp
void OnDestroy()
{
    // Cleanup de eventos para evitar memory leaks
    if (health != null)
    {
        health.OnDeath -= DropMask;
    }
}
```

**Sintaxis:** `evento -= método` (inverso de `+=`)
</details>

<details>
<summary>✅ Solución Completa - OnDestroy()</summary>

```csharp
void OnDestroy()
{
    // CRÍTICO: Limpiar suscripción de eventos
    // Esto previene memory leaks y NullReferenceExceptions
    if (health != null)
    {
        health.OnDeath -= DropMask;
        Debug.Log($"[MaskDrop] Unsubscribed from {gameObject.name} OnDeath event");
    }
}
```

**¿Por qué es crítico?**
- Previene memory leaks
- Evita que el evento intente llamar métodos de objetos destruidos
- Es una BUENA PRÁCTICA siempre que uses eventos
</details>

---

### Paso 5: Implementar DropMask() - Spawn Logic

**TU TURNO:** Implementa la lógica de spawning de la máscara.

**Requisitos:**
1. Validar que maskPrefab y transformToDrop existen
2. Calcular posición de spawn (con offset)
3. Instantiate maskPrefab
4. Configurar transformationType en la máscara
5. Log para debugging

**Pseudocódigo:**
```
DropMask():
    1. Validar maskPrefab != null
    2. Validar transformToDrop != null
    3. spawnPosition = position actual + offset hacia arriba
    4. mask = Instantiate(maskPrefab, spawnPosition, sin rotación)
    5. Obtener TransformMask component del mask
    6. Si existe → configurar transformationType
    7. Log
```

<details>
<summary>💡 Pista 1: Validaciones iniciales</summary>

```csharp
void DropMask()
{
    // Guard: Validar maskPrefab
    if (maskPrefab == null)
    {
        Debug.LogWarning($"[MaskDrop] {gameObject.name} has no mask prefab assigned!");
        return;
    }

    // Guard: Validar transformToDrop
    if (transformToDrop == null)
    {
        Debug.LogWarning($"[MaskDrop] {gameObject.name} has no transformation data assigned!");
        return;
    }

    // Lógica de spawn...
}
```
</details>

<details>
<summary>💡 Pista 2: Calcular posición de spawn</summary>

```csharp
// Calcular posición de spawn (encima del enemy para que sea visible)
Vector3 spawnPosition = transform.position + Vector3.up * spawnHeightOffset;
```

**Vector3.up** = (0, 1, 0) en coordenadas del mundo
**spawnHeightOffset** = 0.5f → Spawn 0.5 unidades arriba
</details>

<details>
<summary>💡 Pista 3: Instantiate y configurar</summary>

```csharp
// Instantiate máscara
GameObject maskInstance = Instantiate(
    maskPrefab,
    spawnPosition,
    Quaternion.identity // Sin rotación
);

// Configurar el TransformationData
TransformMask maskComponent = maskInstance.GetComponent<TransformMask>();
if (maskComponent != null)
{
    // Aquí necesitamos setear el transformationType
    // Pero TransformMask tiene el campo private...
    // ¿Cómo lo solucionamos?
}
```

**Problema:** `TransformMask.transformationType` es private con SerializeField.

**Solución:** Tenemos 2 opciones:
1. Hacer público `transformationType` en TransformMask
2. Agregar método setter en TransformMask

**¿Cuál elegir?** Depende de tu preferencia. Para este proyecto, usaremos un **setter público**.
</details>

<details>
<summary>💡 Pista 4: Solución - Agregar setter a TransformMask (opcional)</summary>

**En TransformMask.cs** (modificación menor):

```csharp
public class TransformMask : MonoBehaviour
{
    // ... código existente ...

    // NUEVO: Setter para configurar transformationType
    public void SetTransformationType(TransformationData data)
    {
        transformationType = data;
    }
}
```

**Luego en MaskDrop.cs:**

```csharp
TransformMask maskComponent = maskInstance.GetComponent<TransformMask>();
if (maskComponent != null)
{
    maskComponent.SetTransformationType(transformToDrop);
}
```

**Alternativa:** Hacer público el campo directamente (más simple para este caso).
</details>

<details>
<summary>✅ Solución Completa - DropMask()</summary>

```csharp
void DropMask()
{
    // Guard: Validar maskPrefab
    if (maskPrefab == null)
    {
        Debug.LogWarning($"[MaskDrop] {gameObject.name} has no mask prefab assigned!");
        return;
    }

    // Guard: Validar transformToDrop
    if (transformToDrop == null)
    {
        Debug.LogWarning($"[MaskDrop] {gameObject.name} has no transformation data assigned!");
        return;
    }

    // Calcular posición de spawn
    Vector3 spawnPosition = transform.position + Vector3.up * spawnHeightOffset;

    // Instantiate máscara
    GameObject maskInstance = Instantiate(
        maskPrefab,
        spawnPosition,
        Quaternion.identity
    );

    // Configurar transformationType en la máscara
    TransformMask maskComponent = maskInstance.GetComponent<TransformMask>();
    if (maskComponent != null)
    {
        maskComponent.SetTransformationType(transformToDrop);
        Debug.Log($"[MaskDrop] {gameObject.name} dropped {transformToDrop.transformName} mask");
    }
    else
    {
        Debug.LogError($"[MaskDrop] Spawned mask doesn't have TransformMask component!");
    }
}
```

**Nota:** Requiere agregar método `SetTransformationType` a TransformMask.cs (ver Paso 6).
</details>

---

### Paso 6: Modificar TransformMask.cs (Setter)

**TU TURNO:** Agrega un método setter a TransformMask para configurar el transformationType.

<details>
<summary>💡 Pista: Método simple</summary>

```csharp
// En TransformMask.cs
public void SetTransformationType(TransformationData data)
{
    transformationType = data;
}
```

Agrégalo después de las variables, antes de OnTriggerEnter2D.
</details>

<details>
<summary>✅ Solución Completa - Setter en TransformMask</summary>

```csharp
// En TransformMask.cs - Agregar este método

/// <summary>
/// Configura el tipo de transformación que otorga esta máscara.
/// Usado por MaskDrop para configurar máscaras spawneadas dinámicamente.
/// </summary>
public void SetTransformationType(TransformationData data)
{
    transformationType = data;
    Debug.Log($"[TransformMask] Transformation type set to: {data?.transformName ?? "null"}");
}
```

**Ubicación:** Después de las variables, antes de `OnTriggerEnter2D()`.
</details>

---

### Paso 7: Script Completo

<details>
<summary>📄 Código Completo - MaskDrop.cs</summary>

```csharp
using UnityEngine;

/// <summary>
/// Componente que hace que un enemy dropee una máscara de transformación al morir.
/// Requiere: Health component en el mismo GameObject
/// </summary>
public class MaskDrop : MonoBehaviour
{
    [Header("Mask Drop Settings")]
    [Tooltip("Prefab de la máscara a instanciar (TransformMask_Frog, etc.)")]
    [SerializeField] private GameObject maskPrefab;

    [Tooltip("Transformación que otorgará la máscara")]
    [SerializeField] private TransformationData transformToDrop;

    [Header("Spawn Settings")]
    [Tooltip("Offset vertical del spawn (para que aparezca encima del enemy)")]
    [SerializeField] private float spawnHeightOffset = 0.5f;

    [Header("Component References")]
    private Health health;

    void Start()
    {
        // Cachear componente Health
        health = GetComponent<Health>();

        // Validar que existe
        if (health == null)
        {
            Debug.LogError($"[MaskDrop] Health component not found on {gameObject.name}!");
            enabled = false;
            return;
        }

        // Suscribirse al evento OnDeath
        health.OnDeath += DropMask;

        Debug.Log($"[MaskDrop] {gameObject.name} ready to drop {transformToDrop?.transformName ?? "Unknown"} mask");
    }

    void OnDestroy()
    {
        // CRÍTICO: Limpiar suscripción de eventos
        // Esto previene memory leaks y NullReferenceExceptions
        if (health != null)
        {
            health.OnDeath -= DropMask;
            Debug.Log($"[MaskDrop] Unsubscribed from {gameObject.name} OnDeath event");
        }
    }

    void DropMask()
    {
        // Guard: Validar maskPrefab
        if (maskPrefab == null)
        {
            Debug.LogWarning($"[MaskDrop] {gameObject.name} has no mask prefab assigned!");
            return;
        }

        // Guard: Validar transformToDrop
        if (transformToDrop == null)
        {
            Debug.LogWarning($"[MaskDrop] {gameObject.name} has no transformation data assigned!");
            return;
        }

        // Calcular posición de spawn
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeightOffset;

        // Instantiate máscara
        GameObject maskInstance = Instantiate(
            maskPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // Configurar transformationType en la máscara
        TransformMask maskComponent = maskInstance.GetComponent<TransformMask>();
        if (maskComponent != null)
        {
            maskComponent.SetTransformationType(transformToDrop);
            Debug.Log($"[MaskDrop] {gameObject.name} dropped {transformToDrop.transformName} mask at {spawnPosition}");
        }
        else
        {
            Debug.LogError($"[MaskDrop] Spawned mask doesn't have TransformMask component!");
        }
    }
}
```
</details>

---

## 🧪 PARTE 3: SETUP EN UNITY Y TESTING (45 min)

### Test 1: Modificar TransformMask.cs

**Primero:** Agrega el setter a TransformMask.cs

```csharp
// En Assets/Scripts/Collectibles/TransformMask.cs
// Agregar DESPUÉS de las variables:

public void SetTransformationType(TransformationData data)
{
    transformationType = data;
    Debug.Log($"[TransformMask] Transformation type set to: {data?.transformName ?? "null"}");
}
```

---

### Test 2: Setup en Enemy Prefab

**Pasos en Unity:**

1. **Abrir Enemy Prefab**
   - Selecciona tu enemy en `Assets/Prefabs/Enemies/` o en Hierarchy

2. **Add Component → MaskDrop**

3. **Configurar en Inspector:**
   - **Mask Prefab:** Arrastrar `TransformMask_Frog` (el prefab genérico)
   - **Transform To Drop:** Arrastrar `Transformation_Frog`
   - **Spawn Height Offset:** 0.5

4. **Verificar componentes requeridos:**
   - ✅ Health.cs presente
   - ✅ MaskDrop.cs configurado

5. **Save Prefab**

---

### Test 3: Prueba de Ciclo Completo

**Setup:**
1. Play mode
2. Atacar enemy hasta matarlo
3. Observar console y escena

**Resultado esperado:**

**Console logs (en orden):**
```
[MaskDrop] EnemyName ready to drop Frog mask
[Health] EnemyName has died!
[MaskDrop] EnemyName dropped Frog mask at (x, y, z)
[TransformMask] Transformation type set to: Frog
[MaskDrop] Unsubscribed from EnemyName OnDeath event
```

**En escena:**
- ✅ Enemy muere y desaparece
- ✅ Máscara aparece en posición del enemy
- ✅ Máscara cae al suelo (si tiene Rigidbody2D)
- ✅ Player puede recoger la máscara
- ✅ Player se transforma

---

### Test 4: Ciclo Completo - Frog

**Testing detallado:**

1. **Preparación:**
   - Enemy con MaskDrop configurado (Frog)
   - Player en forma base

2. **Ejecución:**
   - Matar enemy
   - Máscara aparece
   - Recoger máscara

3. **Verificación:**
   - Console: "Player transformed into Frog"
   - Stats cambian (velocidad, salto)
   - Modelo 3D de Frog aparece
   - CanSwim() = true

---

### Test 5: Múltiples Enemies con Diferentes Máscaras

**Setup:**
1. Crear 3 enemies en escena:
   - Enemy1: Drop Frog
   - Enemy2: Drop Spider
   - Enemy3: Drop Ladybug

2. **Matar en orden**

**Resultado esperado:**
- Cada enemy dropea su máscara correspondiente
- 3 máscaras diferentes en el suelo
- Player puede recogerlas en cualquier orden
- Transformación cambia según máscara

---

### Test 6: Edge Cases

**Test 6.1: MaskPrefab null**
- Enemy con MaskDrop pero sin asignar maskPrefab
- Matar enemy
- **Esperado:** Warning en console, no spawns nada

**Test 6.2: TransformToDrop null**
- Enemy con maskPrefab pero sin transformToDrop
- Matar enemy
- **Esperado:** Warning en console, no spawns nada

**Test 6.3: Enemy destruido antes de morir**
- `Destroy(enemy)` directamente (sin TakeDamage)
- **Esperado:** OnDestroy limpia eventos sin errores

---

## 🐛 DEBUGGING

### Error 1: "NullReferenceException en DropMask"

**Causa:** Health.OnDeath se dispara pero maskPrefab o transformToDrop es null.

**Solución:** Ya implementado con guard clauses.

**Verificar:**
```csharp
if (maskPrefab == null)
{
    Debug.LogWarning("Check Inspector: maskPrefab not assigned!");
    return;
}
```

---

### Error 2: "Máscara no aparece"

**Checklist:**
```
[ ] maskPrefab asignado en Inspector
[ ] transformToDrop asignado en Inspector
[ ] Console muestra "dropped Frog mask"
[ ] Spawn position es visible (no debajo del suelo)
[ ] Prefab tiene TransformMask component
```

**Test de diagnóstico:**
```csharp
Debug.Log($"Spawn position: {spawnPosition}");
Debug.Log($"Mask instantiated: {maskInstance != null}");
Debug.Log($"Mask name: {maskInstance.name}");
```

---

### Error 3: "Memory leak warning" o comportamiento extraño

**Causa:** No se está desuscribiendo en OnDestroy.

**Solución:**
```csharp
void OnDestroy()
{
    if (health != null)
    {
        health.OnDeath -= DropMask; // Asegúrate de que esta línea existe
    }
}
```

**Verificar:** Console debe mostrar "Unsubscribed" cuando el enemy muere.

---

### Error 4: "TransformMask doesn't have SetTransformationType"

**Causa:** No agregaste el método setter a TransformMask.cs.

**Solución:** Agrega el método en TransformMask.cs (ver Paso 6).

---

### Error 5: "Máscara cae infinito"

**Causa:** TransformMask prefab no tiene Rigidbody2D.

**Solución:**
- Abrir prefab TransformMask_Frog
- Add Component → Rigidbody2D
- Gravity Scale: 1
- Freeze Rotation Z: ✅

---

## ✅ CHECKPOINT FINAL

Antes de marcar como completado:

### Funcionalidad
- [ ] MaskDrop.cs compila sin errores
- [ ] TransformMask.cs tiene método SetTransformationType
- [ ] Enemy dropea máscara al morir
- [ ] Máscara cae al suelo correctamente
- [ ] Player puede recoger máscara dropeada
- [ ] Transformación funciona desde máscara dropeada
- [ ] OnDestroy limpia eventos correctamente

### Setup Unity
- [ ] MaskDrop agregado al enemy prefab
- [ ] maskPrefab asignado (TransformMask_Frog)
- [ ] transformToDrop asignado (Transformation_Frog)
- [ ] spawnHeightOffset configurado (0.5)
- [ ] TransformMask prefab tiene Rigidbody2D

### Testing
- [ ] Probado ciclo completo: Matar → Drop → Recoger → Transformar
- [ ] Probado múltiples enemies con diferentes máscaras
- [ ] Verificado cleanup de eventos (console logs)
- [ ] Verificado edge cases (null prefab, null data)

### Code Quality
- [ ] Eventos se limpian en OnDestroy
- [ ] Guard clauses validan nulls
- [ ] Debug logs útiles
- [ ] No hay memory leaks

---

## 🎓 PREGUNTAS DE APRENDIZAJE

<details>
<summary>❓ ¿Por qué es CRÍTICO desuscribirse de eventos en OnDestroy?</summary>

**Respuesta:**

**Problema sin cleanup:**
```
Enemy muere → Destroy(gameObject, 0.5s)
    ↓
MaskDrop destruido PERO health.OnDeath aún referencia DropMask()
    ↓
Si Health intenta disparar OnDeath otra vez = NullReferenceException
    ↓
Garbage Collector NO puede liberar memoria = MEMORY LEAK
```

**Solución:**
```csharp
void OnDestroy()
{
    health.OnDeath -= DropMask; // Limpia la suscripción
}
```

**Regla:** Siempre limpiar eventos en OnDestroy si te suscribes en Start/Awake.
</details>

<details>
<summary>❓ ¿Por qué usar GameObject parameter en Instantiate en vez de solo el prefab?</summary>

**Respuesta:**

```csharp
// Forma correcta
GameObject mask = Instantiate(maskPrefab, position, rotation);

// ❌ Incorrecto (no compila)
Instantiate(maskPrefab);
```

**Razón:**
- `Instantiate` retorna `Object` genérico
- Necesitas cast a `GameObject` para usar `.GetComponent<>()`
- Especificar posición y rotación evita valores por defecto

**Alternativa:**
```csharp
GameObject mask = Instantiate(maskPrefab);
mask.transform.position = spawnPosition;
```

Pero es más código. Mejor usar overload completo.
</details>

<details>
<summary>❓ ¿Qué pasa si olvido cachear 'health' en Start?</summary>

**Respuesta:**

```csharp
// ❌ Sin cachear
void Start()
{
    GetComponent<Health>().OnDeath += DropMask; // No guardamos referencia
}

void OnDestroy()
{
    // ⚠️ Problema: No tenemos 'health' para desuscribirnos
    // GetComponent<Health>().OnDeath -= DropMask; // Puede ser null aquí
}
```

**Solución: Cachear siempre**
```csharp
private Health health; // Variable de clase

void Start()
{
    health = GetComponent<Health>();
    health.OnDeath += DropMask;
}

void OnDestroy()
{
    if (health != null) // Tenemos la referencia
    {
        health.OnDeath -= DropMask;
    }
}
```
</details>

---

## 🚀 PRÓXIMOS PASOS

Una vez completado Issue #17:

### 1. Testing Final del Sistema Completo

**Ciclo completo de transformación:**
```
1. Spawn enemy en escena
2. Atacar hasta matar
3. Enemy dropea máscara
4. Recoger máscara
5. Transformarse
6. Repetir con diferentes enemies/máscaras
```

### 2. Crear Prefabs de Máscaras Adicionales

Si aún no los tienes:
- TransformMask_Spider
- TransformMask_Ladybug

### 3. Configurar Enemies para Dropear Máscaras Específicas

Cada tipo de enemy debe dropear su máscara correspondiente:
- FrogEnemy → Drop Frog mask
- SpiderEnemy → Drop Spider mask
- LadybugEnemy → Drop Ladybug mask

### 4. Commit y Push

```bash
git add Assets/Scripts/Enemies/MaskDrop.cs
git add Assets/Scripts/Collectibles/TransformMask.cs  # Modificación
git add Assets/Prefabs/Enemies/  # Prefabs actualizados
git commit -m "feat: Add MaskDrop system for enemy death

- Implemented MaskDrop.cs with event subscription
- Enemies drop transformation masks on death
- Added SetTransformationType to TransformMask.cs
- Proper event cleanup in OnDestroy to prevent memory leaks
- Configurable spawn offset for masks
- Tested complete transformation cycle

Closes #17

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
git push origin feature/mask-drop-enemy-death
```

### 5. Crear Pull Request

```bash
gh pr create --title "feat: MaskDrop System - Enemy Death" --body "Closes #17"
```

---

## 💡 MEJORAS OPCIONALES (Polish)

Si terminas rápido:

### Visual/Audio Feedback
- [ ] Particle effect al spawner máscara
- [ ] Sonido de drop
- [ ] Bounce effect (máscara rebota al caer)

### Gameplay Feel
- [ ] Drop velocity (máscara sale disparada ligeramente)
- [ ] Glow effect en máscara recién dropeada
- [ ] Fade in de la máscara

**Nota:** Estos van al polish-backlog.md para Día 5.

---

## 🎉 COMPLETANDO DÍA 3

Con Issue #17, **completas el sistema de transformación completo**:

```
✅ Issue #14: TransformationData ScriptableObject
✅ Issue #15: PlayerTransform Component
✅ Issue #16: TransformMask Collectible
✅ Issue #17: MaskDrop on Enemy Death
```

**Sistema completo funcional:**
```
Player ataca → Enemy muere → Dropea máscara → Player recoge → Se transforma
```

---

**¡Éxito con la implementación! Recuerda: 80/20 - Intenta primero, pide ayuda si te atascas >30 min.** 🎮

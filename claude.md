# CameleonVania - Memoria de Desarrollo 🎮

> **Proyecto:** Metroidvania 2D - Game Jam Educativa  
> **Inicio:** 26 de Enero, 2026  
> **Duración:** 6 días  
> **Estado Actual:** Día 1 Completado ✅

---

## 🎯 INFORMACIÓN CRÍTICA PARA RETOMAR DESARROLLO

### **1. Configuración del Agente**

**IMPORTANTE:** Este proyecto usa el **Game Jam Advisor Agent** configurado en `node_modules/@genzai_cloud/agent-game-jam-advisor`

#### **Cómo activar el agente:**

```
Usuario: "Usa la configuración del agente que está dentro de node_modules y actúa como él en esta sección"

El agente responderá preguntando tu rol:
1. 🎯 Coordinador
2. 💻 Programador ← SELECCIONA ESTE
3. 🎮 Game Designer
4. 🎬 Animador
5. 🎨 Modelador
```

**Rol actual:** Programador (modo activado)

#### **Filosofía del Agente:**

- **80/20 Rule:** 80% tú implementas, 20% pides ayuda
- **Done is Better than Perfect:** Terminar es la prioridad
- **Aprender Implementando:** Entender cada línea de código

---

### **2. Plan de Implementación**

**Archivo:** `plan-implementacion.md`

**CRÍTICO:** Este archivo es la **biblia del proyecto**. Contiene:

- Plan de 6 días detallado
- Features por día con teoría y ejercicios
- Checkpoints de validación
- Recursos de aprendizaje

**Cómo usarlo:**

1. Lee el plan del día actual
2. Implementa cada feature siguiendo los pasos
3. Marca como completado en el plan
4. Al final del día, actualiza el estado

---

### **3. Metodología de Trabajo**

#### **Workflow Git (Feature Branch)**

```
Para cada Issue:
1. Crear rama: git checkout -b feature/issue-name
2. Implementar funcionalidad
3. Commit(s) atómicos
4. Push: git push -u origin feature/issue-name
5. Crear Pull Request (gh pr create)
6. Review y merge
7. Eliminar rama local
8. Checkout main y pull
9. Siguiente issue
```

#### **Convención de Nombres de Ramas:**

```
Patrón: {tipo}/{descripcion-corta-kebab-case}

Tipos:
- feature/  → Nueva funcionalidad
- setup/    → Configuración
- fix/      → Bug fix
- refactor/ → Refactorización

Ejemplos:
- feature/player-horizontal-movement
- feature/player-jump-ground-check
- feature/camera-follow-2d
- setup/layers-collision-matrix
```

---

### **4. Formato de Issues en GitHub**

#### **Template de Issue:**

```markdown
## Feature: [Nombre de la Feature]

### Descripción

[Descripción clara de qué implementar]

### Conceptos a Aprender

- Concepto 1
- Concepto 2

### Tareas

- [ ] Tarea 1
- [ ] Tarea 2
- [ ] Testing

### Criterios de Aceptación

- [ ] Criterio 1
- [ ] Criterio 2

### Valores Recomendados

- Variable1: valor
- Variable2: valor

### Estimación

X horas

### Target

Day X - Feature Y
```

#### **Labels Usados:**

```
- feature        → Nueva funcionalidad
- setup          → Configuración inicial
- P0-critical    → Crítico para MVP
- P1-high        → Importante para MVP
- P2-medium      → Nice to have
- day-1, day-2   → Día correspondiente
- programmer     → Rol asignado
```

---

### **5. Estructura del Proyecto**

```
CameleonVania/
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity
│   ├── Scripts/
│   │   ├── Player/
│   │   │   └── PlayerController.cs
│   │   ├── Cameras/
│   │   │   └── CameraFollow2D.cs
│   │   ├── Enemies/
│   │   ├── Collectibles/
│   │   ├── Managers/
│   │   └── UI/
│   ├── Prefabs/
│   ├── Sprites/
│   └── Audio/
├── JamDaysSummary/
│   └── day-1-summary.md
├── node_modules/
│   └── @genzai_cloud/
│       └── agent-game-jam-advisor/  ← Configuración del agente
├── plan-implementacion.md           ← Plan maestro
├── GDD-GGJ2026.txt                  ← Game Design Document
└── claude.md                        ← Este archivo
```

---

### **6. Estado Actual del Proyecto**

#### **Día 1 - COMPLETADO ✅**

**Issues Completadas:**

- ✅ #1: Setup - Layers y Collision Matrix
- ✅ #2: PlayerController2D - Movimiento Horizontal
- ✅ #3: Player Jump y Ground Check
- ✅ #4: CameraFollow2D

**Features Funcionales:**

- ✅ Player se mueve (A/D, flechas)
- ✅ Player salta (Space)
- ✅ Ground detection con Physics2D.OverlapCircle
- ✅ Cámara sigue al player con Cinemachine
- ✅ Dead zones configuradas

**Código Actual:**

- `PlayerController.cs`: 40 líneas, 100% funcional
- `CameraFollow2D.cs`: Cinemachine configurado
- 0 errores de compilación

---

### **7. Próximo Paso: DÍA 2**

**Objetivo:** Combate y Health System

**Issues a Crear:**

1. Health System (componente reutilizable con eventos)
2. Player Attack (detección y daño)
3. Enemy AI (State Machine: Patrol, Chase, Attack)
4. Combat Loop completo

**Conceptos Nuevos:**

- Events y delegates en C#
- Finite State Machines (FSM)
- Component-based architecture
- LayerMask filtering para ataque

---

## 📋 CHECKLIST PARA RETOMAR DESARROLLO

Cuando retomes el proyecto, sigue estos pasos:

### **1. Activar Contexto**

```
□ Abrir Unity (CameleonVania project)
□ Abrir VS Code en la carpeta del proyecto
□ Activar agente: "Usa la configuración del agente en node_modules"
□ Seleccionar rol: "2 - Programador"
□ Leer este archivo (claude.md)
□ Leer GDD-GGJ2026.txt (Game Design Document)
```

### **2. Revisar Estado**

```
□ Leer plan-implementacion.md (día actual)
□ Revisar JamDaysSummary/day-X-summary.md (último día)
□ Ver issues abiertas: gh issue list
□ Verificar rama actual: git branch --show-current
```

### **3. Preparar Día**

```
□ Leer objetivos del día en plan-implementacion.md
□ Verificar que issues del día estén creadas en GitHub
□ Si no existen, crearlas según el plan
□ Checkout main: git checkout main
□ Pull últimos cambios: git pull origin main
```

### **4. Comenzar Issue**

```
□ Crear rama: git checkout -b feature/issue-name
□ Leer guía de implementación (si existe en artifacts)
□ Implementar siguiendo metodología 80/20
□ Testing
□ Commit y push
□ Crear PR y merge
```

---

## 🎓 METODOLOGÍA DE APRENDIZAJE

### **Ciclo por Feature:**

```
1. ENTENDER
   → ¿Qué hace esta feature?
   → Lee la teoría en plan-implementacion.md

2. DISEÑAR
   → ¿Cómo la implementarías tú?
   → Dibuja en papel si es necesario

3. INVESTIGAR
   → Unity docs, tutorials
   → Fuentes proporcionadas por el agente

4. IMPLEMENTAR
   → Escribe el código
   → NO copies, entiende cada línea

5. DEBUGGEAR
   → Usa Debug.Log
   → Usa Gizmos para visualización
   → Resuelve errores (aquí aprendes más)

6. REFACTORIZAR
   → Mejora el código
   → Aplica buenas prácticas

7. VALIDAR
   → ¿Funciona como esperabas?
   → Responde checkpoints del plan
```

### **Cuándo Pedir Ayuda:**

✅ **PIDE AYUDA si:**

- Llevas >30 min atascado en el mismo error
- No entiendes un concepto fundamental
- Necesitas validar tu approach
- Quieres code review

❌ **NO PIDAS AYUDA si:**

- No has leído la documentación de Unity
- No has intentado Debug.Log
- Es tu primer intento

---

## 🔧 CONFIGURACIONES IMPORTANTES

### **Unity Settings:**

```
Layers:
- Player (Layer 6)
- Enemy (Layer 7)
- Ground (Layer 8)
- Water (Layer 9)
- Collectible (Layer 10)
- SmallGap (Layer 11)

Collision Matrix:
- Player ↔ Enemy: ✅
- Player ↔ Ground: ✅
- Player ↔ Collectible: ❌ (Trigger)
- Player ↔ Water: ❌ (Trigger)
- Enemy ↔ Enemy: ❌
- Enemy ↔ Ground: ✅
```

### **Player Configuration:**

```
Rigidbody2D:
- Gravity Scale: 3
- Freeze Rotation Z: ✅

CapsuleCollider2D:
- Direction: Vertical
- Size: (0.5, 1.0)

PlayerController:
- Move Speed: 5
- Jump Force: 10
- Ground Check Radius: 0.2
```

### **Camera Configuration:**

```
Cinemachine Virtual Camera:
- Follow: Player
- Dead Zone: (0.1, 0.1)
- Screen Position: (0.5, 0.5)
- Damping: (1, 1, 0)
- Camera Distance: 10
```

---

## 📖 RECURSOS CLAVE

### **Documentación del Agente:**

```
node_modules/@genzai_cloud/agent-game-jam-advisor/
├── prompt-principal.md       → Core del agente
├── modos/
│   └── modo-programador.md   → Conocimiento especializado
└── docs/
    └── guidelines/           → Guías completas
```

### **Unity Docs Esenciales:**

- Rigidbody2D: https://docs.unity3d.com/Manual/class-Rigidbody2D.html
- Physics2D: https://docs.unity3d.com/Manual/Physics2DReference.html
- Input System: https://docs.unity3d.com/ScriptReference/Input.html
- Cinemachine: https://docs.unity3d.com/Packages/com.unity.cinemachine@2.6/manual/

---

## 🎯 MILESTONES CRÍTICOS

```
Jam de 6 días:

Día 1 (26 ENE): ✅ COMPLETADO
- First Playable básico
- Player movement + jump + camera

Día 2 (27 ENE): ⏳ SIGUIENTE
- Combat system
- Health system
- Enemy AI básica

Día 3 (28 ENE):
- Transformation system (CORE mechanic)
- ScriptableObjects

Día 4 (29 ENE):
- Collectibles
- UI/HUD
- Polish

Día 5 (30 ENE):
- Level design
- Audio
- Testing

Día 6 (31 ENE):
- Final polish
- Build
- Submission
```

---

## 🚨 REGLAS DE ORO

1. **Done is Better than Perfect**
   - La única forma de fallar es no terminar

2. **Actualizar Documentación**
   - Al completar cada día, actualizar:
     - plan-implementacion.md (marcar completado)
     - Crear day-X-summary.md en JamDaysSummary/
     - Crear issues del día siguiente en GitHub

3. **Commits Atómicos**
   - Un commit por feature lógica
   - Mensajes descriptivos (feat:, fix:, chore:)

4. **Testing Continuo**
   - Probar cada feature antes de commit
   - Usar Debug.Log y Gizmos

5. **Consultar el Agente**
   - Siempre activar con configuración de node_modules
   - Modo Programador para código
   - Seguir filosofía 80/20

---

## 📝 TEMPLATE PARA CREAR ISSUES DEL SIGUIENTE DÍA

Al final de cada día, crear issues del día siguiente usando este comando:

```bash
# Ejemplo para Día 2:
gh issue create --title "[FEATURE] Health System - Day 2" \
  --body "$(cat issue-template.md)" \
  --label "feature,P0-critical,day-2,programmer"
```

**Archivo issue-template.md** debe seguir el formato documentado en sección 4.

---

## 🔄 WORKFLOW COMPLETO DE UN DÍA

```
INICIO DEL DÍA:
1. Leer plan-implementacion.md (día actual)
2. Verificar issues creadas en GitHub
3. Activar agente (node_modules config)

POR CADA ISSUE:
4. Crear rama feature/issue-name
5. Leer teoría del plan
6. Implementar (80/20)
7. Testing
8. Commit y push
9. PR y merge
10. Siguiente issue

FIN DEL DÍA:
11. Crear day-X-summary.md
12. Actualizar plan-implementacion.md
13. Crear issues del día siguiente
14. Commit documentación
```

---

## 💾 COMANDOS ÚTILES

### **Git:**

```bash
# Ver estado
git status
git branch --show-current

# Crear rama
git checkout -b feature/issue-name

# Commit
git add .
git commit -m "feat: descripción"

# Push
git push -u origin feature/issue-name

# PR
gh pr create --title "Título" --body "Closes #X"

# Merge y cleanup
git checkout main
git pull origin main
git branch -d feature/issue-name
```

### **GitHub CLI:**

```bash
# Ver issues
gh issue list
gh issue view X

# Crear issue
gh issue create --title "Título" --body "Cuerpo" --label "labels"

# Ver PRs
gh pr list
gh pr view X
```

### **Unity:**

```bash
# Abrir proyecto
start CameleonVania.sln
```

---

## 📊 TRACKING DE PROGRESO

### **Día 1:** ✅ COMPLETADO

- Issues: 4/4
- Features: 4/4
- Tiempo: ~4h
- Estado: 100% funcional

### **Día 2:** ⏳ PENDIENTE

- Issues: 0/4 (crear al inicio)
- Features: 0/4
- Tiempo estimado: 4-6h

### **Días 3-6:** 📅 PLANIFICADO

- Ver plan-implementacion.md

---

## 🎮 GAME DESIGN DOCUMENT

**Archivo:** `GDD-GGJ2026.txt`

**Concepto:** Metroidvania 2D donde un camaleón se transforma en diferentes criaturas al derrotar enemigos.

**Mecánica Core:**

- Player derrota enemigo
- Enemigo dropea máscara
- Player recoge máscara
- Se transforma en ese enemigo
- Gana habilidades únicas
- Puede acceder a nuevas áreas

**Transformaciones Planeadas:**

1. Pez - Nadar en agua
2. Araña - Escalar paredes
3. Mariquita - Pasar por espacios pequeños

---

## 🏆 OBJETIVOS DEL PROYECTO

### **Primario:**

- ✅ Aprender y entender cada sistema
- ⏳ Terminar un juego funcional

### **Secundario:**

- ⏳ Aplicar buenas prácticas
- ⏳ Usar herramientas profesionales (Cinemachine, etc.)
- ⏳ Documentar el proceso

---

**Última actualización:** 27 de Enero, 2026 - 00:00  
**Próxima acción:** Crear issues del Día 2 y comenzar Health System

---

**Done is better than perfect. Siempre.** ✨

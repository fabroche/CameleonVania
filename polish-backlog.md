# 🎨 Polish Backlog - CameleonVania

Lista de mejoras de polish pendientes para implementar en la fase de polish del proyecto.

---

## 🎯 Día 2 - Combat System Polish

### Visual Feedback

- [ ] **Hit Effects**
  - Particle effects al golpear enemigos
  - Screen shake al recibir daño
  - Flash effect en sprites al tomar daño
  - Blood/impact particles

- [ ] **Attack Feedback**
  - Trail effect en ataque del player
  - Attack animation polish
  - Weapon swing VFX
  - Hit pause/freeze frame

### Audio Feedback

- [ ] **Combat Sounds**
  - Player attack sound (swing)
  - Enemy hit sound
  - Player hit sound
  - Enemy death sound
  - Footsteps durante patrol/chase

- [ ] **Ambient Audio**
  - Background music
  - Ambient sounds

### Animation Polish

- [ ] **Player Animations**
  - Attack animation refinement
  - Hit reaction animation
  - Death animation
  - Idle animation improvements

- [ ] **Enemy Animations**
  - Attack animation
  - Hit reaction animation
  - Death animation
  - Patrol/walk animation
  - Chase/run animation

### Gameplay Feel

- [ ] **Knockback Tuning**
  - Ajustar fuerza de knockback
  - Ajustar duración de stun
  - Agregar knockback curve (ease-out)
  - Diferenciar knockback por tipo de ataque

- [ ] **Combat Balance**
  - Fine-tune damage values
  - Adjust health values
  - Balance attack speeds
  - Adjust detection ranges

### UI/UX

- [ ] **Health Display**
  - Health bar para player
  - Health bar para enemies
  - Damage numbers floating text
  - Health bar animations

- [ ] **Combat Indicators**
  - Enemy detection indicator
  - Attack range indicator
  - Cooldown visual feedback

---

## 🎯 Día 3 - Transformation System Polish

### Code Quality & Validation

- [ ] **PlayerTransform Validations**
  - Agregar `enabled = false` en Start() si faltan componentes críticos
  - Validar `transformationData != null` en ApplyTransformationStats()
  - Validar rangos razonables de multipliers (0.1f - 5f)
  - Mejorar mensajes de error con contexto más descriptivo
  - Agregar validación de null antes de llamar setters

- [ ] **TransformationData Validation**
  - Validar que multipliers estén en rangos válidos
  - Advertir si modelPrefab es null pero se intenta transformar
  - Validar que al menos una habilidad esté activa

---

## 🎯 Día 3+ - Future Polish

### Transformation System Polish

#### TransformMask Collectible (Issue #16)
- [ ] **Visual Feedback**
  - Particle effect al recoger máscara
  - Float animation (máscara sube/baja)
  - Glow/pulse effect
- [ ] **Audio Feedback**
  - Sonido de pickup
  - Sonido único por transformación
- [ ] **Gameplay Feel**
  - Magnetismo (player atrae máscaras cercanas)

#### MaskDrop System (Issue #17)
- [ ] **Drop Effects**
  - Particle effect al spawner máscara
  - Sonido de drop
  - Bounce effect (máscara rebota al caer)
- [ ] **Drop Behavior**
  - Drop velocity (máscara sale disparada ligeramente)
  - Glow effect en máscara recién dropeada
  - Fade in de la máscara

#### General Transformation
- [ ] Transformation VFX
- [ ] Transformation sound effects
- [ ] Smooth transition animations

#### Water Zones & Swimming (Issue #24)
- [ ] **Visual Feedback**
  - Water shader/sprite animado
  - Splash particles al entrar/salir del agua
  - Bubble particles mientras nada
  - Distortion effect bajo el agua
- [ ] **Audio Feedback**
  - Sonido de chapuzón al entrar
  - Sonido ambiente bajo el agua
  - Burbujas mientras nada
- [ ] **Gameplay Feel**
  - Swim speed boost (moverte más rápido en agua)
  - Jump desde agua (saltar al salir)
  - Water current (corriente que empuja)

### Level Design Polish

- [ ] Background parallax
- [ ] Environmental details
- [ ] Lighting effects
- [ ] Camera shake events

### General Polish

- [ ] Main menu
- [ ] Pause menu
- [ ] Game over screen
- [ ] Victory screen
- [ ] Transitions between scenes

---

## 📝 Notas

**Prioridad de Polish:**

1. **P0 (Critical):** Visual/audio feedback básico para combate
2. **P1 (High):** Animaciones y efectos de partículas
3. **P2 (Medium):** UI polish y indicators
4. **P3 (Low):** Ambient effects y detalles menores

**Estimación Total:** 4-6 horas para P0-P1 items

**Última actualización:** 31 de Enero, 2026

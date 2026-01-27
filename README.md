# 🦎 CameleonVania

<div align="center">

![Unity](https://img.shields.io/badge/Unity-6.0-black?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=c-sharp)
![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow?style=for-the-badge)

**A 2D Metroidvania where a chameleon transforms into defeated enemies to gain their unique abilities**

[Features](#-features) • [Getting Started](#-getting-started) • [Development](#-development) • [Team](#-team)

</div>

---

## 🎮 About The Game

**CameleonVania** is a 2D Metroidvania platformer developed for the **Global Game Jam 2026**. Play as a chameleon with the unique ability to transform into enemies you defeat, gaining their powers and unlocking new areas of the map.

### 🎯 Core Mechanic

```
Defeat Enemy → Collect Mask → Transform → Gain Abilities → Explore New Areas
```

**Planned Transformations:**

- 🐟 **Fish** - Swim through water sections
- 🕷️ **Spider** - Climb walls and ceilings
- 🐞 **Ladybug** - Fit through small gaps

---

## ✨ Features

### ✅ Implemented (Day 1)

- ✅ **Smooth Player Movement** - Responsive horizontal controls
- ✅ **Precise Jump Mechanics** - Physics-based jumping with ground detection
- ✅ **Professional Camera System** - Cinemachine with dead zones and smooth follow
- ✅ **Optimized Collision System** - Layer-based collision matrix

### 🚧 In Progress (Day 2)

- 🔄 **Health System** - Event-driven health management
- 🔄 **Combat System** - Player attack with enemy detection
- 🔄 **Enemy AI** - State machine-based behavior (Patrol, Chase, Attack)

### 📅 Planned (Days 3-6)

- 📋 **Transformation System** - Core mechanic implementation
- 📋 **Collectibles & Power-ups**
- 📋 **UI/HUD System**
- 📋 **Level Design**
- 📋 **Audio & Polish**

---

## 🚀 Getting Started

### Prerequisites

- **Unity 6.0** or later
- **Git** for version control
- **Visual Studio** or **VS Code** (recommended)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/fabroche/CameleonVania.git
   cd CameleonVania
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Click "Add" → Select the `CameleonVania` folder
   - Open the project

3. **Open the main scene**
   - Navigate to `Assets/Scenes/SampleScene.unity`
   - Press Play ▶️

### Controls

| Action          | Key                     |
| --------------- | ----------------------- |
| Move Left/Right | `A/D` or `Arrow Keys`   |
| Jump            | `Space`                 |
| Attack          | `X` _(Coming in Day 2)_ |

---

## 🛠️ Development

### Project Structure

```
CameleonVania/
├── Assets/
│   ├── Scenes/          # Unity scenes
│   ├── Scripts/         # C# scripts
│   │   ├── Player/      # Player controller and mechanics
│   │   ├── Cameras/     # Camera systems
│   │   ├── Enemies/     # Enemy AI and behavior
│   │   └── Managers/    # Game managers
│   ├── Prefabs/         # Reusable game objects
│   ├── Sprites/         # 2D graphics
│   └── Audio/           # Sound effects and music
├── JamDaysSummary/      # Daily development summaries
├── plan-implementacion.md  # 6-day development plan
├── GDD-GGJ2026.txt      # Game Design Document
└── claude.md            # Development memory and workflow
```

### Git Workflow

We follow a **feature branch workflow**:

```bash
# Create a new feature branch
git checkout -b feature/feature-name

# Make changes and commit
git add .
git commit -m "feat: description"

# Push and create PR
git push -u origin feature/feature-name
gh pr create --title "Feature Title" --body "Closes #X"

# After merge, cleanup
git checkout main
git pull origin main
git branch -d feature/feature-name
```

### Branch Naming Convention

```
feature/  → New functionality
setup/    → Configuration
fix/      → Bug fixes
refactor/ → Code refactoring
```

### Tech Stack

- **Engine:** Unity 6.0
- **Language:** C# 12.0
- **Camera:** Cinemachine
- **Physics:** Unity Physics2D
- **Version Control:** Git + GitHub

---

## 📚 Documentation

- **[Development Plan](plan-implementacion.md)** - 6-day detailed implementation plan
- **[Game Design Document](GDD-GGJ2026.txt)** - Core mechanics and vision
- **[Development Memory](claude.md)** - Workflow, methodology, and guidelines
- **[Daily Summaries](JamDaysSummary/)** - Progress reports for each day

---

## 👥 Team

<table>
  <tr>
    <td align="center">
      <img src="https://github.com/fabroche.png" width="100px;" alt="Frank"/>
      <br />
      <sub><b>Frank (fabroche)</b></sub>
      <br />
      <sub>💻 Programmer</sub>
      <br />
      <sub>🇪🇸 Spanish</sub>
    </td>
    <td align="center">
      <img src="https://via.placeholder.com/100/4A90E2/FFFFFF?text=A" width="100px;" alt="Alfonzo"/>
      <br />
      <sub><b>Alfonzo</b></sub>
      <br />
      <sub>🎨 3D Modeler</sub>
      <br />
      <sub>🇪🇸 Spanish</sub>
    </td>
    <td align="center">
      <img src="https://via.placeholder.com/100/E94B3C/FFFFFF?text=G" width="100px;" alt="Gio"/>
      <br />
      <sub><b>Gio</b></sub>
      <br />
      <sub>🎮 Game Designer</sub>
      <br />
      <sub>🇮🇹 Italian</sub>
    </td>
  </tr>
</table>

**Team Language:** 🇬🇧 English

---

## 📈 Development Progress

### Day 1 ✅ (26 Jan 2026)

- [x] Project setup and layers configuration
- [x] Player horizontal movement
- [x] Jump and ground detection
- [x] Camera follow system

### Day 2 🔄 (27 Jan 2026)

- [ ] Health system with events
- [ ] Player attack system
- [ ] Enemy AI with state machine

### Days 3-6 📅

- [ ] Transformation system (core mechanic)
- [ ] Collectibles and power-ups
- [ ] UI/HUD
- [ ] Level design
- [ ] Audio and polish
- [ ] Final build and submission

---

## 🎯 Milestones

| Milestone        | Target Date | Status         |
| ---------------- | ----------- | -------------- |
| First Playable   | Day 1       | ✅ Complete    |
| Combat System    | Day 2       | 🔄 In Progress |
| Core Mechanic    | Day 3       | 📅 Planned     |
| Alpha Build      | Day 4       | 📅 Planned     |
| Beta Build       | Day 5       | 📅 Planned     |
| Final Submission | Day 6       | 📅 Planned     |

---

## 🔧 Technical Highlights

### Physics System

- **Rigidbody2D** for physics-based movement
- **CapsuleCollider2D** for smooth collisions
- **Layer-based collision matrix** for optimized performance

### Camera System

- **Cinemachine Virtual Camera** for professional camera work
- **Dead zones** for comfortable exploration
- **Smooth damping** for cinematic feel

### Code Quality

- **Component-based architecture** for reusability
- **Event-driven systems** for decoupled code
- **Finite State Machines** for AI behavior
- **Gizmos** for visual debugging

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Global Game Jam 2026** for the opportunity
- **Unity Technologies** for the amazing engine
- **Cinemachine** for professional camera tools
- **Game Jam Advisor Agent** for development guidance

---

## 📞 Contact

**Project Link:** [https://github.com/fabroche/CameleonVania](https://github.com/fabroche/CameleonVania)

**Game Jam:** Global Game Jam 2026

---

<div align="center">

**Made with ❤️ during Global Game Jam 2026**

_"Done is better than perfect"_

</div>

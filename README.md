# Project Zeus

A 2D platformer game built with MonoGame where you play as Adonis on a quest to collect three sacred items and face Zeus in an epic showdown.

![.NET 10](https://img.shields.io/badge/.NET-10.0-blue)
![MonoGame](https://img.shields.io/badge/MonoGame-3.8.4-green)

## 🎮 Game Overview

Project Zeus is a Greek mythology-inspired platformer where players must navigate through challenging levels to collect three items from different locations before confronting Zeus in a final battle.

### Story

As Adonis, you must prove your worth by collecting sacred items from three perilous locations:
- **The Labyrinth** - Navigate a maze while avoiding the Minotaur
- **The Dark Mine** - Journey through treacherous mines filled with carts, stalactites, bats, and a giant bat boss
- **Mount Olympus** - Climb the mountain while dodging rocks thrown by an angry goat

Once all three items are collected and placed in the Pillar Room, you can enter the portal to face Zeus himself.

## 🕹️ Controls

### Keyboard
- **Arrow Keys** or **WASD** - Move left/right
- **Space** or **W/Up Arrow** - Jump
- **E** - Interact (collect items, enter portals, place items)

## 🎯 Levels

### Pillar Room (Hub)
The central hub where you start. Contains three portals leading to different challenges and three pillars where collected items must be placed.

### Maze Level
- Navigate through a labyrinth filled with hedges and vases
- Avoid the patrolling Minotaur who moves in set patterns
- Find the grapes hidden somewhere in the maze
- Return to the starting portal to exit

### Mine Level
- Side-scrolling level with camera following the player
- Dodge mine carts on rails moving toward you
- Avoid stalactites hanging from the ceiling
- Watch out for bats and the GigaBat boss that shoots guano projectiles
- Collect the item at the end and return to the start portal

### Mountain Level
- Vertical climbing level with progressively harder platform jumps
- Moving platforms add extra challenge
- A goat at the summit throws rocks downward
- Reduced jump height increases difficulty
- Reach the top to collect the item

### Zeus Fight Scene
- Final boss encounter (currently in development)
- Triggered after placing all three items in the Pillar Room

## 🏗️ Project Structure

```
project-zeus/
├── ProjectZeus.Core/              # Core game library
│   ├── Constants/                 # Game constants and configuration
│   ├── Entities/                  # Game entities (player, enemies, objects)
│   ├── Game/                      # Game management (SceneManager)
│   ├── Levels/                    # Level implementations
│   ├── Physics/                   # Physics and collision detection
│   ├── Rendering/                 # Rendering helpers and sprite loaders
│   ├── Utilities/                 # Helper utilities
│   └── Content/                   # Game assets
│       └── Sprites/               # Aseprite sprite files
└── ProjectZeus.WindowsDX/         # Windows DirectX runner project
```

## 🛠️ Building and Running

### Prerequisites

- .NET 8 SDK
- MonoGame 3.8.4 or later
- Visual Studio 2026 or Visual Studio Code

### Building

1. Clone the repository:
```bash
git clone https://github.com/hubertklonowski/project-zeus.git
cd project-zeus
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the game:
```bash
cd ProjectZeus.WindowsDX
dotnet run
```

### Building from Visual Studio

1. Open `ProjectZeus.sln`
2. Set `ProjectZeus.WindowsDX` as the startup project
3. Press F5 to build and run

## 🎨 Assets

The game uses Aseprite sprite files (`.aseprite`) for all visual assets:
- **adonis.aseprite** - Player character with 8-frame walking animation
- **zus.aseprite** - Zeus boss character
- **minotaur.aseprite** - Maze level enemy
- **cart.aseprite** - Mine carts
- **bat.aseprite** - Flying bat enemies
- **stalactite.aseprite** - Hanging obstacles
- **goat.aseprite** - Mountain enemy
- **rock.aseprite** - Thrown projectiles
- **grapes.aseprite**, **vase.aseprite**, **hedge.aseprite** - Maze decorations
- **sandtile.aseprite** - Maze floor tiles

## 🎵 Audio

Music playback is supported on Windows platforms. The game includes background music that loops continuously during gameplay.

## 🔧 Technical Details

### Architecture

- **Game Engine**: MonoGame (based on XNA Framework)
- **Target Framework**: .NET 10
- **Platform**: Windows DirectX
- **Sprite System**: Custom Aseprite integration using MonoGame.Aseprite and AsepriteDotNet
- **Physics**: Custom platformer physics with gravity and collision detection

### Key Features

- Multi-scene game state management
- Camera system with smooth following
- Procedural level generation (Mine and Maze)
- Animated sprites with automatic frame selection
- Custom collision detection for platforms and entities
- Item collection and progress tracking system

### Game Constants

Key gameplay values (found in `GameConstants.cs`):
- **Base Screen Size**: 800x480
- **Move Speed**: 180 units/second
- **Jump Velocity**: -560 units/second
- **Gravity**: 900 units/second²
- **Player Size**: 32x48 pixels

## 🐛 Known Issues

- Zeus fight scene is not yet fully implemented
- Some collision detection edge cases may occur
- Performance may vary on lower-end systems

## 🚀 Future Enhancements

- [ ] Complete Zeus boss fight mechanics
- [ ] Add sound effects
- [ ] Implement power-ups and abilities
- [ ] Add more levels and challenges
- [ ] Create a main menu and pause system
- [ ] Add save/load functionality

## 📝 License

This project is provided as-is for educational purposes.

## 🤝 Contributing

This is a personal learning project, but suggestions and feedback are welcome through GitHub issues.

## 👤 Authors

**Hubert Klonowski**
- [@hubertklonowski](https://github.com/hubertklonowski)
- [@tanczacypor](https://imgur.com/W6qYbRO)

## 🙏 Acknowledgments

- Built with [MonoGame](https://www.monogame.net/)
- Sprite handling via [MonoGame.Aseprite](https://github.com/AristurtleDev/MonoGame.Aseprite)
- Inspired by classic platformer games and Greek mythology
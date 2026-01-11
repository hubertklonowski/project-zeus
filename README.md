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

Once all three items are collected and placed in the Pillar Room, you can enter the portal to face Zeus himself in a divine challenge!

## 🕹️ Controls

### Keyboard
- **Arrow Keys** or **WASD** - Move left/right
- **Space** or **W/Up Arrow** - Jump
- **E** - Interact (collect items, enter portals, place items, throw rocks as goat)
- **Q/E** - Cycle through items during Zeus fight
- **Enter** - Confirm item placement during Zeus fight

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
- Epic final boss encounter where you must prove your worth to Zeus
- Place the three sacred items on the sacrifice pillar in the correct order
- Zeus demands items in randomized Greek riddles - choose wisely!
- You have 10 seconds to select and confirm each item using Q/E and Enter
- Three correct answers satisfy the gods and grant you victory
- Wrong answers or running out of time transforms you into a goat
- As a goat, Zeus will hunt you down with powerful stomp attacks
- If Zeus stomps on you, you're transported to the mountain top as the goat
- Throw rocks at climbers below or wait 30 seconds for the credits!

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

- .NET 10 SDK
- MonoGame 3.8.4 or later
- Visual Studio 2022 or Visual Studio Code

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
- **zus.aseprite** - Zeus boss character with stomp animation
- **minotaur.aseprite** - Maze level enemy
- **cart.aseprite** - Mine carts
- **bat.aseprite** - Flying bat enemies
- **stalactite.aseprite** - Hanging obstacles
- **goat.aseprite** - Mountain enemy and player transformation
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
- Dynamic boss fight with multiple outcomes
- Player transformation mechanics
- Timed puzzle-solving challenges

### Game Constants

Key gameplay values (found in `GameConstants.cs`):
- **Base Screen Size**: 800x480
- **Move Speed**: 180 units/second
- **Jump Velocity**: -560 units/second
- **Gravity**: 900 units/second²
- **Player Size**: 32x48 pixels

## 🐛 Known Issues

- Some collision detection edge cases may occur
- Performance may vary on lower-end systems

## 🚀 Future Enhancements

- [ ] Add sound effects for actions and events
- [ ] Implement power-ups and abilities
- [ ] Add more levels and challenges
- [ ] Create a main menu and pause system
- [ ] Add save/load functionality
- [ ] Multiple difficulty settings
- [ ] Leaderboard for speedruns

## 📝 License

This project is provided as-is for educational purposes.

## 🤝 Contributing

This is a personal learning project, but suggestions and feedback are welcome through GitHub issues.

## 👤 Authors
- [@hubertklonowski](https://github.com/hubertklonowski)
- [@tanczacypor](https://imgur.com/W6qYbRO)

## 🙏 Acknowledgments

- Built with [MonoGame](https://www.monogame.net/)
- Sprite handling via [MonoGame.Aseprite](https://github.com/AristurtleDev/monogame-aseprite)
- Inspired by classic platformer games and Greek mythology
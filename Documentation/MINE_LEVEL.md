# Mine Level - How to Play

## Overview
A new mine level has been added to Project Zeus where players can explore a cave environment to collect special items needed for the pillar room.

## Game Flow

1. **Pillar Room** (Main Scene)
   - Start in the pillar room with three pillars
   - Press **M** to enter the Mine Level
   - Goal: Collect items from the mine and place them on the three pillars

2. **Mine Level** (Level 3)
   - Navigate through a cave environment
   - Features:
     - **Stalactites** hanging from the ceiling (decorative)
     - **Torches** providing light (decorative)
     - **Rails** with moving **carts**
     - **Bats** flying around
     - **Pillar Item** to collect
   - Controls:
     - **Arrow keys** or **A/D** to move
     - **Space** or **Up Arrow** to jump
     - **E** to collect the pillar item when near it
   - Objective: Collect the item and reach the exit

3. **Return to Pillar Room**
   - When you reach the exit in the mine level, you return to the pillar room
   - You'll be carrying the collected item
   - Press **E** near a pillar to place the item
   - Once placed, the item cannot be removed

4. **Complete All Pillars**
   - Enter the mine level multiple times if needed (press **M** again)
   - Collect and place items on all three pillars
   - When all three pillars have items, the Zeus Fight Scene begins

## New Features Implemented

### Entities
- **Cart**: Moves back and forth on rails, with visible wheels
- **Bat**: Flies randomly around the cave with animated wings
- **Pillar Item**: Collectible item that hovers and has a glowing effect

### Tiles
- **Stalactite (V)**: Decorative hanging spikes from cave ceiling
- **Torch (T)**: Decorative light source
- **Rail (=)**: Track tiles where carts move
- **Cart (R)**: Spawns a cart that moves on rails
- **Bat (F)**: Spawns a flying bat
- **Pillar Item (I)**: Spawns a collectible pillar item

### Mechanics
- **Item Collection**: Press E when near a pillar item to collect it
- **Item Placement**: Press E when near a pillar (in pillar room) to place the collected item
- **Pillar Locking**: Once an item is placed on a pillar, no other items can be placed there
- **Level Completion**: The mine level can be completed by reaching the exit

## Placeholder Graphics

All graphics are simple placeholder shapes that can be easily replaced:
- **Carts**: Brown rectangles with black circular wheels
- **Stalactites**: Gray triangular spikes pointing downward
- **Torches**: Brown sticks with yellow/orange flame shapes
- **Rails**: Gray horizontal lines with brown wooden ties
- **Pillar Items**: Colored squares with white borders
- **Bats**: Dark purple rectangles with animated wing shapes

## Level Design

The mine level (3.txt) features:
- A starting platform on the left
- Multiple platforms at different heights
- Two sets of rails with moving carts
- Stalactites hanging from the ceiling across the top
- Torches placed strategically for visual interest
- Bats flying in open areas
- A pillar item placed early in the level for collection
- An exit on the right side of the level

The level is designed to be completable - players can jump from platform to platform to reach the exit without requiring advanced platforming skills.

## Technical Details

### New Classes
- `Cart.cs`: Entity that moves on fixed paths
- `Bat.cs`: Flying enemy with random movement
- `PillarItem.cs`: Collectible item for pillar placement

### Modified Classes
- `Level.cs`: Added support for new tile types and entities
- `PlatformerGame.cs`: Added mine level loading and scene management

### Content
- `Levels/3.txt`: Mine level layout
- `Backgrounds/Layer*_3.png`: Cave background layers (currently using placeholders)

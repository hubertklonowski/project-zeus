# Mountain Climbing Level Feature

## Overview
A new mountain climbing level has been added to Project Zeus. Players can enter this level from the pillar room, climb to the top of a mountain while avoiding rocks thrown by a goat, and collect a special item to place in one of the pillar slots.

## Features

### Mountain Level
- **High mountain with platforms**: The mountain features multiple platforms at different heights that players can jump between
- **Placeholder graphics**: All graphics are simple colored rectangles that can be easily replaced:
  - Mountain background: Triangular brown gradient shape
  - Platforms: Brown rectangles with darker outlines
  - Goat: White rectangle with simple facial features (eyes and horns)
  - Rocks: Brown rectangles
  - Item: Gold-colored rectangle with yellow outline

### Goat Enemy
- **Location**: Positioned at the top of the mountain
- **Behavior**: Throws rocks at the player every 2.5 seconds
- **Visual**: Simple white rectangle with black eyes and horn decorations
- **Rocks**: Projectiles thrown downward with slight randomness (70-110 degree angles)

### Item Collection
- **Location**: Next to the goat at the mountain top
- **Collection**: Press 'E' when near the item to collect it
- **Effect**: Item is added to player inventory and player automatically returns to pillar room
- **Placement**: In the pillar room, press 'E' near an empty pillar slot to place the item

### Death & Respawn System
- **Death trigger**: Getting hit by a rock thrown by the goat
- **Respawn location**: Player respawns at the starting position in the pillar room
- **Penalty**: All collected items are lost (inventory cleared and pillar items removed)

### Portal System
- **Location**: Right side of the pillar room (purple rectangle with "M" label)
- **Activation**: Press 'E' when standing in the portal
- **Entry**: Teleports player to the bottom of the mountain
- **Exit**: Walk to the left edge of the screen (after collecting item) or collect the item to return

## Controls
- **Movement**: Arrow keys or A/D keys
- **Jump**: Space or Up arrow
- **Interact**: E key (for picking up items, entering portal, placing items in pillars)

## Game Flow

### Normal Flow - Step by Step Walkthrough
1. Start in the pillar room with three empty pillar slots
2. Walk right to the purple portal (marked with "M")
3. Stand in portal and press 'E' to enter mountain level
4. Player spawns at bottom-left of mountain on the ground
5. Jump onto the first platform at height 400 (Level 1)
6. Continue jumping between platforms - there are multiple valid paths:
   - **Left path**: 100→150→100→150→250→top
   - **Middle path**: 280→340→300→340→top
   - **Right path**: 480→520→500→520→430→top
7. Each vertical gap is 60-80 pixels (max jump = 174 pixels) ✓
8. Each horizontal gap is 100-200 pixels (max jump distance = 223 pixels) ✓
9. Rocks fall from the goat every 2.5 seconds - dodge by moving side-to-side or waiting on lower platforms
10. Reach the top platform (200 pixels wide, plenty of space)
11. Navigate to the right side of the platform where the golden item is
12. Press 'E' to collect the item (goat is on the left side, 90 pixels away)
13. Automatically return to pillar room with item in inventory
14. Walk to one of the three pillar slots
15. Press 'E' near the slot to place the item
16. Repeat steps 2-15 two more times to collect all three items
17. When all three items are placed, the Zeus fight scene begins

### Death Flow
1. Start in the pillar room with three empty pillar slots
2. Enter the purple portal on the right by pressing 'E'
3. Climb the mountain platforms while avoiding rocks
4. Reach the top and press 'E' near the golden item to collect it
5. Automatically return to pillar room
6. Place the item in one of the three pillar slots by pressing 'E' near it
7. Repeat for the other two items (portal can be used multiple times)
8. When all three items are placed, the Zeus fight scene begins

### Death Flow
1. If hit by a rock in the mountain level:
   - Player immediately respawns in pillar room
   - All inventory items are cleared
   - All pillar slots are emptied
   - Must start over collecting items

## Testing Instructions

### Test 1: Portal Entry
1. Run the game
2. Walk to the right side of the pillar room
3. Stand in the purple portal rectangle
4. Press 'E'
5. **Expected**: Player teleports to the bottom of the mountain level

### Test 2: Mountain Climbing
1. Enter the mountain level via portal
2. Use jump (Space) and movement (A/D or arrows) to climb platforms
3. Navigate upward through all platforms
4. **Expected**: Player can successfully reach the top platform where the goat is

### Test 3: Goat Rock Throwing
1. Enter the mountain level
2. Observe the goat at the top of the mountain
3. Wait and watch for rocks falling
4. **Expected**: Goat throws rocks downward every 2.5 seconds with slight angle variation

### Test 4: Death and Respawn
1. Enter the mountain level
2. Get hit by a rock (or wait near bottom for rocks to fall)
3. **Expected**: 
   - Player immediately respawns in pillar room at starting position
   - All inventory is cleared
   - All pillar items are removed

### Test 5: Item Collection
1. Enter the mountain level
2. Successfully climb to the top platform
3. Stand near the golden item (next to the goat)
4. Press 'E'
5. **Expected**:
   - Item disappears
   - Player returns to pillar room
   - Inventory indicator appears (golden square in top-left)
   - Message shows "Item collected! Place it in a pillar."

### Test 6: Item Placement
1. Collect an item from the mountain (Test 5)
2. In the pillar room, walk to one of the three pillars
3. Position player near the blue slot at the top of a pillar
4. Press 'E'
5. **Expected**:
   - Item appears in the pillar slot (colored rectangle)
   - Inventory indicator disappears
   - Item remains in the pillar even after entering/exiting mountain level

### Test 7: Multiple Items
1. Collect and place one item (Tests 5 & 6)
2. Return to the portal and enter mountain level again
3. Collect another item
4. Place it in a different pillar slot
5. Repeat for the third item
6. **Expected**:
   - Each pillar can hold one item
   - Each item has a different color (Gold, DeepSkyBlue, MediumVioletRed)
   - After all three items are placed, Zeus fight scene begins

### Test 8: Death Penalty
1. Collect one or more items and place them in pillars
2. Enter the mountain level
3. Deliberately get hit by a rock
4. **Expected**:
   - Player respawns in pillar room
   - All placed pillar items are removed
   - Must collect all items again from scratch

## Implementation Details

### Files Modified
- `ProjectZeus.Core/PlatformerGame.cs`: Added portal, inventory, and scene management
- `ProjectZeus.Core/MountainLevel.cs`: New file containing mountain level logic

### Key Classes
- **MountainLevel**: Manages the mountain scene, platforms, goat, rocks, and item
- **Platform** (nested): Represents a climbable platform
- **Rock** (nested): Represents a thrown projectile

### Scene States
- `inMountainLevel`: Boolean flag indicating if player is in mountain level
- `hasItemFromMountain`: Boolean flag tracking if player has collected the mountain item
- `mountainLevel.ItemWasCollected`: Flag for triggering auto-return to pillar room
- `mountainLevel.PlayerDied`: Flag for triggering respawn

### Collision Detection
- Platform collision: Top-surface AABB detection with falling velocity check
- Rock collision: Rectangle intersection between rock and player
- Item collection: Rectangle intersection with expanded interaction zone
- Portal interaction: Rectangle intersection check

## Future Enhancement Opportunities
All graphics are implemented as simple placeholder colored rectangles that can be easily replaced with proper sprites:

1. **Mountain**: Replace triangular gradient with mountain sprite/texture
2. **Goat**: Replace rectangle with animated goat sprite
3. **Rocks**: Replace rectangles with rock sprites
4. **Platforms**: Replace rectangles with platform textures
5. **Item**: Replace gold rectangle with custom item sprite
6. **Portal**: Replace purple rectangle with animated portal effect
7. **Particle Effects**: Add effects for rock impacts, item collection
8. **Sound Effects**: Add sounds for goat throwing, rock impacts, item pickup
9. **Animation**: Add goat throwing animation, rock rotation
10. **Difficulty**: Add multiple goats, faster throw rates, or moving platforms

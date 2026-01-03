# Visual Layout of Mountain Level

```
Screen: 800x480 pixels

                     🐐 Goat (white rect)    ⭐ Item (gold rect)
                     ├────────────┬──────────┤
                     │            │          │
                    Y=60         Y=60       Y=70
Top Platform:   ════════════════════════════════  Y=100 (200px wide)
                     ^                    ^
                     │                    │
                 Goat here            Item here


Near Top:       ══════════════          ══════════════  Y=130
                    (120px)                  (120px)


Upper:          ══════════    ══════════════  ══════════  Y=180-190
                   (110px)        (120px)        (110px)


Middle:         ══════════════  ════════════════  ══════════════  Y=250-260
                    (120px)         (130px)           (120px)


Mid-Low:        ══════════    ══════════════      ══════════  Y=320-330
                   (110px)        (120px)            (110px)


Low:            ══════════════  ══════════    ══════════════  Y=390-400
                    (120px)        (110px)        (120px)


Ground:         ═══════════════════════════════════════════  Y=460-480
                              (800px wide)

Player spawns here →  👤

Legend:
═══  Platform (brown rectangles)
🐐   Goat (white with eyes and horns)
⭐   Collectible Item (gold rectangle)
👤   Player spawn point
```

## Three Climbing Paths

### Left Path (Easy)
```
Ground → Platform(100,400) 
      → Platform(150,330) 
      → Platform(100,260) 
      → Platform(150,190) 
      → Platform(250,130) 
      → Top Platform(300,100)
```

### Middle Path (Moderate)
```
Ground → Platform(280,390) 
      → Platform(340,320) 
      → Platform(300,250) 
      → Platform(340,180) 
      → Top Platform(300,100)
```

### Right Path (Balanced)
```
Ground → Platform(480,400) 
      → Platform(520,330) 
      → Platform(500,260) 
      → Platform(520,190) 
      → Platform(430,130) 
      → Top Platform(300,100)
```

## Portal Location in Pillar Room

```
Pillar Room (800x480):

                    "Insert the three items of Zeus"
                    
                    
        ┌──┐                ┌──┐                ┌──┐
        │  │                │  │                │  │        ┌─────┐
        │  │                │  │                │  │        │  M  │ ← Portal
        │  │                │  │                │  │        │     │   (purple)
        │  │                │  │                │  │        └─────┘
        └──┘                └──┘                └──┘
      Pillar 1            Pillar 2            Pillar 3
      
  👤                                                           
Player                                                   
spawn
═══════════════════════════════════════════════════════════════
                        Ground
```

## Rock Throwing Mechanic

```
                         🐐 Goat
                         │
                         │ Throws every 2.5 seconds
                         ▼
                        ●  Rock (20x20 brown rect)
                       ╱ ╲
                      ╱   ╲  Random angle (70-110°)
                     ╱     ╲
                    ●       ●
                   ╱         ╲
                  ╱           ╲
                 ●             ●
                ╱               ╲
               ╱                 ╲
              ●                   ●
          Player can               Player can
          dodge left               dodge right
```

## Game Flow Diagram

```
┌─────────────┐
│ Pillar Room │
│   (Start)   │
└──────┬──────┘
       │ Press E at Portal
       ▼
┌─────────────┐
│  Mountain   │
│   Level     │
└──────┬──────┘
       │
       ├─── Climb Platforms ───┐
       │                       │
       ├─── Dodge Rocks ───────┤
       │                       │
       ▼                       ▼
   Hit by Rock?          Reach Top?
       │                       │
       │ YES                   │ YES
       ▼                       ▼
┌─────────────┐         ┌─────────────┐
│   DEATH     │         │ Collect Item│
│  Respawn in │         │  (Press E)  │
│ Pillar Room │         └──────┬──────┘
│ Lose Items  │                │ Auto-return
└─────────────┘                ▼
                        ┌─────────────┐
                        │ Pillar Room │
                        │ With Item   │
                        └──────┬──────┘
                               │ Press E near pillar
                               ▼
                        ┌─────────────┐
                        │ Place Item  │
                        │ in Pillar   │
                        └──────┬──────┘
                               │ Repeat 3 times
                               ▼
                        ┌─────────────┐
                        │ Zeus Fight  │
                        │    Scene    │
                        └─────────────┘
```

## Collision Detection

### Platform Collision
```
Player falling down:
     ┌──┐
     │░░│ ← Player (32x48)
     └──┘
      ↓ Velocity.Y > 0
═════════════ ← Platform top surface
     Platform
     
Collision zone: 6 pixels tall, 2 pixels above platform
If player.Bottom intersects collision zone → Land on platform
```

### Rock Collision
```
  ┌──┐
  │░░│ ← Player
  └──┘
    
    ●  ← Rock (20x20)
    
If rectangles intersect → Player dies
```

## Color Scheme (All Placeholder)

| Element          | Color                    | RGB          |
|------------------|--------------------------|--------------|
| Sky              | Sky Blue                 | 135,206,235  |
| Mountain         | Brown gradient           | 160,140,120  |
| Platforms        | Light Brown              | 120,100,80   |
| Ground           | Dark Brown               | 100,80,60    |
| Goat             | White                    | 255,255,255  |
| Goat Eyes        | Black                    | 0,0,0        |
| Goat Horns       | Off-White                | 220,220,200  |
| Rocks            | Dark Brown               | 80,70,60     |
| Item             | Gold                     | 255,215,0    |
| Portal           | Purple                   | 100,50,150   |
| Player           | Tan                      | 255,220,180  |

All colors are easy to replace with actual sprite textures.

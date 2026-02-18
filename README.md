# Physics Brawler — Phase 1 Prototype

2-player local physics brawler built in Unity 2D.

## Quick Setup

### Requirements
- Unity **2022 LTS** or newer (2022.3.x recommended)
- 2D packages (included via `Packages/manifest.json`)

### Steps

1. **Open the project** in Unity Hub → "Add project from disk" → select this folder.

2. Wait for Unity to import and compile scripts.

3. In the Unity menu bar click **Brawler → Setup Game Scene**.
   - This creates all GameObjects, prefabs, and saves `Assets/Scenes/GameScene.unity`.

4. Open `Assets/Scenes/GameScene.unity` (double-click in Project window).

5. Press **Play** ▶.

---

## Controls

| Action | Player 1 | Player 2 |
|--------|----------|----------|
| Move left | `A` | `←` |
| Move right | `D` | `→` |
| Jump | `W` | `↑` |
| Punch | `S` | `↓` |
| Shoot (needs gun) | `F` | `Right Ctrl` |

---

## Scene Layout

```
         [PlatformCenter  0, 1]

  [PlatformLeft -5,-1.5]   [PlatformRight 5,-1.5]

P1(-3,0)  [Gun(0,0.5)]  P2(3,0)

        [Ground  0, -4  (20 wide)]

        [DeathZone  0, -7]
```

---

## Project Structure

```
Assets/
  Scripts/
    PlayerMovement.cs   — WASD/Arrow movement + jump
    PlayerCombat.cs     — Punch with knockback (S / ↓)
    PlayerRespawn.cs    — Respawn on death zone touch
    PlayerShooting.cs   — Fire bullets when holding gun
    GunPickup.cs        — Gun pickup logic + drop
    Bullet.cs           — Bullet hit + knockback
    DeathZone.cs        — Trigger respawn on fall
  Editor/
    SetupScene.cs       — One-click scene builder (Editor-only)
  Prefabs/
    Bullet.prefab       — (created by SetupScene)
    Gun.prefab          — (created by SetupScene)
  Scenes/
    GameScene.unity     — (created by SetupScene)
ProjectSettings/
  TagManager.asset      — Defines Player / Ground / DeathZone tags
Packages/
  manifest.json         — Unity 2D package dependencies
```

---

## Done When

- [x] Both players move and jump independently on the same keyboard
- [x] Players fall with gravity and land on platforms
- [x] Falling off the bottom respawns the player at their start position
- [x] Punching a nearby player sends them flying
- [x] One gun spawns in the level, can be picked up by walking into it
- [x] Shooting fires a bullet that knocks back whoever it hits
- [x] Shooting pushes the shooter backward (recoil)

---

## Phase 2 (not yet implemented)

- Health / lives / score system
- Round management and win condition
- Sound effects
- Animations
- More weapons / levels / menus

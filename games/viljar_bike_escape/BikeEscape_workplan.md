# Robin's Great Escape — Workplan

Top-down 2D game. Robin bikes from old office to new office (~3km apart).
Ex-colleagues chase him. Water = swap to canoe/SUP. Reach the new office to win.

---

## Office coordinates

| | Lat | Lon |
|--|-----|-----|
| **Old office (start)** | 60.378289 | 5.329812 |
| **New office (goal)** | 60.3568672 | 5.3426693 |
| **Distance** | ~2.5 km | Bergen, Norway |

---

## Map approach

**Decision: Static stitched OSM image as background.**

Easiest approach for a party game — no tile loading, no runtime API calls.

### Bounding box (with ~1km padding)

```
Min lat (south): 60.3479
Max lat (north): 60.3873
Min lon (west):  5.3116
Max lon (east):  5.3609
```

OSM export URL (set these values in the export sidebar):
`https://www.openstreetmap.org/export#map=14/60.3676/5.3363`

### How to get the map

Option A — OSM export page (simplest, lower res):
1. Open the OSM export URL above
2. Manually enter the bounding box values in the sidebar
3. Export as PNG (~2000px, good enough for a jam game)

Option B — tile stitcher (higher res):
```bash
# install: pip install stitch-osm-tiles
python stitch-osm-tiles.py \
  --bbox 5.3116 60.3479 5.3609 60.3873 \
  --zoom 16 \
  --output assets/map.png
```
Produces ~6000×6000px — readable street names, ~30MB PNG.

### Sprite positions in the image (6000×6000px export)

These are approximate pixel coords for placing Robin's start and the goal marker:

| Point | X (px) | Y (px) |
|-------|--------|--------|
| Old office (start) | ~2214 | ~1374 |
| New office (goal)  | ~3780 | ~4632 |

**Water detection (simple):** Sample pixel color under the player. OSM water tiles render as `#aad3df` (blue). Color-threshold check in `_PhysicsProcess` → swap sprite + movement speed.

Alternative (even simpler): manually paint a second "collision layer" image — white = land, blue = water, red = goal. Sample it at player position. Tiny, fast, predictable.

---

## Scenes

```
BikeEscape.tscn       ← main scene, Node2D root
  MapBackground       ← TextureRect with the OSM image
  Robin               ← CharacterBody2D (player)
  ChasersGroup        ← Node2D, spawns 3-5 chasers
  GoalMarker          ← Area2D at new office position
  HUD                 ← CanvasLayer (timer, distance remaining)
  CollisionMask       ← hidden Sprite2D, sampled for terrain type
```

---

## Scripts

| File | Responsibility |
|------|----------------|
| `BikeEscape.cs` | Root game node, win/lose logic, score saving |
| `Robin.cs` | Player movement (bike vs canoe), sprite swap |
| `Chaser.cs` | Simple AI: steer toward Robin, NavAgent or manual steering |
| `TerrainSampler.cs` | Static helper — sample pixel from collision mask image |

---

## Mechanics

### Movement
- **Bike:** WASD/arrows, moderate speed, can go on roads
- **Canoe:** slower top speed, no friction on land (can't go on land)
- Terrain sampled every physics frame from the hidden mask image
- Transition: play a short "splash" animation, swap sprite, change movement params

### Chasers
- 3–5 ex-colleagues, start at the old office
- Simple seek behavior toward Robin (Vector2.DirectionTo)
- Slightly slower than Robin on roads, same speed on water (they can swim apparently)
- Caught = game over, show score = distance reached in meters

### Win condition
- Robin reaches the GoalArea2D at the new office
- Score = time taken (lower = better) or distance/seconds ratio

### Score saving
```csharp
private void SaveScore(int seconds)
{
    var data = new Godot.Collections.Dictionary
    {
        { "game", "viljar_bike_escape" },
        { "player", "Viljar" },
        { "score", seconds }
    };
    DirAccess.MakeDirRecursiveAbsolute("user://scores");
    using var file = FileAccess.Open("user://scores/viljar_bike_escape.json", FileAccess.ModeFlags.Write);
    file.StoreString(Json.Stringify(data));
}
```

---

## Characters

### Chasers
| Name | Concept | Notes |
|------|---------|-------|
| **Siggurd** | Red Cross worker. On top of a mountain in an ambulance-type car | Comes screaming down? High speed vehicle. Details TBD |
| **Yuval** | Trackmania car | Special power: bugslide — slides/drifts around corners at high speed |
| **Are** | Camps in a tent on the mountain to the south-west | Throws disc golf discs at Robin. Starts stationary at camp, then begins launching discs when Robin gets close enough |
| **Viljar** | TBD | Concept TBD |
| **Trygve** | Kia EV3 | Chases Robin in his Kia EV3 |
| **Dag** | TBD | Concept TBD — may not be used |
| **Jonathan** | TBD | Concept TBD — may not be used |
| **Nils** | TBD | Concept TBD — may not be used |
| **Guy** | TBD | Concept TBD — may not be used |
| **Vegard** | TBD | Concept TBD — may not be used |

### Allies / NPCs
| Name | Concept | Notes |
|------|---------|-------|
| **Nico** | Friendly colleague at Tidsbanken (Conrad Mohrs veg 11). | Special power: pauses the timer — fitting since he works at *Tids*banken. Spawns at goal position (845, -1). Appears in the WIN screen if Robin makes it. |

---

## Assets needed

- [ ] OSM map image (see above — export manually, drop into `assets/map.png`)
- [ ] Collision mask image (paint in any tool — same dimensions as map)
- [ ] Robin bike sprite (can reuse from `shared/characters/robin/` or draw a tiny top-down one)
- [ ] Robin canoe/SUP sprite
- [ ] Chaser sprite (stick figure or head icon will do)
- [ ] Goal marker (flag or office icon)

---

## Step-by-step build order

1. **Get the map image** — export from OSM, crop to bounding box, save as `assets/map.png`
2. **Paint collision mask** — copy map, paint water blue, goal red, everything else white, save as `assets/collision_mask.png`
3. **Scaffold scene** — create `BikeEscape.tscn`, add TextureRect for map, Camera2D following Robin
4. **Player movement** — `Robin.cs` with basic WASD, Camera2D attached, test movement over map
5. **Terrain sampling** — `TerrainSampler.cs`, test water/land switching
6. **Chasers** — `Chaser.cs`, spawn 3 at start, basic seek AI
7. **Win/lose** — GoalArea2D triggers win, chaser overlap triggers lose
8. **HUD** — distance label + timer
9. **Score + return** — save score, call `ReturnToLauncher()`
10. **Polish** — sounds, splash anim, title card

---

## Open questions

- Do we want Robin to follow roads only, or free movement?
  → Free movement is way simpler. Roads-only needs a navmesh.

---

## Backlog / TODO

### Sprites
- [ ] Replace placeholder circles with proper sprites
  - Robin: top-down bike or running man sprite
  - Chasers: one sprite per colleague (can be a small photo/avatar or illustrated head)
  - Find free assets online (itch.io, OpenGameArt) or draw simple top-down figures
  - Water version of Robin: swap to canoe/SUP sprite when IsOnWater

### Chasers — personality & dialogue
- [ ] Each chaser says random lines as they chase — speech bubble pops up near them
  - Trigger: every N seconds, pick a random line from a per-chaser list
  - Display as a Label above the chaser that fades out after ~2s
  - Lines should be in-character (each colleague has their own vibe)

### Caught screen
- [ ] When Robin is caught, show a proper "caught" screen
  - Display which chaser caught him (name + image/photo)
  - Each chaser has an associated image in `assets/chasers/`
  - Could add a funny caption per chaser

### Chaser collision
- [ ] Chasers currently stack on top of each other — add separation
  - Simple fix: each chaser steers away from other chasers when too close
  - Add a "separation force" in Chaser._PhysicsProcess: push away from nearby chasers

### Powerups / trigger zones
- [ ] Area2D trigger zones on the map for special events
  - **Pepsi store** — Robin bikes through, gets a speed boost for ~5s
  - Could add more: coffee shop (faster), construction (slower), etc.
  - Use a separate `Powerup.cs` script on Area2D nodes placed on the map

### Map
- [ ] Replace temp_map with a proper OSM tile-stitched export
  - Use zoom 15–16, bounding box from workplan
  - After replacing: re-tune Robin/chaser start positions and GoalPosition
  - Re-calibrate water detection color if new map uses different tile style

### Music & sound
- [ ] Add silly background music (loop)
  - Free sources: OpenGameArt, itch.io, freesound.org
  - AudioStreamPlayer node on BikeEscape root, autoplay
- [ ] Sound effects: caught sting, win jingle, powerup pickup, splash

### Known bugs / quirks
- Water detection triggers incorrectly on temp map (false positives due to map colors)
  → Will self-correct once proper OSM map is in place
  → If still wrong: use a hand-painted collision mask instead of pixel sampling

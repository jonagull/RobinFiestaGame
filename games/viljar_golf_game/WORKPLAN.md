# Viljar Golf Game — Workplan

## What this game is
Hit golf balls at images of Robin (photo cutouts). He falls over like a cardboard standee when you hit him.
5 shots per round, score = number of hits.

---

## How to play (current)
1. Yellow aim line sweeps left/right automatically — **Space** to lock aim
2. Power bar fills up — **Space** to lock power
3. Golfer swings, ball flies into the screen (gets smaller, parabolic arc)
4. If ball lands on Robin → hitmarker appears, Robin falls backward
5. 5 shots, final score shown

---

## Current file structure
```
games/viljar_golf_game/
  game_info.json          ← launcher metadata
  GolfGame.tscn           ← full scene (golfer, ball, aim line, Robin target, UI)
  GolfGame.cs             ← all game logic
  RobinCutout.gdshader    ← dark-background removal shader (unused now, keep)
  WORKPLAN.md             ← this file
  assets/
    robin/                ← Robin cutout PNGs (removebg)
      DSC09016-removebg-preview.png   ← golf cap, pointing (best reaction)
      DSC00920-removebg-preview.png   ← neutral face
      DSC04104-2-removebg-preview.png ← running (moving target)
      DSC00958-2-removebg-preview.png ← steering wheel (moving target)
    hit_marker.png        ← MW2-style hitmarker
    Dinky_Tiny_Golf_Free/ ← golf asset pack (swing sprites, ball, etc.)
```

---

## Round structure
| Round | Photo | Moves? |
|-------|-------|--------|
| 1 | Pointing (DSC09016) | No — close |
| 2 | Neutral face (DSC00920) | No — medium |
| 3 | Running (DSC04104) | Yes — medium |
| 4 | Steering wheel (DSC00958) | Yes — medium |
| 5 | Pointing (DSC09016) | Yes — far |

Distance tiers: rounds 1–2 close (Y≈380), 3–4 medium (Y≈300), 5 far (Y≈230).

---

## Development history
- **Scaffold** — game_info.json, blank scene with back button
- **Driving range v1** — power bar, ball flies, distance shown, Robin reaction popup
- **Two-click mechanic** — power up to top, then timing back to bottom
- **Pivot to target game** — hit Robin with golf balls instead of distance game
- **Back-view perspective** — golfer centered at bottom, Robin in distance, ball flies up screen
- **Ball trajectory** — parametric arc (sin curve), perspective scale shrink
- **Hit detection** — 2D screen-space circle + pixel-perfect alpha check
- **Hit animation** — Robin falls backward like a cardboard standee (Y scale collapses)
- **Hitmarker** — custom PNG, appears on Robin on hit, stays until next round
- **Pixel-perfect hit** — samples Robin texture alpha at ball landing point

---

## What's working well
- Aim + power two-click mechanic feels good
- Ball arc looks convincing
- Robin falling backward reads clearly
- Hitmarker is satisfying

---

## Known issues / things to improve
- [ ] Robin photos still needed as PNGs: DSC03208 (smug), DSC01043 (laughing suit)
- [ ] Hit radius (90px circle) may still feel slightly off — tweak if needed
- [ ] Pixel-perfect detection requires PNG import as Lossless in Godot import settings
- [ ] Power bar has no visual sweetspot indicator
- [ ] No sound effects yet
- [ ] Final score screen is just text — could show a Robin reaction photo
- [ ] Background is plain black — a background image would help a lot
- [ ] Golfer sprite is side-view (from the pack), not true back-view
- [ ] Robin's initial lean (-0.08 rad) is subtle — true 3D angle needs a shader

## Next steps (pick any)
1. Add a background image / environment
2. Add sound (swing, hit, miss)
3. Score screen with Robin reaction photo
4. More Robin photos = more variety
5. Tune hit detection feel

# sprites/

Place your custom PNG files here. The game loads them automatically.
Missing files get a **coloured placeholder square with an ✕** so you can see
what's missing at a glance. All images should use a **transparent background**.

---

## Dimensions quick-reference

| Category | Recommended size | Notes |
|---|---|---|
| Plants (grid) | **100×100** | Square, centred on cell |
| Seed packet icons (HUD) | **64×64** | Shown in the card at 44×44 px |
| Bloons (normal) | **64×64** | Drawn at 44×44 px (radius 22) |
| MOAB bloon | **128×64** | Landscape, wider than tall |
| Pea projectiles | **32×32** | Drawn at 16×16 px |
| Spike / Thorn projectiles | **32×16** | Drawn at 28×14 / 24×12 px |
| Sun drop / Sun blast | **64×64** | |

> Any PNG size works — bigger = crisper. Stick to powers of 2 (32, 64, 128).

---

## Plants — base sprites

| File | Used for |
|---|---|
| `peashooter.png` | Peashooter at 0-0-0 |          DONE
| `sunflower.png`  | Sunflower at 0-0-0 |       DONE
| `wallnut.png`    | Wallnut at 0-0-0 | done
| `cactus.png`     | Cactus at 0-0-0 |

---

## Plants — Tier 1 upgrade overlays

These are drawn **on top of the base sprite** when that path is at tier 1.
Multiple overlays stack (e.g. paths 0 and 2 both at T1 = two overlays shown).
Draw only the *additional* art (e.g. a hat, a glow) on a transparent background
so they composite cleanly.

| File | Plant | Path | Upgrade name |
|---|---|---|---|
| `peashooter_t1_0.png` | Peashooter | 0 | Repeater |                  DONE 
| `peashooter_t1_1.png` | Peashooter | 1 | Fire Pea |                  DONE
| `peashooter_t1_2.png` | Peashooter | 2 | Enhanced Sight |           DONE
| `sunflower_t1_0.png`  | Sunflower  | 0 | Faster Sun |     DONE
| `sunflower_t1_1.png`  | Sunflower  | 1 | Supporting Growth |      DONE
| `sunflower_t1_2.png`  | Sunflower  | 2 | Solar Power |      DONE
| `wallnut_t1_0.png`    | Wallnut    | 0 | Tough-nut |
| `wallnut_t1_1.png`    | Wallnut    | 1 | Endurian |
| `wallnut_t1_2.png`    | Wallnut    | 2 | Explode-o-nut |
| `cactus_t1_0.png`     | Cactus     | 0 | Heat-tipped Spikes |
| `cactus_t1_1.png`     | Cactus     | 1 | Thorns |
| `cactus_t1_2.png`     | Cactus     | 2 | Razor Sharp Spikes |

---

## Plants — Tier 2 crosspath sprites

When a plant reaches **Tier 2**, the base sprite and all T1 overlays are
**replaced entirely** by one crosspath sprite that represents that exact
upgrade combination. Draw the full plant art (not just an overlay).

### Filename format
```
{plant}_t2_{t2_path}.png           ← T2 with no crosspath
{plant}_t2_{t2_path}_x{x}.png     ← T2 with T1 crosspath on path x
```

### Peashooter T2 sprites

| File | T2 path | Crosspath | Description |
|---|---|---|---|
| `peashooter_t2_0.png`    | 0 (Gatling Pea)     | —        | Pure fire-rate |                  DONE
| `peashooter_t2_0_x1.png` | 0 (Gatling Pea)     | 1 (Fire) | Gatling + Fire Pea |              DONE
| `peashooter_t2_0_x2.png` | 0 (Gatling Pea)     | 2 (Sight)| Gatling + Enhanced Sight |        DONE
| `peashooter_t2_1.png`    | 1 (Explosive Peas)  | —        | Pure explosive |                  DONE
| `peashooter_t2_1_x0.png` | 1 (Explosive Peas)  | 0 (Rate) | Explosive + Repeater |              DONE
| `peashooter_t2_1_x2.png` | 1 (Explosive Peas)  | 2 (Sight)| Explosive + Sight |                  DONE
| `peashooter_t2_2.png`    | 2 (Threepeater)     | —        | Pure three-lane |                  DONE
| `peashooter_t2_2_x0.png` | 2 (Threepeater)     | 0 (Rate) | Threepeater + Repeater |          DONE
| `peashooter_t2_2_x1.png` | 2 (Threepeater)     | 1 (Fire) | Threepeater + Fire Pea |         DONE

### Sunflower T2 sprites

| File | T2 path | Crosspath | Description |
|---|---|---|---|
| `sunflower_t2_0.png`    | 0 (Twin Sunflowers)   | — | |       DONE
| `sunflower_t2_0_x1.png` | 0 (Twin Sunflowers)   | 1 | |   done
| `sunflower_t2_0_x2.png` | 0 (Twin Sunflowers)   | 2 | |   done
| `sunflower_t2_1.png`    | 1 (Photosynthesis)    | — | |   done
| `sunflower_t2_1_x0.png` | 1 (Photosynthesis)    | 0 | |   done
| `sunflower_t2_1_x2.png` | 1 (Photosynthesis)    | 2 | |   done
| `sunflower_t2_2.png`    | 2 (Magnifying Grass)  | — | |   done
| `sunflower_t2_2_x0.png` | 2 (Magnifying Grass)  | 0 | |      done
| `sunflower_t2_2_x1.png` | 2 (Magnifying Grass)  | 1 | |   done

### Wallnut T2 sprites

| File | T2 path | Crosspath | Description |
|---|---|---|---|
| `wallnut_t2_0.png`    | 0 (Tall-nut)       | — | |    done
| `wallnut_t2_0_x1.png` | 0 (Tall-nut)       | 1 | |    done
| `wallnut_t2_0_x2.png` | 0 (Tall-nut)       | 2 | |    done
| `wallnut_t2_1.png`    | 1 (Super Endurian) | — | |    done
| `wallnut_t2_1_x0.png` | 1 (Super Endurian) | 0 | |    done
| `wallnut_t2_1_x2.png` | 1 (Super Endurian) | 2 | |    done
| `wallnut_t2_2.png`    | 2 (Torchwood)      | — | |    done
| `wallnut_t2_2_x0.png` | 2 (Torchwood)      | 0 | |    done
| `wallnut_t2_2_x1.png` | 2 (Torchwood)      | 1 | |    done

### Cactus T2 sprites

| File | T2 path | Crosspath | Description |
|---|---|---|---|
| `cactus_t2_0.png`    | 0 (Headshot!)           | — | |
| `cactus_t2_0_x1.png` | 0 (Headshot!)           | 1 | |
| `cactus_t2_0_x2.png` | 0 (Headshot!)           | 2 | |
| `cactus_t2_1.png`    | 1 (Garden of Thorns)    | — | |
| `cactus_t2_1_x0.png` | 1 (Garden of Thorns)    | 0 | |
| `cactus_t2_1_x2.png` | 1 (Garden of Thorns)    | 2 | |
| `cactus_t2_2.png`    | 2 (Super-Charged Spikes)| — | |
| `cactus_t2_2_x0.png` | 2 (Super-Charged Spikes)| 0 | |
| `cactus_t2_2_x1.png` | 2 (Super-Charged Spikes)| 1 | |

---

## Seed-packet icons (HUD cards)

| File | Size |
|---|---|
| `icon_peashooter.png` | 64×64 |       DONE
| `icon_sunflower.png`  | 64×64 |   done
| `icon_wallnut.png`    | 64×64 |   done
| `icon_cactus.png`     | 64×64 |

---

## Bloons

| File | Size | Notes |
|---|---|---|
| `bloon_red.png`     | 64×64  | |  DONE
| `bloon_blue.png`    | 64×64  | |  DONE
| `bloon_yellow.png`  | 64×64  | |  DONE
| `bloon_pink.png`    | 64×64  | |  DONE
| `bloon_lead.png`    | 64×64  | |  DONE
| `bloon_lead_shield.png`    | 64×64  | |  DONE
| `bloon_ceramic.png` | 64×64  | |  DONE
| `bloon_camo.png`    | 64×64  | |  DONE
| `bloon_moab.png`    | 128×64 | Landscape blimp shape |   DONE

---

## Projectiles

| File | Size |
|---|---|
| `pea.png`          | 32×32 |      DONE
| `pea_fire.png`     | 32×32 |        DONE
| `pea_plasma.png`   | 32×32 |      Done      
| `spike_normal.png` | 32×16 |          
| `spike_heat.png`   | 32×16 |        
| `spike_plasma.png` | 32×16 |      
| `thorn_normal.png` | 32×16 |
| `thorn_plasma.png` | 32×16 |
| `sun_blast.png`    | 32×32 |  done
| `sun_laser.png`    | 32×32 |  done

---

## Sun drop

| File | Size |
|---|---|
| `sun_drop.png` | 64×64 |              DONE

---

## Lawn area backdrops

These fill the sidebar strips to the left and right of the playable grid.
Drop in a PNG and the game uses it automatically; missing files show a coloured placeholder.

| File | Recommended size | Drawn at | Notes |
|---|---|---|---|
| `house_area.png` | **200×625** | `GRID_X × (GRID_ROWS × CELL_H)` = 200×625 px | Left sidebar (house / home base art) |
| `spawn_area.png` | **90×625**  | `(SCREEN_W − GRID_X − GRID_COLS×CELL_W) × 625` ≈ 90×625 px | Right strip where bloons enter |

> Any PNG size works — the sprite is stretched to fill the strip exactly.
> Use a transparent background if you want the fallback colour to show through.

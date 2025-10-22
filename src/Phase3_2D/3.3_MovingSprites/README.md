# 3.3 – Moving Sprites (Player + Enemies)

- Player uses `sprite_sheet_1x8.png` (1×8).
- Enemies use `sprite_sheet_2x4.png` (2×4), patrol left/right.
- Arrow keys AND WASD control the player; Space to jump.
- Animates while moving, idle when stopped. Faces right by default; flips when moving left.
- Screen bounds enforced; AABB collision between player and enemies.

## Controls
- Move Left: A or Left Arrow
- Move Right: D or Right Arrow
- Jump: Space (while grounded)
- Exit: ESC

## How It Works (High-Level)
- Shared quad (VAO/VBO/EBO).
- Per-sprite uniforms: `uModel`, `uUVScale`, `uUVOffset`, `uFlipX`.
- Animation picks frame via row/column from `currentFrame`.
- Physics: gravity, jump impulse, clamp to ground and screen.
- Enemies patrol between two X positions.
- AABB resolves minimal overlap so the player doesn't stick inside enemies.

## Run
```
dotnet run --project src/Phase3_2D/3.3_MovingSprites/3.3_MovingSprites.csproj
```

## Troubleshooting
- If textures are missing, verify they exist in the project output under `textures/`.
- If sprites look flipped vertically, ensure V coords in quad are `[0..1]` as provided. Horizontal flipping is done in the shader via `uFlipX`.


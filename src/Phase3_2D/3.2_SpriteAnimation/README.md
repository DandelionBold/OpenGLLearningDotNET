# Project 3.2: Sprite Animation - Animate Sprites Using Sprite Sheets

This project teaches you how to create animated sprites using sprite sheets (texture atlases) and UV offset animation.

## What You'll Learn

- **Sprite Sheets**: How to organize multiple animation frames in one texture
- **UV Offset Animation**: Moving UV coordinates to show different frames
- **Frame Timing**: Controlling animation speed using delta time
- **Animation Loops**: Cycling through frames continuously
- **Texture Sampling**: Using nearest-neighbor filtering for pixel-perfect sprites

## How It Works (High-Level)

1. **Sprite Sheet**: One texture contains multiple animation frames arranged in a grid
2. **Base UVs**: Vertex UVs represent the first frame (frame 0)
3. **UV Offset**: Calculate offset based on current frame and time
4. **Animation**: Add offset to base UVs in vertex shader to show current frame
5. **Loop**: Cycle through frames continuously using modulo arithmetic

## Sprite Sheet Concept

A sprite sheet is a texture containing multiple frames arranged in a grid:

```
┌─────────────────────────────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │   │  ← Row 0
├─────────┼─────────┼─────────┤   │
│ Frame 3 │ Frame 4 │ Frame 5 │   │  ← Row 1  
└─────────┴─────────┴─────────┘   │
```

Each frame has UV coordinates:
- Frame 0: (0.0, 0.0) to (0.33, 0.5)
- Frame 1: (0.33, 0.0) to (0.66, 0.5)
- Frame 2: (0.66, 0.0) to (1.0, 0.5)
- etc.

## Animation Technique

1. **Calculate Current Frame**: `currentFrame = (int)(time / frameDuration) % totalFrames`
2. **Convert to UV Offset**: `uOffset = (currentFrame % framesPerRow) * frameWidth`
3. **Send to Shader**: Pass UV offset as uniform to vertex shader
4. **Apply in Shader**: `finalUV = baseUV + uUVOffset`

## Files

- `Program.cs`: Animation logic, frame timing, UV offset calculation
- `Shaders/shader.vert`: Applies UV offset to show current frame
- `Shaders/shader.frag`: Samples sprite sheet texture
- `textures/sprite_sheet.png`: Sprite sheet texture (copy from 3.1 for testing)

## Animation Parameters

```csharp
int framesPerRow = 4;     // How many frames per row
int totalFrames = 8;      // Total animation frames
float animationFPS = 10f; // Animation speed (frames per second)
```

## Shader Overview

### Vertex (`shader.vert`)

- Inputs: `aPosition (vec3)`, `aTexCoord (vec2)` - base UVs for frame 0
- Uniforms: `uProjection (mat4)`, `uUVOffset (vec2)` - UV offset for current frame
- Output: `vTexCoord = aTexCoord + uUVOffset` - animated UVs

### Fragment (`shader.frag`)

- Input: `vTexCoord (vec2)` - animated UV coordinates
- Uniform: `uTexture0 (sampler2D)` - sprite sheet texture
- Output: `FragColor = texture(uTexture0, vTexCoord)`

## Texture Parameters Used

- **Minification**: `Nearest` - pixel-perfect scaling
- **Magnification**: `Nearest` - pixel-perfect scaling
- **Wrap S/T**: `ClampToEdge` - prevent bleeding between frames

These give crisp, pixel-perfect sprites without blurring.

## Run

```bash
cd src/Phase3_2D/3.2_SpriteAnimation
# ensure textures/sprite_sheet.png exists (copy from 3.1 if needed)
dotnet run
```

## Experiments

1. **Change Animation Speed**: Modify `animationFPS` (try 5, 15, 30)
2. **Adjust Frame Count**: Change `totalFrames` and `framesPerRow`
3. **Debug UVs**: Uncomment UV visualization in fragment shader
4. **Add Transparency**: Uncomment alpha discard in fragment shader
5. **Multiple Rows**: Extend to support multi-row sprite sheets

## Creating Your Own Sprite Sheet

1. **Arrange Frames**: Place animation frames in a grid
2. **Consistent Size**: All frames should be the same size
3. **Power of 2**: Use texture dimensions that are powers of 2
4. **No Gaps**: Frames should touch each other (no padding)
5. **Update Parameters**: Adjust `framesPerRow` and `totalFrames`

## Common Issues

- **No Animation**: Check that `sprite_sheet.png` exists and parameters are correct
- **Wrong Frame Size**: Verify `framesPerRow` matches your sprite sheet layout
- **Blurry Sprites**: Ensure texture filtering is set to `Nearest`
- **Frames Skipping**: Check `animationFPS` and `totalFrames` values

## Next

- **3.3**: Moving Sprites - Add keyboard control to move animated sprites
- **3.4**: Simple 2D Game - Combine sprites, movement, and game logic
- **3.5**: Particle System - Create effects using animated particles

## Key Concepts Mastered

✅ **Sprite Sheets** - Multiple frames in one texture  
✅ **UV Offset Animation** - Moving UVs to show different frames  
✅ **Frame Timing** - Time-based animation control  
✅ **Animation Loops** - Continuous cycling through frames  
✅ **Pixel-Perfect Rendering** - Nearest-neighbor filtering for crisp sprites

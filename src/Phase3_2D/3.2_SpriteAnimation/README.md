# Project 3.2: Sprite Animation - Animate Sprites Using Sprite Sheets

This project teaches you how to create animated sprites using sprite sheets (texture atlases) and UV offset animation with **heavy comments**, **visual diagrams**, and **beginner-friendly analogies**.

## 🎯 What You'll Learn

- **Sprite Sheets**: How to organize multiple animation frames in one texture
- **UV Offset Animation**: Moving UV coordinates to show different frames
- **Frame Timing**: Controlling animation speed using delta time
- **Animation Loops**: Cycling through frames continuously
- **Texture Sampling**: Using nearest-neighbor filtering for pixel-perfect sprites
- **Color Blending**: Understanding texture filtering and interpolation

## 🎬 How It Works (Step-by-Step Walkthrough)

### Step 1: Understanding Sprite Sheets
A sprite sheet is like a **comic book page** with multiple panels:

```
┌─────────────────────────────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │   │  ← Row 0 (like comic panels)
├─────────┼─────────┼─────────┤   │
│ Frame 3 │ Frame 4 │ Frame 5 │   │  ← Row 1 (more panels)
└─────────┴─────────┴─────────┘   │
```

**Analogy**: Think of it like a **flipbook** where each page is a frame, but instead of flipping pages, we move our "viewing window" across the sprite sheet.

### Step 2: UV Coordinate System
UV coordinates are like **map coordinates** for textures:

```
Texture UV Space (0.0 to 1.0):
┌─────────────────────────────────┐
│(0,1)                    (1,1)    │  ← Top row
│                                 │
│                                 │
│(0,0)                    (1,0)   │  ← Bottom row
└─────────────────────────────────┘

Each frame occupies a portion:
Frame 0: (0.0, 0.0) to (0.25, 1.0)
Frame 1: (0.25, 0.0) to (0.5, 1.0)
Frame 2: (0.5, 0.0) to (0.75, 1.0)
Frame 3: (0.75, 0.0) to (1.0, 1.0)
```

### Step 3: Animation Technique (The Magic!)
We don't change the texture - we change **where we look**:

```
Base UVs (Frame 0):     UV Offset:        Final UVs (Frame 2):
┌─────────┐             ┌─────────┐       ┌─────────┐
│(0,1)    │             │(0.5,0)  │       │(0.5,1)  │
│         │      +      │         │  =    │         │
│(0,0)    │             │(0,0)    │       │(0.5,0)  │
└─────────┘             └─────────┘       └─────────┘
```

**Analogy**: It's like having a **magic magnifying glass** that can instantly jump to different parts of a large picture.

## 🎨 Visual Diagrams: Color Blending and Interpolation

### Texture Filtering Comparison

**Linear Filtering (Smooth)** - Like looking through frosted glass:
```
Original Pixels:     Linear Result:
┌─┬─┬─┐              ┌─────────┐
│█│▒│░│     →        │  smooth  │
└─┴─┴─┘              │ gradient │
                     └─────────┘
```

**Nearest Filtering (Crisp)** - Like looking through clear glass:
```
Original Pixels:     Nearest Result:
┌─┬─┬─┐              ┌─────────┐
│█│▒│░│     →        │█│█│░│░│  │
└─┴─┴─┘              └─────────┘
```

### UV Interpolation Across Triangles

When UVs are interpolated across a triangle, each pixel gets a smoothly blended UV coordinate:

```
Triangle with UVs:
     (0.0, 1.0) ←─── Top vertex
        /\
       /  \
      /    \
     /      \
(0.0, 0.0)   (1.0, 0.0)
Bottom-left  Bottom-right

Interpolated UVs create smooth gradients:
┌─────────────────┐
│(0.0,1.0) (0.5,1.0) (1.0,1.0)│
│(0.0,0.5) (0.5,0.5) (1.0,0.5)│
│(0.0,0.0) (0.5,0.0) (1.0,0.0)│
└─────────────────┘
```

## 🔧 Technical Implementation

### Animation Parameters
```csharp
int framesPerRow = 4;     // How many frames per row (like columns in a table)
int totalFrames = 8;      // Total animation frames (like pages in a flipbook)
float animationFPS = 10f; // Animation speed (frames per second)

// ✨ NEW: UV coordinates are automatically calculated!
// frameWidth = 1.0 / framesPerRow (e.g., 8 frames = 0.125 per frame)
// This ensures UVs always match your sprite sheet layout
```

### Frame Calculation (The Math!)
```csharp
// Step 1: Calculate frame duration
float frameDuration = 1.0f / animationFPS;  // 0.1 seconds for 10 FPS

// Step 2: Calculate current frame based on time
currentFrame = (int)(frameTime / frameDuration) % totalFrames;

// Step 3: Convert frame to UV offset
float uOffset = (currentFrame % framesPerRow) * frameWidth;
float vOffset = (currentFrame / framesPerRow) * frameHeight;
```

**Analogy**: It's like a **clock hand** that ticks through frames at a steady rate, then loops back to the beginning.

## 🎮 Shader Overview (Heavily Commented)

### Vertex Shader (`shader.vert`)
```glsl
// INPUTS: Position and base UVs (for frame 0)
layout(location = 0) in vec3 aPosition;   // Where the vertex is
layout(location = 1) in vec2 aTexCoord;   // Base UVs (frame 0)

// OUTPUTS: Animated UVs to fragment shader
out vec2 vTexCoord;

// UNIFORMS: Projection matrix and UV offset
uniform mat4 uProjection;  // 2D projection
uniform vec2 uUVOffset;   // Offset to current frame

void main()
{
    // Add UV offset to show current animation frame
    vTexCoord = aTexCoord + uUVOffset;
    
    // Convert position to screen coordinates
    gl_Position = uProjection * vec4(aPosition, 1.0);
}
```

### Fragment Shader (`shader.frag`)
```glsl
// INPUTS: Animated UVs from vertex shader
in vec2 vTexCoord;

// OUTPUTS: Final pixel color
out vec4 FragColor;

// UNIFORMS: Sprite sheet texture
uniform sampler2D uTexture0;

void main()
{
    // Sample the sprite sheet at animated UVs
    FragColor = texture(uTexture0, vTexCoord);
    
    // Optional: Add transparency support
    // if (FragColor.a < 0.1) discard;
}
```

## 🎯 Beginner-Friendly Analogies

### 1. Sprite Sheet = Comic Book
- **Frames** = Individual comic panels
- **Animation** = Quickly flipping through panels
- **UV Offset** = Moving your finger to point at different panels

### 2. UV Coordinates = Map Coordinates
- **Texture** = A city map
- **UVs** = Street addresses (0.0 to 1.0)
- **Sampling** = Looking up what's at a specific address

### 3. Animation = Magic Window
- **Sprite Sheet** = Large painting
- **Quad** = Magic window that shows part of the painting
- **UV Offset** = Moving the window to show different parts

### 4. Frame Timing = Clock Ticking
- **Time** = Clock hands moving
- **FPS** = How fast the clock ticks
- **Current Frame** = Which hour the clock shows

## 🚀 How to Run

```bash
cd src/Phase3_2D/3.2_SpriteAnimation
# ensure textures/sprite_sheet.png exists (copy from 3.1 if needed)
dotnet run
```

## 🧪 Experiments (Try These!)

### 1. Change Animation Speed
```csharp
// Try different speeds:
float animationFPS = 5f;   // Slow motion
float animationFPS = 15f;  // Fast motion
float animationFPS = 30f;  // Very fast
```

### 2. Debug UV Visualization
Uncomment this line in the fragment shader:
```glsl
FragColor = vec4(vTexCoord, 0.0, 1.0);  // Shows UV coordinates as colors
```

### 3. Add Transparency
Uncomment this line in the fragment shader:
```glsl
if (FragColor.a < 0.1) discard;  // Discard transparent pixels
```

### 4. Color Effects
Try these in the fragment shader:
```glsl
FragColor = FragColor * vec4(1.0, 0.5, 0.5, 1.0);  // Red tint
FragColor = FragColor * 1.5;                        // Brighten
FragColor = pow(FragColor, vec4(1.2));             // Increase contrast
```

## 🎨 Creating Your Own Sprite Sheet

### Step-by-Step Guide:

1. **Arrange Frames**: Place animation frames in a grid
2. **Consistent Size**: All frames should be the same size
3. **Power of 2**: Use texture dimensions that are powers of 2 (64, 128, 256, 512, 1024)
4. **No Gaps**: Frames should touch each other (no padding)
5. **Update Parameters**: Adjust `framesPerRow` and `totalFrames`

### Example Layout:
```
4x2 Sprite Sheet (4 frames per row, 2 rows):
┌─────────────────────────────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │   │  ← Row 0
├─────────┼─────────┼─────────┤   │
│ Frame 3 │ Frame 4 │ Frame 5 │   │  ← Row 1
└─────────┴─────────┴─────────┘   │
```

## 🔍 Common Issues & Solutions

### Issue: No Animation
**Cause**: Sprite sheet not found or parameters incorrect
**Solution**: Check that `sprite_sheet.png` exists and `framesPerRow` matches your layout

### Issue: Wrong Frame Size
**Cause**: `framesPerRow` doesn't match sprite sheet layout
**Solution**: Count frames per row in your sprite sheet and update the parameter

### Issue: Blurry Sprites
**Cause**: Using linear filtering instead of nearest
**Solution**: Ensure texture filtering is set to `Nearest` in the C# code

### Issue: Frames Skipping
**Cause**: `animationFPS` too high or `totalFrames` wrong
**Solution**: Lower `animationFPS` or verify `totalFrames` count

### Issue: Sprite Cut-Off or Frame Bleeding ⚠️ CRITICAL!
**Cause**: Sprite sheet dimensions don't match frame layout math
**Solution**: **The sprite sheet dimensions MUST exactly match the frame layout!**

**Example Problem:**
- You have 8 frames, each 100 px wide
- Expected width: 8 × 100 = **800 px** ✓
- Actual width: **790 px** ❌ (WRONG!)

**The Math Behind It:**
```
Code calculates UV width: 1.0 / 8 = 0.125 (assumes 800 px)
But actual frame width: 100 / 790 = 0.1266 (different!)
Result: UV coordinates don't align with frame boundaries = cut-off/bleeding!
```

**The Fix:**
Resize your sprite sheet to match the math:
- 8 frames × 100 px = **800 px total width** ✓
- 1 row × 201 px = **201 px total height** ✓

**Formula:**
```
Total Width = Frames Per Row × Frame Width
Total Height = Number of Rows × Frame Height
```

**Pro Tip:** Use image dimensions that are **powers of 2** (256, 512, 1024) or exact multiples of your frame count for perfect alignment!

## 🎓 Key Concepts Mastered

✅ **Sprite Sheets** - Multiple frames in one texture  
✅ **UV Offset Animation** - Moving UVs to show different frames  
✅ **Frame Timing** - Time-based animation control  
✅ **Animation Loops** - Continuous cycling through frames  
✅ **Pixel-Perfect Rendering** - Nearest-neighbor filtering for crisp sprites  
✅ **Color Blending** - Understanding texture filtering and interpolation  
✅ **UV Interpolation** - How UVs blend across triangles  
✅ **Transparency** - Alpha channel support for sprite backgrounds

## 🚀 Next Steps

- **3.3**: Moving Sprites - Add keyboard control to move animated sprites
- **3.4**: Simple 2D Game - Combine sprites, movement, and game logic
- **3.5**: Particle System - Create effects using animated particles

## 💡 Pro Tips

1. **Use Nearest Filtering**: Always use `Nearest` for pixel art sprites
2. **Power of 2 Textures**: Use dimensions like 256x256, 512x512 for best performance
3. **Frame Consistency**: Keep all frames the same size for smooth animation
4. **Debug with UVs**: Visualize UV coordinates to debug animation issues
5. **Test Different Speeds**: Experiment with various FPS values for different effects
6. **✨ Auto-Calculated UVs**: UV coordinates are now calculated automatically based on `framesPerRow` - just change the parameter and the code adapts!

---

**Congratulations!** You now understand sprite animation - one of the most important techniques in 2D game development! 🎉

# Project 3.2b: Sprite Animation 2x4 - Multi-Row Sprite Sheet Animation

This project teaches you how to create animated sprites using **2x4 sprite sheet layouts** (2 rows × 4 columns) with **heavy comments**, **visual diagrams**, and **beginner-friendly analogies**.

## 🎯 What You'll Learn

- **Multi-Row Sprite Sheets**: How to organize animation frames across multiple rows
- **2D Grid Animation**: Calculating row/column positions from frame numbers
- **UV Offset Animation**: Moving UV coordinates to show different frames in a grid
- **Frame Timing**: Controlling animation speed using delta time
- **Animation Loops**: Cycling through frames continuously across rows
- **Texture Sampling**: Using nearest-neighbor filtering for pixel-perfect sprites
- **Auto-Calculated UVs**: UV coordinates automatically calculated from layout parameters

## 🎬 How It Works (Step-by-Step Walkthrough)

### Step 1: Understanding 2x4 Sprite Sheets
A 2x4 sprite sheet is like a **comic book page** with multiple panels arranged in a grid:

```
┌─────────────────────────────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │ Frame 3 │  ← Row 0 (top)
├─────────┼─────────┼─────────┼─────────┤
│ Frame 4 │ Frame 5 │ Frame 6 │ Frame 7 │  ← Row 1 (bottom)
└─────────┴─────────┴─────────┴─────────┘
```

**Analogy**: Think of it like a **flipbook** where each page is a frame, but instead of flipping pages, we move our "viewing window" across the sprite sheet grid.

### Step 2: UV Coordinate System for 2x4 Layout
UV coordinates are like **map coordinates** for textures, but now we have a 2D grid:

```
Texture UV Space (0.0 to 1.0):
┌─────────────────────────────────┐
│(0,0.5)  (0.25,0.5)  (0.5,0.5)  (0.75,0.5)│  ← Row 0 (top half)
│(0,0)    (0.25,0)    (0.5,0)    (0.75,0)  │  ← Row 1 (bottom half)
└─────────────────────────────────┘

Each frame occupies a portion:
Frame 0: (0.0, 0.0) to (0.25, 0.5)     ← Row 0, Col 0
Frame 1: (0.25, 0.0) to (0.5, 0.5)     ← Row 0, Col 1
Frame 2: (0.5, 0.0) to (0.75, 0.5)    ← Row 0, Col 2
Frame 3: (0.75, 0.0) to (1.0, 0.5)    ← Row 0, Col 3
Frame 4: (0.0, 0.5) to (0.25, 1.0)    ← Row 1, Col 0
Frame 5: (0.25, 0.5) to (0.5, 1.0)   ← Row 1, Col 1
Frame 6: (0.5, 0.5) to (0.75, 1.0)   ← Row 1, Col 2
Frame 7: (0.75, 0.5) to (1.0, 1.0)   ← Row 1, Col 3
```

### Step 3: Animation Technique (The Magic!)
We don't change the texture - we change **where we look**:

```
Base UVs (Frame 0):     UV Offset:        Final UVs (Frame 5):
┌─────────┐             ┌─────────┐       ┌─────────┐
│(0,0.5)  │             │(0.25,0.5)│       │(0.25,1.0)│
│         │      +      │         │  =    │         │
│(0,0)    │             │(0,0)    │       │(0.25,0.5)│
└─────────┘             └─────────┘       └─────────┘
```

**Analogy**: It's like having a **magic magnifying glass** that can instantly jump to different parts of a large picture grid.

## 🎨 Visual Diagrams: Multi-Row Animation

### Frame Layout Visualization

**2x4 Sprite Sheet Layout:**
```
┌─────────────────────────────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │ Frame 3 │  ← Row 0
├─────────┼─────────┼─────────┼─────────┤
│ Frame 4 │ Frame 5 │ Frame 6 │ Frame 7 │  ← Row 1
└─────────┴─────────┴─────────┴─────────┘
```

**Frame Numbering:**
```
┌─────────────────────────────────┐
│   0    │   1    │   2    │   3    │  ← Row 0
├─────────┼─────────┼─────────┼─────────┤
│   4    │   5    │   6    │   7    │  ← Row 1
└─────────┴─────────┴─────────┴─────────┘
```

### UV Coordinate Mapping

**Row 0 (Top Half):**
```
┌─────────────────────────────────┐
│(0,0.5)  (0.25,0.5)  (0.5,0.5)  (0.75,0.5)│
│(0,0)    (0.25,0)    (0.5,0)    (0.75,0)  │
└─────────────────────────────────┘
```

**Row 1 (Bottom Half):**
```
┌─────────────────────────────────┐
│(0,1.0)  (0.25,1.0)  (0.5,1.0)  (0.75,1.0)│
│(0,0.5)  (0.25,0.5)  (0.5,0.5)  (0.75,0.5)│
└─────────────────────────────────┘
```

## 🔧 Technical Implementation

### Animation Parameters
```csharp
int framesPerRow = 4;     // How many frames per row (4 columns)
int numberOfRows = 2;     // How many rows (2 rows)
int totalFrames = 8;      // Total animation frames (2×4=8)
float animationFPS = 12f; // Animation speed (frames per second)

// ✨ NEW: UV coordinates are automatically calculated!
// frameWidth = 1.0 / framesPerRow = 1.0 / 4 = 0.25
// frameHeight = 1.0 / numberOfRows = 1.0 / 2 = 0.5
// This ensures UVs always match your sprite sheet layout

// 🔮 FUTURE: Spacing parameters for sprite sheets with gaps
float horizontalSpacing = 0.0f;  // Horizontal spacing between frames (0.0 = no spacing)
float verticalSpacing = 0.0f;    // Vertical spacing between frames (0.0 = no spacing)

// 🔮 FUTURE: Padding parameters for sprite sheets with frame padding
float horizontalPadding = 0.0f;  // Horizontal padding around each frame (0.0 = no padding)
float verticalPadding = 0.0f;    // Vertical padding around each frame (0.0 = no padding)
```

### Frame Calculation (The Math!)
```csharp
// Step 1: Calculate frame duration
float frameDuration = 1.0f / animationFPS;  // 0.083 seconds for 12 FPS

// Step 2: Calculate current frame based on time
currentFrame = (int)(frameTime / frameDuration) % totalFrames;

// Step 3: Convert frame to row/column position
int row = currentFrame / framesPerRow;  // 0 or 1 for 2 rows
int col = currentFrame % framesPerRow;  // 0, 1, 2, or 3 for 4 columns

// Step 4: Convert row/column to UV offset
float uOffset = col * frameWidth;   // 0.0, 0.25, 0.5, or 0.75
float vOffset = row * frameHeight;  // 0.0 or 0.5
```

**Analogy**: It's like a **clock hand** that ticks through frames at a steady rate, then loops back to the beginning, but now it moves in a 2D grid pattern.

### Frame-to-UV Examples

**Frame 0 (Row 0, Col 0):**
- `row = 0 / 4 = 0`
- `col = 0 % 4 = 0`
- `uOffset = 0 * 0.25 = 0.0`
- `vOffset = 0 * 0.5 = 0.0`
- **UV Range**: (0.0, 0.0) to (0.25, 0.5)

**Frame 5 (Row 1, Col 1):**
- `row = 5 / 4 = 1`
- `col = 5 % 4 = 1`
- `uOffset = 1 * 0.25 = 0.25`
- `vOffset = 1 * 0.5 = 0.5`
- **UV Range**: (0.25, 0.5) to (0.5, 1.0)

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
    if (FragColor.a < 0.1) discard;
}
```

## 🎯 Beginner-Friendly Analogies

### 1. Sprite Sheet = Comic Book Grid
- **Frames** = Individual comic panels in a 2x4 grid
- **Animation** = Quickly moving your finger across the grid
- **UV Offset** = Moving your finger to point at different panels

### 2. UV Coordinates = Map Grid Coordinates
- **Texture** = A city map divided into districts
- **UVs** = Street addresses (0.0 to 1.0) in a grid system
- **Sampling** = Looking up what's at a specific grid address

### 3. Animation = Magic Window
- **Sprite Sheet** = Large painting divided into sections
- **Quad** = Magic window that shows part of the painting
- **UV Offset** = Moving the window to show different sections

### 4. Frame Timing = Clock Ticking
- **Time** = Clock hands moving
- **FPS** = How fast the clock ticks
- **Current Frame** = Which grid position the clock shows

## 🚀 How to Run

```bash
cd src/Phase3_2D/3.2b_SpriteAnimation_2x4
# ensure textures/sprite_sheet.png exists (2x4 layout)
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

## 🎨 Creating Your Own 2x4 Sprite Sheet

### Step-by-Step Guide:

1. **Arrange Frames**: Place animation frames in a 2x4 grid
2. **Consistent Size**: All frames should be the same size
3. **Power of 2**: Use texture dimensions that are powers of 2 (256, 512, 1024)
4. **No Gaps**: Frames should touch each other (no padding)
5. **Update Parameters**: Adjust `framesPerRow`, `numberOfRows`, and `totalFrames`

### Example Layout:
```
2x4 Sprite Sheet (2 rows, 4 columns):
┌─────────────────────────────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │ Frame 3 │  ← Row 0
├─────────┼─────────┼─────────┼─────────┤
│ Frame 4 │ Frame 5 │ Frame 6 │ Frame 7 │  ← Row 1
└─────────┴─────────┴─────────┴─────────┘
```

## 📐 Understanding Spacing vs Padding

### Spacing vs Padding - What's the Difference?

**Spacing** = Gaps **between** frames
**Padding** = Gaps **around** each frame (inside the frame boundary)

### Visual Example:

```
SPACING (between frames):
┌─────────┐  GAP  ┌─────────┐  GAP  ┌─────────┐
│ Frame 0 │       │ Frame 1 │       │ Frame 2 │
└─────────┘       └─────────┘       └─────────┘

PADDING (around each frame):
┌─────────────┐
│    PADDING  │
│ ┌─────────┐ │
│ │ Content │ │  ← Actual sprite content
│ └─────────┘ │
│    PADDING  │
└─────────────┘
```

### When to Use Each:

**Use Spacing when:**
- Frames are separated by gaps in the sprite sheet
- You want to skip empty areas between frames
- Example: `horizontalSpacing = 0.02f` (2% gap between frames)

**Use Padding when:**
- Each frame has empty space around the actual sprite content
- You want to crop out the padding to show only the sprite
- Example: `horizontalPadding = 0.01f` (1% padding around each frame)

### Future Implementation:

When these parameters are implemented, the UV calculation would be:
```csharp
// Effective frame size (accounting for padding)
float effectiveFrameWidth = frameWidth - (2 * horizontalPadding);
float effectiveFrameHeight = frameHeight - (2 * verticalPadding);

// UV offset calculation (accounting for spacing)
float uOffset = col * (frameWidth + horizontalSpacing) + horizontalPadding;
float vOffset = row * (frameHeight + verticalSpacing) + verticalPadding;
```

## 🔍 Common Issues & Solutions

### Issue: No Animation
**Cause**: Sprite sheet not found or parameters incorrect
**Solution**: Check that `sprite_sheet.png` exists and `framesPerRow`/`numberOfRows` match your layout

### Issue: Wrong Frame Size
**Cause**: `framesPerRow` or `numberOfRows` doesn't match sprite sheet layout
**Solution**: Count frames per row and number of rows in your sprite sheet and update the parameters

### Issue: Blurry Sprites
**Cause**: Using linear filtering instead of nearest
**Solution**: Ensure texture filtering is set to `Nearest` in the C# code

### Issue: Frames Skipping
**Cause**: `animationFPS` too high or `totalFrames` wrong
**Solution**: Lower `animationFPS` or verify `totalFrames` count

### Issue: Wrong Row/Column
**Cause**: Frame calculation math is incorrect
**Solution**: Verify the row/column calculation:
```csharp
int row = currentFrame / framesPerRow;  // Should give 0 or 1 for 2 rows
int col = currentFrame % framesPerRow;  // Should give 0, 1, 2, or 3 for 4 columns
```

### Issue: Sprite Cut-Off or Frame Bleeding ⚠️ CRITICAL!
**Cause**: Sprite sheet dimensions don't match frame layout math
**Solution**: **The sprite sheet dimensions MUST exactly match the frame layout!**

**Example Problem:**
- You have 2 rows × 4 columns, each frame 128×126 px
- Expected width: 4 × 128 = **512 px** ✓
- Expected height: 2 × 126 = **252 px** ✓
- Actual dimensions: **512×252 px** ✓ (CORRECT!)

**The Math Behind It:**
```
Code calculates UV width: 1.0 / 4 = 0.25 (assumes 512 px)
Code calculates UV height: 1.0 / 2 = 0.5 (assumes 252 px)
Actual frame width: 128 / 512 = 0.25 ✓
Actual frame height: 126 / 252 = 0.5 ✓
Result: UV coordinates align with frame boundaries = perfect animation!
```

**The Fix:**
Resize your sprite sheet to match the math:
- 4 frames × 128 px = **512 px total width** ✓
- 2 rows × 126 px = **252 px total height** ✓

**Formula:**
```
Total Width = Frames Per Row × Frame Width
Total Height = Number of Rows × Frame Height
```

**Pro Tip:** Use image dimensions that are **powers of 2** (256, 512, 1024) or exact multiples of your frame count for perfect alignment!

## 🎓 Key Concepts Mastered

✅ **Multi-Row Sprite Sheets** - Multiple frames arranged in a 2D grid  
✅ **2D Grid Animation** - Calculating row/column positions from frame numbers  
✅ **UV Offset Animation** - Moving UVs to show different frames in a grid  
✅ **Frame Timing** - Time-based animation control  
✅ **Animation Loops** - Continuous cycling through frames across rows  
✅ **Pixel-Perfect Rendering** - Nearest-neighbor filtering for crisp sprites  
✅ **Auto-Calculated UVs** - UV coordinates automatically calculated from layout parameters  
✅ **Transparency** - Alpha channel support for sprite backgrounds

## 🚀 Next Steps

- **3.3**: Camera System - Add pan, zoom, and follow functionality
- **3.4**: Sprite Batching - Render hundreds of sprites efficiently
- **3.5**: Tilemap Rendering - Build 2D game worlds

## 💡 Pro Tips

1. **Use Nearest Filtering**: Always use `Nearest` for pixel art sprites
2. **Power of 2 Textures**: Use dimensions like 256x256, 512x512 for best performance
3. **Frame Consistency**: Keep all frames the same size for smooth animation
4. **Debug with UVs**: Visualize UV coordinates to debug animation issues
5. **Test Different Speeds**: Experiment with various FPS values for different effects
6. **✨ Auto-Calculated UVs**: UV coordinates are now calculated automatically based on `framesPerRow` and `numberOfRows` - just change the parameters and the code adapts!
7. **Grid Layout Planning**: Plan your sprite sheet layout before creating the image to ensure perfect alignment

---

**Congratulations!** You now understand multi-row sprite animation - a crucial technique for complex 2D game development! 🎉

# Project 3.2c: Multi-Sheet Sprite Animation with Keyboard Controls

**Learning Goals**: Keyboard input handling, dynamic texture switching, runtime resource management, interactive animation control

---

## 🎮 What You'll Learn

1. **Keyboard Input**: Using Silk.NET Input system to handle key presses
2. **Dynamic Texture Switching**: Changing textures at runtime
3. **Multiple Sprite Sheets**: Managing different sprite sheet layouts (1x8 vs 2x4)
4. **Interactive Controls**: User-controlled animation speed and sprite switching
5. **Resource Management**: Loading and managing multiple textures efficiently

---

## 🎯 Project Goals

This project demonstrates **interactive sprite animation** with:
- **Multiple sprite sheets**: Switch between 1x8 and 2x4 layouts dynamically
- **Keyboard controls**: Space to switch sheets, arrows to control speed
- **Real-time updates**: Changes take effect immediately
- **Clean state management**: Properly update all parameters when switching

---

## 📐 Sprite Sheet Layouts

### Layout 1: 1 Row × 8 Columns (sprite_sheet_1x8.png)
```
┌─────────┬─────────┬─────────┬─────────┬─────────┬─────────┬─────────┬─────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │ Frame 3 │ Frame 4 │ Frame 5 │ Frame 6 │ Frame 7 │
└─────────┴─────────┴─────────┴─────────┴─────────┴─────────┴─────────┴─────────┘

UV Dimensions:
- Frame Width: 0.125 (1/8)
- Frame Height: 1.0 (full height)
- Total Frames: 8
```

### Layout 2: 2 Rows × 4 Columns (sprite_sheet_2x4.png)
```
┌─────────┬─────────┬─────────┬─────────┐
│ Frame 0 │ Frame 1 │ Frame 2 │ Frame 3 │  ← Row 0
├─────────┼─────────┼─────────┼─────────┤
│ Frame 4 │ Frame 5 │ Frame 6 │ Frame 7 │  ← Row 1
└─────────┴─────────┴─────────┴─────────┘

UV Dimensions:
- Frame Width: 0.25 (1/4)
- Frame Height: 0.5 (1/2)
- Total Frames: 8
```

---

## 🎮 Keyboard Controls

| Key | Action | Description |
|-----|--------|-------------|
| **1** | Choose Sheet 1 | Switch to 1x8 sprite sheet layout |
| **2** | Choose Sheet 2 | Switch to 2x4 sprite sheet layout |
| **SPACE** | Pause/Unpause | Toggle animation pause state |
| **← (Left Arrow)** | Previous Frame | Step back one frame (hold for continuous) |
| **→ (Right Arrow)** | Next Frame | Step forward one frame (hold for continuous) |
| **↑ (Arrow Up)** | Increase Speed | Increase animation FPS (hold for continuous) |
| **↓ (Arrow Down)** | Decrease Speed | Decrease animation FPS (hold for continuous) |
| **ESC** | Exit | Close the application |

### Control Details
- **Number Keys**: Direct sprite sheet selection (no cycling needed)
- **Manual Frame Control**: Left/Right arrows only work when animation is paused
- **Continuous Key Holding**: Arrow keys repeat at 0.1 second intervals when held down
- **Pause State**: SPACE toggles between running and paused
- **Speed Control**: 
  - **Minimum FPS**: 1.0 (very slow)
  - **Maximum FPS**: 30.0 (very fast)
  - **Default FPS**: 12.0
  - **Step**: 1.0 FPS per key press/repeat

---

## 🔧 Technical Implementation

### Multiple Sprite Sheets System

```csharp
// Store multiple textures
private static uint[] textureIds = new uint[2];
private static string[] textureFiles = new string[]
{
    "sprite_sheet_1x8.png",  // Sheet 0
    "sprite_sheet_2x4.png"   // Sheet 1
};

// Each sheet has its own layout parameters
private static int[] framesPerRowArray = new int[] { 8, 4 };
private static int[] numberOfRowsArray = new int[] { 1, 2 };
private static int[] totalFramesArray = new int[] { 8, 8 };

// Track which sheet is active
private static int currentSheetIndex = 0;
```

### Keyboard Input Setup

```csharp
// 1. Create input context when window loads
inputContext = window.CreateInput();

// 2. Register keyboard callback
foreach (var keyboard in inputContext.Keyboards)
{
    keyboard.KeyDown += OnKeyDown;
}

// 3. Handle key presses
private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
{
    if (key == Key.Space)
    {
        // Switch sprite sheets
        int nextIndex = (currentSheetIndex + 1) % textureFiles.Length;
        SwitchSpriteSheet(nextIndex);
    }
    else if (key == Key.Up)
    {
        // Increase FPS
        animationFPS = Math.Min(animationFPS + fpsStep, maxFPS);
    }
    else if (key == Key.Down)
    {
        // Decrease FPS
        animationFPS = Math.Max(animationFPS - fpsStep, minFPS);
    }
}
```

### Sprite Sheet Switching

```csharp
private static void SwitchSpriteSheet(int index)
{
    // 1. Update current index
    currentSheetIndex = index;
    
    // 2. Load new layout parameters
    framesPerRow = framesPerRowArray[index];
    numberOfRows = numberOfRowsArray[index];
    totalFrames = totalFramesArray[index];
    
    // 3. Recalculate frame dimensions
    frameWidth = 1.0f / framesPerRow;
    frameHeight = 1.0f / numberOfRows;
    
    // 4. Reset animation state
    currentFrame = 0;
    frameTime = 0f;
    
    // 5. Update quad UVs for new layout
    InitializeQuadVertices();
    
    // 6. Re-upload VBO data
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
    gl.BufferData(...);  // Upload new UV coordinates
}
```

### Rendering with Active Sprite Sheet

```csharp
private static void OnRender(double delta)
{
    // 1. Update animation (frame calculation)
    UpdateAnimation(delta);
    
    // 2. Bind the ACTIVE sprite sheet texture
    gl.ActiveTexture(TextureUnit.Texture0);
    gl.BindTexture(TextureTarget.Texture2D, textureIds[currentSheetIndex]);
    
    // 3. Draw quad
    gl.BindVertexArray(vao);
    gl.DrawElements(PrimitiveType.Triangles, 6, ...);
}
```

---

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     USER INPUT                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  SPACE   │  │ ARROW UP │  │ARROW DOWN│  │   ESC    │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │
│       │             │              │             │         │
└───────┼─────────────┼──────────────┼─────────────┼─────────┘
        │             │              │             │
        ▼             ▼              ▼             ▼
  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
  │ Switch   │  │Increase  │  │Decrease  │  │  Close   │
  │  Sheet   │  │   FPS    │  │   FPS    │  │  Window  │
  └────┬─────┘  └────┬─────┘  └────┬─────┘  └──────────┘
       │             │              │
       ▼             ▼              ▼
┌────────────────────────────────────────┐
│      UPDATE ANIMATION PARAMETERS       │
│  • frameWidth, frameHeight             │
│  • framesPerRow, numberOfRows          │
│  • animationFPS                        │
│  • Reset currentFrame, frameTime       │
│  • Recalculate quad UVs                │
└────────────┬───────────────────────────┘
             │
             ▼
┌────────────────────────────────────────┐
│          RENDER FRAME                  │
│  1. Calculate current frame            │
│  2. Calculate UV offset                │
│  3. Bind active texture                │
│  4. Draw animated sprite               │
└────────────────────────────────────────┘
```

---

## 🎬 Animation Flow

### Frame 0: Load Resources
```
Load sprite_sheet_1x8.png → textureIds[0]
Load sprite_sheet_2x4.png → textureIds[1]
Set currentSheetIndex = 0 (start with 1x8)
Calculate initial frame dimensions
```

### Every Frame: Update and Render
```
1. Accumulate time: frameTime += delta
2. Calculate current frame: (int)(frameTime / frameDuration) % totalFrames
3. Convert frame to row/col: row = frame / framesPerRow, col = frame % framesPerRow
4. Calculate UV offset: uOffset = col * (frameWidth + spacing)
5. Send UV offset to shader as uniform
6. Bind active texture: textureIds[currentSheetIndex]
7. Draw quad with animated UVs
```

### On Number Key Press: Direct Sprite Sheet Selection
```
1: SwitchSpriteSheet(0) → 1x8 layout
2: SwitchSpriteSheet(1) → 2x4 layout
```

### On SPACE Press: Pause/Unpause
```
isPaused = !isPaused
Console: "Animation PAUSED" or "Animation RESUMED"
```

### On LEFT/RIGHT Press: Manual Frame Control (Paused Only)
```
LEFT:  currentFrame = (currentFrame - 1 + totalFrames) % totalFrames
RIGHT: currentFrame = (currentFrame + 1) % totalFrames
Console: "Manual frame: X"
```

### On ARROW UP/DOWN Press: Adjust Speed
```
ARROW UP:   animationFPS = min(animationFPS + 1, maxFPS)
ARROW DOWN: animationFPS = max(animationFPS - 1, minFPS)

Effect: Changes frameDuration = 1.0 / animationFPS
Faster FPS → Shorter frameDuration → Frames change more quickly
Slower FPS → Longer frameDuration → Frames change more slowly
```

---

## 🔍 Common Issues & Solutions

### Issue: Input Not Working
**Cause**: Input context not created or keyboard callbacks not registered  
**Solution**: Verify `inputContext = window.CreateInput()` runs in `OnLoad()`, check callback registration

### Issue: Sprite Sheet Not Switching
**Cause**: Texture not bound or wrong texture ID used  
**Solution**: Check `gl.BindTexture(TextureTarget.Texture2D, textureIds[currentSheetIndex])` in render loop

### Issue: Wrong Frame Dimensions After Switch
**Cause**: Forgot to recalculate `frameWidth` and `frameHeight`  
**Solution**: Ensure `SwitchSpriteSheet()` recalculates: `frameWidth = 1.0f / framesPerRow`

### Issue: UVs Not Updated After Switch
**Cause**: Quad vertices not re-uploaded to GPU  
**Solution**: Call `gl.BufferData()` after `InitializeQuadVertices()` to upload new UV coordinates

### Issue: Animation Continues from Wrong Frame After Switch
**Cause**: `currentFrame` and `frameTime` not reset  
**Solution**: Set `currentFrame = 0; frameTime = 0f;` in `SwitchSpriteSheet()`

---

## 🎓 Key Concepts Explained

### 1. Input System Architecture

**Input Context**: Central manager for all input devices (keyboard, mouse, gamepad)
```csharp
IInputContext inputContext = window.CreateInput();
```

**Keyboard Callbacks**: Functions called automatically when keys are pressed
```csharp
keyboard.KeyDown += OnKeyDown;  // Register callback
keyboard.KeyUp += OnKeyUp;      // Optional: handle key release
```

**Event-Driven**: Your code doesn't check for keys every frame; instead, callbacks fire when events happen

### 2. Dynamic Resource Management

**Pre-Loading**: Load all resources at startup, store IDs
```csharp
for (int i = 0; i < textureFiles.Length; i++)
{
    textureIds[i] = LoadTexture(textureFiles[i]);
}
```

**Switching**: Just change which ID you bind (no reload needed)
```csharp
gl.BindTexture(TextureTarget.Texture2D, textureIds[currentSheetIndex]);
```

**Benefit**: Fast switching (no disk I/O), smooth transitions

### 3. State Synchronization

When switching sprite sheets, **ALL related state must update**:
- ✅ Texture ID (`currentSheetIndex`)
- ✅ Layout parameters (`framesPerRow`, `numberOfRows`, `totalFrames`)
- ✅ Frame dimensions (`frameWidth`, `frameHeight`)
- ✅ Animation state (`currentFrame`, `frameTime`)
- ✅ Quad UVs (re-calculate and re-upload)

**Missing one = visual glitches!**

### 4. Frame-Independent Animation Speed

**Problem**: Different computers have different frame rates (60 FPS, 144 FPS, etc.)

**Solution**: Use `delta` time (time since last frame)
```csharp
frameTime += (float)delta;  // Accumulate elapsed time
currentFrame = (int)(frameTime / frameDuration) % totalFrames;
```

**Result**: Animation runs at same speed on all computers, regardless of FPS

---

## 🚀 How to Run

### Option 1: Using dotnet CLI (PowerShell)
```powershell
dotnet run --project src\Phase3_2D\3.2c_SpriteAnimation_MultiSheet\3.2c_SpriteAnimation_MultiSheet.csproj
```

### Option 2: Using Visual Studio Code
1. Open the project in VS Code
2. Press `F5` or select "Run > Start Debugging"
3. Choose ".NET Core Launch" if prompted

---

## 🎨 Customization Ideas

### Add More Sprite Sheets
```csharp
// 1. Add texture file to textures folder
// 2. Update arrays:
private static string[] textureFiles = new string[]
{
    "sprite_sheet_1x8.png",
    "sprite_sheet_2x4.png",
    "sprite_sheet_4x4.png"  // NEW!
};

private static int[] framesPerRowArray = new int[] { 8, 4, 4 };  // NEW
private static int[] numberOfRowsArray = new int[] { 1, 2, 4 };  // NEW
private static int[] totalFramesArray = new int[] { 8, 8, 16 }; // NEW
```

### Add More Controls
```csharp
// Number keys to jump to specific sheets
if (key == Key.Number1) SwitchSpriteSheet(0);
if (key == Key.Number2) SwitchSpriteSheet(1);

// Left/Right arrows to step through frames manually
if (key == Key.Left) currentFrame = (currentFrame - 1 + totalFrames) % totalFrames;
if (key == Key.Right) currentFrame = (currentFrame + 1) % totalFrames;

// P key to pause/unpause animation
if (key == Key.P) isPaused = !isPaused;
```

### Add Visual Feedback
```csharp
// Display current settings on screen
Console.WriteLine($"Sheet: {textureFiles[currentSheetIndex]}, FPS: {animationFPS}, Frame: {currentFrame}");

// Or use ImGui for on-screen display (advanced)
```

---

## 🔗 Related Projects

- **3.2_SpriteAnimation**: Basic sprite animation (1x8 layout)
- **3.2b_SpriteAnimation_2x4**: Multi-row sprite animation (2x4 layout)
- **3.2c_SpriteAnimation_MultiSheet**: **This project** - Multi-sheet with keyboard controls

---

## 📚 What's Next?

**Project 3.3**: Camera System
- Pan (move view)
- Zoom (scale view)
- Follow sprites automatically
- View boundaries and constraints

**Project 3.4**: Sprite Batching
- Render hundreds of sprites efficiently
- Batch draw calls
- Instanced rendering
- Performance optimization

**Project 3.5**: Tilemap Rendering
- Build 2D game worlds
- Tile-based rendering
- Efficient culling
- Scrolling backgrounds

---

## 🎉 Congratulations!

You've learned:
- ✅ Keyboard input handling with Silk.NET
- ✅ Dynamic texture switching at runtime
- ✅ Managing multiple sprite sheet layouts
- ✅ Interactive animation control
- ✅ State synchronization and management

**You now have the skills to build interactive 2D games with responsive controls!** 🎮


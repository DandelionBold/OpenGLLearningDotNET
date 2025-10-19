# Phase 1: Foundation Setup

Welcome to Phase 1! This is where your OpenGL journey begins.

## What You'll Learn

- Set up .NET project with Silk.NET
- Create your first OpenGL window
- Understand the render loop
- Clear the screen with colors
- Handle keyboard and mouse input

## Time Estimate

**3-5 hours total** for all three projects in this phase

---

## Project 1.1: Empty Window ⭐

**Goal**: Create a window that opens and displays a solid color.

### Key Concepts

#### The Render Loop

Every OpenGL application follows this pattern:

```
Initialize → Loop (Update → Render) → Cleanup
```

1. **Initialize** (`OnLoad`): Set up OpenGL, load resources
2. **Update** (`OnUpdate`): Handle input, update game state
3. **Render** (`OnRender`): Draw everything to the screen
4. **Cleanup** (`OnClosing`): Free resources

#### Delta Time

`deltaTime` tells you how much time passed since the last frame. This is crucial for:

- Smooth animations
- Frame-rate independent movement
- Calculating FPS (Frames Per Second)

```csharp
FPS = 1.0 / deltaTime
```

#### The OpenGL Context

`GL gl` is your connection to the graphics card. Every OpenGL function is called through this object.

### How to Run

```bash
cd src/Phase1_Foundation/1.1_EmptyWindow
dotnet run
```

### What You Should See

A window with a dark blue background. That's it! Simple but important.

### Experiments to Try

1. **Change the window size**:

   ```csharp
   options.Size = new Vector2D<int>(1920, 1080);
   ```

2. **Change the background color**:

   ```csharp
   // Red background
   gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

   // Green background
   gl.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);

   // White background
   gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
   ```

3. **Print the FPS**:
   Uncomment the line in `OnUpdate` to see your frame rate.

### Understanding Colors in OpenGL

Colors use RGBA format (Red, Green, Blue, Alpha):

- Each value ranges from `0.0` (none) to `1.0` (full)
- Alpha controls transparency (1.0 = fully opaque)

Examples:

- `(1, 0, 0, 1)` = Pure Red
- `(0, 1, 0, 1)` = Pure Green
- `(0, 0, 1, 1)` = Pure Blue
- `(1, 1, 0, 1)` = Yellow (Red + Green)
- `(0.5, 0.5, 0.5, 1)` = Gray
- `(0, 0, 0, 1)` = Black
- `(1, 1, 1, 1)` = White

---

## Project 1.2: Window with Changing Colors

**Status**: Coming Soon!

**Goal**: Animate the background color over time.

**New Concepts**:

- Using `deltaTime` for animations
- Time-based color interpolation
- Math for smooth transitions

---

## Project 1.3: Input Handling

**Status**: Coming Soon!

**Goal**: Respond to keyboard and mouse input.

**New Concepts**:

- Keyboard events
- Mouse events
- Input-based interactions

---

## Common Issues & Solutions

### Issue: Window opens but immediately closes

**Solution**: Make sure you're calling `window.Run()` - this keeps the window open.

### Issue: Build errors about missing packages

**Solution**: Run `dotnet restore` in the project directory.

### Issue: Window is blank/black instead of colored

**Solution**: Check that you're calling both `gl.ClearColor()` AND `gl.Clear()`.

### Issue: "Cannot create OpenGL context" error

**Solution**: Your graphics drivers might be outdated. Update them from your GPU manufacturer's website.

---

## Resources

- [Silk.NET Documentation](https://dotnet.github.io/Silk.NET/)
- [OpenGL Reference](https://www.khronos.org/opengl/)
- [Learn OpenGL](https://learnopengl.com/) - Excellent resource (uses C++, but concepts translate)

---

## Next Phase

Once you complete all three projects in Phase 1, you'll move to **Phase 2: Your First Triangle**, where you'll learn about:

- Vertex Buffer Objects (VBO)
- Vertex Array Objects (VAO)
- Shaders
- Drawing actual shapes!

---

**Tips for Success**:

1. Type the code yourself - don't just copy/paste
2. Experiment with the values
3. Read all the comments
4. Try to break things and fix them
5. Take breaks when stuck!

Good luck! 🚀

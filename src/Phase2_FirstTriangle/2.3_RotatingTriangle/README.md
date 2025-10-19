# Project 2.3: Rotating Triangle - Transformation Matrices

**Make It Spin! 🔄**

---

## 🎯 What You'll See

A **gradient triangle** that **continuously spins** around its center!

```
Frame 1:       *       Frame 2:      *        Frame 3:    *
              /🔴\                  /🔴\                 /🔴\
             / 🟡 \                / 🟡\                / 🟡 \
            /🟢___🔵\            /🟢___🔵\          /🟢______🔵\
            
         (Starting)            (Rotated 30°)      (Rotated 60°)
         
         ... keeps spinning forever! 🔄
```

---

## 🆕 What's New from Project 2.2

| Project 2.2 | Project 2.3 |
|-------------|-------------|
| **Static** triangle | **Rotating** triangle |
| No transformation | **Matrix transformation** |
| No uniforms | **Uniform variable** (transform matrix) |
| Just position and color | Position, color, **AND rotation**! |

---

## 🔑 Key Concepts

### 1. What is a Transformation?

**Transformation** = Changing position, rotation, or size

Three basic types:
- **Translation**: Moving (shift left/right, up/down)
- **Rotation**: Spinning around a point
- **Scale**: Making bigger or smaller

### 2. What is a Matrix?

**Matrix** = A grid of numbers that represents a transformation

```
4×4 Matrix (16 numbers):

[ m00  m01  m02  m03 ]
[ m10  m11  m12  m13 ]
[ m20  m21  m22  m23 ]
[ m30  m31  m32  m33 ]
```

**Why matrices?**
- One matrix can represent rotation + translation + scale!
- Matrix multiplication is SUPER fast on GPU
- Standard method in all 3D graphics

### 3. What is a Uniform?

**Uniform** = Data sent from C# to shader that's THE SAME for all vertices

```
Vertex Attributes (different per vertex):
  Vertex 1: Position (0.0, 0.5, 0.0), Color RED
  Vertex 2: Position (-0.5, -0.5, 0.0), Color GREEN
  Vertex 3: Position (0.5, -0.5, 0.0), Color BLUE
  
Uniform (same for all vertices):
  Transform Matrix: [rotation by 45°]
  
ALL 3 vertices get rotated by the SAME angle!
```

---

## 🔧 How It Works

### The Pipeline

```
┌─────────────────────────────────────────────────────────┐
│                   ROTATION PIPELINE                     │
└─────────────────────────────────────────────────────────┘

FRAME 1:
C# Code (CPU):
  time = 0.0 seconds
  angle = 0.0 radians (0°)
  matrix = CreateRotationZ(0.0)
      ↓
Send to Shader:
  gl.UniformMatrix4fv(transformLocation, matrix)
      ↓
Vertex Shader (GPU):
  vertex1 = matrix × (0.0, 0.5, 0.0) → (0.0, 0.5, 0.0)  [no rotation yet]
  vertex2 = matrix × (-0.5, -0.5, 0.0) → (-0.5, -0.5, 0.0)
  vertex3 = matrix × (0.5, -0.5, 0.0) → (0.5, -0.5, 0.0)
      ↓
Triangle drawn at original position
─────────────────────────────────────────────────────────

FRAME 60 (1 second later at 60 FPS):
C# Code (CPU):
  time = 1.0 seconds
  angle = 1.0 radians (≈57°)
  matrix = CreateRotationZ(1.0)
      ↓
Send to Shader:
  gl.UniformMatrix4fv(transformLocation, matrix)
      ↓
Vertex Shader (GPU):
  vertex1 = matrix × (0.0, 0.5, 0.0) → (0.27, 0.42, 0.0)  [rotated!]
  vertex2 = matrix × (-0.5, -0.5, 0.0) → (-0.04, -0.70, 0.0)  [rotated!]
  vertex3 = matrix × (0.5, -0.5, 0.0) → (0.70, -0.04, 0.0)  [rotated!]
      ↓
Triangle drawn at ROTATED position!
```

This happens 60+ times per second → smooth spinning animation!

---

## 📊 Understanding Matrices (Beginner-Friendly!)

### What Does a Rotation Matrix Look Like?

For rotating around Z-axis by angle θ (theta):

```
[ cos(θ)  -sin(θ)   0   0 ]
[ sin(θ)   cos(θ)   0   0 ]
[   0        0      1   0 ]
[   0        0      0   1 ]
```

**Example**: Rotate by 90° (π/2 radians)
```
θ = 90°
cos(90°) = 0
sin(90°) = 1

Matrix:
[  0  -1   0   0 ]
[  1   0   0   0 ]
[  0   0   1   0 ]
[  0   0   0   1 ]

Apply to point (1, 0, 0):
  x' = (0×1) + (-1×0) = 0
  y' = (1×1) + (0×0) = 1
  Result: (0, 1, 0)
  
(1, 0) rotated 90° → (0, 1) ✅ Correct!
```

**Don't worry about the math!** The `Matrix4x4.CreateRotationZ()` function does it all for you!

---

## 🎓 Deep Dive: Uniform Variables

### What are Uniforms?

**Uniforms** are like global constants visible to ALL vertices/pixels in one draw call.

**Think of it like this:**
```
Vertex Attributes = Personal ID card (different for each person)
Uniforms = Room temperature (same for everyone in the room)
```

### Attribute vs Uniform

| Aspect | Vertex Attribute | Uniform |
|--------|------------------|---------|
| **Varies per** | Vertex | Draw call |
| **Example** | Position, Color | Transform, Time |
| **In C#** | VertexAttribPointer | UniformXX functions |
| **In Shader** | `in vec3 aPosition` | `uniform mat4 transform` |
| **Use for** | Per-vertex data | Global settings |

### How to Use Uniforms

**Step 1: Declare in shader**
```glsl
uniform mat4 transform;  // Declare uniform
```

**Step 2: Get location in C#**
```csharp
int loc = gl.GetUniformLocation(shaderProgram, "transform");
```

**Step 3: Set value in C#**
```csharp
gl.UniformMatrix4fv(loc, 1, false, matrixPointer);
```

### Common Uniform Types

```csharp
// Matrix (4×4)
gl.UniformMatrix4fv(location, 1, false, matrixPtr);

// Single float
gl.Uniform1f(location, 1.5f);

// Vector2 (2 floats)
gl.Uniform2f(location, x, y);

// Vector3 (3 floats)
gl.Uniform3f(location, r, g, b);

// Vector4 (4 floats)
gl.Uniform4f(location, r, g, b, a);

// Integer
gl.Uniform1i(location, 5);
```

---

## 🔄 Understanding Rotation

### 2D Rotation (What We're Doing)

We rotate around the **Z-axis** (perpendicular to screen):

```
     Before          After 45° rotation
     
       *                  *
      / \                / \
     /   \              /   \
    *-----*            *     *
    
    Straight       Tilted to the right
```

### Rotation Around Different Axes

```
X-Axis: Tilts forward/backward (like a door opening)
Y-Axis: Turns left/right (like a steering wheel)
Z-Axis: Spins clockwise/counter-clockwise (like a record player)
```

In 2D (this project), we only rotate around Z!

### Angles in Radians vs Degrees

OpenGL uses **radians**, not degrees!

```
Degrees → Radians:
  0°   = 0 rad
  90°  = π/2 ≈ 1.57 rad
  180° = π ≈ 3.14 rad
  270° = 3π/2 ≈ 4.71 rad
  360° = 2π ≈ 6.28 rad

Formula: radians = degrees × (π / 180)
```

**In our code**: Angle increases continuously (0, 0.1, 0.2, ... 6.28, 6.38, ...)  
After 2π (6.28), it keeps going → continuous rotation!

---

## 💡 Matrix Transformations Explained Simply

### The Magic of Matrix Multiplication

When you multiply a matrix by a position:
```
Matrix × Position = New Position
```

**Example**:
```
Original position: (0.5, 0.0, 0.0)  [point to the right]

Rotation matrix (90°) × position:
  Result: (0.0, 0.5, 0.0)  [point upward!]
```

The point **rotated 90 degrees**!

### Combining Multiple Transformations

You can combine transformations by multiplying matrices:

```csharp
// Separate transformations
Matrix4x4 rotation = Matrix4x4.CreateRotationZ(angle);
Matrix4x4 scale = Matrix4x4.CreateScale(0.5f);
Matrix4x4 translation = Matrix4x4.CreateTranslation(0.3f, 0.0f, 0.0f);

// Combine them!
Matrix4x4 finalTransform = scale * rotation * translation;

// ORDER MATTERS!
// rotation * scale ≠ scale * rotation
// Typically: Scale → Rotate → Translate (SRT order)
```

---

## 🔍 Common Questions

### Q: Why do we send the matrix every frame?

**A**: Because it CHANGES every frame! The rotation angle increases, so the matrix changes. We need to update the shader with the new rotation.

### Q: Can I use uniform for other things besides matrices?

**A**: YES! Uniforms are used for EVERYTHING that's the same across vertices:
- Current time
- Object colors
- Light positions
- Material properties
- Texture samplers
- Almost everything!

### Q: What's the difference between uniform and attribute?

**A**:
- **Attribute**: Different for EACH vertex (position, color, etc.)
- **Uniform**: SAME for ALL vertices in one draw call (transform, time, etc.)

### Q: Why use System.Numerics.Matrix4x4?

**A**: .NET has built-in matrix math! It provides:
- `CreateRotationX/Y/Z` - rotation matrices
- `CreateScale` - scale matrices
- `CreateTranslation` - translation matrices
- Matrix multiplication operators
- All the math you need!

### Q: Can I rotate around a different point?

**A**: Yes! You need to:
1. Translate to origin (0, 0)
2. Rotate
3. Translate back

Example in next project!

---

## 🧪 Experiments to Try

### 1. Change Rotation Speed

```csharp
// In OnRender:
float rotationSpeed = 0.5f;  // Slower
float rotationSpeed = 3.0f;  // Faster!
```

### 2. Rotate Backwards

```csharp
float angle = -totalTime * rotationSpeed;  // Negative = counter-clockwise
```

### 3. Scale While Rotating

```csharp
Matrix4x4 rot = Matrix4x4.CreateRotationZ(angle);
Matrix4x4 scale = Matrix4x4.CreateScale(0.7f);  // 70% size
Matrix4x4 transform = rot * scale;
```

### 4. Pulsating Size

```csharp
float sizeMultiplier = (float)(Math.Sin(totalTime * 2) * 0.3 + 0.7);
Matrix4x4 rot = Matrix4x4.CreateRotationZ(angle);
Matrix4x4 scale = Matrix4x4.CreateScale(sizeMultiplier);
Matrix4x4 transform = rot * scale;
// Triangle spins AND grows/shrinks!
```

### 5. Move While Rotating (Orbit!)

```csharp
float orbitRadius = 0.3f;
float orbitX = (float)Math.Cos(totalTime) * orbitRadius;
float orbitY = (float)Math.Sin(totalTime) * orbitRadius;

Matrix4x4 rot = Matrix4x4.CreateRotationZ(angle);
Matrix4x4 trans = Matrix4x4.CreateTranslation(orbitX, orbitY, 0);
Matrix4x4 transform = rot * trans;
// Triangle spins in a circle!
```

---

## 📖 Summary

### What You Learned

1. ✅ **Transformation Matrices** - How to move/rotate/scale objects
2. ✅ **Uniform Variables** - Sending data to shaders (same for all vertices)
3. ✅ **Matrix Multiplication** - Combining transformations
4. ✅ **Time-Based Animation** - Creating continuous motion
5. ✅ **Matrix4x4 class** - .NET's built-in matrix math

### Key Takeaways

```
Matrix = Instructions for transforming positions
Uniform = Global data sent to shader
Matrix × Position = Transformed Position
Time → Angle → Matrix → Rotation Animation!
```

### The Power of Matrices

**Without matrices**: To rotate 1000 vertices, calculate sin/cos 1000 times  
**With matrices**: Calculate matrix ONCE, GPU multiplies super fast!

Matrices are why modern games can render millions of vertices in real-time!

---

## 🎯 Next Steps

**Project 2.4: Multiple Shapes**
- Element Buffer Objects (EBO/IBO)
- Index buffers for efficiency
- Draw multiple objects
- Complete Phase 2!

---

## 🌟 Real-World Applications

Everything that moves in games uses transformation matrices:
- Character movement
- Camera rotation
- Object physics
- Particle effects
- UI animations
- EVERYTHING!

You've just learned how 3D games work! 🎮✨

---

**Congratulations!** You can now:
- Draw shapes ✅
- Color them with gradients ✅
- Rotate them smoothly ✅

One more project and Phase 2 is complete! 🚀


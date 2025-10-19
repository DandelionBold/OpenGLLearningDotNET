using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;
using System.Numerics;

// ============================================================================
// PROJECT 2.3: ROTATING TRIANGLE - TRANSFORMATION MATRICES!
// ============================================================================
//
// THIS PROJECT TEACHES:
// - Transformation matrices (rotation, translation, scale)
// - Uniform variables (passing data to shaders)
// - Time-based animation
// - The transformation pipeline
//
// WHAT'S NEW:
// - We calculate a ROTATION MATRIX in C# every frame
// - We send it to the vertex shader as a UNIFORM
// - The shader MULTIPLIES each vertex by the matrix
// - The triangle ROTATES! 🔄
//
// THE RESULT:
// A spinning gradient triangle! It never stops spinning!
// ============================================================================

namespace RotatingTriangle;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    
    private static uint vbo;
    private static uint vao;
    private static uint shaderProgram;
    
    // ========================================================================
    // TIME TRACKING (for rotation animation)
    // ========================================================================
    private static float totalTime = 0.0f;
    
    static void Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("PROJECT 2.3: ROTATING TRIANGLE!");
        Console.WriteLine("====================================\n");
        Console.WriteLine("Get ready to see your first TRANSFORMATION!");
        Console.WriteLine("The triangle will SPIN continuously!\n");
        
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "2.3 - Spinning Triangle (Transformations!)";
        
        window = Window.Create(options);
        
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClosing;
        
        window.Run();
    }
    
    private static void OnLoad()
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("LOADING: Creating Spinning Triangle");
        Console.WriteLine("═══════════════════════════════════════\n");
        
        gl = window!.CreateOpenGL();
        
        Console.WriteLine($"✓ OpenGL Version: {gl.GetStringS(StringName.Version)}\n");
        
        // ====================================================================
        // STEP 1: DEFINE VERTICES (with colors, like Project 2.2)
        // ====================================================================
        Console.WriteLine("STEP 1: Defining colored vertices");
        Console.WriteLine("───────────────────────────────────\n");
        
        float[] vertices = new float[]
        {
            // Position + Color (6 floats per vertex)
            // Top vertex - RED
             0.0f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,
            // Bottom-left - GREEN
            -0.5f, -0.5f,  0.0f,  0.0f,  1.0f,  0.0f,
            // Bottom-right - BLUE
             0.5f, -0.5f,  0.0f,  0.0f,  0.0f,  1.0f
        };
        
        Console.WriteLine("✓ 3 vertices with positions and colors defined\n");
        
        // ====================================================================
        // STEP 2: CREATE VAO & VBO (same as Project 2.2)
        // ====================================================================
        Console.WriteLine("STEP 2: Creating VAO and VBO");
        Console.WriteLine("──────────────────────────────\n");
        
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        
        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        
        unsafe
        {
            fixed (float* buf = vertices)
            {
                gl.BufferData(
                    BufferTargetARB.ArrayBuffer,
                    (nuint)(vertices.Length * sizeof(float)),
                    buf,
                    BufferUsageARB.StaticDraw
                );
            }
        }
        
        Console.WriteLine($"✓ Created VAO {vao} and VBO {vbo}");
        Console.WriteLine($"✓ Uploaded {vertices.Length} floats to GPU\n");
        
        // ====================================================================
        // STEP 3: CONFIGURE ATTRIBUTES (same as Project 2.2)
        // ====================================================================
        Console.WriteLine("STEP 3: Configuring vertex attributes");
        Console.WriteLine("───────────────────────────────────────\n");
        
        // Attribute 0: Position (X, Y, Z)
        unsafe
        {
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 
                                   6 * sizeof(float), (void*)0);
        }
        gl.EnableVertexAttribArray(0);
        
        // Attribute 1: Color (R, G, B)
        unsafe
        {
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false,
                                   6 * sizeof(float), (void*)(3 * sizeof(float)));
        }
        gl.EnableVertexAttribArray(1);
        
        Console.WriteLine("✓ Attribute 0: Position (3 floats, stride 24, offset 0)");
        Console.WriteLine("✓ Attribute 1: Color (3 floats, stride 24, offset 12)\n");
        
        // ====================================================================
        // STEP 4: LOAD SHADERS
        // ====================================================================
        Console.WriteLine("STEP 4: Loading shaders with transformation support");
        Console.WriteLine("──────────────────────────────────────────────────────\n");
        
        shaderProgram = CreateShaderProgram("Shaders/shader.vert", "Shaders/shader.frag");
        
        Console.WriteLine($"✓ Shader program {shaderProgram} created");
        Console.WriteLine("✓ Shaders support 'transform' uniform variable\n");
        
        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("SETUP COMPLETE!");
        Console.WriteLine("═══════════════════════════════════════\n");
        Console.WriteLine("You should see a SPINNING gradient triangle!");
        Console.WriteLine("Watch it rotate smoothly! 🔄\n");
    }
    
    private static void OnUpdate(double deltaTime)
    {
        // Accumulate time
        totalTime += (float)deltaTime;
    }
    
    private static void OnRender(double deltaTime)
    {
        // Clear screen
        gl!.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // ====================================================================
        // THE NEW PART: CREATE ROTATION MATRIX!
        // ====================================================================
        
        // Calculate rotation angle based on time
        // One full rotation per 3 seconds
        float rotationSpeed = 1.0f; // Adjust for faster/slower rotation
        float angle = totalTime * rotationSpeed; // Angle in radians
        
        // Create a rotation matrix around Z-axis (2D rotation)
        // Matrix4x4.CreateRotationZ creates a rotation matrix
        Matrix4x4 transform = Matrix4x4.CreateRotationZ(angle);
        
        // WHAT IS THIS MATRIX DOING?
        // It creates a 4×4 grid of numbers that represents:
        // "Rotate everything around the Z-axis by 'angle' radians"
        //
        // As time increases → angle increases → rotation increases → triangle spins!
        
        // ====================================================================
        // SEND THE MATRIX TO THE SHADER (UNIFORM VARIABLE!)
        // ====================================================================
        
        // First, use our shader program
        gl.UseProgram(shaderProgram);
        
        // NEW! Get the location of the "transform" uniform in the shader
        int transformLocation = gl.GetUniformLocation(shaderProgram, "transform");
        
        // WHAT IS GetUniformLocation?
        // It finds the "address" of the uniform variable in the shader
        // Think of it like looking up a variable's location in memory
        
        if (transformLocation == -1)
        {
            Console.WriteLine("WARNING: 'transform' uniform not found in shader!");
        }
        
        // NEW! Send the matrix to the shader
        unsafe
        {
            // UniformMatrix4 in Silk.NET (note: no 'fv' suffix)
            gl.UniformMatrix4(transformLocation, 1, false, (float*)&transform);
        }
        
        // WHAT IS UniformMatrix4fv?
        // - Uniform: Set a uniform variable
        // - Matrix4: It's a 4×4 matrix
        // - fv: Float values
        // Parameters:
        //   - Location: where to send it (transformLocation)
        //   - Count: 1 matrix
        //   - Transpose: false (don't flip rows/columns)
        //   - Pointer to data: the matrix
        
        // ====================================================================
        // DRAW THE TRIANGLE
        // ====================================================================
        gl.BindVertexArray(vao);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        
        // WHAT HAPPENS NOW:
        // 1. GPU executes vertex shader for each vertex
        // 2. Each vertex is multiplied by the rotation matrix
        // 3. Vertices are now at rotated positions!
        // 4. Triangle is drawn at the new rotated position
        // 5. Next frame, angle increases → triangle rotates more
        // 6. Continuous spinning! 🔄
    }
    
    private static void OnClosing()
    {
        Console.WriteLine("\n[Cleanup] Deleting GPU objects...");
        gl!.DeleteBuffer(vbo);
        gl.DeleteVertexArray(vao);
        gl.DeleteProgram(shaderProgram);
        gl?.Dispose();
        Console.WriteLine("[Cleanup] Done!");
    }
    
    // ========================================================================
    // HELPER FUNCTIONS
    // ========================================================================
    
    private static uint CreateShaderProgram(string vertexPath, string fragmentPath)
    {
        string vertexCode = File.ReadAllText(vertexPath);
        string fragmentCode = File.ReadAllText(fragmentPath);
        
        uint vertexShader = CompileShader(ShaderType.VertexShader, vertexCode);
        uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentCode);
        
        uint program = gl!.CreateProgram();
        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);
        
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            throw new Exception($"Shader linking failed: {gl.GetProgramInfoLog(program)}");
        }
        
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        
        return program;
    }
    
    private static uint CompileShader(ShaderType type, string source)
    {
        uint shader = gl!.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
        if (compileStatus == 0)
        {
            throw new Exception($"Shader compilation failed: {gl.GetShaderInfoLog(shader)}");
        }
        
        return shader;
    }
}

// ============================================================================
// 🎉 CONGRATULATIONS! 🎉
// ============================================================================
//
// YOU JUST LEARNED:
//
// 1. **TRANSFORMATION MATRICES** ⭐
//    - 4×4 grid of numbers that transform positions
//    - Can represent rotation, translation, scale, or all combined
//    - Matrix × Position = Transformed Position
//
// 2. **UNIFORM VARIABLES** ⭐
//    - Data sent from C# to shaders
//    - SAME value for all vertices (unlike vertex attributes)
//    - Perfect for: matrices, time, colors, settings, etc.
//
// 3. **MATRIX OPERATIONS**
//    - Matrix4x4.CreateRotationZ(angle) - rotate around Z axis
//    - Matrix multiplication in shader: transform * vec4(position, 1.0)
//    - GPU handles the math super fast!
//
// 4. **TIME-BASED TRANSFORMATIONS**
//    - Accumulate time in Update
//    - Calculate transform based on time
//    - Create smooth animations!
//
// KEY DIFFERENCES FROM PROJECT 2.2:
//
// Project 2.2:
//   - Static triangle (doesn't move)
//   - No transformations
//
// Project 2.3:
//   - Dynamic triangle (rotates!)
//   - Uses transformation matrix
//   - Introduces uniform variables
//
// THE PIPELINE NOW:
//
// C# (CPU):
//   Calculate rotation matrix based on time
//      ↓
// Send to GPU:
//   gl.UniformMatrix4fv(...) - upload matrix
//      ↓
// Vertex Shader (GPU):
//   Multiply each vertex by matrix → rotated position
//      ↓
// Fragment Shader (GPU):
//   Color the pixels (same as before)
//      ↓
// Screen:
//   Spinning triangle! 🔄
//
// EXPERIMENTS TO TRY:
//
// 1. Change rotation speed:
//    float rotationSpeed = 2.0f;  // Spin faster!
//    float rotationSpeed = 0.2f;  // Spin slower
//
// 2. Try different transformations:
//    Matrix4x4 transform = Matrix4x4.CreateScale(1.5f);  // Make bigger
//    Matrix4x4 transform = Matrix4x4.CreateTranslation(0.3f, 0.0f, 0.0f);  // Move right
//
// 3. Combine transformations:
//    Matrix4x4 rot = Matrix4x4.CreateRotationZ(angle);
//    Matrix4x4 scale = Matrix4x4.CreateScale(0.7f);
//    Matrix4x4 transform = rot * scale;  // Rotate AND scale!
//
// 4. Pulsating rotation:
//    float scale = (float)(Math.Sin(totalTime * 2) * 0.3 + 0.7);
//    Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scale);
//    Matrix4x4 transform = Matrix4x4.CreateRotationZ(angle) * scaleMatrix;
//    // Triangle spins AND pulses in size!
//
// NEXT: Project 2.4 - Multiple Shapes
// We'll learn Element Buffer Objects (indices) to draw shapes efficiently!
//
// ============================================================================

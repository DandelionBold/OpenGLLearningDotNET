using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;

// ============================================================================
// PROJECT 2.2: MULTI-COLOR TRIANGLE - GRADIENTS & INTERPOLATION!
// ============================================================================
//
// THIS PROJECT TEACHES:
// - How to add COLOR as a vertex attribute
// - How to configure VAO for multiple attributes
// - How GPU interpolation creates smooth gradients
// - The power of the graphics pipeline!
//
// WHAT'S NEW FROM PROJECT 2.1:
// - Before: 3 floats per vertex (X, Y, Z)
// - Now: 6 floats per vertex (X, Y, Z, R, G, B)
//
// THE RESULT:
// A triangle with RED top, GREEN bottom-left, BLUE bottom-right
// with BEAUTIFUL smooth color blending in between! 🌈
// ============================================================================

namespace MultiColorTriangle;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    
    private static uint vbo;
    private static uint vao;
    private static uint shaderProgram;
    
    static void Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("PROJECT 2.2: MULTI-COLOR TRIANGLE!");
        Console.WriteLine("====================================\n");
        Console.WriteLine("Get ready to see GPU INTERPOLATION in action!");
        Console.WriteLine("Your triangle will have smooth color gradients!\n");
        
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "2.2 - Gradient Triangle (Vertex Colors!)";
        
        window = Window.Create(options);
        
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Closing += OnClosing;
        
        window.Run();
    }
    
    private static void OnLoad()
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("LOADING: Creating Gradient Triangle");
        Console.WriteLine("═══════════════════════════════════════\n");
        
        gl = window!.CreateOpenGL();
        
        Console.WriteLine($"✓ OpenGL Version: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"✓ Graphics Card: {gl.GetStringS(StringName.Renderer)}\n");
        
        // ====================================================================
        // STEP 1: DEFINE VERTICES WITH COLORS!
        // ====================================================================
        Console.WriteLine("STEP 1: Defining vertices WITH colors");
        Console.WriteLine("───────────────────────────────────────\n");
        
        // THIS IS THE BIG CHANGE!
        // Before (Project 2.1): Each vertex had 3 floats (X, Y, Z)
        // Now (Project 2.2): Each vertex has 6 floats (X, Y, Z, R, G, B)
        
        float[] vertices = new float[]
        {
            // Vertex 1: TOP - RED
            //  X      Y      Z      R      G      B
             0.0f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f,   // Position + Red color
            
            // Vertex 2: BOTTOM-LEFT - GREEN
            //  X      Y      Z      R      G      B
            -0.5f, -0.5f,  0.0f,  0.0f,  1.0f,  0.0f,   // Position + Green color
            
            // Vertex 3: BOTTOM-RIGHT - BLUE
            //  X      Y      Z      R      G      B
             0.5f, -0.5f,  0.0f,  0.0f,  0.0f,  1.0f    // Position + Blue color
        };
        
        // UNDERSTANDING THE DATA:
        // We now have 3 vertices × 6 floats = 18 floats total
        // 
        // Each vertex is: [X, Y, Z, R, G, B]
        //                  └position┘ └color┘
        //
        // The GPU will automatically BLEND the colors between vertices!
        
        Console.WriteLine("Vertex 1 (Top):          Position( 0.0,  0.5, 0.0) + RED  (1.0, 0.0, 0.0)");
        Console.WriteLine("Vertex 2 (Bottom-Left):  Position(-0.5, -0.5, 0.0) + GREEN(0.0, 1.0, 0.0)");
        Console.WriteLine("Vertex 3 (Bottom-Right): Position( 0.5, -0.5, 0.0) + BLUE (0.0, 0.0, 1.0)");
        Console.WriteLine($"Total: {vertices.Length} floats = {vertices.Length * 4} bytes\n");
        
        // ====================================================================
        // STEP 2: CREATE VAO
        // ====================================================================
        Console.WriteLine("STEP 2: Creating VAO");
        Console.WriteLine("─────────────────────\n");
        
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        
        Console.WriteLine($"✓ Created and bound VAO {vao}\n");
        
        // ====================================================================
        // STEP 3: CREATE VBO AND UPLOAD DATA
        // ====================================================================
        Console.WriteLine("STEP 3: Creating VBO and uploading data");
        Console.WriteLine("────────────────────────────────────────\n");
        
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
        
        Console.WriteLine($"✓ Created VBO {vbo}");
        Console.WriteLine($"✓ Uploaded {vertices.Length} floats to GPU\n");
        
        // ====================================================================
        // STEP 4: CONFIGURE VERTEX ATTRIBUTES (THE IMPORTANT PART!)
        // ====================================================================
        Console.WriteLine("STEP 4: Configuring vertex attributes");
        Console.WriteLine("───────────────────────────────────────\n");
        
        // THIS IS WHERE WE TELL OPENGL:
        // "The data has BOTH position AND color per vertex!"
        
        // ----------------------------------------------------------------
        // ATTRIBUTE 0: POSITION (X, Y, Z)
        // ----------------------------------------------------------------
        Console.WriteLine("Configuring Attribute 0: POSITION");
        
        unsafe
        {
            gl.VertexAttribPointer(
                0,                              // Attribute location 0 (matches shader)
                3,                              // 3 components (X, Y, Z)
                VertexAttribPointerType.Float,  // Type: float
                false,                          // Don't normalize
                6 * sizeof(float),              // Stride: 6 floats (pos + color)
                (void*)0                        // Offset: starts at byte 0
            );
        }
        gl.EnableVertexAttribArray(0);
        
        // STRIDE EXPLANATION:
        // Stride = "How many bytes to skip to get to the NEXT vertex?"
        // 
        // Our data: [X₁ Y₁ Z₁ R₁ G₁ B₁] [X₂ Y₂ Z₂ R₂ G₂ B₂] [X₃...]
        //            └─ Vertex 1 ──────┘ └─ Vertex 2 ──────┘
        //
        // From X₁ to X₂, we skip 6 floats = 24 bytes
        // So stride = 6 * sizeof(float) = 24 bytes
        
        // OFFSET EXPLANATION:
        // Offset = "Where does this attribute start in EACH vertex?"
        // Position starts at the BEGINNING (byte 0)
        
        Console.WriteLine("  Location: 0");
        Console.WriteLine("  Components: 3 (X, Y, Z)");
        Console.WriteLine("  Type: Float");
        Console.WriteLine("  Stride: 24 bytes (6 floats)");
        Console.WriteLine("  Offset: 0 bytes (starts at beginning)");
        Console.WriteLine("  ✓ Enabled\n");
        
        // ----------------------------------------------------------------
        // ATTRIBUTE 1: COLOR (R, G, B) - NEW!
        // ----------------------------------------------------------------
        Console.WriteLine("Configuring Attribute 1: COLOR (NEW!)");
        
        unsafe
        {
            gl.VertexAttribPointer(
                1,                              // Attribute location 1 (matches shader)
                3,                              // 3 components (R, G, B)
                VertexAttribPointerType.Float,  // Type: float
                false,                          // Don't normalize
                6 * sizeof(float),              // Stride: 6 floats (same as position!)
                (void*)(3 * sizeof(float))      // Offset: starts at byte 12 (after X,Y,Z)
            );
        }
        gl.EnableVertexAttribArray(1);
        
        // OFFSET EXPLANATION:
        // Color starts AFTER position (3 floats later)
        // 
        // [X Y Z | R G B]
        //  0 4 8  12 16 20  ← byte positions
        //  └─┘    └─ color starts here (byte 12)
        //  position
        //
        // 3 floats × 4 bytes = 12 bytes offset
        
        Console.WriteLine("  Location: 1");
        Console.WriteLine("  Components: 3 (R, G, B)");
        Console.WriteLine("  Type: Float");
        Console.WriteLine("  Stride: 24 bytes (6 floats)");
        Console.WriteLine("  Offset: 12 bytes (after position)");
        Console.WriteLine("  ✓ Enabled\n");
        
        // VISUALIZING THE VERTEX FORMAT:
        Console.WriteLine("Complete vertex format:");
        Console.WriteLine("  [X₁ Y₁ Z₁ R₁ G₁ B₁] [X₂ Y₂ Z₂ R₂ G₂ B₂] [X₃ Y₃ Z₃ R₃ G₃ B₃]");
        Console.WriteLine("   └attr0─┘ └attr1─┘   └attr0─┘ └attr1─┘   └attr0─┘ └attr1─┘");
        Console.WriteLine("   Position  Color      Position  Color      Position  Color\n");
        
        // ====================================================================
        // STEP 5: LOAD SHADERS
        // ====================================================================
        Console.WriteLine("STEP 5: Loading shaders");
        Console.WriteLine("────────────────────────\n");
        
        shaderProgram = CreateShaderProgram("Shaders/shader.vert", "Shaders/shader.frag");
        
        Console.WriteLine($"✓ Shader program {shaderProgram} created\n");
        
        // Unbind
        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("SETUP COMPLETE!");
        Console.WriteLine("═══════════════════════════════════════\n");
        Console.WriteLine("You should now see a triangle with:");
        Console.WriteLine("  🔴 RED at the top");
        Console.WriteLine("  🟢 GREEN at bottom-left");
        Console.WriteLine("  🔵 BLUE at bottom-right");
        Console.WriteLine("  🌈 SMOOTH COLOR BLENDING in between!\n");
        Console.WriteLine("This is GPU INTERPOLATION in action! ✨\n");
    }
    
    private static void OnRender(double deltaTime)
    {
        // Clear screen
        gl!.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // Draw the gradient triangle!
        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        
        // WHAT'S HAPPENING:
        // 1. GPU reads 3 vertices from VBO
        // 2. Vertex shader processes each vertex:
        //    - Outputs position
        //    - Outputs color (RED, GREEN, or BLUE)
        // 3. GPU creates triangle and fills pixels
        // 4. GPU AUTOMATICALLY calculates intermediate colors:
        //    - Between RED and GREEN → YELLOW-ISH
        //    - Between GREEN and BLUE → CYAN-ISH
        //    - Between BLUE and RED → MAGENTA-ISH
        //    - Center of triangle → GRAY-ISH (mix of all 3)
        // 5. Fragment shader receives the interpolated color
        // 6. Pixel gets colored!
        //
        // All this happens in MICROSECONDS! 🚀
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
// 1. **Multiple Vertex Attributes**
//    - Position (attribute 0)
//    - Color (attribute 1)
//    - You can have many attributes per vertex!
//
// 2. **Stride and Offset**
//    - Stride: bytes between vertices (6 floats = 24 bytes)
//    - Offset: where each attribute starts in a vertex
//
// 3. **GPU INTERPOLATION** ⭐ MOST IMPORTANT!
//    - GPU automatically blends values between vertices
//    - This creates smooth gradients
//    - Works for ANY "out" variable in vertex shader
//    - This is how textures, lighting, and everything else works!
//
// 4. **Varying Variables**
//    - "out" in vertex shader
//    - "in" in fragment shader
//    - Automatically interpolated by GPU
//
// KEY TAKEAWAYS:
//
// - Before: One color for entire triangle (boring)
// - Now: Different color per vertex → smooth gradients (beautiful!)
// - The GPU does the hard work of blending
// - This is the foundation for ALL visual effects in games!
//
// WHAT THIS ENABLES:
//
// - Smooth lighting (coming in Phase 4!)
// - Texture mapping (coming in Phase 3!)
// - Normal mapping (coming in Phase 5!)
// - Essentially EVERYTHING in 3D graphics!
//
// EXPERIMENTS TO TRY:
//
// 1. Change vertex colors:
//    - Try all RED: (1, 0, 0) for all vertices
//    - Try grayscale: (0.5, 0.5, 0.5) shades
//    - Try rainbow: different bright colors
//
// 2. Add more vertices:
//    - Make a square (6 vertices = 2 triangles)
//    - Give each corner a different color!
//
// 3. Modify the fragment shader:
//    - Try the experiments listed in shader.frag
//    - Make black and white
//    - Invert colors
//    - Play around!
//
// NEXT: Project 2.3 - Rotating Triangle
// We'll learn transformation matrices to make the triangle SPIN! 🔄
//
// ============================================================================

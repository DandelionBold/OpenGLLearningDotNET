using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;

// ============================================================================
// PROJECT 2.1B: DRAWING TWO SHAPES - Understanding BINDING!
// ============================================================================
// 
// THIS EXAMPLE SHOWS:
// - How to draw MULTIPLE shapes (triangle + square)
// - How BINDING works (the state machine concept)
// - ONE VBO with ALL data
// - ONE VAO for same format
// - Drawing different parts with DrawArrays
//
// THIS ANSWERS YOUR QUESTION:
// "Do I need new VAO/VBO for each shape?"
// ANSWER: Not if they have the same format! Store all in ONE VBO!
// ============================================================================

namespace TwoShapes;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    
    // ========================================================================
    // ONLY ONE VBO AND ONE VAO!
    // ========================================================================
    // We're going to store BOTH shapes in ONE VBO
    // And use ONE VAO (because they have the same format: just positions)
    private static uint vbo;   // ONE buffer for BOTH shapes
    private static uint vao;   // ONE configuration for BOTH shapes
    private static uint shaderProgram;
    
    static void Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("DRAWING TWO SHAPES - ONE VBO!");
        Console.WriteLine("====================================\n");
        Console.WriteLine("This shows you the 'binding' concept");
        Console.WriteLine("Watch how we store triangle AND square in ONE VBO!\n");
        
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "2.1b - Triangle AND Square (ONE VBO!)";
        
        window = Window.Create(options);
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Closing += OnClosing;
        window.Run();
    }
    
    private static void OnLoad()
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("SETUP: Creating TWO shapes in ONE VBO");
        Console.WriteLine("═══════════════════════════════════════\n");
        
        gl = window!.CreateOpenGL();
        
        // ====================================================================
        // THE KEY: ALL DATA IN ONE ARRAY!
        // ====================================================================
        Console.WriteLine("STEP 1: Combining ALL shape data into ONE array");
        Console.WriteLine("─────────────────────────────────────────────────\n");
        
        float[] allShapes = new float[]
        {
            // ============================================================
            // TRIANGLE DATA (3 vertices = 9 floats)
            // ============================================================
            //   X      Y      Z
             0.5f,  0.5f,  0.0f,   // Triangle vertex 1 (top right area)
             0.2f,  0.0f,  0.0f,   // Triangle vertex 2
             0.8f,  0.0f,  0.0f,   // Triangle vertex 3
            
            // ============================================================
            // SQUARE DATA (6 vertices = 18 floats)
            // A square is made of 2 triangles
            // ============================================================
            // First triangle of square
            -0.7f,  0.5f,  0.0f,   // Top-left
            -0.7f,  0.0f,  0.0f,   // Bottom-left
            -0.3f,  0.5f,  0.0f,   // Top-right
            
            // Second triangle of square
            -0.3f,  0.5f,  0.0f,   // Top-right (again)
            -0.7f,  0.0f,  0.0f,   // Bottom-left (again)
            -0.3f,  0.0f,  0.0f    // Bottom-right
        };
        
        Console.WriteLine("Triangle data: vertices 0-2 (indices 0-8)");
        Console.WriteLine("  Vertex 0: ( 0.5,  0.5, 0.0)");
        Console.WriteLine("  Vertex 1: ( 0.2,  0.0, 0.0)");
        Console.WriteLine("  Vertex 2: ( 0.8,  0.0, 0.0)\n");
        
        Console.WriteLine("Square data: vertices 3-8 (indices 9-26)");
        Console.WriteLine("  First triangle: vertices 3, 4, 5");
        Console.WriteLine("  Second triangle: vertices 6, 7, 8\n");
        
        Console.WriteLine($"Total: {allShapes.Length} floats = {allShapes.Length * 4} bytes\n");
        
        // ====================================================================
        // CREATE ONE VAO
        // ====================================================================
        Console.WriteLine("STEP 2: Creating ONE VAO (one format for all)");
        Console.WriteLine("──────────────────────────────────────────────\n");
        
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);  // "Select this VAO"
        
        Console.WriteLine($"✓ Created and bound VAO {vao}\n");
        
        // ====================================================================
        // CREATE ONE VBO AND UPLOAD ALL DATA
        // ====================================================================
        Console.WriteLine("STEP 3: Creating ONE VBO with ALL shape data");
        Console.WriteLine("───────────────────────────────────────────────\n");
        
        vbo = gl.GenBuffer();
        
        // THIS IS THE KEY: "BIND" MEANS "SELECT AS CURRENT"
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        Console.WriteLine($"✓ Created VBO {vbo}");
        Console.WriteLine($"✓ BOUND VBO {vbo} ← This is now the 'CURRENTLY SELECTED' buffer");
        Console.WriteLine("  (Like opening a file or selecting a notebook)\n");
        
        // Upload ALL data to the CURRENTLY SELECTED buffer (vbo)
        unsafe
        {
            fixed (float* buf = allShapes)
            {
                gl.BufferData(
                    BufferTargetARB.ArrayBuffer,  // Affects CURRENTLY BOUND buffer
                    (nuint)(allShapes.Length * sizeof(float)),
                    buf,
                    BufferUsageARB.StaticDraw
                );
            }
        }
        
        Console.WriteLine($"✓ Uploaded ALL {allShapes.Length} floats to VBO {vbo}");
        Console.WriteLine("  This includes BOTH triangle AND square data!\n");
        
        // ====================================================================
        // DESCRIBE THE FORMAT (same for both shapes)
        // ====================================================================
        Console.WriteLine("STEP 4: Describing vertex format");
        Console.WriteLine("──────────────────────────────────\n");
        
        unsafe
        {
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (void*)0);
        }
        gl.EnableVertexAttribArray(0);
        
        Console.WriteLine("✓ Format: 3 floats per vertex (X, Y, Z)\n");
        
        // ====================================================================
        // LOAD SHADERS
        // ====================================================================
        shaderProgram = CreateShaderProgram("Shaders/shader.vert", "Shaders/shader.frag");
        Console.WriteLine($"✓ Shader program {shaderProgram} ready\n");
        
        // Unbind (optional, good practice)
        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("SETUP COMPLETE!");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("You should see:");
        Console.WriteLine("  - Orange TRIANGLE on the right");
        Console.WriteLine("  - Orange SQUARE on the left\n");
    }
    
    private static void OnRender(double deltaTime)
    {
        // Clear screen
        gl!.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // Use our shader and VAO
        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);
        
        // ====================================================================
        // THE MAGIC: DRAW DIFFERENT PARTS OF THE SAME VBO!
        // ====================================================================
        
        // Draw triangle (vertices 0, 1, 2 = first 3 vertices)
        gl.DrawArrays(
            PrimitiveType.Triangles,   // Drawing triangles
            0,                         // Start at vertex index 0
            3                          // Draw 3 vertices
        );
        // This draws the FIRST 3 vertices from the VBO
        // Which is our triangle data!
        
        // Draw square (vertices 3, 4, 5, 6, 7, 8 = next 6 vertices)
        gl.DrawArrays(
            PrimitiveType.Triangles,   // Drawing triangles
            3,                         // Start at vertex index 3 ← NOTICE THE OFFSET!
            6                          // Draw 6 vertices (2 triangles)
        );
        // This draws vertices 3-8 from the VBO
        // Which is our square data!
        
        // ====================================================================
        // UNDERSTANDING DrawArrays PARAMETERS:
        // ====================================================================
        // DrawArrays(primitive, START, COUNT)
        //
        // Our VBO has 9 vertices total:
        //   [0][1][2] [3][4][5][6][7][8]
        //    Triangle    Square
        //
        // First call: DrawArrays(Triangles, 0, 3)
        //   → Start at vertex 0, draw 3 vertices
        //   → Draws vertices [0][1][2] = TRIANGLE
        //
        // Second call: DrawArrays(Triangles, 3, 6)
        //   → Start at vertex 3, draw 6 vertices
        //   → Draws vertices [3][4][5][6][7][8] = SQUARE
        //
        // THE VBO STAYS THE SAME!
        // We're just drawing DIFFERENT PARTS of it!
        // ====================================================================
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
// KEY TAKEAWAYS:
// ============================================================================
//
// 1. **ONE VBO CAN HOLD MULTIPLE SHAPES!**
//    Just put all the data in one array
//
// 2. **DrawArrays lets you draw PARTS of the VBO**
//    DrawArrays(type, START, COUNT)
//    - START = which vertex to begin at
//    - COUNT = how many vertices to draw
//
// 3. **You DON'T need new VBO for each shape**
//    (Unless you want to update them separately or they have different formats)
//
// 4. **The "BINDING" concept**:
//    - gl.BindBuffer(vbo) = "Select this VBO as current"
//    - gl.BufferData(...) = "Upload to CURRENTLY SELECTED VBO"
//    - It's like opening a file before writing to it!
//
// 5. **Think of it like a desk**:
//    - Your desk can only have ONE document on it at a time
//    - BindBuffer = "Put this document on the desk"
//    - BufferData = "Write on the document currently on the desk"
//    - Bind different buffer = "Replace with different document"
//
// ============================================================================
//
// EXPERIMENTS TO TRY:
//
// 1. Add a THIRD shape (another triangle)
//    - Add 3 more vertices to allShapes array
//    - Call DrawArrays a third time with new START index
//
// 2. Change the square's position
//    - Modify the square vertices in allShapes
//
// 3. Make the triangle bigger
//    - Change its vertex positions
//
// 4. Try drawing just the square
//    - Comment out the first DrawArrays call
//
// ============================================================================
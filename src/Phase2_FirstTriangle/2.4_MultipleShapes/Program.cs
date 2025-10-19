using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;
using System.Numerics;

// ============================================================================
// PROJECT 2.4: MULTIPLE SHAPES - ELEMENT BUFFER OBJECTS (EBO)!
// ============================================================================
//
// THIS PROJECT TEACHES:
// - EBO/IBO (Element/Index Buffer Objects) - Efficient vertex reuse!
// - Drawing multiple different objects
// - Organizing complex scenes
// - Batch rendering basics
//
// THE BIG INNOVATION: INDEX BUFFERS!
// Before: Drawing a square = 6 vertices (2 triangles, duplicated corners)
// Now: Drawing a square = 4 vertices + 6 indices (no duplication!)
//
// THE RESULT:
// Multiple shapes (triangles, squares) drawn efficiently!
// ============================================================================

namespace MultipleShapes;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    
    // ========================================================================
    // GPU OBJECTS - Now with EBO!
    // ========================================================================
    private static uint vbo;   // Vertex Buffer (positions + colors)
    private static uint vao;   // Vertex Array (configuration)
    private static uint ebo;   // NEW! Element Buffer (indices)
    private static uint shaderProgram;
    
    private static float totalTime = 0.0f;
    
    static void Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("PROJECT 2.4: MULTIPLE SHAPES!");
        Console.WriteLine("====================================\n");
        Console.WriteLine("Learn how to draw shapes EFFICIENTLY using indices!");
        Console.WriteLine("We'll draw a square with only 4 vertices instead of 6!\n");
        
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "2.4 - Multiple Shapes (Index Buffers!)";
        
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
        Console.WriteLine("LOADING: Creating Multiple Shapes");
        Console.WriteLine("═══════════════════════════════════════\n");
        
        gl = window!.CreateOpenGL();
        
        Console.WriteLine($"✓ OpenGL Version: {gl.GetStringS(StringName.Version)}\n");
        
        // ====================================================================
        // STEP 1: DEFINE VERTICES (UNIQUE VERTICES ONLY!)
        // ====================================================================
        Console.WriteLine("STEP 1: Defining UNIQUE vertices (no duplication!)");
        Console.WriteLine("────────────────────────────────────────────────────\n");
        
        // THE BIG DIFFERENCE: We only define each corner ONCE!
        // No need to repeat vertices for shared corners!
        
        float[] vertices = new float[]
        {
            // SQUARE (4 unique vertices)
            // Position           Color (gradient)
            // X      Y     Z     R     G     B
            -0.5f,  0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  // [0] Top-left - RED
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  // [1] Bottom-left - GREEN
             0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  // [2] Bottom-right - BLUE
             0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f   // [3] Top-right - YELLOW
        };
        
        // COMPARISON:
        //
        // WITHOUT INDEX BUFFER (Old way - Project 2.1b):
        //   To draw a square, we need 6 vertices (2 triangles):
        //   Triangle 1: Top-left, Bottom-left, Top-right
        //   Triangle 2: Top-right, Bottom-left, Bottom-right
        //   → 6 vertices total (with duplicates!)
        //
        // WITH INDEX BUFFER (New way - This project!):
        //   We define 4 unique vertices
        //   Then tell GPU which ones to connect using INDICES
        //   → Only 4 vertices! 33% less data!
        
        Console.WriteLine("Square defined with 4 unique vertices:");
        Console.WriteLine("  [0] Top-left:     (-0.5,  0.5) RED");
        Console.WriteLine("  [1] Bottom-left:  (-0.5, -0.5) GREEN");
        Console.WriteLine("  [2] Bottom-right: ( 0.5, -0.5) BLUE");
        Console.WriteLine("  [3] Top-right:    ( 0.5,  0.5) YELLOW");
        Console.WriteLine($"  Total: {vertices.Length} floats = {vertices.Length * 4} bytes\n");
        
        // ====================================================================
        // STEP 2: DEFINE INDICES (THE NEW PART!)
        // ====================================================================
        Console.WriteLine("STEP 2: Defining indices (how to connect vertices)");
        Console.WriteLine("────────────────────────────────────────────────────\n");
        
        // INDICES tell OpenGL which vertices to connect to form triangles
        // Think of it like "connect-the-dots" instructions!
        
        uint[] indices = new uint[]
        {
            // First triangle (top-left half of square)
            0, 1, 3,   // Connect vertices 0 → 1 → 3
            
            // Second triangle (bottom-right half of square)
            1, 2, 3    // Connect vertices 1 → 2 → 3
        };
        
        // VISUAL EXPLANATION:
        //
        // Our 4 vertices:
        //   [0]----[3]
        //    |      |
        //    |      |
        //   [1]----[2]
        //
        // First triangle (0, 1, 3):
        //   [0]----[3]
        //    |\     |
        //    | \    |
        //   [1]  X  |
        //
        // Second triangle (1, 2, 3):
        //   [0]    X[3]
        //    |    / |
        //    |   /  |
        //   [1]----[2]
        //
        // Together they form a square! █
        
        Console.WriteLine("Indices defined (connect-the-dots instructions):");
        Console.WriteLine("  Triangle 1: vertices [0, 1, 3] (top-left half)");
        Console.WriteLine("  Triangle 2: vertices [1, 2, 3] (bottom-right half)");
        Console.WriteLine($"  Total: {indices.Length} indices = {indices.Length * 4} bytes\n");
        
        Console.WriteLine("Vertex Savings:");
        Console.WriteLine("  Without indices: 6 vertices × 6 floats = 36 floats = 144 bytes");
        Console.WriteLine("  With indices:    4 vertices × 6 floats = 24 floats = 96 bytes");
        Console.WriteLine("                   + 6 indices × 1 uint = 6 uints = 24 bytes");
        Console.WriteLine("                   Total = 120 bytes (16% savings!)");
        Console.WriteLine("  For complex models with thousands of vertices, this is HUGE!\n");
        
        // ====================================================================
        // STEP 3: CREATE VAO & VBO (same as before)
        // ====================================================================
        Console.WriteLine("STEP 3: Creating VAO and VBO");
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
        
        Console.WriteLine($"✓ Created VAO {vao}");
        Console.WriteLine($"✓ Created VBO {vbo}");
        Console.WriteLine($"✓ Uploaded {vertices.Length} floats to VBO\n");
        
        // ====================================================================
        // STEP 4: CREATE EBO (NEW!)
        // ====================================================================
        Console.WriteLine("STEP 4: Creating EBO (Element Buffer Object) - NEW!");
        Console.WriteLine("─────────────────────────────────────────────────────\n");
        
        // EBO = Element Buffer Object (also called IBO = Index Buffer Object)
        // It stores the INDICES that tell OpenGL which vertices to connect
        
        ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        
        // IMPORTANT: We bind to ElementArrayBuffer, not ArrayBuffer!
        // - ArrayBuffer = vertex data (positions, colors, etc.)
        // - ElementArrayBuffer = indices (which vertices to use)
        
        unsafe
        {
            fixed (uint* buf = indices)
            {
                gl.BufferData(
                    BufferTargetARB.ElementArrayBuffer,   // Note: ElementArrayBuffer!
                    (nuint)(indices.Length * sizeof(uint)),
                    buf,
                    BufferUsageARB.StaticDraw
                );
            }
        }
        
        Console.WriteLine($"✓ Created EBO {ebo}");
        Console.WriteLine($"✓ Uploaded {indices.Length} indices to EBO");
        Console.WriteLine("✓ EBO bound to VAO (VAO remembers it!)\n");
        
        // IMPORTANT NOTE:
        // When we bind EBO while a VAO is active, the VAO "remembers" the EBO
        // This means when we later bind the VAO, the EBO is automatically bound too!
        
        // ====================================================================
        // STEP 5: CONFIGURE ATTRIBUTES (same as Project 2.2 & 2.3)
        // ====================================================================
        Console.WriteLine("STEP 5: Configuring vertex attributes");
        Console.WriteLine("───────────────────────────────────────\n");
        
        // Position attribute
        unsafe
        {
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
                                   6 * sizeof(float), (void*)0);
        }
        gl.EnableVertexAttribArray(0);
        
        // Color attribute
        unsafe
        {
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false,
                                   6 * sizeof(float), (void*)(3 * sizeof(float)));
        }
        gl.EnableVertexAttribArray(1);
        
        Console.WriteLine("✓ Attribute 0: Position");
        Console.WriteLine("✓ Attribute 1: Color\n");
        
        // ====================================================================
        // STEP 6: LOAD SHADERS
        // ====================================================================
        shaderProgram = CreateShaderProgram("Shaders/shader.vert", "Shaders/shader.frag");
        Console.WriteLine($"✓ Shader program {shaderProgram} ready\n");
        
        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("SETUP COMPLETE!");
        Console.WriteLine("═══════════════════════════════════════\n");
        Console.WriteLine("You should see a SPINNING GRADIENT SQUARE!");
        Console.WriteLine("Notice: Only 4 vertices, but 2 triangles drawn!");
        Console.WriteLine("That's the power of index buffers! 🚀\n");
    }
    
    private static void OnUpdate(double deltaTime)
    {
        totalTime += (float)deltaTime;
    }
    
    private static void OnRender(double deltaTime)
    {
        gl!.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // Create rotation matrix
        float angle = totalTime;
        Matrix4x4 transform = Matrix4x4.CreateRotationZ(angle);
        
        // Send to shader
        gl.UseProgram(shaderProgram);
        int transformLoc = gl.GetUniformLocation(shaderProgram, "transform");
        unsafe
        {
            gl.UniformMatrix4(transformLoc, 1, false, (float*)&transform);
        }
        
        // ====================================================================
        // DRAW USING INDICES (THE NEW WAY!)
        // ====================================================================
        gl.BindVertexArray(vao);
        
        // NEW! Use DrawElements instead of DrawArrays
        unsafe
        {
            gl.DrawElements(
                PrimitiveType.Triangles,          // Draw triangles
                6,                                 // 6 indices (2 triangles × 3 indices)
                DrawElementsType.UnsignedInt,     // Index type (uint)
                (void*)0                           // Offset in index buffer
            );
        }
        
        // WHAT'S THE DIFFERENCE?
        //
        // DrawArrays (old way):
        //   "Draw vertices 0, 1, 2, then 3, 4, 5"
        //   Uses vertices in sequential order
        //
        // DrawElements (new way):
        //   "Look at the index buffer, it tells you which vertices to use"
        //   Index buffer says: [0, 1, 3, 1, 2, 3]
        //   So it draws: vertex 0, vertex 1, vertex 3, then vertex 1, vertex 2, vertex 3
        //   Vertices can be REUSED!
        //
        // WHY THIS IS BETTER:
        // - Less memory (no duplicate vertices)
        // - Faster (less data to transfer)
        // - Essential for complex 3D models (cubes, spheres, etc.)
        
        // ====================================================================
        // WHAT HAPPENS WHEN DrawElements RUNS:
        // ====================================================================
        // 1. GPU reads indices from EBO: [0, 1, 3, 1, 2, 3]
        // 2. For each index, fetch the corresponding vertex from VBO
        // 3. Process 6 vertices through vertex shader (some are same vertex!)
        // 4. Form 2 triangles
        // 5. Rasterize and color → Square on screen!
    }
    
    private static void OnClosing()
    {
        Console.WriteLine("\n[Cleanup] Deleting GPU objects...");
        gl!.DeleteBuffer(vbo);
        gl.DeleteBuffer(ebo);  // Don't forget to delete EBO!
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
// 🎉 CONGRATULATIONS! PHASE 2 COMPLETE! 🎉
// ============================================================================
//
// YOU JUST LEARNED:
//
// 1. **EBO (Element Buffer Object)** / **IBO (Index Buffer Object)** ⭐
//    - Stores indices instead of duplicating vertices
//    - Dramatically reduces memory usage
//    - Essential for complex 3D models
//    - A cube has 8 corners but 12 triangles (36 vertices without indices!)
//    - With indices: only 8 vertices + 36 indices!
//
// 2. **DrawElements vs DrawArrays**
//    - DrawArrays: Use vertices in sequential order
//    - DrawElements: Use vertices in order specified by indices
//    - DrawElements is more efficient for complex shapes
//
// 3. **Index Buffers**
//    - Separate buffer type (ElementArrayBuffer)
//    - Contains indices (uint) not vertex data (float)
//    - VAO remembers the bound EBO
//    - When you bind VAO, EBO is automatically bound too!
//
// 4. **Efficient Shape Drawing**
//    - Square: 4 vertices + 6 indices (not 6 vertices!)
//    - Cube: 8 vertices + 36 indices (not 36 vertices!)
//    - Sphere: Hundreds of vertices, thousands of indices
//
// PHASE 2 RECAP - WHAT YOU MASTERED:
//
// ✅ Project 2.1: Basic triangle, VBO, VAO, Shaders
// ✅ Project 2.2: Vertex colors, interpolation, gradients
// ✅ Project 2.3: Transformations, matrices, uniforms, rotation
// ✅ Project 2.4: Index buffers, EBO, efficient rendering
//
// YOU NOW KNOW:
// - How to send data to GPU
// - How to organize that data (VAO)
// - How to write GPU programs (shaders)
// - How to transform objects (matrices)
// - How to draw efficiently (indices)
// - THE COMPLETE GRAPHICS PIPELINE!
//
// EXPERIMENTS TO TRY:
//
// 1. Add a second shape (triangle) alongside the square:
//    - Add 3 more vertices to the vertices array
//    - Add 3 more indices to the indices array
//    - Both shapes will spin together!
//
// 2. Draw a hexagon (6-sided shape):
//    - 7 vertices (6 corners + 1 center)
//    - 18 indices (6 triangles × 3 indices)
//    - Each triangle goes from center to two corners
//
// 3. Spin shapes at different speeds:
//    - Create two transformation matrices with different angles
//    - Use multiple draw calls with different uniforms
//
// 4. Make shapes orbit each other:
//    - Use CreateTranslation to move shapes
//    - Combine rotation + translation
//
// NEXT PHASE: PHASE 3 - 2D GRAPHICS MASTERY!
// - Texture loading and mapping
// - Sprites and sprite sheets
// - 2D transformations
// - Building a simple 2D game!
//
// ============================================================================

using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;

// ============================================================================
// PROJECT 2.1: YOUR FIRST TRIANGLE!
// ============================================================================
// This is a HUGE milestone! You're about to draw actual geometry!
// 
// WHAT YOU'LL LEARN:
// - Vertex Buffer Objects (VBO) - how to send data to GPU
// - Vertex Array Objects (VAO) - how to describe that data
// - Shaders - programs that run on the GPU
// - The graphics pipeline
// - Drawing your first triangle!
// ============================================================================

namespace ColoredTriangle;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    
    // ========================================================================
    // OpenGL OBJECTS
    // ========================================================================
    // These are handles (IDs) to objects stored on the GPU
    private static uint vbo; // Vertex Buffer Object - stores vertex data
    private static uint vao; // Vertex Array Object - describes vertex format
    private static uint shaderProgram; // Compiled shader program
    
    static void Main(string[] args)
    {
        Console.WriteLine("Starting OpenGL Application...");
        Console.WriteLine("Phase 2.1: Your First Triangle!");
        Console.WriteLine("================================\n");
        Console.WriteLine("This is it! Your first real graphics programming!\n");
        
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "2.1 - My First Triangle!";
        
        window = Window.Create(options);
        
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Closing += OnClosing;
        
        window.Run();
        
        Console.WriteLine("\nApplication closed successfully!");
    }
    
    private static void OnLoad()
    {
        Console.WriteLine("[OnLoad] Initializing OpenGL...");
        gl = window!.CreateOpenGL();
        
        Console.WriteLine($"[OnLoad] OpenGL Version: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"[OnLoad] GLSL Version: {gl.GetStringS(StringName.ShadingLanguageVersion)}\n");
        
        // ====================================================================
        // STEP 1: DEFINE THE TRIANGLE VERTICES
        // ====================================================================
        // Each vertex has 3 components: X, Y, Z
        // We're in 2D, so Z is always 0
        // OpenGL screen coordinates go from -1 to 1
        
        float[] vertices = new float[]
        {
            // X     Y      Z
             0.0f,  0.5f,  0.0f,  // Top vertex
            -0.5f, -0.5f,  0.0f,  // Bottom-left vertex
             0.5f, -0.5f,  0.0f   // Bottom-right vertex
        };
        
        Console.WriteLine("[OnLoad] Triangle vertices defined:");
        Console.WriteLine("  Top:          ( 0.0,  0.5, 0.0)");
        Console.WriteLine("  Bottom-Left:  (-0.5, -0.5, 0.0)");
        Console.WriteLine("  Bottom-Right: ( 0.5, -0.5, 0.0)\n");
        
        // ====================================================================
        // STEP 2: CREATE AND BIND VAO
        // ====================================================================
        // VAO (Vertex Array Object) remembers how to interpret vertex data
        // Think of it as a "configuration" that we can switch between
        
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        
        Console.WriteLine($"[OnLoad] Created VAO (ID: {vao})");
        
        // ====================================================================
        // STEP 3: CREATE AND FILL VBO
        // ====================================================================
        // VBO (Vertex Buffer Object) stores the actual vertex data on the GPU
        
        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        
        // Copy our vertex data to the GPU
        unsafe
        {
            fixed (float* buf = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, 
                             (nuint)(vertices.Length * sizeof(float)), 
                             buf, 
                             BufferUsageARB.StaticDraw);
            }
        }
        
        Console.WriteLine($"[OnLoad] Created VBO (ID: {vbo})");
        Console.WriteLine($"[OnLoad] Uploaded {vertices.Length} floats ({vertices.Length * sizeof(float)} bytes) to GPU\n");
        
        // ====================================================================
        // STEP 4: DESCRIBE THE VERTEX FORMAT
        // ====================================================================
        // Tell OpenGL how to interpret the data in the VBO
        // Each vertex has 3 floats (X, Y, Z)
        
        unsafe
        {
            gl.VertexAttribPointer(
                0,                              // Attribute location (matches shader)
                3,                              // Number of components (X, Y, Z = 3)
                VertexAttribPointerType.Float,  // Type of data
                false,                          // Don't normalize
                3 * sizeof(float),              // Stride (bytes between vertices)
                (void*)0                        // Offset (start at beginning)
            );
        }
        gl.EnableVertexAttribArray(0);      // Enable attribute 0
        
        Console.WriteLine("[OnLoad] Vertex attribute configured:");
        Console.WriteLine("  Location: 0");
        Console.WriteLine("  Components: 3 (X, Y, Z)");
        Console.WriteLine("  Type: Float");
        Console.WriteLine($"  Stride: {3 * sizeof(float)} bytes\n");
        
        // ====================================================================
        // STEP 5: LOAD AND COMPILE SHADERS
        // ====================================================================
        
        Console.WriteLine("[OnLoad] Loading shaders...");
        shaderProgram = CreateShaderProgram("Shaders/shader.vert", "Shaders/shader.frag");
        Console.WriteLine($"[OnLoad] Shader program created (ID: {shaderProgram})\n");
        
        // Unbind (optional but good practice)
        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        
        Console.WriteLine("[OnLoad] Setup complete! Ready to render.\n");
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine("You should see an orange triangle!");
        Console.WriteLine("=".PadRight(50, '=') + "\n");
    }
    
    private static void OnRender(double deltaTime)
    {
        // ====================================================================
        // CLEAR THE SCREEN
        // ====================================================================
        gl!.ClearColor(0.1f, 0.1f, 0.1f, 1.0f); // Dark gray background
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // ====================================================================
        // DRAW THE TRIANGLE
        // ====================================================================
        
        // 1. Use our shader program
        gl.UseProgram(shaderProgram);
        
        // 2. Bind our VAO (this sets up the vertex format)
        gl.BindVertexArray(vao);
        
        // 3. DRAW!
        gl.DrawArrays(PrimitiveType.Triangles,  // What to draw
                     0,                         // Start at vertex 0
                     3);                        // Draw 3 vertices
        
        // That's it! OpenGL now:
        // 1. Sends our 3 vertices to the vertex shader
        // 2. Creates a triangle between them
        // 3. Runs the fragment shader for every pixel inside
        // 4. Displays the result!
    }
    
    private static void OnClosing()
    {
        Console.WriteLine("\n[OnClosing] Cleaning up GPU resources...");
        
        // Delete GPU objects
        gl!.DeleteBuffer(vbo);
        gl.DeleteVertexArray(vao);
        gl.DeleteProgram(shaderProgram);
        
        Console.WriteLine("[OnClosing] Deleted VBO, VAO, and shader program");
        
        gl?.Dispose();
        Console.WriteLine("[OnClosing] Cleanup complete!");
    }
    
    // ========================================================================
    // SHADER LOADING FUNCTIONS
    // ========================================================================
    
    private static uint CreateShaderProgram(string vertexPath, string fragmentPath)
    {
        // Load shader source code from files
        string vertexCode = File.ReadAllText(vertexPath);
        string fragmentCode = File.ReadAllText(fragmentPath);
        
        // Compile shaders
        uint vertexShader = CompileShader(ShaderType.VertexShader, vertexCode);
        uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentCode);
        
        // Create program and link shaders
        uint program = gl!.CreateProgram();
        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);
        
        // Check for linking errors
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string infoLog = gl.GetProgramInfoLog(program);
            Console.WriteLine($"[ERROR] Shader program linking failed:\n{infoLog}");
            throw new Exception("Shader program linking failed!");
        }
        
        // Clean up - we don't need the individual shaders anymore
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        
        Console.WriteLine($"  ✓ Vertex shader compiled");
        Console.WriteLine($"  ✓ Fragment shader compiled");
        Console.WriteLine($"  ✓ Shaders linked into program");
        
        return program;
    }
    
    private static uint CompileShader(ShaderType type, string source)
    {
        uint shader = gl!.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        
        // Check for compilation errors
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
        if (compileStatus == 0)
        {
            string infoLog = gl.GetShaderInfoLog(shader);
            string shaderType = type == ShaderType.VertexShader ? "Vertex" : "Fragment";
            Console.WriteLine($"[ERROR] {shaderType} shader compilation failed:\n{infoLog}");
            throw new Exception($"{shaderType} shader compilation failed!");
        }
        
        return shader;
    }
}

// ============================================================================
// CONGRATULATIONS! 🎉🎉🎉
// ============================================================================
// If you see a triangle on screen, you've just crossed a MAJOR threshold!
// 
// YOU NOW UNDERSTAND:
// 
// 1. THE GRAPHICS PIPELINE:
//    CPU → VBO (GPU Memory) → Vertex Shader → Triangle Assembly →
//    → Rasterization → Fragment Shader → Screen
// 
// 2. VERTEX BUFFER OBJECTS (VBO):
//    - Store vertex data on the GPU
//    - Much faster than sending data every frame
//    - Created with GenBuffer, filled with BufferData
// 
// 3. VERTEX ARRAY OBJECTS (VAO):
//    - Describe the format of vertex data
//    - Tell OpenGL how to interpret the VBO
//    - Can switch between different formats instantly
// 
// 4. SHADERS:
//    - Programs that run on the GPU
//    - Vertex Shader: Processes each vertex
//    - Fragment Shader: Colors each pixel
//    - Written in GLSL (OpenGL Shading Language)
// 
// 5. THE RENDERING PROCESS:
//    - BindVertexArray (set format)
//    - UseProgram (set shaders)
//    - DrawArrays (DRAW!)
// 
// THE PIPELINE IN DETAIL:
// 
// 1. You send 3 vertices to the GPU (via VBO)
// 2. Vertex shader runs 3 times (once per vertex)
// 3. OpenGL connects the vertices into a triangle
// 4. OpenGL figures out which pixels are inside (rasterization)
// 5. Fragment shader runs for EACH pixel inside the triangle
// 6. The colored pixels appear on screen!
// 
// EXPERIMENTS TO TRY:
// 
// 1. Change the vertex positions - make the triangle bigger or smaller
// 2. Change the fragment shader color
// 3. Add a 4th vertex (it won't do anything - why?)
// 4. Try moving vertices outside -1 to 1 range
// 5. Make the triangle point down instead of up
// 
// NEXT UP: Project 2.2 - Multi-Color Triangle
// We'll learn how to pass color data per-vertex and create gradients!
// ============================================================================

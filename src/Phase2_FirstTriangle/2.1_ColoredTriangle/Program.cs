using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;

// ============================================================================
// PROJECT 2.1: YOUR FIRST TRIANGLE - BEGINNER EDITION
// ============================================================================
// 
// THIS IS A BIG JUMP! Don't worry if it seems complex at first.
// Read every comment carefully. I'll explain EVERYTHING step by step.
//
// WHAT'S DIFFERENT FROM PROJECT 1.1-1.3?
// - Before: We only CLEARED the screen with colors
// - Now: We're going to DRAW ACTUAL SHAPES on the GPU!
//
// THE BIG PICTURE:
// 1. We send triangle data FROM C# (CPU) TO GPU memory
// 2. We tell the GPU how to process it (using shaders)
// 3. The GPU draws the triangle super fast
// 4. We see it on screen!
// ============================================================================

namespace ColoredTriangle;

class Program
{
    // ========================================================================
    // VARIABLES (Global state for our application)
    // ========================================================================
    private static IWindow? window;  // The window (same as before)
    private static GL? gl;           // OpenGL context (same as before)
    
    // ========================================================================
    // NEW! OpenGL OBJECT HANDLES
    // ========================================================================
    // These are NUMBERS (IDs) that refer to objects stored on the GPU.
    // Think of them like "lockers" in GPU memory - we store data there.
    
    // VBO = "Vertex Buffer Object"
    // This stores our triangle's vertex data (positions) ON THE GPU
    // It's like uploading a file to the GPU's memory
    private static uint vbo;
    
    // VAO = "Vertex Array Object"  
    // This describes HOW to read the data in the VBO
    // It's like instructions: "The VBO contains 3 numbers per vertex (X, Y, Z)"
    private static uint vao;
    
    // Shader Program = Compiled GPU programs
    // These are the vertex.shader and fragment.shader files, compiled and linked
    private static uint shaderProgram;
    
    // WHY UINT?
    // "uint" = unsigned integer = positive whole number (1, 2, 3, ...)
    // OpenGL uses IDs (numbers) to refer to GPU objects
    
    static void Main(string[] args)
    {
        Console.WriteLine("================================");
        Console.WriteLine("PROJECT 2.1: YOUR FIRST TRIANGLE!");
        Console.WriteLine("================================\n");
        Console.WriteLine("This is the MOST IMPORTANT graphics programming lesson!");
        Console.WriteLine("We're going to draw a triangle using the GPU.\n");
        Console.WriteLine("Take your time reading the code comments.");
        Console.WriteLine("Every line is explained!\n");
        
        // Create window (same as before)
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "2.1 - My First Triangle! [Read the code comments]";
        
        window = Window.Create(options);
        
        // Register event handlers (same as before)
        window.Load += OnLoad;      // Called once at startup
        window.Render += OnRender;  // Called every frame
        window.Closing += OnClosing;// Called at shutdown
        
        // Start the application loop
        window.Run();
        
        Console.WriteLine("\nApplication closed!");
    }
    
    // ========================================================================
    // ONLOAD - Called ONCE when the window opens
    // ========================================================================
    // This is where we set up everything we need for rendering
    // ========================================================================
    private static void OnLoad()
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("LOADING: Setting up OpenGL and Triangle");
        Console.WriteLine("═══════════════════════════════════════\n");
        
        // Initialize OpenGL (same as before)
        gl = window!.CreateOpenGL();
        
        Console.WriteLine($"✓ OpenGL Version: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"✓ Shader Language Version: {gl.GetStringS(StringName.ShadingLanguageVersion)}");
        Console.WriteLine($"✓ Graphics Card: {gl.GetStringS(StringName.Renderer)}\n");
        
        // ====================================================================
        // STEP 1: DEFINE THE TRIANGLE (in CPU memory, in C#)
        // ====================================================================
        Console.WriteLine("STEP 1: Defining triangle vertices in C# (CPU)");
        Console.WriteLine("─────────────────────────────────────────────\n");
        
        // Each vertex (corner point) has 3 numbers: X, Y, Z
        // We need 3 vertices to make a triangle
        // So total: 3 vertices × 3 numbers = 9 floats
        
        float[] vertices = new float[]
        {
            // Vertex 1 (Top point)
            //  X      Y      Z
             0.0f,  0.5f,  0.0f,   // Top center of screen
            
            // Vertex 2 (Bottom-left point)
            // X       Y      Z
            -0.5f, -0.5f,  0.0f,   // Bottom-left
            
            // Vertex 3 (Bottom-right point)
            // X      Y      Z
             0.5f, -0.5f,  0.0f    // Bottom-right
        };
        
        // COORDINATE EXPLANATION:
        // OpenGL screen goes from -1 to +1 in both X and Y
        //   -1.0 ← → +1.0  (X axis - left to right)
        //   -1.0 ↓ ↑ +1.0  (Y axis - bottom to top)
        //   Center is at (0.0, 0.0)
        //
        // Our triangle:
        //   Top vertex:    ( 0.0,  0.5) = Slightly above center
        //   Bottom-left:   (-0.5, -0.5) = Left side, below center
        //   Bottom-right:  ( 0.5, -0.5) = Right side, below center
        
        Console.WriteLine("Triangle Vertices (positions):");
        Console.WriteLine("  Vertex 1: ( 0.0,  0.5, 0.0) ← Top");
        Console.WriteLine("  Vertex 2: (-0.5, -0.5, 0.0) ← Bottom-Left");
        Console.WriteLine("  Vertex 3: ( 0.5, -0.5, 0.0) ← Bottom-Right");
        Console.WriteLine($"  Total: {vertices.Length} floats = {vertices.Length * 4} bytes\n");
        
        // ====================================================================
        // STEP 2: CREATE VAO (Vertex Array Object)
        // ====================================================================
        Console.WriteLine("STEP 2: Creating VAO (Vertex Array Object)");
        Console.WriteLine("────────────────────────────────────────────\n");
        
        // WHAT IS A VAO?
        // Think of it as a "configuration object" that remembers:
        // - Which VBO to use
        // - How to interpret the data in that VBO
        // - Which vertex attributes are enabled
        //
        // WHY BIND?
        // "Binding" is like "selecting" or "activating"
        // After we bind a VAO, all vertex setup commands affect that VAO
        
        vao = gl.GenVertexArray();      // Generate a new VAO (get an ID number)
        gl.BindVertexArray(vao);        // Bind it (select it as active)
        
        Console.WriteLine($"✓ Created VAO with ID: {vao}");
        Console.WriteLine($"✓ Bound VAO {vao} (it's now active)\n");
        
        // ====================================================================
        // STEP 3: CREATE VBO AND UPLOAD DATA TO GPU
        // ====================================================================
        Console.WriteLine("STEP 3: Creating VBO and uploading vertex data to GPU");
        Console.WriteLine("────────────────────────────────────────────────────────\n");
        
        // WHAT IS A VBO?
        // "Vertex Buffer Object" = A chunk of memory ON THE GPU
        // We upload our vertex data (the 9 floats) to this GPU memory
        // This is MUCH faster than sending data every frame!
        
        // Create the VBO
        vbo = gl.GenBuffer();                              // Generate a new buffer (get an ID)
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);   // Bind it as an "array buffer"
        
        // WHAT IS BufferTargetARB.ArrayBuffer?
        // It tells OpenGL: "This buffer contains vertex attribute data"
        // (There are other buffer types for other purposes)
        
        Console.WriteLine($"✓ Created VBO with ID: {vbo}");
        Console.WriteLine($"✓ Bound VBO {vbo} as ArrayBuffer");
        
        // Now upload the vertex data to GPU
        // This is where we copy our C# array to GPU memory
        unsafe
        {
            // WHY UNSAFE?
            // We need to pass a memory pointer to OpenGL
            // C# normally protects memory, but here we need direct access
            
            fixed (float* buf = vertices)
            {
                // "fixed" prevents C# garbage collector from moving the array
                // "buf" is now a pointer to the first element
                
                gl.BufferData(
                    BufferTargetARB.ArrayBuffer,              // Which buffer to fill (our VBO)
                    (nuint)(vertices.Length * sizeof(float)), // Size in bytes
                    buf,                                       // Pointer to data
                    BufferUsageARB.StaticDraw                 // Usage hint
                );
                
                // WHAT IS StaticDraw?
                // It tells OpenGL: "This data won't change often"
                // OpenGL can optimize based on this hint
            }
        }
        
        Console.WriteLine($"✓ Uploaded {vertices.Length} floats ({vertices.Length * sizeof(float)} bytes) to GPU");
        Console.WriteLine("✓ Vertex data is now in GPU memory!\n");
        
        // ====================================================================
        // STEP 4: DESCRIBE HOW TO READ THE VBO DATA
        // ====================================================================
        Console.WriteLine("STEP 4: Configuring vertex attributes (how to read VBO)");
        Console.WriteLine("─────────────────────────────────────────────────────────\n");
        
        // Now we tell OpenGL HOW to interpret the data in the VBO
        // Our data is: [X, Y, Z, X, Y, Z, X, Y, Z]
        // We need to tell OpenGL: "Every 3 floats is one vertex position"
        
        unsafe
        {
            gl.VertexAttribPointer(
                0,                              // Attribute index (location = 0 in shader)
                3,                              // Size (3 numbers per vertex: X, Y, Z)
                VertexAttribPointerType.Float,  // Type of each number (float)
                false,                          // Normalized? (no, we want exact values)
                3 * sizeof(float),              // Stride (bytes between vertices)
                (void*)0                        // Offset (start at byte 0)
            );
        }
        
        // DETAILED EXPLANATION OF EACH PARAMETER:
        //
        // Parameter 1: "0" (Attribute Location)
        //   This matches "layout(location = 0)" in the vertex shader
        //   It's like saying "This is attribute slot #0"
        //
        // Parameter 2: "3" (Size - components per vertex)
        //   Each vertex has 3 numbers (X, Y, Z)
        //   If we were doing 2D only, this would be 2
        //
        // Parameter 3: "Float" (Data type)
        //   Each number is a float (32-bit decimal number)
        //
        // Parameter 4: "false" (Normalized)
        //   false = Use values as-is
        //   true = Convert values to 0.0-1.0 range
        //   We want our exact values, so false
        //
        // Parameter 5: "3 * sizeof(float)" (Stride)
        //   Stride = "How many bytes to skip to get to the next vertex?"
        //   Our data: [X₁, Y₁, Z₁, X₂, Y₂, Z₂, ...]
        //   To go from X₁ to X₂, we skip 3 floats = 12 bytes
        //
        // Parameter 6: "(void*)0" (Offset)
        //   Offset = "Where does this attribute start in the buffer?"
        //   0 = Starts at the beginning
        //   If position started at byte 12, we'd use (void*)12
        
        // Now enable this attribute
        gl.EnableVertexAttribArray(0);
        
        // WHAT DOES EnableVertexAttribArray DO?
        // It activates attribute location 0
        // Without this, the shader won't receive any data!
        
        Console.WriteLine("✓ Vertex Attribute Configuration:");
        Console.WriteLine("    Location: 0 (matches shader)");
        Console.WriteLine("    Components: 3 (X, Y, Z)");
        Console.WriteLine("    Type: Float");
        Console.WriteLine("    Stride: 12 bytes (3 floats × 4 bytes)");
        Console.WriteLine("    Offset: 0 (starts at beginning)");
        Console.WriteLine("✓ Attribute location 0 enabled\n");
        
        // ====================================================================
        // STEP 5: LOAD AND COMPILE SHADERS
        // ====================================================================
        Console.WriteLine("STEP 5: Loading and compiling shaders");
        Console.WriteLine("────────────────────────────────────────\n");
        
        // WHAT ARE SHADERS?
        // Shaders are small programs that run ON THE GPU
        // They're written in GLSL (OpenGL Shading Language)
        // We have two shader files:
        //   - shader.vert (Vertex Shader) - processes vertices
        //   - shader.frag (Fragment Shader) - colors pixels
        
        shaderProgram = CreateShaderProgram("Shaders/shader.vert", "Shaders/shader.frag");
        
        Console.WriteLine($"✓ Shader program created with ID: {shaderProgram}");
        Console.WriteLine("✓ Shaders are ready to use!\n");
        
        // ====================================================================
        // STEP 6: UNBIND (Clean up, optional but good practice)
        // ====================================================================
        gl.BindVertexArray(0);                     // Unbind VAO
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0); // Unbind VBO
        
        // WHY UNBIND?
        // It's like "deselecting" - prevents accidental modifications
        // Good practice but not strictly necessary
        
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("SETUP COMPLETE! Ready to render!");
        Console.WriteLine("═══════════════════════════════════════\n");
        Console.WriteLine("You should now see an ORANGE TRIANGLE!");
        Console.WriteLine("(If not, check console for errors)\n");
    }
    
    // ========================================================================
    // ONRENDER - Called EVERY FRAME (60+ times per second!)
    // ========================================================================
    // This is where we actually draw the triangle
    // ========================================================================
    private static void OnRender(double deltaTime)
    {
        // ====================================================================
        // CLEAR THE SCREEN (same as before)
        // ====================================================================
        gl!.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);  // Dark gray background
        gl.Clear(ClearBufferMask.ColorBufferBit);  // Clear previous frame
        
        // ====================================================================
        // DRAW THE TRIANGLE!
        // ====================================================================
        // This is surprisingly simple after all the setup!
        // Three commands: Use program, Bind VAO, Draw
        
        // STEP 1: Activate our shader program
        gl.UseProgram(shaderProgram);
        // This tells OpenGL: "Use these shaders for rendering"
        
        // STEP 2: Bind our VAO
        gl.BindVertexArray(vao);
        // This tells OpenGL: "Use this vertex configuration"
        // The VAO "remembers" our VBO and how to read it
        
        // STEP 3: DRAW!!!
        gl.DrawArrays(
            PrimitiveType.Triangles,   // What shape to draw (triangles)
            0,                         // Start at vertex index 0
            3                          // Draw 3 vertices (= 1 triangle)
        );
        
        // WHAT JUST HAPPENED?
        // 1. GPU reads 3 vertices from VBO (9 floats)
        // 2. Vertex shader runs 3 times (once per vertex)
        // 3. OpenGL connects the 3 points into a triangle shape
        // 4. OpenGL figures out which pixels are inside the triangle
        // 5. Fragment shader runs for each pixel (colors them orange)
        // 6. Triangle appears on screen!
        //
        // All of this happens on the GPU in MICROSECONDS!
        
        // ====================================================================
        // WHAT IF WE WANTED TO DRAW MORE TRIANGLES?
        // ====================================================================
        // We could call DrawArrays again with different vertices
        // Or we could have more data in the VBO
        // Example: 6 vertices = 2 triangles, 9 vertices = 3 triangles, etc.
    }
    
    // ========================================================================
    // ONCLOSING - Called when window closes
    // ========================================================================
    private static void OnClosing()
    {
        Console.WriteLine("\n═══════════════════════");
        Console.WriteLine("CLEANING UP GPU OBJECTS");
        Console.WriteLine("═══════════════════════\n");
        
        // Delete all GPU objects we created
        // This frees the GPU memory
        
        gl!.DeleteBuffer(vbo);
        Console.WriteLine($"✓ Deleted VBO {vbo}");
        
        gl.DeleteVertexArray(vao);
        Console.WriteLine($"✓ Deleted VAO {vao}");
        
        gl.DeleteProgram(shaderProgram);
        Console.WriteLine($"✓ Deleted shader program {shaderProgram}");
        
        gl?.Dispose();
        Console.WriteLine("✓ Disposed OpenGL context\n");
        Console.WriteLine("Goodbye!");
    }
    
    // ========================================================================
    // HELPER FUNCTIONS FOR SHADER LOADING
    // ========================================================================
    // These functions load and compile the shader files
    // You don't need to fully understand these yet, but here's what they do:
    // ========================================================================
    
    /// <summary>
    /// Loads vertex and fragment shaders, compiles them, and links them into a program
    /// </summary>
    private static uint CreateShaderProgram(string vertexPath, string fragmentPath)
    {
        Console.WriteLine($"  Loading vertex shader: {vertexPath}");
        string vertexCode = File.ReadAllText(vertexPath);
        
        Console.WriteLine($"  Loading fragment shader: {fragmentPath}");
        string fragmentCode = File.ReadAllText(fragmentPath);
        
        Console.WriteLine("  Compiling vertex shader...");
        uint vertexShader = CompileShader(ShaderType.VertexShader, vertexCode);
        
        Console.WriteLine("  Compiling fragment shader...");
        uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentCode);
        
        Console.WriteLine("  Linking shader program...");
        uint program = gl!.CreateProgram();
        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);
        
        // Check if linking was successful
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string infoLog = gl.GetProgramInfoLog(program);
            Console.WriteLine($"  ❌ ERROR: Shader linking failed!");
            Console.WriteLine($"  {infoLog}");
            throw new Exception("Shader program linking failed!");
        }
        
        Console.WriteLine("  ✓ Shaders linked successfully!");
        
        // Clean up individual shaders (we don't need them anymore)
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        
        return program;
    }
    
    /// <summary>
    /// Compiles a single shader (vertex or fragment)
    /// </summary>
    private static uint CompileShader(ShaderType type, string source)
    {
        uint shader = gl!.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        
        // Check if compilation was successful
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
        if (compileStatus == 0)
        {
            string infoLog = gl.GetShaderInfoLog(shader);
            string shaderTypeName = type == ShaderType.VertexShader ? "Vertex" : "Fragment";
            Console.WriteLine($"  ❌ ERROR: {shaderTypeName} shader compilation failed!");
            Console.WriteLine($"  {infoLog}");
            throw new Exception($"{shaderTypeName} shader compilation failed!");
        }
        
        return shader;
    }
}

// ============================================================================
// 🎉 CONGRATULATIONS!  🎉
// ============================================================================
// If you see an orange triangle, you've done it!
// 
// SUMMARY OF WHAT YOU LEARNED:
//
// 1. **VBO (Vertex Buffer Object)**
//    - Stores vertex data on the GPU
//    - Much faster than sending data every frame
//    - Created with GenBuffer, filled with BufferData
//
// 2. **VAO (Vertex Array Object)**
//    - Describes the format of vertex data
//    - "Remembers" VBO configuration
//    - Can switch between different vertex formats quickly
//
// 3. **Shaders**
//    - Small programs that run on the GPU
//    - Vertex Shader: Processes each vertex (corner)
//    - Fragment Shader: Colors each pixel
//    - Written in GLSL (similar to C)
//
// 4. **The Graphics Pipeline**
//    CPU → VBO (GPU Memory) → Vertex Shader → 
//    → Connect Vertices → Rasterize → Fragment Shader → Screen
//
// 5. **The Rendering Loop**
//    Every frame:
//      - Use shader program (UseProgram)
//      - Bind VAO (BindVertexArray)
//      - Draw (DrawArrays)
//
// EXPERIMENTS TO TRY:
//
// 1. Change the vertex positions in OnLoad:
//    - Make the triangle bigger (use values like 0.8 instead of 0.5)
//    - Make it smaller (use values like 0.2)
//    - Move it around (change the Y values)
//
// 2. Change the color in shader.frag:
//    - Try vec4(1.0, 0.0, 0.0, 1.0) for RED
//    - Try vec4(0.0, 1.0, 0.0, 1.0) for GREEN
//    - Try vec4(1.0, 1.0, 0.0, 1.0) for YELLOW
//
// 3. Add a 4th vertex (won't work, why?)
//    - DrawArrays draws 3 vertices at a time for triangles
//    - To draw 2 triangles, you need 6 vertices!
//
// 4. Change the background color in OnRender
//    - Try gl.ClearColor(0.2f, 0.3f, 0.4f, 1.0f) for blue-ish
//
// NEXT PROJECT: 2.2 - Multi-Color Triangle
// We'll learn how to give each vertex a different color!
// This creates a cool gradient effect!
//
// ============================================================================
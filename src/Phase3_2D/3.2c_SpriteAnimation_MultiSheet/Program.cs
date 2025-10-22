using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using Silk.NET.Input;
using System;
using System.IO;
using StbImageSharp;
using System.Numerics;

// ============================================================================
// PROJECT 3.2c: MULTI-SHEET SPRITE ANIMATION WITH KEYBOARD CONTROLS
// ============================================================================
// OVERVIEW (READ THIS FIRST!)
//
// GOAL:
//   - Switch between multiple sprite sheets dynamically using keyboard input
//   - Control animation speed with arrow keys (up/down)
//   - Learn keyboard input handling in Silk.NET
//   - Demonstrate dynamic resource loading and switching
//
// WHAT'S NEW VS PROJECT 3.2/3.2b:
//   - Multiple sprite sheets: 1x8 layout AND 2x4 layout
//   - Keyboard input: Space to switch sheets, Arrow Up/Down for speed
//   - Dynamic texture loading: switch textures at runtime
//   - Input system: handle key press events
//
// KEYBOARD CONTROLS:
//   - SPACE: Switch between sprite sheets (1x8 ↔ 2x4)
//   - ARROW UP: Increase animation speed
//   - ARROW DOWN: Decrease animation speed
//   - ESC: Close window
//
// SPRITE SHEETS:
//   1) sprite_sheet_1x8.png (from 3.2):
//      - 8 frames in 1 row
//      - frameWidth = 0.125 (1/8)
//      - frameHeight = 1.0
//   
//   2) sprite_sheet_2x4.png (from 3.2b):
//      - 8 frames in 2 rows × 4 columns
//      - frameWidth = 0.25 (1/4)
//      - frameHeight = 0.5 (1/2)
//
// BIG-PICTURE FLOW:
//   CPU (C#):
//     1) Initialize both sprite sheets (load textures)
//     2) Set up keyboard input handling
//     3) On SPACE key: switch active sprite sheet and update parameters
//     4) On ARROW keys: adjust animation speed (FPS)
//     5) Calculate current frame and UV offset
//     6) Bind active texture and draw animated sprite
//
// INPUT HANDLING FLOW:
//   1) Register keyboard callback when window loads
//   2) When key pressed: check which key it is
//   3) If SPACE: toggle currentSheetIndex, reload texture
//   4) If ARROW UP/DOWN: increase/decrease animationFPS
//   5) Update happens automatically next frame
//
// FILES CREATED:
//   - textures/sprite_sheet_1x8.png  (1 row × 8 columns)
//   - textures/sprite_sheet_2x4.png  (2 rows × 4 columns)
//   - Shaders/shader.vert            (vertex shader with UV offset)
//   - Shaders/shader.frag            (fragment shader)
//   - Program.cs                     (this file - multi-sheet logic)
//   - README.md                      (project guide)
//
// ============================================================================

namespace SpriteAnimationMultiSheet;

class Program
{
    // ========================================================================
    // WINDOW + OPENGL CONTEXT
    // ========================================================================
    private static IWindow? window;  // Silk.NET window
    private static GL? gl;           // OpenGL context
    private static IInputContext? inputContext;  // Input system

    // ========================================================================
    // GPU OBJECT HANDLES
    // ========================================================================
    private static uint vao;             // Vertex Array Object
    private static uint vbo;             // Vertex Buffer Object
    private static uint ebo;             // Element Buffer Object
    private static uint shaderProgram;   // Shader program
    
    // ========================================================================
    // SPRITE SHEET SYSTEM
    // ========================================================================
    // We support multiple sprite sheets with different layouts
    private static int currentSheetIndex = 0;  // Which sprite sheet is active (0 or 1)
    private static uint[] textureIds = new uint[2];  // Texture IDs for both sprite sheets
    private static string[] textureFiles = new string[]
    {
        "sprite_sheet_1x8.png",  // Sheet 0: 1 row × 8 columns
        "sprite_sheet_2x4.png"   // Sheet 1: 2 rows × 4 columns
    };
    
    // ========================================================================
    // SPRITE SHEET PARAMETERS
    // ========================================================================
    // Each sprite sheet has its own layout parameters
    // Sheet 0 (1x8): framesPerRow=8, numberOfRows=1, totalFrames=8
    // Sheet 1 (2x4): framesPerRow=4, numberOfRows=2, totalFrames=8
    private static int[] framesPerRowArray = new int[] { 8, 4 };
    private static int[] numberOfRowsArray = new int[] { 1, 2 };
    private static int[] totalFramesArray = new int[] { 8, 8 };
    
    // Current active parameters (updated when switching sheets)
    private static int framesPerRow = 8;     // Current frames per row
    private static int numberOfRows = 1;     // Current number of rows
    private static int totalFrames = 8;      // Current total frames
    
    // ========================================================================
    // ANIMATION PARAMETERS
    // ========================================================================
    private static float animationFPS = 12f;  // Animation speed (controlled by arrow keys)
    private static float minFPS = 1f;         // Minimum FPS
    private static float maxFPS = 30f;        // Maximum FPS
    private static float fpsStep = 1f;        // FPS change per key press
    private static bool isPaused = false;     // Pause state (controlled by SPACE)
    
    // ========================================================================
    // SPACING AND PADDING PARAMETERS
    // ========================================================================
    private static float horizontalSpacing = 0.0f;  // Horizontal spacing between frames
    private static float verticalSpacing = 0.0f;    // Vertical spacing between frames
    private static float horizontalPadding = 0.0f;  // Horizontal padding around entire image
    private static float verticalPadding = 0.0f;    // Vertical padding around entire image
    
    // Calculated values (updated each frame)
    private static float frameWidth;        // Width of one frame in UV space
    private static float frameHeight;       // Height of one frame in UV space
    private static int currentFrame;        // Current frame index (0 to totalFrames-1)
    private static float frameTime;         // Time accumulator for frame timing

    // ========================================================================
    // GEOMETRY: A QUAD FOR THE SPRITE
    // ========================================================================
    // Base UVs represent ONE frame (frame 0) - not the entire sprite sheet!
    // We'll update these when switching sprite sheets
    private static float[] quadVertices = new float[]
    {
        //   x      y     z     u     v
        -0.3f,  0.5f, 0.0f,   0.0f, 0.0f,       // top-left
        -0.3f, -0.5f, 0.0f,   0.0f, 1.0f,       // bottom-left
         0.3f, -0.5f, 0.0f,   0.125f, 1.0f,     // bottom-right
         0.3f,  0.5f, 0.0f,   0.125f, 0.0f      // top-right
    };

    // ========================================================================
    // INDICES FOR QUAD (2 TRIANGLES)
    // ========================================================================
    private static uint[] quadIndices = new uint[]
    {
        0, 1, 2,  // First triangle (top-left, bottom-left, bottom-right)
        2, 3, 0   // Second triangle (bottom-right, top-right, top-left)
    };

    // ========================================================================
    // MAIN ENTRY POINT
    // ========================================================================
    static void Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("PROJECT 3.2c: MULTI-SHEET ANIMATION!");
        Console.WriteLine("====================================");
        Console.WriteLine();
        Console.WriteLine("CONTROLS:");
        Console.WriteLine("  1, 2: Choose sprite sheet (1x8 or 2x4)");
        Console.WriteLine("  SPACE: Pause/Unpause animation");
        Console.WriteLine("  LEFT/RIGHT: Manual frame control");
        Console.WriteLine("  ARROW UP: Increase animation speed");
        Console.WriteLine("  ARROW DOWN: Decrease animation speed");
        Console.WriteLine("  ESC: Exit");
        Console.WriteLine();

        // Create window
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "3.2c: Multi-Sheet Sprite Animation";
        window = Window.Create(options);

        // Register callbacks
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Closing += OnClose;

        // Run window loop
        window.Run();
    }

    // ========================================================================
    // ONLOAD - INITIALIZE OPENGL, LOAD BOTH SPRITE SHEETS, SET UP INPUT
    // ========================================================================
    private static unsafe void OnLoad()
    {
        // Get OpenGL context
        gl = window!.CreateOpenGL();
        
        Console.WriteLine($"OpenGL Version: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine();

        // ---------------------------------------------------------
        // 1) SET UP KEYBOARD INPUT
        // ---------------------------------------------------------
        inputContext = window.CreateInput();
        foreach (var keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
        }
        Console.WriteLine("Keyboard input registered.");

        // ---------------------------------------------------------
        // 2) CREATE VERTEX ARRAY OBJECT (VAO)
        // ---------------------------------------------------------
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        // ---------------------------------------------------------
        // 3) CREATE AND UPLOAD VERTEX BUFFER OBJECT (VBO)
        // ---------------------------------------------------------
        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        
        fixed (float* buf = quadVertices)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(quadVertices.Length * sizeof(float)),
                buf,
                BufferUsageARB.StaticDraw);
        }

        // ---------------------------------------------------------
        // 4) CREATE AND UPLOAD ELEMENT BUFFER OBJECT (EBO)
        // ---------------------------------------------------------
        ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        
        fixed (uint* buf = quadIndices)
        {
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(quadIndices.Length * sizeof(uint)),
                buf,
                BufferUsageARB.StaticDraw);
        }

        // ---------------------------------------------------------
        // 5) CONFIGURE VERTEX ATTRIBUTES
        // ---------------------------------------------------------
        // Position attribute (location 0): 3 floats (x, y, z)
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
            5 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);

        // Texture coordinate attribute (location 1): 2 floats (u, v)
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
            5 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        // ---------------------------------------------------------
        // 6) LOAD AND COMPILE SHADERS
        // ---------------------------------------------------------
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string vertPath = Path.Combine(exeDir, "Shaders", "shader.vert");
        string fragPath = Path.Combine(exeDir, "Shaders", "shader.frag");
        shaderProgram = CreateShaderProgram(vertPath, fragPath);
        gl.UseProgram(shaderProgram);

        // ---------------------------------------------------------
        // 7) LOAD BOTH SPRITE SHEET TEXTURES
        // ---------------------------------------------------------
        Console.WriteLine("Loading sprite sheets...");
        for (int i = 0; i < textureFiles.Length; i++)
        {
            string texturePath = Path.Combine(exeDir, "textures", textureFiles[i]);
            textureIds[i] = LoadTexture(texturePath);
            Console.WriteLine($"  Loaded: {textureFiles[i]} (ID: {textureIds[i]})");
        }

        // ---------------------------------------------------------
        // 8) SET UP ORTHOGRAPHIC PROJECTION MATRIX
        // ---------------------------------------------------------
        // Create 2D orthographic projection for pixel-perfect rendering
        // This maps screen coordinates (-1 to +1) to our desired viewport
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(
            -1.0f, 1.0f,    // left, right
            -1.0f, 1.0f,    // bottom, top  
            -1.0f, 1.0f     // near, far
        );

        // Send projection matrix to shader
        int projectionLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        if (projectionLoc != -1)
        {
            unsafe
            {
                float* matrixPtr = stackalloc float[16];
                matrixPtr[0] = projection.M11; matrixPtr[1] = projection.M12; matrixPtr[2] = projection.M13; matrixPtr[3] = projection.M14;
                matrixPtr[4] = projection.M21; matrixPtr[5] = projection.M22; matrixPtr[6] = projection.M23; matrixPtr[7] = projection.M24;
                matrixPtr[8] = projection.M31; matrixPtr[9] = projection.M32; matrixPtr[10] = projection.M33; matrixPtr[11] = projection.M34;
                matrixPtr[12] = projection.M41; matrixPtr[13] = projection.M42; matrixPtr[14] = projection.M43; matrixPtr[15] = projection.M44;
                gl.UniformMatrix4(projectionLoc, 1, false, matrixPtr);
            }
        }

        // ---------------------------------------------------------
        // 9) INITIALIZE ANIMATION PARAMETERS FOR FIRST SPRITE SHEET
        // ---------------------------------------------------------
        SwitchSpriteSheet(0);  // Start with sheet 0 (1x8)

        Console.WriteLine();
        Console.WriteLine("Setup complete. Starting animation.");
        Console.WriteLine();
    }

    // ========================================================================
    // KEYBOARD INPUT HANDLER
    // ========================================================================
    private static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        // Number keys: Choose sprite sheet directly
        if (key == Key.Number1)
        {
            SwitchSpriteSheet(0);  // Switch to 1x8 sprite sheet
        }
        else if (key == Key.Number2)
        {
            SwitchSpriteSheet(1);  // Switch to 2x4 sprite sheet
        }
        
        // SPACE: Pause/Unpause animation
        else if (key == Key.Space)
        {
            isPaused = !isPaused;
            Console.WriteLine($"Animation {(isPaused ? "PAUSED" : "RESUMED")}");
        }
        
        // LEFT/RIGHT: Manual frame control (only when paused)
        else if (key == Key.Left && isPaused)
        {
            currentFrame = (currentFrame - 1 + totalFrames) % totalFrames;
            Console.WriteLine($"Manual frame: {currentFrame}");
        }
        else if (key == Key.Right && isPaused)
        {
            currentFrame = (currentFrame + 1) % totalFrames;
            Console.WriteLine($"Manual frame: {currentFrame}");
        }
        
        // ARROW UP: Increase FPS
        else if (key == Key.Up)
        {
            animationFPS = Math.Min(animationFPS + fpsStep, maxFPS);
            Console.WriteLine($"Animation FPS: {animationFPS:F1} (Speed UP)");
        }
        
        // ARROW DOWN: Decrease FPS
        else if (key == Key.Down)
        {
            animationFPS = Math.Max(animationFPS - fpsStep, minFPS);
            Console.WriteLine($"Animation FPS: {animationFPS:F1} (Speed DOWN)");
        }
        
        // ESC: Close window
        else if (key == Key.Escape)
        {
            window?.Close();
        }
    }

    // ========================================================================
    // SWITCH SPRITE SHEET - UPDATE ALL PARAMETERS
    // ========================================================================
    private static unsafe void SwitchSpriteSheet(int index)
    {
        currentSheetIndex = index;
        
        // Update layout parameters
        framesPerRow = framesPerRowArray[index];
        numberOfRows = numberOfRowsArray[index];
        totalFrames = totalFramesArray[index];
        
        // Calculate frame dimensions
        frameWidth = 1.0f / framesPerRow;
        frameHeight = 1.0f / numberOfRows;
        
        // Reset animation state
        currentFrame = 0;
        frameTime = 0f;
        
        // Update quad UVs for new sprite sheet
        InitializeQuadVertices();
        
        // Re-upload VBO data with new UVs
        gl!.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (float* buf = quadVertices)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(quadVertices.Length * sizeof(float)),
                buf,
                BufferUsageARB.StaticDraw);
        }
        
        // Print info
        Console.WriteLine($"══════════════════════════════════════");
        Console.WriteLine($"Switched to: {textureFiles[index]}");
        Console.WriteLine($"  Layout: {framesPerRow} columns × {numberOfRows} rows = {totalFrames} frames");
        Console.WriteLine($"  Frame size: {frameWidth:F4} × {frameHeight:F4}");
        Console.WriteLine($"  Animation FPS: {animationFPS:F1}");
        Console.WriteLine($"  Status: {(isPaused ? "PAUSED" : "RUNNING")}");
        Console.WriteLine($"══════════════════════════════════════");
    }

    // ========================================================================
    // INITIALIZE QUAD VERTICES - CALCULATE UV COORDINATES AUTOMATICALLY
    // ========================================================================
    private static void InitializeQuadVertices()
    {
        // Calculate UV coordinates with padding and spacing
        float uStart = horizontalPadding;
        float uEnd = frameWidth + horizontalPadding;
        float vStart = verticalPadding;
        float vEnd = frameHeight + verticalPadding;
        
        // Update UV coordinates in quadVertices array
        // Each vertex has 5 components: x, y, z, u, v
        // UV indices: vertex 0 = [3,4], vertex 1 = [8,9], vertex 2 = [13,14], vertex 3 = [18,19]
        
        // FLIP V COORDINATES to fix sprite orientation (0.0 ↔ 1.0)
        quadVertices[3] = uStart;   // Top-left U
        quadVertices[4] = vEnd;     // Top-left V (FLIPPED: was vStart)
        
        quadVertices[8] = uStart;   // Bottom-left U
        quadVertices[9] = vStart;   // Bottom-left V (FLIPPED: was vEnd)
        
        quadVertices[13] = uEnd;    // Bottom-right U
        quadVertices[14] = vStart;  // Bottom-right V (FLIPPED: was vEnd)
        
        quadVertices[18] = uEnd;    // Top-right U
        quadVertices[19] = vEnd;    // Top-right V (FLIPPED: was vStart)
    }

    // ========================================================================
    // ONRENDER - CALLED EVERY FRAME
    // ========================================================================
    private static unsafe void OnRender(double delta)
    {
        // Clear screen
        gl!.Clear(ClearBufferMask.ColorBufferBit);

        // Update animation
        UpdateAnimation(delta);

        // Bind active sprite sheet texture
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, textureIds[currentSheetIndex]);

        // Draw the quad
        gl.BindVertexArray(vao);
        gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }

    // ========================================================================
    // UPDATE ANIMATION - CALCULATE CURRENT FRAME AND UV OFFSET
    // ========================================================================
    private static void UpdateAnimation(double delta)
    {
        // Only update animation if not paused
        if (!isPaused)
        {
            // Accumulate time
            frameTime += (float)delta;

            // Calculate frame duration
            float frameDuration = 1.0f / animationFPS;

            // Calculate current frame
            currentFrame = (int)(frameTime / frameDuration) % totalFrames;
        }

        // Calculate UV offset with spacing (always update UVs, even when paused)
        int row = currentFrame / framesPerRow;
        int col = currentFrame % framesPerRow;
        
        float uOffset = col * (frameWidth + horizontalSpacing);
        float vOffset = row * (frameHeight + verticalSpacing);

        // Send UV offset to shader
        int uvOffsetLoc = gl!.GetUniformLocation(shaderProgram, "uUVOffset");
        if (uvOffsetLoc != -1)
        {
            gl.Uniform2(uvOffsetLoc, uOffset, vOffset);
        }
    }

    // ========================================================================
    // LOAD TEXTURE FROM FILE
    // ========================================================================
    private static uint LoadTexture(string path)
    {
        // Load image using StbImageSharp
        StbImage.stbi_set_flip_vertically_on_load(1);
        
        using (var stream = File.OpenRead(path))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            // Generate OpenGL texture
            uint textureId = gl!.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, textureId);

            // Upload image data to GPU
            unsafe
            {
                fixed (byte* ptr = image.Data)
                {
                    gl.TexImage2D(
                        TextureTarget.Texture2D,
                        0,
                        InternalFormat.Rgba,
                        (uint)image.Width,
                        (uint)image.Height,
                        0,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        ptr
                    );
                }
            }

            // Set texture parameters
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            return textureId;
        }
    }

    // ========================================================================
    // CREATE SHADER PROGRAM FROM FILES
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
            string infoLog = gl.GetProgramInfoLog(program);
            throw new Exception($"Program link failed: {infoLog}");
        }

        gl.DetachShader(program, vertexShader);
        gl.DetachShader(program, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        return program;
    }

    // ========================================================================
    // COMPILE SHADER
    // ========================================================================
    private static uint CompileShader(ShaderType type, string source)
    {
        uint shader = gl!.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            string infoLog = gl.GetShaderInfoLog(shader);
            throw new Exception($"{type} compile failed: {infoLog}");
        }

        return shader;
    }

    // ========================================================================
    // ONCLOSE - CLEANUP
    // ========================================================================
    private static void OnClose()
    {
        Console.WriteLine("Cleaning up...");

        // Delete textures
        foreach (uint textureId in textureIds)
        {
            gl!.DeleteTexture(textureId);
        }

        // Delete buffers
        gl!.DeleteBuffer(vbo);
        gl.DeleteBuffer(ebo);
        gl.DeleteVertexArray(vao);
        gl.DeleteProgram(shaderProgram);
        
        // Dispose input
        inputContext?.Dispose();
    }
}


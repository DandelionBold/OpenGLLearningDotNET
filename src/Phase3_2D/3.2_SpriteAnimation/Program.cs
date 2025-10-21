using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;
using StbImageSharp;
using System.Numerics;

// ============================================================================
// PROJECT 3.2: SPRITE ANIMATION - ANIMATE SPRITES USING SPRITE SHEETS
// ============================================================================
// OVERVIEW (READ THIS FIRST!)
//
// GOAL:
//   - Display animated sprites using sprite sheets (texture atlases)
//   - Animate through frames by changing UV coordinates over time
//   - Learn sprite sheet concepts and UV offset animation
//
// WHAT'S NEW VS PROJECT 3.1:
//   - Sprite sheets: one texture contains multiple animation frames
//   - UV offset animation: move UV coordinates to show different frames
//   - Frame timing: control animation speed using delta time
//   - Animation loops: cycle through frames continuously
//
// SPRITE SHEET CONCEPT:
//   A sprite sheet is a texture containing multiple frames arranged in a grid:
//   
//   ┌─────────────────────────────────┐
//   │ Frame 0 │ Frame 1 │ Frame 2 │   │  ← Row 0
//   ├─────────┼─────────┼─────────┤   │
//   │ Frame 3 │ Frame 4 │ Frame 5 │   │  ← Row 1  
//   └─────────┴─────────┴─────────┘   │
//   
//   Each frame has UV coordinates:
//   Frame 0: (0.0, 0.0) to (0.33, 0.5)
//   Frame 1: (0.33, 0.0) to (0.66, 0.5)
//   Frame 2: (0.66, 0.0) to (1.0, 0.5)
//   etc.
//
// ANIMATION TECHNIQUE:
//   1) Calculate current frame based on time
//   2) Convert frame number to UV offset
//   3) Send UV offset to vertex shader as uniform
//   4) Vertex shader adds offset to base UVs
//
// BIG-PICTURE FLOW:
//   CPU (C#):
//     1) Load sprite sheet texture
//     2) Define animation parameters (frames per row, total frames, FPS)
//     3) Calculate current frame from elapsed time
//     4) Convert frame to UV offset
//     5) Send UV offset to shader as uniform
//     6) Draw quad with animated UVs
//
// SHADER DATA FLOW:
//   Vertex:
//     - Input: aPosition (vec3), aTexCoord (vec2) - base UVs for frame 0
//     - Uniform: uUVOffset (vec2) - offset to current frame
//     - Output: vTexCoord = aTexCoord + uUVOffset
//   Fragment:
//     - Input: vTexCoord (animated UVs)
//     - Uniform: uTexture0 (sprite sheet)
//     - Output: FragColor = texture(uTexture0, vTexCoord)
//
// FILES CREATED:
//   - Shaders/shader.vert    (vertex shader with UV offset)
//   - Shaders/shader.frag    (fragment shader)
//   - Program.cs             (this file - animation logic)
//   - README.md              (project guide)
//
// IMPORTANT: Place a sprite sheet at: src/Phase3_2D/3.2_SpriteAnimation/textures/sprite_sheet.png
// ============================================================================

namespace SpriteAnimation;

class Program
{
    // ========================================================================
    // WINDOW + OPENGL CONTEXT
    // ========================================================================
    private static IWindow? window;  // Silk.NET window
    private static GL? gl;           // OpenGL context

    // ========================================================================
    // GPU OBJECT HANDLES
    // ========================================================================
    private static uint vao;             // Vertex Array Object
    private static uint vbo;             // Vertex Buffer Object
    private static uint ebo;             // Element Buffer Object
    private static uint shaderProgram;   // Shader program
    private static uint textureId;       // Sprite sheet texture

    // ========================================================================
    // ANIMATION PARAMETERS
    // ========================================================================
    // These control how the sprite sheet is interpreted and animated
    private static int framesPerRow = 8;     // How many frames per row in sprite sheet (8 frames in 1 row)
    private static int totalFrames = 8;      // Total number of animation frames
    private static float animationFPS = 12f; // Animation speed (frames per second) - faster for running
    
    // Calculated values (updated each frame)
    private static float frameWidth;        // Width of one frame in UV space (1.0 / framesPerRow)
    private static float frameHeight;       // Height of one frame in UV space
    private static int currentFrame;        // Current frame index (0 to totalFrames-1)
    private static float frameTime;         // Time accumulator for frame timing

    // ========================================================================
    // GEOMETRY: A QUAD FOR THE SPRITE
    // ========================================================================
    // Same as 3.1, but UVs represent the FIRST frame (frame 0)
    // The vertex shader will offset these UVs to show other frames
    private static readonly float[] quadVertices = new float[]
    {
        //   x      y     z     u     v
        -0.5f,  0.5f, 0.0f,   0.0f, 1.0f,   // top-left
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f,   // bottom-left
         0.5f, -0.5f, 0.0f,   1.0f, 0.0f,   // bottom-right
         0.5f,  0.5f, 0.0f,   1.0f, 1.0f    // top-right
    };

    // Indices for two triangles
    private static readonly uint[] quadIndices = new uint[]
    {
        0, 1, 2,
        0, 2, 3
    };

    // ========================================================================
    // PROGRAM ENTRY
    // ========================================================================
    static void Main(string[] args)
    {
        Console.WriteLine("====================================");
        Console.WriteLine("PROJECT 3.2: SPRITE ANIMATION!");
        Console.WriteLine("====================================\n");

        // WINDOW CREATION:
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "3.2 - Sprite Animation";

        window = Window.Create(options);
        window.Load += OnLoad;     // called once when GL context is ready
        window.Render += OnRender; // called every frame
        window.Closing += OnClosing; // called on shutdown
        window.Run();              // enter main loop
    }

    // ========================================================================
    // ONLOAD - RUNS ONCE
    // ========================================================================
    private static void OnLoad()
    {
        // Create an OpenGL interface from the window context
        gl = window!.CreateOpenGL();
        Console.WriteLine($"OpenGL Version: {gl.GetStringS(StringName.Version)}\n");

        // ------------------------
        // 1) CREATE AND BIND A VAO
        // ------------------------
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        // -------------------------------
        // 2) CREATE VBO & UPLOAD VERTICES
        // -------------------------------
        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe
        {
            fixed (float* buf = quadVertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(quadVertices.Length * sizeof(float)),
                    buf,
                    BufferUsageARB.StaticDraw);
            }
        }

        // ---------------------------------
        // 3) CREATE EBO & UPLOAD INDICES
        // ---------------------------------
        ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        unsafe
        {
            fixed (uint* ibuf = quadIndices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(quadIndices.Length * sizeof(uint)),
                    ibuf,
                    BufferUsageARB.StaticDraw);
            }
        }

        // --------------------------------------------------------------------------
        // 4) CONFIGURE VERTEX ATTRIBUTES
        // --------------------------------------------------------------------------
        int stride = 5 * sizeof(float);
        unsafe
        {
            // Attribute 0 = position (x,y,z)
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
            gl.EnableVertexAttribArray(0);

            // Attribute 1 = UV (u,v) - base UVs for frame 0
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(1);
        }

        // -----------------------------
        // 5) COMPILE/LINK GLSL SHADERS
        // -----------------------------
        // Get the directory where the executable is located
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string vertPath = Path.Combine(exeDir, "Shaders", "shader.vert");
        string fragPath = Path.Combine(exeDir, "Shaders", "shader.frag");
        shaderProgram = CreateShaderProgram(vertPath, fragPath);

        // ------------------------------------------------------------
        // 6) LOAD SPRITE SHEET TEXTURE
        // ------------------------------------------------------------
        string texturePath = Path.Combine(exeDir, "textures", "sprite_sheet.png");
        textureId = LoadTexture(texturePath);
        if (textureId == 0)
        {
            Console.WriteLine("WARNING: Sprite sheet not found. Place a PNG at textures/sprite_sheet.png");
            Console.WriteLine("For testing, you can copy sample.png from 3.1 and rename it.");
        }

        // Tell the shader which texture unit the sampler should read from
        gl.UseProgram(shaderProgram);
        int samplerLoc = gl.GetUniformLocation(shaderProgram, "uTexture0");
        if (samplerLoc != -1)
        {
            gl.Uniform1(samplerLoc, 0); // 0 = GL_TEXTURE0
        }

        // ---------------------------------------------------------
        // 7) CREATE ORTHOGRAPHIC PROJECTION FOR 2D
        // ---------------------------------------------------------
        float aspect = window.Size.X / (float)window.Size.Y;
        Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(-aspect, aspect, -1f, 1f, -1f, 1f);
        int projLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        if (projLoc != -1)
        {
            float[] mat = new float[]
            {
                ortho.M11, ortho.M12, ortho.M13, ortho.M14,
                ortho.M21, ortho.M22, ortho.M23, ortho.M24,
                ortho.M31, ortho.M32, ortho.M33, ortho.M34,
                ortho.M41, ortho.M42, ortho.M43, ortho.M44
            };
            unsafe
            {
                fixed (float* p = mat)
                {
                    gl.UniformMatrix4(projLoc, 1, false, p);
                }
            }
        }

        // ---------------------------------------------------------
        // 8) INITIALIZE ANIMATION PARAMETERS
        // ---------------------------------------------------------
        // Calculate UV dimensions for one frame
        frameWidth = 1.0f / framesPerRow;  // If 4 frames per row, each frame is 0.25 wide
        frameHeight = 1.0f;                 // Assume single row for now (can be extended)
        
        currentFrame = 0;
        frameTime = 0f;

        Console.WriteLine($"Animation Setup:");
        Console.WriteLine($"  Frames per row: {framesPerRow}");
        Console.WriteLine($"  Total frames: {totalFrames}");
        Console.WriteLine($"  Animation FPS: {animationFPS}");
        Console.WriteLine($"  Frame UV size: {frameWidth:F2} x {frameHeight:F2}");
        Console.WriteLine($"Setup complete. Starting sprite animation.\n");
    }

    // ========================================================================
    // ONRENDER - CALLED EVERY FRAME
    // ========================================================================
    private static void OnRender(double delta)
    {
        // Clear old frame
        gl!.ClearColor(0.12f, 0.12f, 0.15f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        // ---------------------------------------------------------
        // UPDATE ANIMATION
        // ---------------------------------------------------------
        UpdateAnimation(delta);

        // ---------------------------------------------------------
        // RENDER SPRITE
        // ---------------------------------------------------------
        // Use our shader program
        gl.UseProgram(shaderProgram);

        // Bind texture unit 0 and the sprite sheet texture
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, textureId);

        // Bind VAO (brings VBO/EBO & attribute format into effect)
        gl.BindVertexArray(vao);

        // Draw 2 triangles = 6 indices
        unsafe
        {
            gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
        }
    }

    // ========================================================================
    // UPDATE ANIMATION - CALCULATE CURRENT FRAME AND UV OFFSET
    // ========================================================================
    private static void UpdateAnimation(double delta)
    {
        // Accumulate time
        frameTime += (float)delta;

        // Calculate how long each frame should be displayed
        float frameDuration = 1.0f / animationFPS; // e.g., 0.1 seconds for 10 FPS

        // Calculate current frame based on elapsed time
        currentFrame = (int)(frameTime / frameDuration) % totalFrames;

        // Calculate UV offset for current frame
        // Frame 0: offset (0, 0)
        // Frame 1: offset (frameWidth, 0)
        // Frame 2: offset (2 * frameWidth, 0)
        // etc.
        float uOffset = (currentFrame % framesPerRow) * frameWidth;
        float vOffset = (currentFrame / framesPerRow) * frameHeight;

        // Send UV offset to vertex shader
        int uvOffsetLoc = gl!.GetUniformLocation(shaderProgram, "uUVOffset");
        if (uvOffsetLoc != -1)
        {
            gl.Uniform2(uvOffsetLoc, uOffset, vOffset);
        }

        // Optional: Print frame info every second (for debugging)
        if ((int)frameTime != (int)(frameTime - delta))
        {
            Console.WriteLine($"Frame: {currentFrame}, UV Offset: ({uOffset:F2}, {vOffset:F2})");
        }
    }

    // ========================================================================
    // ONCLOSING - CLEAN UP GPU RESOURCES
    // ========================================================================
    private static void OnClosing()
    {
        Console.WriteLine("Cleaning up...");
        if (textureId != 0) gl!.DeleteTexture(textureId);
        gl!.DeleteBuffer(vbo);
        gl.DeleteBuffer(ebo);
        gl.DeleteVertexArray(vao);
        gl.DeleteProgram(shaderProgram);
        gl.Dispose();
    }

    // ========================================================================
    // TEXTURE LOADING - SAME AS PROJECT 3.1
    // ========================================================================
    private static uint LoadTexture(string path)
    {
        if (!File.Exists(path))
        {
            return 0;
        }

        using var fs = File.OpenRead(path);
        var image = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);
        if (image == null) return 0;

        uint tex = gl!.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, tex);

        unsafe
        {
            fixed (byte* p = image.Data)
            {
                gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    (int)InternalFormat.Rgba,
                    (uint)image.Width,
                    (uint)image.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    p
                );
            }
        }

        // Set texture parameters for sprite sheets
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        gl.BindTexture(TextureTarget.Texture2D, 0);
        return tex;
    }

    // ========================================================================
    // SHADER UTILS - SAME AS PROJECT 3.1
    // ========================================================================
    private static uint CreateShaderProgram(string vertPath, string fragPath)
    {
        string vsrc = File.ReadAllText(vertPath);
        string fsrc = File.ReadAllText(fragPath);

        uint vs = CompileShader(ShaderType.VertexShader, vsrc);
        uint fs = CompileShader(ShaderType.FragmentShader, fsrc);

        uint prog = gl!.CreateProgram();
        gl.AttachShader(prog, vs);
        gl.AttachShader(prog, fs);
        gl.LinkProgram(prog);

        gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out int ok);
        if (ok == 0)
        {
            var log = gl.GetProgramInfoLog(prog);
            throw new Exception("Program link failed: " + log);
        }

        gl.DeleteShader(vs);
        gl.DeleteShader(fs);
        return prog;
    }

    private static uint CompileShader(ShaderType type, string src)
    {
        uint s = gl!.CreateShader(type);
        gl.ShaderSource(s, src);
        gl.CompileShader(s);

        gl.GetShader(s, ShaderParameterName.CompileStatus, out int ok);
        if (ok == 0)
        {
            var log = gl.GetShaderInfoLog(s);
            throw new Exception($"{type} compile failed: {log}");
        }
        return s;
    }
}
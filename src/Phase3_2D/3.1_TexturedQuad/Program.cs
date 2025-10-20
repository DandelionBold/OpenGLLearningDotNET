using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;
using StbImageSharp;
using System.Numerics;

// ============================================================================
// PROJECT 3.1: TEXTURED QUAD - LOAD AND RENDER AN IMAGE (PNG/JPG)
// ============================================================================
// OVERVIEW (READ THIS FIRST!)
//
// GOAL:
//   - Display an image (e.g., PNG) on the screen using a textured rectangle (quad).
//
// WHAT'S NEW VS PHASE 2:
//   - Load an image file from disk (using StbImageSharp)
//   - Upload image data to GPU as an OpenGL Texture2D
//   - Add UVs (texture coordinates) to your vertices
//   - Sample the texture in a fragment shader using sampler2D
//   - Use a 2D orthographic projection for pixel-like coordinates
//
// BIG-PICTURE FLOW:
//   CPU (C#):
//     1) Create quad vertices with positions+UVs, indices (EBO)
//     2) Load image → pixels in CPU → upload to GPU texture
//     3) Configure texture filter/wrap; create shaders
//     4) Create orthographic projection (2D)
//     5) Draw the quad; shader samples texture and shows the image
//
// SHADER DATA FLOW:
//   Vertex:
//     - Input: aPosition (vec3), aTexCoord (vec2)
//     - Output: vTexCoord → to fragment shader
//     - Apply: uProjection * vec4(aPosition, 1)
//   Fragment:
//     - Input: vTexCoord
//     - Uniform: uTexture0 (sampler2D bound to texture unit 0)
//     - Output: FragColor = texture(uTexture0, vTexCoord)
//
// FILES CREATED:
//   - Shaders/shader.vert    (vertex shader)
//   - Shaders/shader.frag    (fragment shader)
//   - Program.cs             (this file - detailed comments)
//   - README.md              (project guide)
//
// IMPORTANT: Place an image at: src/Phase3_2D/3.1_TexturedQuad/textures/sample.png
// ============================================================================

namespace TexturedQuad;

class Program
{
    // ========================================================================
    // WINDOW + OPENGL CONTEXT
    // ========================================================================
    private static IWindow? window;  // Silk.NET window (creates surface for drawing)
    private static GL? gl;           // OpenGL context (the API surface to the GPU)

    // ========================================================================
    // GPU OBJECT HANDLES
    // ========================================================================
    private static uint vao;             // Vertex Array Object - remembers vertex format & EBO binding
    private static uint vbo;             // Vertex Buffer Object - stores vertex data (positions + UVs)
    private static uint ebo;             // Element Buffer Object - stores indices (which vertices to draw)
    private static uint shaderProgram;   // Shader program - vertex + fragment shaders linked together
    private static uint textureId;       // OpenGL texture object ID

    // ========================================================================
    // GEOMETRY: A QUAD (RECTANGLE) MADE OF TWO TRIANGLES
    // ========================================================================
    // Vertex format: Interleaved
    //   - 3 floats for position: x, y, z
    //   - 2 floats for UV:       u, v
    //
    // Coordinate system:
    //   - Positions are centered at origin (0,0,0) with width/height ~1 unit
    //   - UV coordinates go from 0..1 across the texture
    //     (0,0) = bottom-left of the image, (1,1) = top-right (with our current shader)
    private static readonly float[] quadVertices = new float[]
    {
        //   x      y     z     u     v
        -0.5f,  0.5f, 0.0f,   0.0f, 1.0f,   // top-left
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f,   // bottom-left
         0.5f, -0.5f, 0.0f,   1.0f, 0.0f,   // bottom-right
         0.5f,  0.5f, 0.0f,   1.0f, 1.0f    // top-right
    };

    // Indices define which vertices make up triangles (re-using shared verts):
    // Triangle 1: 0,1,2  |  Triangle 2: 0,2,3
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
        Console.WriteLine("PROJECT 3.1: TEXTURED QUAD!");
        Console.WriteLine("====================================\n");

        // WINDOW CREATION:
        // We request a default 800x600 window; the rest of our OpenGL setup happens via callbacks below
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "3.1 - Textured Quad";

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
        // VAO is a small state object that "remembers" vertex attribute setup and EBO binding.
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        // -------------------------------
        // 2) CREATE VBO & UPLOAD VERTICES
        // -------------------------------
        // VBO holds our quadVertices array in GPU memory.
        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe
        {
            fixed (float* buf = quadVertices) // pin managed array to get pointer
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(quadVertices.Length * sizeof(float)), // number of bytes
                    buf,                                          // pointer to data
                    BufferUsageARB.StaticDraw);                    // usage hint
            }
        }

        // ---------------------------------
        // 3) CREATE EBO & UPLOAD INDICES
        // ---------------------------------
        // EBO holds indices so we can re-use vertices (more efficient)
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
        // 4) CONFIGURE VERTEX ATTRIBUTES (HOW TO READ THE VERTEX DATA IN THE VBO)
        // --------------------------------------------------------------------------
        // Our vertex has 5 floats per vertex: [x, y, z, u, v] => stride = 5 * sizeof(float)
        int stride = 5 * sizeof(float);
        unsafe
        {
            // Attribute 0 = position (x,y,z) -> 3 floats, starts at offset 0
            gl.VertexAttribPointer(
                0,                                  // attribute location in shader
                3,                                  // number of components
                VertexAttribPointerType.Float,      // type of each component
                false,                              // normalize?
                (uint)stride,                       // stride (bytes between vertices)
                (void*)0                            // offset from start of one vertex
            );
            gl.EnableVertexAttribArray(0);

            // Attribute 1 = UV (u,v) -> 2 floats, starts after 3 floats (pos)
            gl.VertexAttribPointer(
                1,                                  // attribute location in shader
                2,                                  // number of components
                VertexAttribPointerType.Float,      // type of each component
                false,                              // normalize?
                (uint)stride,                       // stride
                (void*)(3 * sizeof(float))          // offset: 3 floats for pos
            );
            gl.EnableVertexAttribArray(1);
        }

        // -----------------------------
        // 5) COMPILE/LINK GLSL SHADERS
        // -----------------------------
        // Loads Shaders/shader.vert + Shaders/shader.frag and creates a program
        shaderProgram = CreateShaderProgram("Shaders/shader.vert", "Shaders/shader.frag");

        // ------------------------------------------------------------
        // 6) LOAD TEXTURE FROM DISK → UPLOAD TO GPU AS Texture2D
        // ------------------------------------------------------------
        // NOTE: This expects an image at textures/sample.png relative to the project folder.
        // Supported: PNG/JPG (we request RGBA to be safe)
        textureId = LoadTexture("textures/sample.png");
        if (textureId == 0)
        {
            Console.WriteLine("WARNING: Texture not found. Place a PNG at textures/sample.png.");
            // The program still runs; the sampler returns black if no texture bound.
        }

        // Tell the shader which texture unit the sampler should read from:
        // We will bind the texture to GL texture unit 0 at draw time, so set sampler to 0.
        gl.UseProgram(shaderProgram);
        int samplerLoc = gl.GetUniformLocation(shaderProgram, "uTexture0");
        if (samplerLoc != -1)
        {
            gl.Uniform1(samplerLoc, 0); // 0 = GL_TEXTURE0
        }

        // ---------------------------------------------------------
        // 7) CREATE ORTHOGRAPHIC PROJECTION FOR 2D (NO PERSPECTIVE)
        // ---------------------------------------------------------
        // We want x ∈ [−aspect, +aspect], y ∈ [−1, +1] so content is not stretched.
        float aspect = window.Size.X / (float)window.Size.Y;
        Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(-aspect, aspect, -1f, 1f, -1f, 1f);
        int projLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        if (projLoc != -1)
        {
            // OpenGL expects column-major matrices. System.Numerics is row-major but
            // we provide the float array in the required format and pass transpose=false.
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

        Console.WriteLine("Setup complete. Rendering textured quad.\n");
    }

    // ========================================================================
    // ONRENDER - CALLED EVERY FRAME
    // ========================================================================
    private static void OnRender(double delta)
    {
        // Clear old frame
        gl!.ClearColor(0.12f, 0.12f, 0.15f, 1.0f); // dark bluish background
        gl.Clear(ClearBufferMask.ColorBufferBit);

        // Use our shader program
        gl.UseProgram(shaderProgram);

        // Bind texture unit 0 and the 2D texture we created
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
    // ONCLOSING - CLEAN UP GPU RESOURCES
    // ========================================================================
    private static void OnClosing()
    {
        Console.WriteLine("Cleaning up...");
        if (textureId != 0) gl!.DeleteTexture(textureId);  // free texture
        gl!.DeleteBuffer(vbo);                              // free VBO
        gl.DeleteBuffer(ebo);                               // free EBO
        gl.DeleteVertexArray(vao);                          // free VAO
        gl.DeleteProgram(shaderProgram);                    // free shader program
        gl.Dispose();                                       // release OpenGL interface
    }

    // ========================================================================
    // TEXTURE LOADING - LOAD PNG/JPG → CREATE Texture2D
    // ========================================================================
    private static uint LoadTexture(string path)
    {
        // 1) Check if file exists
        if (!File.Exists(path))
        {
            return 0; // missing texture is okay; we warn and continue
        }

        // 2) Read the image into CPU memory using StbImageSharp
        using var fs = File.OpenRead(path);
        // Request RGBA (4 channels) to make GL upload straightforward
        var image = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);
        if (image == null) return 0;

        // 3) Generate a GL texture ID and bind it as the current Texture2D
        uint tex = gl!.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, tex);

        // 4) Upload pixel data to GPU
        // Note: image.Data is byte[] containing all pixels in RGBA8
        unsafe
        {
            fixed (byte* p = image.Data)
            {
                gl.TexImage2D(
                    TextureTarget.Texture2D,    // target (2D texture)
                    0,                          // mip level 0
                    (int)InternalFormat.Rgba,   // internal format on GPU
                    (uint)image.Width,          // width
                    (uint)image.Height,         // height
                    0,                          // border = 0
                    PixelFormat.Rgba,           // incoming pixel format
                    PixelType.UnsignedByte,     // incoming pixel type
                    p                           // pointer to pixels
                );
            }
        }

        // 5) Set sampler parameters (filtering & wrapping)
        // Filtering controls how texture scales:
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        // Wrapping controls behavior when UVs go outside [0,1]
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);

        // 6) Generate mipmaps (smaller versions for minification)
        gl.GenerateMipmap(TextureTarget.Texture2D);

        // 7) Unbind texture for safety; we will bind it again before drawing
        gl.BindTexture(TextureTarget.Texture2D, 0);
        return tex;
    }

    // ========================================================================
    // SHADER UTILS - COMPILE/LINK HELPERS
    // ========================================================================
    private static uint CreateShaderProgram(string vertPath, string fragPath)
    {
        // Read shader source files
        string vsrc = File.ReadAllText(vertPath);
        string fsrc = File.ReadAllText(fragPath);

        // Compile individual shaders
        uint vs = CompileShader(ShaderType.VertexShader, vsrc);
        uint fs = CompileShader(ShaderType.FragmentShader, fsrc);

        // Create program and link
        uint prog = gl!.CreateProgram();
        gl.AttachShader(prog, vs);
        gl.AttachShader(prog, fs);
        gl.LinkProgram(prog);

        // Verify link status
        gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out int ok);
        if (ok == 0)
        {
            var log = gl.GetProgramInfoLog(prog);
            throw new Exception("Program link failed: " + log);
        }

        // We can delete the shader objects once linked
        gl.DeleteShader(vs);
        gl.DeleteShader(fs);
        return prog;
    }

    private static uint CompileShader(ShaderType type, string src)
    {
        uint s = gl!.CreateShader(type);
        gl.ShaderSource(s, src);
        gl.CompileShader(s);

        // Verify compile status
        gl.GetShader(s, ShaderParameterName.CompileStatus, out int ok);
        if (ok == 0)
        {
            var log = gl.GetShaderInfoLog(s);
            throw new Exception($"{type} compile failed: {log}");
        }
        return s;
    }
}

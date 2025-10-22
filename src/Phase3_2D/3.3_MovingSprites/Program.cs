using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using StbImageSharp;

namespace _3._3_MovingSprites;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    private static IInputContext? input;
    private static IKeyboard? keyboard;

    private static uint vao, vbo, ebo, shader;
    private static uint texPlayer, texEnemy;

    private const int playerFramesPerRow = 8;
    private const int playerRows = 1;
    private const int playerTotalFrames = 8;
    private const int enemyFramesPerRow = 4;
    private const int enemyRows = 2;
    private const int enemyTotalFrames = 8;

    private static float playerFrameW = 1.0f / playerFramesPerRow;
    private static float playerFrameH = 1.0f / playerRows;
    private static float enemyFrameW = 1.0f / enemyFramesPerRow;
    private static float enemyFrameH = 1.0f / enemyRows;

    private struct Sprite
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 size;
        public bool facingRight;
        public int currentFrame;
        public float frameTime;
        public float animationFPS;
        public int framesPerRow;
        public int rows;
        public int totalFrames;
        public float frameW;
        public float frameH;
        public uint texture;
        public bool isPlayer;
        public bool isGrounded;
        public float patrolMinX;
        public float patrolMaxX;
        public float moveSpeed;
    }

    private static Sprite player;
    private static Sprite[] enemies = Array.Empty<Sprite>();

    // World/physics
    private static float gravity = -3.5f;
    private static float jumpImpulse = 1.8f;
    private static float groundY = -0.7f;
    private static float playerSpeed = 1.5f;
    private static float enemySpeed = 0.8f;

    private static double lastTime;

    public static void Main()
    {
        var opts = WindowOptions.Default;
        opts.Title = "3.3: Moving Sprites";
        opts.Size = new Vector2D<int>(960, 720);
        window = Window.Create(opts);
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClose;
        window.Run();
    }

    private static unsafe void OnLoad()
    {
        gl = window!.CreateOpenGL();
        input = window.CreateInput();
        foreach (var kb in input.Keyboards)
        {
            keyboard = kb;
            kb.KeyDown += OnKeyDown;
        }

        gl!.ClearColor(0.08f, 0.08f, 0.10f, 1f);

        // Quad geometry (pos, uv)
        float[] quad = new float[]
        {
            //   x      y     z     u     v
            -0.5f,  0.5f, 0f,    0f, 1f,
            -0.5f, -0.5f, 0f,    0f, 0f,
             0.5f, -0.5f, 0f,    1f, 0f,
             0.5f,  0.5f, 0f,    1f, 1f,
        };
        uint[] indices = { 0, 1, 2, 0, 2, 3 };

        vao = gl.GenVertexArray();
        vbo = gl.GenBuffer();
        ebo = gl.GenBuffer();
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (float* p = quad)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quad.Length * sizeof(float)), p, BufferUsageARB.StaticDraw);
        }
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (uint* ip = indices)
        {
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), ip, BufferUsageARB.StaticDraw);
        }
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        // Shaders
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        shader = CreateShaderProgram(Path.Combine(exeDir, "Shaders", "shader.vert"),
                                     Path.Combine(exeDir, "Shaders", "shader.frag"));
        gl.UseProgram(shader);

        // Projection
        var proj = Matrix4x4.CreateOrthographicOffCenter(-1f, 1f, -1f, 1f, -1f, 1f);
        int projLoc = gl.GetUniformLocation(shader, "uProjection");
        if (projLoc != -1)
        {
            float* m = stackalloc float[16];
            m[0] = proj.M11; m[1] = proj.M12; m[2] = proj.M13; m[3] = proj.M14;
            m[4] = proj.M21; m[5] = proj.M22; m[6] = proj.M23; m[7] = proj.M24;
            m[8] = proj.M31; m[9] = proj.M32; m[10] = proj.M33; m[11] = proj.M34;
            m[12] = proj.M41; m[13] = proj.M42; m[14] = proj.M43; m[15] = proj.M44;
            gl.UniformMatrix4(projLoc, 1, false, m);
        }

        // Textures
        texPlayer = LoadTexture(Path.Combine(exeDir, "textures", "sprite_sheet_1x8.png"));
        texEnemy = LoadTexture(Path.Combine(exeDir, "textures", "sprite_sheet_2x4.png"));

        // Sprites init
        player = new Sprite
        {
            position = new Vector2(-0.7f, groundY + 0.25f),
            velocity = Vector2.Zero,
            size = new Vector2(0.25f, 0.5f),
            facingRight = true,
            currentFrame = 0,
            frameTime = 0f,
            animationFPS = 12f,
            framesPerRow = playerFramesPerRow,
            rows = playerRows,
            totalFrames = playerTotalFrames,
            frameW = playerFrameW,
            frameH = playerFrameH,
            texture = texPlayer,
            isPlayer = true,
            isGrounded = true,
            moveSpeed = playerSpeed
        };

        enemies = new Sprite[4];
        for (int i = 0; i < enemies.Length; i++)
        {
            float x = -0.3f + i * 0.35f;
            enemies[i] = new Sprite
            {
                position = new Vector2(x, groundY + 0.22f),
                velocity = new Vector2((i % 2 == 0 ? enemySpeed : -enemySpeed), 0f),
                size = new Vector2(0.22f, 0.44f),
                facingRight = (i % 2 == 0),
                currentFrame = 0,
                frameTime = 0f,
                animationFPS = 8f,
                framesPerRow = enemyFramesPerRow,
                rows = enemyRows,
                totalFrames = enemyTotalFrames,
                frameW = enemyFrameW,
                frameH = enemyFrameH,
                texture = texEnemy,
                isPlayer = false,
                isGrounded = true,
                patrolMinX = x - 0.25f,
                patrolMaxX = x + 0.25f,
                moveSpeed = enemySpeed
            };
        }

        lastTime = window!.Time;
    }

    private static void OnKeyDown(IKeyboard kb, Key key, int code)
    {
        if (key == Key.Escape)
            window?.Close();
    }

    private static void OnUpdate(double dt)
    {
        if (keyboard == null) return;

        // Input: arrows + WASD
        float horizontal = 0f;
        if (keyboard.IsKeyPressed(Key.Right) || keyboard.IsKeyPressed(Key.D)) horizontal += 1f;
        if (keyboard.IsKeyPressed(Key.Left) || keyboard.IsKeyPressed(Key.A)) horizontal -= 1f;

        // Move player horizontally
        player.velocity.X = horizontal * player.moveSpeed;
        if (horizontal > 0.01f) player.facingRight = true;
        else if (horizontal < -0.01f) player.facingRight = false;

        // Jump
        if (player.isGrounded && (keyboard.IsKeyPressed(Key.Space) || keyboard.IsKeyPressed(Key.W) || keyboard.IsKeyPressed(Key.Up)))
        {
            player.velocity.Y = jumpImpulse;
            player.isGrounded = false;
        }

        // Gravity
        player.velocity.Y += gravity * (float)dt;

        // Integrate
        player.position += player.velocity * (float)dt;

        // Ground collision
        float halfH = player.size.Y * 0.5f;
        if (player.position.Y - halfH <= groundY)
        {
            player.position.Y = groundY + halfH;
            player.velocity.Y = 0f;
            player.isGrounded = true;
        }

        // Screen bounds for player
        ClampToScreen(ref player);

        // Enemy patrol
        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            e.position += new Vector2(e.velocity.X, 0f) * (float)dt;
            if (e.position.X < e.patrolMinX)
            {
                e.position.X = e.patrolMinX;
                e.velocity.X = MathF.Abs(e.moveSpeed);
                e.facingRight = true;
            }
            else if (e.position.X > e.patrolMaxX)
            {
                e.position.X = e.patrolMaxX;
                e.velocity.X = -MathF.Abs(e.moveSpeed);
                e.facingRight = false;
            }
            ClampToScreen(ref e);
            enemies[i] = e;
        }

        // Player vs enemies collisions (AABB resolve minimal)
        for (int i = 0; i < enemies.Length; i++)
        {
            ResolveAABB(ref player, ref enemies[i]);
        }

        // Animations
        UpdateAnimation(ref player, dt);
        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            UpdateAnimation(ref e, dt);
            enemies[i] = e;
        }
    }

    private static unsafe void OnRender(double dt)
    {
        gl!.Clear(ClearBufferMask.ColorBufferBit);
        gl.UseProgram(shader);
        gl.BindVertexArray(vao);

        // Draw player
        DrawSprite(player);
        // Draw enemies
        for (int i = 0; i < enemies.Length; i++) DrawSprite(enemies[i]);
    }

    private static void OnClose()
    {
        // Cleanup
        gl?.DeleteBuffer(vbo);
        gl?.DeleteBuffer(ebo);
        gl?.DeleteVertexArray(vao);
        if (texPlayer != 0) gl?.DeleteTexture(texPlayer);
        if (texEnemy != 0) gl?.DeleteTexture(texEnemy);
        if (shader != 0) gl?.DeleteProgram(shader);
    }

    private static void UpdateAnimation(ref Sprite s, double dt)
    {
        bool moving = MathF.Abs(s.velocity.X) > 0.01f || MathF.Abs(s.velocity.Y) > 0.01f;
        if (moving)
        {
            s.frameTime += (float)dt;
            float frameDur = 1.0f / s.animationFPS;
            s.currentFrame = (int)(s.frameTime / frameDur) % s.totalFrames;
        }
        else
        {
            s.frameTime = 0f;
            s.currentFrame = 0; // idle
        }
    }

    private static void ClampToScreen(ref Sprite s)
    {
        Vector2 half = s.size * 0.5f;
        // X
        if (s.position.X - half.X < -1f) { s.position.X = -1f + half.X; s.velocity.X = 0f; }
        if (s.position.X + half.X > 1f) { s.position.X = 1f - half.X; s.velocity.X = 0f; }
        // Y
        if (s.position.Y - half.Y < -1f) { s.position.Y = -1f + half.Y; s.velocity.Y = 0f; s.isGrounded = true; }
        if (s.position.Y + half.Y > 1f) { s.position.Y = 1f - half.Y; s.velocity.Y = 0f; }
    }

    private static void ResolveAABB(ref Sprite a, ref Sprite b)
    {
        Vector2 aHalf = a.size * 0.5f;
        Vector2 bHalf = b.size * 0.5f;
        Vector2 delta = b.position - a.position;
        float overlapX = (aHalf.X + bHalf.X) - MathF.Abs(delta.X);
        float overlapY = (aHalf.Y + bHalf.Y) - MathF.Abs(delta.Y);
        if (overlapX > 0f && overlapY > 0f)
        {
            // push player out along minimal axis
            if (overlapX < overlapY)
            {
                float dir = MathF.Sign(delta.X);
                a.position.X -= overlapX * dir;
                a.velocity.X = 0f;
            }
            else
            {
                float dir = MathF.Sign(delta.Y);
                a.position.Y -= overlapY * dir;
                a.velocity.Y = 0f;
                if (dir > 0f) a.isGrounded = true; // player on top of enemy
            }
        }
    }

    private static unsafe void DrawSprite(in Sprite s)
    {
        // Model = T * S
        var model = Matrix4x4.CreateScale(s.size.X, s.size.Y, 1f) *
                    Matrix4x4.CreateTranslation(new Vector3(s.position, 0f));
        int modelLoc = gl!.GetUniformLocation(shader, "uModel");
        if (modelLoc != -1)
        {
            float* m = stackalloc float[16];
            m[0] = model.M11; m[1] = model.M12; m[2] = model.M13; m[3] = model.M14;
            m[4] = model.M21; m[5] = model.M22; m[6] = model.M23; m[7] = model.M24;
            m[8] = model.M31; m[9] = model.M32; m[10] = model.M33; m[11] = model.M34;
            m[12] = model.M41; m[13] = model.M42; m[14] = model.M43; m[15] = model.M44;
            gl!.UniformMatrix4(modelLoc, 1, false, m);
        }

        // UV scale/offset
        Vector2 scale = new Vector2(s.frameW, s.frameH);
        int scaleLoc = gl!.GetUniformLocation(shader, "uUVScale");
        if (scaleLoc != -1) gl.Uniform2(scaleLoc, scale.X, scale.Y);

        int col = s.currentFrame % s.framesPerRow;
        int row = s.currentFrame / s.framesPerRow;
        Vector2 offset = new Vector2(col * s.frameW, row * s.frameH);
        int offLoc = gl!.GetUniformLocation(shader, "uUVOffset");
        if (offLoc != -1) gl.Uniform2(offLoc, offset.X, offset.Y);

        int flipLoc = gl!.GetUniformLocation(shader, "uFlipX");
        gl!.Uniform1(flipLoc, s.facingRight ? 0 : 1);

        // Bind texture and draw
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, s.texture);
        int texLoc = gl.GetUniformLocation(shader, "uTexture0");
        if (texLoc != -1) gl.Uniform1(texLoc, 0);
        gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static unsafe uint LoadTexture(string path)
    {
        using var fs = File.OpenRead(path);
        var img = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);
        uint id = gl!.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, id);
        fixed (byte* p = img.Data)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)img.Width, (uint)img.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        gl.GenerateMipmap(TextureTarget.Texture2D);
        return id;
    }

    private static uint CompileShader(ShaderType type, string src)
    {
        uint s = gl!.CreateShader(type);
        gl.ShaderSource(s, src);
        gl.CompileShader(s);
        string info = gl.GetShaderInfoLog(s);
        if (!string.IsNullOrEmpty(info))
        {
            Console.WriteLine($"{type} compile log: {info}");
        }
        return s;
    }

    private static uint CreateShaderProgram(string vertPath, string fragPath)
    {
        string vert = File.ReadAllText(vertPath);
        string frag = File.ReadAllText(fragPath);
        uint vs = CompileShader(ShaderType.VertexShader, vert);
        uint fs = CompileShader(ShaderType.FragmentShader, frag);
        uint prog = gl!.CreateProgram();
        gl.AttachShader(prog, vs);
        gl.AttachShader(prog, fs);
        gl.LinkProgram(prog);
        gl.DeleteShader(vs);
        gl.DeleteShader(fs);
        return prog;
    }
}



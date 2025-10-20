using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;
using StbImageSharp;
using System.Numerics;

namespace TexturedQuad_Stretch;

class Program
{
    private static IWindow? window;
    private static GL? gl;

    private static uint vao, vbo, ebo, shaderProgram, textureId;

    // Square quad: texture will stretch to fit this shape
    private static readonly float[] vertices = new float[]
    {
        // x,     y,     z,   u, v
        -0.5f,  0.5f,  0f,  0f, 1f,
        -0.5f, -0.5f,  0f,  0f, 0f,
         0.5f, -0.5f,  0f,  1f, 0f,
         0.5f,  0.5f,  0f,  1f, 1f,
    };

    private static readonly uint[] indices = new uint[] { 0,1,2, 0,2,3 };

    static void Main(string[] args)
    {
        var opts = WindowOptions.Default;
        opts.Size = new Vector2D<int>(800, 600);
        opts.Title = "3.1a - Textured Quad (Stretch)";
        window = Window.Create(opts);
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Closing += OnClosing;
        window.Run();
    }

    private static void OnLoad()
    {
        gl = window!.CreateOpenGL();

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe { fixed (float* p = vertices) gl.BufferData(BufferTargetARB.ArrayBuffer,(nuint)(vertices.Length*sizeof(float)), p, BufferUsageARB.StaticDraw);}        

        ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        unsafe { fixed (uint* ip = indices) gl.BufferData(BufferTargetARB.ElementArrayBuffer,(nuint)(indices.Length*sizeof(uint)), ip, BufferUsageARB.StaticDraw);}        

        int stride = 5 * sizeof(float);
        unsafe {
            gl.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,(uint)stride,(void*)0);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(1,2,VertexAttribPointerType.Float,false,(uint)stride,(void*)(3*sizeof(float)));
            gl.EnableVertexAttribArray(1);
        }

        shaderProgram = CreateShaderProgram("Shaders/shader.vert","Shaders/shader.frag");

        textureId = LoadTexture("textures/sample.png");
        gl.UseProgram(shaderProgram);
        int samplerLoc = gl.GetUniformLocation(shaderProgram, "uTexture0");
        if (samplerLoc != -1) gl.Uniform1(samplerLoc, 0);

        float aspect = window.Size.X / (float)window.Size.Y;
        Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(-aspect, aspect, -1f, 1f, -1f, 1f);
        int projLoc = gl.GetUniformLocation(shaderProgram, "uProjection");
        if (projLoc != -1)
        {
            float[] m = new float[]{
                ortho.M11,ortho.M12,ortho.M13,ortho.M14,
                ortho.M21,ortho.M22,ortho.M23,ortho.M24,
                ortho.M31,ortho.M32,ortho.M33,ortho.M34,
                ortho.M41,ortho.M42,ortho.M43,ortho.M44};
            unsafe { fixed(float* pm=m) gl.UniformMatrix4(projLoc,1,false,pm);}        
        }
    }

    private static void OnRender(double dt)
    {
        gl!.ClearColor(0.12f,0.12f,0.15f,1f);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.UseProgram(shaderProgram);
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, textureId);
        gl.BindVertexArray(vao);
        unsafe { gl.DrawElements(PrimitiveType.Triangles,6,DrawElementsType.UnsignedInt,(void*)0);}        
    }

    private static void OnClosing()
    {
        if(textureId!=0) gl!.DeleteTexture(textureId);
        gl!.DeleteBuffer(vbo); gl.DeleteBuffer(ebo); gl.DeleteVertexArray(vao); gl.DeleteProgram(shaderProgram); gl.Dispose();
    }

    private static uint LoadTexture(string path)
    {
        if(!File.Exists(path)) return 0;
        using var fs = File.OpenRead(path);
        var img = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);
        uint tex = gl!.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, tex);
        unsafe { fixed(byte* p = img.Data) gl.TexImage2D(TextureTarget.Texture2D,0,(int)InternalFormat.Rgba,(uint)img.Width,(uint)img.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,p);}        
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        gl.GenerateMipmap(TextureTarget.Texture2D);
        gl.BindTexture(TextureTarget.Texture2D, 0);
        return tex;
    }

    private static uint CreateShaderProgram(string vp, string fp)
    {
        string vsrc = File.ReadAllText(vp);
        string fsrc = File.ReadAllText(fp);
        uint vs = Compile(ShaderType.VertexShader, vsrc);
        uint fs = Compile(ShaderType.FragmentShader, fsrc);
        uint prog = gl!.CreateProgram();
        gl.AttachShader(prog, vs); gl.AttachShader(prog, fs); gl.LinkProgram(prog);
        gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out int ok); if(ok==0) throw new Exception(gl.GetProgramInfoLog(prog));
        gl.DeleteShader(vs); gl.DeleteShader(fs); return prog;
    }
    private static uint Compile(ShaderType t, string s)
    { uint sh = gl!.CreateShader(t); gl.ShaderSource(sh,s); gl.CompileShader(sh); gl.GetShader(sh, ShaderParameterName.CompileStatus, out int ok); if(ok==0) throw new Exception(gl.GetShaderInfoLog(sh)); return sh; }
}

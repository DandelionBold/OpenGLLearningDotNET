using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.IO;
using StbImageSharp;
using System.Numerics;

namespace TexturedQuad_PreserveAspect;

class Program
{
    private static IWindow? window; private static GL? gl;
    private static uint vao,vbo,ebo,shaderProgram,textureId;

    // Square unit quad; we will scale by texture aspect via model matrix
    private static readonly float[] vertices = new float[]
    {
        -0.5f,  0.5f, 0f,  0f,1f,
        -0.5f, -0.5f, 0f,  0f,0f,
         0.5f, -0.5f, 0f,  1f,0f,
         0.5f,  0.5f, 0f,  1f,1f,
    };
    private static readonly uint[] indices = new uint[]{0,1,2,0,2,3};

    private static float textureAspect = 1f; // width/height

    static void Main(string[] args)
    {
        var opts = WindowOptions.Default; opts.Size = new Vector2D<int>(800,600); opts.Title = "3.1b - Textured Quad (Preserve Aspect)";
        window = Window.Create(opts);
        window.Load += OnLoad; window.Render += OnRender; window.Closing += OnClosing; window.Run();
    }

    private static void OnLoad()
    {
        gl = window!.CreateOpenGL();
        vao = gl.GenVertexArray(); gl.BindVertexArray(vao);
        vbo = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe { fixed(float* p=vertices) gl.BufferData(BufferTargetARB.ArrayBuffer,(nuint)(vertices.Length*sizeof(float)),p,BufferUsageARB.StaticDraw);}        
        ebo = gl.GenBuffer(); gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        unsafe { fixed(uint* ip=indices) gl.BufferData(BufferTargetARB.ElementArrayBuffer,(nuint)(indices.Length*sizeof(uint)),ip,BufferUsageARB.StaticDraw);}        
        int stride = 5*sizeof(float);
        unsafe{
            gl.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false,(uint)stride,(void*)0); gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(1,2,VertexAttribPointerType.Float,false,(uint)stride,(void*)(3*sizeof(float))); gl.EnableVertexAttribArray(1);
        }

        shaderProgram = CreateShaderProgram("Shaders/shader.vert","Shaders/shader.frag");

        // Load texture and capture aspect
        (textureId, textureAspect) = LoadTextureWithAspect("textures/sample.png");
        gl.UseProgram(shaderProgram);
        int sam = gl.GetUniformLocation(shaderProgram, "uTexture0"); if(sam!=-1) gl.Uniform1(sam,0);

        // Projection
        float aspect = window.Size.X/(float)window.Size.Y;
        Matrix4x4 proj = Matrix4x4.CreateOrthographicOffCenter(-aspect,aspect,-1,1,-1,1);
        UploadMatrix("uProjection", proj);
    }

    private static void OnRender(double dt)
    {
        gl!.ClearColor(0.12f,0.12f,0.15f,1f); gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.UseProgram(shaderProgram);

        // Build model matrix that scales X by texture aspect to preserve image ratio
        Matrix4x4 model = Matrix4x4.CreateScale(textureAspect, 1f, 1f);
        UploadMatrix("uModel", model);

        gl.ActiveTexture(TextureUnit.Texture0); gl.BindTexture(TextureTarget.Texture2D, textureId);
        gl.BindVertexArray(vao);
        unsafe { gl.DrawElements(PrimitiveType.Triangles,6,DrawElementsType.UnsignedInt,(void*)0);}        
    }

    private static void OnClosing(){ if(textureId!=0) gl!.DeleteTexture(textureId); gl!.DeleteBuffer(vbo); gl.DeleteBuffer(ebo); gl.DeleteVertexArray(vao); gl.DeleteProgram(shaderProgram); gl.Dispose(); }

    private static (uint tex, float aspect) LoadTextureWithAspect(string path)
    {
        if(!File.Exists(path)) return (0,1f);
        using var fs = File.OpenRead(path);
        var img = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);
        float aspect = img.Width/(float)img.Height;
        uint tex = gl!.GenTexture(); gl.BindTexture(TextureTarget.Texture2D, tex);
        unsafe { fixed(byte* p=img.Data) gl.TexImage2D(TextureTarget.Texture2D,0,(int)InternalFormat.Rgba,(uint)img.Width,(uint)img.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,p);}        
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,(int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,(int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,(int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,(int)GLEnum.Repeat);
        gl.GenerateMipmap(TextureTarget.Texture2D); gl.BindTexture(TextureTarget.Texture2D,0);
        return (tex, aspect);
    }

    private static void UploadMatrix(string uniformName, Matrix4x4 m)
    {
        int loc = gl!.GetUniformLocation(shaderProgram, uniformName); if(loc==-1) return;
        float[] a = new float[]{m.M11,m.M12,m.M13,m.M14,m.M21,m.M22,m.M23,m.M24,m.M31,m.M32,m.M33,m.M34,m.M41,m.M42,m.M43,m.M44};
        unsafe { fixed(float* p=a) gl.UniformMatrix4(loc,1,false,p);}        
    }

    private static uint CreateShaderProgram(string vp,string fp){ string v=File.ReadAllText(vp); string f=File.ReadAllText(fp); uint vs=Compile(ShaderType.VertexShader,v); uint fs=Compile(ShaderType.FragmentShader,f); uint prog=gl!.CreateProgram(); gl.AttachShader(prog,vs); gl.AttachShader(prog,fs); gl.LinkProgram(prog); gl.GetProgram(prog,ProgramPropertyARB.LinkStatus,out int ok); if(ok==0) throw new Exception(gl.GetProgramInfoLog(prog)); gl.DeleteShader(vs); gl.DeleteShader(fs); return prog; }
    private static uint Compile(ShaderType t,string s){ uint id=gl!.CreateShader(t); gl.ShaderSource(id,s); gl.CompileShader(id); gl.GetShader(id,ShaderParameterName.CompileStatus,out int ok); if(ok==0) throw new Exception(gl.GetShaderInfoLog(id)); return id; }
}

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

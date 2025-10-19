# OpenGL Learning with Silk.NET

Welcome to your OpenGL learning journey! This repository contains a complete beginner-to-advanced course for learning OpenGL graphics programming in C#/.NET using Silk.NET.

## 🚀 Quick Start

### Run Your First OpenGL Application

```bash
cd src/Phase1_Foundation/1.1_EmptyWindow
dotnet run
```

You should see a window open with a dark blue background. Congratulations - you've just run your first OpenGL program!

---

## 📁 Project Structure

```
OpenGLLearning/
├── OpenGLLearning.sln          # Visual Studio solution
├── README.md                   # This file
├── PROJECT_STATUS.md           # Current progress tracker
├── .gitignore                  # Git ignore rules
└── src/
    └── Phase1_Foundation/
        ├── README.md           # Phase 1 guide and tutorials
        └── 1.1_EmptyWindow/    # Your first OpenGL project
```

---

## 🎯 Learning Path

This course is divided into 6 phases, taking you from complete beginner to confident OpenGL developer.

### Phase 1: Foundation Setup (Week 1 - 3-5 hours) ✅ COMPLETE!

- ✅ **1.1**: Empty window that opens and closes
- ✅ **1.2**: Window with changing background colors  
- ✅ **1.3**: Responding to keyboard/mouse input

### Phase 2: Your First Triangle (Week 1-2 - 8-12 hours) ✅ COMPLETE!

- ✅ **2.1**: Colored Triangle - VBO, VAO, Shaders, Pipeline
- ✅ **2.2**: Multi-Color Triangle - Gradients & GPU Interpolation
- ✅ **2.3**: Rotating Triangle - Matrices & Transformations
- ✅ **2.4**: Multiple Shapes - Index Buffers (EBO)

### Phase 3: 2D Graphics Mastery (Week 3-5 - 15-20 hours) 🟡 NEXT!

- ⏳ **3.1**: Textured Quad - Load and display images
- ⏳ **3.2**: Sprite Animation - Sprite sheets
- ⏳ **3.3**: Moving Sprites - Keyboard control
- ⏳ **3.4**: Simple 2D Game - Pong or Breakout!
- ⏳ **3.5**: Particle System - Effects

### Phase 4: Introduction to 3D (Week 6-8 - 20-25 hours) ⚪

- 3D coordinate systems
- Camera system
- Lighting (Phong model)
- Loading 3D models

### Phase 5: Advanced Topics (Week 9-10 - 15-20 hours) ⚪

- Shadow mapping
- Normal mapping
- Post-processing effects
- Skybox

### Phase 6: Final Showcase Project (Week 11-12 - 15-25 hours) ⚪

- Build a complete 3D application
- Combine all learned concepts
- Create something impressive!

**Total Time**: 8-12 weeks (at 1-2 hours daily)

---

## 📚 Documentation

- **[PROJECT_STATUS.md](PROJECT_STATUS.md)** - See current progress and next steps
- **[src/Phase1_Foundation/README.md](src/Phase1_Foundation/README.md)** - Detailed Phase 1 guide
- **[Learning Plan]** - Full roadmap with all projects and concepts (see plan file)

---

## 🛠️ Technologies Used

- **.NET 9.0** - Modern C# development
- **Silk.NET 2.22.0** - OpenGL bindings for .NET
  - Silk.NET.OpenGL - OpenGL API
  - Silk.NET.Windowing - Window creation
  - Silk.NET.Maths - Graphics math
  - Silk.NET.GLFW - Cross-platform windowing

---

## 💻 Requirements

- .NET 9.0 SDK or later
- Windows, macOS, or Linux
- Graphics card with OpenGL 3.3+ support
- Any code editor (Visual Studio, VS Code, Rider, etc.)

### Check Your Setup

```bash
# Check .NET version
dotnet --version

# Should show 9.0.x or later
```

---

## 🎓 Learning Tips

1. **Type, Don't Copy**: Type out the code yourself to build muscle memory
2. **Read the Comments**: Every project has extensive inline documentation
3. **Experiment**: Change values, break things, and fix them
4. **Take Breaks**: Learning graphics programming can be intense
5. **Ask Questions**: If stuck, review the README files and comments
6. **Track Progress**: Check off completed projects in PROJECT_STATUS.md

---

## 🎨 What You'll Build

By the end of this course, you'll have built:

- 24 progressively complex projects
- Your own 2D game (Pong or Breakout clone)
- 3D scenes with lighting and shadows
- A model viewer
- A final showcase project of your choice:
  - 3D Scene Explorer, or
  - Simple 3D Game, or
  - Graphics Tech Demo

---

## 📖 Resources

### Official Documentation

- [Silk.NET Docs](https://dotnet.github.io/Silk.NET/)
- [OpenGL Reference](https://www.khronos.org/opengl/)

### Tutorials (concepts translate from C++ to C#)

- [Learn OpenGL](https://learnopengl.com/) - Excellent OpenGL tutorial
- [OpenGL Tutorial](http://www.opengl-tutorial.org/)

### Tools

- [Shader Toy](https://www.shadertoy.com/) - GLSL shader playground
- [GLSL Sandbox](http://glslsandbox.com/) - Another shader editor

---

## 🏁 Getting Started

1. **Verify the build**:

   ```bash
   dotnet build OpenGLLearning.sln
   ```

2. **Run the first project**:

   ```bash
   cd src/Phase1_Foundation/1.1_EmptyWindow
   dotnet run
   ```

3. **Read the code**:
   Open `src/Phase1_Foundation/1.1_EmptyWindow/Program.cs` and read through all the comments.

4. **Experiment**:
   Try the experiments listed in `src/Phase1_Foundation/README.md`

5. **Move forward**:
   Once comfortable, we'll build Project 1.2!

---

## 📊 Progress Tracking

See [PROJECT_STATUS.md](PROJECT_STATUS.md) for:

- Current phase and project
- Completed projects checklist
- Next steps
- Overall progress percentage

---

## 🤝 Support

If you get stuck:

1. Check the README in the current phase folder
2. Read the inline code comments
3. Review the PROJECT_STATUS.md for common issues
4. Make sure your graphics drivers are up to date

---

## 🎉 You're Ready!

You have everything you need to start learning OpenGL. The first project is already complete and ready to run.

**Your first step**: Run the 1.1_EmptyWindow project and see your first OpenGL window!

```bash
cd src/Phase1_Foundation/1.1_EmptyWindow
dotnet run
```

Happy coding! 🚀✨

---

**Current Status**: Phase 2 COMPLETE! ✅ (29% of entire course!)  
**Next Up**: Phase 3 - 2D Graphics Mastery (Textures & Games!)

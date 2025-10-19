#version 330 core

// ============================================================================
// VERTEX SHADER - SUPER DETAILED EXPLANATION
// ============================================================================
// 
// WHAT IS A SHADER?
// A shader is a small program that runs ON THE GPU (not your CPU!).
// Your C# code runs on the CPU, but shaders run on the graphics card.
// This is MUCH FASTER for graphics operations!
//
// WHAT IS A VERTEX SHADER?
// This shader runs ONCE for EACH VERTEX (point) you send from C#.
// If you send 3 vertices (for a triangle), this runs 3 times.
// If you send 1000 vertices, this runs 1000 times - IN PARALLEL on GPU!
//
// ANALOGY:
// Think of vertices like dots in a connect-the-dots picture.
// The vertex shader processes each dot individually.
// ============================================================================

// ============================================================================
// INPUT VARIABLES (data coming FROM C#)
// ============================================================================
// "in" means this variable receives data from our C# program
// "layout(location = 0)" is like a mailbox number - it tells OpenGL
// which piece of data this is (we can have multiple inputs)
//
// "vec3" means 3 floats: (x, y, z) position
// "aPosition" is the name we chose (the 'a' means 'attribute')
layout(location = 0) in vec3 aPosition;

// WHY location = 0?
// In C#, we called: gl.VertexAttribPointer(0, ...)
// The '0' there matches 'location = 0' here!
// This connects the data from C# to this shader variable.

// ============================================================================
// MAIN FUNCTION (runs for each vertex)
// ============================================================================
void main()
{
    // ========================================================================
    // OUTPUT THE VERTEX POSITION
    // ========================================================================
    // "gl_Position" is a BUILT-IN variable that MUST be set
    // It's a vec4 (4 floats: x, y, z, w)
    // 
    // But wait! aPosition is vec3 (3 floats), not vec4!
    // vec4(aPosition, 1.0) converts it:
    //   - Takes x, y, z from aPosition
    //   - Adds 1.0 as the 4th component (w)
    //
    // WHAT IS W?
    // For now, ALWAYS use 1.0 for w. It's used for advanced math later.
    // Just remember: positions need 4 components (x, y, z, w=1.0)
    
    gl_Position = vec4(aPosition, 1.0);
    
    // WHAT DOES THIS MEAN?
    // We're telling OpenGL: "Put this vertex at position (x, y, z)"
    // OpenGL will use this position to draw the triangle.
    
    // ========================================================================
    // OPENGL COORDINATE SYSTEM (VERY IMPORTANT!)
    // ========================================================================
    // OpenGL uses "Normalized Device Coordinates" (NDC)
    // The screen goes from -1.0 to +1.0 in both X and Y:
    //
    //         Y
    //         ↑
    //         |
    //  (-1,1) +----------+ (1,1)     Top of screen
    //         |          |
    //         |          |
    // --------+----------+--------> X
    //  (-1,0) |  (0,0)   | (1,0)    Center
    //         |          |
    //         |          |
    // (-1,-1) +----------+ (1,-1)   Bottom of screen
    //
    // Examples:
    //   ( 0.0,  0.0) = Center of screen
    //   ( 0.0,  1.0) = Top center
    //   ( 0.0, -1.0) = Bottom center
    //   (-1.0,  0.0) = Left center
    //   ( 1.0,  0.0) = Right center
    //
    // If you go outside -1 to 1, it's off-screen (clipped)!
}

// ============================================================================
// WHAT HAPPENS AFTER THIS SHADER RUNS?
// ============================================================================
// 
// STEP 1: This shader runs 3 times (once per vertex)
//   Run 1: vertex at ( 0.0,  0.5, 0.0) → outputs position ( 0.0,  0.5, 0.0, 1.0)
//   Run 2: vertex at (-0.5, -0.5, 0.0) → outputs position (-0.5, -0.5, 0.0, 1.0)
//   Run 3: vertex at ( 0.5, -0.5, 0.0) → outputs position ( 0.5, -0.5, 0.0, 1.0)
//
// STEP 2: OpenGL connects these 3 points into a TRIANGLE (primitive assembly)
//
// STEP 3: OpenGL figures out which PIXELS are inside the triangle (rasterization)
//
// STEP 4: For EACH pixel inside, the FRAGMENT SHADER runs (next file!)
//
// VISUAL:
//   Vertex Shader → 3 positions → Triangle shape → Filled pixels → Fragment Shader
// ============================================================================


#version 330 core

// ============================================================================
// VERTEX SHADER
// ============================================================================
// This is your first shader! It runs once for each vertex (point) you send.
// 
// The vertex shader's job is to:
// 1. Receive vertex data (position, color, etc.)
// 2. Transform it (we're not doing this yet)
// 3. Pass it to the fragment shader
// ============================================================================

// INPUT: Position data from our C# program
// layout(location = 0) means this is the first attribute we send
layout(location = 0) in vec3 aPosition;

// MAIN FUNCTION: Runs for each vertex
void main()
{
    // gl_Position is a built-in output variable
    // It must be a vec4 (x, y, z, w)
    // We add 1.0 as the w component (for now, always use 1.0)
    gl_Position = vec4(aPosition, 1.0);
    
    // COORDINATE SYSTEM:
    // In OpenGL, the screen goes from -1 to 1:
    //   (-1,  1) -------- ( 1,  1)  Top
    //      |                 |
    //      |     (0, 0)      |      Center
    //      |                 |
    //   (-1, -1) -------- ( 1, -1)  Bottom
}

// ============================================================================
// WHAT HAPPENS NEXT?
// ============================================================================
// After this shader runs for all 3 vertices of our triangle:
// 1. OpenGL creates a triangle between them (rasterization)
// 2. For each pixel inside the triangle, the fragment shader runs
// 3. The fragment shader decides what color that pixel should be
// ============================================================================


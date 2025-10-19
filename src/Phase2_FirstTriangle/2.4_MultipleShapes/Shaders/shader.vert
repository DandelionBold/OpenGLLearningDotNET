#version 330 core

// ============================================================================
// VERTEX SHADER - WITH TRANSFORMATION MATRIX!
// ============================================================================
//
// THIS IS NEW! We're now applying a TRANSFORMATION to vertices!
//
// WHAT ARE TRANSFORMATIONS?
// - Rotation: Spin the triangle around
// - Translation: Move the triangle
// - Scale: Make bigger or smaller
// 
// We use a MATRIX (4x4 grid of numbers) to do this!
//
// THE BIG PICTURE:
// C# sends a rotation matrix → We multiply vertex position by it →
// → Vertex gets rotated! → Triangle spins! 🔄
// ============================================================================

// ============================================================================
// INPUT: Per-Vertex Data (same as Project 2.2)
// ============================================================================
layout(location = 0) in vec3 aPosition;  // Vertex position
layout(location = 1) in vec3 aColor;     // Vertex color

// ============================================================================
// OUTPUT: To Fragment Shader (same as Project 2.2)
// ============================================================================
out vec3 vertexColor;

// ============================================================================
// UNIFORM: Data from C# (SAME for ALL vertices!)
// ============================================================================
//
// NEW CONCEPT: UNIFORM VARIABLES
//
// "uniform" means this value is THE SAME for all vertices in ONE draw call
// It's like a global constant sent from C# to the shader
//
// DIFFERENCE between "in" and "uniform":
// - "in" = different value PER VERTEX (position, color)
// - "uniform" = SAME value for ALL VERTICES (transformation matrix, time, etc.)
//
// Example:
// - All 3 vertices use the SAME rotation angle
// - But each vertex has DIFFERENT position and color

uniform mat4 transform;

// WHAT IS mat4?
// mat4 = 4×4 matrix (grid of 16 numbers)
// Used for transformations in 3D graphics
//
// [ m00 m01 m02 m03 ]
// [ m10 m11 m12 m13 ]
// [ m20 m21 m22 m23 ]
// [ m30 m31 m32 m33 ]
//
// Don't worry about the math details yet!
// Just know: matrix × position = transformed position

// ============================================================================
// MAIN FUNCTION
// ============================================================================
void main()
{
    // NEW! Apply transformation to the position
    // We multiply the transform matrix by the position
    gl_Position = transform * vec4(aPosition, 1.0);
    
    // WHAT THIS DOES:
    // If transform is a rotation matrix, the vertex rotates!
    // If transform is identity matrix (no change), vertex stays same
    
    // BEFORE (Project 2.1 & 2.2):
    //   gl_Position = vec4(aPosition, 1.0);  // No transformation
    //
    // NOW:
    //   gl_Position = transform * vec4(aPosition, 1.0);  // Apply transformation!
    
    // Pass color to fragment shader (unchanged)
    vertexColor = aColor;
}

// ============================================================================
// HOW ROTATION WORKS
// ============================================================================
//
// FRAME 1: C# sends rotation matrix for 0° → vertices at original positions
// FRAME 2: C# sends rotation matrix for 1° → vertices rotate slightly
// FRAME 3: C# sends rotation matrix for 2° → vertices rotate more
// ...
// FRAME 360: C# sends rotation matrix for 360° → back to start!
//
// The triangle appears to SPIN smoothly! 🔄
//
// VISUAL:
//   Frame 1:       *         (0°)
//   Frame 60:     *          (60°)
//   Frame 120:   *           (120°)
//   Frame 180:  *            (180° - upside down!)
//   Frame 240:   *           (240°)
//   Frame 300:    *          (300°)
//   Frame 360:     *         (360° - full circle!)
//
// ============================================================================

// ============================================================================
// UNDERSTANDING MATRIX MULTIPLICATION
// ============================================================================
//
// SIMPLE EXPLANATION:
// Matrix multiplication transforms (changes) a position
//
// WITHOUT transformation:
//   Position (0.5, 0.0, 0.0) → stays (0.5, 0.0, 0.0)
//
// WITH rotation transformation:
//   Position (0.5, 0.0, 0.0) → becomes (0.35, 0.35, 0.0) (rotated 45°)
//
// The matrix "rotates" the point around the origin (0, 0, 0)!
//
// THE MATH (don't worry if confusing):
//   [x']   [m00 m01 m02 m03]   [x]
//   [y'] = [m10 m11 m12 m13] × [y]
//   [z']   [m20 m21 m22 m23]   [z]
//   [w']   [m30 m31 m32 m33]   [w]
//
// Just remember: "Matrix × Vector = Transformed Vector"
// ============================================================================

// ============================================================================
// WHY USE MATRICES?
// ============================================================================
//
// Matrices can combine MULTIPLE transformations:
// - Rotation + Translation + Scale = ONE matrix!
//
// This is MUCH faster than doing each separately
//
// Example: To rotate AND move AND scale:
//   result = scale × rotation × translation × position
// 
// But we can pre-calculate:
//   finalMatrix = scale × rotation × translation  (done once in C#)
//   result = finalMatrix × position  (done in shader - fast!)
//
// This is why 3D games can transform millions of vertices per second!
// ============================================================================


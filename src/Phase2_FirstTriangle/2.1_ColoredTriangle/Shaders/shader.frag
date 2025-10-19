#version 330 core

// ============================================================================
// FRAGMENT SHADER
// ============================================================================
// This shader runs for EVERY PIXEL inside your triangle!
// 
// The fragment shader's job is to:
// 1. Receive data from the vertex shader
// 2. Calculate the final color for this pixel
// 3. Output that color
// ============================================================================

// OUTPUT: The color of this pixel
// out vec4 means we're sending out a vec4
// FragColor is the name we chose (you can name it anything)
out vec4 FragColor;

// MAIN FUNCTION: Runs for each pixel
void main()
{
    // Set this pixel to orange
    // vec4 is (Red, Green, Blue, Alpha)
    // Values range from 0.0 to 1.0
    FragColor = vec4(1.0, 0.5, 0.2, 1.0);  // Orange color
    
    // TRY THESE:
    // vec4(1.0, 0.0, 0.0, 1.0)  // Pure red
    // vec4(0.0, 1.0, 0.0, 1.0)  // Pure green
    // vec4(0.0, 0.5, 1.0, 1.0)  // Light blue
}

// ============================================================================
// SHADER PIPELINE RECAP:
// ============================================================================
// C# Program sends vertices → Vertex Shader processes each vertex →
// → OpenGL creates triangle → Fragment Shader colors each pixel →
// → Final image on screen!
// ============================================================================


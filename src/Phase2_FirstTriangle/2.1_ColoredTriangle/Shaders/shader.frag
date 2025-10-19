#version 330 core

// ============================================================================
// FRAGMENT SHADER - SUPER DETAILED EXPLANATION
// ============================================================================
//
// WHAT IS A FRAGMENT SHADER?
// This shader runs ONCE for EVERY PIXEL inside your triangle!
// If your triangle covers 1000 pixels, this runs 1000 times!
// Again, it runs IN PARALLEL on the GPU - super fast!
//
// WHAT'S THE DIFFERENCE FROM VERTEX SHADER?
// - Vertex Shader: Runs per VERTEX (per corner point) - runs 3 times for a triangle
// - Fragment Shader: Runs per PIXEL (per screen dot) - runs MANY times!
//
// ANALOGY:
// Vertex shader = Placing the corners of a paper cutout
// Fragment shader = Coloring in every dot inside that cutout
//
// WHY "FRAGMENT" not "PIXEL"?
// Technically "fragment" is more accurate, but think of it as "pixel"
// ============================================================================

// ============================================================================
// OUTPUT VARIABLES (data going TO the screen)
// ============================================================================
// "out" means this variable SENDS data out of the shader
// "vec4" means 4 floats: (Red, Green, Blue, Alpha)
// "FragColor" is the name we chose (you can name it anything)
out vec4 FragColor;

// IMPORTANT: This is THE COLOR that will appear on screen!
// Whatever you set FragColor to becomes the pixel's color.

// ============================================================================
// MAIN FUNCTION (runs for each pixel!)
// ============================================================================
void main()
{
    // ========================================================================
    // SET THE PIXEL COLOR
    // ========================================================================
    // vec4(R, G, B, A) creates a color with 4 components:
    //   R = Red    (0.0 = no red,   1.0 = full red)
    //   G = Green  (0.0 = no green, 1.0 = full green)
    //   B = Blue   (0.0 = no blue,  1.0 = full blue)
    //   A = Alpha  (0.0 = invisible, 1.0 = fully visible)
    //
    // This is different from 0-255 range you might know!
    // OpenGL uses 0.0 to 1.0 (floating point)
    
    FragColor = vec4(1.0, 0.5, 0.2, 1.0);  // Orange color
    
    // LET'S BREAK DOWN THIS ORANGE:
    //   1.0 = Full red
    //   0.5 = Half green  
    //   0.2 = Little bit of blue
    //   1.0 = Fully opaque (not transparent)
    // Red + some green + tiny blue = Orange!
    
    // ========================================================================
    // TRY THESE COLORS (Replace the line above):
    // ========================================================================
    // FragColor = vec4(1.0, 0.0, 0.0, 1.0);  // Pure RED
    // FragColor = vec4(0.0, 1.0, 0.0, 1.0);  // Pure GREEN
    // FragColor = vec4(0.0, 0.0, 1.0, 1.0);  // Pure BLUE
    // FragColor = vec4(1.0, 1.0, 0.0, 1.0);  // YELLOW (red + green)
    // FragColor = vec4(1.0, 0.0, 1.0, 1.0);  // MAGENTA (red + blue)
    // FragColor = vec4(0.0, 1.0, 1.0, 1.0);  // CYAN (green + blue)
    // FragColor = vec4(1.0, 1.0, 1.0, 1.0);  // WHITE (all colors)
    // FragColor = vec4(0.0, 0.0, 0.0, 1.0);  // BLACK (no colors)
    // FragColor = vec4(0.5, 0.5, 0.5, 1.0);  // GRAY (half of everything)
}

// ============================================================================
// HOW THIS SHADER GETS CALLED
// ============================================================================
//
// After the vertex shader and rasterization:
//
// 1. OpenGL determines your triangle covers pixels at:
//    (400, 300), (401, 300), (402, 300), (400, 301), etc.
//    (These are screen coordinates in pixels)
//
// 2. For EACH of these pixels, OpenGL calls this shader
//    Call 1: → FragColor = orange → pixel (400, 300) becomes orange
//    Call 2: → FragColor = orange → pixel (401, 300) becomes orange
//    Call 3: → FragColor = orange → pixel (402, 300) becomes orange
//    ... thousands of times ...
//
// 3. All these orange pixels together form your orange triangle!
//
// ============================================================================
// THE BIG PICTURE PIPELINE
// ============================================================================
//
// C# Code (CPU):
//   "Here are 3 vertices with positions"
//      ↓
// Vertex Shader (GPU) runs 3 times:
//   Vertex 1: position → gl_Position
//   Vertex 2: position → gl_Position
//   Vertex 3: position → gl_Position
//      ↓
// OpenGL connects them into a triangle
//      ↓
// Rasterization: "Which pixels are inside?"
//      ↓
// Fragment Shader (GPU) runs once per pixel:
//   Pixel 1 → FragColor = orange
//   Pixel 2 → FragColor = orange
//   Pixel 3 → FragColor = orange
//   ...thousands of pixels...
//      ↓
// Final image on screen!
//
// ============================================================================


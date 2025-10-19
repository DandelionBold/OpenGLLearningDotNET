#version 330 core

// ============================================================================
// VERTEX SHADER - WITH COLOR SUPPORT!
// ============================================================================
//
// THIS IS NEW! We're now accepting TWO pieces of data per vertex:
// 1. Position (X, Y, Z)
// 2. Color (R, G, B)
//
// The vertex shader will:
// - Process the position (as before)
// - Pass the color to the fragment shader
//
// The GPU will automatically BLEND colors between vertices!
// This is called INTERPOLATION - it's MAGIC! ✨
// ============================================================================

// ============================================================================
// INPUT FROM C# (Per-Vertex Data)
// ============================================================================

// Position input (same as before)
// layout(location = 0) matches the first VertexAttribPointer call in C#
layout(location = 0) in vec3 aPosition;

// NEW! Color input (one color per vertex!)
// layout(location = 1) matches the second VertexAttribPointer call in C#
layout(location = 1) in vec3 aColor;

// IMPORTANT: Each VERTEX gets its own position AND color
// Vertex 1: position + red color
// Vertex 2: position + green color
// Vertex 3: position + blue color

// ============================================================================
// OUTPUT TO FRAGMENT SHADER
// ============================================================================

// NEW! We output color to the fragment shader
// "out" means this data goes TO the next stage
// The GPU will automatically interpolate this between vertices!
out vec3 vertexColor;

// HOW INTERPOLATION WORKS:
// If vertex 1 is RED and vertex 2 is GREEN,
// the pixels between them will be YELLOW-ISH (blend of red and green)
// The GPU does this AUTOMATICALLY for ALL "out" variables!

// ============================================================================
// MAIN FUNCTION
// ============================================================================
void main()
{
    // Set position (same as before)
    gl_Position = vec4(aPosition, 1.0);
    
    // NEW! Pass the color to the fragment shader
    // This color will be INTERPOLATED across the triangle
    vertexColor = aColor;
    
    // WHAT HAPPENS:
    // 1. This shader runs 3 times (once per vertex)
    // 2. Each run outputs a different color:
    //    - Vertex 1 → outputs RED
    //    - Vertex 2 → outputs GREEN
    //    - Vertex 3 → outputs BLUE
    // 3. GPU automatically blends these colors for pixels in between!
    // 4. Fragment shader receives the BLENDED color for each pixel
}

// ============================================================================
// VISUAL EXPLANATION OF INTERPOLATION
// ============================================================================
//
// Imagine our triangle with vertices colored:
//
//              RED
//               *
//              /🔴\
//             / 🟡 \      ← Middle pixels are RED+GREEN = YELLOW
//            /  🟢  \     ← Middle pixels are RED+BLUE+GREEN = MIXED
//           /________\
//        GREEN      BLUE
//
// The GPU fills in ALL the colors between the corners automatically!
// This is why graphics cards are so powerful - they do this SUPER FAST
// for MILLIONS of pixels!
//
// TECHNICAL TERM: This is called "INTERPOLATION" or "VARYING"
// The color "varies" across the triangle surface
// ============================================================================

// ============================================================================
// DATA FLOW
// ============================================================================
//
// C# sends per-vertex data:
//   Vertex 1: Position(0, 0.5, 0), Color(1, 0, 0) RED
//   Vertex 2: Position(-0.5, -0.5, 0), Color(0, 1, 0) GREEN
//   Vertex 3: Position(0.5, -0.5, 0), Color(0, 0, 1) BLUE
//      ↓
// This shader processes each vertex:
//   Run 1: aPosition=(0,0.5,0), aColor=(1,0,0) → vertexColor=RED
//   Run 2: aPosition=(-0.5,-0.5,0), aColor=(0,1,0) → vertexColor=GREEN
//   Run 3: aPosition=(0.5,-0.5,0), aColor=(0,0,1) → vertexColor=BLUE
//      ↓
// GPU interpolates for each pixel:
//   Pixel near top: mostly RED
//   Pixel in center: mix of RED+GREEN+BLUE
//   Pixel near bottom-left: mostly GREEN
//   Pixel near bottom-right: mostly BLUE
//      ↓
// Fragment shader receives interpolated color for each pixel
// ============================================================================


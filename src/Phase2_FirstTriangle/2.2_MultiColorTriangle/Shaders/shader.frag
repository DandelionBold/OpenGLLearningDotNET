#version 330 core

// ============================================================================
// FRAGMENT SHADER - USING INTERPOLATED COLORS!
// ============================================================================
//
// THIS IS NEW! Instead of setting a FIXED color for all pixels,
// we now receive a DIFFERENT color for EACH pixel!
//
// The color comes from the vertex shader, and the GPU has
// automatically BLENDED it based on the vertex colors!
//
// This is the MAGIC of the graphics pipeline! ✨
// ============================================================================

// ============================================================================
// INPUT FROM VERTEX SHADER (Interpolated!)
// ============================================================================

// NEW! We receive color from the vertex shader
// "in" means this data comes FROM the previous stage
// The name must match the "out" variable in the vertex shader
in vec3 vertexColor;

// IMPORTANT: This color is DIFFERENT for EVERY pixel!
// The GPU automatically calculated it by blending vertex colors
//
// Example for a triangle with RED, GREEN, BLUE vertices:
// - Pixel at top corner: vertexColor ≈ (1.0, 0.0, 0.0) = RED
// - Pixel in center: vertexColor ≈ (0.33, 0.33, 0.33) = GRAY-ISH
// - Pixel near green corner: vertexColor ≈ (0.0, 1.0, 0.0) = GREEN
//
// The GPU does this AUTOMATICALLY! No code needed!

// ============================================================================
// OUTPUT TO SCREEN
// ============================================================================

// Output color for this pixel
out vec4 FragColor;

// ============================================================================
// MAIN FUNCTION
// ============================================================================
void main()
{
    // NEW! Use the interpolated color from the vertex shader
    // Add alpha (opacity) = 1.0 for fully visible
    FragColor = vec4(vertexColor, 1.0);
    
    // THAT'S IT! So simple!
    // We just use the color that was automatically interpolated for us
    
    // WHAT'S HAPPENING:
    // 1. This shader runs once for EVERY pixel inside the triangle
    // 2. Each pixel gets a DIFFERENT vertexColor value
    // 3. The GPU calculated this by blending the vertex colors
    // 4. We output it, and the pixel becomes that color!
}

// ============================================================================
// COMPARISON WITH PROJECT 2.1
// ============================================================================
//
// PROJECT 2.1 (Solid Color):
//   FragColor = vec4(1.0, 0.5, 0.2, 1.0);  // Every pixel = orange
//
// PROJECT 2.2 (Gradient):
//   FragColor = vec4(vertexColor, 1.0);     // Every pixel = different!
//
// The difference is simple:
// - Before: Same color for all pixels (boring but simple)
// - Now: Different color per pixel (beautiful gradients!)
// ============================================================================

// ============================================================================
// UNDERSTANDING INTERPOLATION (VISUAL)
// ============================================================================
//
// Imagine you have 3 paint buckets at the triangle corners:
// - Top corner: RED paint
// - Bottom-left: GREEN paint  
// - Bottom-right: BLUE paint
//
// The GPU "pours" the paint and lets it mix naturally:
//
//              🪣 RED
//               *
//              /🔴\
//             / 🟡🟢\      ← Colors blend smoothly
//            /🔴🟡🟢🔵\
//           /__________\
//       🪣 GREEN   🪣 BLUE
//
// That's exactly what interpolation does!
// It smoothly blends values between the corners
// ============================================================================

// ============================================================================
// WHY THIS IS IMPORTANT
// ============================================================================
//
// Interpolation is FUNDAMENTAL to graphics:
//
// 1. TEXTURES (later): Texture coordinates are interpolated
//    → This is how images wrap onto 3D models!
//
// 2. LIGHTING (later): Light values are interpolated
//    → This is how smooth shading works!
//
// 3. NORMAL VECTORS (later): Surface normals are interpolated
//    → This is how curved surfaces look smooth!
//
// 4. EVERYTHING: Almost all visual effects use interpolation
//
// You've just learned one of the MOST IMPORTANT GPU features! 🎉
// ============================================================================

// ============================================================================
// EXPERIMENTS TO TRY
// ============================================================================
//
// 1. Try only using one color channel:
//    FragColor = vec4(vertexColor.r, 0.0, 0.0, 1.0);  // Only red channel
//
// 2. Try inverting colors:
//    FragColor = vec4(1.0 - vertexColor, 1.0);
//
// 3. Try making it black and white (grayscale):
//    float gray = (vertexColor.r + vertexColor.g + vertexColor.b) / 3.0;
//    FragColor = vec4(gray, gray, gray, 1.0);
//
// 4. Try multiplying the color:
//    FragColor = vec4(vertexColor * 2.0, 1.0);  // Brighter!
//
// Play around! Shaders are fun! 🎨
// ============================================================================


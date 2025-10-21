#version 330 core

// ============================================================================
// FRAGMENT SHADER - SPRITE ANIMATION (HEAVILY COMMENTED)
// ============================================================================
// PURPOSE: Runs once per PIXEL inside the triangle(s)
// - Receives animated UVs from vertex shader
// - Samples the sprite sheet texture at those UVs  
// - Writes the resulting color to the screen
//
// SPRITE SHEET CONCEPT:
//   The texture contains multiple animation frames arranged in a grid.
//   vTexCoord points to the correct frame based on the UV offset applied
//   in the vertex shader.
//
//   Example sprite sheet layout (4 frames per row):
//   ┌─────────────────────────────────┐
//   │ Frame 0 │ Frame 1 │ Frame 2 │   │  ← UVs: (0,0)-(0.25,1), (0.25,0)-(0.5,1), etc.
//   ├─────────┼─────────┼─────────┤   │
//   │ Frame 3 │ Frame 4 │ Frame 5 │   │  ← UVs: (0,0)-(0.25,0.5), (0.25,0)-(0.5,0.5), etc.
//   └─────────┴─────────┴─────────┘   │
//
// UV COORDINATE SYSTEM:
//   UV coordinates go from 0.0 to 1.0 across the entire texture:
//   - (0.0, 0.0) = bottom-left corner of texture
//   - (1.0, 1.0) = top-right corner of texture
//   - Each frame occupies a portion of this UV space
//
// COLOR BLENDING AND INTERPOLATION:
//   When we sample a texture, OpenGL automatically interpolates between
//   nearby pixels to create smooth colors. This is called "texture filtering".
//   For sprites, we use NEAREST filtering to get pixel-perfect, crisp edges.
//
//   Linear Filtering (smooth):     Nearest Filtering (crisp):
//   ┌─┬─┬─┐                       ┌─┬─┬─┐
//   │█│▒│░│  →  smooth gradient   │█│█│░│  →  sharp edges
//   └─┴─┴─┘                       └─┴─┴─┘
//
// ALPHA CHANNEL AND TRANSPARENCY:
//   Textures can have an alpha channel (RGBA) for transparency.
//   Alpha values: 1.0 = fully opaque, 0.0 = fully transparent
//   We can discard transparent pixels to create sprites with transparent backgrounds.
// ============================================================================

// INPUTS FROM VERTEX SHADER
in vec2 vTexCoord;        // animated UV coordinates from vertex shader

// OUTPUT TO FRAMEBUFFER  
out vec4 FragColor;       // final color output (RGBA)

// UNIFORMS FROM C# CODE
uniform sampler2D uTexture0;  // sprite sheet texture

void main()
{
    // ========================================================================
    // STEP 1: SAMPLE THE SPRITE SHEET TEXTURE
    // ========================================================================
    // The texture() function samples the sprite sheet at the given UV coordinates.
    // This will show the current frame of the animation based on the UV offset
    // calculated in the vertex shader.
    //
    // Think of it like this:
    // - The sprite sheet is like a comic book page with multiple panels
    // - vTexCoord tells us which panel (frame) to look at
    // - texture() reads the color from that specific panel
    FragColor = texture(uTexture0, vTexCoord);

    // ========================================================================
    // STEP 2: OPTIONAL TRANSPARENCY SUPPORT
    // ========================================================================
    // Many sprite sheets have transparent backgrounds (alpha = 0).
    // We can discard these pixels to create sprites that don't show
    // the background color.
    //
    // Uncomment the next line to enable transparency:
    // if (FragColor.a < 0.1) discard;
    
    // ========================================================================
    // STEP 3: OPTIONAL DEBUG VISUALIZATION
    // ========================================================================
    // For debugging, we can visualize UV coordinates as colors:
    // - Red channel = U coordinate (horizontal position in texture)
    // - Green channel = V coordinate (vertical position in texture)
    // - Blue = 0, Alpha = 1
    //
    // Uncomment the next line to see UV coordinates as colors:
    // FragColor = vec4(vTexCoord, 0.0, 1.0);
    
    // ========================================================================
    // STEP 4: OPTIONAL COLOR MODIFICATIONS
    // ========================================================================
    // We can modify colors for effects:
    // - Tinting: multiply by a color
    // - Brightness: multiply all channels
    // - Contrast: adjust color ranges
    //
    // Examples (uncomment to try):
    // FragColor = FragColor * vec4(1.0, 0.5, 0.5, 1.0);  // Red tint
    // FragColor = FragColor * 1.5;                        // Brighten
    // FragColor = pow(FragColor, vec4(1.2));             // Increase contrast
}
#version 330 core

// ============================================================================
// FRAGMENT SHADER - SPRITE ANIMATION (BEGINNER-FRIENDLY COMMENTS)
// ============================================================================
// PURPOSE:
//   Runs once per PIXEL inside the triangle(s).
//   - Receives animated UVs from vertex shader
//   - Samples the sprite sheet texture at those UVs
//   - Writes the resulting color to the screen
//
// INPUTS (from vertex shader):
//   in vec2 vTexCoord  -> animated UV coordinates (base + offset)
//
// UNIFORMS (set from C# code):
//   uniform sampler2D uTexture0  -> sprite sheet texture bound to GL_TEXTURE0
//
// OUTPUTS:
//   out vec4 FragColor -> final pixel color written to the framebuffer
//
// SPRITE SHEET SAMPLING:
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
// ============================================================================

in vec2 vTexCoord;        // animated UV coordinates from vertex shader
out vec4 FragColor;       // final color output

uniform sampler2D uTexture0;  // sprite sheet texture

void main()
{
    // Sample the sprite sheet texture at the animated UV coordinates
    // This will show the current frame of the animation
    FragColor = texture(uTexture0, vTexCoord);

    // Optional: Add transparency support
    // If the texture has alpha channel, discard transparent pixels
    // if (FragColor.a < 0.1) discard;

    // Optional: Debug UV visualization
    // FragColor = vec4(vTexCoord, 0.0, 1.0);  // shows UV coordinates as colors
}

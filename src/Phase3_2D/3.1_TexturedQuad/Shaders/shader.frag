#version 330 core

// ============================================================================
// FRAGMENT SHADER - TEXTURED QUAD (BEGINNER-FRIENDLY COMMENTS)
// ============================================================================
// PURPOSE:
//   Runs once per PIXEL inside the triangle(s).
//   - Receives interpolated UVs (vTexCoord)
//   - Samples the texture (uTexture0) using those UVs
//   - Writes the resulting color to the screen (FragColor)
//
// INPUTS (from vertex shader):
//   in vec2 vTexCoord  -> interpolated UV per pixel
//
// UNIFORMS (set from C# code):
//   uniform sampler2D uTexture0  -> texture bound to GL_TEXTURE0
//
// OUTPUTS:
//   out vec4 FragColor -> final pixel color written to the framebuffer
// ============================================================================

in vec2 vTexCoord;        // interpolated UV from vertex shader
out vec4 FragColor;       // final color output

uniform sampler2D uTexture0;  // our image

void main()
{
    // Sample the texture at coordinates vTexCoord
    // UV coordinates:
    //   - u (x): 0 at left, 1 at right
    //   - v (y): 0 at bottom, 1 at top (with our current image/UV setup)
    FragColor = texture(uTexture0, vTexCoord);

    // If you want to debug UVs, try visualizing them:
    // FragColor = vec4(vTexCoord, 0.0, 1.0);  // shows a gradient based on UVs
}

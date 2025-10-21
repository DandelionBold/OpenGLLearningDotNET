#version 330 core

// ============================================================================
// VERTEX SHADER - SPRITE ANIMATION (BEGINNER-FRIENDLY COMMENTS)
// ============================================================================
// PURPOSE:
//   Runs once per VERTEX.
//   - Receives position and base UV coordinates from CPU
//   - Applies UV offset to show current animation frame
//   - Applies 2D orthographic projection
//   - Forwards animated UVs to the fragment shader
//
// INPUTS (from VBO via glVertexAttribPointer in C#):
//   layout(location=0) vec3 aPosition  -> vertex position (x,y,z)
//   layout(location=1) vec2 aTexCoord  -> base UV coordinates for frame 0
//
// OUTPUTS (to fragment shader):
//   out vec2 vTexCoord  -> animated UV coordinates (base + offset)
//
// UNIFORMS (set from C#):
//   uniform mat4 uProjection  -> orthographic projection matrix for 2D
//   uniform vec2 uUVOffset    -> UV offset for current animation frame
//
// SPRITE ANIMATION TECHNIQUE:
//   Base UVs represent frame 0 of the sprite sheet.
//   We add uUVOffset to shift to the current frame.
//   Example: if frameWidth = 0.25 and currentFrame = 2:
//     uUVOffset = (0.5, 0.0)  // 2 * 0.25 = 0.5
//     Final UV = baseUV + (0.5, 0.0)
// ============================================================================

layout(location = 0) in vec3 aPosition;   // vertex position
layout(location = 1) in vec2 aTexCoord;   // base UV coordinates (frame 0)

out vec2 vTexCoord;                       // animated UV coordinates

uniform mat4 uProjection;                 // 2D orthographic projection matrix
uniform vec2 uUVOffset;                   // UV offset for current animation frame

void main()
{
    // Add UV offset to show current animation frame
    // This shifts the UV coordinates to the correct frame in the sprite sheet
    vTexCoord = aTexCoord + uUVOffset;

    // Convert the vertex position to clip space using projection
    // gl_Position is REQUIRED; it determines where the vertex appears on screen
    gl_Position = uProjection * vec4(aPosition, 1.0);
}

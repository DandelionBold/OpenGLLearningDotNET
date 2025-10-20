#version 330 core

// ============================================================================
// VERTEX SHADER - TEXTURED QUAD (BEGINNER-FRIENDLY COMMENTS)
// ============================================================================
// PURPOSE:
//   Runs once per VERTEX.
//   - Receives position and texture coordinates (UVs) from CPU
//   - Applies 2D orthographic projection (uProjection)
//   - Forwards UVs to the fragment shader
//
// INPUTS (from VBO via glVertexAttribPointer in C#):
//   layout(location=0) vec3 aPosition  -> vertex position (x,y,z)
//   layout(location=1) vec2 aTexCoord  -> vertex UV coordinates (u,v)
//
// OUTPUTS (to fragment shader):
//   out vec2 vTexCoord  -> interpolated UV for each pixel
//
// UNIFORMS (set from C#):
//   uniform mat4 uProjection  -> orthographic projection matrix for 2D
// ============================================================================

layout(location = 0) in vec3 aPosition;   // vertex position
layout(location = 1) in vec2 aTexCoord;   // vertex UV (0..1 range)

out vec2 vTexCoord;                       // passed to fragment shader

uniform mat4 uProjection;                 // 2D orthographic projection matrix

void main()
{
    // Forward the UVs so the fragment shader can sample the texture
    vTexCoord = aTexCoord;

    // Convert the vertex position to clip space using projection
    // gl_Position is REQUIRED; it determines where the vertex appears on screen
    gl_Position = uProjection * vec4(aPosition, 1.0);
}

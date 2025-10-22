#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 uProjection;
uniform mat4 uModel;
uniform vec2 uUVOffset;   // per-sprite frame offset in UV space
uniform vec2 uUVScale;    // per-sprite frame size in UV space
uniform int uFlipX;       // 0 = normal, 1 = horizontally flipped

out vec2 vTexCoord;

void main()
{
    gl_Position = uProjection * uModel * vec4(aPos, 1.0);

    // Base quad uses [0,0]..[1,1] UVs. Apply flip and scale/offset to select a frame.
    float u = aTexCoord.x;
    if (uFlipX == 1)
    {
        u = 1.0 - u;
    }
    vec2 frameUV = vec2(u, aTexCoord.y) * uUVScale + uUVOffset;
    vTexCoord = frameUV;
}



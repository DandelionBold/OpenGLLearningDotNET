#version 330 core

layout(location=0) in vec3 aPosition;
layout(location=1) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform mat4 uProjection;
uniform mat4 uModel; // scales X by texture aspect

void main(){
    vTexCoord = aTexCoord;
    gl_Position = uProjection * uModel * vec4(aPosition,1.0);
}



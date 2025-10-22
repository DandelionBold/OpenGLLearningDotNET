#version 330 core

in vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture0;

void main()
{
    vec4 color = texture(uTexture0, vTexCoord);
    if (color.a < 0.1)
        discard;
    FragColor = color;
}



#version 330 core

layout (location = 0) in vec2 aPosition;

uniform float uProgress; // 1.0 = lleno, 0.0 = vacío

void main()
{
    vec2 pos = aPosition;
    pos.x = -1.0 + (pos.x + 1.0) * uProgress;

    gl_Position = vec4(pos, 0.0, 1.0);
}

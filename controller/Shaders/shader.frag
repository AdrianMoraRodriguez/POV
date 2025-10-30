#version 330 core 

out vec4 OutFragColor;

uniform vec3 diffuse_color;

void main()
{
    OutFragColor=vec4(diffuse_color,1.0f);
}

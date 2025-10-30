#version 330 core

layout(location=0) in vec3 aPosition;
layout(location=1) in float aWeight;

uniform mat4 view;
uniform mat4 model;
uniform mat4 projection;



void main(){
    vec4 position=vec4(aPosition,1.0f) * model * view * projection;
    gl_Position=position;
}
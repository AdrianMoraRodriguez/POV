#version 330 core

layout(location=0) in vec3 aPosition;
layout(location=1) in vec4 aColor;

uniform mat4 view;
uniform mat4 model;
uniform mat4 projection;

out vec4 VertexColor;

void main() {
  vec4 position = vec4(aPosition, 1.0f) * model * view * projection;
  VertexColor = aColor;
  gl_Position = position;
}

#version 330 core 
in vec4 VertexColor;
out vec4 OutFragColor;
void main()
{
  OutFragColor = VertexColor;
}
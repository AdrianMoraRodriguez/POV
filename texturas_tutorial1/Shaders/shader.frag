#version 450 


layout(location=0) in vec2 TexCoord;
layout(location=1) in vec4 VertexNormal; 

layout(location=0) out vec4 OutFragColor;

uniform sampler2D uTexture;

uniform int bTex;
uniform vec3 diffuse_color;
uniform vec3 AmbientLight;
uniform vec3 DirLight0Diffuse;
uniform vec3 DirLight0Direction;

void main()
{
    vec4 tSample=texture(uTexture,TexCoord);

    float cl=max(dot(DirLight0Direction,VertexNormal.xyz),0);
   
    vec3 color= bTex==1 ? tSample.rgb : diffuse_color;
    vec4 newcolor=vec4(AmbientLight*color+cl*DirLight0Diffuse.rgb*color,1.0);
    OutFragColor=newcolor;
}   



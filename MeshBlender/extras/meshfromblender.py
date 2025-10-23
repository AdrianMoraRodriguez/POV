import bpy
import os
import json

sobjects = bpy.context.selected_objects

object = sobjects[0]

object_data = object.data

vertices = object_data.vertices

default_material={}
default_material["name"]="default"
default_material["diffuse_color"]=[1.0,1.0,1.0,1.0]



indexdata = {}
slots=[]

mesh_data = {}

mesh_data["materials"]=[default_material]

material_slots=object.material_slots
for ms in material_slots:
    indexdata[ms.material.name]=[]
    slots.append(ms.material.name)
    mat={}
    mat["name"]=ms.material.name
    mat["diffuse_color"]=[v for v in ms.material.diffuse_color]
    mesh_data["materials"].append(mat)



mesh_data["nvertex"] = len(vertices)
operated_vertices = []
for vert in vertices:
    coord = vert.co
    operated_vertices.append(coord[0])
    operated_vertices.append(coord[2])
    operated_vertices.append(-coord[1])

mesh_data["vertexdata"] = operated_vertices
mesh_data["weightdata"] = [1.0] * len(vertices)

polygons = object_data.polygons


mesh_data["nindex"] = [0] * (len(mesh_data["materials"])-1)
mesh_data["indexdata"]=[ [] for _ in mesh_data["materials"]]
mesh_data["indexdata"].pop()

for polygon in polygons:
    verts = polygon.vertices
    slot=polygon.material_index
    mesh_data["indexdata"][slot].append(verts[0])
    mesh_data["indexdata"][slot].append(verts[1])
    mesh_data["indexdata"][slot].append(verts[2])
    mesh_data["nindex"][slot]+=3
   


dir_path = os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
# new_path=os.path.dirname(os.path.join(dir_path,'..\..'))

new_path = os.path.join(dir_path, "mesh.json")

print(new_path)

with open(new_path, "w") as file:
    json.dump(mesh_data, file)

print("Finalizada la escritura de los datos")
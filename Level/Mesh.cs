using OpenTK.Mathematics;
using Optional;
using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;

public class Mesh {

    // Información de colisión
    public class CollisionMeshInfo {
        public string type {get; set; }="unknown";
        public float[] location {get; set; }= {0.0f,0.0f,0.0f};
        public float[] scale {get;  set; } = {1.0f,1.0f,1.0f};
        public float[] rotation {get;  set; } = {0.0f,0.0f,0.0f};
    }

    private RetrievedCollision _collisionData  = new RetrievedCollision();
    public CollisionMeshInfo collisionData {get; private set; }= new CollisionMeshInfo();

    // Datos del mesh
    public float[] vertexData {get; private set; } = new float[0];
    public int[] indexData {get; private set;} = new int[0];

    public int [] slotData {get; private set;} = new int[0];

    public RetrievedMaterial[] matData {get; private set;} = new RetrievedMaterial[0];

    private RetrievedMesh _retrievedMesh = new RetrievedMesh();

    public int vertexBuffer {get; set;} // VBO
    public int indexBuffer {get; set;} // EBO

    public int vertexArray {get; set;} // VAO

    
    public Mesh(){}

    public Mesh(RetrievedMesh retMesh): this() {
        _retrievedMesh=retMesh;
    }
     

 public void Make() 
 {

        var mesh=_retrievedMesh;

        // Cargar datos de colisión
        collisionData.type=_retrievedMesh.collision.type;
        _retrievedMesh.collision.location.CopyTo(collisionData.location,0);
        _retrievedMesh.collision.scale.CopyTo(collisionData.scale,0);
        _retrievedMesh.collision.rotation.CopyTo(collisionData.rotation,0);

        matData=new RetrievedMaterial[mesh.materials.Length-1];
        for(int i=1;i<mesh.materials.Length;i++){
            matData[i-1]=mesh.materials[i]; // The first one is a default
        }

        if(mesh.vertexdata is null || mesh.weightdata is null || mesh.indexdata is null)
            throw new Exception("Error: mesh data is wrong or empty");
        int nvertices=mesh.vertexdata.Length;
        int nweight=mesh.weightdata.Length;

        if((nvertices/3) != nweight)
            throw new Exception("Number of vertex weights is different of number of vertices");

        vertexData=new float[nvertices+nweight];
        int nvalues = 4; // 3 components per vers, 1 per weight
        for(int i=0,j=0,k=0;i<mesh.vertexdata.Length;i=i+3,j=j+1,k=k+nvalues){
            vertexData[k]=mesh.vertexdata[i];
            vertexData[k+1]=mesh.vertexdata[i+1];
            vertexData[k+2]=mesh.vertexdata[i+2];
            vertexData[k+3]=mesh.weightdata[j];
        }
        slotData=new int[mesh.materials.Length-1];
        
        int nindex=0;
        for(int i=0;i<(mesh.materials.Length-1);i++)
        {
            slotData[i]=nindex;
            nindex+=mesh.indexdata[i].Length;
        
        }

        indexData=new int[nindex];
        int count=0;
        for(int i=0;i<(mesh.materials.Length-1);i++){
            for(int j=0;j<mesh.indexdata[i].Length;j++)
                indexData[count++]=mesh.indexdata[i][j];
        }

    }

    public void Draw(Shader _shader, Option<Vector3> dcolor) // El shader es para cambiar el uniform (diffuse_color) y el Option es un tipo de dato que puede o no tener valor
    {
        Vector3 vcolor; 
        if (indexData is not null && slotData is not null)
        {
            for (int i = 0; i < slotData.Length; i++)
            {
                vcolor = dcolor.ValueOr(new Vector3(  // ValueOr devuelve el valor si existe, o el que se pasa como parámetro si no existe
                    matData[i].diffuse_color[0],
                    matData[i].diffuse_color[1],
                    matData[i].diffuse_color[2]));
               _shader.SetVector3("diffuse_color", vcolor);
                int nelements = 0;
                if (i == (slotData.Length - 1))
                    nelements = indexData.Length - slotData[i];
                else
                    nelements = slotData[i + 1] - slotData[i];
                GL.DrawElements(PrimitiveType.Triangles, nelements, DrawElementsType.UnsignedInt, ref indexData[slotData[i]]);
            }
        }
    }

}

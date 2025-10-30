public class Mesh {

    public float[] vertexData {get; private set; } = new float[0];
    public int[] indexData {get; private set;} = new int[0];

    public int [] slotData {get; private set;} = new int[0];

    public RetrievedMaterial[] matData {get; private set;} = new RetrievedMaterial[0];

    private RetrievedMesh _retrievedMesh = new RetrievedMesh();

    public int vertexBuffer {get; set;} // VBO
    public int indexBuffer {get; set;} // EBO

    public int vertexArray {get; set;} // VAO

    
    public Mesh(){
        


    }

    public Mesh(RetrievedMesh retMesh)
    : this() {

        _retrievedMesh=retMesh;

    }
     

 public void Make(){
        

        var mesh=_retrievedMesh;


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

}

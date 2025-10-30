namespace POV{
public class RetrievedMaterial {
    public string name {get; init;} = "Unknown";
    public float[] diffuse_color {get; init;} = {1.0f,1.0f,1.0f,1.0f};
}


public class RetrievedMesh {

    public RetrievedMaterial[] materials {get; init;} = new RetrievedMaterial[0];
    public int nvertex {get; set;}

    public float[] vertexdata {get; init;} = new float[0];
    public float[]  weightdata {get; init;}= new float[0];
    public int[] nindex {get; init;}=new int[0];

    public int[][] indexdata {get; init;}= new int[0][];

}
}
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Actor 
{

    public bool Enabled {get; set;}
    public string StaticMeshId {get; set;}
    public Matrix4 Model=new Matrix4();

    public Actor() 
    {
        this.Enabled = false;
        this.StaticMeshId = "";
    }

    public void SetTransform(Vector3 positionVector, Vector3 axisVector, float angle, Vector3 scale)
    {
        Model = Matrix4.CreateScale(scale) * Matrix4.CreateFromAxisAngle(axisVector,angle) * Matrix4.CreateTranslation(positionVector);
    }




  


}
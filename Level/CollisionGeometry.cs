using OpenTK.Mathematics;

public class CollisionGeometry {

    public Vector3 u0 {get; set;}
    public Vector3 u1 {get; set;}
    public Vector3 u2 {get; set;}
    public Vector3 dimensions {get; set;}
    public Vector3 center {get; set;}

    public CollisionGeometry()
    {
        u0=new Vector3(1.0f,0.0f,0.0f);
        u1=new Vector3(0.0f,1.0f,0.0f);
        u2=new Vector3(0.0f,0.0f,1.0f);
        dimensions=new Vector3(1.0f,1.0f,1.0f);
        center=new Vector3(0.0f,0.0f,0.0f);
    }

    public CollisionGeometry(Vector3 center, Vector3 dim)
    {
        u0=new Vector3(1.0f,0.0f,0.0f);
        u1=new Vector3(0.0f,1.0f,0.0f);
        u2=new Vector3(0.0f,0.0f,1.0f);
        dimensions=new Vector3(dim);
        center=new Vector3(center);
    }

    public void Transform(Matrix4 Model)
    {
        Vector3 Scale = Model.ExtractScale();
        Console.WriteLine($"Scale {Scale}");
        dimensions=Vector3.Multiply(dimensions,Scale);
        Console.WriteLine($"Dimensions {dimensions}");
        Matrix4 Rotation = Model.ClearScale().ClearTranslation(); 
        u0=(new Vector4(u0)*Rotation).Xyz;
        u1=(new Vector4(u1)*Rotation).Xyz;
        u2=(new Vector4(u2)*Rotation).Xyz;
        Vector3 Translation = Model.ExtractTranslation();
        center=center+Translation;
    }

    public override string ToString()
    {
        return String.Format("Center:{0}\nDimensions:{1}\nu0:{2}\nu1:{3}\nu2:{4}",center, dimensions, u0, u1,u2);
    }
}

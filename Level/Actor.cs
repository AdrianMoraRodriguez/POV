using Optional;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Actor 
{

    public bool Enabled {get; set;}
    public string StaticMeshId {get; set;}
    public Matrix4 Model = new Matrix4();

    public string CollisionMeshId {get; set;}
    public Matrix4 StartCollisionModel=new Matrix4();
    public Option<CollisionGeometry> CollisionGeometry=Option.None<CollisionGeometry>();


    public Actor() 
    {
        this.Enabled = false;
        this.StaticMeshId = "";
        this.CollisionMeshId = "";
    }

    public void SetTransform(Vector3 positionVector, Vector3 axisVector, float angle, Vector3 scale)
    {
        Model = Matrix4.CreateScale(scale) * Matrix4.CreateFromAxisAngle(axisVector,angle) * Matrix4.CreateTranslation(positionVector);
    }

    public void SaveModel()
    {
        _backModel = Model;
    }

    public void Scale(Vector3 scale)
    {
        Model = Matrix4.CreateScale(scale) * Model;
    }

    public void RestoreModel()
    {
        Model = _backModel;
    }

    public void SetCollisionGeometry(Dictionary<string,Mesh> AssetCollection)
    {
        if(!AssetCollection.ContainsKey(CollisionMeshId))
            return;
        Mesh CollisionMesh = AssetCollection[CollisionMeshId];
        Vector3 defaultCenter=new Vector3(0.0f,0.0f,0.0f);
        Vector3 defaultDimensions=new Vector3(1.0f,1.0f,1.0f);
        // Initialize
        switch(CollisionMesh.collisionData.type)
        {
            case "nocollision":
                CollisionGeometry=Option.None<CollisionGeometry>();
                break;
            case "box":
                CollisionGeometry=Option.Some<CollisionGeometry>(new CollisionBox(defaultCenter,defaultDimensions));
                break;
            case "ellipsoid":
                CollisionGeometry=Option.Some<CollisionGeometry>(new CollisionEllipsoid(defaultCenter,defaultDimensions));
                break;
            default:
                CollisionGeometry=Option.Some(new CollisionGeometry(defaultCenter,defaultDimensions));
                break;        

        }
        if(CollisionGeometry.HasValue)
        {
            float[] location=CollisionMesh.collisionData.location;
            float[] rotation=CollisionMesh.collisionData.rotation;
            float[] scale=CollisionMesh.collisionData.scale;
            Matrix4 ModelScale=Matrix4.CreateScale(scale[0],scale[1], scale[2]);
            // Rotation> Its imported in Euler system Pitch, Yaw, Roll
            Matrix4 ModelRotYaw=Matrix4.CreateRotationY(rotation[1]);
            Vector4 uX=Vector4.UnitX*ModelRotYaw;
            Matrix4 ModelRotYawPitch=ModelRotYaw*Matrix4.CreateFromAxisAngle(uX.Xyz,rotation[0]);
            Vector4 uZ=Vector4.UnitZ*ModelRotYawPitch;
            Matrix4 ModelRotation=ModelRotYawPitch*Matrix4.CreateFromAxisAngle(uZ.Xyz,rotation[2]);
            Matrix4 ModelTranslation=Matrix4.CreateTranslation(location[0],location[1],location[2]);
            // Recalculate CollisionGeometry Parameters
            StartCollisionModel=ModelScale*ModelRotation*ModelTranslation;
            CollisionGeometry.ValueOrFailure("Unexpected empty CollisionGeometry").Transform(StartCollisionModel);
        }
    }

    public void UpdateCollisionModel()
    {
        CollisionGeometry.ValueOrFailure("Unexpected empty CollisionGeometry").Transform(StartCollisionModel*Model);
    }


    private Matrix4 _backModel = new Matrix4();
}
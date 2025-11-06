using OpenTK.Mathematics;

class CollisionBox : CollisionGeometry {

    public CollisionBox() : base() {}
    public CollisionBox(Vector3 center, Vector3 dim) : base(center,dim) {}

    public Vector3[] GetCorners()
    {
        Vector3[] corners = new Vector3[8];
        Vector3 dim=dimensions;
        corners[0]=new Vector3(center.X+dim.X/2.0f,center.Y+dim.Y/2.0f,center.X+dim.Z/2.0f);
        corners[1]=new Vector3(center.X+dim.X/2.0f,center.Y-dim.Y/2.0f,center.X+dim.Z/2.0f);
        corners[2]=new Vector3(center.X-dim.X/2.0f,center.Y+dim.Y/2.0f,center.X+dim.Z/2.0f);
        corners[3]=new Vector3(center.X-dim.X/2.0f,center.Y-dim.Y/2.0f,center.X+dim.Z/2.0f);
        corners[4]=new Vector3(center.X+dim.X/2.0f,center.Y+dim.Y/2.0f,center.X-dim.Z/2.0f);
        corners[5]=new Vector3(center.X+dim.X/2.0f,center.Y-dim.Y/2.0f,center.X-dim.Z/2.0f);
        corners[6]=new Vector3(center.X-dim.X/2.0f,center.Y+dim.Y/2.0f,center.X-dim.Z/2.0f);
        corners[7]=new Vector3(center.X-dim.X/2.0f,center.Y-dim.Y/2.0f,center.X-dim.Z/2.0f);
        return corners;
    }
}

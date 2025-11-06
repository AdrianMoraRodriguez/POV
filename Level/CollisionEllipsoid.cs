using OpenTK.Mathematics;
using System;

class CollisionEllipsoid : CollisionGeometry {

    public Matrix3 M {get; private set;}
    public Matrix3 V {get; private set;}

    public CollisionEllipsoid() : base() {
        CalculateVM();
    }

    public CollisionEllipsoid(Vector3 center, Vector3 dim ) : base(center,dim) {
        CalculateVM();
    }

    private void CalculateVM()
    {
        //Rotation Matrix Transposed
        Matrix3 Vt=new Matrix3(u0,u1,u2);
        float d0 = 1.0f / dimensions.X;
        float d1 = 1.0f / dimensions.Y;
        float d2 = 1.0f / dimensions.Z;
        Matrix3 D2 = new Matrix3(d0 * d0, 0.0f, 0.0f, 0.0f, d1 * d1, 0.0f, 0.0f, 0.0f, d2 * d2);
        V = Matrix3.Transpose(Vt);
        M = V * D2 * Vt;
    }

    public bool EllipsoidContainsVertex(Vector3 p)
    {
        Vector3 pmk = p - center;
        //Console.WriteLine($"{center} {dimensions} {p}");
        return (Vector3.Dot(pmk * M, pmk) <= 1);
    }
}

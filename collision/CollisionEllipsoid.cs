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
        Vector3 u0 = parameters.uX;
        Vector3 u1 = parameters.uY;
        Vector3 u2 = parameters.uZ;
        Vector3 dimensions = parameters.dimensions;
        Matrix3 Vt=new Matrix3(u0,u1,u2);
        
        float d0=1.0f/dimensions.X;
        float d1=1.0f/dimensions.Y;
        float d2=1.0f/dimensions.Z;
        Matrix3 D2 = new Matrix3(d0*d0,0.0f,0.0f,0.0f,d1*d1,0.0f,0.0f,0.0f,d2*d2);
        V=Matrix3.Transpose(Vt);
        M= V*D2*Vt;
        


    }

    public bool EllipsoidContainsVertex(Vector3 p)
    {
        Vector3 pmk=p - parameters.center;
	CalculateVM();
        return (Vector3.Dot(pmk*M,pmk)<=1);

    }


    public bool EllipsoidCrossEdge(Vector3 p0, Vector3 p1)
    {
	    // This works if p1 and p2 are outside the ellipsoid
	
	    Vector3 p10=p1-p0;
	    Vector3 p0k=p0-parameters.center;
	    float q0=Vector3.Dot(p0k*M,p0k)-1;
	    float q1=2*Vector3.Dot(p0k*M,p10);
	    float q2=Vector3.Dot(p10*M,p10);
	    if((q1*q1-4*q0*q2)>=0)
	    {
		    float s=(float)Math.Sqrt(q1*q1-4*q2*q0);
		    float t0=(-q1-s)/(2*q2);
		    float t1=(-q1+s)/(2*q2);
		    // overlap with [0,1]
		    if(t1<0 || t0>1)
			    return false;
		    else
			    return true;
		    
	    }
	    else
		    return false;



    }
}

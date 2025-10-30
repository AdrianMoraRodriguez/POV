using System;
using System.Text.Json;
using LearnOpenTK.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using POV;


public class Window : GameWindow
{

    public float[] vertexData={

			// POS 	// COLOR
             -0.5f, -0.5f, -0.5f, 1.0f,  0.0f, 0.0f,1.0f, //0 
             -0.5f, -0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, //1
             -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f,1.0f, //2
             -0.5f, 0.5f,  0.5f, 1.0f, 0.0f, 1.0f, 1.0f, //3
             0.5f, -0.5f, -0.5f, 1.0f,  0.0f, 0.0f,1.0f, //4
             0.5f, -0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, //5
             0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f,1.0f, //6
             0.5f, 0.5f,  0.5f, 1.0f, 0.0f, 1.0f, 1.0f //7

    };

    public int[] indexData={
        6,4,0, // CCK
        0,2,6, //
        7,1,5, //
        1,7,3, //
        3,2,0, //
        0,1,3, //
        4,6,7, // 
        7,5,4, //
        0,4,5, //
        5,1,0, //
        7,6,2, //
        2,3,7 //
    };


    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : base(gameWindowSettings,nativeWindowSettings)
    {
        _camera=new Camera(Vector3.UnitZ*3,Size.X / (float)Size.Y); 
        _Model= new Matrix4();
        _RotAngle=0.0f;
        MonitorInfo minfo = Monitors.GetMonitorFromWindow(this);
        _horizontalResolution=minfo.HorizontalResolution;
        _verticalResolution=minfo.VerticalResolution;
        
        _controller=new Controller(_horizontalResolution,_verticalResolution);

    }


protected override void OnLoad()
 {
    base.OnLoad();
    // Leer archivo json
    string ?text=null;
    text=File.ReadAllText("mesh.json");
    if(text is null)
            throw new Exception("Error finding or reading file");
    else
      _retrievedMesh =  JsonSerializer.Deserialize<RetrievedMesh>(text,_jsonOptions); 
        
    if(_retrievedMesh is null)
           throw new Exception("Error retrieving mesh");

    _mesh=new Mesh(_retrievedMesh);
    _mesh.Make();

    vertexData=_mesh.vertexData;
    indexData=_mesh.indexData;

	GL.ClearColor(0.2f,0.2f,0.2f,1.0f); // Color de borrado
    GL.Enable(EnableCap.CullFace);  // Elimina las caras traseras 
    GL.Enable(EnableCap.DepthTest);  

    // Buffers
    _vertexBuffer=GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer,_vertexBuffer);
    GL.BufferData(BufferTarget.ArrayBuffer,vertexData.Length*sizeof(float),vertexData,BufferUsageHint.StaticDraw);

    _indexBuffer=GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer,_indexBuffer);
    GL.BufferData(BufferTarget.ElementArrayBuffer,indexData.Length*sizeof(int),indexData,BufferUsageHint.StaticDraw);

    _shader=new Shader("Shaders/shader.vert","Shaders/shader.frag");
    _shader.Use();

    // Paso13. Creamos un VertexArray que es una ubicaci'on para VAOs.
    _vertexArray=GL.GenVertexArray();
    GL.BindVertexArray(_vertexArray);

    // Paso 14. Creamos el VAO para el atributo aPosition del shader
    var posLocation = _shader.GetAttribLocation("aPosition");
    GL.EnableVertexAttribArray(posLocation);
    GL.VertexAttribPointer(posLocation,3,VertexAttribPointerType.Float,false,4*sizeof(float),0); 

    // Paso 15. Creamos el VAO para el atributo aWeight del shader
    var colorLocation = _shader.GetAttribLocation("aWeight");
    GL.EnableVertexAttribArray(colorLocation);
    GL.VertexAttribPointer(colorLocation,1,VertexAttribPointerType.Float,false,4*sizeof(float),3*sizeof(float));


}

 protected override void OnUpdateFrame(FrameEventArgs e)
{
    base.OnUpdateFrame(e);

    // Controller Update
    _controller.UpdateState(this.KeyboardState,this.MouseState,e);
    // Update GameState
    UpdateGameState((float)e.Time);

	Matrix4.CreateFromAxisAngle(_Axis,_RotAngle,out _Model); 
    _RotAngle+=_RotSpeed*(float)e.Time;
    if(_RotAngle>=MathHelper.TwoPi)
          _RotAngle=0;

}
	
	
protected override void OnRenderFrame(FrameEventArgs args)
{
    // Sin _shader or _mesh no podemos hacer nada
    if(_shader==null || _mesh==null )
    {
        return;
    }
    base.OnRenderFrame(args);

    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
 
// Paso18. Hacemos bind al contenedor de VAOs.
    GL.BindVertexArray(_vertexArray);

// Paso19. Actualizamos los uniforms. Esto es una transferencia entre la CPU y la GPU. Son las matrices
// de las transformaciones geom'etricas
    
    _shader.SetMatrix4("model",_Model);
    _shader.SetMatrix4("view",_camera.GetViewMatrix());
    _shader.SetMatrix4("projection",_camera.GetProjectionMatrix());

// Paso 20. Lanzamos la orden Draw
if (_mesh.indexData is not null && _mesh.slotData is not null){
            for(int i=0;i<_mesh.slotData.Length;i++)
            {
            RetrievedMaterial mat = _mesh.matData[i];
            float[] color = mat.diffuse_color;
            Vector3 vcolor = new Vector3(color[0], color[1], color[2]);
            _shader.SetVector3("diffuse_color",vcolor);   
		int nelements=0;
                if(i==(_mesh.slotData.Length-1))
                    nelements=_mesh.indexData.Length-_mesh.slotData[i];
                else
                    nelements=_mesh.slotData[i+1]-_mesh.slotData[i];
                GL.DrawElements(PrimitiveType.Triangles,nelements,DrawElementsType.UnsignedInt, ref _mesh.indexData[_mesh.slotData[i]]);

            }
}


// Paso 21. Hacemos el swap del doble buffer.
SwapBuffers();


}
	
protected override void OnUnload()
{
	
        GL.BindBuffer(BufferTarget.ArrayBuffer,0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer,0);
        GL.BindVertexArray(0);

        base.OnUnload();
}

protected void UpdateGameState(float deltaTime)
{
    // Camera
    Vector3 movement = _controller.GetMovement();
    _camera.Position += _camera.Front * movement.X;
    _camera.Position += _camera.Right * movement.Y;
    _camera.Position += _camera.Up * movement.Z;
    // Arm
    Angles2D deltaAngles = _controller.GetArmOrientation();
    Angles2D cameraAngles = new Angles2D(_camera.Yaw, _camera.Pitch);
    cameraAngles += deltaAngles;
    _camera.Yaw = (float)cameraAngles.Yaw; // Se recalculan los vectores de la cámara en la clase Camera (función UpdateVectors)
    _camera.Pitch = (float)cameraAngles.Pitch;
}


    private int _vertexBuffer;
    private int _vertexArray;
    private int _indexBuffer;
	private Shader? _shader ;
    
	private Camera _camera;

    
    
    private Matrix4 _Model;

    private readonly Vector3 _Axis=new Vector3(1.0f,1.0f,1.0f);

    private readonly float _RotSpeed=1.0f;

    private float _RotAngle;


    private RetrievedMesh? _retrievedMesh;
    private readonly JsonSerializerOptions _jsonOptions= new();

    private Mesh ?_mesh;

    private Controller _controller;
    private int _horizontalResolution;
    private int _verticalResolution;


}

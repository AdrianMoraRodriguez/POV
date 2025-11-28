using System;
using System.Text.Json;
using LearnOpenTK.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Optional;
using Optional.Unsafe;


public class Window : GameWindow
{
int frames = 60;
float prevAngle = 0.0f;
public RetrievedMaterial[] ?matData;

public Dictionary<string,Mesh> AssetCollection {get; set;}

public string levelFilePath {get; set;}

private Level _level=new Level();

private bool bDrawCollision=false;



    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : base(gameWindowSettings,nativeWindowSettings)
    {
                levelFilePath="assets/level.json";
        AssetCollection=new Dictionary<string,Mesh>();

        // Monitor and resolution
        MonitorInfo minfo = Monitors.GetMonitorFromWindow(this);
        _horizontalResolution=minfo.HorizontalResolution;
        _verticalResolution=minfo.VerticalResolution;
	//_camera=new Camera(Vector3.UnitZ*3,Size.X / (float)Size.Y); 
	Console.WriteLine($"Hor {_horizontalResolution} Vert {_verticalResolution}");
	_camera=new Camera(Vector3.UnitZ*3,_horizontalResolution / (float)_verticalResolution); 


        _controller=new Controller(_horizontalResolution,_verticalResolution);
        _controller.Speed=2.0f;

               

    }

protected void UpdateGameState(float deltaTime){
    
    // Camera
    Vector3 movement=_controller.GetMovement();
   
    //_camera.Position += _camera.Front*movement.X;
    //_camera.Position += _camera.Right*movement.Y;
    //_camera.Position += _camera.Up*movement.Z;
        
    // Arm
    Angles2D deltaAngles = _controller.GetArmOrientation();
    Angles2D cameraAngles = new Angles2D(_camera.Yaw, _camera.Pitch);
    cameraAngles += deltaAngles;
    _camera.Yaw = (float)cameraAngles.Yaw; // Se recalculan los vectores de la cámara en la clase Camera (función UpdateVectors)
    _camera.Pitch = (float)cameraAngles.Pitch;

    // Angles2D deltaAngles=_controller.GetArmOrientation();
    // Angles2D cameraAngles = new Angles2D(_camera.Yaw,_camera.Pitch);
    // cameraAngles += deltaAngles;
    // _camera.Yaw = (float)cameraAngles.Yaw;
    // _camera.Pitch = (float)cameraAngles.Pitch;

    // Buscamos el pawn
    Actor? pawn;
    if( _level.ActorCollection.TryGetValue("apawn", out pawn))
    {
        Vector3 forward = _camera.Front;
        forward.Y = 0.0f; // No quiero que el avance tenga componente vertical
        forward = Vector3.Normalize(forward);
        Matrix4 pawnModel = pawn.Model; // Cojo matriz del actor principal
        Vector3 translation = forward * movement.X + _camera.Right * movement.Y + _camera.Up * movement.Z;
        // Versión antigua sin tener en cuenta la cámara: Vector3 translation = (movement.Y, movement.Z, -movement.X); // movement.X es avance hacia adelante, movement.Y es desplazamiento lateral y movement.Z es desplazamiento vertical
        pawn.Model = pawn.Model * Matrix4.CreateTranslation(translation);
        //Versión antigua poner que la cámara gire alrededor del actor
        //_camera.Position= new Vector3(_camera.Position.X, pawnNewPosition.Y + _controller.CameraDistance, pawnNewPosition.Z + 2 * _controller.CameraDistance);
        //_camera.Position += new Vector3(translation.X, 0.0f, translation.Z);
        // pawnNewPosition - _camera.Front

        //pawn.CollisionModel = pawn.CollisionModel * Matrix4.CreateTranslation(translation);

        // Check for collisions.
        //pawn.CollisionGeometry.ValueOrFailure("Pawn without collision geometry").Transform(pawn.Model);
        pawn.UpdateCollisionModel();

        foreach(string actorid in _level.ActorCollection.Keys){
            if(actorid=="apawn")
                continue;
            Actor actor=_level.ActorCollection[actorid];
            if( !actor.Enabled)
                continue;
            if (Collision.CheckEB(pawn, actor))
                {
		// Restore previous
		    pawn.RestoreModel();
		    pawn.UpdateCollisionModel();
		    translation=new Vector3(0.0f,0.0f,0.0f);


                }
        }
        //Vector3 cameraForward = _camera.Front;
        float yaw = (float)(_camera.Yaw * Math.PI / 180.0f + Math.PI / 2); // Añado PI para que mire hacia el actor;
        Quaternion pawnRotation = pawn.Model.ExtractRotation();
        Vector3 euler = pawnRotation.ToEulerAngles();
        //Console.WriteLine($"Pawn euler before: {euler} Yaw: {yaw}");
        //float angle = (float)Math.Atan2(euler.X, euler.Z);
        //Console.WriteLine($"euler: {euler} Yaw: {yaw}");
        //Console.WriteLine($"Pawn euler: {euler} Camera Forward: {cameraForward} ");
        //Console.WriteLine($"Angle: {angle} PrevAngle: {prevAngle}");
        //Console.WriteLine($"Dot: {dot} Angle: {angle} euler: {euler} cameraForward: {cameraForward}");
        //Vector3 cross = Vector3.Cross(forward, cameraForward);
        if (Math.Abs(yaw - euler.Y) > 1e-6)
        {
            float angleDifference = -(float)((yaw - euler.Y) - Math.PI); // Resto PI para que mire hacia el actor
            pawn.Model = pawn.Model * Matrix4.CreateRotationY(angleDifference);
        }
        Console.WriteLine($"Pawn euler after: {pawn.Model.ExtractRotation().ToEulerAngles()} Yaw: {yaw}");
        Vector3 pawnNewPosition = pawn.Model.ExtractTranslation();
        Vector3 BehindOffset = forward * _controller.CameraDistance;

        _camera.Position = pawnNewPosition - BehindOffset + new Vector3(0.0f, _controller.CameraDistance / 2, 0.0f); // Le sumo la mitad de la distancia en Y para que esté un poco más alto
        //Console.WriteLine($"Pawn Pos: {pawnNewPosition}");
        pawn.UpdateCollisionModel();

    } else
    {
    
    _camera.Position += _camera.Front*movement.X;
    _camera.Position += _camera.Right*movement.Y;
    _camera.Position += _camera.Up*movement.Z;

    //TODO: First person collision
    
    
    }

    
}

protected void InitializeLevel()
{
        _level=new Level(levelFilePath);
        _level.LoadLevel(AssetCollection);
}


protected override void OnLoad()
 {
    base.OnLoad();
    InitializeLevel();

	GL.ClearColor(0.2f,0.2f,0.2f,1.0f); // Color de borrado
    GL.Enable(EnableCap.CullFace);  // Elimina las caras traseras 
    GL.Enable(EnableCap.DepthTest);  

    _shader=new Shader("Shaders/shader.vert","Shaders/shader.frag");
    _shader.Use();

    List<string> activeMeshes = _level.GetActiveMeshes(AssetCollection);

foreach(string meshid in activeMeshes){
    Console.WriteLine($"Active mesh{meshid}");
    if(AssetCollection[meshid] is null )
               throw new Exception("Mesh with empty data"); 
    if(AssetCollection[meshid].vertexData is null )
               throw new Exception("Mesh with empty data"); 
           
    int _vertexBuffer=GL.GenBuffer();
    AssetCollection[meshid].vertexBuffer=_vertexBuffer;
    GL.BindBuffer(BufferTarget.ArrayBuffer,_vertexBuffer);
    GL.BufferData(BufferTarget.ArrayBuffer,
        AssetCollection[meshid].vertexData.Length*sizeof(float),
        AssetCollection[meshid].vertexData,
        BufferUsageHint.StaticDraw);
        
    int _vertexArray=GL.GenVertexArray();
    AssetCollection[meshid].vertexArray=_vertexArray;
    GL.BindVertexArray(_vertexArray);

     int _indexBuffer=GL.GenBuffer();
    AssetCollection[meshid].indexBuffer=_indexBuffer;
    GL.BindBuffer(BufferTarget.ElementArrayBuffer,_indexBuffer);
    GL.BufferData(BufferTarget.ElementArrayBuffer,
    AssetCollection[meshid].indexData.Length*sizeof(int),
    AssetCollection[meshid].indexData,BufferUsageHint.StaticDraw);

    // Paso 14. Creamos el VAO para el atributo aPosition del shader
    var posLocation = _shader.GetAttribLocation("aPosition");
    GL.EnableVertexAttribArray(posLocation);
    GL.VertexAttribPointer(posLocation,3,VertexAttribPointerType.Float,false,4*sizeof(float),0); 

    // Paso 15. Creamos el VAO para el atributo aWeight del shader
    var colorLocation = _shader.GetAttribLocation("aWeight");
    GL.EnableVertexAttribArray(colorLocation);
    GL.VertexAttribPointer(colorLocation,1,VertexAttribPointerType.Float,false,4*sizeof(float),3*sizeof(float));

    // Unbind VBO, EBO and VAO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer,0);
            GL.BindBuffer(BufferTarget.ArrayBuffer,0);
            GL.BindVertexArray(0);

        }


}

float time=0.0f;
 protected override void OnUpdateFrame(FrameEventArgs e)
{
   time+=(float)e.Time;
   
    base.OnUpdateFrame(e);
    if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            // If it is, close the window.
            Close();
        }
    if (KeyboardState.IsKeyDown(Keys.C) && time>0.5f)
    {
        bDrawCollision=!bDrawCollision;
	time=0.0f;
    }
	//Matrix4.CreateFromAxisAngle(_Axis,_RotAngle,out _Model); 
    //_RotAngle+=_RotSpeed*(float)e.Time;
    //if(_RotAngle>=MathHelper.TwoPi)
    // Controller Update
     _controller.UpdateState(this.KeyboardState,this.MouseState,e);

    // Update GameState
    UpdateGameState((float)e.Time);


}
	
	
protected override void OnRenderFrame(FrameEventArgs args)
{
    // Sin _shader or _mesh no podemos hacer nada
    if(_shader==null)
    {
        return;
    }
    base.OnRenderFrame(args);
    GL.Enable(EnableCap.DepthTest);  
    GL.Enable(EnableCap.StencilTest);  

    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit );

List<string> activeMeshes = _level.GetActiveMeshes(AssetCollection);

    foreach(string actorid in _level.ActorCollection.Keys){
        Actor actor=_level.ActorCollection[actorid];
        if( !actor.Enabled)
            continue;
        
        //Collisions
        Mesh ?mesh;
        if(bDrawCollision)
        {
            if(! AssetCollection.ContainsKey(actor.CollisionMeshId))
                continue;
            else{
                mesh=AssetCollection[actor.CollisionMeshId];
            }
        }
        else
        {
            if(! AssetCollection.ContainsKey(actor.StaticMeshId))
                continue;
            else
                mesh=AssetCollection[actor.StaticMeshId];

        }

        if(mesh is null)
            throw new Exception("Trying to render an actor without mesh");
        
        // Binding mesh VAO
        GL.BindVertexArray(mesh.vertexArray);
            Matrix4 model;
            if (bDrawCollision)
            {
                model = actor.CollisionModel;
            }
            else
            {
                model = actor.Model;
            }

                _shader.SetMatrix4("model", model);
        _shader.SetMatrix4("view",_camera.GetViewMatrix());
        _shader.SetMatrix4("projection",_camera.GetProjectionMatrix());

        // Paso 20. Lanzamos la orden Draw
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);  
        GL.StencilFunc(StencilFunction.Always,1,0xFF);
        GL.StencilMask(0xFF);
 
        mesh.Draw(_shader,Option.None<Vector3>());
    
        GL.StencilFunc(StencilFunction.Notequal,1,0xFF);
        GL.StencilMask(0x00);
    
        actor.SaveModel();
            //actor.Scale(new Vector3(1.02f,1.02f,1.02f));
            //    actor.UpdateCollisionModel();
            model = Matrix4.CreateScale(1.02f, 1.02f, 1.02f) * model;

        _shader.SetMatrix4("model",model);

        mesh.Draw(_shader,Option.Some(new Vector3(0.0f,0.0f,0.0f)));
        //GL.StencilFunc(StencilFunction.Always,1,0xFF);
        //GL.StencilMask(0xFF);
        //actor.RestoreModel();
        //    actor.UpdateCollisionModel();


        GL.BindVertexArray(0);

  } // Loop sobre los actores
        GL.StencilMask(0xFF);
    
// Paso 21. Hacemos el swap del doble buffer.
SwapBuffers();
// if (frames == 0) // BORRAR
// {
//     Close();
// } else
// {
//     frames--;
// }

}

protected override void OnResize(ResizeEventArgs e)
{
    base.OnResize(e);
    GL.Viewport(0,0,Size.X,Size.Y);
}
	
 protected override void OnUnload()
{
	
        GL.BindBuffer(BufferTarget.ArrayBuffer,0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer,0);
        GL.BindVertexArray(0);

        base.OnUnload();
}
private Shader? _shader ;
    
	private Camera _camera;

    
    
    private Controller _controller;
    private int _horizontalResolution;
    private int _verticalResolution;





}

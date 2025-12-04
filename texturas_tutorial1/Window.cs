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
public RetrievedMaterial[] ?matData;

public Dictionary<string,Asset> AssetCollection {get; set;}

public string levelFilePath {get; set;}

private Level _level=new Level();

private bool bDrawCollision=false;



    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : base(gameWindowSettings,nativeWindowSettings)
    {
        levelFilePath="assets/level.json";
        AssetCollection=new Dictionary<string,Asset>();

        // Monitor and resolution
        MonitorInfo minfo = Monitors.GetMonitorFromWindow(this);
        _horizontalResolution=minfo.HorizontalResolution;
        _verticalResolution=minfo.VerticalResolution;
	//_camera=new Camera(Vector3.UnitZ*3,Size.X / (float)Size.Y); 
	Console.WriteLine($"Hor {_horizontalResolution} Vert {_verticalResolution}");
	_camera=new Camera(Vector3.UnitZ*3,_horizontalResolution / (float)_verticalResolution); 
	_camera.Fov=45.0f;


        _controller=new Controller(_horizontalResolution,_verticalResolution);
        _controller.Speed=2.0f;

               

    }

protected void UpdateGameState(float deltaTime){
    
    // Camera
    float sensibility = 0.1f;
    Vector3 movement=_controller.GetMovement();
   
    //_camera.Position += _camera.Front*movement.X;
    //_camera.Position += _camera.Right*movement.Y;
    //_camera.Position += _camera.Up*movement.Z;

    Angles2D deltaAngles = _controller.GetArmOrientation();
    deltaAngles.Yaw *= sensibility;
    deltaAngles.Pitch *= sensibility;
    Angles2D cameraAngles = new Angles2D(_camera.Yaw, _camera.Pitch);
    cameraAngles += deltaAngles;
    _camera.Yaw = (float)cameraAngles.Yaw;
    _camera.Pitch = (float)cameraAngles.Pitch;

    // Pawn
    float speed = _controller.Speed;

    // Buscamos el pawn
    Actor? pawn;
    if( _level.ActorCollection.TryGetValue("apawn", out pawn))
    {
        Vector3 forward = _camera.Front;
        Vector3 scale = pawn.Model.ExtractScale();
        forward.Y = 0.0f; // No quiero que el avance tenga componente vertical
        forward = Vector3.Normalize(forward);
        Vector3 translation = forward * movement.X + _camera.Right * movement.Y + _camera.Up * movement.Z;
        translation *= speed;
        // Versión antigua sin tener en cuenta la cámara: Vector3 translation = (movement.Y, movement.Z, -movement.X); // movement.X es avance hacia adelante, movement.Y es desplazamiento lateral y movement.Z es desplazamiento vertical
        //pawn.Model = pawn.Model * Matrix4.CreateTranslation(translation);
        if (translation.X != 0.0f)
        {
            float targetYaw = MathF.Atan2(-forward.X, -forward.Z);
            pawn.Model = Matrix4.CreateScale(scale) * Matrix4.CreateRotationY(targetYaw) * Matrix4.CreateTranslation(pawn.Model.ExtractTranslation());
        }   
        pawn.Model = pawn.Model * Matrix4.CreateTranslation(translation);

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
            Console.WriteLine($"Chocaste con: {actorid}");
		    pawn.RestoreModel();
		    pawn.UpdateCollisionModel();
		    translation=new Vector3(0.0f,0.0f,0.0f);


                }
        }
 

        

        Vector3 pawnNewPosition = pawn.Model.ExtractTranslation();
        Vector3 behindOffset = _camera.Front * _controller.CameraDistance;
        behindOffset.Y = 0.0f;
        //Console.WriteLine($"Pawn Pos: {pawnNewPosition} ");
        _camera.Position = pawnNewPosition - behindOffset + new Vector3(0.0f, _controller.CameraDistance / 2, 0.0f);
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

    Mesh mesh= (Mesh) AssetCollection[meshid];
    if(mesh is null )
               throw new Exception("Mesh with empty data"); 
    if(mesh.vertexData is null )
               throw new Exception("Mesh with empty data"); 
           
    int _vertexBuffer=GL.GenBuffer();
    mesh.vertexBuffer=_vertexBuffer;
    GL.BindBuffer(BufferTarget.ArrayBuffer,_vertexBuffer);
    GL.BufferData(BufferTarget.ArrayBuffer,
        mesh.vertexData.Length*sizeof(float),
        mesh.vertexData,
        BufferUsageHint.StaticDraw);
        
    int _vertexArray=GL.GenVertexArray();
    mesh.vertexArray=_vertexArray;
    GL.BindVertexArray(_vertexArray);

     int _indexBuffer=GL.GenBuffer();
    mesh.indexBuffer=_indexBuffer;
    GL.BindBuffer(BufferTarget.ElementArrayBuffer,_indexBuffer);
    GL.BufferData(BufferTarget.ElementArrayBuffer,
    mesh.indexData.Length*sizeof(int),
    mesh.indexData,BufferUsageHint.StaticDraw);

    //
    // Paso 14. Creamos el VAO para el atributo aPosition del shader
    var posLocation = _shader.GetAttribLocation("aPosition");
    if(posLocation!=(-1))
    {
    GL.EnableVertexAttribArray(posLocation);
    GL.VertexAttribPointer(posLocation,3,VertexAttribPointerType.Float,false,9*sizeof(float),0);
    }

    // Paso 15. Creamos el VAO para el atributo aWeight del shader
    var weightLocation = _shader.GetAttribLocation("aWeight");
    if(weightLocation!=(-1))
    {
    GL.EnableVertexAttribArray(weightLocation);
    GL.VertexAttribPointer(weightLocation,1,VertexAttribPointerType.Float,false,9*sizeof(float),3*sizeof(float));
    }

    var uvLocation = _shader.GetAttribLocation("aTexCoord");
    if(uvLocation!=(-1))
    {

    GL.EnableVertexAttribArray(uvLocation);
    GL.VertexAttribPointer(uvLocation,2,VertexAttribPointerType.Float,false,9*sizeof(float),4*sizeof(float));
    }

    var normalLocation = _shader.GetAttribLocation("aNormal");
    if(normalLocation!=(-1))
    {
    GL.EnableVertexAttribArray(normalLocation);
    GL.VertexAttribPointer(normalLocation,3,VertexAttribPointerType.Float,false,9*sizeof(float),6*sizeof(float));
    }

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
                mesh=(Mesh) AssetCollection[actor.CollisionMeshId];
            }
        }
        else
        {
            if(! AssetCollection.ContainsKey(actor.StaticMeshId))
                continue;
            else
                mesh=(Mesh) AssetCollection[actor.StaticMeshId];

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

       _shader.SetMatrix4("normalTransformMatrix",actor.NormalTransform);
       _shader.SetVector3("AmbientLight",new Vector3(0.1f,0.1f,0.1f));
       _shader.SetVector3("DirLight0Diffuse",new Vector3(0.6f,0.6f,0.6f));
       _shader.SetVector3("DirLight0Direction",Vector3.Normalize(new Vector3(1.0f,1.0f,1.0f)));


        // Paso 20. Lanzamos la orden Draw
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);  
        GL.StencilFunc(StencilFunction.Always,1,0xFF);
        GL.StencilMask(0xFF);
 
        mesh.Draw(_shader,Option.None<Vector3>(),AssetCollection);
    
        GL.StencilFunc(StencilFunction.Notequal,1,0xFF);
        GL.StencilMask(0x00);
    
        actor.SaveModel();
            //actor.Scale(new Vector3(1.02f,1.02f,1.02f));
            //    actor.UpdateCollisionModel();
            model = Matrix4.CreateScale(1.02f, 1.02f, 1.02f) * model;

        _shader.SetMatrix4("model",model);

        mesh.Draw(_shader,Option.Some(new Vector3(0.0f,0.0f,0.0f)),AssetCollection);
        //GL.StencilFunc(StencilFunction.Always,1,0xFF);
        //GL.StencilMask(0xFF);
        //actor.RestoreModel();
        //    actor.UpdateCollisionModel();


        GL.BindVertexArray(0);

  } // Loop sobre los actores
        GL.StencilMask(0xFF);
    
// Paso 21. Hacemos el swap del doble buffer.
SwapBuffers();


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

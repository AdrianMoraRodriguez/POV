using System;
using System.Text.Json;
using LearnOpenTK.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Optional;


public class Window : GameWindow
{

    public RetrievedMaterial[] ?matData;
    public Dictionary<string,Mesh> AssetCollection {get; set;}
    public string levelFilePath {get; set;}

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    : base(gameWindowSettings,nativeWindowSettings)
    {
        _camera=new Camera(Vector3.UnitZ*3,Size.X / (float)Size.Y); 
        MonitorInfo minfo = Monitors.GetMonitorFromWindow(this);
        _horizontalResolution=minfo.HorizontalResolution;
        _verticalResolution=minfo.VerticalResolution;
        _controller=new Controller(_horizontalResolution,_verticalResolution);
        levelFilePath="assets/level.json";
        AssetCollection=new Dictionary<string,Mesh>();

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

        List<string> activeMeshes = _level.GetActiveMeshes();

        foreach(string meshid in activeMeshes)
        {

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

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            // If it is, close the window.
            Close();
        }
        // Controller Update
        _controller.UpdateState(this.KeyboardState,this.MouseState,e);
        // Update GameState
        UpdateGameState((float)e.Time);
        
    }
	
	
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        // Sin _shader no podemos hacer nada
        if(_shader==null)
        {
            return;
        }
        base.OnRenderFrame(args);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.StencilTest);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit| ClearBufferMask.StencilBufferBit);

        List<string> activeMeshes = _level.GetActiveMeshes();
        foreach(string actorid in _level.ActorCollection.Keys){
            Actor actor=_level.ActorCollection[actorid];
            if( !actor.Enabled)
                continue;
            Mesh? mesh= AssetCollection[actor.StaticMeshId];
            if(mesh is null)
                throw new Exception("Trying to render an actor without mesh");
            // Binding mesh VAO
            GL.BindVertexArray(mesh.vertexArray);
            _shader.SetMatrix4("model",actor.Model);
            _shader.SetMatrix4("view",_camera.GetViewMatrix());
            _shader.SetMatrix4("projection",_camera.GetProjectionMatrix());

            // Configurar Stencil
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace); // Se especifica como se va a modificar el buffer de stencil. El primer argumento es si falla el test de profundidad, el segundo si falla el test de stencil y el tercero si ninguno falla
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF); // Determina el test que se realiza.
            GL.StencilMask(0xFF); // FF significa 8 bits a 1, es decir, que todos los bits se pueden escribir, el 0x00 significa que no se puede escribir ninguno
            // Paso 20. Lanzamos la orden Draw
            mesh.Draw(_shader, Option.None<Vector3>());
            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF); // Solo se renderizan fragmentos que no coincidan con los valores que se han puesto a 1 (objetos renderizados)
            GL.StencilMask(0x00); // No se escribe nada en el buffer de stencil
            
            actor.SaveModel();
            actor.Scale(new Vector3(1.02f,1.02f,1.02f));
            _shader.SetMatrix4("model",actor.Model);
            mesh.Draw(_shader, Option.Some(new Vector3(0.0f,0.0f,0.0f)));
            actor.RestoreModel();
            
            GL.BindVertexArray(0);
        }
        GL.StencilMask(0xFF); // Se vuelve a permitir la escritura en el buffer de stencil
        // Paso 21. Hacemos el swap del doble buffer.
        SwapBuffers();
    }
	
    protected override void OnUnload()
    {

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);

            base.OnUnload();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0,0,Size.X,Size.Y);
    }

    protected void UpdateGameState(float deltaTime)
    {
        // Camera
        Vector3 movement = _controller.GetMovement();
        
        // Arm
        Angles2D deltaAngles = _controller.GetArmOrientation();
        Angles2D cameraAngles = new Angles2D(_camera.Yaw, _camera.Pitch); // Lo voy a tener que modificar
        cameraAngles += deltaAngles;
        _camera.Yaw = (float)cameraAngles.Yaw; // Se recalculan los vectores de la cámara en la clase Camera (función UpdateVectors)
        _camera.Pitch = (float)cameraAngles.Pitch;
        Actor pawn;
        if( _level.ActorCollection.TryGetValue("apawn", out pawn)) // Se busca al actor principal
        {
            Vector3 forward = _camera.Front;
            forward.Y = 0.0f; // No quiero que el avance tenga componente vertical
            forward = Vector3.Normalize(forward);
            Matrix4 pawnModel = pawn.Model; // Cojo matriz del actor principal
            Vector3 translation = forward * movement.X + _camera.Right * movement.Y + _camera.Up * movement.Z;
            // Versión antigua sin tener en cuenta la cámara: Vector3 translation = (movement.Y, movement.Z, -movement.X); // movement.X es avance hacia adelante, movement.Y es desplazamiento lateral y movement.Z es desplazamiento vertical
            pawn.Model = pawn.Model * Matrix4.CreateTranslation(translation);
            Vector3 pawnNewPosition = pawn.Model.ExtractTranslation();
            //Versión antigua poner que la cámara gire alrededor del actor
            //_camera.Position= new Vector3(_camera.Position.X, pawnNewPosition.Y + _controller.CameraDistance, pawnNewPosition.Z + 2 * _controller.CameraDistance);
            //_camera.Position += new Vector3(translation.X, 0.0f, translation.Z);
            // pawnNewPosition - _camera.Front
            Vector3 BehindOffset = forward * _controller.CameraDistance;
            _camera.Position = pawnNewPosition - BehindOffset + new Vector3(0.0f, _controller.CameraDistance / 2, 0.0f); // Le sumo la mitad de la distancia en Y para que esté un poco más alto
        } else {
            _camera.Position += _camera.Front * movement.X;
            _camera.Position += _camera.Right * movement.Y;
            _camera.Position += _camera.Up * movement.Z;
        }
    }


	private Shader? _shader ;
	private Camera _camera;

    private Controller _controller;
    private int _horizontalResolution;
    private int _verticalResolution;

    private Level _level=new Level();

}

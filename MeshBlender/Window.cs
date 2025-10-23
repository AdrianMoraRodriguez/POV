using System;
using System.Text.Json;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using LearnOpenTK.Common;
using POV;


public class Window : GameWindow
{
  public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings): base(gameWindowSettings,nativeWindowSettings)
  {
    _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y); 
    _Model = new Matrix4();
    _RotAngle = 0.0f;
  }

  public float[] vertexData = {
		// POS 	            // COLOR
    -0.5f, -0.5f, -0.5f, 1.0f,  0.0f, 0.0f,1.0f, //0 
    -0.5f, -0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, //1
    -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f,1.0f, //2
    -0.5f, 0.5f,  0.5f, 1.0f, 0.0f, 1.0f, 1.0f, //3
    0.5f, -0.5f, -0.5f, 1.0f,  0.0f, 0.0f,1.0f, //4
    0.5f, -0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, //5
    0.5f, 0.5f, -0.5f, 0.0f, 0.0f, 1.0f,1.0f, //6
    0.5f, 0.5f,  0.5f, 1.0f, 0.0f, 1.0f, 1.0f //7
  };

  public int[] indexData = {
    6,4,0, //
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

  protected override void OnLoad()
  {
    base.OnLoad();

    // Carga del mesh desde el archivo JSON
    string? text = null;
    text = File.ReadAllText("extras/mesh.json"); // Si da error es esto, quita extras/ y pon el mesh.json en la carpeta base
        if(text is null)
            throw new Exception("Error finding or reading file");
        else
            _retrievedMesh =  JsonSerializer.Deserialize<RetrievedMesh>(text,_jsonOptions); 
        
        if(_retrievedMesh is null)
            throw new Exception("Error retrieving mesh");

    // Creación del mesh
    _mesh=new Mesh(_retrievedMesh);
        _mesh.Make();

        vertexData=_mesh.vertexData;
        indexData=_mesh.indexData;

    // Configuración inicial de OpenGL
    GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
    GL.Enable(EnableCap.CullFace);
    GL.Enable(EnableCap.DepthTest);

    // Configuración de buffers
    _vertexBuffer = GL.GenBuffer(); // Un GenBuffer genera un nuevo buffer y devuelve su ID
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
    GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw); // Transfiere el miembro vertexData al buffer de vértices con una memoria estática
    _indexBuffer = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
    GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(int), indexData, BufferUsageHint.StaticDraw); // Transfiere el miembro indexData al buffer de índices con una memoria estática
    
    // Compilación y enlace de shaders
    _shader = new Shader("Shaders/shader.vert","Shaders/shader.frag");
    _shader.Use();

    // Configuración del Vertex Array Object (VAO)
    _vertexArray = GL.GenVertexArray(); // Un Vertex array lo que contiene es la información de cómo interpretar los datos de los buffers
    GL.BindVertexArray(_vertexArray);
    var posLocation = _shader.GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(posLocation);
        GL.VertexAttribPointer(posLocation,3,VertexAttribPointerType.Float,false,4*sizeof(float),0);
        var colorLocation = _shader.GetAttribLocation("aWeight");
        GL.EnableVertexAttribArray(colorLocation);
        GL.VertexAttribPointer(colorLocation,1,VertexAttribPointerType.Float,false,1*sizeof(float),3*sizeof(float));
  }


  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    Matrix4.CreateFromAxisAngle(_Axis,_RotAngle,out _Model); // Crea matriz de rotación a partir de un eje y un ángulo (eje, ángulo, matriz dónde guardar el resultado)
    _RotAngle += _RotSpeed * (float)e.Time; // Se actualiza el ángulo de rotación
    if (_RotAngle >= MathHelper.TwoPi) {
      _RotAngle = 0;
    }
  }

  protected override void OnRenderFrame(FrameEventArgs args)
  {
    base.OnRenderFrame(args);

    // Limpieza de pantalla y enlace del VAO
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    GL.BindVertexArray(_vertexArray); // Se enlaza el VAO

    // Se configuran los uniforms
    _shader.SetMatrix4("model", _Model); // Se le asigna al uniform "model" lo que contiene la matriz _Model
    _shader.SetMatrix4("view", _camera.GetViewMatrix());
    _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

    // Dibujo de la figura
    if (_mesh.indexData is not null && _mesh.slotData is not null){
            for(int i=0;i<_mesh.slotData.Length;i++)
            {   
                int nelements=0;
                if(i==(_mesh.slotData.Length-1))
                    nelements=_mesh.indexData.Length-_mesh.slotData[i];
                else
                    nelements=_mesh.slotData[i+1]-_mesh.slotData[i];
                GL.DrawElements(PrimitiveType.Triangles,nelements,DrawElementsType.UnsignedInt, ref _mesh.indexData[_mesh.slotData[i]]);

            }
        }
    SwapBuffers(); /* El buffer en el que se ha dibujado pasa a ser el que se muestra en pantalla y el otro queda libre para dibujar en él en el siguiente frame 
                      (siempre tenemos más de un buffer (cuantos más buffers más tiempo para dibujar sin que se congele la imagen, pero hay más retraso entre la acción 
                                                                                                                                              y lo que se ve en pantalla))*/
	}
	
  protected override void OnUnload()
  {
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    GL.BindVertexArray(0);
    base.OnUnload();
  }


  private int _vertexBuffer; // ID del buffer de vértices

  private int _vertexArray; // ID del array de vértices

  private int _indexBuffer; // ID del buffer de índices

  private Shader? _shader;
  
	private Camera _camera;

  private Matrix4 _Model;

  private readonly Vector3 _Axis = new Vector3(1.0f, 1.0f, 1.0f);

  private readonly float _RotSpeed = 1.0f;
  
  private float _RotAngle;

  private RetrievedMesh _retrievedMesh;
  private readonly JsonSerializerOptions _jsonOptions = new();
  private Mesh _mesh;

}
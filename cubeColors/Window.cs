using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using LearnOpenTK.Common;

public class Window : GameWindow
{

  public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings): base(gameWindowSettings,nativeWindowSettings)
  {
    _camera=new Camera(Vector3.UnitZ*3,Size.X / (float)Size.Y); 
    _Model= new Matrix4();
    _RotAngle=0.0f;
  }

  public float[] vertexData = {
    // Cara frontal (roja)
    -0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f,  // 0
     0.5f, -0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f,  // 1
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f,  // 2
    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 0.0f, 1.0f,  // 3
    // Cara trasera (verde)
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,  // 4
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,  // 5
     0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,  // 6
    -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f, 1.0f,  // 7
    // Cara izquierda (azul)
    -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 1.0f, 1.0f,  // 8
    -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f, 1.0f,  // 9
    -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f, 1.0f,  // 10
    -0.5f,  0.5f, -0.5f,  0.0f, 0.0f, 1.0f, 1.0f,  // 11
    // Cara derecha (amarilla)
     0.5f, -0.5f, -0.5f,  1.0f, 1.0f, 0.0f, 1.0f,  // 12
     0.5f, -0.5f,  0.5f,  1.0f, 1.0f, 0.0f, 1.0f,  // 13
     0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.0f, 1.0f,  // 14
     0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f, 1.0f,  // 15
    // Cara superior (rosa)
    -0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 1.0f, 1.0f,  // 16
     0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 1.0f, 1.0f,  // 17
     0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f, 1.0f,  // 18
    -0.5f,  0.5f,  0.5f,  1.0f, 0.0f, 1.0f, 1.0f,  // 19
    // Cara inferior (cian)
    -0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 1.0f, 1.0f,  // 20
     0.5f, -0.5f, -0.5f,  0.0f, 1.0f, 1.0f, 1.0f,  // 21
     0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 1.0f, 1.0f,  // 22
    -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 1.0f, 1.0f   // 23
  };

  public int[] indexData = {
    0, 1, 2, 2, 3, 0,  //
    4, 7, 6, 4, 6, 5,  //
    8, 9, 10, 10, 11, 8,  // 
    12, 14, 13, 14, 12, 15,  // 
    16, 18, 17, 18, 16, 19,  //
    20, 21, 22, 22, 23, 20   //
  };


  protected override void OnLoad()
  {
    base.OnLoad();
    GL.ClearColor(0.2f,0.2f,0.2f,1.0f);
    GL.Enable(EnableCap.CullFace);
    GL.Enable(EnableCap.DepthTest);
    _vertexBuffer=GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer,_vertexBuffer);
    GL.BufferData(BufferTarget.ArrayBuffer,vertexData.Length*sizeof(float),vertexData,BufferUsageHint.StaticDraw);
    _indexBuffer=GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ElementArrayBuffer,_indexBuffer);
    GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * sizeof(int), indexData, BufferUsageHint.StaticDraw);
    _shader = new Shader("Shaders/shader.vert","Shaders/shader.frag");
    _shader.Use();
    _vertexArray=GL.GenVertexArray();
    GL.BindVertexArray(_vertexArray);
    var posLocation = _shader.GetAttribLocation("aPosition");
    GL.EnableVertexAttribArray(posLocation);
    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
    var colorLocation = _shader.GetAttribLocation("aColor");
    GL.EnableVertexAttribArray(colorLocation);
    GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
  }

  protected override void OnUpdateFrame(FrameEventArgs e)
  {
    base.OnUpdateFrame(e);
    Matrix4.CreateFromAxisAngle(_Axis,_RotAngle,out _Model); 
    _RotAngle+=_RotSpeed*(float)e.Time;
    if (_RotAngle >= MathHelper.TwoPi)
      _RotAngle = 0;
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    GL.BindVertexArray(_vertexArray);
    _shader.SetMatrix4("model", _Model);
    _shader.SetMatrix4("view", _camera.GetViewMatrix());
    _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
    GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, indexData);
    SwapBuffers();
  }

  protected override void OnRenderFrame(FrameEventArgs args)
  {
    base.OnRenderFrame(args);
	}
	
  protected override void OnUnload()
  {
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    GL.BindVertexArray(0);
    base.OnUnload();
  }


  private int _vertexBuffer;
  
  private int _vertexArray;

  private int _indexBuffer;

  private Shader? _shader;
  
	private Camera _camera;

  private Matrix4 _Model;

  private readonly Vector3 _Axis = new Vector3(1.0f, 1.0f, 1.0f);

  private readonly float _RotSpeed = 1.0f;
  
  private float _RotAngle;

}
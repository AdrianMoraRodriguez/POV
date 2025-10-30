using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

public static class Program
{
    public static void Main()
    {
        var nativeWindowSetting=new NativeWindowSettings
        {
            ClientSize=new Vector2i(800,600),
            Title="OpenTk Cube"
        };

        using(var window=new Window(GameWindowSettings.Default,nativeWindowSetting))
        {
            window.Run();
        }
    }
}
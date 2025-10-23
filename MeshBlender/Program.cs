using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

public static class Program
{
    public static void Main()
    {
        var nativeWindowSetting = new NativeWindowSettings
        {
            Size = new Vector2i(800, 600),
            Title = "Blender Mesh"
        };

        using (var window = new Window(GameWindowSettings.Default, nativeWindowSetting))
        {
            window.Run();
        }
    }
}
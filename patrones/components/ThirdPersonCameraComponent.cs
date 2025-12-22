using OpenTK.Mathematics;
using LearnOpenTK.Common;

public class ThirdPersonCameraComponent : IComponent
{
    private readonly Camera _camera;
    private readonly IController _controller;
    private readonly float _cameraDistance;
    private readonly float _sensibility;

    public ThirdPersonCameraComponent(
        Camera camera,
        IController controller,
        float cameraDistance,
        float sensibility)
    {
        _camera = camera;
        _controller = controller;
        _cameraDistance = cameraDistance;
        _sensibility = sensibility;
    }

    public void Update(Actor pawn, float dt)
    {
        Angles2D deltaAngles = _controller.GetArmOrientation();
        deltaAngles.Yaw *= _sensibility;
        deltaAngles.Pitch *= _sensibility;

        _camera.Yaw += (float)deltaAngles.Yaw;
        _camera.Pitch += (float)deltaAngles.Pitch;

        Vector3 pawnPos = pawn.Model.ExtractTranslation();
        Vector3 behindOffset = _camera.Front * _cameraDistance;
        behindOffset.Y = 0.0f;
        _camera.Position = pawnPos - behindOffset + new Vector3(0.0f, _cameraDistance / 2.0f, 0.0f);
    }
}

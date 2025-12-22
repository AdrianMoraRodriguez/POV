using OpenTK.Mathematics;
using System;
using LearnOpenTK.Common;


public class PawnMovementComponent : IComponent
{
    private readonly IController _controller;
    private readonly Camera _camera;
    private readonly Level _level;
    private readonly float _speed;
    private readonly Action? _onWin;

    public PawnMovementComponent(
        IController controller,
        Camera camera,
        Level level,
        float speed,
        Action onWin)
    {
        _controller = controller;
        _camera = camera;
        _level = level;
        _speed = speed;
        _onWin = onWin;
    }

    public void Update(Actor pawn, float dt)
    {
        Vector3 movement = _controller.GetMovement();

        Vector3 forward = _camera.Front;
        Vector3 scale   = pawn.Model.ExtractScale();

        forward.Y = 0.0f; 
        forward   = Vector3.Normalize(forward);
        Vector3 translation =
            forward * movement.X +
            _camera.Right * movement.Y +
            _camera.Up    * movement.Z;

        translation *= _speed;
        if (translation.X != 0.0f)
        {
            float targetYaw = MathF.Atan2(-forward.X, -forward.Z);

            pawn.Model =
                Matrix4.CreateScale(scale)
                * Matrix4.CreateRotationY(targetYaw)
                * Matrix4.CreateTranslation(
                    pawn.Model.ExtractTranslation());
        }

        pawn.SaveModel();
        pawn.Model *= Matrix4.CreateTranslation(translation);
        pawn.UpdateCollisionModel();

        foreach (var pair in _level.ActorCollection)
        {
            Actor actor = pair.Value;

            if (actor == pawn || !actor.Enabled)
                continue;

            if (Collision.CheckEB(pawn, actor))
            {
                if (pair.Key == "amonkey")
                {
                    _onWin?.Invoke();
                }

                pawn.RestoreModel();
                pawn.UpdateCollisionModel();
                break;
            }
        }
    }
}

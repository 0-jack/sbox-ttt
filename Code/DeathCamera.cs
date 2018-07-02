using Sandbox;

namespace SuicideBarrels
{
    [ClassLibrary]
    public class DeathCamera : Controllable
    {
        protected SpringArmCamera _camera;
        protected BaseEntity _target;
        protected Vector3 _forceDirection;

        public bool WantsToRespawn { get; protected set; }

        protected override void Initialize()
        {
            base.Initialize();

            if (Authority)
            {
                WantsToRespawn = false;
            }

            if (Client)
            {
                _camera = new SpringArmCamera
                {
                    Replicates = false,
                    Owner = this,
                    IgnoreCollisionEntity = this,
                    TargetArmLength = 200.0,
                };

                _camera.Parent = this;
                _camera.Spawn();

                _target = null;
            }
        }

        protected override void OnPossessed()
        {
            base.OnPossessed();

            if (Client && Player != null)
            {
                Player.ViewTarget = _camera;

                if (_target == null)
                {
                    var deathTarget = Gamemode.Current.DeathTarget;
                    _target = deathTarget.Entity;

                    if (_target != null)
                    {
                        _camera.TargetSocket = deathTarget.Socket;
                        _camera.TargetOffset = deathTarget.Offset;
                        _camera.AttachTo(_target, deathTarget.Socket, AttachmentRule.KeepRelative);
                    }
                }
            }
        }

        protected override void DoInput()
        {
            base.DoInput();

            if (Input.JustPressed(Button.LeftMouseButton))
            {
                ServerSetWantsToRespawn();
            }

            double yaw = Input.Value(Axis.MouseX) * 0.05;
            double pitch = Input.Value(Axis.MouseY) * -0.05;
            var eyeInput = Quaternion.FromAngles(pitch, yaw, 0);

            AddEyeInput(eyeInput);

            _forceDirection = Vector3.Zero;

            if (Input.IsDown(Button.LeftShift))
            {
                if (Input.IsDown(Button.W)) _forceDirection += EyeAngles.Forward;
                if (Input.IsDown(Button.S)) _forceDirection -= EyeAngles.Forward;
                if (Input.IsDown(Button.D)) _forceDirection += EyeAngles.Right;
                if (Input.IsDown(Button.A)) _forceDirection -= EyeAngles.Right;

                _forceDirection = _forceDirection.Normal;
            }
        }

        protected override void Tick()
        {
            base.Tick();

            if (Client && Player != null)
            {
                if (_target != null && _target is BaseWorldEntity mesh &&
                    _forceDirection.Length > 0.0f)
                {
                    mesh.SetAllVelocity(_forceDirection * 2000.0 * Time.Delta);
                }
            }
        }

        [Client]
        public void ClientClearTarget()
        {
            Gamemode.Current.ClearDeathTarget();
        }

        [Server]
        protected void ServerSetWantsToRespawn()
        {
            WantsToRespawn = true;
        }
    }
}

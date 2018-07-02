using Sandbox;

namespace SuicideBarrels
{
    [ClassLibrary]
    public class SpectatorControllable : Controllable
    {
        protected double FlySpeed => 1500.0;

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void Tick()
        {
            base.Tick();
        }

        protected override void DoInput()
        {
            base.DoInput();

            var movementInput = Vector3.Zero;

            if (Input.IsDown(Button.W)) movementInput += Vector3.Forward;
            if (Input.IsDown(Button.S)) movementInput -= Vector3.Forward;
            if (Input.IsDown(Button.D)) movementInput += Vector3.Right;
            if (Input.IsDown(Button.A)) movementInput -= Vector3.Right;

            movementInput = movementInput.Normal;

            if (Input.IsDown(Button.LeftShift)) movementInput *= 1.5;
            else if (Input.IsDown(Button.LeftControl)) movementInput *= 0.5;

            double yaw = Input.Value(Axis.MouseX) * 0.05;
            double pitch = Input.Value(Axis.MouseY) * -0.05;
            var eyeInput = Quaternion.FromAngles(pitch, yaw, 0);

            AddMovementInput(movementInput);
            AddEyeInput(eyeInput);
        }

        protected override void TickMovement()
        {
            base.TickMovement();

            var inputVector = ConsumeInputVector();
            var moveDirection = EyeAngles.Forward * inputVector.x + EyeAngles.Right * inputVector.y;
            var velocity = moveDirection * FlySpeed;

            Position += velocity * Time.Delta;
        }

        [Server]
        protected void ServerMovement(Vector3 position, Quaternion eyeAngles)
        {
            Position = position;
            EyeAngles = eyeAngles;
        }

        public override void OnUpdateView(ref ViewInfo viewInfo)
        {
            viewInfo.Fov = 90.0;
        }
    }
}

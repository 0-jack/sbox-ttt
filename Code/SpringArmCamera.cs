using Sandbox;

namespace SuicideBarrels
{
    [ClassLibrary]
    public class SpringArmCamera : CameraEntity
    {
        public string TargetSocket { get; set; }
        public double TargetArmLength { get; set; }
        public Vector3 TargetOffset { get; set; }
        public double ProbeSize { get; set; }
        public bool DoCollisionTest { get; set; }
        public bool UseControllerRotation { get; set; }

        public BaseEntity IgnoreCollisionEntity { get; set; }

        public SpringArmCamera()
        {
            TargetArmLength = 200.0f;
            TargetOffset = Vector3.Zero;
            ProbeSize = 5.0f;
            DoCollisionTest = true;
            UseControllerRotation = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void Tick()
        {
            base.Tick();

            UpdateDesired();
        }

        public void UpdateDesired()
        {
            if (UseControllerRotation)
            {
                if (Owner is Controllable controller && controller.IsValid)
                {
                    Rotation = controller.EyeAngles;
                }
            }

            UpdateDesiredArmLocation();
        }

        protected void UpdateDesiredArmLocation()
        {
            var origin = TargetOffset;
            var target = Rotation.Forward * -TargetArmLength;

            var parentRotation = Quaternion.Identity;
            var offset = Vector3.Zero;

            if (Parent is BaseWorldEntity parent)
            {
                parentRotation = parent.GetSocketRotation(TargetSocket);
                offset = parent.GetSocketPosition(TargetSocket);

                origin = parentRotation.Unrotate(origin);
                target = parentRotation.Unrotate(target);
            }

            target += TargetOffset;

            if (DoCollisionTest)
            {
                var start = offset + origin;
                var end = offset + parentRotation.Rotate(target);

                if (World.SweepSphere(out var hitResult, start, end, ProbeSize, "Pawn", IgnoreCollisionEntity ?? Owner))
                {
                    target = origin + ((target - origin) * hitResult.Fraction);
                }
            }

            RelativePosition = target;
        }
    }
}

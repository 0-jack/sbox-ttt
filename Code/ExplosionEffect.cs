using Sandbox;

namespace SuicideBarrels
{
    [ClassLibrary]
    class ExplosionEffect : ParticleSystem
    {
        protected override void Initialize()
        {
            base.Initialize();

            Material = Material.Library.Get("Particles/poof.mat");

            SpawnRadius = 100;
            SpawnRadialVelocity = Curve.Random(-500, 500);
            Drag = 5;
            SpawnSize = Curve3.Random(250, 400);
            LifeScale = Curve3.Linear(0.75, 1.0);
            SpawnColor = new Color(0x4c4c4c);
            AlphaScaleOverLife = new Curve(0.75, 0);
            LifeTime = Curve.Random(1.5, 2);
            Align = ParticleAlignment.Square;
            Sort = ParticleSort.ProjectedDistance;

            CollisionMax = 0;
            EnableCollisions = true;

            WithBurst(500);

            new ParticleSystem
            {
                Position = Position,
                Rotation = Rotation,
                Material = Material.Library.Get("Particles/spark.mat"),
                SpawnColor = Curve3.RandomLinear(new Color(1, 0.7, 0.4), new Color(1, 1, 0.4)),
                LifeTime = Curve.Random(1, 1.5),
                SpawnSize = Curve3.RandomLinear(new Vector3(2, 3, 1), new Vector3(5, 15, 1)),
                LifeScale = Curve3.Linear(1.0, 0.0),
                Drag = 5,
                AlphaScaleOverLife = Curve.Linear(1, 0),
                CollisionMax = 3,
                EnableCollisions = true,
                CollisionDamp = Curve3.Random(0.1, 0.5),
                Align = ParticleAlignment.Velocity,
                OnCollisionMax = ParticleCollisionComplete.Freeze,
                SpeedScale = new Vector3(1, 0.01, 1),
                SpeedScaleMax = new Vector3(1, 100, 1),
                Sort = ParticleSort.Distance,
            }
            .WithConeVelocity(Curve.Random(0, 360), Curve.Random(1000, 2000))
            .WithBurst(250)
            .Spawn();
        }
    }
}

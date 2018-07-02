using Sandbox;

namespace SuicideBarrels
{
    public struct BulletHit
    {
        public BaseEntity Entity;
        public Vector3 Direction;
        public Vector3 Location;
        public Vector3 Normal;
        public int BoneIndex;
        public int Surface;
        public int FaceIndex;
    }

    public class BaseWeapon : BaseWorldEntity, IInventoryEntity
    {
        public virtual int InventoryOrder => 1234;
        public virtual double FireRate => 1.0f;

        protected double _lastShootTime = 0.0;

        protected override void Initialize()
        {
            base.Initialize();
        }

        public virtual void DoInput()
        {
            if (Input.IsDown(Button.LeftMouseButton))
            {
                ShootPrimary(Input.JustPressed(Button.LeftMouseButton));
            }

            if (Input.JustPressed(Button.RightMouseButton))
            {
                ShootSecondary(Input.JustPressed(Button.RightMouseButton));
            }
        }

        protected virtual void ShootPrimary(bool pressed)
        {
        }

        protected virtual void ShootSecondary(bool pressed)
        {
        }

        public void FireBullet(Vector3 origin, Vector3 direction, double damage, double maxDistance, double bulletForce)
        {
            var hitTest = GetBulletHit(origin, direction, maxDistance);
            if (hitTest == null) return;

            var hit = hitTest.Value;
            World.Gamemode.WorldImpact(hit.Location, hit.Normal, hit.Direction, hit.Surface, 1.0f, hit.Entity);

            foreach (var ent in hit.Entity.AncestorsAndSelf)
            {
                if (ent is BaseWorldEntity worldEntity && worldEntity.SimulatePhysics)
                {
                    worldEntity.AddImpulseAtLocation(direction.Normal * bulletForce, hit.Location, hit.BoneIndex);
                }

                if (Authority && ent is ICanBeDamaged damageable)
                {
                    var damageInfo = new DamageInfo
                    {
                        Amount = damage,
                        Force = bulletForce,
                        Weapon = this,
                        Player = Owner,
                        Location = hit.Location,
                        Normal = hit.Normal,
                        Source = Owner.EyePosition
                    };

                    damageable.TakeDamage(damageInfo);
                }
            }
        }

        protected BulletHit? GetBulletHit(Vector3 origin, Vector3 direction, double maxDistance)
        {
            if (Owner == null)
            {
                return null;
            }

            Vector3 traceEnd = origin + direction * maxDistance;
            World.LineTrace(out var hitResult, Owner.EyePosition, traceEnd, CollisionChannel.GameTraceChannel1, Owner);

            if (!hitResult.BlockingHit)
            {
                return null;
            }

            return new BulletHit
            {
                Direction = direction,
                Entity = hitResult.Entity,
                Location = hitResult.Location,
                Normal = hitResult.Normal,
                BoneIndex = hitResult.BoneIndex,
                Surface = hitResult.SurfaceType,
                FaceIndex = hitResult.FaceIndex,
            };
        }
    }
}

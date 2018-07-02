using Sandbox;

namespace SuicideBarrels
{
    [ClassLibrary]
    public class PistolWeapon : BaseWeapon
    {
        public override double FireRate => 0.2;

        public double BulletDamage => 100;
        public double BulletDistance => 10000;
        public double BulletForce => 20000;

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void ShootPrimary(bool pressed)
        {
            base.ShootPrimary(pressed);

            if (!pressed) return;

            if (_lastShootTime > Time.Now - FireRate)
            {
                return;
            }

            _lastShootTime = Time.Now;

            // Client side bullet fire
            FireBullet(Owner.EyePosition, Owner.EyeRotation.Forward);

            // Server side bullet fire
            ServerShootPrimary(Owner.EyePosition, Owner.EyeRotation.Forward);

            if (Client)
            {
                UI.Crosshair.Current?.Punch(5.0);
            }
        }

        [Server]
        protected void ServerShootPrimary(Vector3 origin, Vector3 direction)
        {
            FireBullet(origin, direction);

            BroadcastShootPrimary();
        }

        protected void FireBullet(Vector3 origin, Vector3 direction)
        {
            FireBullet(origin, direction, BulletDamage, BulletDistance, BulletForce);
        }

        [Multicast]
        protected void BroadcastShootPrimary()
        {
            if (Owner == null) return;

            Owner.PlaySoundOnAttachment("Sounds/Weapons/pistol/pistol_fire2.wav", "", Vector3.Zero, 1.0, Random.Double(1.0, 1.1));
        }
    }
}

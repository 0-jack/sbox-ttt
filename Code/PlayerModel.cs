using System;
using Sandbox;
using System.Threading.Tasks;

namespace SuicideBarrels
{
    [ClassLibrary]
    public class PlayerModel : SkeletalMeshEntity
    {
        protected override void Initialize()
        {
            base.Initialize();
        }

        public void Ragdoll()
        {
            Collision = true;
            CollisionProfileName = "Ragdoll";
            SimulatePhysics = true;

            StopAnimation();
        }

        public async void DestroyLater(double delay)
        {
            while (UniqueId == Gamemode.Current.DeathTarget.Entity?.UniqueId)
            {
                await Task.Yield();
            }

            await Delay(TimeSpan.FromSeconds(delay));

            Destroy();
        }
    }
}

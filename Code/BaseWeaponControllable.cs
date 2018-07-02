using Sandbox;

namespace SuicideBarrels
{
    public class BaseWeaponControllable : CharacterControllable
    {
        [Replicate]
        public BaseWeapon ActiveWeapon { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void DoInput()
        {
            base.DoInput();

            ActiveWeapon?.DoInput();
        }
    }
}

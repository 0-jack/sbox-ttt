using Sandbox;
using System.Collections.Generic;

namespace SuicideBarrels.UI
{
    [ClassLibrary]
    class Crosshair : HudComponent
    {
        public static Crosshair Current;

        public bool Visible { get; set; }

        public Crosshair()
        {
            Current = this;
            Visible = false;
        }

        public override void Tick()
        {
            base.Tick();

            if (HumanControllable.Local != null &&
                HumanControllable.Local is HumanControllable human)
            {
                Visible = human.ActiveWeapon != null;
            }
            else if (Visible)
            {
                Visible = false;
            }
        }

        public void Punch(double offset)
        {
            Call("Punch", offset);
        }
    }
}

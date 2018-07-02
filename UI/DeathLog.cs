using Sandbox;
using System.Collections.Generic;

namespace SuicideBarrels.UI
{
    [ClassLibrary]
    class DeathLog : HudComponent
    {
        public static DeathLog Current;

        public DeathLog()
        {
            Current = this;
        }

        public struct Entry
        {
            public string Id;
            public string Attacker;
            public string Victim;
            public double Time;
            public string AttackerColor;
            public string VictimColor;
        }

        public List<Entry> Entries { get; } = new List<Entry>();

        public void Add(string attacker, string victim, Color attackerColor, Color victimColor)
        {
            Entries.Add(new Entry
            {
                Id = System.Guid.NewGuid().ToString(),
                Attacker = attacker,
                Victim = victim,
                Time = Time.Real,
                AttackerColor = attackerColor.Hex,
                VictimColor = victimColor.Hex,
            });
        }

        public override void Tick()
        {
            Entries.RemoveAll(x => x.Time < Time.Real - 3.0);
        }
    }
}

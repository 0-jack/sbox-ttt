using Sandbox;

namespace SuicideBarrels
{
    [ClassLibrary]
    internal class Hud : EngineHud
    {
        public UI.Crosshair Crosshair { get; protected set; }
        public UI.Scoreboard Scoreboard { get; protected set; }
        public UI.PhaseDisplay PhaseDisplay { get; protected set; }
        public UI.DeathNotify DeathLog { get; protected set; }

        protected override void Initialize()
        {
            base.Initialize();

            if (Client)
            {
                Components.Add(Crosshair = new UI.Crosshair());
                Components.Add(Scoreboard = new UI.Scoreboard());
                Components.Add(PhaseDisplay = new UI.PhaseDisplay());
                Components.Add(DeathLog = new UI.DeathNotify());
            }
        }

        protected override void Tick()
        {
            base.Tick();
        }

        [Multicast]
        public void AddDeathLog(string killer, string killed, Team killerTeam, Team killedTeam)
        {
            DeathLog?.Add(killer, killed, Gamemode.TeamColor(killerTeam), Gamemode.TeamColor(killedTeam));
        }

        [ConsoleCommand("say")]
        public static void Say(string message)
        {
            Player.Local?.Message(message);
        }
    }
}

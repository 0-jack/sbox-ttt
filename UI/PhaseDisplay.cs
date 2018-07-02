using Sandbox;
using System;

namespace SuicideBarrels.UI
{
    [ClassLibrary]
    public class PhaseDisplay : HudComponent
    {
        public string Title { get; private set; }

        public override void Tick()
        {
            base.Tick();

            var gamemode = Gamemode.Current;

            if (gamemode == null)
            {
                Title = string.Empty;

                return;
            }

            switch (gamemode.Phase)
            {
                case Phase.WaitingForPlayers:
                    Title = $"Waiting for players ({gamemode.PlayersWaiting}/{Gamemode.PlayersNeeded})";
                    break;
                case Phase.RoundStarting:
                    Title = "Round starting...";
                    break;
                case Phase.RoundWarmup:
                    Title = "Round Warmup...";
                    break;
                case Phase.RoundOver:
                    Title = "Round over!";
                    break;
                case Phase.RoundActive:
                    var time = TimeSpan.FromSeconds(gamemode.RoundTimerSeconds);
                    Title = $"Round {gamemode.Round} ({time.ToString(@"mm\:ss")})";
                    break;
            }
        }
    }
}

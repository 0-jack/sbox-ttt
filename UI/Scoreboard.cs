using Sandbox;
using System.Linq;

namespace SuicideBarrels.UI
{
    [ClassLibrary]
    class Scoreboard : HudComponent
    {
        public bool Visible { get; set; }

        public string ServerName { get; set; } = "My Server";

        public object PlayerList
        {
            get
            {
                if (Hud == null) return null;
                if (Hud.World == null) return null;

                return Hud.World.Connections.Select(x => new
                {
                    UserId = x.UserId.ToString(),
                    x.Username,
                    x.Kills,
                    x.Deaths,
                    x.Ping,
                    x.Score,
                    x.IsLocalPlayer
                });
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (Player.Local != null)
            {
                ServerName = Player.Local.ServerName;
            }
        }

        public override void OnInput()
        {
            Visible = Input.IsDown(Button.Tab);
        }
    }
}

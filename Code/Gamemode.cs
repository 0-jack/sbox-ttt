using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Random = Sandbox.Random;
using Math = Sandbox.Math;

namespace SuicideBarrels
{
    enum Phase
    {
        WaitingForPlayers,
        RoundOver,
        RoundStarting,
        RoundWarmup,
        RoundActive,
    }

    enum Team : int
    {
        Spectator,
        Human,
        Barrel,
    }

    public struct DeathTarget
    {
        public BaseEntity Entity;
        public string Socket;
        public Vector3 Offset;

        public static readonly DeathTarget NoTarget = new DeathTarget
        {
            Entity = null,
            Socket = string.Empty,
            Offset = Vector3.Zero
        };
    }

    [ClassLibrary(Name = "SuicideBarrelsGamemode")]
    class Gamemode : BaseGamemode
    {
        public static Gamemode Current { get; set; }

        [Replicate]
        public Hud MyHud { get; private set; }

        [Replicate]
        public Phase Phase { get; set; }

        [Replicate]
        public int Round { get; set; }

        private double _timeToStartRound;
        private double _timeToEndRound;
        private double _timeToRoundWarmup;
        private double _roundOverTime;

        public double TimeToEndRound => _timeToEndRound;

        [Replicate]
        public int RoundTimerSeconds { get; set; }

        [ConsoleVariable(Name = "round_time", Help = "Duration of a round")]
        public static double RoundTime { get; set; } = 120;

        [ConsoleVariable(Name = "round_warmup_time", Help = "Duration of round warmup")]
        private static double RoundWarmupTime { get; set; } = 5.0;

        [ConsoleVariable(Name = "round_warmup_starting_time", Help = "Duration of starting round warmup")]
        private static double RoundWarmupStartingTime { get; set; } = 2.0;

        [ConsoleVariable(Name = "round_over_time", Help = "Duration of round over")]
        private static double RoundOverTime { get; set; } = 5.0;

        [ConsoleVariable(Name = "players_needed", Help = "How many players needed to start a round")]
        public static int PlayersNeeded { get; set; } = 2;

        protected List<Player> _players;
        public int PlayerCount => _players.Count;

        [Replicate]
        public int PlayersWaiting { get; set; } = 0;

        public DeathTarget DeathTarget { get; set; }

        public static Color TeamColor(Team team)
        {
            switch (team)
            {
                case Team.Barrel: return Color.Red;
                case Team.Human: return Color.Green;
                case Team.Spectator: return new Color(0.7, 0.7, 0.7);
                default: return Color.White;
            }
        }

        public static double TeamSpawnOffset(Team team)
        {
            switch (team)
            {
                case Team.Barrel: return 45;
                case Team.Human: return 70;
                case Team.Spectator: return 140;
                default: return 0;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            Current = this;

            if (Server)
            {
                MyHud = World.CreateAndSpawnEntity<Hud>();

                Phase = Phase.WaitingForPlayers;
                Round = 0;
                RoundTimerSeconds = 0;
                PlayersWaiting = 0;

                _players = new List<Player>();
            }

            PreloadAssets();
        }

        // Ghetto asset preloading until we do this properly
        protected static void PreloadAssets()
        {
            foreach (var taunt in BarrelControllable.Taunts)
            {
                Sound.Library.Get(taunt, false);
            }

            Sound.Library.Get("Sounds/weapons/explode3.wav");
            Sound.Library.Get("Sounds/weapons/explode4.wav");
            Sound.Library.Get("Sounds/weapons/explode5.wav");
            Sound.Library.Get("Sounds/buttons/button17.wav");
            Sound.Library.Get("Sounds/buttons/button17.wav");
            Sound.Library.Get("Sounds/weapons/cguard/charging.wav");
            Sound.Library.Get("Sounds/Weapons/pistol/pistol_fire2.wav", false);
            SkeletalModel.Library.Get("models/player/mossman.mdl", false);
            Model.Library.Get("models/props_c17/oildrum001_explosive.mdl", false);
            Material.Library.Get("Particles/spark.mat");
            Material.Library.Get("Particles/poof.mat");
        }

        protected override void Tick()
        {
            base.Tick();

            if (Authority)
            {
                ServerTick();
            }

            if (Client && Player.Local != null)
            {
                if (Player.Local.Controlling is DeathCamera == false)
                {
                    ClearDeathTarget();
                }
            }
        }

        protected void ServerTick()
        {
            switch (Phase)
            {
                case Phase.WaitingForPlayers:
                    WaitingForPlayers();
                    break;

                case Phase.RoundStarting:
                    RoundStarting();
                    break;

                case Phase.RoundWarmup:
                    RoundWarmup();
                    break;

                case Phase.RoundActive:
                    RoundActive();
                    break;

                case Phase.RoundOver:
                    RoundOver();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        protected void SwitchPhase(Phase phase)
        {
            if (Phase == phase) return;

            Phase = phase;
        }

        protected void WaitingForPlayers()
        {
            PlayersWaiting = PlayerCount;

            if (PlayerCount < PlayersNeeded)
            {
                return;
            }

            _timeToRoundWarmup = Time.Now + RoundWarmupStartingTime;
            SwitchPhase(Phase.RoundStarting);
        }

        protected void RoundStarting()
        {
            if (Time.Now < _timeToRoundWarmup)
            {
                return;
            }

            MyHud.BroadcastMessage("Round warmup has started!");

            foreach (var player in _players)
            {
                if (player == null) continue;

                player.Team = (int)Team.Human;
                RespawnPlayer(player);
            }

            _timeToStartRound = Time.Now + RoundWarmupTime;
            SwitchPhase(Phase.RoundWarmup);
        }

        protected void RoundWarmup()
        {
            if (Time.Now < _timeToStartRound)
            {
                return;
            }

            var barrel = _players[Random.Int(0, _players.Count -  1)];

            if (PlayerCount == 1)
            {
                barrel = null;
            }

            foreach (var player in _players)
            {
                if (player == null || player == barrel) continue;

                player.Team = (int)Team.Human;
                RespawnPlayer(player);
            }

            if (barrel != null)
            {
                barrel.Team = (int)Team.Barrel;
                RespawnPlayer(barrel);

                MyHud.BroadcastMessage($"{barrel.Name} is the barrel!");
            }

            Round++;

            BroadcastRoundStarted();

            _timeToEndRound = Time.Now + RoundTime;
            SwitchPhase(Phase.RoundActive);
        }

        protected void RoundActive()
        {
            RoundTimerSeconds = Math.RoundToInt(_timeToEndRound - Time.Now);

            var roundActive = Time.Now < _timeToEndRound;
            var humansLeft = Enumerable.Count(_players.Where(x => x.Team == (int)Team.Human));

            if (roundActive && humansLeft > 0)
            {
                return;
            }

            if (!roundActive && humansLeft > 0)
            {
                MyHud.BroadcastMessage("Round has ended, humans win!");
            }
            else if (humansLeft == 0)
            {
                MyHud.BroadcastMessage("Round has ended, barrels win!");
            }

            BroadcastRoundOver();

            _roundOverTime = Time.Now + RoundOverTime;
            SwitchPhase(Phase.RoundOver);
        }

        protected void RoundOver()
        {
            if (Time.Now < _roundOverTime)
            {
                return;
            }

            foreach (var player in _players)
            {
                if (player == null) continue;

                player.Team = (int)Team.Spectator;

                if (player.Controlling != null && player.Controlling.IsValid)
                {
                    player.Controlling.Destroy();
                }

                RespawnPlayer(player);
            }

            SwitchPhase(Phase.WaitingForPlayers);
        }

        [Multicast]
        protected void BroadcastRoundStarted()
        {
            _timeToEndRound = Time.Now + RoundTime;

            MyHud.Chatbox?.AddMessage("", $"Round has started! ({RoundTime}) second round", Color.White);
        }

        [Multicast]
        protected void BroadcastRoundOver()
        {
            World.PlaySound2D("Sounds/ambient/alarms/klaxon1.wav");
        }

        public override Controllable CreateControllable(Player player)
        {
            switch ((Team)player.Team)
            {
                case Team.Spectator: return new SpectatorControllable();
                case Team.Barrel: return new BarrelControllable();
                case Team.Human: return new HumanControllable();
                default: return null;
            }
        }

        public override void LoadMap(string name)
        {
            DefaultPostProcess.MotionBlurAmount = 0;

            base.LoadMap(name);
        }

        public override void OnPlayerJoined(Player player)
        {
            if (Phase == Phase.RoundActive)
            {
                player.Team = (int)Team.Barrel;
            }
            else
            {
                // Start player off as spectator
                player.Team = (int)Team.Spectator;
            }

            _players.Add(player);

            MyHud.BroadcastMessage($"{player.Name} joined the game");

            if (Authority)
            {
                RespawnPlayer(player);
            }
        }

        public override void OnPlayerLeave(Player player)
        {
            base.OnPlayerLeave(player);

            _players.Remove(player);

            MyHud.BroadcastMessage($"{player.Name} left the game");
        }

        public override void OnPlayerDied(Player player, Controllable controllable)
        {
            if (player == null)
            {
                return;
            }

            MyHud.BroadcastMessage($"{player.Name} has died");

            if (Authority)
            {
                var position = controllable.Position;
                var eyeAngles = controllable.EyeAngles;

                var deathCamera = new DeathCamera();
                deathCamera.Spawn();
                player.Controlling = deathCamera;

                deathCamera.Position = position;
                deathCamera.Teleport(position);

                deathCamera.EyeAngles = eyeAngles;
                deathCamera.ClientEyeAngles = eyeAngles;

                if (Phase == Phase.RoundActive && player.Team == (int)Team.Human)
                {
                    player.Team = (int)Team.Barrel;
                }

                if (Phase != Phase.RoundOver)
                {
                    RespawnPlayerLater(player, deathCamera, 3.0);
                }
            }
        }

        public override void OnPlayerMessage(string playerName, int team, string message)
        {
            var color = TeamColor((Team)team);
            MyHud?.Chatbox?.AddMessage(playerName, message, color);
        }

        public override bool AllowPlayerMessage(Player sender, Player receiver, string message)
        {
            return true;
        }

        public override void RespawnPlayer(Player player)
        {
            Log.Assert(Authority);

            if (player.Controlling is DeathCamera deathCamera &&
                deathCamera.IsValid)
            {
                deathCamera.ClientClearTarget();
            }

            player.Controlling?.Destroy();

            var controllable = CreateControllable(player);
            controllable.Spawn();
            player.Controlling = controllable;

            var spawnPoint = FindSpawnPoint();

            if (spawnPoint != null)
            {
                var spawnangles = spawnPoint.Rotation.ToAngles();
                spawnangles = spawnangles.WithZ(0).WithY(spawnangles.Y + 180.0f);

                var eyeAngles = Quaternion.FromAngles(spawnangles);

                var team = (Team)player.Team;
                var heightOffset = TeamSpawnOffset(team);

                var position = spawnPoint.Position + Vector3.Up * heightOffset;
                controllable.Position = position;
                controllable.Teleport(position);
                controllable.ClientLocation = position;
                controllable.EyeAngles = eyeAngles;
                controllable.ClientEyeAngles = eyeAngles;
            }
            else
            {
                Log.Warning("Player {0} couldn't find spawn point", player);
            }

            controllable.OnRespawned();
        }

        async void RespawnPlayerLater(Player player, DeathCamera deathCamera, double delay)
        {
            await Delay(TimeSpan.FromSeconds(delay));

            while (deathCamera.IsValid && !deathCamera.WantsToRespawn)
            {
                await Task.Yield();
            }

            if (Phase != Phase.RoundActive &&
                Phase != Phase.RoundWarmup)
            {
                return;
            }

            if (deathCamera != null && deathCamera.IsValid)
            {
                deathCamera.ClientClearTarget();
                deathCamera.Destroy();
            }

            RespawnPlayer(player);
        }

        public override void OnLocalInput()
        {
            base.OnLocalInput();
        }

        public void ClearDeathTarget()
        {
            DeathTarget = DeathTarget.NoTarget;
        }

        public void RegisterDeath(Player killer, Player killed, Team killedTeam)
        {
            MyHud?.AddDeathLog(killer.Name, killed.Name, (Team)killer.Team, killedTeam);
        }
    }
}

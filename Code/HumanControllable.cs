using Sandbox;

namespace SuicideBarrels
{
    [ClassLibrary]
    public partial class HumanControllable : BaseWeaponControllable, ICanBeDamaged, IHasHealth, IHasInventory
    {
        public static HumanControllable Local { get; private set; }

        protected PlayerModel _model;
        protected Quaternion _modelRotation;
        protected double _rotationSpeed => 10.0;

        [ConsoleVariable(Name = "ragdoll_fade_time", Help = "How long to keep ragdolls around")]
        public static double RagdollFadeTime { get; set; } = 20;

        public double MaxHealth { get; set; } = 100;

        [Replicate]
        protected double _health { get; set; }

        public virtual double Health
        {
            get => _health;
            set
            {
                var oldHealth = _health;
                _health = Math.Clamp(value, 0, MaxHealth);

                if (oldHealth > _health && _health <= 0)
                {
                    Die();
                }
            }
        }

        protected bool IsAlive => _health > 0;
        protected bool IsDead => !IsAlive;

        protected override void Initialize()
        {
            base.Initialize();

            if (Authority)
            {
                Health = 100;

                ActiveWeapon = new PistolWeapon { Owner = this, Replicates = true, };
                ActiveWeapon.Spawn();
            }

            if (Client)
            {
                _model = new PlayerModel
                {
                    Model = SkeletalModel.Library.Get("models/player/mossman.mdl"),
                    RelativePosition = Vector3.Down * StandingHeight,
                    RelativeRotation = Quaternion.Identity,
                    Collision = false,
                    Replicates = false,
                    Interpolated = false,
                    HideFromOwner = true,
                    Owner = this,
                };

                _model.AttachTo(this, AttachmentRule.KeepRelative);
                _model.Spawn();
            }
        }

        protected override void OnPossessed()
        {
            base.OnPossessed();

            Local = this;
        }

        protected override void Tick()
        {
            base.Tick();

            if (Client && _model != null)
            {
                var heading = ControlledByLocalPlayer ? EyeAngles.Yaw : _eyeAngles.Y;
                var targetRotation = Quaternion.FromAngles(0, heading + 90, 0);
                var delta = Math.Clamp(Time.Delta * _rotationSpeed, 0.0, 1.0);
                _modelRotation = Quaternion.Slerp(_modelRotation, targetRotation, delta);
                _model.RelativeRotation = _modelRotation;
            }
        }

        protected override void DoInput()
        {
            base.DoInput();

            var movementInput = Vector3.Zero;

            if (Input.IsDown(Button.W)) movementInput += Vector3.Forward;
            if (Input.IsDown(Button.S)) movementInput -= Vector3.Forward;
            if (Input.IsDown(Button.D)) movementInput += Vector3.Right;
            if (Input.IsDown(Button.A)) movementInput -= Vector3.Right;
            if (Input.JustPressed(Button.V)) Noclip = !Noclip;

            movementInput = movementInput.Normal;

            if (Input.IsDown(Button.LeftShift)) movementInput *= 1.5;

            if (Input.JustPressed(Button.X))
            {
                Suicide();
            }

            JumpInput = Input.IsDown(Button.SpaceBar);
            CrouchInput = Input.IsDown(Button.LeftControl);

            double yaw = Input.Value(Axis.MouseX) * 0.05;
            double pitch = Input.Value(Axis.MouseY) * -0.05;
            var eyeInput = Quaternion.FromAngles(pitch, yaw, 0);

            AddMovementInput(movementInput);
            AddEyeInput(eyeInput);
        }

        public bool Give(string name, int amount = 1)
        {
            // No pickups yet
            return false;
        }

        public bool TakeDamage(DamageInfo damage)
        {
            var player = Player;
            var team = (Team)player.Team;

            Health -= damage.Amount;

            if (IsDead)
            {
                if (damage.Player is BarrelControllable attacker &&
                    attacker.Player != null)
                {
                    attacker.Player.Kills += 1;

                    Gamemode.Current.RegisterDeath(attacker.Player, player, team);
                }

                if (player != null)
                {
                    player.Deaths += 1;
                }
            }

            return IsDead;
        }

        public override void Die()
        {
            BroadcastDeath();

            Gamemode.Current.OnPlayerDied(Player, this);

            Destroy();
        }

        [Multicast]
        protected void BroadcastDeath()
        {
            if (ControlledByLocalPlayer)
            {
                Gamemode.Current.DeathTarget = new DeathTarget
                {
                    Entity = _model,
                    Socket = "ValveBiped.Bip01_Pelvis",
                    Offset = Vector3.Zero,
                };
            }

            if (_model != null)
            {
                _model.Owner = null;
                _model.Ragdoll();
                _model.SetAllVelocity(Rep_MovementVelocity);
                _model.DestroyLater(RagdollFadeTime);
            }
        }

        [Server]
        protected void Suicide()
        {
            Die();
        }
    }
}

using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Math = Sandbox.Math;
using Random = Sandbox.Random;

namespace SuicideBarrels
{
    [ClassLibrary]
    public partial class BarrelControllable : CharacterControllable, ICanBeDamaged, IHasHealth
    {
        public static readonly string[] Taunts = new string[]
        {
            "Sounds/vo/npc/male01/behindyou01.wav",
            "Sounds/vo/npc/male01/behindyou02.wav",
            "Sounds/vo/npc/male01/zombies01.wav",
            "Sounds/vo/npc/male01/watchout.wav",
            "Sounds/vo/npc/male01/upthere01.wav",
            "Sounds/vo/npc/male01/upthere02.wav",
            "Sounds/vo/npc/male01/thehacks01.wav",
            "Sounds/vo/npc/male01/strider_run.wav",
            "Sounds/vo/npc/male01/runforyourlife01.wav",
            "Sounds/vo/npc/male01/runforyourlife02.wav",
            "Sounds/vo/npc/male01/runforyourlife03.wav",
        };

        protected override double ColliderWidth => 29;
        protected override double ColliderDepth => 29;
        protected override double StandingHeight => 45.0;
        protected override double CrouchingHeight => 45.0;

        protected double _detonateReady;

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
                    Explode();
                }
            }
        }

        protected bool IsAlive => _health > 0;
        protected bool IsDead => !IsAlive;

        protected BaseMeshEntity _model;
        protected BaseMeshEntity _debrisModel;
        protected SpringArmCamera _camera;

        protected bool _canTaunt = true;
        protected bool _canDetonate = true;
        protected bool _detonated = false;

        protected Quaternion _modelRotation;
        protected double _rotationSpeed => 10.0;

        [Replicate]
        protected double _heading { get; set; }

        protected virtual double _explosionRadius => 600;
        protected virtual double _explosionDamage => 250.0;

        [ConsoleVariable(Name = "barrel_explosion_force", Help = "How hard to push away physics objects")]
        public static double ExplosionForce { get; set; } = 2000;

        protected override void Initialize()
        {
            base.Initialize();

            if (Authority)
            {
                Health = 100;

                _detonateReady = Time.Now + 1.0;
                _heading = EyeAngles.Yaw;
            }

            Collider.CollisionProfileName = "CharacterMesh";

            if (Client)
            {
                _modelRotation = Quaternion.FromAngles(0, _heading, 0);

                _model = new BaseMeshEntity
                {
                    Model = Model.Library.Get("models/props_c17/oildrum001_explosive.mdl"),
                    RelativePosition = Vector3.Down * StandingHeight,
                    RelativeRotation = _modelRotation,
                    Collision = false,
                    Replicates = false,
                    Interpolated = false,
                    HideFromOwner = true,
                    Owner = this,
                };

                _model.AttachTo(this, AttachmentRule.KeepRelative);
                _model.Spawn();

                _debrisModel = new BaseMeshEntity
                {
                    Model = Model.Library.Get("models/props_junk/watermelon01.mdl"),
                    RelativePosition = Vector3.Zero,
                    RelativeRotation = Quaternion.Identity,
                    Collision = false,
                    Replicates = false,
                    Interpolated = false,
                    Hidden = true,
                    Owner = this,
                };

                _debrisModel.AttachTo(this, AttachmentRule.KeepRelative);
                _debrisModel.Spawn();

                const double ProbeRadius = 5.0;

                _camera = new SpringArmCamera
                {
                    Replicates = false,
                    Owner = this,
                    IgnoreCollisionEntity = this,
                    TargetArmLength = 300.0,
                    TargetOffset = Vector3.Up * ((StandingHeight * 2.0) - (ProbeRadius * 2.0)),
                    ProbeSize = ProbeRadius,
                };

                _camera.Parent = _model;
                _camera.Spawn();
            }
        }

        protected override void OnPossessed()
        {
            base.OnPossessed();

            if (Client && Player != null)
            {
                Player.ViewTarget = _camera;
            }
        }

        protected override void Tick()
        {
            base.Tick();

            if (Client && _model != null)
            {
                var heading = ControlledByLocalPlayer ? EyeAngles.Yaw : _heading;
                var targetRotation = Quaternion.FromAngles(0, heading, 0);
                var delta = Math.Clamp(Time.Delta * _rotationSpeed, 0.0, 1.0);
                _modelRotation = Quaternion.Slerp(_modelRotation, targetRotation, delta);
                _model.RelativeRotation = _modelRotation;

                if (Player != null && Player.ViewTarget != _camera)
                {
                    Player.ViewTarget = _camera;
                }
            }
        }

        protected override void TickMovement()
        {
            base.TickMovement();

            if (Authority)
            {
                _heading = EyeAngles.Yaw;
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

            movementInput = movementInput.Normal;

            if (Input.IsDown(Button.LeftShift)) movementInput *= 1.5;
            else if (Input.IsDown(Button.LeftControl)) movementInput *= 0.25;

            JumpInput = Input.IsDown(Button.SpaceBar);

            if (Input.JustPressed(Button.V)) Noclip = !Noclip;

            double yaw = Input.Value(Axis.MouseX) * 0.05;
            double pitch = Input.Value(Axis.MouseY) * -0.05;
            var eyeInput = Quaternion.FromAngles(pitch, yaw, 0);

            if (IsAlive)
            {
                AddMovementInput(movementInput);
            }

            AddEyeInput(eyeInput);

            if (IsAlive)
            {
                if (Input.JustPressed(Button.LeftMouseButton))
                {
                    DoSuicide();
                }

                if (Input.JustPressed(Button.RightMouseButton))
                {
                    DoTaunt();
                }
            }
        }

        [Server]
        protected async void DoTaunt()
        {
            if (!_canTaunt) return;
            if (!IsAlive) return;

            BroadcastTaunt(Random.Int(0, Taunts.Length - 1));

            _canTaunt = false;
            await Delay(TimeSpan.FromSeconds(2.0));
            _canTaunt = true;
        }

        [Multicast]
        protected void BroadcastTaunt(int index)
        {
            if (!Client) return;
            if (index < 0 || index >= Taunts.Length) return;

            var taunt = Taunts[index];
            PlaySoundOnAttachment(taunt, "", Vector3.Up * StandingHeight, 1.0, Random.Double(1.1, 1.35));
        }

        [Server]
        protected void DoSuicide()
        {
            Detonate();
        }

        public bool TakeDamage(DamageInfo damage)
        {
            var player = Player;
            var team = (Team)player.Team;

            Health -= damage.Amount;

            if (IsDead)
            {
                if (damage.Player is HumanControllable attacker &&
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

        public async void Detonate()
        {
            if (!Authority) return;
            if (IsDead || !_canDetonate) return;

            if (Time.Now < _detonateReady) return;

            _canDetonate = false;

            BroadcastDetonate();
            await Delay(TimeSpan.FromSeconds(1.0));

            if (IsDead) return;

            if (Player != null)
            {
                Player.Deaths += 1;
            }

            _detonated = true;
            Health = 0;
        }

        public void Explode()
        {
            if (!IsValid || IsPendingDestroy) return;
            if (!Authority) return;

            if (_detonated)
            {
                World.GetEntitiesInRadius(Position, _explosionRadius, out List<BaseEntity> entities);
                var hitEntities = entities.Select(e => e.GetParentOfType<Controllable>() ?? e).Distinct();

                foreach (var entity in hitEntities)
                {
                    var damageable = entity as ICanBeDamaged;
                    if (damageable == null || damageable == this)
                    {
                        continue;
                    }

                    var traceStart = entity is Controllable player ? player.EyePosition : entity.Position;
                    World.LineTrace(out var hitResult, traceStart, Position, CollisionChannel.Visibility, entity);

                    var distance = Vector3.DistanceBetween(entity.Position, Position);
                    var intensity = Math.Max(Math.Pow((1.0 - (distance / _explosionRadius)), 1.0), 0.0);

                    var damageInfo = new DamageInfo
                    {
                        Amount = (int)(_explosionDamage * intensity),
                        Force = intensity,
                        Weapon = this,
                        Player = this,
                        Location = entity.Position,
                        Source = Position
                    };

                    damageable.TakeDamage(damageInfo);
                }
            }

            BroadcastExplosion();

            Die();
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
                Gamemode.Current.DeathTarget = new DeathTarget { Entity = _debrisModel };
            }

            if (_debrisModel != null)
            {
                _debrisModel.Owner = null;
                _debrisModel.Collision = true;
                _debrisModel.CollisionProfileName = "Ragdoll";
                _debrisModel.SimulatePhysics = true;
                _debrisModel.Hidden = false;

                var direction = Quaternion.Random.Forward;

                _debrisModel.SetAllVelocity(Rep_MovementVelocity + (direction * 2000.0));
                _debrisModel.AddTorque(direction * 200.0, true);
            }
        }

        [Multicast]
        protected void BroadcastDetonate()
        {
            if (!Client || IsDead) return;

            EmitSound("Sounds/weapons/cguard/charging.wav");
        }

        protected void EmitSound(string sound)
        {
            if (IsDead || !IsValid || IsPendingDestroy) return;
            PlaySoundOnAttachment(sound, "", Vector3.Up * StandingHeight);
        }

        [Multicast]
        protected void BroadcastExplosion()
        {
            if (!Client) return;

            new ExplosionEffect
            {
                Position = Position
            }
            .Spawn();

            World.AddRadialImpulse(Position, _explosionRadius, ExplosionForce, true, true);

            PlaySound($"Sounds/weapons/explode{Random.Int(3, 5)}.wav");
        }
    }
}

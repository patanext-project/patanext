using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics.Collidables;
using Box2D.NetStandard.Collision.Shapes;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CPike;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Time.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Pike.AttackCommand
{
	public class WooyariMultiAttack : ScriptBase<WooyariMultiAttackAbilityProvider>
	{
		private IManagedWorldTime          worldTimeManaged;
		private ExecuteActiveAbilitySystem execute;

		private IPhysicsSystem physicsSystem;

		private Entity swingSettings;
		private Entity stabSettings;
		
		public WooyariMultiAttack(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTimeManaged);
			DependencyResolver.Add(() => ref execute);
			DependencyResolver.Add(() => ref physicsSystem);

			swingSettings = World.Mgr.CreateEntity();
			swingSettings.Set<Shape>(new CircleShape {Radius = 7});
			
			stabSettings  = World.Mgr.CreateEntity();
			stabSettings.Set<Shape>(new PolygonShape(3.5f, 0.5f, new Vector2(4, 0), 0));
		}
		
		private Dictionary<WooyariMultiAttackAbility.ECombo, (int buildUpMs, int releaseMs)> timeMap = new()
		{
			{WooyariMultiAttackAbility.ECombo.Stab, (300, 200)},
			{WooyariMultiAttackAbility.ECombo.StabStab, (150, 150)},
			{WooyariMultiAttackAbility.ECombo.StabStabStab, (150, 300)},
			{WooyariMultiAttackAbility.ECombo.Slash, (200, 400)},
			
			{WooyariMultiAttackAbility.ECombo.Swing, (300, 200)},
			{WooyariMultiAttackAbility.ECombo.Spin, (200, 400)},
			{WooyariMultiAttackAbility.ECombo.Dash, (200, 400)},
			
			{WooyariMultiAttackAbility.ECombo.PingPongUp, (200, 150)},
			{WooyariMultiAttackAbility.ECombo.PingPongDown, (200, 200)},
			
			{WooyariMultiAttackAbility.ECombo.Uppercut, (200, 400)},
			{WooyariMultiAttackAbility.ECombo.UppercutThenStab, (200, 400)},
		};

		private WorldTime worldTime;
		private GameTime  gameTime;

		protected override void OnSetup(Span<GameEntityHandle> abilities)
		{
			worldTime = worldTimeManaged.ToStruct();
			
			GameWorld.TryGetSingleton(out gameTime);
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			var inputBuffer = GetBuffer<WooyariMultiAttackQueuedInputs>(self);

			ref var ability = ref GetComponentData<WooyariMultiAttackAbility>(self);
			ability.Cooldown -= worldTime.Delta;

			ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

			ref readonly var position  = ref GetComponentData<Position>(owner).Value;
			ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
			ref readonly var direction = ref GetComponentData<UnitDirection>(owner);

			ref var velocity = ref GetComponentData<Velocity>(owner);

			GameWorld.RemoveComponent(self.Handle, AsComponentType<HitBox>());
			GetBuffer<HitBoxHistory>(self).Clear();

			if (ability.PreviousActivation != state.ActivationVersion)
			{
				ability.PreviousActivation = state.ActivationVersion;
				ability.AttackCount        = 0;
				ability.Combo              = default;
				ability.Current            = default;
				
				inputBuffer.Clear();
			}

			if (inputBuffer.Count + 1 <= ability.MaxAttacksPerCommand
			    && TryGetComponentData(owner, out Relative<PlayerDescription> relativePlayer)
			    && TryGetComponentData(relativePlayer.Target, out GameRhythmInputComponent rhythmInput))
			{
				if (rhythmInput.AbilityInterFrame.HasBeenPressed(gameTime.Frame))
					inputBuffer.Add(new WooyariMultiAttackQueuedInputs(convertInput(rhythmInput.Ability)));
			}

			// Ok, if we're not active or not chaining, stop there.
			// At least we saved inputs in the buffer before.
			if (!state.IsActiveOrChaining)
			{
				ability.StopAttack();
				return;
			}

			if (ability.IsAttackingAndUpdate(worldTime.Total))
			{
				if (velocity.Value.Y <= 0)
					controlVelocity.StayAtCurrentPositionX(1);

				if (ability.CanAttackThisFrame(in worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
				{
					// attack code
					AddComponent(self, new HitBox(owner, default));
					GetComponentData<Position>(self).Value       = position + new Vector3(0, 1, 0);
					GetComponentData<DamageFrameData>(self)      = new DamageFrameData(GetComponentData<UnitPlayState>(owner));
					GetComponentData<HitBoxAgainstEnemies>(self) = new HitBoxAgainstEnemies(GetComponentData<Relative<TeamDescription>>(owner).Target);

					switch (ability.Current)
					{
						case WooyariMultiAttackAbility.EAttackType.None:
							break;
						case WooyariMultiAttackAbility.EAttackType.Uppercut:
							var jump = 8f;
							if (ability.Combo == WooyariMultiAttackAbility.ECombo.Slash)
								jump += 2;

							velocity.Value.Y = Math.Min(Math.Max(velocity.Value.Y, 12), velocity.Value.Y + jump);
							break;
						case WooyariMultiAttackAbility.EAttackType.Stab:
							if (ability.Combo == WooyariMultiAttackAbility.ECombo.Dash)
							{
								controlVelocity.IsActive = false;

								velocity.Value.X += 8;
								velocity.Value.Y += 3.25f;
							}

							break;
						case WooyariMultiAttackAbility.EAttackType.Swing:
							if (ability.Combo == WooyariMultiAttackAbility.ECombo.None)
							{
								controlVelocity.IsActive = false;

								velocity.Value.X =  -5;
								velocity.Value.Y -= 10;

								if (TryGetComponentData(owner, out GroundState groundState)
								    && groundState.Value)
									velocity.Value.Y = Math.Max(velocity.Value.Y, 2.5f);

							}

							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					switch (ability.Current)
					{
						case WooyariMultiAttackAbility.EAttackType.Swing:
						case WooyariMultiAttackAbility.EAttackType.Uppercut:
							physicsSystem.AssignCollider(self.Handle, swingSettings);
							break;
						case WooyariMultiAttackAbility.EAttackType.Stab:
							physicsSystem.AssignCollider(self.Handle, stabSettings);
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(ability.Current), "couldn't attack!");
					}
				}
			}
			else if (state.IsChaining)
			{
				controlVelocity.StayAtCurrentPositionX(10);
			}

			ability.Next = default;
			if (inputBuffer.Count > 0)
				ability.Next = inputBuffer[0].Type;

			var (enemyPrioritySelf, _) = GetNearestEnemy(owner.Handle, 4, null);
			if (state.IsActive && enemyPrioritySelf != default
			                   && ability.AttackStart == TimeSpan.Zero)
			{
				var targetPosition = GetComponentData<Position>(enemyPrioritySelf).Value;
				controlVelocity.SetAbsolutePositionX(targetPosition.X, 50);
				controlVelocity.OffsetFactor = 0;

				if (ability.Next != default && ability.AttackCount + 1 <= ability.MaxAttacksPerCommand
				                            && ability.TriggerAttack(worldTime))
				{
					inputBuffer.RemoveAt(0);
					ability.AttackCount++;

					var previous = ability.Current;
					ability.Current = ability.Next;

					ability.Combo = ability.Combo switch
					{
						WooyariMultiAttackAbility.ECombo.Stab => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Stab => WooyariMultiAttackAbility.ECombo.StabStab,
							WooyariMultiAttackAbility.EAttackType.Uppercut => WooyariMultiAttackAbility.ECombo.Slash,
							_ => WooyariMultiAttackAbility.ECombo.None
						},
						WooyariMultiAttackAbility.ECombo.StabStab => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Stab => WooyariMultiAttackAbility.ECombo.StabStabStab,
							_ => WooyariMultiAttackAbility.ECombo.None
						},
						WooyariMultiAttackAbility.ECombo.StabStabStab => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Stab => WooyariMultiAttackAbility.ECombo.Stab,
						},
						WooyariMultiAttackAbility.ECombo.Slash => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Swing => WooyariMultiAttackAbility.ECombo.PingPongDown,
							_ => WooyariMultiAttackAbility.ECombo.None
						},
						WooyariMultiAttackAbility.ECombo.Swing => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Swing => WooyariMultiAttackAbility.ECombo.Spin,
							WooyariMultiAttackAbility.EAttackType.Uppercut => WooyariMultiAttackAbility.ECombo.PingPongUp,
							WooyariMultiAttackAbility.EAttackType.Stab => WooyariMultiAttackAbility.ECombo.Dash,
							_ => WooyariMultiAttackAbility.ECombo.None
						},
						WooyariMultiAttackAbility.ECombo.Spin => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Swing => WooyariMultiAttackAbility.ECombo.Spin,
							_ => WooyariMultiAttackAbility.ECombo.None
						},
						WooyariMultiAttackAbility.ECombo.PingPongUp => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Swing => WooyariMultiAttackAbility.ECombo.PingPongDown,

							_ => WooyariMultiAttackAbility.ECombo.None
						},
						WooyariMultiAttackAbility.ECombo.PingPongDown => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Uppercut => WooyariMultiAttackAbility.ECombo.PingPongUp,

							// go to Dash combo after a pingpong swing
							WooyariMultiAttackAbility.EAttackType.Stab => WooyariMultiAttackAbility.ECombo.Dash,
							
							_ => WooyariMultiAttackAbility.ECombo.None
						},
						WooyariMultiAttackAbility.ECombo.Uppercut => ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Swing => WooyariMultiAttackAbility.ECombo.PingPongDown,
							WooyariMultiAttackAbility.EAttackType.Stab => WooyariMultiAttackAbility.ECombo.UppercutThenStab,
							_ => WooyariMultiAttackAbility.ECombo.None
						},
						_ => WooyariMultiAttackAbility.ECombo.None
					};

					if (ability.Combo == WooyariMultiAttackAbility.ECombo.None)
					{
						ability.Combo = ability.Current switch
						{
							WooyariMultiAttackAbility.EAttackType.Uppercut => WooyariMultiAttackAbility.ECombo.Uppercut,
							WooyariMultiAttackAbility.EAttackType.Stab => WooyariMultiAttackAbility.ECombo.Stab,
							WooyariMultiAttackAbility.EAttackType.Swing => WooyariMultiAttackAbility.ECombo.Swing,
							_ => throw new ArgumentOutOfRangeException(nameof(ability.Current), "Couldn't find a correct default combo.")
						};
					}
					
					ability.DelayBeforeAttack = TimeSpan.FromMilliseconds(timeMap[ability.Combo].buildUpMs);
					ability.PauseAfterAttack  = TimeSpan.FromMilliseconds(timeMap[ability.Combo].releaseMs);
				}
			}
		}

		private WooyariMultiAttackAbility.EAttackType convertInput(AbilitySelection selection)
		{
			return selection switch
			{
				AbilitySelection.Horizontal => WooyariMultiAttackAbility.EAttackType.Stab,
				AbilitySelection.Top => WooyariMultiAttackAbility.EAttackType.Uppercut,
				AbilitySelection.Bottom => WooyariMultiAttackAbility.EAttackType.Swing,
				_ => throw new NotImplementedException(selection.ToString())
			};	
		}
	}
}
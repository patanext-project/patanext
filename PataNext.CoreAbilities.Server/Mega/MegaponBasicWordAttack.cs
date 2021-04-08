using System;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.CoreAbilities.Mixed;
using PataNext.CoreAbilities.Mixed.CMega;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server.Mega
{
	public class MegaponBasicWordAttack : ScriptBase<MegaponBasicWordAttackAbilityProvider>
	{
		private IManagedWorldTime          worldTime;
		private BouncingProjectileProvider projectileProvider;
		private ExecuteActiveAbilitySystem execute;

		private GameResourceDb<GameGraphicResource> graphicDb;
		private ResPathGen                          resPathGen;

		public MegaponBasicWordAttack(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
			DependencyResolver.Add(() => ref projectileProvider);
			DependencyResolver.Add(() => ref execute);
			DependencyResolver.Add(() => ref graphicDb);
			DependencyResolver.Add(() => ref resPathGen);
		}

		private GameResource<GameGraphicResource>[] graphics;

		private float  dt;
		private Random random = new Random(Environment.TickCount);

		protected override void OnSetup(Span<GameEntityHandle> abilities)
		{
			if (graphics == default)
			{
				graphics = new[]
				{
					graphicDb.GetOrCreate(resPathGen.Create(new[] {"Models", "InGame", "Projectiles", "ElementalWord", "ProjectileTo"}, ResPath.EType.ClientResource)),
					graphicDb.GetOrCreate(resPathGen.Create(new[] {"Models", "InGame", "Projectiles", "ElementalWord", "ProjectileRe"}, ResPath.EType.ClientResource)),
					graphicDb.GetOrCreate(resPathGen.Create(new[] {"Models", "InGame", "Projectiles", "ElementalWord", "ProjectileSo"}, ResPath.EType.ClientResource))
				};
			}
			
			dt = (float) worldTime.Delta.TotalSeconds;
			random.Next();
		}

		protected override void OnExecute(GameEntity owner, GameEntity self, ref AbilityState state)
		{
			ref var abilitySettings = ref GetComponentData<MegaponBasicWordAttackAbility>(self);
			ref var abilityState    = ref GetComponentData<MegaponBasicWordAttackAbility.State>(self);
			abilityState.Cooldown -= worldTime.Delta;

			ref var controlVelocity = ref GetComponentData<AbilityControlVelocity>(self);

			ref readonly var position  = ref GetComponentData<Position>(owner).Value;
			ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);
			ref readonly var direction = ref GetComponentData<UnitDirection>(owner);
			ref readonly var offset    = ref GetComponentData<UnitTargetOffset>(owner);

			var throwOffset = new Vector2(direction.Value, 2.5f);

			if (!state.IsActiveOrChaining)
			{
				abilityState.StopAttack();
				return;
			}

			// If true, we're currently in the attack phase
			if (abilityState.IsAttackingAndUpdate(abilitySettings, worldTime.Total))
			{
				// When we attack, we should stay at our current position, and add a very small deceleration 
				controlVelocity.StayAtCurrentPositionX(1);

				if (abilityState.CanAttackThisFrame(abilitySettings, worldTime.Total, TimeSpan.FromSeconds(playState.AttackSpeed)))
				{
					const int maxBounces = 6;

					// Todo: accuracy should be stored in UnitPlayState (so we could have spears/skills that would be more precise)
					var accuracy       = AbilityUtility.CompileStat(GetComponentData<AbilityEngineSet>(self), 0.5f, 1, 2f, 2f);
					var throwOffsetXYZ = new Vector3(throwOffset, 0);
					for (var i = 0; i < 3; i++)
					{
						execute.Post.Schedule(projectileProvider.SpawnAndForget, new BouncingProjectileCreate
						{
							Owner = owner,
							Pos = position + throwOffsetXYZ + new Vector3
							{
								X = i switch
								{
									0 => -0.1f,
									1 => 1f,
									_ => 0f
								},
								Y = i switch
								{
									0 => 0.4f,
									1 => 0.0f,
									_ => -0.4f
								}
							},
							Vel = new Vector3
							{
								X = (abilitySettings.ThrowVelocity.X - (accuracy * (float) random.NextDouble() * 0.2f) + i * 0.5f) * direction.Value,
								Y = abilitySettings.ThrowVelocity.Y + accuracy * (float) random.NextDouble() - 0.25f
							},
							Gravity    = new Vector3(abilitySettings.Gravity, 0),
							MaxBounces = maxBounces,
							Duration   = TimeSpan.FromSeconds(2.25f),
							Reflection = new Vector3(0.8f, 0.5f, 0),
							IsBox      = false,
							Graphic    = graphics[i]
						}, default);
					}
				}
			}
			else if (state.IsChaining)
				controlVelocity.StayAtCurrentPositionX(50);

			var (enemyPrioritySelf, _) = GetNearestEnemy(owner.Handle, 4, null);
			if (state.IsActive && enemyPrioritySelf != default)
			{
				var targetPosition = GetComponentData<Position>(enemyPrioritySelf).Value;
				var deltaPosition  = PredictTrajectory.Simple(throwOffset - Vector2.UnitX * offset.Attack, abilitySettings.ThrowVelocity * direction.FactorX, abilitySettings.Gravity);
				// Search for any weakpoint the enemy has, and if it does, add it to the deltaPosition var
				if (TryGetComponentBuffer<UnitWeakPoint>(enemyPrioritySelf, out var weakPoints)
				    && weakPoints.GetNearest(targetPosition - position) is var (weakPoint, dist) && dist >= 0)
					deltaPosition += weakPoint.XY();

				targetPosition.X -= deltaPosition.X;

				if (abilityState.AttackStart == default)
					controlVelocity.SetAbsolutePositionX(targetPosition.X, 50);

				// We should have a mercy in the distance of where the unit is and where it should throw. (it shouldn't be able to only throw at a perfect position)
				const float distanceMercy = 4f;
				// If we're near enough of where we should throw the spear, throw it.
				if (MathF.Abs(targetPosition.X - position.X) < distanceMercy && abilityState.TriggerAttack(worldTime.ToStruct()))
				{
				}

				controlVelocity.OffsetFactor = 0;
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			projectileProvider = null;
		}
	}
}
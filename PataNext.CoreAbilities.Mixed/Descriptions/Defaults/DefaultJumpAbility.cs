using System;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using JetBrains.Annotations;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.CoreAbilities.Mixed.Defaults
{
	public struct DefaultJumpAbility : IComponentData
	{
		public int LastActiveId;

		public bool  IsJumping;
		public float ActiveTime;

		public class Register : RegisterGameHostComponentData<DefaultJumpAbility>
		{
		}

		public class Serializer : ArchetypeOnlySerializerBase<DefaultJumpAbility>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}

	public class DefaultJumpAbilityProvider : BaseRuntimeRhythmAbilityProvider<DefaultJumpAbility>
	{
		public DefaultJumpAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "jump";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<JumpCommand>();
		}
	}

	public class DefaultJumpAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;

		public DefaultJumpAbilitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery abilityQuery;

		/*public override void OnAbilityUpdate()
		{
			var dt = (float) worldTime.Delta.TotalSeconds;

			foreach (var entity in (abilityQuery ??= CreateEntityQuery(new[]
			{
				typeof(DefaultJumpAbility),
				typeof(AbilityState)
			})))
			{
				ref var          ability   = ref GetComponentData<DefaultJumpAbility>(entity);
				ref readonly var state     = ref GetComponentData<AbilityState>(entity);
				ref readonly var owner     = ref GetComponentData<Owner>(entity).Target;
				ref readonly var playState = ref GetComponentData<UnitPlayState>(owner);

				if (state.ActivationVersion != ability.LastActiveId)
				{
					ability.IsJumping    = false;
					ability.ActiveTime   = 0;
					ability.LastActiveId = state.ActivationVersion;
				}

				ref var velocity = ref GetComponentData<Velocity>(owner).Value;
				if (!state.IsActiveOrChaining)
				{
					if (ability.IsJumping)
					{
						velocity.Y = Math.Max(0, velocity.Y - 60 * (ability.ActiveTime * 2));
					}

					ability.ActiveTime = 0;
					ability.IsJumping  = false;
					continue;
				}

				const float startJumpTime = 0.5f;

				var wasJumping = ability.IsJumping;
				ability.IsJumping = ability.ActiveTime <= startJumpTime;

				if (!wasJumping && ability.IsJumping)
					velocity.Y = Math.Max(velocity.Y + 25, 30);
				else if (ability.IsJumping && velocity.Y > 0)
					velocity.Y = Math.Max(velocity.Y - 60 * dt, 0);

				if (ability.ActiveTime < 3.25f)
					velocity.X = MathUtils.LerpNormalized(velocity.X, 0, dt * (ability.ActiveTime + 1) * Math.Max(0, 1 + playState.Weight * 0.1f));

				if (!ability.IsJumping && velocity.Y > 0)
				{
					velocity.Y = Math.Max(velocity.Y - 10 * dt, 0);
					velocity.Y = MathUtils.LerpNormalized(velocity.Y, 0, 5 * dt);
				}

				ability.ActiveTime += dt;

				ref var unitController = ref GetComponentData<UnitControllerState>(owner);
				unitController.ControlOverVelocityX = ability.ActiveTime < 3.25f;
				unitController.ControlOverVelocityY = ability.ActiveTime < 2.5f;
			}
		}*/
		public override void OnAbilityUpdate()
		{
			
		}
	}
}
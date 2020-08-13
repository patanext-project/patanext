using System;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay;
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.Simulation.Mixed.Abilities.CTate
{
	public struct TaterazayBasicDefendStayAbility : IComponentData
	{
		public class Register : RegisterGameHostComponentData<TaterazayBasicDefendStayAbility>
		{
		}
	}

	public class TaterazayBasicDefendStayAbilityProvider : BaseRhythmAbilityProvider<TaterazayBasicDefendStayAbility>
	{
		protected override string FilePathPrefix => "tate";

		public TaterazayBasicDefendStayAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "CTate.BasicDefendStay";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<DefendCommand>();
		}
	}

	public class TaterazayBasicDefendStayAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;

		public TaterazayBasicDefendStayAbilitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery abilityQuery;

		public override void OnAbilityUpdate()
		{
			var dt = (float) worldTime.Delta.TotalSeconds;
			
			var abilityStateAccessor    = new ComponentDataAccessor<AbilityState>(GameWorld);
			var ownerAccessor           = new ComponentDataAccessor<Owner>(GameWorld);
			var playStateAccessor       = new ComponentDataAccessor<UnitPlayState>(GameWorld);
			var controllerStateAccessor = new ComponentDataAccessor<UnitControllerState>(GameWorld);
			var velocityAccessor        = new ComponentDataAccessor<Velocity>(GameWorld);
			foreach (var entity in (abilityQuery ??= CreateEntityQuery(stackalloc[]
			{
				AsComponentType<AbilityState>(),
				AsComponentType<TaterazayBasicDefendStayAbility>(),
				AsComponentType<Owner>()
			})).GetEntities())
			{
				ref readonly var state = ref abilityStateAccessor[entity];
				if (!state.IsActiveOrChaining)
					continue;

				ref readonly var owner          = ref ownerAccessor[entity].Target;
				ref readonly var playState      = ref playStateAccessor[owner];
				ref var          unitController = ref controllerStateAccessor[owner];
				ref var          velocity       = ref velocityAccessor[owner].Value;
				if (!state.IsActive)
				{
					if (state.IsChaining)
					{
						unitController.ControlOverVelocityX = true;
						velocity.X                          = MathHelper.LerpNormalized(velocity.X, 0, playState.GetAcceleration() * 50 * dt);
					}

					continue;
				}

				ref readonly var targetEntity = ref GetComponentData<Relative<UnitTargetDescription>>(owner).Target;
				ref readonly var unitPosition = ref GetComponentData<Position>(owner).Value;

				var targetPosition = GetComponentData<Position>(targetEntity).Value.X;
				velocity.X = AbilityUtility.GetTargetVelocityX(new AbilityUtility.GetTargetVelocityParameters
				{
					TargetPosition   = new Vector3(targetPosition, 0, 0),
					PreviousPosition = unitPosition,
					PreviousVelocity = velocity,
					PlayState        = playState,
					Acceleration     = 5,
					Delta            = dt
				});
				unitController.ControlOverVelocityX = true;
			}
		}
	}
}
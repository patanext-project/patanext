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
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Simulation.Mixed.Abilities.CTate
{
	public struct TaterazayBasicDefendFrontalAbility : IComponentData
	{
		public float Range;

		public class Register : RegisterGameHostComponentData<TaterazayBasicDefendFrontalAbility>
		{
		}
	}

	public class TaterazayBasicDefendFrontalAbilityProvider : BaseRhythmAbilityProvider<TaterazayBasicDefendFrontalAbility>
	{
		protected override string FilePathPrefix => "tate";

		public TaterazayBasicDefendFrontalAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "CTate.BasicDefendFrontal";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<DefendCommand>();
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			GameWorld.GetComponentData<TaterazayBasicDefendFrontalAbility>(entity).Range = 10;
		}
	}

	public class TaterazayBasicDefendFrontalAbilitySystem : BaseAbilitySystem
	{
		private IManagedWorldTime worldTime;

		public TaterazayBasicDefendFrontalAbilitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery abilityQuery;

		public override void OnAbilityUpdate()
		{
			var abilityAccessor         = new ComponentDataAccessor<TaterazayBasicDefendFrontalAbility>(GameWorld);
			var abilityStateAccessor    = new ComponentDataAccessor<AbilityState>(GameWorld);
			var controlVelocityAccessor = new ComponentDataAccessor<AbilityControlVelocity>(GameWorld);
			foreach (var entity in (abilityQuery ??= CreateEntityQuery(stackalloc[]
			{
				AsComponentType<AbilityState>(),
				AsComponentType<TaterazayBasicDefendFrontalAbility>(),
				AsComponentType<Owner>()
			})).GetEntities())
			{
				ref readonly var state = ref abilityStateAccessor[entity];
				if (!state.IsActiveOrChaining)
					continue;

				ref readonly var ability = ref abilityAccessor[entity];
				ref var          control = ref controlVelocityAccessor[entity];
				if (!state.IsActive)
				{
					if (state.IsChaining)
					{
						control.StayAtCurrentPositionX(5);
					}

					continue;
				}

				control.SetTargetPositionX(ability.Range, 50, 0.75f);
			}
		}
	}
}
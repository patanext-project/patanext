using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.CoreAbilities.Mixed.CTate
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

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
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
			})))
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

				control.SetCursorPositionX(ability.Range, 50, 0.75f);
			}
		}
	}
}
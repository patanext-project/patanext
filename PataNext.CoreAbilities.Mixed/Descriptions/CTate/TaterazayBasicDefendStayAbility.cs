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
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.CoreAbilities.Mixed.CTate
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
		public override    string MasterServerId => resPath.Create(new[] {"ability", "tate", "def_defstay"}, ResPath.EType.MasterServer);

		public TaterazayBasicDefendStayAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

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
			var abilityStateAccessor    = new ComponentDataAccessor<AbilityState>(GameWorld);
			var controlVelocityAccessor = new ComponentDataAccessor<AbilityControlVelocity>(GameWorld);
			foreach (var entity in (abilityQuery ??= CreateEntityQuery(stackalloc[]
			{
				AsComponentType<AbilityState>(),
				AsComponentType<TaterazayBasicDefendStayAbility>(),
				AsComponentType<Owner>()
			})))
			{
				if (!GetComponentData<GroundState>(GetComponentData<Owner>(entity).Target).Value)
					continue;
					
				ref readonly var state = ref abilityStateAccessor[entity];
				if (!state.IsActiveOrChaining)
					continue;

				ref var control = ref controlVelocityAccessor[entity];
				if (!state.IsActive)
				{
					if (state.IsChaining)
					{
						control.StayAtCurrentPositionX(5);
					}

					continue;
				}

				control.ResetPositionX(50);
			}
		}
	}
}
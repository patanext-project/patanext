using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Passes;
using PataNext.Simulation.mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Abilities
{
	public class ExecuteActiveAbilitySystem : GameAppSystem, IAbilitySimulationPass
	{
		public readonly IScheduler Post;

		public ExecuteActiveAbilitySystem(WorldCollection collection) : base(collection)
		{
			Post = new Scheduler();
		}

		private EntityQuery abilityMaskQuery;
		private EntityQuery setupQuery;
		private EntityQuery executorQuery;

		public void OnAbilitySimulationPass()
		{
			(abilityMaskQuery ??= CreateEntityQuery(new[]
			{
				typeof(AbilityState),
				typeof(ExecutableAbility)
			})).CheckForNewArchetypes();

			var setupAccessor = GetAccessor<SetupExecutableAbility>();
			foreach (var entity in setupQuery ??= CreateEntityQuery(new[]
			{
				typeof(SetupExecutableAbility)
			}))
			{
				setupAccessor[entity].Function(Safe(entity));
			}

			var ownerActiveAbilityAccessor = GetAccessor<OwnerActiveAbility>();
			var executableAccessor         = GetAccessor<ExecutableAbility>();
			foreach (var entity in executorQuery ??= CreateEntityQuery(new[]
			{
				typeof(OwnerActiveAbility)
			}))
			{
				ref readonly var ownerActiveAbility = ref ownerActiveAbilityAccessor[entity];

				// Don't execute duplicate abilities
				if (ownerActiveAbility.PreviousActive != ownerActiveAbility.Active)
				{
					TryInvoke(entity, ownerActiveAbility.PreviousActive, in executableAccessor);
					TryInvoke(entity, ownerActiveAbility.Active, in executableAccessor);
					if (ownerActiveAbility.PreviousActive != ownerActiveAbility.Incoming
					    && ownerActiveAbility.Active != ownerActiveAbility.Incoming)
						TryInvoke(entity, ownerActiveAbility.Incoming, in executableAccessor);
				}
				else
				{
					TryInvoke(entity, ownerActiveAbility.Active, in executableAccessor);
					if (ownerActiveAbility.Active != ownerActiveAbility.Incoming)
						TryInvoke(entity, ownerActiveAbility.Incoming, in executableAccessor);
				}
			}

			Post.Run();
		}

		private void TryInvoke(GameEntityHandle owner, GameEntity ability, in ComponentDataAccessor<ExecutableAbility> accessor)
		{
			if (abilityMaskQuery.MatchAgainst(ability.Handle))
				accessor[ability.Handle].Function?.Invoke(Safe(owner), ability, ref GetComponentData<AbilityState>(ability.Handle));
		}
	}
}
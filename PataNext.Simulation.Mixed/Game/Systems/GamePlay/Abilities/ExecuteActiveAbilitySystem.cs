using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Passes;
using PataNext.Simulation.mixed.Components.GamePlay.Abilities;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Utility.Misc.EntitySystem;

namespace PataNext.Module.Simulation.Game.GamePlay.Abilities
{
	public class ExecuteActiveAbilitySystem : GameAppSystem, IAbilitySimulationPass
	{
		public readonly IScheduler Post;

		private IBatchRunner runner;

		public ExecuteActiveAbilitySystem(WorldCollection collection) : base(collection)
		{
			Post = new Scheduler();
			
			DependencyResolver.Add(() => ref runner);
		}

		private EntityQuery abilityMaskQuery;
		private EntityQuery setupQuery;
		private EntityQuery executorQuery;

		private ArchetypeSystem<byte> foreachSystem;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			foreachSystem = new ArchetypeSystem<byte>((in ReadOnlySpan<GameEntityHandle> entities, in SystemState<byte> state) =>
			{
				var ownerActiveAbilityAccessor = GetAccessor<OwnerActiveAbility>();
				var executableAccessor         = GetAccessor<ExecutableAbility>();
				foreach (ref readonly var entity in entities)
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
			}, executorQuery = CreateEntityQuery(new[]
			{
				typeof(OwnerActiveAbility),
				typeof(SimulationAuthority)
			}));
		}

		public void OnAbilitySimulationPass()
		{
			var setupAccessor = GetAccessor<SetupExecutableAbility>();
			foreach (var entity in setupQuery ??= CreateEntityQuery(new[]
			{
				typeof(SetupExecutableAbility)
			}))
			{
				setupAccessor[entity].Function(Safe(entity));
			}
			
			// Make sure that this mask is being updated after setups are called.
			// It's possible that an ability may change its archetype, and this could throw exceptions in EntityQuery.MatchAgainst
			// if it this section was put on the top.
			(abilityMaskQuery ??= CreateEntityQuery(new[]
			{
				typeof(AbilityState),
				typeof(ExecutableAbility)
			})).CheckForNewArchetypes();

			runner.WaitForCompletion(runner.Queue(foreachSystem));

			Post.Run();
		}

		private void TryInvoke(GameEntityHandle owner, GameEntity ability, in ComponentDataAccessor<ExecutableAbility> accessor)
		{
			if (abilityMaskQuery.MatchAgainst(ability.Handle))
				accessor[ability.Handle].Function?.Invoke(Safe(owner), ability, ref GetComponentData<AbilityState>(ability.Handle));
		}
	}
}
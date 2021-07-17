using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Special.Ai;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Special.Ai
{
	public class SimpleAiSystem : GameAppSystem, IAbilityPreSimulationPass
	{
		private IManagedWorldTime worldTime;

		public SimpleAiSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery aiQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			aiQuery = CreateEntityQuery(new[] { typeof(SimpleAiActionIndex), typeof(SimpleAiActions), typeof(OwnerActiveAbility) });
		}

		public void OnAbilityPreSimulationPass()
		{
			var total = worldTime.Total;

			var controllerAccessor    = GetAccessor<SimpleAiActionIndex>();
			var actionsAccessor       = GetBufferAccessor<SimpleAiActions>();
			var activeAbilityAccessor = GetAccessor<OwnerActiveAbility>();
			foreach (var entity in aiQuery)
			{
				ref var controller         = ref controllerAccessor[entity];
				ref var ownerActiveAbility = ref activeAbilityAccessor[entity];

				var buffer = actionsAccessor[entity];
				if (buffer.Count == 0)
					continue;

				SimpleAiActions current;
				if (!HasComponent<LivableIsDead>(entity))
				{
					if (controller.TimeBeforeNextAbility == TimeSpan.Zero)
						controller.TimeBeforeNextAbility = total + buffer[controller.Index].Duration;

					if (controller.TimeBeforeNextAbility <= total)
					{
						var previousAbility = buffer[controller.Index].AbilityTarget;
						if (GameWorld.Exists(previousAbility))
						{
							GetComponentData<AbilityState>(previousAbility).Phase = EAbilityPhase.Chaining;
							ownerActiveAbility.PreviousActive                     = previousAbility;
						}

						controller.Index++;
						if (controller.Index >= buffer.Count)
							controller.Index = 0;

						controller.TimeBeforeNextAbility = total + buffer[controller.Index].Duration;
					}
					
					current = buffer[controller.Index];
				}
				else
				{
					current = default;
					var previousAbility = buffer[controller.Index].AbilityTarget;
					if (GameWorld.Exists(previousAbility))
						GetComponentData<AbilityState>(previousAbility).Phase = EAbilityPhase.None;
				}
				
				switch (current.Type)
				{
					case SimpleAiActions.EType.Wait:
						break;

					case SimpleAiActions.EType.Ability:
						ownerActiveAbility.Active                                   = current.AbilityTarget;
						GetComponentData<AbilityState>(current.AbilityTarget).Phase = EAbilityPhase.Active;
						break;
				}
			}
		}
	}
}
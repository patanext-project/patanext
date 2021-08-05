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

				var update = false;
				
				SimpleAiActions current;
				if (!HasComponent<LivableIsDead>(entity))
				{
					if (controller.TimeBeforeNextAbility == TimeSpan.Zero)
						controller.TimeBeforeNextAbility = total + buffer[controller.Index].Duration;

					if (controller.TimeBeforeNextAbility <= total)
					{
						var previousAbility = buffer[controller.Index].AbilityTarget;
						if (GameWorld.Exists(controller.PreviousAbility))
						{
							GetComponentData<AbilityState>(controller.PreviousAbility).Phase = EAbilityPhase.None;
						}
						controller.PreviousAbility = previousAbility;
						
						controller.Index++;
						if (controller.Index >= buffer.Count)
							controller.Index = 0;
						
						controller.TimeBeforeNextAbility = total + buffer[controller.Index].Duration;
						update                           = true;
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
						if (GameWorld.Exists(controller.PreviousAbility))
						{
							GetComponentData<AbilityState>(controller.PreviousAbility).Phase = EAbilityPhase.Chaining;
							ownerActiveAbility.PreviousActive                                = controller.PreviousAbility;
						}

						break;

					case SimpleAiActions.EType.Ability:
						ownerActiveAbility.Active                                   = current.AbilityTarget;

						ref var abilityState = ref GetComponentData<AbilityState>(current.AbilityTarget);
						if ((GetComponentData<AbilityActivation>(current.AbilityTarget).Type & EAbilityActivationType.HeroMode) != 0)
						{
							if (controller.TimeBeforeNextAbility - total > current.Duration - TimeSpan.FromMilliseconds(500))
								abilityState.Phase = EAbilityPhase.HeroActivation;
							else
								abilityState.Phase = EAbilityPhase.Active;
						}
						else
							abilityState.Phase = EAbilityPhase.Active;

						if (update)
						{
							abilityState.ActivationVersion++;
							abilityState.UpdateVersion++;
						}

						if (GameWorld.Exists(controller.PreviousAbility))
						{
							GetComponentData<AbilityState>(controller.PreviousAbility).Phase = EAbilityPhase.None;
							ownerActiveAbility.PreviousActive                                = controller.PreviousAbility;
						}

						break;
				}
				
				if (buffer.Count > 2 && update)
				{
					var str = $"Actions: <Update {update}>\n";
					for (var index = 0; index < buffer.Count; index++)
					{
						var ac = buffer[index];
						str += $"  [{ac.Type}] <Duration {ac.Duration.TotalMilliseconds}ms>";
						if (ac.Type == SimpleAiActions.EType.Ability)
						{
							str += $" <Target {ac.AbilityTarget}> <Phase {GetComponentData<AbilityState>(ac.AbilityTarget).Phase}>";
							if (controller.PreviousAbility == ac.AbilityTarget)
								str += " <Previous>";
							else if (controller.Index == index)
								str += " <Current>";
						}

						str += "\n";
					}

					Console.WriteLine(str);
				}
			}
		}
	}
}
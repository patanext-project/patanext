using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Special.Ai;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.CoreMissions.Server.Providers
{
	public class SimpleAiBotUnitProvider : BaseProvider<SimpleAiBotUnitProvider.Create>
	{
		public struct Create
		{
			public BotUnitProvider.Create Parent;

			public (TimeSpan duration, Func<GameEntityHandle, GameEntityHandle> generateAbility)[] Actions;
		}

		private BotUnitProvider parent;

		public SimpleAiBotUnitProvider([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref parent);
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			parent.GetComponents(entityComponents);

			entityComponents.AddRange(new[]
			{
				AsComponentType<SimpleAiActionIndex>(),
				AsComponentType<SimpleAiActions>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			parent.SetEntityData(entity, data.Parent);

			if (data.Actions == null)
				return;
			
			var buffer = GetBuffer<SimpleAiActions>(entity);
			foreach (var (duration, generateAbility) in data.Actions)
			{
				var action = new SimpleAiActions();

				var abilityHandle = generateAbility == null ? default : generateAbility(entity);
				if (abilityHandle == default)
					action.SetWait(duration);
				else
					action.SetAbility(Safe(abilityHandle), duration);

				buffer.Add(action);
			}
		}
	}
}
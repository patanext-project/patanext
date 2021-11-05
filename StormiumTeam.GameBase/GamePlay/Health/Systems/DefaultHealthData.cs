using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.GamePlay.Health.Systems.Base;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.GamePlay.Health.Systems
{
	public struct DefaultHealthData : IComponentData
	{
		public int Value;
		public int Max;
	}

	public class DefaultHealthProcess : HealthProcessSystemBase<DefaultHealthData>
	{
		public DefaultHealthProcess(WorldCollection collection) : base(collection)
		{
		}

		protected override ConcreteHealthValue OnExecute(GameEntityHandle healthEntity, in GameEntity owner, Span<ModifyHealthEvent> healthEvents, ref DefaultHealthData data)
		{
			foreach (ref var healthEvent in healthEvents)
			{
				if (healthEvent.Target != owner)
					continue;

				var difference = data.Value;
				switch (healthEvent.Type)
				{
					case ModifyHealthType.Add:
						data.Value = Math.Clamp(data.Value + healthEvent.Consumed, 0, data.Max);
						break;
					case ModifyHealthType.SetFixed:
						Math.Clamp(healthEvent.Consumed, 0, data.Max);
						break;
					case ModifyHealthType.SetMax:
						data.Value = data.Max;
						break;
					case ModifyHealthType.SetNone:
						data.Value = 0;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				healthEvent.Consumed -= Math.Abs(data.Value - difference);
			}

			return new ConcreteHealthValue
			{
				Value = data.Value,
				Max   = data.Max
			};
		}
	}

	public class DefaultHealthProvider : BaseProvider<DefaultHealthProvider.Create>
	{
		public struct Create
		{
			public int        value, max;
			public GameEntity owner;
		}

		public DefaultHealthProvider(WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				GameWorld.AsComponentType<HealthDescription>(),
				GameWorld.AsComponentType<DefaultHealthData>(),
				GameWorld.AsComponentType<ConcreteHealthValue>(),
				GameWorld.AsComponentType<Owner>(),
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			GameWorld.GetComponentData<DefaultHealthData>(entity) = new DefaultHealthData {Value = data.value, Max = data.max};
			GameWorld.GetComponentData<Owner>(entity)             = new Owner(data.owner);

			GameWorld.Link(entity, data.owner.Handle, true);
		}
	}
}
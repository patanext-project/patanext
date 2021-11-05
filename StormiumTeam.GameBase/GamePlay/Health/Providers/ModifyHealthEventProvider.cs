using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.GamePlay.Health.Providers
{
	public class ModifyHealthEventProvider : BaseProvider<ModifyHealthEvent>
	{
		public ModifyHealthEventProvider(WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.Add(GameWorld.AsComponentType<ModifyHealthEvent>());
		}

		public override void SetEntityData(GameEntityHandle entity, ModifyHealthEvent data)
		{
			GameWorld.GetComponentData<ModifyHealthEvent>(entity) = data;
		}
	}
}
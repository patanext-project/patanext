using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Simulation.Utility.Resource.Systems;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.Units
{
	public readonly struct UnitArchetype : IComponentData
	{
		public readonly GameResource<UnitArchetypeResource> Resource;

		public UnitArchetype(GameResource<UnitArchetypeResource> id)
		{
			Resource = id;
		}

		public class KeepArchetype : KeepAliveResourceFromData<UnitArchetypeResource, UnitArchetype>
		{
			public KeepArchetype(WorldCollection collection) : base(collection)
			{
			}

			protected override void KeepAlive(Span<bool> keep, Span<UnitArchetype> self, Span<GameResource<UnitArchetypeResource>> resources)
			{
				for (var i = 0; i != self.Length; i++)
				{
					var idx = resources.IndexOf(self[i].Resource);
					if (idx >= 0)
						keep[idx] = true;
				}
			}
		}

		public bool Equals(UnitArchetype unitArchetype)
		{
			return Resource.Equals(unitArchetype.Resource);
		}
	}
}
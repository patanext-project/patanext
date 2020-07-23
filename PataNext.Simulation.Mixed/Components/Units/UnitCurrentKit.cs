using System;
using GameHost.Core.Ecs;
using GameHost.Native;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Simulation.Utility.Resource.Systems;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.Units
{
	public readonly struct UnitCurrentKit : IComponentData
	{
		public readonly GameResource<IUnitKitResource> Resource;

		public UnitCurrentKit(GameResource<IUnitKitResource> id)
		{
			Resource = id;
		}

		public class KeepKit : KeepAliveResourceFromData<IUnitKitResource, UnitCurrentKit>
		{
			public KeepKit(WorldCollection collection) : base(collection)
			{
			}

			protected override void KeepAlive(Span<bool> keep, Span<UnitCurrentKit> self, Span<GameResource<IUnitKitResource>> resources)
			{
				for (var i = 0; i != self.Length; i++)
				{
					var idx = resources.IndexOf(self[i].Resource);
					if (idx >= 0)
						keep[idx] = true;
				}
			}
		}
	}
}
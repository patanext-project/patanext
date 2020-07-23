using System;
using GameHost.Core.Ecs;
using GameHost.Native;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Simulation.Utility.Resource.Systems;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.Units
{
	public struct UnitDisplayedEquipment : IComponentBuffer
	{
		public GameResource<IUnitAttachmentResource> Attachment;
		public GameResource<IEquipmentResource>      Resource;

		public class Register : RegisterGameHostComponentBuffer<UnitDisplayedEquipment>
		{
		}

		public class KeepAttachment : KeepAliveResourceFromBuffer<IUnitAttachmentResource, UnitDisplayedEquipment>
		{
			public KeepAttachment(WorldCollection collection) : base(collection)
			{
			}

			protected override void KeepAlive(Span<bool> keep, Span<UnitDisplayedEquipment> self, Span<GameResource<IUnitAttachmentResource>> resources)
			{
				for (var i = 0; i != self.Length; i++)
				{
					var idx = resources.IndexOf(self[i].Attachment);
					if (idx >= 0)
						keep[idx] = true;
				}
			}
		}

		public class KeepEquipment : KeepAliveResourceFromBuffer<IEquipmentResource, UnitDisplayedEquipment>
		{
			public KeepEquipment(WorldCollection collection) : base(collection)
			{
			}

			protected override void KeepAlive(Span<bool> keep, Span<UnitDisplayedEquipment> self, Span<GameResource<IEquipmentResource>> resources)
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
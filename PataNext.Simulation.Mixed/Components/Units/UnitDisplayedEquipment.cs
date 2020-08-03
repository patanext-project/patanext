using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using GameHost.Simulation.Utility.Resource.Systems;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.Units
{
	public struct UnitDisplayedEquipment : IComponentBuffer
	{
		public GameResource<UnitAttachmentResource> Attachment;
		public GameResource<EquipmentResource>      Resource;

		public class Register : RegisterGameHostComponentBuffer<UnitDisplayedEquipment>
		{
		}

		public class KeepAttachment : KeepAliveResourceFromBuffer<UnitAttachmentResource, UnitDisplayedEquipment>
		{
			public KeepAttachment(WorldCollection collection) : base(collection)
			{
			}

			protected override void KeepAlive(Span<bool> keep, Span<UnitDisplayedEquipment> self, Span<GameResource<UnitAttachmentResource>> resources)
			{
				for (var i = 0; i != self.Length; i++)
				{
					var idx = resources.IndexOf(self[i].Attachment);
					if (idx >= 0)
						keep[idx] = true;
				}
			}
		}

		public class KeepEquipment : KeepAliveResourceFromBuffer<EquipmentResource, UnitDisplayedEquipment>
		{
			public KeepEquipment(WorldCollection collection) : base(collection)
			{
			}

			protected override void KeepAlive(Span<bool> keep, Span<UnitDisplayedEquipment> self, Span<GameResource<EquipmentResource>> resources)
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
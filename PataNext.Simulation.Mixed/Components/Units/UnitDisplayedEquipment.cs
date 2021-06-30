using System;
using DefaultEcs;
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

		public UnitDisplayedEquipment(GameResource<UnitAttachmentResource> attachment, GameResource<EquipmentResource> resource)
		{
			Attachment = attachment;
			Resource   = resource;
		}

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

	// it's like DisplayedEquipment, but with stats modification
	public struct UnitDefinedEquipments : IComponentBuffer
	{
		public GameResource<UnitAttachmentResource> Attachment;
		public Entity                               Item;

		public UnitDefinedEquipments(GameResource<UnitAttachmentResource> attachment, Entity item)
		{
			Attachment = attachment;
			Item       = item;
		}
	}
}
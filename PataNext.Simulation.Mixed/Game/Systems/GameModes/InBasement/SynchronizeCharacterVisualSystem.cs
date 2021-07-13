using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes.InBasement
{
	public class SynchronizeCharacterVisualSystem : GameAppSystem
	{
		private GameResourceDb<UnitAttachmentResource> attachDb;
		private GameResourceDb<EquipmentResource>      equipDb;
		private ResPathGen resPathGen;

		public SynchronizeCharacterVisualSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref attachDb);
			DependencyResolver.Add(() => ref equipDb);
			DependencyResolver.Add(() => ref resPathGen);
		}

		private EntityQuery characterQuery;
		private EntityQuery validUnitMask;

		private GameResource<UnitAttachmentResource> maskAttachment;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			maskAttachment = attachDb.GetOrCreate(new(resPathGen.Create(new[] { "equip_root", "mask" }, ResPath.EType.MasterServer)));

			characterQuery = CreateEntityQuery(new[] { typeof(SynchronizeCharacterVisual), typeof(UnitArchetype), typeof(UnitDisplayedEquipment), typeof(Relative<PlayerDescription>) });
			validUnitMask  = CreateEntityQuery(new[] { typeof(UnitArchetype), typeof(UnitDisplayedEquipment), typeof(Relative<PlayerDescription>) });
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && characterQuery.Any();
		}

		private Dictionary<GameResource<UnitKitResource>, GameResource<EquipmentResource>> kitToMaskMap = new();

		protected override void OnUpdate()
		{
			base.OnUpdate();
			
			validUnitMask.CheckForNewArchetypes();

			var relativeAccessor           = GetAccessor<Relative<PlayerDescription>>();
			var displayedEquipmentAccessor = GetBufferAccessor<UnitDisplayedEquipment>();
			var archetypeAccessor          = GetAccessor<UnitArchetype>();
			var ownedAccessor              = GetBufferAccessor<OwnedRelative<ArmyUnitDescription>>();
			
			foreach (var character in characterQuery)
			{
				ref readonly var player = ref relativeAccessor[character];

				var characterArchetype  = archetypeAccessor[character];
				var characterEquipments = displayedEquipmentAccessor[character];
				characterEquipments.Clear();
				
				foreach (var unit in validUnitMask)
				{
					if (relativeAccessor[unit].Target != player.Target)
						continue;
					
					if (!archetypeAccessor[unit].Equals(characterArchetype))
						continue;
					
					var unitEquipments = displayedEquipmentAccessor[unit];
					foreach (var elem in unitEquipments)
					{
						var resource = GetComponentData<UnitAttachmentResource>(elem.Attachment.Handle).Value.Span;
						if (resource.IndexOf("helm") >= 0)
						{
							characterEquipments.Add(elem);
						}
						else if (resource.IndexOf("mask") >= 0)
						{
							characterEquipments.Add(elem);
						}
					}
				}

				/*if (!maskFound && TryGetComponentData(character, out UnitCurrentKit kit))
				{
					if (!kitToMaskMap.TryGetValue(kit.Resource, out var mask))
					{
						var resource = GetComponentData<UnitKitResource>(kit.Resource.Handle).Value.Span;
						mask = equipDb.GetOrCreate(new(resPathGen.Create(new[] { "equipment", "mask", resource.ToString() }, ResPath.EType.ClientResource)));

						kitToMaskMap[kit.Resource] = mask;
					}

					characterEquipments.Add(new(maskAttachment, mask));
				}*/
			}
		}
	}
}
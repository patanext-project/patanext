using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using PataNext.Module.Simulation.Components;
using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Network.MasterServer.Systems
{
	public class MasterServerPlayerInventory : PlayerInventoryBase
	{
		private struct ToMasterServerId
		{
			public string Value;
		}
		
		private PooledDictionary<string, string>                                  guidToItemType    = new();
		private PooledDictionary<string, PooledDictionary<string, InventoryItem>> typeToGuidItemMap = new();

		public readonly string SaveId;

		public MasterServerPlayerInventory(string saveId)
		{
			SaveId    = saveId;
		}

		public override void Read<TFill, TRestrict>(TFill list, TRestrict restrictTypes = default)
		{
			if (restrictTypes?.Count > 0)
			{
				foreach (var type in restrictTypes)
					if (typeToGuidItemMap.TryGetValue(type, out var dict))
						foreach (var (_, item) in dict)
							list.Add(item);
				return;
			}

			foreach (var (_, dict) in typeToGuidItemMap)
			foreach (var (_, item) in dict)
				list.Add(item);
		}

		public override InventoryItem GetItemInfo(Entity entity)
		{
			if (entity.TryGet(out ToMasterServerId masterServerId)
			    && guidToItemType.TryGetValue(masterServerId.Value, out var type)
			    && typeToGuidItemMap.TryGetValue(type, out var dict)
			    && dict.TryGetValue(masterServerId.Value, out var inventoryItem))
			{
				return inventoryItem;
			}

			return default;
		}

		public override void Dispose()
		{
			foreach (var (_, ent) in msIdToEntity)
				ent.Dispose();
			msIdToEntity.Dispose();
			guidToItemType.Dispose();
			typeToGuidItemMap.Dispose();
		}

		internal PooledDictionary<string, Entity> msIdToEntity = new();

		internal void setMasterServerItems(Func<string, Entity> create, string[] itemGuids)
		{
			foreach (var guid in itemGuids)
				if (!msIdToEntity.ContainsKey(guid))
					msIdToEntity.Add(guid, create(guid));

			using var scheduler = new Scheduler();
			foreach (var (guid, ent) in msIdToEntity)
			{
				if (itemGuids.AsSpan().Contains(guid))
					continue;

				ent.Dispose();
				scheduler.Schedule(args =>
				{
					args.map.Remove(args.key);
					if (args.guidToItemType.TryGetValue(args.key, out var type))
					{
						args.guidToItemType.Remove(args.key);
						args.typeToGuidItemMap[type].Remove(args.key);
					}
				}, (map: msIdToEntity, key: guid, guidToItemType, typeToGuidItemMap), default);
			}

			scheduler.Run();
		}

		internal void setKnownItem(string guid, InventoryItem item)
		{
			if (!guidToItemType.TryGetValue(guid, out var previousType)
			    || item.TypeId.FullString != previousType)
			{
				guidToItemType[guid] = item.TypeId.FullString;
			}

			if (!typeToGuidItemMap.TryGetValue(item.TypeId.FullString, out var dict))
				typeToGuidItemMap[item.TypeId.FullString] = dict = new();

			dict[guid] = item;
		}
	}
}
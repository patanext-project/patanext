using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Threading;
using PataNext.Module.Simulation.Components;
using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Network.MasterServer.Systems
{
	public class MasterServerPlayerInventory : PlayerInventoryBase
	{
		private PooledDictionary<string, string>                                  guidToItemType    = new();
		private PooledDictionary<string, PooledDictionary<string, InventoryItem>> typeToGuidItemMap = new();

		public readonly string SaveId;

		public MasterServerPlayerInventory(string saveId)
		{
			SaveId = saveId;
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
				scheduler.Schedule(args => args.map.Remove(args.key), (map: msIdToEntity, key: guid), default);
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
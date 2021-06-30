using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using PataNext.Game.GameItems;
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

		private PooledDictionary<string, string>                           guidToItemType    = new();
		private PooledDictionary<string, PooledDictionary<string, Entity>> typeToGuidItemMap = new();

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

		public override void GetItemsOfAsset<TFill>(TFill list, Entity assetEntity)
		{
			if (!typeToGuidItemMap.TryGetValue(assetEntity.Get<GameItemDescription>().Type, out var dict))
				return;

			foreach (var (_, item) in dict)
				if (item.Get<TrucItemInventory>().AssetEntity == assetEntity)
					list.Add(item);
		}

		public override Entity GetStack(Entity assetEntity)
		{
			if (!typeToGuidItemMap.TryGetValue(assetEntity.Get<GameItemDescription>().Type, out var dict))
				return default;

			foreach (var (_, item) in dict)
				if (item.Get<TrucItemInventory>().AssetEntity == assetEntity)
					return assetEntity;

			return default;
		}

		public override bool Contains(Entity itemEntity)
		{
			return itemEntity.TryGet(out ToMasterServerId masterServerId)
			       && msIdToEntity.TryGetValue(masterServerId.Value, out var expected)
			       && itemEntity == expected;
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
			{
				if (!msIdToEntity.ContainsKey(guid))
				{
					var entity = create(guid);
					entity.Set(new ToMasterServerId { Value = guid });

					msIdToEntity.Add(guid, entity);
				}
			}

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

		internal Entity setKnownItem(string guid, Entity assetEntity)
		{
			var desc = assetEntity.Get<GameItemDescription>();

			if (!guidToItemType.TryGetValue(guid, out var previousType)
			    || desc.Type != previousType)
			{
				guidToItemType[guid] = desc.Type;
			}

			if (!typeToGuidItemMap.TryGetValue(desc.Type, out var dict))
				typeToGuidItemMap[desc.Type] = dict = new();

			var entity = msIdToEntity[guid];
			entity.Set(new TrucItemInventory { AssetEntity = assetEntity });

			dict[guid] = entity;
			return entity;
		}
	}
}
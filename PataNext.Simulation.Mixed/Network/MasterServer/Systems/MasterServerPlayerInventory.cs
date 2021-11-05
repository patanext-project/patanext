using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using Microsoft.Extensions.Logging;
using PataNext.Game.GameItems;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.Network;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network.MasterServer.AssetService;
using StormiumTeam.GameBase.Network.MasterServer.Utility;
using StormiumTeam.GameBase.SystemBase;
using ZLogger;

namespace PataNext.Module.Simulation.Network.MasterServer.Systems
{
	public class MasterServerPlayerInventoryProvider : AppSystem
	{
		private World   itemWorld;
		private ILogger logger;

		public MasterServerPlayerInventoryProvider(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref logger);

			AddDisposable(itemWorld = new());
		}

		public MasterServerPlayerInventory Create(string saveId)
		{
			return new(saveId, guid =>
			{
				var entity = itemWorld.CreateEntity();
				entity.Set(new GetItemDetailsRequest(guid));

				return entity;
			}, (origin, attachment, guid) =>
			{
				if (!origin.Has<MasterServerUnitPresetData>())
				{
					logger.ZLogError("{0} is not an unit controlled by the MasterServer", origin);
					return;
				}

				var preset = origin.GetData<MasterServerUnitPresetData>().PresetGuid.ToString();
				// We need the attachment to be a GUID and not the resource path
				RequestUtility.CreateTracked(World.Mgr, new GetAssetGuidRequest { Path = new(attachment) }, (Entity _, GetAssetGuidRequest.Response response) =>
				{
					RequestUtility.CreateFireAndForget(World.Mgr, new SetPresetEquipments(preset, response.Guid, guid));
				});
			});
		}
	}

	public class MasterServerPlayerInventory : PlayerInventoryBase, ISwapEquipmentInventory
	{
		private struct ToMasterServerId
		{
			public string Value;
		}

		private PooledDictionary<string, string>                           guidToItemType    = new();
		private PooledDictionary<string, PooledDictionary<string, Entity>> typeToGuidItemMap = new();

		public readonly string SaveId;

		private Func<string, Entity> createEntity;
		private Action<SafeEntityFocus, string, string>             swapEquipment;

		public MasterServerPlayerInventory(string saveId, Func<string, Entity> createEntity, Action<SafeEntityFocus, string, string> swapEquipment)
		{
			SaveId = saveId;

			this.createEntity  = createEntity;
			this.swapEquipment = swapEquipment;
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
				if (item.Get<ItemInventory>().AssetEntity == assetEntity)
					list.Add(item);
		}

		public override Entity GetStack(Entity assetEntity)
		{
			if (!typeToGuidItemMap.TryGetValue(assetEntity.Get<GameItemDescription>().Type, out var dict))
				return default;

			foreach (var (_, item) in dict)
				if (item.Get<ItemInventory>().AssetEntity == assetEntity)
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
		internal PooledList<string>               newItems = new();

		internal void setMasterServerItems(string[] itemGuids)
		{
			foreach (var guid in itemGuids)
			{
				if (!msIdToEntity.ContainsKey(guid))
				{
					var entity = createEntity(guid);
					entity.Set(new ToMasterServerId { Value = guid });

					msIdToEntity.Add(guid, entity);
					newItems.Add(guid);
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
			entity.Set(new ItemInventory { AssetEntity = assetEntity });

			dict[guid] = entity;
			return entity;
		}

		// -- This will create a temporary item, or get an existing one
		// This method must only be used in rare case such as receiving equipment from an unit (since it may be possible that the inventory isn't totally updated)
		internal Entity getOrCreateTemporaryItem(string guid, Entity assetEntity)
		{
			if (!msIdToEntity.TryGetValue(guid, out var itemEntity) || !itemEntity.Has<ItemInventory>())
			{
				itemEntity = createEntity(guid);
				itemEntity.Set(new ToMasterServerId { Value = guid });

				msIdToEntity[guid] = itemEntity;
				newItems.Add(guid); // TODO: instead of directly adding it to the list, there should be some sort of confirmation
				
				setKnownItem(guid, assetEntity);
			}

			return itemEntity;
		}

		public void RequestSwap(SafeEntityFocus origin, string attachment, Entity newEquipmentEntity)
		{
			swapEquipment(origin, attachment, newEquipmentEntity.Get<ToMasterServerId>().Value);
		}
	}
}
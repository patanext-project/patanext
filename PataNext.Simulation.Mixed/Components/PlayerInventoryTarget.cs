using System;
using System.Collections;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Components
{
	public struct PlayerInventoryTarget : IComponentData
	{
		public Entity Value;

		public PlayerInventoryTarget(Entity value)
		{
			Value = value;
		}
	}

	public struct InventoryItem
	{
		// don't use AdditionalData if IsStackable is enabled
		public Entity AdditionalData;

		public bool IsStackable;
		public int  Count;

		public ResPath AssetId;
		public ResPath TypeId;

		public static InventoryItem Single(ResPath assetId, ResPath typeId, Entity additionalData = default)
		{
			return new()
			{
				AdditionalData = additionalData,

				AssetId = assetId,
				TypeId  = typeId
			};
		}

		public static InventoryItem Stack(int count, ResPath assetId, ResPath typeId)
		{
			return new()
			{
				IsStackable = true,
				Count       = count,

				AssetId = assetId,
				TypeId  = typeId
			};
		}
	}

	public abstract class PlayerInventoryBase : IDisposable
	{
		public void Read<TFill>(TFill list)
			where TFill : IList<InventoryItem>
		{
			Read<TFill, string[]>(list);
		}
		
		public abstract void Read<TFill, TRestrict>(TFill list, TRestrict restrictTypes = default)
			where TFill : IList<InventoryItem>
			where TRestrict : IList<string>;

		public abstract void Dispose();
	}

	public interface IWritablePlayerInventory
	{
		void SetStackable(ResPath assetId);
		
		void Add(InventoryItem    item);
		bool Remove(InventoryItem item);

		void Clear();
		void Clear(ResPath type);
	}

	public class PlayerInventoryNoRead : PlayerInventoryBase
	{
		public override void Read<TFill, TRestrict>(TFill list, TRestrict restrictTypes = default)
		{
		}

		public override void Dispose()
		{
		}
	}

	public class LocalPlayerInventory : PlayerInventoryBase,
	                                    IWritablePlayerInventory
	{
		private PooledDictionary<string, PooledList<InventoryItem>> itemsByType = new();
		private HashSet<string>                                     stackableMap = new();

		public void SetStackable(ResPath assetId)
		{
			stackableMap.Add(assetId.FullString);
		}
		
		public override void Read<TFill, TRestrict>(TFill list, TRestrict restrictTypes = default)
		{
			if (restrictTypes?.Count > 0)
			{
				foreach (var type in restrictTypes)
					if (itemsByType.TryGetValue(type, out var items))
						foreach (var item in items)
							list.Add(item);

				return;
			}

			foreach (var (_, items) in itemsByType)
			foreach (var item in items)
				list.Add(item);
		}

		public override void Dispose()
		{
			Clear();
			itemsByType.Dispose();
		}

		private int getStackIndex(string typeId, string assetId)
		{
			var items = itemsByType[typeId];
			for (var i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (item.IsStackable && item.AssetId.FullString == assetId)
					return i;
			}

			return -1;
		}

		public void Add(InventoryItem item)
		{
			if (!itemsByType.TryGetValue(item.TypeId.FullString, out var list))
				itemsByType[item.TypeId.FullString] = list = new();

			if (item.IsStackable || stackableMap.Contains(item.AssetId.FullString))
			{
				var stackIndex = getStackIndex(item.TypeId.FullString, item.AssetId.FullString);
				if (stackIndex >= 0)
				{
					var curr = list[stackIndex];
					curr.Count       += Math.Max(item.Count, 1);
					list[stackIndex] =  curr;
					return;
				}
			}

			list.Add(item);
		}

		public bool Remove(InventoryItem item)
		{
			if (!itemsByType.TryGetValue(item.TypeId.FullString, out var list))
				return false;

			if (item.IsStackable || stackableMap.Contains(item.AssetId.FullString))
			{
				var stackIndex = getStackIndex(item.TypeId.FullString, item.AssetId.FullString);
				if (stackIndex >= 0)
				{
					var curr = list[stackIndex];
					curr.Count += Math.Min(item.Count, -1);
					if (curr.Count <= 0)
					{
						list.RemoveAt(stackIndex);
						return true;
					}

					list[stackIndex] = curr;
					return true;
				}
			}

			return list.Remove(item);
		}

		public void Clear()
		{
			foreach (var (_, list) in itemsByType)
				list.Dispose();

			itemsByType.Clear();
		}

		public void Clear(ResPath type)
		{
			if (itemsByType.TryGetValue(type.FullString, out var list))
				list.Clear();
		}
	}
}
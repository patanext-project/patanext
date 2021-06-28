using System;
using GameHost.Core;
using PataNext.MasterServerShared.Services;

namespace PataNext.Module.Simulation.Network.MasterServer
{
	public class ItemHubReceiver : IItemHubReceiver
	{
		public event Action         InventoryUpdate;
		public event Action<string> ItemUpdate;

		public void OnInventoryUpdate()
		{
			InventoryUpdate?.Invoke();
		}

		public void OnItemUpdate(string itemGuid)
		{
			ItemUpdate?.Invoke(itemGuid);
		}
	}
}
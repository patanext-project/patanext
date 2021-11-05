using System;
using PataNext.MasterServerShared.Services;

namespace PataNext.Module.Simulation.Network.MasterServer
{
	public class HubFormationReceiver : IFormationReceiver
	{
		public void OnFormationUpdate(string fromSaveId)
		{
			Console.WriteLine($"Formation of {fromSaveId} updated!");
		}
	}
}
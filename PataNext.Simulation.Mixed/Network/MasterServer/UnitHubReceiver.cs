using System;
using PataNext.MasterServerShared.Services;

namespace PataNext.Module.Simulation.Network.MasterServer
{
	public class UnitHubReceiver : IUnitHubReceiver
	{
		public void OnHierarchyUpdate(string unitId)
		{
			Console.WriteLine($"Hierarchy of {unitId} updated.");
		}
	}
}
using System;
using PataNext.MasterServerShared.Services;

namespace PataNext.Module.Simulation.Network.MasterServer
{
	public class UnitPresetHubReceiver : IUnitPresetHubReceiver
	{
		public void OnPresetUpdate(string presetId)
		{
			Console.WriteLine($"Preset {presetId} updated.");
		}
	}
}
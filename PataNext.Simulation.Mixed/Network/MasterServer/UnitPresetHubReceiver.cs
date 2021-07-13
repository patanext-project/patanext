using System;
using PataNext.MasterServerShared.Services;

namespace PataNext.Module.Simulation.Network.MasterServer
{
	public class UnitPresetHubReceiver : IUnitPresetHubReceiver
	{
		public event Action<string> PresetUpdate; 

		public void OnPresetUpdate(string presetId)
		{
			PresetUpdate?.Invoke(presetId);
		}
	}
}
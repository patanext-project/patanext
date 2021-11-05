using System.Collections.Generic;
using DefaultEcs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Components.Hideout
{
	// This is not a Revolution/GHSimulation component!
	public struct UpdateUnitEquipmentRequest
	{
		public SafeEntityFocus            Entity;
		public Dictionary<string, Entity> Updates;

		public UpdateUnitEquipmentRequest(SafeEntityFocus entity, Dictionary<string, Entity> updates)
		{
			Entity  = entity;
			Updates = updates;
		}
	}
}
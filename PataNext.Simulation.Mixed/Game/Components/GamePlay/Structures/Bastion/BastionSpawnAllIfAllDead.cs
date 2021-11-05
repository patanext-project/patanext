using System;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion
{
	public struct BastionSpawnAllIfAllDead : IComponentData
	{
		public TimeSpan Accumulated;
		public TimeSpan Delay;
	}
}
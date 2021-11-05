using System;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion
{
	public struct RemoveBastionUnitWhenDead : IComponentData
	{
		public TimeSpan Delay;
	}
}
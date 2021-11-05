using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion
{
	public struct BastionSpawnUnitPeriodically : IComponentData
	{
		public TimeSpan Accumulated;
		public TimeSpan Period;

		public int SpawnCount;
	}
}
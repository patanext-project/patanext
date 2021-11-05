using System;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.Time.Components
{
	/// <summary>
	/// Indicate when an entity should be removed
	/// </summary>
	public readonly struct RemoveEntityEndTime : IComponentData
	{
		public readonly TimeSpan Value;

		public RemoveEntityEndTime(TimeSpan time) => Value = time;
	}
}
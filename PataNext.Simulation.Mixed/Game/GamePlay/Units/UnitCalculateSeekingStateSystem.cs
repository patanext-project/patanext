using System;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitCalculateSeekingStateSystem : GameLoopAppSystem, IUpdateSimulationPass
	{
		public UnitCalculateSeekingStateSystem(WorldCollection collection) : base(collection, false)
		{
			Add((GameEntity entity, ref UnitEnemySeekingState seekingState, ref Relative<TeamDescription> team) =>
			{
				return true;
			});
		}

		public void OnSimulationUpdate() => RunExecutors();
	}
}
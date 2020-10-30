using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay
{
    /// <summary>
    /// Get team enemies of this team
    /// </summary>
    public struct TeamEnemies : IComponentBuffer 
    {
        public GameEntity Team;

        public TeamEnemies(GameEntity team) => Team = team;
    }
}
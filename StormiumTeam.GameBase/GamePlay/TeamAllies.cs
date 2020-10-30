using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay
{
    /// <summary>
    /// Get team allies of this team
    /// </summary>
    public struct TeamAllies : IComponentBuffer 
    {
        public GameEntity Team;

        public TeamAllies(GameEntity team) => Team = team;
    }
}
using PataNext.Game.Modules.Simulation.Application;
using revecs;

namespace PataNext.Game.Modules.Simulation;

public partial struct GameTimeSingleton :
    IQuery<Read<GameTime>>,
    Singleton
{

}
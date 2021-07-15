using System.Threading.Tasks;
using GameHost.Simulation.TabEcs;

namespace PataNext.Game.Scenar
{
	public interface IScenar
	{
		Task StartAsync(GameEntity self, GameEntity creator);
		Task LoopAsync();
		Task CleanupAsync(bool reuse);
	}
}
using System.Threading.Tasks;

namespace PataNext.Game.Scenar
{
	public interface IScenar
	{
		Task StartAsync();
		Task LoopAsync();
		Task CleanupAsync(bool reuse);
	}
}
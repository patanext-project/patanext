using System.Threading.Tasks;

namespace PataNext.Game.Scenar
{
	public interface IScenar
	{
		Task Start();
		Task Loop();
		Task Cleanup();
	}
}
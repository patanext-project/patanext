using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Platform;

namespace PataNext.Export.Desktop.Tests
{
	public static class Program
	{
		public static void Main()
		{
			using (osu.Framework.Platform.GameHost host = Host.GetSuitableHost("visual-tests"))
			using (var game = new TestGameBrowser())
				host.Run(game);
		}
	}
}
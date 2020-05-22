using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using GameHost.Core.Threading;
using GameHost.Injection;
using PataNext.Module.Presentation;
using PataponGameHost;

namespace PataNext.Export.Desktop
{
	class Program
	{
		static void Main(string[] args)
		{
			GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

			using var game = new ClientGameBootstrap(new Context(null));
			game.OverrideExternalAssemblies.Add(typeof(PataNext.Game.GameModule).Assembly);
			game.OverrideExternalAssemblies.Add(typeof(Module.Simulation.CustomModule).Assembly);
			game.OverrideExternalAssemblies.Add(typeof(CustomModule).Assembly);
			game.Run();

			while (game.IsRunning)
			{
			}
		}
	}
}
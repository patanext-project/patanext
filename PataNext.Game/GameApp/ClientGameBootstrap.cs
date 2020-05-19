using System;
using System.Collections.Generic;
using System.Reflection;
using DryIoc;
using GameHost;
using GameHost.Applications;
using GameHost.Core.Game;
using GameHost.Injection;
using GameHost.Input.OpenTKBackend;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;

namespace PataponGameHost
{
	public class ClientGameBootstrap : GameBootstrapBase
	{
		/// <summary>
		/// Kinda ugly, but it give us the possibility to make sure internal assemblies override external assemblies...
		/// </summary>
		public List<Assembly> OverrideExternalAssemblies = new List<Assembly>();

		private GameWindow window;
		private Instance   instance;

		protected override void RunGame()
		{
			var disposableObjects = new List<IDisposable>();

			window = new GameFrame(Context, disposableObjects);

			Context.Bind<Instance, Instance>(Instance.CreateInstance<Instance>("Client", Context));
			Context.Bind<INativeWindow, GameWindow>(window);
			Context.Bind<IGameWindow, GameWindow>(window);

			GameAppShared.Init(Context, ref disposableObjects);

			var inputClient = new GameInputThreadingClient();
			inputClient.Connect();

			// Set backends (later there should be support for Unity inputs)
			inputClient.SetBackend<OpenTkInputBackend>();

			var simulationClient = new GameSimulationThreadingClient();
			simulationClient.Connect();

			simulationClient.AddInstance(Context.Container.Resolve<Instance>());

			var mainThreadClient = new MainThreadClient();

			window.UpdateFrame += args => { mainThreadClient.Listener.Update(); };

			window.Run();

			foreach (var obj in disposableObjects)
				obj.Dispose();

			Dispose();
		}

		public override bool IsRunning => window.Exists && !window.IsExiting;

		public override GameInformation GetGameInformation()
		{
			return new GameInformation
			{
				Name         = "PataNext",
				NameAsFolder = "PataNextClient"
			};
		}

		public ClientGameBootstrap(Context context) : base(context)
		{
		}
	}
}
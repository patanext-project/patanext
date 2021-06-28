//#define ENABLE_SERVER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using ENet;
using GameHost.Applications;
using GameHost.Audio.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Execution;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.Inputs.Systems;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Simulation.Application;
using GameHost.Simulation.Features.ShareWorldState;
using GameHost.Simulation.TabEcs;
using GameHost.Threading;
using GameHost.Transports;
using GameHost.Transports.Transports.Ruffles;
using GameHost.Worlds;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.GameModes;
using PataNext.Module.Simulation.RuntimeTests.GameModes;
using PataNext.Simulation.Client;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddApplicationSystem : AppSystem
	{
		private GlobalWorld             globalWorld;
		private CancellationTokenSource ccs;

		private Scheduler scheduler;

		public AddApplicationSystem(WorldCollection collection) : base(collection)
		{
			ccs = new CancellationTokenSource();

			DependencyResolver.Add(() => ref globalWorld);
			DependencyResolver.Add(() => ref scheduler);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			/*var enetServer = new ENetTransportDriver(32);
			var addr       = new Address {Port = 6410};
			addr.SetIP("0.0.0.0");
			//addr.SetIP("127.0.0.1");
			
			var reliableChannel = enetServer.CreateChannel(typeof(ReliableChannel));

			var bind   = enetServer.Bind(addr);
			if (bind != 0)
				throw new InvalidOperationException("Couldn't bind");
			
			var listen = enetServer.Listen();
			if (listen != 0)
				throw new InvalidOperationException("Couldn't listen");*/

			var enetServer = new RuffleTransportDriver();

			/*var enetServer = new ThreadTransportDriver(64);
			enetServer.Listen();*/

			var reliableChannel = default(TransportChannel);
			
#if ENABLE_SERVER
			var serverApp = AddApp("server", new SimulationApplication(globalWorld, null));
			{
				if (!enetServer.Listen(6410))
					if (!enetServer.Listen(6411))
						throw new InvalidOperationException("couldn't bind");
				
				if (!(serverApp.Get<IListener>() is IApplication app))
					throw new InvalidOperationException();

				var serverGameWorld = new ContextBindingStrategy(app.Data.Context, false).Resolve<GameWorld>();
				serverGameWorld.AddComponent<AtCityGameModeData>(serverGameWorld.CreateEntity());

				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.SerializerCollection));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.UpdateDriverSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSystems));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSnapshotSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.AddComponentsServerFeature));
				
				app.Data.Collection.GetOrCreate(typeof(AddShareSimulationWorldFeature));
				
				app.Data.World.CreateEntity()
				   .Set<IFeature>(new GameHost.Revolution.NetCode.LLAPI.Systems.ServerFeature(enetServer, reliableChannel));
				   
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(enetServer.TransportAddress);
				Console.ResetColor();
				
				AddDisposable(enetServer);
			}
#endif

			var listener = new ThreadListenerCollection("Client", ccs.Token);
			for (var i = 0; i < 1; i++)
			{
				var appName = "client";
				if (i > 0)
					appName += "#" + i;

				if (i == 0)
				{
					AddClient(appName, 1000, true, listener, enetServer);
				}
				else
				{
					AddClient(appName, 2000 + (1000 * i), false, listener, enetServer);
				}
			}

			AddApp("Audio", new AudioApplication(globalWorld, null));
		}

		private void AddClient(string appName, int delayMs, bool first, ThreadListenerCollection listener, TransportDriver driver)
		{
			var clientApp = AddApp(appName, new SimulationApplication(globalWorld, null), listener);
			{
				clientApp.Set<IClientSimulationApplication>();

				if (!(clientApp.Get<IListener>() is IApplication app))
					throw new InvalidOperationException();

				if (first)
				{
					app.Data.Collection.GetOrCreate(wc => new DiscordFeature(wc));
					app.Data.Collection.GetOrCreate(typeof(AddShareSimulationWorldFeature));
				}
				
				
				var serverGameWorld = new ContextBindingStrategy(app.Data.Context, false).Resolve<GameWorld>();
				//serverGameWorld.AddComponent<BasicTestGameMode>(serverGameWorld.CreateEntity());
				
				//app.Data.Collection.GetOrCreate(typeof(RuntimeTestCoopMission));

				app.Data.Collection.GetOrCreate(typeof(RuntimeTestUnitBarracks));

				app.Data.Collection.GetOrCreate(typeof(SendWorldStateSystem));
				app.Data.Collection.GetOrCreate(typeof(SharpDxInputSystem));

				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.SerializerCollection));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.UpdateDriverSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSystems));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSnapshotSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.AddComponentsClientFeature));

				Task.Delay(delayMs).ContinueWith(_ =>
				{
					scheduler.Schedule(() =>
					{
						ClientFeature clientFeature = null;
#if ENABLE_SERVER
						if (driver.TransportAddress.Connect() is not { } clientDriver)
							throw new NullReferenceException(nameof(clientDriver));
						clientFeature = new GameHost.Revolution.NetCode.LLAPI.Systems.ClientFeature(clientDriver, default);
#endif

						if (clientFeature != null)
						{
							app.Data.World.CreateEntity().Set<IFeature>(clientFeature);
						}
					}, default);
				});
			}
		}

		private Entity AddApp<T>(string name, T app, ListenerCollectionBase collectionBase = null)
			where T : class, IApplication, IListener
		{
			var listener = World.Mgr.CreateEntity();
			listener.Set<ListenerCollectionBase>(collectionBase ?? new ThreadListenerCollection(name, ccs.Token));

			//app.Data.World.CreateEntity().Set();

			var systems = new List<Type>();
			AppSystemResolver.ResolveFor<T>(systems);

			foreach (var type in systems)
				app.Data.Collection.GetOrCreate(type);

			var applicationEntity = World.Mgr.CreateEntity();
			applicationEntity.Set<IListener>(app);
			applicationEntity.Set(new PushToListenerCollection(listener));
			applicationEntity.Set(new ApplicationName(name));

			app.AssignedEntity = applicationEntity;

			return applicationEntity;
		}

		public override void Dispose()
		{
			base.Dispose();
			ccs.Cancel();
		}
	}
}
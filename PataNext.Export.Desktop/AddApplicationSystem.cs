using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DefaultEcs;
using ENet;
using GameHost.Applications;
using GameHost.Audio.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Execution;
using GameHost.Core.IO;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Threading;
using GameHost.Transports;
using GameHost.Worlds;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Simulation.Client;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public class AddApplicationSystem : AppSystem
	{
		private GlobalWorld             globalWorld;
		private CancellationTokenSource ccs;

		public AddApplicationSystem(WorldCollection collection) : base(collection)
		{
			ccs = new CancellationTokenSource();

			DependencyResolver.Add(() => ref globalWorld);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			var enetServer = new ENetTransportDriver(32);
			var addr       = new Address {Port = 0};
			addr.SetIP("127.0.0.1");
			
			var reliableChannel = enetServer.CreateChannel(typeof(ReliableChannel));

			var bind   = enetServer.Bind(addr);
			if (bind != 0)
				throw new InvalidOperationException("Couldn't bind");
			
			var listen = enetServer.Listen();
			if (listen != 0)
				throw new InvalidOperationException("Couldn't listen");

			var serverApp = AddApp("server", new SimulationApplication(globalWorld, null));
			{
				if (!(serverApp.Get<IListener>() is IApplication app))
					throw new InvalidOperationException();

				var serverGameWorld = new ContextBindingStrategy(app.Data.Context, false).Resolve<GameWorld>();
				serverGameWorld.AddComponent<AtCityGameModeData>(serverGameWorld.CreateEntity());

				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.SerializerCollection));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.UpdateDriverSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSystems));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSnapshotSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.AddComponentsServerFeature));
				
				app.Data.World.CreateEntity()
				   .Set<IFeature>(new GameHost.Revolution.NetCode.LLAPI.Systems.ServerFeature(enetServer, reliableChannel));
			}

			var clientApp = AddApp("client", new SimulationApplication(globalWorld, null));
			{
				clientApp.Set<IClientSimulationApplication>();

				if (!(clientApp.Get<IListener>() is IApplication app))
					throw new InvalidOperationException();
				
				/*var serverGameWorld = new ContextBindingStrategy(app.Data.Context, false).Resolve<GameWorld>();
				serverGameWorld.AddComponent<BasicTestGameMode>(serverGameWorld.CreateEntity());*/

				app.Data.Collection.GetOrCreate(wc => new DiscordFeature(wc));

				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.SerializerCollection));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.UpdateDriverSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSystems));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSnapshotSystem));
				app.Data.Collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.AddComponentsClientFeature));

				Console.WriteLine(enetServer.TransportAddress);

				if (enetServer.TransportAddress.Connect() is not ENetTransportDriver clientDriver)
					throw new NullReferenceException(nameof(clientDriver));
				
				reliableChannel = clientDriver.CreateChannel(typeof(ReliableChannel));
				
				app.Data.World.CreateEntity()
				   .Set<IFeature>(new GameHost.Revolution.NetCode.LLAPI.Systems.ClientFeature(clientDriver, reliableChannel));
			}

			AddApp("Audio", new AudioApplication(globalWorld, null));
			
			AddDisposable(enetServer);
		}

		private Entity AddApp<T>(string name, T app)
			where T : class, IApplication, IListener
		{
			var listener = World.Mgr.CreateEntity();
			listener.Set<ListenerCollectionBase>(new ThreadListenerCollection(name, ccs.Token));

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
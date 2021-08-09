using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Inputs.Systems;
using GameHost.Simulation.Application;
using GameHost.Simulation.Features.ShareWorldState;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.RuntimeTests.GameModes;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Bootstrap;

namespace PataNext.Export.Desktop.Bootstrap
{
	public class ConfigurableBootstrapFile
	{
		public bool UseMasterServer = false;
		public bool UseGameServer   = false;

		public int ClientCount = 1;
	}

	public class ConfigurableBootstrap : BootstrapEntry<ConfigurableBootstrapFile>
	{
		public const string NameId = "configurable";

		public override string Id => NameId;

		private AddApplicationSystem addApplicationSystem;

		public ConfigurableBootstrap([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref addApplicationSystem);
		}

		protected override void OnExecute(ConfigurableBootstrapFile args)
		{
			for (var i = 0; i < args.ClientCount; i++)
			{
				var name = "client";
				if (i > 0)
					name += "client" + i;

				var (entity, app) = addApplicationSystem.AddClient(name, 1000, i == 0);

				OnCreateClient(entity, app);

				if (args.UseMasterServer)
					app.Data.Collection.GetOrCreate(typeof(AddMasterServerFeature));
			}
		}

		protected virtual void OnCreateClient(Entity clientEntity, SimulationApplication application)
		{
			var collection = application.Data.Collection;
			collection.GetOrCreate(typeof(SendWorldStateSystem));
			collection.GetOrCreate(typeof(SharpDxInputSystem));

			collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.SerializerCollection));
			collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.UpdateDriverSystem));
			collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSystems));
			collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.SendSnapshotSystem));
			collection.GetOrCreate(typeof(GameHost.Revolution.NetCode.LLAPI.Systems.AddComponentsClientFeature));
		}
	}
}
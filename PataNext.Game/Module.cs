using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.IO;
using GameHost.Simulation.Application;
using GameHost.Simulation.Utility.Resource;
using GameHost.Threading;
using GameHost.Utility;
using GameHost.Worlds;
using PataNext.Game.Abilities;
using PataNext.Game.BGM;
using PataNext.Game.GameItems;
using StormiumTeam.GameBase;
using Module = PataNext.Game.Module;

[assembly: RegisterAvailableModule("PataNext.Game", "guerro", typeof(Module))]

namespace PataNext.Game
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global  = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			var systems = new List<Type>();
			AppSystemResolver.ResolveFor<SimulationApplication>(GetType().Assembly, systems);

			global.Context.BindExisting(DefaultEntity<ResPathDefaults>.Create(global.World, new() {Author = "st", ModPack = "pn"}));

			Storage.Subscribe((_, exteriorStorage) =>
			{
				var storage = exteriorStorage switch
				{
					{ } => new StorageCollection {exteriorStorage, DllStorage},
					null => new StorageCollection {DllStorage}
				};

				var itemStorage = storage.GetOrCreateDirectoryAsync("items").Result;
				
				global.Context.BindExisting(new EquipmentItemMetadataStorage(itemStorage.GetOrCreateDirectoryAsync("equipments").Result));
			}, true);

			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Schedule(() =>
					{
						foreach (var type in systems)
							simulationApplication.Data.Collection.GetOrCreate(type);
					}, default);
				}
			}
		}
	}
}
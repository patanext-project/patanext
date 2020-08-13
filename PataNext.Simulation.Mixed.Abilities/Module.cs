using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.IO;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase;

[assembly: RegisterAvailableModule("PataNext Standard Abilities", "guerro", typeof(PataNext.Simulation.Mixed.Abilities.Module))]

namespace PataNext.Simulation.Mixed.Abilities
{
	public class Module : GameHostModule, IModuleHasAbilityDescStorage
	{
		private AbilityDescStorage abilityDescStorage;

		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultMarchAbilityProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultMarchAbilitySystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultRetreatAbilityProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultRetreatAbilitySystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultJumpAbilityProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultJumpAbilitySystem));
					
					simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultChargeAbilityProvider));

					simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendFrontalAbilityProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendFrontalAbilitySystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendStayAbilityProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendStayAbilitySystem));
					
					simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayEnergyFieldAbilityProvider));
					simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayEnergyFieldAbilitySystem));

					simulationApplication.Data.Collection.GetOrCreate(typeof(Subset.DefaultSubsetMarchAbilitySystem));
				}
			}

			Storage.Subscribe((_, exteriorStorage) =>
			{
				var storage = exteriorStorage switch
				{
					{} => new StorageCollection {exteriorStorage, DllStorage},
					null => new StorageCollection {DllStorage}
				};

				abilityDescStorage = new AbilityDescStorage(storage.GetOrCreateDirectoryAsync("Abilities").Result);
			}, true);
		}

		public AbilityDescStorage Value => abilityDescStorage;
	}
}
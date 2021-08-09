using System;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Modules;
using GameHost.Core.Modules.Feature;
using GameHost.Core.Threading;
using GameHost.Injection;
using GameHost.IO;
using GameHost.Revolution.NetCode.LLAPI;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Utility;
using GameHost.Worlds;
using PataNext.Game.Abilities;
using Module = PataNext.CoreAbilities.Mixed.Module;

[assembly: RegisterAvailableModule("PataNext Standard Abilities", "guerro", typeof(Module))]

namespace PataNext.CoreAbilities.Mixed
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(Ctx, true).Resolve<GlobalWorld>();
			
			AddDisposable(ApplicationTracker.Track(this, (SimulationApplication simulationApplication) =>
			{
				var sc     = simulationApplication.Data.Collection.GetOrCreate(wc => new SerializerCollection(wc));
				var appCtx = simulationApplication.Data.Context;

				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultMarchAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultMarchAbilitySystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultBackwardAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultBackwardAbilitySystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultRetreatAbilityProvider));
				sc.Register(inst => new Defaults.DefaultRetreatAbility.Serializer(inst, appCtx));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultJumpAbilityProvider));
				sc.Register(inst => new Defaults.DefaultJumpAbility.Serializer(inst, appCtx));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultPartyAbilityProvider));
				sc.Register(inst => new Defaults.DefaultPartyAbility.Serializer(inst, appCtx));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Defaults.DefaultChargeAbilityProvider));
				sc.Register(inst => new Defaults.DefaultChargeAbility.Serializer(inst, appCtx));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendFrontalAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendFrontalAbilitySystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendStayAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicDefendStayAbilitySystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayCounterAbilityProvider));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayEnergyFieldAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayEnergyFieldAbilitySystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazayBasicAttackAbilityProvider));
				sc.Register(inst => new CTate.TaterazayBasicAttackAbility.Serializer(inst, appCtx));
				sc.Register(inst => new CTate.TaterazayBasicAttackAbility.State.Serializer(inst, appCtx));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CGuard.GuardiraBasicDefendAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CGuard.GuardiraMegaShieldAbilityProvider));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CYari.YaridaBasicAttackAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CYari.YaridaLeapAttackAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CYari.YaridaFearSpearAbilityProvider));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CPike.WooyariMultiAttackAbilityProvider));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CYumi.YumiyachaBasicAttackAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CYumi.YumiyachaSnipeAttackAbilityProvider));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CMega.MegaponBasicSonicAttackAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CMega.MegaponBasicWordAttackAbilityProvider));
				simulationApplication.Data.Collection.GetOrCreate(typeof(CMega.MegaponBasicMagicAttackAbilityProvider));

				simulationApplication.Data.Collection.GetOrCreate(typeof(Subset.DefaultSubsetMarchAbilitySystem));

				simulationApplication.Data.Collection.GetOrCreate(typeof(CTate.TaterazaySuperAbilityProvider));
			}));

			Storage.Subscribe((_, exteriorStorage) =>
			{
				var storage = exteriorStorage switch
				{
					{} => new StorageCollection {exteriorStorage, DllStorage},
					null => new StorageCollection {DllStorage}
				};

				Ctx.BindExisting(new AbilityDescStorage(storage.GetOrCreateDirectoryAsync("Abilities").Result));
			}, true);

			global.Scheduler.Schedule(tryLoadModule, SchedulingParameters.AsOnce);
		}
		
		private void tryLoadModule()
		{
			var global = new ContextBindingStrategy(Ctx.Parent, true).Resolve<GlobalWorld>();
			foreach (var ent in global.World)
			{
				if (ent.TryGet(out RegisteredModule registeredModule)
				    && registeredModule.State == ModuleState.None
				    && (registeredModule.Description.NameId == "PataNext.CoreAbilities.Server"))
				{
					Console.WriteLine("Load Server Module!");
					global.World.CreateEntity()
					      .Set(new RequestLoadModule("PataNext.CoreAbilities.Server", ent));
					return;
				}
			}
			
			global.Scheduler.Schedule(tryLoadModule, SchedulingParameters.AsOnce);
		}

		protected override void OnDispose()
		{
		}
	}
}
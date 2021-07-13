using DefaultEcs;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using PataNext.Game.Abilities;
using PataNext.Module.Simulation.Systems.GhRpc;
using PataNext.Simulation.Client.Rpc;
using PataNext.Simulation.Client.Rpc.City;
using PataNext.Simulation.Client.Systems;
using PataNext.Simulation.Client.Systems.Inputs;

namespace PataNext.Simulation.Client
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			global.Collection.GetOrCreate(typeof(ConnectToServerRpc.System));
			global.Collection.GetOrCreate(typeof(DisconnectFromServerRpc.System));
			global.Collection.GetOrCreate(typeof(SendServerNoticeRpc.System));
			//global.Collection.GetOrCreate(typeof(GetInventoryRpc.System));
			global.Collection.GetOrCreate(typeof(GetSavePresetsRpc.Process));
			global.Collection.GetOrCreate(typeof(GetItemDetailsRpc.Process));
			global.Collection.GetOrCreate(typeof(SetEquipmentUnitRpc.Process));
			global.Collection.GetOrCreate(typeof(HeadquartersGetUnitsRpc.Process));
			global.Collection.GetOrCreate(typeof(CopyPresetToUnitRpc.Process));
			global.Collection.GetOrCreate(typeof(UnitOverviewGetRestrictedItemInventory.Process));

			global.Collection.GetOrCreate(typeof(UnitOverviewStatisticsRpc.Process));
			
			global.Collection.GetOrCreate(typeof(ModifyPlayerCityLocationRpc.Process));
			global.Collection.GetOrCreate(typeof(ObeliskStartMissionRpc.Process));

			foreach (var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					if (!simulationApplication.AssignedEntity.Has<IClientSimulationApplication>())
						continue;

					simulationApplication.Schedule(() =>
					{
						simulationApplication.Data.Collection.GetOrCreate(typeof(RegisterRhythmEngineInputSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(RegisterFreeRoamInputSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(AbilityHeroVoiceManager));

						simulationApplication.Data.Collection.GetOrCreate(typeof(InterpolateForeignUnitsPosition));
					}, default);
				}
			}
		}
	}
}
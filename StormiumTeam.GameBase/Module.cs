using System;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs.Passes;
using GameHost.Core.Modules;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Threading;
using GameHost.Worlds;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.StandardAuthService;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.UserService;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Time;
using StormiumTeam.GameBase.Time.Components;

[assembly: RegisterAvailableModule("GameBase", "StormiumTeam", typeof(StormiumTeam.GameBase.Module))]

namespace StormiumTeam.GameBase
{
	public class Module : GameHostModule
	{
		public Module(Entity source, Context ctxParent, GameHostModuleDescription description) : base(source, ctxParent, description)
		{
			var global = new ContextBindingStrategy(ctxParent, true).Resolve<GlobalWorld>();
			foreach (ref readonly var listener in global.World.Get<IListener>())
			{
				if (listener is SimulationApplication simulationApplication)
				{
					simulationApplication.Schedule(() =>
					{
						var systemCollection = simulationApplication.Data.Collection.DefaultSystemCollection;
						systemCollection.AddPass(new IPreUpdateSimulationPass.RegisterPass(), new[] {typeof(UpdatePassRegister)}, null);
						systemCollection.AddPass(new IUpdateSimulationPass.RegisterPass(), new[] {typeof(IPreUpdateSimulationPass.RegisterPass)}, null);
						systemCollection.AddPass(new IPostUpdateSimulationPass.RegisterPass(), new[] {typeof(IUpdateSimulationPass.RegisterPass)}, null);

						simulationApplication.Data.Collection.GetOrCreate(typeof(SetGameTimeSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(PhysicsSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(BuildTeamEntityContainerSystem));
						
						simulationApplication.Data.Collection.GetOrCreate(typeof(HitBoxAgainstEnemiesSystem));

						simulationApplication.Data.Collection.GetOrCreate(typeof(MasterServerManageSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(CurrentUserSystem));
						simulationApplication.Data.Collection.GetOrCreate(typeof(DisconnectUserRequest.Process));

						simulationApplication.Data.Collection.GetOrCreate(typeof(ConnectUserRequest.Process));
						
						simulationApplication.Data.Collection.GetOrCreate(typeof(RemoveEntityWithEndTimeSystem));
					}, default);
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.Application;
using GameHost.Worlds;
using Grpc.Core;
using MagicOnion.Server;
using PataNext.Module.Simulation.Network.MasterServer.Services;
using StormiumTeam.GameBase.Network.MasterServer;
using StormiumTeam.GameBase.Network.MasterServer.StandardAuthService;
using StormiumTeam.GameBase.Network.MasterServer.User;
using StormiumTeam.GameBase.Network.MasterServer.UserService;
using StormiumTeam.GameBase.Network.MasterServer.Utility;

namespace PataNext.Export.Desktop
{
	// TODO: This shouldn't be created from a system, but from the game bootstrap
	// Or atleast have a way to get the configuration file from Global
	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddMasterServerFeature : AppSystem
	{
		private TaskScheduler taskScheduler;

		public AddMasterServerFeature(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref taskScheduler);

			collection.Mgr.CreateEntity().Set<IFeature>(new MasterServerFeature("localhost:12345"));

return;

			World.Mgr.CreateEntity().Set(new DisconnectUserRequest("12345689"));
			World.Mgr.CreateEntity().Set(ConnectUserRequest.ViaLogin("guerro323", "1733125552"));
			World.Mgr.SubscribeComponentChanged((in Entity e, in CurrentUser prev, in CurrentUser curr) =>
			{
				if (string.IsNullOrEmpty(curr.Value.Token))
					return;
				
				RequestUtility.CreateTracked(World.Mgr, new ListGameSaveRequest(), (Entity _, ListGameSaveRequest.Response response) =>
				{
					foreach (var saveId in response.Results)
						Console.WriteLine($"saveId={saveId}");
				});
			});
		}
	}
}
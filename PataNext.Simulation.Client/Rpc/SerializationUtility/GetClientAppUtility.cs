using System;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Simulation.Application;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Threading;
using PataNext.Module.Simulation.Components.Network;
using StormiumTeam.GameBase.Roles.Descriptions;

namespace PataNext.Game.Rpc.SerializationUtility
{
	public static class GetClientAppUtility
	{
		public static SimulationApplication Get(AppSystem system)
		{
			foreach (var app in system.World.Mgr.Get<IListener>())
			{
				if (!(app is SimulationApplication simuApp))
					continue;

				Console.WriteLine(simuApp.AssignedEntity.Get<ApplicationName>().Value);
				if (!simuApp.AssignedEntity.Get<ApplicationName>().Value.StartsWith("client"))
					continue;

				return simuApp;
			}

			throw new InvalidOperationException("no client app found");
		}

		public static string? GetLocalPlayerSave(SimulationApplication app)
		{
			var gameWorld = new ContextBindingStrategy(app.Data.Context, true).Resolve<GameWorld>();
			if (gameWorld != null && gameWorld.TryGetSingleton<PlayerIsLocal>(out GameEntityHandle playerHandle)
			                      && gameWorld.HasComponent<PlayerAttachedGameSave>(playerHandle))
			{
				return gameWorld.GetComponentData<PlayerAttachedGameSave>(playerHandle).Guid.ToString();
			}

			return null;
		}
	}
}
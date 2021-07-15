using System;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using Microsoft.Extensions.Logging;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components.GameModes;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.GameModes;
using ZLogger;

namespace PataNext.Simulation.Client.Rpc.City
{
	public struct LeaveMissionAndReturnToCityRpc : IGameHostRpcPacket
	{
		public class Process : RpcPacketSystem<LeaveMissionAndReturnToCityRpc>
		{
			private ILogger logger;
			
			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref logger);
			}

			public override    string MethodName => "PataNext.Client.City.LeaveMissionAndReturnToCity";
			protected override void   OnNotification(LeaveMissionAndReturnToCityRpc notification)
			{
				var app = GetClientAppUtility.Get(this);
				app.Schedule(() =>
				{
					var gameWorld = new ContextBindingStrategy(app.Data.Context, true).Resolve<GameWorld>();
					if (!gameWorld.TryGetSingleton<AtCityGameModeData>(out GameEntityHandle cityHandle))
					{
						logger.ZLogError("No city gamemode found");
						return;
					}

					if (gameWorld.HasComponent<CityCurrentGameModeTarget>(cityHandle))
					{
						var currentGameModeTarget = gameWorld.GetComponentData<CityCurrentGameModeTarget>(cityHandle);
						if (!gameWorld.Exists(currentGameModeTarget.Entity))
						{
							throw new InvalidOperationException($"invalid entity {currentGameModeTarget.Entity}");
						}

						gameWorld.AddComponent(currentGameModeTarget.Entity.Handle, new GameModeRequestCleanUp());
						logger.ZLogInformation("requesting cleanup");
					}
					else
						logger.ZLogWarning("not in a mission");
				}, default);
			}
		}
	}
}
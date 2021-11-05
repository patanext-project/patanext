using System;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Utility;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components.GamePlay.Units;

namespace PataNext.Simulation.Client.Rpc
{
	public struct UnitOverviewStatisticsRpc : IGameHostRpcPacket
	{
		public const string RpcMethodName = "PataNext.Client.UnitOverview.GetStatisticsMini";
		
			public struct StatusEffect
			{
				public string Name;

				public float Power;
				public float Resistance;
				public float GainPerSecond;
				public float Immunity;
			}

			public int   Health      { get; set; }
			public int   Defense     { get; set; }
			public int   Strength    { get; set; }
			public float AttackSpeed { get; set; }

			public StatusEffect[] StatusEffects { get; set; }

			public GameEntity TargetEntity { get; set; }

		public class Process : RpcPacketSystem<UnitOverviewStatisticsRpc>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => RpcMethodName;

			protected override async void OnNotification(UnitOverviewStatisticsRpc request)
			{
				/*var client = GetClientAppUtility.Get(this);
				if (client == null)
					return await WithError(1, "no client app found");

				return await client.TaskScheduler.StartUnwrap(async () =>
				{
					var gameWorld = new ContextBindingStrategy(client.Data.Context, false).Resolve<GameWorld>();
					Console.WriteLine("1. " + request.TargetEntity);
					if (!gameWorld.Exists(request.TargetEntity))
						return await WithError(2, $"target entity {request.TargetEntity} not found!");

					Console.WriteLine("2.");
					if (!gameWorld.HasComponent<UnitStatistics>(request.TargetEntity.Handle))
						return await WithError(3, $"target entity {request.TargetEntity} is not a valid unit!");
  
					var stats = gameWorld.GetComponentData<UnitStatistics>(request.TargetEntity.Handle);
					Console.WriteLine($"{stats.Health}");
					return await WithResult(new()
					{
						Health      = stats.Health,
						Defense     = stats.Defense,
						Strength    = stats.Attack,
						AttackSpeed = stats.AttackSpeed
					});
				});*/
			}
		}
	}
}
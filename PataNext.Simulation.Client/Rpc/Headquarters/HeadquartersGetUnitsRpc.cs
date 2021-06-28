using System.Collections.Generic;
using System.Threading.Tasks;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Utility;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components.Army;
using StormiumTeam.GameBase.Roles.Components;

namespace PataNext.Simulation.Client.Rpc
{
	public struct HeadquartersGetUnitsRpc : IGameHostRpcWithResponsePacket<HeadquartersGetUnitsRpc.Response>
	{
		public struct Response : IGameHostRpcResponsePacket
		{
			public enum ESquadType
			{
				Standard,
				Hatapon,
				Player,
			}

			public struct Squad
			{
				public ESquadType Type;

				public GameEntity   Leader;
				public GameEntity[] Soldiers;
			}

			public Squad[] Squads;
		}

		public class Process : RpcPacketWithResponseSystem<HeadquartersGetUnitsRpc, Response>
		{
			public Process(WorldCollection collection) : base(collection)
			{
			}

			public override string MethodName => "PataNext.Client.Headquarters.GetUnits";

			protected override async ValueTask<Response> GetResponse(HeadquartersGetUnitsRpc request)
			{
				var client = GetClientAppUtility.Get(this);
				return await client.TaskScheduler.StartUnwrap(async () =>
				{
					var gameWorld = new ContextBindingStrategy(client.Data.Context, false).Resolve<GameWorld>();
					var squads    = new List<Response.Squad>();

					using var formationQuery = new EntityQuery(gameWorld, new[]
					{
						gameWorld.AsComponentType<ArmyFormationDescription>()
					});
					if (!formationQuery.Any())
						return await WithResult(default);

					var formation = formationQuery.GetEnumerator().First;
					var squadBuffer = gameWorld.GetBuffer<OwnedRelative<ArmySquadDescription>>(formation)
					                           .Reinterpret<GameEntity>();
					foreach (var squadEntity in squadBuffer)
					{
						var squad = new Response.Squad();
						var buffer = gameWorld.GetBuffer<OwnedRelative<ArmyUnitDescription>>(squadEntity.Handle)
						                      .Reinterpret<GameEntity>();

						squad.Soldiers = new GameEntity[buffer.Count - 1];
						for (var i = 0; i < buffer.Count; i++)
						{
							var entity = buffer[i];
							if (i == 0)
								squad.Leader = entity;
							else
								squad.Soldiers[i] = entity;
						}

						squads.Add(squad);
					}

					return await WithResult(new()
					{
						Squads = squads.ToArray()
					});
				});
			}
		}
	}
}
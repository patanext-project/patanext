using System;
using System.Collections.Generic;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Features;
using GameHost.Revolution.NetCode.Components;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.Snapshot.Systems.Instigators;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Network
{
	/// <summary>
	/// Create player entities based on connected instigators on a server.
	/// </summary>
	/// <remarks>
	///	When an instigator is destroyed, that player entity will be removed (if it's attached with <see cref="CreatedByThisSystem"/> component)
	/// When the server broadcaster is destroyed, all player entities will be removed (if it's attached with <see cref="CreatedByThisSystem"/> component)
	/// </remarks>
	public class CreateGamePlayerOnConnectionSystem : GameAppSystem
	{
		public struct CreatedByThisSystem : IComponentData
		{
			public int InstigatorId;
		}

		private GetFeature<ServerFeature> serverFeature;

		public CreateGamePlayerOnConnectionSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref serverFeature, new FeatureDependencyStrategy<ServerFeature>(collection, f => f is ServerFeature));

			AddDisposable(collection.Mgr.SubscribeComponentRemoved((in Entity entity, in GameEntity gameEntity) =>
			{
				if (!HasComponent<CreatedByThisSystem>(gameEntity.Handle))
					return;

				// it may be possible that it's not the same entity at all, so check the version before destroying the handle...
				if (GameWorld.Safe(gameEntity.Handle).Version == gameEntity.Version)
					GameWorld.RemoveEntity(gameEntity.Handle);
			}));
		}

		protected override void OnUpdate()
		{
			// Destroy all players created by us
			if (serverFeature.Count == 0)
			{
				using var destroyList = new PooledList<GameEntityHandle>();
				foreach (var ent in GameWorld.QueryEntity(stackalloc[] {AsComponentType<CreatedByThisSystem>(), AsComponentType<PlayerDescription>()}, Span<ComponentType>.Empty))
				{
					destroyList.Add(ent);
				}

				destroyList.ForEach(GameWorld.RemoveEntity);
				return;
			}

			foreach (var (entity, feature) in serverFeature)
			{
				if (!entity.TryGet(out BroadcastInstigator broadcastInstigator))
					throw new InvalidOperationException("A ServerFeature should have a BroadcastInstigator");

				foreach (var client in broadcastInstigator.clients)
				{
					var storage = client.Storage;
					if (!storage.TryGet(out GameEntity gameEntity) || !GameWorld.Exists(gameEntity))
					{
						gameEntity = Safe(CreateEntity());

						AddComponent(gameEntity, new PlayerDescription());
						AddComponent(gameEntity, new CreatedByThisSystem {InstigatorId = client.InstigatorId});
						AddComponent(gameEntity, new NetReportTime());
						AddComponent(gameEntity, new NetworkedEntity());

						storage.Set(gameEntity);
					}

					client.OwnedEntities.Add(new ClientOwnedEntity(gameEntity, 0, default));

					if (client.Storage.TryGet(out List<GameTime> times))
					{
						ref var report = ref GetComponentData<NetReportTime>(gameEntity);
						if (times.Count == 0)
						{
							report.Continuous++;
						}
						else
						{
							report.Continuous = 0;
							report.Begin      = times[0];
							report.End        = times[^1];
						}

						//Console.WriteLine($"C={report.Continuous} B={report.Begin.Frame} E={report.End.Frame}");
					}
				}
			}
		}
	}
}
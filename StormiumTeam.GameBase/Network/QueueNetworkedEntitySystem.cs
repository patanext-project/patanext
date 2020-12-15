using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Features;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Revolution.Snapshot.Systems.Instigators;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Network
{
	public struct NetworkedEntity : IComponentData
	{
	}

	public struct OwnedNetworkedEntity : IComponentData
	{
		public readonly GameEntity Parent;

		public OwnedNetworkedEntity(GameEntity entity)
		{
			Parent = entity;
		}
	}

	[UpdateBefore(typeof(SendSnapshotSystem))]
	public class QueueNetworkedEntitySystem : GameAppSystem
	{
		private GetFeature<MultiplayerFeature> features;

		public QueueNetworkedEntitySystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref features, new FeatureDependencyStrategy<MultiplayerFeature>(collection, f => f is MultiplayerFeature));
		}

		public override bool CanUpdate()
		{
			return base.CanUpdate() && features.Count != 0;
		}

		private EntityQuery networkedEntities, ownedEntities;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			networkedEntities   ??= CreateEntityQuery(new[] {typeof(NetworkedEntity)});
			ownedEntities       ??= CreateEntityQuery(new[] {typeof(SnapshotOwnedWriteArchetype)});

			foreach (var (entity, feature) in features)
			{
				if (!entity.TryGet(out BroadcastInstigator broadcastInstigator))
					continue;

				foreach (var handle in networkedEntities)
				{
					//Console.WriteLine($"queue {handle}");
					broadcastInstigator.QueuedEntities[Safe(handle)] = EntitySnapshotPriority.SendAtAllCost;

					if (HasComponent<OwnedNetworkedEntity>(handle))
					{
						var next = Safe(handle);
						while (GameWorld.Exists(next))
						{
							if (TryGetComponentData(next, out OwnedNetworkedEntity ownedNetworkedEntity))
								next = ownedNetworkedEntity.Parent;
							else
								break;
						}

						if (GameWorld.Exists(next) && TryGetComponentData(next, out CreateGamePlayerOnConnectionSystem.CreatedByThisSystem internalData)
						                           && broadcastInstigator.TryGetClient(internalData.InstigatorId, out var client))
						{
							client.OwnedEntities.Add(new ClientOwnedEntity(Safe(handle), 0, default));
						}
					}
				}

				foreach (var handle in ownedEntities)
				{
					broadcastInstigator.QueuedEntities[Safe(handle)] = EntitySnapshotPriority.Component;
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Features;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Revolution.Snapshot.Systems.Instigators;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource.Components;
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
		public QueueNetworkedEntitySystem(WorldCollection collection) : base(collection)
		{
		}
		
		private EntityQuery networkedEntities,
		                    ownedEntities,
		                    // TODO: Should we really send resources like that? Should we restrict it to be server only?
		                    resourceEntities;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			networkedEntities = CreateEntityQuery(new[] {typeof(NetworkedEntity)});
			ownedEntities     = CreateEntityQuery(new[] {typeof(SnapshotOwnedWriteArchetype)});
			resourceEntities  = CreateEntityQuery(new[] {typeof(IsResourceEntity)});

			AddDisposable(World.Mgr.Subscribe((in SendSnapshotSystem.PreSendEvent ev) =>
			{
				var broadcastInstigator = ev.Instigator;
				foreach (var handle in networkedEntities)
				{
					//Console.WriteLine($"queue {handle}");
					broadcastInstigator.QueuedEntities[Safe(handle)] = EntitySnapshotPriority.Archetype;

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

				foreach (var handle in resourceEntities)
				{
					if (HasComponent<SnapshotEntity>(handle) && !HasComponent<SnapshotEntity.CreatedByThisWorld>(handle))
						continue;
					
					//Console.WriteLine($"queue {handle.Id}");
					broadcastInstigator.QueuedEntities[Safe(handle)] = EntitySnapshotPriority.SendAtAllCost;
				}

				foreach (var handle in ownedEntities)
				{
					broadcastInstigator.QueuedEntities[Safe(handle)] = EntitySnapshotPriority.Component;
				}
			}));
		}
	}
}
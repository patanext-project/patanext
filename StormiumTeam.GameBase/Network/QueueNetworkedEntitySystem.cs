using System;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Features;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Revolution.Snapshot.Systems.Instigators;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Network
{
	public struct NetworkedEntity : IComponentData
	{
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

			networkedEntities ??= CreateEntityQuery(new[] {typeof(NetworkedEntity)});
			ownedEntities     ??= CreateEntityQuery(new[] {typeof(SnapshotOwnedWriteArchetype)});

			foreach (var (entity, feature) in features)
			{
				if (!entity.TryGet(out BroadcastInstigator broadcastInstigator))
					continue;

				foreach (var handle in networkedEntities)
				{
					//Console.WriteLine($"queue {handle}");
					broadcastInstigator.QueuedEntities[Safe(handle)] = EntitySnapshotPriority.SendAtAllCost;
				}

				foreach (var handle in ownedEntities)
				{
					broadcastInstigator.QueuedEntities[Safe(handle)] = EntitySnapshotPriority.Component;
				}
			}
		}
	}
}
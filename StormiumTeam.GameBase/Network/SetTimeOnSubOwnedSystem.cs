using System;
using System.Diagnostics;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Revolution.NetCode.Components;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.Network
{
	[UpdateAfter(typeof(UpdateDriverSystem))]
	public class SetTimeOnSubOwnedSystem : GameAppSystem
	{
		private struct SetBySystem : IComponentData
		{
		}

		private Scheduler scheduler;

		private Action<(GameEntityHandle, GameEntityHandle)> assignDelegate;
		private Action<GameEntityHandle>                     removeDelegate;

		public SetTimeOnSubOwnedSystem(WorldCollection collection) : base(collection)
		{
			scheduler = new Scheduler();

			assignDelegate = assign;
			removeDelegate = remove;
		}

		private EntityQuery? ownedQuery;
		private EntityQuery? nonOwnedQuery;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var snapshotEntityAccessor = GetAccessor<SnapshotEntity>();
			foreach (var entityHandle in ownedQuery ??= CreateEntityQuery(new[]
			{
				typeof(SnapshotOwnedWriteArchetype),
				typeof(SnapshotEntity)
			}, new[]
			{
				typeof(NetReportTime),
				typeof(SetBySystem)
			}))
			{
				ref readonly var snapshotEntity = ref snapshotEntityAccessor[entityHandle];
				Debug.Assert(snapshotEntity.Storage.IsAlive, "snapshotEntity.Storage.IsAlive");
				
				if (!snapshotEntity.Storage.TryGet(out GameEntity gameEntity))
					continue;
				
				scheduler.Schedule(assignDelegate, (entityHandle, gameEntity.Handle), default);
			}

			foreach (var entityHandle in nonOwnedQuery ??= CreateEntityQuery(new[]
			{
				typeof(NetReportTime),
				typeof(SetBySystem)
			}, new[]
			{
				typeof(SnapshotOwnedWriteArchetype)
			}))
			{
				scheduler.Schedule(removeDelegate, entityHandle, default);
			}

			scheduler.Run();
		}

		private void assign((GameEntityHandle entityHandle, GameEntityHandle ownerHandle) args)
		{
			GameWorld.AssignComponent(args.entityHandle, GameWorld.GetComponentReference<NetReportTime>(args.ownerHandle));
			GameWorld.AddComponent(args.entityHandle, new SetBySystem());
		}

		private void remove(GameEntityHandle entityHandle)
		{
			GameWorld.RemoveComponent(entityHandle, AsComponentType<NetReportTime>());
		}
	}
}
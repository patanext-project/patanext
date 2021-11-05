using System;
using System.Collections.Generic;
using System.Numerics;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.LLAPI.Systems;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Simulation.Features.ShareWorldState;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using JetBrains.Annotations;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Network;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Simulation.Client.Systems
{
	[UpdateAfter(typeof(UpdateDriverSystem))]
	[UpdateBefore(typeof(SendWorldStateSystem))]
	public class InterpolateForeignUnitsPosition : GameAppSystem
	{
		private InstigatorTimeSystem instigatorTimeSystem;
		
		public InterpolateForeignUnitsPosition([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref instigatorTimeSystem);
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery       toInterpolateQuery;
		private IManagedWorldTime worldTime;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			toInterpolateQuery = CreateEntityQuery(new[]
			{
				typeof(SnapshotEntity),
				typeof(Position),
				typeof(Position.Snapshot)
			}, new []
			{
				typeof(SimulationAuthority),
				typeof(SnapshotEntity.CreatedByThisWorld)
			});
		}

		struct PositionBuffer
		{
			public bool                             IsPlaying;
			public List<(Vector3 pos, Vector3 vel)> Queue;

			public Vector3 LastPos;
			public Vector3 LastVel;
			public Vector3 Pos;
			public Vector3 Vel;

			public float LastTime;
			public float Time;
			public float Mark;
		}

		private PositionBuffer[] buffers = Array.Empty<PositionBuffer>();

		protected override void OnUpdate()
		{
			base.OnUpdate();

			var snapshotEntityAccessor = GetAccessor<SnapshotEntity>();
			var snapshotBufferAccessor = GetBufferAccessor<Position.Snapshot>();
			var positionAccessor       = GetAccessor<Position>();
			foreach (var entity in toInterpolateQuery)
			{
				var (isDefault, time)    =  instigatorTimeSystem.GetTime(snapshotEntityAccessor[entity].Instigator);
				time.currentInterpolated -= TimeSpan.FromSeconds(time.delta);

				var snapshots   = snapshotBufferAccessor[entity];
				var position = positionAccessor[entity];

				ref var buffer = ref GameWorld.Boards.Entity.GetColumn(entity.Id, ref buffers);
				if (buffer.Queue is { } queue)
				{
					queue.Add((position.Value, default));
				}
				
				if (buffer.IsPlaying == false)
				{
					buffer.Queue = new List<(Vector3 pos, Vector3 vel)>();
				}
				else
				{
					
				}
			}
		}

		/*private Position.Snapshot[] beginFrames   = Array.Empty<Position.Snapshot>();
		private Position.Snapshot[] endFrames     = Array.Empty<Position.Snapshot>();
		private float[]             previousDelta = Array.Empty<float>();
		protected override void OnUpdate()
		{
			base.OnUpdate();
			
			GameWorld.Boards.Entity.GetColumn(0, ref beginFrames);
			GameWorld.Boards.Entity.GetColumn(0, ref endFrames);
			GameWorld.Boards.Entity.GetColumn(0, ref previousDelta);

			var snapshotEntityAccessor = GetAccessor<SnapshotEntity>();
			var snapshotBufferAccessor = GetBufferAccessor<Position.Snapshot>();
			var positionAccessor       = GetAccessor<Position>();
			foreach (var entity in toInterpolateQuery)
			{
				var (isDefault, time)    =  instigatorTimeSystem.GetTime(snapshotEntityAccessor[entity].Instigator);
				time.currentInterpolated -= TimeSpan.FromSeconds(time.delta);

				var buffer   = snapshotBufferAccessor[entity];
				var position = positionAccessor[entity];

				for (var i = 0; i < buffer.Count && buffer.Count > 25; i++)
				{
					var frame = buffer[i];
					if (beginFrames[entity.Id].Tick == 0 || previousDelta[entity.Id] >= 1)
					{
						if (frame.Tick > endFrames[entity.Id].Tick && frame.Tick >= time.interpolatedFrame - 10)
						{
							if (i + 11 <= buffer.Count)
							{
								beginFrames[entity.Id]   = frame;
								endFrames[entity.Id]     = buffer[i + 10];
								previousDelta[entity.Id] = 0;
							}
						}
					}
				}


				if (beginFrames[entity.Id].Tick != 0)
				{
					Position f = default, l = default;
					beginFrames[entity.Id].ToComponent(ref f, default);
					endFrames[entity.Id].ToComponent(ref l, default);

					position.Value           =  Vector3.Lerp(f.Value, l.Value, previousDelta[entity.Id]);
					previousDelta[entity.Id] += (float) (time.delta / worldTime.Delta.TotalSeconds / (endFrames[entity.Id].Tick - beginFrames[entity.Id].Tick));
				}
			}
		}*/
	}
}
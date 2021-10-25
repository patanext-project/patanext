using System;
using System.Diagnostics.CodeAnalysis;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace StormiumTeam.GameBase.GamePlay
{
	/// <summary>
	/// Get team enemies of this team
	/// </summary>
	public struct TeamEntityContainer : IComponentBuffer
	{
		public GameEntity Value;

		public TeamEntityContainer(GameEntity entity) => Value = entity;
		
		public struct Snapshot : IReadWriteSnapshotData<Snapshot, GhostSetup>, ISnapshotSyncWithComponent<TeamEntityContainer, GhostSetup>
		{
			public uint Tick { get; set; }

			public Ghost Ghost;
			public void Serialize(in     BitBuffer           buffer,    in Snapshot   baseline, in GhostSetup setup)
			{
				buffer.AddGhostDelta(Ghost, baseline.Ghost);
			}

			public void Deserialize(in   BitBuffer           buffer,    in Snapshot   baseline, in GhostSetup setup)
			{
				Ghost = buffer.ReadGhostDelta(baseline.Ghost);
			}

			public void FromComponent(in TeamEntityContainer component, in GhostSetup setup)
			{
				Ghost = setup.ToGhost(component.Value);
			}

			public void ToComponent(ref  TeamEntityContainer component, in GhostSetup setup)
			{
				component.Value = setup.FromGhost(Ghost);
			}
		}

		public class Serializer : DeltaBufferSerializerBase<Snapshot, TeamEntityContainer, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}

	public class BuildTeamEntityContainerSystem : GameAppSystem, IPreUpdateSimulationPass
	{
		public BuildTeamEntityContainerSystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery teamQuery, childQuery;

		public void OnBeforeSimulationUpdate()
		{
			var bufferAccessor = GetBufferAccessor<TeamEntityContainer>();
			foreach (var teamHandle in teamQuery ??= CreateEntityQuery(new[] {typeof(TeamEntityContainer)}))
				bufferAccessor[teamHandle].Clear();

			foreach (var child in childQuery ??= CreateEntityQuery(new[] {typeof(Relative<TeamDescription>)}))
			{
				var teamHandle = GetComponentData<Relative<TeamDescription>>(child).Handle;
				if (!teamQuery.MatchAgainst(teamHandle))
					continue;
				
				bufferAccessor[teamHandle].Add(new TeamEntityContainer(Safe(child)));
			}
		}
	}
}
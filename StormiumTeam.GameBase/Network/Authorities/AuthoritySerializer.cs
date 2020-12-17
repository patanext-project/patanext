using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Network.Authorities
{
	public class AuthoritySerializer<TComponent> : SerializerBase
		where TComponent : struct, IComponentData
	{
		public static IAuthorityArchetype CreateAuthorityArchetype(GameWorld gameWorld)
		{
			return new SimpleAuthorityArchetype(gameWorld, 
				gameWorld.AsComponentType<SetRemoteAuthority<TComponent>>(),
				gameWorld.AsComponentType<TComponent>(),
				gameWorld.AsComponentType<ForceTemporaryAuthority<TComponent>>());
		}
		
		public AuthoritySerializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context context) : base(instigator, context)
		{
		}

		public override void UpdateMergeGroup(ReadOnlySpan<Entity> clients, MergeGroupCollection collection)
		{
		}

		protected override ISerializerArchetype GetSerializerArchetype()
		{
			return null;
		}

		protected override IAuthorityArchetype GetAuthorityArchetype()
		{
			return CreateAuthorityArchetype(GameWorld);
		}

		protected override void OnSerialize(BitBuffer bitBuffer, SerializationParameters parameters, MergeGroup @group, ReadOnlySpan<GameEntityHandle> entities)
		{
		}

		protected override void OnDeserialize(BitBuffer bitBuffer, DeserializationParameters parameters, ISerializer.RefData refData)
		{
		}
	}
}
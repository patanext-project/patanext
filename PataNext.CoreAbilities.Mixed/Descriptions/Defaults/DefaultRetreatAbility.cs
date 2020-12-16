using System.Diagnostics.CodeAnalysis;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;

namespace PataNext.CoreAbilities.Mixed.Defaults
{
	public struct DefaultRetreatAbility : IComponentData
	{
		public const float StopTime      = 1.5f;
		public const float MaxActiveTime = StopTime + 0.5f;

		public int LastActiveId;

		public float AccelerationFactor;
		public float StartPosition;
		public float BackVelocity;
		public bool  IsRetreating;
		public float ActiveTime;

		public class Register : RegisterGameHostComponentData<DefaultRetreatAbility>
		{
		}
		
		public class Serializer : ArchetypeOnlySerializerBase<DefaultRetreatAbility>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}

	public class DefaultRetreatAbilityProvider : BaseRuntimeRhythmAbilityProvider<DefaultRetreatAbility>
	{
		public DefaultRetreatAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "retreat";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<RetreatCommand>();
		}

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);
			GameWorld.GetComponentData<DefaultRetreatAbility>(entity) = new DefaultRetreatAbility
			{
				AccelerationFactor = 1
			};
		}
	}
}
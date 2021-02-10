using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.Defaults
{
	public struct DefaultJumpAbility : IComponentData
	{
		public int LastActiveId;

		public bool  IsJumping;
		public float ActiveTime;

		public class Register : RegisterGameHostComponentData<DefaultJumpAbility>
		{
		}

		public class Serializer : ArchetypeOnlySerializerBase<DefaultJumpAbility>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}

	public class DefaultJumpAbilityProvider : BaseRuntimeRhythmAbilityProvider<DefaultJumpAbility>
	{
		public DefaultJumpAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => resPath.Create(new [] { "ability", "default", "jump" }, ResPath.EType.MasterServer);

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<JumpCommand>();
		}
	}
}
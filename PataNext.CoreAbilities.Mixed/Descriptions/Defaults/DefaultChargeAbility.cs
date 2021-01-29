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

namespace PataNext.CoreAbilities.Mixed.Defaults
{
	public struct DefaultChargeAbility : IComponentData
	{
		public class Register : RegisterGameHostComponentData<DefaultChargeAbility>
		{
		}

		public class Serializer : ArchetypeOnlySerializerBase<DefaultChargeAbility>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}
	}

	public class DefaultChargeAbilityProvider : BaseRhythmAbilityProvider<DefaultChargeAbility>
	{
		public DefaultChargeAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override string MasterServerId => "charge";

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<ChargeCommand>();
		}
	}
}
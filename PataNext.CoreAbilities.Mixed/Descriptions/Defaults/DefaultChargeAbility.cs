using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;

namespace PataNext.CoreAbilities.Mixed.Defaults
{
	public struct DefaultChargeAbility : IComponentData
	{
		public class Register : RegisterGameHostComponentData<DefaultChargeAbility>
		{
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
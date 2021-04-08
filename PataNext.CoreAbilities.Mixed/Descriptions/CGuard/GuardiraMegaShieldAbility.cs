using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.CoreAbilities.Mixed.Descriptions;
using PataNext.CoreAbilities.Mixed.Subset;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CGuard
{
	public struct GuardiraMegaShieldAbility : IComponentData
	{
		
	}

	public class GuardiraMegaShieldAbilityProvider : BaseRuntimeRhythmAbilityProvider<GuardiraMegaShieldAbility>
	{
		public GuardiraMegaShieldAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override    string MasterServerId => resPath.GetAbility("guard", "mega_shield");
		protected override string FilePathPrefix => "guard";

		public override ComponentType GetChainingCommand()
		{
			return AsComponentType<DefendCommand>();
		}

		public override ComponentType[] GetHeroModeAllowedCommands()
		{
			return new[] {GameWorld.AsComponentType<MarchCommand>()};
		}
		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);

			entityComponents.Add(GameWorld.AsComponentType<DefaultSubsetMarch>());
		}

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			GameWorld.GetComponentData<AbilityActivation>(entity).Type = EAbilityActivationType.HeroMode | EAbilityActivationType.Alive;
			GameWorld.GetComponentData<DefaultSubsetMarch>(entity) = new DefaultSubsetMarch
			{
				Target             = DefaultSubsetMarch.ETarget.Cursor,
				AccelerationFactor = 0.75f
			};
		}
	}
}
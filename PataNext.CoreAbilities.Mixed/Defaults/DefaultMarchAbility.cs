using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.CoreAbilities.Mixed.Subset;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;

namespace PataNext.CoreAbilities.Mixed.Defaults
{
	/// <summary>
	/// The default march ability
	/// </summary>
	/// <remarks>
	///	If you wish to modify the AccelerationFactor, do it so in the attached <see cref="DefaultMarchAbility"/> component
	/// </remarks>
	public struct DefaultMarchAbility : IComponentData
	{
		public class Register : RegisterGameHostComponentData<DefaultMarchAbility>
		{}
	}

	public class DefaultMarchAbilityProvider : BaseRhythmAbilityProvider<DefaultMarchAbility>
	{
		public override string MasterServerId => "march";

		public DefaultMarchAbilityProvider(WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);
			entityComponents.Add(GameWorld.AsComponentType<DefaultSubsetMarch>());
		}

		public override void SetEntityData(GameEntity entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);

			GameWorld.GetComponentData<DefaultSubsetMarch>(entity) = new DefaultSubsetMarch
			{
				AccelerationFactor = 1,
				Target             = DefaultSubsetMarch.ETarget.All
			};
		}

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<MarchCommand>();
		}
	}

	public class DefaultMarchAbilitySystem : BaseAbilitySystem
	{
		public DefaultMarchAbilitySystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery abilityQuery;

		public override void OnAbilityUpdate()
		{
			var abilityAccessor = new ComponentDataAccessor<AbilityState>(GameWorld);
			var subsetAccessor = new ComponentDataAccessor<DefaultSubsetMarch>(GameWorld);
			foreach (var entity in (abilityQuery ??= CreateEntityQuery(new[]
			{
				typeof(DefaultMarchAbility),
				typeof(DefaultSubsetMarch),
				typeof(AbilityState)
			})))
			{
				subsetAccessor[entity].IsActive = abilityAccessor[entity].IsActive;
			}
		}
	}
}
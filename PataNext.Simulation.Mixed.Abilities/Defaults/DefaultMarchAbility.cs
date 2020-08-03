using System.Collections.Generic;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Simulation.Mixed.Abilities.Subset;
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine.DefaultCommands;

namespace PataNext.Simulation.Mixed.Abilities.Defaults
{
	/// <summary>
	/// The default march ability
	/// </summary>
	/// <remarks>
	///	If you wish to modify the AccelerationFactor, do it so in the attached <see cref="DefaultMarchAbility"/> component
	/// </remarks>
	public struct DefaultMarchAbility : IComponentData
	{
	}

	public class DefaultMarchAbilityProvider : BaseRhythmAbilityProvider<DefaultMarchAbility>
	{
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

			GameWorld.GetComponentData<DefaultSubsetMarch>(entity).AccelerationFactor = 1;
			GameWorld.GetComponentData<DefaultSubsetMarch>(entity).Target             = DefaultSubsetMarch.ETarget.All;
		}

		public override ComponentType GetChainingCommand()
		{
			return GameWorld.AsComponentType<MarchCommand>();
		}
	}

	public class DefaultMarchAbilitySystem : BaseAbilitySystem
	{
		private EntityQuery abilityQuery;

		public DefaultMarchAbilitySystem(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			abilityQuery = CreateEntityQuery(new[]
			{
				typeof(DefaultMarchAbility),
				typeof(DefaultSubsetMarch),
				typeof(AbilityState)
			});
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			foreach (var entity in abilityQuery.GetEntities())
			{
				var state = GetComponentData<AbilityState>(entity);
				GetComponentData<DefaultSubsetMarch>(entity).IsActive = (state.Phase & EAbilityPhase.Active) != 0;
			}
		}
	}
}
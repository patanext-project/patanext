using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreMissions.Server.Providers
{
	public class TowerBastionProvider : BastionDynamicGroupProvider
	{
		public TowerBastionProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);
			
			entityComponents.AddRange(new[]
			{
				AsComponentType<UnitTargetDescription>(),
				AsComponentType<Position>(),
				AsComponentType<UnitEnemySeekingState>()
			});
		}
	}
}
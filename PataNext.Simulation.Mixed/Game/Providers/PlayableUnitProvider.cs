using System.Diagnostics;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Providers
{
	public class PlayableUnitProvider : BaseProvider<PlayableUnitProvider.Create>
	{
		public struct Create
		{
			public UnitStatistics? Statistics;
			public UnitDirection   Direction;
		}

		public PlayableUnitProvider(WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(stackalloc[]
			{
				GameWorld.AsComponentType<UnitDescription>(),

				GameWorld.AsComponentType<UnitStatistics>(),
				GameWorld.AsComponentType<UnitPlayState>(),
				GameWorld.AsComponentType<UnitControllerState>(),
				GameWorld.AsComponentType<UnitDirection>(),
				GameWorld.AsComponentType<UnitTargetOffset>(),

				GameWorld.AsComponentType<GroundState>(),
				GameWorld.AsComponentType<Position>(),
				GameWorld.AsComponentType<Velocity>(),
				
				GameWorld.AsComponentType<OwnerActiveAbility>(),
				GameWorld.AsComponentType<OwnedRelative<AbilityDescription>>()
			});
		}

		public override void SetEntityData(GameEntity                entity, Create data)
		{
			Debug.Assert(data.Statistics != null, "data.Statistics != null");
			
			GameWorld.GetComponentData<UnitStatistics>(entity) = data.Statistics.Value;
			GameWorld.GetComponentData<UnitDirection>(entity) = data.Direction;
		}
	}
}
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Systems;

namespace PataNext.Module.Simulation.Game.Providers
{
	public class FreeRoamUnitProvider : PlayableUnitProvider
	{
		public FreeRoamUnitProvider(WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);

			entityComponents.AddRange(new[]
			{
				GameWorld.AsComponentType<UnitFreeRoamMovement>()
			});
		}
	}
}
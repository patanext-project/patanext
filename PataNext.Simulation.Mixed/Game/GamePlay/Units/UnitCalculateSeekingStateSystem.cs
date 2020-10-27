using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Native.Fixed;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.HLAPI;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.Game.GamePlay.Units
{
	public class UnitCalculateSeekingStateSystem : GameAppSystem, IUpdateSimulationPass
	{
		public UnitCalculateSeekingStateSystem(WorldCollection collection) : base(collection)
		{
		}

		private GameEntity testEntity;

        protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
        {
            base.OnDependenciesResolved(dependencies);

			testEntity = CreateEntity();
			AddComponent(testEntity, new Position {Value = {}});
			GameWorld.AddBuffer<UnitWeakPoint>(testEntity)
					 .Add(new UnitWeakPoint(new System.Numerics.Vector3 {Y = 1}));
        }

        

		private EntityQuery unitQuery;

		public void OnSimulationUpdate() 
		{
			foreach (var entity in unitQuery ??= CreateEntityQuery(new [] 
			{
				typeof(UnitEnemySeekingState)
			}))
			{
				ref var seekingState = ref GetComponentData<UnitEnemySeekingState>(entity);
				seekingState.Enemy = testEntity;
			}
		}
	}
}
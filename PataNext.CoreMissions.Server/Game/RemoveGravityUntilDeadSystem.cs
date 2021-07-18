using System;
using System.Collections.Generic;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Game.GamePlay.Units;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.CoreMissions.Server.Game
{
	public struct RemoveGravityUntilDead : IComponentData
	{
	}

	[UpdateBefore(typeof(UnitPhysicsSystem))]
	public class RemoveGravityUntilDeadSystem : GameAppSystem, IPostUpdateSimulationPass
	{
		public RemoveGravityUntilDeadSystem([NotNull] WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery           unitQuery;

		public void log(object obj)
		{
			Console.WriteLine("  JS: " + obj);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			unitQuery = CreateEntityQuery(new[] { typeof(UnitControllerState), typeof(RemoveGravityUntilDead) }, none: new[] { typeof(LivableIsDead) });
		}

		public void OnAfterSimulationUpdate()
		{
			var stateAccessor = GetAccessor<UnitControllerState>();
			foreach (var ent in unitQuery)
			{
				stateAccessor[ent].ControlOverVelocityY = true;
			}
		}
	}
}
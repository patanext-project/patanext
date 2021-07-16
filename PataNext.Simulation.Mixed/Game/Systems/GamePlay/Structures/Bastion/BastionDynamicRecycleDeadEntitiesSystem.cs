using System;
using System.Collections.Generic;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Worlds.Components;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.Structures.Bastion;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Structures.Bastion
{
	[UpdateBefore(typeof(BuildTeamEntityContainerSystem))] // Make sure that the buffer TeamEntityContainer is rebuilt after we destroy entities
	public class BastionDynamicRecycleDeadEntitiesSystem : GameAppSystem, IPreUpdateSimulationPass
	{
		private IManagedWorldTime worldTime;

		public BastionDynamicRecycleDeadEntitiesSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref worldTime);
		}

		private EntityQuery bastionQuery;
		private EntityQuery unitMask;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			bastionQuery = CreateEntityQuery(new[]
			{
				typeof(BastionDescription),
				typeof(BastionEntities)
			});

			unitMask = CreateEntityQuery(new[]
			{
				typeof(RemoveBastionUnitWhenDead),
				typeof(LivableIsDead)
			});
		}

		public void OnBeforeSimulationUpdate()
		{
			var dt = worldTime.Delta;

			unitMask.CheckForNewArchetypes();

			var entitiesAccessor  = GetBufferAccessor<BastionEntities>();
			var conditionAccessor = GetAccessor<RemoveBastionUnitWhenDead>();
			foreach (var entity in bastionQuery)
			{
				var buffer = entitiesAccessor[entity].Reinterpret<GameEntity>();
				for (var i = 0; i < buffer.Count; i++)
				{
					var remove = GameWorld.Exists(buffer[i]) == false;
					if (!remove && unitMask.MatchAgainst(buffer[i].Handle))
					{
						ref var delay = ref conditionAccessor[buffer[i].Handle].Delay;
						if (delay <= TimeSpan.Zero)
							remove = true;
						else
							delay -= dt;

						if (remove)
							RemoveEntity(buffer[i]);
					}
					
					if (remove)
						buffer.RemoveAt(i--);
				}
			}
		}
	}
}
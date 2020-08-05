using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace StormiumTeam.GameBase.SystemBase
{
	public abstract class BuildContainerSystem<TDescription> : GameAppSystem, IPreUpdateSimulationPass
		where TDescription : struct, IEntityDescription
	{
		public BuildContainerSystem(WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery ownerQuery, childQuery;
		
		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			ownerQuery = CreateEntityQuery(stackalloc []
			{
				GameWorld.AsComponentType<OwnedRelative<TDescription>>()
			});
			childQuery = CreateEntityQuery(stackalloc[]
			{
				GameWorld.AsComponentType<TDescription>(),
				GameWorld.AsComponentType<Owner>()
			});
		}

		public virtual bool AutoUpdate => true;

		public void OnBeforeSimulationUpdate()
		{
			if (!AutoUpdate)
				return;
			ForceUpdate();
		}

		public void ForceUpdate()
		{
			// We could actually get the buffer Componentboard for more linear performance
			foreach (var owner in ownerQuery.GetEntities())
				GameWorld.GetBuffer<OwnedRelative<TDescription>>(owner).Clear();

			foreach (var child in childQuery.GetEntities())
			{
				var owner = GetComponentData<Owner>(child).Target;
				if (!GameWorld.Contains(owner))
					throw new InvalidOperationException();
				GameWorld.GetBuffer<OwnedRelative<TDescription>>(owner).Add(new OwnedRelative<TDescription>(child));
			}
		}
	}
}
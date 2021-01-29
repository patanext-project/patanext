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

			ownerQuery = CreateEntityQuery(new[]
			{
				GameWorld.AsComponentType<OwnedRelative<TDescription>>()
			});
			childQuery = CreateEntityQuery(new[]
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
			var bufferAccessor = GetBufferAccessor<OwnedRelative<TDescription>>();
			foreach (var owner in ownerQuery.GetEnumerator())
				bufferAccessor[owner].Clear();

			foreach (var child in childQuery.GetEnumerator())
			{
				var owner = GetComponentData<Owner>(child).Target;
				if (!ownerQuery.MatchAgainst(owner.Handle))
					continue;

				bufferAccessor[owner.Handle].Add(new OwnedRelative<TDescription>(Safe(child)));
			}
		}
	}
}
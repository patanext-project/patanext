using System;
using System.Collections.Generic;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.Special.Squad;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.GamePlay.Special.Squad
{
	public class UpdateSquadUnitDisplacementSystem : GameAppSystem, IPreUpdateSimulationPass
	{
		private PooledList<GameEntity> validEntities = new();

		public UpdateSquadUnitDisplacementSystem([NotNull] WorldCollection collection) : base(collection)
		{
			AddDisposable(validEntities);
		}

		private EntityQuery squadQuery;
		private EntityQuery unitMask;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			squadQuery = CreateEntityQuery(new[] { typeof(AutoSquadUnitDisplacement), typeof(SquadEntityContainer) });
			unitMask   = CreateEntityQuery(new[] { typeof(UnitTargetOffset) }, none: new[] { typeof(LivableIsDead) });
		}

		public void OnBeforeSimulationUpdate()
		{
			unitMask.CheckForNewArchetypes();

			var indexFromSquadComponentType = AsComponentType<InGameSquadOffset>();

			var displacementAccessor = GetAccessor<AutoSquadUnitDisplacement>();
			var containerAccessor    = GetBufferAccessor<SquadEntityContainer>();
			var offsetAccessor       = GetAccessor<UnitTargetOffset>();
			foreach (var squadEntity in squadQuery)
			{
				ref readonly var displacement = ref displacementAccessor[squadEntity];
				var              buffer       = containerAccessor[squadEntity].Reinterpret<GameEntity>();

				var initialOffset = 0f;
				if (HasComponent(squadEntity, indexFromSquadComponentType))
					initialOffset = GameWorld.GetComponentData<InGameSquadOffset>(squadEntity, indexFromSquadComponentType).Value * 1.5f;

				validEntities.Clear();
				foreach (var ent in buffer)
					if (unitMask.MatchAgainst(ent.Handle))
						validEntities.Add(ent);

				var length = validEntities.Count;
				for (var unitIdx = 0; unitIdx < length; unitIdx++)
				{
					var unit = validEntities[unitIdx];

					ref var offset = ref offsetAccessor[unit.Handle];
					offset.Idle   = initialOffset + UnitTargetOffset.CenterComputeV1(unitIdx, length, displacement.Space);
					offset.Attack = UnitTargetOffset.CenterComputeV1(unitIdx, length, displacement.Space);
				}
			}
		}
	}
}
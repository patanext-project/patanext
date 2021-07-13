using System.Collections.Generic;
using System.Numerics;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GameModes.Payload;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.GameModes.Versus
{
	public class PayloadMoveSystem : GameAppSystem
	{
		public PayloadMoveSystem([NotNull] WorldCollection collection) : base(collection)
		{
		}

		private EntityQuery payloadQuery,
		                    unitQuery,
		                    payloadTeamMask;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			payloadQuery    = CreateEntityQuery(new[] {typeof(MovablePayload), typeof(Position), typeof(Velocity), typeof(Relative<TeamDescription>)});
			unitQuery       = CreateEntityQuery(new[] {typeof(Position), typeof(Relative<TeamDescription>)});
			payloadTeamMask = CreateEntityQuery(new[] {typeof(TeamEntityContainer)});
		}

		/// <summary>
		/// Update the system (should be called by the gamemode)
		/// </summary>
		public void Update()
		{
			if (!CanUpdate())
				return;

			payloadTeamMask.CheckForNewArchetypes();

			var teamAccessor     = GetAccessor<Relative<TeamDescription>>();
			var payloadAccessor  = GetAccessor<MovablePayload>();
			var positionAccessor = GetAccessor<Position>();
			var velocityAccessor = GetAccessor<Velocity>();
			foreach (var handle in payloadQuery)
			{
				ref readonly var teamEntity = ref teamAccessor[handle].Target;
				if (!payloadTeamMask.MatchAgainst(teamEntity.Handle))
					continue;

				ref var payload  = ref payloadAccessor[handle];
				ref var position = ref positionAccessor[handle].Value;
				ref var velocity = ref velocityAccessor[handle].Value;

				payload.CurrentSpeed = 0;

				var entityBuffer = GetBuffer<TeamEntityContainer>(teamEntity.Handle);
				foreach (var enemyEntity in entityBuffer.Reinterpret<GameEntity>())
				{
					if (Vector3.Distance(positionAccessor[enemyEntity.Handle].Value, position) <= payload.CaptureRadius)
					{
						// The first player should give a relative large speed, but the more there is player, the more it should be slow
						if (payload.CurrentSpeed == 0)
							payload.CurrentSpeed = 1;
						else
							payload.CurrentSpeed += 0.5f;
					}
				}

				velocity = new Vector3(payload.CurrentSpeed * payload.SpeedFactor, 0, 0);
			}
		}
	}
}
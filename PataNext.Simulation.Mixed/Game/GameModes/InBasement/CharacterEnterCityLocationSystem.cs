using System;
using System.Collections.Generic;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Revolution.NetCode.Components;
using GameHost.Simulation.Utility.EntityQuery;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GameModes.City;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.Module.Simulation.GameModes.InBasement
{
	[UpdateAfter(typeof(AtCityGameModeSystem))]
	public class CharacterEnterCityLocationSystem : GameAppSystem
	{
		private IPhysicsSystem      physicsSystem;
		private NetReportTimeSystem reportTimeSystem;

		public CharacterEnterCityLocationSystem([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref physicsSystem);
			DependencyResolver.Add(() => ref reportTimeSystem);
		}

		private EntityQuery characterQuery, locationQuery;
		private EntityQuery validPlayerQuery;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			characterQuery = CreateEntityQuery(new[] { typeof(UnitFreeRoamMovement), typeof(Position), typeof(PhysicsCollider), typeof(Relative<PlayerDescription>) });
			locationQuery  = CreateEntityQuery(new[] { typeof(CityLocationTag), typeof(PhysicsCollider) });

			validPlayerQuery = CreateEntityQuery(new[] { typeof(PlayerDescription), typeof(FreeRoamInputComponent), typeof(PlayerCurrentCityLocation) });
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			validPlayerQuery.CheckForNewArchetypes();

			var positionAccessor            = GetAccessor<Position>();
			var relativePlayerAccessor      = GetAccessor<Relative<PlayerDescription>>();
			var inputAccessor               = GetAccessor<FreeRoamInputComponent>();
			var currentCityLocationAccessor = GetAccessor<PlayerCurrentCityLocation>();
			foreach (var location in locationQuery)
			{
				foreach (var character in characterQuery)
				{
					ref readonly var player = ref relativePlayerAccessor[character].Target;
					if (!validPlayerQuery.MatchAgainst(player.Handle))
						continue;

					ref var currentLocation = ref currentCityLocationAccessor[player.Handle];

					var reportTime = reportTimeSystem.Get(player.Handle, out var fromEntity);
					var overlap    = physicsSystem.Overlap(character, location);
					
					if (overlap && inputAccessor[player.Handle].Down.HasBeenPressed(reportTime.Active) && !GameWorld.Exists(currentLocation.Entity))
					{
						Console.WriteLine("Enter Barracks!");
						currentLocation.Entity = Safe(location);
					}
				}
			}
		}
	}
}
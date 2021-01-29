using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Game.Providers;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.GameModes.DataCoopMission
{
	public class CoopMissionRhythmEngineProvider : BaseProvider<CoopMissionRhythmEngineProvider.Create>
	{
		public struct Create
		{
			public RhythmEngineProvider.Create Base;

			public GameEntity Player;
		}

		private RhythmEngineProvider parent;

		public CoopMissionRhythmEngineProvider([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref parent);
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			parent.GetComponents(entityComponents);

			entityComponents.AddRange(new[]
			{
				AsComponentType<Relative<PlayerDescription>>(),
				AsComponentType<SimulationAuthority>()
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			if (!GameWorld.Exists(data.Player)) throw new InvalidOperationException($"Player Entity {data.Player} doesn't exist!");

			parent.SetEntityData(entity, data.Base);

			GetComponentData<Relative<PlayerDescription>>(entity) = new Relative<PlayerDescription>(data.Player);
			GameWorld.Link(entity, data.Player.Handle, true);
		}
	}
}
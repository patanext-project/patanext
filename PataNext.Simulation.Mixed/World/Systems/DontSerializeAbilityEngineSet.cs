using System;
using System.Collections.Generic;
using GameHost.Core.Ecs;
using GameHost.Simulation.Application;
using GameHost.Simulation.Features.ShareWorldState;
using GameHost.Simulation.TabEcs;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;
using PataNext.Module.Simulation.Game.GamePlay.Abilities;
using RevolutionSnapshot.Core.Buffers;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Systems
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class DontSerializeAbilityEngineSet : GameAppSystem
	{
		private SendWorldStateSystem sendWorldStateSystem;
		
		public DontSerializeAbilityEngineSet(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref sendWorldStateSystem);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			sendWorldStateSystem.SetComponentSerializer(AsComponentType<AbilityEngineSet>(), new Empty());
			sendWorldStateSystem.SetComponentSerializer(AsComponentType<AbilityModifyStatsOnChaining>(), new Empty());
		}
		
		public class Empty : IShareComponentSerializer
		{
			public bool CanSerialize(GameWorld              world,  Span<GameEntityHandle> entities, ComponentBoardBase board)
			{
				return true;
			}

			public void SerializeBoard(ref DataBufferWriter buffer, GameWorld              world,    Span<GameEntityHandle>             entities, ComponentBoardBase board)
			{
			}
		}
	}
}
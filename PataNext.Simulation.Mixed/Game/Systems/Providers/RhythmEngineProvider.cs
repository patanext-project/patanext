using System;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Game.Providers
{
	public class RhythmEngineProvider : BaseProvider<RhythmEngineProvider.Create>
	{
		public struct Create
		{
			public RhythmEngineState State;
			public TimeSpan          StartTime;
			public TimeSpan?         BeatInterval;
			public int?              MaxBeats;
		}

		public RhythmEngineProvider([NotNull] WorldCollection collection) : base(collection)
		{
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			entityComponents.AddRange(new[]
			{
				AsComponentType<RhythmEngineDescription>(),
				AsComponentType<RhythmEngineController>(),
				AsComponentType<RhythmEngineSettings>(),
				AsComponentType<RhythmEngineLocalState>(),
				AsComponentType<RhythmEngineExecutingCommand>(),
				AsComponentType<RhythmEngineCommandProgressBuffer>(),
				AsComponentType<RhythmEnginePredictedCommandBuffer>(),
				AsComponentType<GameCommandState>(),
				AsComponentType<GameCombo.Settings>(),
				AsComponentType<GameCombo.State>(),
				AsComponentType<RhythmSummonEnergy>(),
				AsComponentType<RhythmSummonEnergyMax>(),
			});
		}

		public override void SetEntityData(GameEntityHandle entity, Create data)
		{
			GetComponentData<RhythmEngineController>(entity) = new RhythmEngineController
			{
				State     = data.State,
				StartTime = data.StartTime
			};
			GetComponentData<RhythmEngineSettings>(entity) = new RhythmEngineSettings
			{
				BeatInterval = data.BeatInterval ?? TimeSpan.FromSeconds(0.5),
				MaxBeat      = data.MaxBeats ?? 4
			};

			GameCombo.AddToEntity(GameWorld, entity);
			RhythmSummonEnergy.AddToEntity(GameWorld, entity);
		}
	}
}
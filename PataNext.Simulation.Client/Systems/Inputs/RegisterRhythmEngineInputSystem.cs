using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Game.Inputs.Actions;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.Providers;
using PataNext.Module.Simulation.Game.RhythmEngine;
using PataNext.Module.Simulation.Game.RhythmEngine.Systems;
using PataNext.Module.Simulation.Passes;
using PataNext.Module.Simulation.Resources;
using PataNext.Module.Simulation.Resources.Keys;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Time;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Simulation.Client.Systems.Inputs
{
	[UpdateAfter(typeof(SetGameTimeSystem))]
	[UpdateAfter(typeof(ReceiveInputDataSystem))]
	[UpdateBefore(typeof(ManageComponentTagSystem))]
	public class RegisterRhythmEngineInputSystem : AppSystem, IRhythmEngineSimulationPass
	{
		private InputDatabase     inputDatabase;
		private IManagedWorldTime time;

		private GameResourceDb<RhythmCommandResource, RhythmCommandResourceKey> localCommandDb;
		private GameResourceDb<UnitKitResource, UnitKitResourceKey> localKitDb;

		private PlayableUnitProvider    playableUnitProvider;
		private AbilityCollectionSystem abilityCollectionSystem;

		public RegisterRhythmEngineInputSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref inputDatabase);
			DependencyResolver.Add(() => ref gameWorld);
			DependencyResolver.Add(() => ref time);
			
			DependencyResolver.Add(() => ref playableUnitProvider);
			DependencyResolver.Add(() => ref abilityCollectionSystem);
		}

		private Dictionary<int, Entity> rhythmActionMap;
		private Entity                  panningAction;

		private GameWorld  gameWorld;
		private GameEntity gameEntityTest;

		private int spawnInFrame;
		
		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			spawnInFrame = 2;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (spawnInFrame == -999 || spawnInFrame-- > 0)
				return;
			spawnInFrame = -999;
			
			localCommandDb = new GameResourceDb<RhythmCommandResource, RhythmCommandResourceKey>(gameWorld);
			localKitDb     = new GameResourceDb<UnitKitResource, UnitKitResourceKey>(gameWorld);

			rhythmActionMap = new Dictionary<int, Entity>();
			for (var i = 0; i < 4; i++)
			{
				var input = i switch
				{
					0 => new CInput("keyboard/numpad4"),
					1 => new CInput("keyboard/numpad6"),
					2 => new CInput("keyboard/numpad2"),
					3 => new CInput("keyboard/numpad8"),
					_ => throw new InvalidOperationException()
				};
				rhythmActionMap[i] = inputDatabase.RegisterSingle<RhythmInputAction>(new RhythmInputAction.Layout("kb and mouse", input));
			}

			panningAction = inputDatabase.RegisterSingle<AxisAction>(new AxisAction.Layout("kb and mouse", new[] {new CInput("keyboard/leftArrow")}, new[] {new CInput("keyboard/rightArrow")}));

			gameEntityTest = gameWorld.CreateEntity();
			gameWorld.AddComponent(gameEntityTest, new PlayerDescription());
			gameWorld.AddComponent(gameEntityTest, new PlayerInputComponent());
			gameWorld.AddComponent(gameEntityTest, new PlayerIsLocal());

			var unitTarget = gameWorld.CreateEntity();
			gameWorld.AddComponent(unitTarget, new UnitTargetDescription());
			gameWorld.AddComponent(unitTarget, new Position());
			
			var unit = playableUnitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
			{
				Statistics = new UnitStatistics
				{
					BaseWalkSpeed = 2,
					FeverWalkSpeed = 2.2f,
					MovementAttackSpeed = 3.1f,
					Weight = 8.5f,
				},
				Direction  = UnitDirection.Right
			});
			gameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResourceKey("taterazay"))));
			gameWorld.AddComponent(unit, new Relative<PlayerDescription>(gameEntityTest));
			gameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(unitTarget));
			gameWorld.AddComponent(unit, new UnitTargetOffset());
			gameWorld.AddComponent(unit, new UnitTargetControlTag());

			var marchAbility = abilityCollectionSystem.SpawnFor("march", unit);

			var rhythmEngine = gameWorld.CreateEntity();
			gameWorld.AddComponent(rhythmEngine, new RhythmEngineDescription());
			gameWorld.AddComponent(rhythmEngine, new RhythmEngineController {State      = RhythmEngineState.Playing, StartTime = time.Total.Add(TimeSpan.FromSeconds(2))});
			gameWorld.AddComponent(rhythmEngine, new RhythmEngineSettings {BeatInterval = TimeSpan.FromSeconds(0.5), MaxBeat   = 4});
			gameWorld.AddComponent(rhythmEngine, new RhythmEngineLocalState());
			gameWorld.AddComponent(rhythmEngine, new RhythmEngineExecutingCommand());
			gameWorld.AddComponent(rhythmEngine, new Relative<PlayerDescription>(gameEntityTest));
			gameWorld.AddComponent(rhythmEngine, gameWorld.AsComponentType<RhythmEngineLocalCommandBuffer>());
			gameWorld.AddComponent(rhythmEngine, gameWorld.AsComponentType<RhythmEnginePredictedCommandBuffer>());
			gameWorld.AddComponent(rhythmEngine, new GameCommandState());
			gameWorld.AddComponent(rhythmEngine, new IsSimulationOwned());

			gameWorld.AddComponent(unit, new Relative<RhythmEngineDescription>(rhythmEngine));

			GameCombo.AddToEntity(gameWorld, rhythmEngine);
		}

		public void OnRhythmEngineSimulationPass()
		{
			if (spawnInFrame != -999)
				return;
			
			GameTime gameTime = default;
			foreach (var entity in gameWorld.QueryEntityWith(stackalloc [] {gameWorld.AsComponentType<GameTime>()}))
			{
				gameTime = gameWorld.GetComponentData<GameTime>(entity);
				break;
			}

			if (gameTime.Frame == default)
				return;
			
			ref var input = ref gameWorld.GetComponentData<PlayerInputComponent>(gameEntityTest);
			input.Panning = panningAction.Get<AxisAction>().Value;

			foreach (var kvp in rhythmActionMap)
			{
				var     rhythmAction = kvp.Value.Get<RhythmInputAction>();
				ref var action       = ref input.Actions[kvp.Key];

				action.IsActive  = rhythmAction.Active;
				action.IsSliding = (action.IsSliding && rhythmAction.UpCount > 0) || rhythmAction.IsSliding;

				if (rhythmAction.DownCount > 0)
					action.InterFrame.Pressed = gameTime.Frame;

				if (rhythmAction.UpCount > 0)
					action.InterFrame.Released = gameTime.Frame;
			}
		}
	}
}
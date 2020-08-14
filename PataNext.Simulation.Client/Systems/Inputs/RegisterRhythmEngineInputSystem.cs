using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using BepuUtilities;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Modules;
using GameHost.Core.Threading;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.IO;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using Newtonsoft.Json;
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
using PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine;
using StormiumTeam.GameBase.Camera.Components;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Time;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Simulation.Client.Systems.Inputs
{
	// TODO: Temporary struct until we have real JSON support for editing inputs
	public struct JInputSettings
	{
		public float    SliderSensibility;
		public string[] PataKeys;
		public string[] PonKeys;
		public string[] DonKeys;
		public string[] ChakaKeys;
		public string[] Ability0Keys;
		public string[] Ability1Keys;
		public string[] Ability2Keys;
		public string[] PanningNegativeKeys;
		public string[] PanningPositiveKeys;
	}

	[UpdateAfter(typeof(SetGameTimeSystem))]
	[UpdateAfter(typeof(ReceiveInputDataSystem))]
	[UpdateBefore(typeof(ManageComponentTagSystem))]
	public class RegisterRhythmEngineInputSystem : AppSystem, IRhythmEngineSimulationPass
	{
		private InputDatabase     inputDb;
		private IManagedWorldTime time;

		private PlayableUnitProvider    playableUnitProvider;
		private AbilityCollectionSystem abilityCollectionSystem;

		private IScheduler scheduler;
		private GameHostModule module;

		private JInputSettings inputSettings;

		public RegisterRhythmEngineInputSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref inputDb);
			DependencyResolver.Add(() => ref gameWorld);
			DependencyResolver.Add(() => ref time);

			DependencyResolver.Add(() => ref playableUnitProvider);
			DependencyResolver.Add(() => ref abilityCollectionSystem);
			
			DependencyResolver.Add(() => ref scheduler);
			DependencyResolver.Add(() => ref module);
		}

		private Dictionary<int, Entity> rhythmActionMap;
		private Entity                  panningAction;
		private Entity                  ability0Action;
		private Entity                  ability1Action;
		private Entity                  ability2Action;

		private GameWorld  gameWorld;
		private GameEntity gameEntityTest;

		private int spawnInFrame;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			spawnInFrame = 10;
			
			module.Storage.Subscribe((_, storage) =>
			{
				module.Storage.UnsubscribeCurrent();

				scheduler.Schedule(() => { CreateInputs(new StorageCollection {storage, module.DllStorage}.GetOrCreateDirectoryAsync("Inputs").Result); }, default);
			}, true);
		}

		private void CreateInputs(IStorage storage)
		{
			rhythmActionMap = new Dictionary<int, Entity>();
			
			using var stream = new MemoryStream(storage.GetFilesAsync("input.json").Result.First().GetContentAsync().Result);
			using var reader = new JsonTextReader(new StreamReader(stream));
			
			var serializer = new JsonSerializer();
			var input      = serializer.Deserialize<JInputSettings>(reader);
			Console.WriteLine(input.SliderSensibility);

			static CInput[] ct(IEnumerable<string> map) => map.Select(str => new CInput(str)).ToArray();

			const string lyt = "kb and mouse";
			rhythmActionMap[0] = inputDb.RegisterSingle<RhythmInputAction>(new RhythmInputAction.Layout(lyt, ct(input.PataKeys)));
			rhythmActionMap[1] = inputDb.RegisterSingle<RhythmInputAction>(new RhythmInputAction.Layout(lyt, ct(input.PonKeys)));
			rhythmActionMap[2] = inputDb.RegisterSingle<RhythmInputAction>(new RhythmInputAction.Layout(lyt, ct(input.DonKeys)));
			rhythmActionMap[3] = inputDb.RegisterSingle<RhythmInputAction>(new RhythmInputAction.Layout(lyt, ct(input.ChakaKeys)));
			
			ability0Action = inputDb.RegisterSingle<PressAction>(new PressAction.Layout(lyt, ct(input.Ability0Keys)));
			ability1Action = inputDb.RegisterSingle<PressAction>(new PressAction.Layout(lyt, ct(input.Ability1Keys)));
			ability2Action = inputDb.RegisterSingle<PressAction>(new PressAction.Layout(lyt, ct(input.Ability2Keys)));
			
			panningAction = inputDb.RegisterSingle<AxisAction>(new AxisAction.Layout(lyt, ct(input.PanningNegativeKeys), ct(input.PanningPositiveKeys)));

			inputSettings = input;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (spawnInFrame == -999 || spawnInFrame-- > 0)
				return;
			spawnInFrame = -999;

			var localKitDb    = new GameResourceDb<UnitKitResource, UnitKitResourceKey>(gameWorld);
			var localAttachDb = new GameResourceDb<UnitAttachmentResource, UnitAttachmentResourceKey>(gameWorld);
			var localEquipDb  = new GameResourceDb<EquipmentResource, EquipmentResourceKey>(gameWorld);
			
			gameEntityTest = gameWorld.CreateEntity();
			gameWorld.AddComponent(gameEntityTest, new PlayerDescription());
			gameWorld.AddComponent(gameEntityTest, new PlayerInputComponent());
			gameWorld.AddComponent(gameEntityTest, new PlayerIsLocal());

			var unitTarget = gameWorld.CreateEntity();
			gameWorld.AddComponent(unitTarget, new UnitTargetDescription());
			gameWorld.AddComponent(unitTarget, new Position());
			gameWorld.AddComponent(unitTarget, new Relative<PlayerDescription>(gameEntityTest));

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
			GameCombo.AddToEntity(gameWorld, rhythmEngine);
			RhythmSummonEnergy.AddToEntity(gameWorld, rhythmEngine);

			for (var i = 0; i != 1; i++)
			{
				var unit = playableUnitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
				{
					Statistics = new UnitStatistics
					{
						BaseWalkSpeed       = 2,
						FeverWalkSpeed      = 2.2f,
						MovementAttackSpeed = 3.1f,
						Weight              = 8.5f,
					},
					Direction = UnitDirection.Right
				});
				gameWorld.GetComponentData<Position>(unit).Value.X       += 8;
				gameWorld.GetComponentData<Position>(unitTarget).Value.X -= 8;

				gameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResourceKey("taterazay"))));
				gameWorld.AddComponent(unit, new Relative<PlayerDescription>(gameEntityTest));
				gameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(unitTarget));
				gameWorld.AddComponent(unit, new UnitEnemySeekingState());
				gameWorld.AddComponent(unit, new UnitTargetOffset());
				gameWorld.AddComponent(unit, new UnitTargetControlTag());

				var displayedEquip = gameWorld.AddBuffer<UnitDisplayedEquipment>(unit);
				displayedEquip.Add(new UnitDisplayedEquipment
				{
					Attachment = localAttachDb.GetOrCreate("Mask"),
					Resource   = localEquipDb.GetOrCreate("Masks/n_kibadda")
				});
				displayedEquip.Add(new UnitDisplayedEquipment
				{
					Attachment = localAttachDb.GetOrCreate("LeftEquipment"),
					Resource   = localEquipDb.GetOrCreate("Shields/default_shield")
				});
				displayedEquip.Add(new UnitDisplayedEquipment
				{
					Attachment = localAttachDb.GetOrCreate("RightEquipment"),
					Resource   = localEquipDb.GetOrCreate("Swords/default_sword")
				});

				abilityCollectionSystem.SpawnFor("march", unit);
				abilityCollectionSystem.SpawnFor("backward", unit);
				abilityCollectionSystem.SpawnFor("retreat", unit);
				abilityCollectionSystem.SpawnFor("jump", unit);
				abilityCollectionSystem.SpawnFor("party", unit);
				abilityCollectionSystem.SpawnFor("charge", unit);
				abilityCollectionSystem.SpawnFor("CTate.BasicDefendFrontal", unit);
				abilityCollectionSystem.SpawnFor("CTate.BasicDefendStay", unit, AbilitySelection.Top);
				abilityCollectionSystem.SpawnFor("CTate.EnergyField", unit);

				gameWorld.AddComponent(gameEntityTest, new ServerCameraState
				{
					Data =
					{
						Mode   = CameraMode.Forced,
						Offset = RigidTransform.Identity,
						Target = unit
					}
				});

				gameWorld.AddComponent(unit, new Relative<RhythmEngineDescription>(rhythmEngine));
			}

			// No favor 
			for (var i = 0; i != 32; i++)
			{
				var unit = playableUnitProvider.SpawnEntityWithArguments(new PlayableUnitProvider.Create
				{
					Statistics = new UnitStatistics
					{
						BaseWalkSpeed       = 0.75f,
						FeverWalkSpeed      = 0.75f,
						MovementAttackSpeed = 0.75f,
						Weight              = 8.5f,
					},
					Direction = UnitDirection.Right
				});

				var tt = gameWorld.CreateEntity();
				gameWorld.AddComponent(tt, new UnitTargetDescription());
				gameWorld.AddComponent(tt, new Position());
				gameWorld.GetComponentData<Position>(tt).Value.X = i * 10;

				gameWorld.AddComponent(unit, new UnitCurrentKit(localKitDb.GetOrCreate(new UnitKitResourceKey("yarida"))));
				gameWorld.AddComponent(unit, new UnitEnemySeekingState());
				gameWorld.AddComponent(unit, new UnitTargetOffset());
				gameWorld.AddComponent(unit, new Relative<UnitTargetDescription>(tt));

				var displayedEquip = gameWorld.AddBuffer<UnitDisplayedEquipment>(unit);
				displayedEquip.Add(new UnitDisplayedEquipment
				{
					Attachment = localAttachDb.GetOrCreate("Mask"),
					Resource   = localEquipDb.GetOrCreate("Masks/n_yarida")
				});
				displayedEquip.Add(new UnitDisplayedEquipment
				{
					Attachment = localAttachDb.GetOrCreate("RightEquipment"),
					Resource   = localEquipDb.GetOrCreate("Spears/default_spear")
				});
				displayedEquip.Add(new UnitDisplayedEquipment
				{
					Attachment = localAttachDb.GetOrCreate("LeftEquipment"),
					Resource   = localEquipDb.GetOrCreate("Masks/n_taterazay")
				});
			}
			
			/*Console.WriteLine("BEGIN");
			foreach (var pass in World.DefaultSystemCollection.Passes)
			{
				Console.WriteLine($"\t{pass.GetType()}");
				foreach (var element in pass.RegisteredObjects)
				{
					Console.WriteLine($"\t\t{element.GetType()}");
				}
			}

			Console.WriteLine("END");*/
		}

		private void SetAbility(in GameTime gameTime, ref PlayerInputComponent playerInputComponent, in AbilitySelection newSelection)
		{
			playerInputComponent.AbilityInterFrame.Pressed = gameTime.Frame;
			playerInputComponent.Ability                   = newSelection;
		}

		public void OnRhythmEngineSimulationPass()
		{
			if (spawnInFrame != -999)
				return;

			// inputs not created
			if (rhythmActionMap == null)
				return;

			GameTime gameTime = default;
			foreach (var entity in gameWorld.QueryEntityWith(stackalloc[] {gameWorld.AsComponentType<GameTime>()}))
			{
				gameTime = gameWorld.GetComponentData<GameTime>(entity);
				break;
			}

			if (gameTime.Frame == default)
				return;

			ref var input = ref gameWorld.GetComponentData<PlayerInputComponent>(gameEntityTest);
			input.Panning = panningAction.Get<AxisAction>().Value;

			if (ability0Action.Get<PressAction>().HasBeenPressed)
				SetAbility(in gameTime, ref input, AbilitySelection.Horizontal);
			else if (ability1Action.Get<PressAction>().HasBeenPressed)
				SetAbility(in gameTime, ref input, AbilitySelection.Top);
			else if (ability2Action.Get<PressAction>().HasBeenPressed)
				SetAbility(in gameTime, ref input, AbilitySelection.Bottom);

			foreach (var kvp in rhythmActionMap)
			{
				var     rhythmAction = kvp.Value.Get<RhythmInputAction>();
				ref var action       = ref input.Actions[kvp.Key];

				action.IsActive  = rhythmAction.Active;
				action.IsSliding = (action.IsSliding && rhythmAction.UpCount > 0) || rhythmAction.ActiveTime.TotalSeconds >= inputSettings.SliderSensibility;

				if (rhythmAction.DownCount > 0)
					action.InterFrame.Pressed = gameTime.Frame;

				if (rhythmAction.UpCount > 0)
					action.InterFrame.Released = gameTime.Frame;
			}
		}
	}
}
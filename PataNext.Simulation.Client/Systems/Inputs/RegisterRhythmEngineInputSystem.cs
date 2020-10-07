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
using StormiumTeam.GameBase.SystemBase;
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
	public class RegisterRhythmEngineInputSystem : GameAppSystem, IRhythmEngineSimulationPass
	{
		private InputDatabase     inputDb;
		private IManagedWorldTime time;

		private PlayableUnitProvider    playableUnitProvider;
		private AbilityCollectionSystem abilityCollectionSystem;

		private IScheduler     scheduler;
		private GameHostModule module;

		private JInputSettings inputSettings;

		public RegisterRhythmEngineInputSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref inputDb);
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

		private void SetAbility(in GameTime gameTime, ref GameRhythmInputComponent playerInputComponent, in AbilitySelection newSelection)
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
			foreach (var entity in GameWorld.QueryEntityWith(stackalloc[] {GameWorld.AsComponentType<GameTime>()}))
			{
				gameTime = GameWorld.GetComponentData<GameTime>(entity);
				break;
			}

			if (gameTime.Frame == default)
				return;

			var query = GameWorld.QueryEntityWith(stackalloc[] {AsComponentType<GameRhythmInputComponent>()});
			if (!query.TryGetFirst(out var playerEntity))
				return;

			ref var input = ref GameWorld.GetComponentData<GameRhythmInputComponent>(playerEntity);
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
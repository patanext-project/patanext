using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.Simulation.Utility.EntityQuery;
using PataNext.Game.Inputs.Actions;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Game.RhythmEngine.Systems;
using PataNext.Module.Simulation.Passes;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.Time;
using StormiumTeam.GameBase.Time.Components;

namespace PataNext.Simulation.Client.Systems.Inputs
{
	public struct RhythmInputDescription
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
	public class RegisterRhythmEngineInputSystem : RegisterInputSystemBase<RhythmInputDescription>, IRhythmEngineSimulationPass
	{
		public RegisterRhythmEngineInputSystem(WorldCollection collection) : base(collection)
		{
		}

		private Dictionary<int, Entity> rhythmActionMap;
		private Entity                  panningAction;
		private Entity                  ability0Action;
		private Entity                  ability1Action;
		private Entity                  ability2Action;

		protected override void CreateInputs(in RhythmInputDescription input, bool isUpdate)
		{
			rhythmActionMap ??= new Dictionary<int, Entity>
			{
				{0, default},
				{1, default},
				{2, default},
				{3, default}
			};

			static CInput[] ct(IEnumerable<string> map) => map.Select(str => new CInput(str)).ToArray();

			const string lyt = "kb and mouse";
			rhythmActionMap[0] = InputDatabase.UpdateSingle<RhythmInputAction>(rhythmActionMap[0], new RhythmInputAction.Layout(lyt, ct(input.PataKeys)));
			rhythmActionMap[1] = InputDatabase.UpdateSingle<RhythmInputAction>(rhythmActionMap[1], new RhythmInputAction.Layout(lyt, ct(input.PonKeys)));
			rhythmActionMap[2] = InputDatabase.UpdateSingle<RhythmInputAction>(rhythmActionMap[2], new RhythmInputAction.Layout(lyt, ct(input.DonKeys)));
			rhythmActionMap[3] = InputDatabase.UpdateSingle<RhythmInputAction>(rhythmActionMap[3], new RhythmInputAction.Layout(lyt, ct(input.ChakaKeys)));

			InputDatabase.UpdateSingle<PressAction>(ref ability0Action, new PressAction.Layout(lyt, ct(input.Ability0Keys)));
			InputDatabase.UpdateSingle<PressAction>(ref ability1Action, new PressAction.Layout(lyt, ct(input.Ability1Keys)));
			InputDatabase.UpdateSingle<PressAction>(ref ability2Action, new PressAction.Layout(lyt, ct(input.Ability2Keys)));

			InputDatabase.UpdateSingle<AxisAction>(ref panningAction, new AxisAction.Layout(lyt, ct(input.PanningNegativeKeys), ct(input.PanningPositiveKeys)));
		}

		private void SetAbility(in GameTime gameTime, ref GameRhythmInputComponent playerInputComponent, in AbilitySelection newSelection)
		{
			playerInputComponent.AbilityInterFrame.Pressed = gameTime.Frame;
			playerInputComponent.Ability                   = newSelection;
		}

		public void OnRhythmEngineSimulationPass()
		{
			// inputs not created
			if (rhythmActionMap == null)
				return;

			if (!GameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			var playerQuery = GameWorld.QueryEntityWith(stackalloc[]
			{
				AsComponentType<PlayerIsLocal>(),
				AsComponentType<GameRhythmInputComponent>()
			});
			if (!playerQuery.TryGetFirst(out var playerEntity))
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
				action.IsSliding = (action.IsSliding && rhythmAction.UpCount > 0) || rhythmAction.ActiveTime.TotalSeconds >= InputSettings.SliderSensibility;

				if (rhythmAction.DownCount > 0)
					action.InterFrame.Pressed = gameTime.Frame;

				if (rhythmAction.UpCount > 0)
					action.InterFrame.Released = gameTime.Frame;
			}
		}
	}
}
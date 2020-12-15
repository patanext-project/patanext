
using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.Revolution.Snapshot.Systems.Components;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Time;
using PataNext.Module.Simulation.Components;
using PataNext.Module.Simulation.Game.RhythmEngine.Systems;
using StormiumTeam.GameBase.Network.Authorities;
using StormiumTeam.GameBase.Time;

namespace PataNext.Simulation.Client.Systems.Inputs
{
	public struct FreeRoamInputDescription
	{
		public string[] HorizontalNegativeKeys;
		public string[] HorizontalPositiveKeys;
		public string[] JumpKeys;
		public string[] CrouchKeys;

		public string[] ConfirmKeys;
		public string[] CancelKeys;
	}

	[UpdateAfter(typeof(SetGameTimeSystem))]
	[UpdateAfter(typeof(ReceiveInputDataSystem))]
	[UpdateBefore(typeof(ManageComponentTagSystem))]
	public class RegisterFreeRoamInputSystem : RegisterInputSystemBase<FreeRoamInputDescription>
	{
		public RegisterFreeRoamInputSystem(WorldCollection collection) : base(collection)
		{
		}

		private Entity horizontalAction;

		private Entity jumpAction;
		private Entity crouchAction;

		private Entity confirmAction;
		private Entity cancelAction;

		private bool created;
		protected override void CreateInputs(in FreeRoamInputDescription input, bool isUpdate)
		{
			created = true;
			
			static CInput[] ct(IEnumerable<string> map) => map?.Select(str => new CInput(str)).ToArray() ?? Array.Empty<CInput>();

			const string lyt = "kb and mouse";
			InputDatabase.UpdateSingle<AxisAction>(ref horizontalAction, new AxisAction.Layout(lyt, ct(input.HorizontalNegativeKeys), ct(input.HorizontalPositiveKeys)));

			InputDatabase.UpdateSingle<PressAction>(ref jumpAction, new PressAction.Layout(lyt, ct(input.JumpKeys)));
			InputDatabase.UpdateSingle<PressAction>(ref crouchAction, new PressAction.Layout(lyt, ct(input.CrouchKeys)));

			InputDatabase.UpdateSingle<PressAction>(ref confirmAction, new PressAction.Layout(lyt, ct(input.ConfirmKeys)));
			InputDatabase.UpdateSingle<PressAction>(ref cancelAction, new PressAction.Layout(lyt, ct(input.CancelKeys)));
		}

		private EntityQuery playerQuery;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (!created || !GameWorld.TryGetSingleton(out GameTime gameTime))
				return;

			var inputAccessor = GetAccessor<FreeRoamInputComponent>();
			foreach (var player in playerQuery ??= CreateEntityQuery(new[]
			{
				typeof(FreeRoamInputComponent),
				typeof(InputAuthority)
			}))
			{
				ref var input = ref inputAccessor[player];
				input.HorizontalMovement = horizontalAction.Get<AxisAction>().Value;
				if (jumpAction.Get<PressAction>().DownCount > 0)
					input.Up.Pressed = gameTime.Frame;
				if (jumpAction.Get<PressAction>().UpCount > 0)
					input.Up.Released = gameTime.Frame;
			}
		}
	}
}
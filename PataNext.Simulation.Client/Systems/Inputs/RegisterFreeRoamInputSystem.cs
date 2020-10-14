
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using PataNext.Module.Simulation.Game.RhythmEngine.Systems;
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

		protected override void CreateInputs(in FreeRoamInputDescription input, bool isUpdate)
		{
			static CInput[] ct(IEnumerable<string> map) => map.Select(str => new CInput(str)).ToArray();

			const string lyt = "kb and mouse";
			InputDatabase.UpdateSingle<AxisAction>(ref horizontalAction, new AxisAction.Layout(lyt, ct(input.HorizontalNegativeKeys), ct(input.HorizontalPositiveKeys)));

			InputDatabase.UpdateSingle<PressAction>(ref jumpAction, new PressAction.Layout(lyt, ct(input.JumpKeys)));
			InputDatabase.UpdateSingle<PressAction>(ref crouchAction, new PressAction.Layout(lyt, ct(input.CrouchKeys)));

			InputDatabase.UpdateSingle<PressAction>(ref confirmAction, new PressAction.Layout(lyt, ct(input.ConfirmKeys)));
			InputDatabase.UpdateSingle<PressAction>(ref cancelAction, new PressAction.Layout(lyt, ct(input.CancelKeys)));
		}
	}
}
using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Input;
using GameHost.Input.Default;

namespace PataponGameHost.Inputs
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class ApplyInputSystem : AppSystem
	{
		private InputDatabase inputDatabase;

		public ApplyInputSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref inputDatabase);
		}

		private Entity                  playerEntity;
		private Dictionary<int, Entity> actionPerRhythmKey;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			actionPerRhythmKey = new Dictionary<int, Entity>();
			for (var i = 0; i != 4; i++)
			{
				var input = i switch
				{
					0 => new CInput("keyboard/keypad4"),
					1 => new CInput("keyboard/keypad6"),
					2 => new CInput("keyboard/keypad2"),
					3 => new CInput("keyboard/keypad8"),
					_ => throw new InvalidOperationException()
				};
				actionPerRhythmKey[i] = inputDatabase.RegisterSingle<PressAction>(new PressAction.Layout("kb and mouse", input));
			}

			playerEntity = World.Mgr.CreateEntity();
			playerEntity.Set(new PlayerInput());
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			ref var playerInput = ref playerEntity.Get<PlayerInput>();
			foreach (var (key, inputEnt) in actionPerRhythmKey)
			{
				ref readonly var press = ref inputEnt.Get<PressAction>();
				playerInput.Actions[key] = new PlayerInput.RhythmAction
				{
					IsActive    = press.DownCount > 0,
					FrameUpdate = press.DownCount > 0 || press.UpCount > 0
				};
			}

			playerEntity.NotifyChanged<PlayerInput>();
		}
	}
}
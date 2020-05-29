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
				actionPerRhythmKey[i] = inputDatabase.RegisterSingle<RhythmInputAction>(new RhythmInputAction.Layout("kb and mouse", input));
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
				ref readonly var action = ref inputEnt.Get<RhythmInputAction>();
				playerInput.Actions[key] = new PlayerInput.RhythmAction
				{
					IsSliding   = action.IsSliding,
					IsActive    = action.Active,
					FrameUpdate = action.DownCount > 0 || action.UpCount > 0
				};

				if (action.IsSliding)
				{
					Console.WriteLine($"sliding!");
				}
			}

			playerEntity.NotifyChanged<PlayerInput>();
		}
	}
}
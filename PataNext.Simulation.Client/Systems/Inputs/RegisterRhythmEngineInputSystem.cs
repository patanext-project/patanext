using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;

namespace PataNext.Simulation.Client.Systems.Inputs
{
	public class RegisterRhythmEngineInputSystem : AppSystem
	{
		private InputDatabase inputDatabase;

		public RegisterRhythmEngineInputSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref inputDatabase);
		}

		private Dictionary<int, Entity> actionPerRhythmKey;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			actionPerRhythmKey = new Dictionary<int, Entity>();
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
				actionPerRhythmKey[i] = inputDatabase.RegisterSingle<PressAction>(new PressAction.Layout("kb and mouse", input));
			}
		}
	}
}
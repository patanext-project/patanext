using System;
using System.Collections.Generic;
using DefaultEcs;
using GameBase.Roles.Descriptions;
using GameHost.Core.Ecs;
using GameHost.Inputs.DefaultActions;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.Simulation.TabEcs;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components;

namespace PataNext.Simulation.Client.Systems.Inputs
{
	public class RegisterRhythmEngineInputSystem : AppSystem
	{
		private InputDatabase     inputDatabase;
		private IManagedWorldTime time;

		public RegisterRhythmEngineInputSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref inputDatabase);
			DependencyResolver.Add(() => ref gameWorld);
			DependencyResolver.Add(() => ref time);
		}

		private Dictionary<int, Entity> rhythmActionMap;
		private Entity                  abilityAction;

		private GameWorld  gameWorld;
		private GameEntity gameEntityTest;

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

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
				rhythmActionMap[i] = inputDatabase.RegisterSingle<PressAction>(new PressAction.Layout("kb and mouse", input));
			}

			abilityAction = inputDatabase.RegisterSingle<AxisAction>(new AxisAction.Layout("kb and mouse", new[] {new CInput("keyboard/leftArrow")}, new[] {new CInput("keyboard/rightArrow")}));

			gameEntityTest = gameWorld.CreateEntity();
			gameWorld.AddComponent(gameEntityTest, new PlayerDescription());
			gameWorld.AddComponent(gameEntityTest, new PlayerInputComponent());
		}
	}
}
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.GamePlay.Abilities
{
	public struct AbilityEngineSet : IComponentData
	{
		public GameEntity Engine;

		public RhythmEngineController       Process;
		public RhythmEngineSettings         Settings;
		public RhythmEngineExecutingCommand CurrentCommand;
		public GameCombo.State              ComboState;
		public GameCommandState             CommandState;

		public GameEntity      Command, PreviousCommand;
		public GameCombo.State Combo,   PreviousCombo;

		public class Register : RegisterGameHostComponentData<AbilityEngineSet>
		{
		}
	}
}
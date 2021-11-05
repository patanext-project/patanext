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
		public GameEntityHandle Engine;

		public RhythmEngineLocalState       Process;
		public RhythmEngineSettings         Settings;
		public RhythmEngineExecutingCommand CurrentCommand;
		public GameCombo.State              ComboState;
		public GameCombo.Settings           ComboSettings;
		public GameCommandState             CommandState;

		public GameResource<RhythmCommandResource> Command, PreviousCommand;

		public class Register : RegisterGameHostComponentData<AbilityEngineSet>
		{
		}
	}
}
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Simulation.mixed.Components.GamePlay.RhythmEngine.DefaultCommands
{
	public struct MarchCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<MarchCommand>
		{
		}
	}
	
	public struct RetreatCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<RetreatCommand>
		{
		}
	}
	
	public struct JumpCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<JumpCommand>
		{
		}
	}
}
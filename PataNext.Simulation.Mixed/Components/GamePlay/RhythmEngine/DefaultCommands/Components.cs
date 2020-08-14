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
	
	public struct ChargeCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<ChargeCommand>
		{
		}
	}
	
	public struct JumpCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<JumpCommand>
		{
		}
	}
	
	public struct DefendCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<DefendCommand>
		{
		}
	}
	
	public struct AttackCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<AttackCommand>
		{
		}
	}
	
	public struct SummonCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<SummonCommand>
		{
		}
	}
	
	public struct PartyCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<PartyCommand>
		{
		}
	}
	
	public struct BackwardCommand : IComponentData
	{
		public class Register : RegisterGameHostComponentData<BackwardCommand>
		{
		}
	}
	
	public struct QuickDefend : IComponentData
	{
		public class Register : RegisterGameHostComponentData<QuickDefend>
		{
		}
	}
	
	public struct QuickRetreat : IComponentData
	{
		public class Register : RegisterGameHostComponentData<QuickRetreat>
		{
		}
	}
}
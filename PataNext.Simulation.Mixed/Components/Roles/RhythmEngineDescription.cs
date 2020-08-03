using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct RhythmEngineDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<RhythmEngineDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentData<RhythmEngineDescription>
		{
		}
	}
}
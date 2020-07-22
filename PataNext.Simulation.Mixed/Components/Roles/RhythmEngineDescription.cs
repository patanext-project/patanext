using GameBase.Roles.Components;
using GameBase.Roles.Interfaces;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct RhythmEngineDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<RhythmEngineDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentSystemBase<RhythmEngineDescription>
		{
		}
	}
}
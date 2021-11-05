using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct UnitTargetDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<UnitTargetDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentData<UnitTargetDescription>
		{
		}
	}
}
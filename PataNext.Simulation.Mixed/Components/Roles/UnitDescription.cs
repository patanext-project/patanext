using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct UnitDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<UnitDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentData<UnitDescription>
		{
		}
	}
}
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct ProjectileDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<ProjectileDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentData<ProjectileDescription>
		{
		}
	}
}
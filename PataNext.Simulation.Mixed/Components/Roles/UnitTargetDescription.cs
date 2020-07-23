using GameBase.Roles.Components;
using GameBase.Roles.Interfaces;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;

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
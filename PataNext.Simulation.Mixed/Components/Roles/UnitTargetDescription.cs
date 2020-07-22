using GameBase.Roles.Components;
using GameBase.Roles.Interfaces;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct UnitTargetDescription : IEntityDescription
	{
		public class RegisterRelative : RegisterGameHostComponentSystemBase<Relative<UnitTargetDescription>>
		{
		}

		public class Register : RegisterGameHostComponentSystemBase<UnitTargetDescription>
		{
		}
	}
}
using GameHost.Core.Ecs;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct AbilityDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<AbilityDescription>.Register
		{
		}

		public class RegisterContainer : BuildContainerSystem<AbilityDescription>
		{
			public RegisterContainer(WorldCollection collection) : base(collection)
			{
			}
		}

		public class Register : RegisterGameHostComponentData<AbilityDescription>
		{
		}
	}
}
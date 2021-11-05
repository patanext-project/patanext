using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace StormiumTeam.GameBase.Roles.Descriptions
{
	public struct TeamDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<TeamDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentData<TeamDescription>
		{
		}
	}
}
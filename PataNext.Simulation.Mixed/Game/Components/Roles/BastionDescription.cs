using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace PataNext.Module.Simulation.Components.Roles
{
	public struct BastionDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<BastionDescription>.Register
		{
		}
	}
}
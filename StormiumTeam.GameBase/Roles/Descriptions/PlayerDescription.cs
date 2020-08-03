using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace StormiumTeam.GameBase.Roles.Descriptions
{
	public struct PlayerDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<PlayerDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentData<PlayerDescription>
		{
		}
	}

	/// <summary>
	/// Indicate whether or not this player is local
	/// </summary>
	public struct PlayerIsLocal : IComponentData
	{
		public class Register : RegisterGameHostComponentData<PlayerIsLocal>
		{
		}
	}
}
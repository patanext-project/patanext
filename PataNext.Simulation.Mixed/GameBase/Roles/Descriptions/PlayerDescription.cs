using GameBase.Roles.Components;
using GameBase.Roles.Interfaces;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;

namespace GameBase.Roles.Descriptions
{
	public struct PlayerDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<PlayerDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentSystemBase<PlayerDescription>
		{
		}
	}

	/// <summary>
	/// Indicate whether or not this player is local
	/// </summary>
	public struct PlayerIsLocal : IComponentData
	{
		public class Register : RegisterGameHostComponentSystemBase<PlayerIsLocal>
		{
		}
	}
}
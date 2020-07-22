using GameBase.Roles.Components;
using GameBase.Roles.Interfaces;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;

namespace GameBase.Roles.Descriptions
{
	public struct PlayerDescription : IEntityDescription
	{
		public class RegisterRelative : RegisterGameHostComponentSystemBase<Relative<PlayerDescription>>
		{
		}
	}
}
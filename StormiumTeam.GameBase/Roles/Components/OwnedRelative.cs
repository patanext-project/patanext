using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace StormiumTeam.GameBase.Roles.Components
{
	public readonly struct OwnedRelative<T> : IComponentBuffer
		where T : IEntityDescription
	{
		public readonly GameEntity Target;

		public OwnedRelative(GameEntity entity)
		{
			Target = entity;
		}
	}
}